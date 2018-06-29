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
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Model;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Digiphoto.Lumen.Core.Eventi;
using Digiphoto.Lumen.Applicazione;

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

		CopiaImmaginiWorker _copiaImmaginiWorker;


		public ParamScarica ultimaChiavettaInserita {
			get {
				return _ultimaChiavettaInserita;
			}
			private set {
				_ultimaChiavettaInserita = value;
			}
		}

		private ParamScarica _paramScarica;



		//		List<Fotografo> _fotografiAttivi;
		public IEnumerable<Fotografo> fotografiAttivi {
			get {
				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
				var fotografi = dbContext.Fotografi.Where( f => f.attivo == true ).OrderBy( f => f.iniziali );
				return fotografi;
			}
		}

		public override bool possoChiudere()
		{
			//return !(_copiaImmaginiWorker != null && _copiaImmaginiWorker.disposed == false);
			return (statoScarica == StatoScarica.Idle);
		}

		#endregion



		public ScaricatoreFotoSrvImpl() {
			statoScarica = StatoScarica.Idle;
		}

		~ScaricatoreFotoSrvImpl() {
			// avviso se il thread di copia è ancora attivo
			if( _copiaImmaginiWorker != null && _copiaImmaginiWorker.disposed == false ) {
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

			_paramScarica = paramScarica;

			seNonPossoScaricareSpaccati();

			// Se per sbaglio ho ancora un worker aperto, lo chiudo
			chiudiWorker();

			// --- Pubblico un messaggio di inizio scaricamento foto
			ScaricoFotoMsg scaricoFotoMsg = new ScaricoFotoMsg( this, "Inizio Scarico Foto" );
			scaricoFotoMsg.esitoScarico = new EsitoScarico {
				riscontratiErrori = false
			};
			scaricoFotoMsg.fase = FaseScaricoFoto.InizioScarico;
			scaricoFotoMsg.sorgente = _paramScarica.cartellaSorgente != null ? _paramScarica.cartellaSorgente : _paramScarica.nomeFileSingolo;
			scaricoFotoMsg.showInStatusBar = true;
			pubblicaMessaggio( scaricoFotoMsg );
			// --- 

			// Creo il worker che copierà le foto in background
			_copiaImmaginiWorker = new CopiaImmaginiWorker( paramScarica, elaboraFotoAcquisite );

			// Lancio il worker che scarica ed elabora le foto.
			bool usaThreadSeparato = String.IsNullOrEmpty( paramScarica.nomeFileSingolo );
			if( usaThreadSeparato )
				_copiaImmaginiWorker.Start();
			else
				_copiaImmaginiWorker.StartSingleThread();

			statoScarica = StatoScarica.Scaricamento;
		}


		private void elaboraFotoAcquisite( EsitoScarico esitoScarico ) {

			_giornale.Debug( "Inizio elaboraFotoAcquisite()" );

			StringBuilder msg = new StringBuilder();
			if( esitoScarico.riscontratiErrori )
				msg.AppendFormat( "Errore in acquisizione {0} foto. Verificare!", esitoScarico.totFotoCopiateOk );
			else
				msg.AppendFormat( "Scaricate OK {0} foto.", + esitoScarico.totFotoCopiateOk );
			msg.Append( " Togliere la card" );

			ScaricoFotoMsg scaricoFotoMsg = new ScaricoFotoMsg( this, msg.ToString() );
			scaricoFotoMsg.esitoScarico = esitoScarico;
			scaricoFotoMsg.esito = esitoScarico.riscontratiErrori ? Esito.Errore : Esito.Ok;
			// Finito: genero un evento per notificare che l'utente può togliere la flash card.
			scaricoFotoMsg.fase = FaseScaricoFoto.FineScarico;
			scaricoFotoMsg.sorgente = _paramScarica.cartellaSorgente != null ? _paramScarica.cartellaSorgente : _paramScarica.nomeFileSingolo;
			scaricoFotoMsg.showInStatusBar = true;
			
			// battezzo la flashcard al fotografo corrente
			battezzaFlashCard( _paramScarica );

			// Se il drive che ho appena scaricato è rimovibile, allora lo smonto
			// Richiesta di Ciccio del 29-03-2018 non smontiamo piu la card
			// smontaSeRimovibile();



			// Rendo pubblico l'esito dello scarico in modo che la UI possa notificare l'utente di togliere 
			// la flash card.
			/*
			 * per l'onride mi serve che lanci il msg anche su un solo file
			 * però mi pare che nessuno utilizzi il modo singolo. quindi lo pubblico sempre
			 * 
			if( _paramScarica.nomeFileSingolo == null && _paramScarica.cartellaSorgente != null )
			*/
				pubblicaMessaggio( scaricoFotoMsg );
			
			_giornale.Debug( "Scaricate " + esitoScarico.totFotoCopiateOk + " foto. Si può togliere la card" );

			// -- inizio elaborazione 

			ElaboratoreFotoAcquisite elab = new ElaboratoreFotoAcquisite( esitoScarico.fotoDaLavorare, _paramScarica );
			statoScarica = StatoScarica.Provinatura;
			elab.elabora( esitoScarico.tempo );

			_giornale.Debug( "Elaborazione terminata. Inserite " + elab.contaAggiunteDb + " foto nel database" );

			statoScarica = StatoScarica.Idle;

			// Rendo pubblico l'esito dell'elaborazione così che si può aggiornare la libreria.
			scaricoFotoMsg.esitoScarico.totFotoProvinate = elab.contaAggiunteDb;
			if( scaricoFotoMsg.esitoScarico.totFotoProvinate != scaricoFotoMsg.esitoScarico.totFotoScaricate )
				scaricoFotoMsg.esitoScarico.riscontratiErrori = true;

			scaricoFotoMsg.fase = FaseScaricoFoto.FineLavora;
			scaricoFotoMsg.descrizione = "Provinatura foto terminata. Inserite " + elab.contaAggiunteDb + " foto nel database";
			scaricoFotoMsg.showInStatusBar = true;
			pubblicaMessaggio( scaricoFotoMsg );

			// Chiudo il worker che ha finito il suo lavoro
			// _copiaImmaginiWorker.Stop();


			// Faccio un controllo: Se le foto scaricate non coincidono con quelle elaborate (ovvero scritte nel db e provinate)
			// Allora c'è stato un problema. Devo avvisare l'utente di ricostruire il database
			bool problemiInVista = (esitoScarico.riscontratiErrori || esitoScarico.totFotoCopiateOk != elab.contaAggiunteDb);

			if( problemiInVista ) {

				// TODO stampare nel log tutto l'oggetto EsitoScarico
				_giornale.Warn( String.Format( "Riscontrata incongruenza database. copiate={0} elab={1}", esitoScarico.totFotoCopiateOk, elab.contaAggiunteDb ) );

				RilevataInconsistenzaDatabaseMsg inconsistenzaMsg = new RilevataInconsistenzaDatabaseMsg( this );
				inconsistenzaMsg.descrizione = "Inconsistenza database. Lanciare ricostruzione!";
				inconsistenzaMsg.showInStatusBar = true;
				inconsistenzaMsg.giornataDaVerificare = LumenApplication.Instance.stato.giornataLavorativa;
				pubblicaMessaggio( inconsistenzaMsg );
			}

		}

		private void smontaSeRimovibile() {

			try {

				if( _paramScarica.cartellaSorgente.Length >= 2 && _paramScarica.cartellaSorgente[1] == ':' ) {
					char driveLetter = _paramScarica.cartellaSorgente[0];
					DriveInfo driveInfo = new DriveInfo( driveLetter.ToString() );
					if( driveInfo.DriveType == DriveType.Removable && driveInfo.IsReady )
						UsbEjectWithExe.usbEject( driveLetter );
				}

			} catch( Exception ) {
			}
		}


		private void seNonPossoScaricareSpaccati() {

			if( isRunning == false )
				throw new InvalidOperationException( "Il servizio è fermo. Impossibile scaricare le foto adesso" );

			if( !(_paramScarica.nomeFileSingolo == null ^ _paramScarica.cartellaSorgente == null) )
				throw new ArgumentException( "specificare la cartella da scaricare, oppure il nome del file singolo da scaricare. Uno, l'altro ma non tutti e due" );

			if( _paramScarica.cartellaSorgente != null ) {

				if( Directory.Exists( _paramScarica.cartellaSorgente ) == false )
					throw new FileNotFoundException( "cartella da scaricare inesistente:\n" + _paramScarica.cartellaSorgente );

				// Se devo spostare le foto, allora testo che la cartella sia scrivibile
				if( _paramScarica.eliminaFilesSorgenti && !PathUtil.isCartellaScrivibile( _paramScarica.cartellaSorgente ) )
					throw new InvalidOperationException( "La cartella indicata non è scrivibile. Impossibile spostare i files" );
			}

			if( _paramScarica.nomeFileSingolo != null && File.Exists( _paramScarica.nomeFileSingolo ) == false )
				throw new FileNotFoundException( "file da scaricare inesistente:\n" + _paramScarica.nomeFileSingolo );

			if( _paramScarica.flashCardConfig.idFotografo == null )
				throw new ArgumentException( "fotografo non indicato" );
		}


		// Se mi arriva il messaggio di cambio volume, memorizzo i dati
		public override void OnNext( Messaggio messaggio ) {

			if( messaggio is VolumeCambiatoMsg ) {
				VolumeCambiatoMsg vcm = (VolumeCambiatoMsg)messaggio;

				if( vcm.montato ) {
					// E' stata inserita una flash card (o un disco rimovibile)
					ultimaChiavettaInserita = creaParamScarica( vcm.nomeVolume );

					if( ultimaChiavettaInserita.flashCardConfig != null ) {
						Messaggio msg = new Messaggio( this, "E' stata inserita una memory-card" );
						msg.senderTag = "::OnLetturaFlashCardConfig";
						msg.showInStatusBar = true;
						pubblicaMessaggio( msg );
					}
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
		 * Eseguo anche il controllo che il fotografo non sia l'ARTISTA. Infatti se sto creando 
		 * delle cornici, non ho bisogno di battezzare la card (perché non esiste una card).
		 * <returns>Ritorna true se dovevo battezzare ed è andato tutto bene</returns>
		 */
		public bool battezzaFlashCard( ParamScarica param ) {

			_giornale.Debug( "Inizio battezzaFlashCard()" );

			bool riuscito = false;

			if( param.cartellaSorgente == null )
				return riuscito;

			try {

				// Eseguo il controllo soltanto se il disco è rimovibile
				DriveInfo driveInfo = new DriveInfo( param.cartellaSorgente );
				if( driveInfo.DriveType == DriveType.Removable ) {

					string nomeFileConfig = Path.Combine( param.cartellaSorgente, FlashCardConfig.NOMEFILECONFIG );

					FlashCardConfig.serialize( nomeFileConfig, param.flashCardConfig );

					riuscito = true;

					_giornale.Debug( "Battezzata memory card al fotografo: " + param.flashCardConfig.idFotografo );
				}

			} catch( Exception ee ) {
				// pazienza. Non è grave.
				_giornale.Debug( "Non sono riuscito a battezzare la flash card", ee );
			}

			_giornale.Debug( "Finito battezzaFlashCard(). riuscito=" + riuscito );
			return riuscito;
		}

		private void chiudiWorker() {
			if( _copiaImmaginiWorker != null && _copiaImmaginiWorker.disposed == false ) {
				_copiaImmaginiWorker.Dispose();
			}
		}

		protected override void Dispose( bool disposing ) {

			try {
				chiudiWorker();
			} catch( Exception e ) {
				_giornale.Error( "worker copia fallita dispose", e );
			} finally {
				base.Dispose( disposing );
			}
		}

		/// <summary>
		/// StatoRun di questo servizio. Vedere apposita enumerazione
		/// </summary>
		private StatoScarica _statoScarica;
		public StatoScarica statoScarica {
			get {
				return _statoScarica;
			}
			private set {
				if( value != _statoScarica ) {
					_statoScarica = value;

					// Notifico tutti
					CambioStatoMsg msg = new CambioStatoMsg( this );
					msg.nuovoStato = (int)_statoScarica;
					msg.descrizione = this.GetType().Name + ": nuovo statoScarica -> " + _statoScarica.ToString();
					pubblicaMessaggio( msg );
				}
			}
		}
	}

}
