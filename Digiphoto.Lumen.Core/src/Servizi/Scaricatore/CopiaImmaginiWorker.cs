using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Threading;
using log4net;
using Digiphoto.Lumen.Database;
using System.IO;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	internal class CopiaImmaginiWorker : WorkerThreadBase {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(CopiaImmaginiWorker) );

		private ParamScarica _paramScarica;

		internal delegate void ElaboraImmaginiAcquisiteCallback( EsitoScarico esitoScarico );

		ElaboraImmaginiAcquisiteCallback _elaboraImmaginiAcquisiteCallback;

		EsitoScarico _esitoScarico;

		public CopiaImmaginiWorker( ParamScarica paramScarica, ElaboraImmaginiAcquisiteCallback elaboraCallback ) {
			_paramScarica = paramScarica;
			_elaboraImmaginiAcquisiteCallback = elaboraCallback;
		}

		/**
		 * Mi dice se ho completato la copia
		 * torna TRUE anche se la copia è stata stoppata
		 */
		internal bool disposed {
			get {
				return base.Disposed;
			}
		}

		/**
		 * Processo reale di trasferimento immagini
		 */
		protected override void Work() {

			int conta = 0;
			DateTime oraInizio = DateTime.Now;


			_giornale.Debug( "Inizio a trasferire le foto da " + _paramScarica.cartellaSorgente );
			

			// Pattern Unit-of-work
			using( new UnitOfWorkScope( true ) ) {

				string nomeDirDest = calcolaCartellaDestinazione();

				// Creo la cartella che conterrà le foto
				Directory.CreateDirectory( nomeDirDest );

				// Creo la cartella che conterrà i provini
				PathUtil.creaCartellaProvini( new FileInfo( nomeDirDest ) );

				_esitoScarico = new EsitoScarico();
				
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


				// ::: Ultima fase eleboro le foto memorizzando nel db e creando le dovute cache
				_elaboraImmaginiAcquisiteCallback.Invoke( _esitoScarico );

			}
		}


		private string calcolaCartellaDestinazione() {
			
			string [] pezzi = new string [3];

			Configurazione configurazione = LumenApplication.Instance.configurazione;
			Lumen.Applicazione.Stato stato = LumenApplication.Instance.stato;

			pezzi[0] = configurazione.getCartellaRepositoryFoto();
			pezzi[1] = String.Format( "{0:yyyy-MM-dd}", stato.giornataLavorativa )+configurazione.suffissoCartellaGiorni();
            pezzi[2] = _paramScarica.flashCardConfig.idFotografo + configurazione.suffissoCartellaFoto();

			return Path.Combine( pezzi );
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

				++_esitoScarico.totFotoCopiateOk;
				_esitoScarico.fotoDaLavorare.Add( new FileInfo( nomeFileDest ) );
				_giornale.Debug( "Ok copiato il file : " + nomeFileSrc );

				// rendo il file di sola lettura. Mi serve per protezione
				// TODO un domani potrei anche renderlo HIDDEN
				File.SetAttributes( nomeFileDest, FileAttributes.Archive | FileAttributes.ReadOnly );

			} catch( Exception ee ) {
				_esitoScarico.riscontratiErrori = true;
				++_esitoScarico.totFotoNonCopiate;
				copiato = false;
				_giornale.Error( "Il file " + nomeFileSrc + " non è stato copiato ", ee );
			}

			return copiato;
		}

	}

}
