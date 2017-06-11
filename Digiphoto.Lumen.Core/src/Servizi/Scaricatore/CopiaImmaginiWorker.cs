using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Threading;
using log4net;
using Digiphoto.Lumen.Core.Database;
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

		/// <summary>
		/// In alcune situazioni, devo evitare di far partire la copia e la elaborazione in thread
		/// separati.
		/// Per esempio quando lavoro una sola foto per la cornice.
		/// In questo caso, non avvio i worker, ma eseguo il metodo work in modo assestante.
		/// </summary>
		public void StartSingleThread() {
			ThrowIfDisposedOrDisposing();
			Work();
		}

		/**
		 * Processo reale di trasferimento immagini
		 */
		protected override void Work() {

			int conta = 0;
			


			_giornale.Debug( "Inizio a trasferire le foto da " + _paramScarica.cartellaSorgente );
			


			string nomeDirDest = calcolaCartellaDestinazione();

			// Creo la cartella che conterrà le foto
			Directory.CreateDirectory( nomeDirDest );

			// Creo la cartella che conterrà i provini
			PathUtil.creaCartellaProvini( new FileInfo( nomeDirDest ) );

			_esitoScarico = new EsitoScarico();

			ScaricoFotoMsg scaricoFotoMsg = new ScaricoFotoMsg(this, "Notifica progresso");
			scaricoFotoMsg.fase = FaseScaricoFoto.Scaricamento;
			scaricoFotoMsg.esitoScarico = _esitoScarico;
			scaricoFotoMsg.sorgente = _paramScarica.cartellaSorgente != null ? _paramScarica.cartellaSorgente : _paramScarica.nomeFileSingolo;
			scaricoFotoMsg.showInStatusBar = false;

			try {

				if( _paramScarica.nomeFileSingolo != null ) {

					// Lavoro un solo file che mi è stato indicato. Serve per creare una maschera quando lavoro con le cornici.
					if( scaricaAsincronoUnFile( _paramScarica.nomeFileSingolo, nomeDirDest ) ) {
						++conta;
						if( conta % 20 == 0 ) {
							scaricoFotoMsg.esitoScarico.totFotoScaricateProg = conta;
							LumenApplication.Instance.bus.Publish( scaricoFotoMsg );
						}
					} else {
						// La copia di questo file non è andata a buon fine
						_esitoScarico.riscontratiErrori = true;
					}

				} else {

					// Faccio giri diversi per i vari formati grafici che sono indicati nella configurazione (jpg, tif)
					string[] estensioni = Configurazione.UserConfigLumen.estensioniGrafiche.Split( ';' );
					foreach( string estensione in estensioni ) {

						string[] files = Directory.GetFiles( _paramScarica.cartellaSorgente, searchPattern: "*" + estensione, searchOption: SearchOption.AllDirectories );

						// trasferisco tutti i files elencati
						foreach( string nomeFileSrc in files ) {
							if( scaricaAsincronoUnFile( nomeFileSrc, nomeDirDest ) ) {
								++conta;
								if( conta % 20 == 0 ) {
									scaricoFotoMsg.esitoScarico.totFotoScaricateProg = conta;
									LumenApplication.Instance.bus.Publish( scaricoFotoMsg );
								}
							} else {
								// La copia di questo file non è andata a buon fine
								_esitoScarico.riscontratiErrori = true;
							}
						}
					}
				}

			} catch( Exception qq ) {
				// Se casco qui, probabilmente è perché è stata sfilata la memorycard prima che sia completato lo scarico.
				_esitoScarico.riscontratiErrori = true;
				_giornale.Error( "Errore imprevisto durante scarico card", qq );
			}

			if (conta != 0)
			{
				scaricoFotoMsg.esitoScarico.totFotoScaricate = conta;
				scaricoFotoMsg.esitoScarico.totFotoScaricateProg = conta;
				LumenApplication.Instance.bus.Publish(scaricoFotoMsg);
			}

			// Nel log scrivo anche il tempo che ci ho messo a scaricare le foto. Mi servirà per profilare
			TimeSpan tempoImpiegato = DateTime.Now.Subtract( _esitoScarico.tempo );
			_giornale.Info( "Terminato trasferimento di " + conta + " foto. Tempo impiegato = " + tempoImpiegato );


			// Deve essere già aperto
			using( new UnitOfWorkScope( true ) ) {
					// ::: Ultima fase eleboro le foto memorizzando nel db e creando le dovute cache
				_elaboraImmaginiAcquisiteCallback.Invoke( _esitoScarico );
			}

			_giornale.Debug( "Terminato background worker per copia files" );

		}


		private string calcolaCartellaDestinazione() {
			
			string [] pezzi = new string [3];

			Configurazione configurazione = LumenApplication.Instance.configurazione;
			Lumen.Applicazione.Stato stato = LumenApplication.Instance.stato;

			pezzi[0] = Configurazione.cartellaRepositoryFoto;
			pezzi[1] = String.Format( "{0:yyyy-MM-dd}", stato.giornataLavorativa )+Configurazione.suffissoCartellaGiorni;
            pezzi[2] = _paramScarica.flashCardConfig.idFotografo + Configurazione.suffissoCartellaFoto;

			return Path.Combine( pezzi );
		}

		/**
		 * Se  va tutto bene ritorna true
		 */
		private bool scaricaAsincronoUnFile( string nomeFileSrc, string nomeDirDest ) {

			FileInfo fileInfoSrc = new FileInfo( nomeFileSrc );
			string nomeOrig = fileInfoSrc.Name;

			string nomeFileDest;
			int tenta = 0;
			do {
				nomeFileDest = Path.Combine( nomeDirDest, nomeOrig );
				if( tenta++ > 0 ) {

					int pos = nomeFileDest.LastIndexOf( "." );
					if( pos >= 0 ) {
						nomeFileDest = nomeFileDest.Insert( pos, "_" + tenta );
					} else
						throw new Exception( "nome file senza estensione" );
				}
			} while( System.IO.File.Exists( nomeFileDest ) );
			

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
