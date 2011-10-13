using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.IO;
using log4net;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using System.Threading;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Model;
using System.Data.Objects;
using System.Linq;

namespace Digiphoto.Lumen.Servizi.Scaricatore {


	/** 
	 * Questo servizio deve essere usato una volta e sola e poi distrutto.
	 * In questo modo sono sicuro che non ci sono esecuzioni che si pestano 
	 * i piedi.
	 */
	public class ScaricatoreFotoSrvImpl : ServizioImpl, IScaricatoreFotoSrv {

	
		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ScaricatoreFotoSrvImpl ) );

		#region Proprietà

		ParamScarica _ultimaChiavettaInserita = null;

		public ParamScarica ultimaChiavettaInserita {
			get {
				return _ultimaChiavettaInserita;
			}
			private set {
				_ultimaChiavettaInserita = value;
			}
		}

		private bool _morto = false;
		private ParamScarica _paramScarica;
		private Thread _threadCopia;

		/** Questo rappresenta l'esito dello scaricamento delle foto */
		public ScaricoFotoMsg scaricoFotoMsg {
			get;
			private set;
		}
		
		#endregion



		public ScaricatoreFotoSrvImpl() {
		}

		~ScaricatoreFotoSrvImpl() {
			// avviso se il thread di copia è ancora attivo
			if( _threadCopia != null && _threadCopia.IsAlive ) {
				_giornale.Warn( "Il thread di copia file è ancora attivo. Non è stata fatta la Join o la Abort.\nProababilmente il programma si inchioderà" );
			}
		}

		/**
		 * Lo scopo di questa operazione è quella di scaricare le foto
		 * dalla flash card all'hard disk locale il più velocemente
		 * possibile. In questo modo posso congedare il fotografo che
		 * sta aspettando. Durante questa fase, non ho feedback con l'utente.
		 * Terminato lo scarico, allora posso muovere il file nella giusta 
		 * cartella di destinazione, scrivendo nel database e sollevando gli
		 * eventi per poter visualizzare le foto.
		 */
		public void scarica( ParamScarica paramScarica ) {

			if( !isRunning ) {
				_giornale.Warn( "Il servizio è stoppato. Non scarico le foto" );
				return;
			}

			_paramScarica = paramScarica;

			seNonPossoScaricareSpaccati();

			// Scarico in un thread separato per non bloccare l'applicazione
			_threadCopia = new Thread( scaricaAsincrono );
			_threadCopia.Start();

		}

		
		private void scaricaAsincrono() {

			int conta = 0;
			DateTime oraInizio = DateTime.Now;


			_giornale.Debug( "Inizio a trasferire le foto da " + _paramScarica.cartellaSorgente );
			

			// Pattern Unit-of-work
			using( new UnitOfWorkScope( true ) ) {

				string nomeDirDest = calcolaCartellaDestinazione();

				// Creo la cartella che conterrà le foto
				Directory.CreateDirectory( nomeDirDest );

				// Creo la cartella che conterrà i provini
				PathUtil.creaCartellaProvini( new FileInfo(nomeDirDest) );
				
				scaricoFotoMsg = new ScaricoFotoMsg();

				// Faccio giri diversi per i vari formati grafici che sono indicati nella configurazione (jpg, tif)
				string [] estensioni = Properties.Settings.Default.estensioniGrafiche.Split( ';' );
				foreach( string estensione in estensioni ) {

					string [] files = Directory.GetFiles( _paramScarica.cartellaSorgente, searchPattern: estensione, searchOption: SearchOption.AllDirectories );

					// trasferisco tutti i files elencati
					foreach( string nomeFileSrc in files ) {
						if( scaricaAsincronoUnFile( nomeFileSrc, nomeDirDest ) )
							++conta;
					}

				}

				// Nel log scrivo anche il tempo che ci ho messo a scaricare le foto. Mi servirà per profilare
				TimeSpan tempoImpiegato = DateTime.Now.Subtract( oraInizio );
				_giornale.Info( "Terminato trasferimento di " + conta + " foto. Tempo impiegato = " + tempoImpiegato );

				// Finito: genero un evento per notificare che l'utente può togliere la flash card.
				scaricoFotoMsg.fase = Fase.FineScarico;
				scaricoFotoMsg.descrizione = "Acquisizione foto terminata";
				scaricoFotoMsg.cartellaSorgente = _paramScarica.cartellaSorgente;

				// battezzo la flashcard al fotografo corrente
				battezzaFlashCard( _paramScarica );

				// Rendo pubblico l'esito dello scarico in modo che la UI possa notificare l'utente di togliere 
				// la flash card.
				pubblicaMessaggio( scaricoFotoMsg );


				// ::: Ultima fase eleboro le foto memorizzando nel db e creando le dovute cache
				elaboraFotoAcquisite();

			}
		}

		/**
		 * Se  va tutto bene ritorna true
		 */
		private bool scaricaAsincronoUnFile( string nomeFileSrc, string nomeDirDest ) {

			FileInfo fileInfoSrc = new FileInfo( nomeFileSrc );
			string nomeOrig = fileInfoSrc.Name;
			string nomeFileDest = Path.Combine( nomeDirDest, nomeOrig );


			// TODO : il file potrebbe esistere con lo stesso nome, ma essere differente.
			//        andrebbe gestita una opzione di sovrascrittura. Per ora non mi preoccupo
			//        e non sovrascrivo mai. Se c'è una collisione, basterà cambiare operatore.
			bool sovrascrivi = false;
			bool copiato;

			try {

				if( _paramScarica.eliminaFilesSorgenti )
					File.Move( nomeFileSrc, nomeFileDest );
				else
					File.Copy( nomeFileSrc, nomeFileDest, sovrascrivi );

				copiato = true;
				++scaricoFotoMsg.totFotoCopiateOk;
				scaricoFotoMsg.fotoDaLavorare.Add( new FileInfo( nomeFileDest ) );
				_giornale.Debug( "Ok copiato il file : " + nomeFileSrc );

				// rendo il file di sola lettura. Mi serve per protezione
				// TODO un domani potrei anche renderlo HIDDEN
				File.SetAttributes( nomeFileDest, FileAttributes.Archive | FileAttributes.ReadOnly );

			} catch( Exception ee ) {
				scaricoFotoMsg.riscontratiErrori = true;
				++scaricoFotoMsg.totFotoNonCopiate;
				copiato = false;
				_giornale.Error( "Il file " + nomeFileSrc + " non è stato copiato ", ee );
			}

			return copiato;
		}

		private void elaboraFotoAcquisite() {

			ElaboratoreFotoAcquisite elab = new ElaboratoreFotoAcquisite( scaricoFotoMsg.fotoDaLavorare, _paramScarica );
			elab.elaboora();

			_giornale.Debug( "Elaborazione terminata. Inserite " + elab.conta + " foto nel database" );
			
			// Rendo pubblico l'esito dell'elaborazione così che si può aggiornare la libreria.
			scaricoFotoMsg.fase = Fase.FineLavora;
			scaricoFotoMsg.descrizione = "Provinatura foto terminata";
			pubblicaMessaggio( scaricoFotoMsg );

            //Edward84
            using(LumenEntities dbContext = new LumenEntities()){

                Fotografo fotografo = (Fotografo)dbContext.Fotografi.FirstOrDefault<Fotografo>(ff => ff.id == _paramScarica.flashCardConfig.idFotografo);

                ScaricoCard scaricoCard = new ScaricoCard();
                scaricoCard.totFoto = (short)elab.numeroFotoAcquisite();
               
                scaricoCard.Fotografo = fotografo;
                scaricoCard.tempo = DateTime.Now;

                dbContext.ScarichiCards.AddObject(scaricoCard);
                dbContext.SaveChanges();
            }
           
		}

		private void seNonPossoScaricareSpaccati() {

			// Voglio evitare doppie esecuzioni. Si scarica e poi si distrugge
			if( _morto )
				throw new InvalidOperationException( "Il metodo scarica si può chiamare solo una volta" );

			if( isRunning == false )
				throw new InvalidOperationException( "Il servizio è fermo. Impossibile scaricare le foto adesso" );

			if( !Directory.Exists( _paramScarica.cartellaSorgente ) )
				throw new FileNotFoundException( "cartella da scaricare inesistente:\n" + _paramScarica.cartellaSorgente );

			if( _paramScarica.flashCardConfig.idFotografo == null )
				throw new ArgumentException( "fotografo non indicato" );

		}


		// Se mi arriva il messaggio di cambio volume, memorizzo i dati
		public override void OnNext( Messaggio messaggio ) {

			if( messaggio is VolumeCambiatoMessaggio ) {
				VolumeCambiatoMessaggio vcm = (VolumeCambiatoMessaggio)messaggio;

				if( vcm.montato ) {
					// E' stata inserita una flash card (o un disco rimovibile)
					ultimaChiavettaInserita = creaParamScarica( vcm.nomeVolume );
				} else
					ultimaChiavettaInserita = null;

			}

			base.OnNext( messaggio );
		}

		/** Provo a leggere sulla flash card se esiste il file di configurazione */
		private ParamScarica creaParamScarica( string driveName ) {

			ParamScarica p = new ParamScarica();
			p.cartellaSorgente = driveName;

			FlashCardConfig fcc = null;

			string nomeFileConfig = Path.Combine( driveName, FlashCardConfig.NOMEFILECONFIG );
			if( File.Exists( nomeFileConfig ) ) {
				try {
					fcc = FlashCardConfig.Deserialize( nomeFileConfig );
				} catch( Exception e ) {
					_giornale.Debug( "File riconoscimento flash card nel drive : " + driveName, e );
				} finally {
					p.flashCardConfig = fcc;
				}
			}

			return p;
		}


		/**
		 * Scrive il file xml di configurazione sulla flash card.
		 * Se il disco NON è rimovibile, non faccio nulla.
		 * Ritorna true se tutto bene.
		 */
		public bool battezzaFlashCard( ParamScarica param ) {

			bool riuscito = false;

			// Eseguo il controllo soltanto se il disco è rimovibile
			DriveInfo driveInfo = new DriveInfo( param.cartellaSorgente );
			if( driveInfo.DriveType == DriveType.Removable ) {

				try {

					string nomeFileConfig = Path.Combine( param.cartellaSorgente, FlashCardConfig.NOMEFILECONFIG );

					FlashCardConfig.serialize( nomeFileConfig, param.flashCardConfig );

					riuscito = true;

				} catch( Exception ee ) {
					// pazienza. Non è grave.
					_giornale.Debug( "Non sono riuscito a battezzare la flash card", ee );
				}
			}

			return riuscito;
		}


		public string calcolaCartellaDestinazione() {
			
			string [] pezzi = new string [3];

			pezzi[0] = configurazione.getCartellaRepositoryFoto();
			pezzi[1] = String.Format( "{0:yyyy-MM-dd}", configurazione.dataLegale );
			pezzi[2] = _paramScarica.flashCardConfig.idFotografo;

			return Path.Combine( pezzi );
		}

		public override void Dispose() {

			try {

				// Se il tread di copia è ancora vivo, lo uccido
				if( _threadCopia != null ) {
					if( _threadCopia.IsAlive )
						_threadCopia.Abort();
					else
						_threadCopia.Join();
				}

			} finally {
				base.Dispose();
			}
		}

	}

}
