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

		CopiaImmaginiWorker _copiaImmaginiWorker;


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

		#endregion



		public ScaricatoreFotoSrvImpl() {
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

			_copiaImmaginiWorker = new CopiaImmaginiWorker( paramScarica, elaboraFotoAcquisite );
			_copiaImmaginiWorker.Start();
		}


		private void elaboraFotoAcquisite( EsitoScarico esitoScarico ) {

			ScaricoFotoMsg scaricoFotoMsg = new ScaricoFotoMsg( "Scaricate " + esitoScarico.totFotoCopiateOk + " foto" );

			// Finito: genero un evento per notificare che l'utente può togliere la flash card.
			scaricoFotoMsg.fase = Fase.FineScarico;
			scaricoFotoMsg.descrizione = "Acquisizione foto terminata";
			scaricoFotoMsg.cartellaSorgente = _paramScarica.cartellaSorgente;

			// battezzo la flashcard al fotografo corrente
			battezzaFlashCard( _paramScarica );

			// Rendo pubblico l'esito dello scarico in modo che la UI possa notificare l'utente di togliere 
			// la flash card.
			pubblicaMessaggio( scaricoFotoMsg );


			// -- inizio elaborazione 

			ElaboratoreFotoAcquisite elab = new ElaboratoreFotoAcquisite( esitoScarico.fotoDaLavorare, _paramScarica );
			elab.elabora();

			_giornale.Debug( "Elaborazione terminata. Inserite " + elab.conta + " foto nel database" );
			
			// Rendo pubblico l'esito dell'elaborazione così che si può aggiornare la libreria.
			scaricoFotoMsg.fase = Fase.FineLavora;
			scaricoFotoMsg.descrizione = "Provinatura foto terminata";
			pubblicaMessaggio( scaricoFotoMsg );


			// Chiudo il worker che ha finito il suo lavoro
			//_copiaImmaginiWorker.Stop();
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



		public override void Dispose() {

			try {

				if( _copiaImmaginiWorker != null && _copiaImmaginiWorker.disposed == false ) {
					_copiaImmaginiWorker.Dispose();
				}
			} catch( Exception e ) {
				_giornale.Error( "worker copia fallita dispose", e );
			} finally {
				base.Dispose();
			}
		}

	}

}
