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
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Scaricatore {

	internal class ClonaImmaginiWorker : WorkerThreadBase {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(CopiaImmaginiWorker) );

		internal delegate void ElaboraImmaginiAcquisiteCallback( EsitoScarico esitoScarico );

		Fotografia[] fotografie;

		ElaboraImmaginiAcquisiteCallback _elaboraImmaginiAcquisiteCallback;

		EsitoScarico _esitoScarico;

		public ClonaImmaginiWorker( Fotografia[] fotografie, ElaboraImmaginiAcquisiteCallback elaboraCallback ) {
			this.fotografie = fotografie;
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
			DateTime oraInizio = DateTime.Now;

			_giornale.Debug( "Inizio a clonare le foto");

			_esitoScarico = new EsitoScarico();

			// trasferisco tutti i files elencati
			foreach (Fotografia foto in fotografie)
			{

				if (clonaAsincronoUnFile(foto.nomeFile, foto.nomeFile))
					++conta;
			}


			// Nel log scrivo anche il tempo che ci ho messo a scaricare le foto. Mi servirà per profilare
			TimeSpan tempoImpiegato = DateTime.Now.Subtract( oraInizio );
			_giornale.Info( "Terminato trasferimento di " + conta + " foto. Tempo impiegato = " + tempoImpiegato );


			// Deve essere già aperto
			using( new UnitOfWorkScope( true ) ) {
					// ::: Ultima fase eleboro le foto memorizzando nel db e creando le dovute cache
				_elaboraImmaginiAcquisiteCallback.Invoke( _esitoScarico );
			}


		}

		/**
		 * Se  va tutto bene ritorna true
		 */
		private bool clonaAsincronoUnFile( string nomeFileSrc, string nomeDirDest ) {

			FileInfo fileInfoSrc = new FileInfo( nomeFileSrc );
			string nomeOrig = fileInfoSrc.Name;
			string nomeFileDest = Path.Combine( nomeDirDest, nomeOrig+"_CLONE" );

			bool sovrascrivi = false;
			bool copiato;

			try {

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
