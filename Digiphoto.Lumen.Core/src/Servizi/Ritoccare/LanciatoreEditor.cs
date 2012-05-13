using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.IO;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using System.Diagnostics;
using log4net;

namespace Digiphoto.Lumen.Servizi.Ritoccare {

	/// <summary>
	/// Questa classe mi serve per lanciare un editor esterno tipo GIMP
	/// per modificare le foto.
	/// </summary>
	internal class LanciatoreEditor {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(LanciatoreEditor) );

		bool _lancioSingolo;
		List<Fotografia> _fotosDaModificare;
		FileInfo [] _immaginiTemporanee;
		List<string> _gruppiDiLancio;

		public LanciatoreEditor() {
			_fotosDaModificare = new List<Fotografia>();
		}

		public LanciatoreEditor( Fotografia foto ) : this() {
			_fotosDaModificare.Add( foto );
		}

		public LanciatoreEditor( Fotografia [] fotografie ) : this() {
			_fotosDaModificare.AddRange( fotografie );
		}

		~LanciatoreEditor() {

			// Se per qualche motivo mi è rimasto qualche file temporaneo, allora lo elimino
			foreach( FileInfo f in _immaginiTemporanee ) {
				if( f.Exists ) {
					_giornale.Warn( "immagine temporanea non ancora eliminata: " + f.Name );
					try {
						f.Delete();
					} catch( Exception ) {
					}
				}
			}
		}

		public void lancia() {
			preparaLancioProcesso();
		}

		private void preparaLancioProcesso() {

			EditorEsternoConfig eCfg = LumenApplication.Instance.configurazione.editorEsternoConfig;
			_lancioSingolo = !(eCfg.gestisceMultiArgs);

			_immaginiTemporanee = creaImmaginiTemporanee();

			// Ora devo decidese se il mio programma esterno supporta il passaggio di tutti i parametri sulla riga di comando
			if( _lancioSingolo ) {
				foreach( FileInfo fileInfo in _immaginiTemporanee )
					lancioProgrammaEsterno( fileInfo.Name );
			} else {
				creaParamCommandLine( eCfg.commandLine.Length );
				foreach( string gruppo in _gruppiDiLancio ) {
					lancioProgrammaEsterno( gruppo );
				}
			}

		} 

		private void lancioProgrammaEsterno( string arguments ) {

			// Creo una nuova struttura con le informazioni per lanciare il nuovo processo.
			ProcessStartInfo pInfo = new ProcessStartInfo();
			// nome completo dell'eseguibile
			pInfo.FileName = LumenApplication.Instance.configurazione.editorEsternoConfig.commandLine;
			// i nomi delle immagini da aprire
			pInfo.Arguments = arguments;
			// la cartella temporanea di work
			pInfo.WorkingDirectory = Path.GetTempPath();

			//Start the process.
			Process p = Process.Start( pInfo );

			//Wait for the window to finish loading.
			p.WaitForInputIdle();

			//Wait for the process to end.
			p.WaitForExit();
		}


		/// <summary>
		/// Metto in fila tutti i nomi di file (senza percorso)
		/// </summary>
		/// <returns>una stringa tipo questa:
		///  "img1.jpg img2.jpg imgN.jpg"
		/// </returns>
		void creaParamCommandLine( int lenCmd ) {

			_gruppiDiLancio = new List<string>();

			StringBuilder gruppo = new StringBuilder( " " );
			foreach( FileInfo fileInfo in _immaginiTemporanee ) {

				if( lenCmd + gruppo.Length + fileInfo.Name.Length > EditorEsternoConfig.MaxLenCmd ) {
					// sfora. Mi fermo e creo un nuovo gruppo
					_gruppiDiLancio.Add( gruppo.ToString() );
					gruppo = new StringBuilder( " " );
				}

				gruppo.Append( fileInfo.Name );
				gruppo.Append( " " );
			}

			// Vediamo se è rimasto qualcosa fuori....
			if( gruppo.Length > 1 )
				_gruppiDiLancio.Add( gruppo.ToString() );
		}

		/// <summary>
		/// Creo le immagini temporanee per darle da mangiare a gimp.
		/// </summary>
		/// <param name="fotografie"></param>
		/// <returns></returns>
		private FileInfo [] creaImmaginiTemporanee() {

			// Creo un vettore uguale a quello di partenza, con i nomi dei files temporanei che vado a creare
			FileInfo [] tempFilesInfo = new FileInfo [_fotosDaModificare.Count];

			for( int ciclo = 0; ciclo < _fotosDaModificare.Count; ciclo++ ) {

				Fotografia foto = _fotosDaModificare[ciclo];
				string nomeFilePartenza = null;

				// Prima verifico se esiste un file con già l'immagine risultante. 
				// In tal caso scelgo subito questa.
				if( nomeFilePartenza == null && AiutanteFoto.esisteFileRisultante( foto ) )
					nomeFilePartenza = PathUtil.nomeCompletoRisultante( foto );

				if( nomeFilePartenza == null && foto.correzioniXml != null ) {
					// TODO qui ci sono delle modifiche da apportare che non sono state ancora applicate.
					//      devo creare la risultante e poi ripartire dall'inizio.
					//      Per ora salto questo passaggio. Non ho tempo. Rimane da fare per dopo.
				}

				// Se ancora non ho rimediato niente prendo l'immagine originale
				if( nomeFilePartenza == null )
					nomeFilePartenza = PathUtil.nomeCompletoOrig( foto );

				// Creo un nome di file temporaneo
				string tempFile = PathUtil.dammiTempFileConEstesione( Path.GetExtension( foto.nomeFile ) );

				// copio il file da modificare e tolgo il flag di read-only
				File.Copy( nomeFilePartenza, tempFile, true );
				File.SetAttributes( tempFile, FileAttributes.Normal );
				tempFilesInfo [ciclo] = new FileInfo( tempFile );
				
				Trace.WriteLine( "prima len=" + tempFilesInfo [ciclo].Length + "  wrtime=" + tempFilesInfo [ciclo].LastWriteTime.ToLongTimeString() );
			}

			return tempFilesInfo;
		}

		/// <summary>
		/// Cerco di capire quali file sono stati modificati ed aggiorno le relative Fotografie
		/// </summary>
		internal List<Fotografia> applicaImmaginiModificate() {

			List<Fotografia>modificate = new List<Fotografia>();

			for( int ii = 0; ii < _immaginiTemporanee.Length; ii++ ) {

				// Prima il file era cosi.
				FileInfo prima = _immaginiTemporanee.ElementAt( ii );
				Trace.WriteLine( "mezzo len=" + prima.Length + "  wrtime=" + prima.LastWriteTime.ToLongTimeString() );

				// Ora provo a rilegge adesso le info del file
				FileInfo dopo = new FileInfo( prima.FullName );
				Trace.WriteLine( "prima len=" + dopo.Length + "  wrtime=" + dopo.LastWriteTime.ToLongTimeString() );


				// Cerco di capire se il file è stato modificat
				if( prima.Length != dopo.Length || prima.LastWriteTime != dopo.LastWriteTime ) {

					// ok è modificato
					Fotografia fotoModif = _fotosDaModificare.ElementAt( ii );
					modificate.Add( fotoModif );

					// Ok il file è stato modificato. Allora devo copiarlo su quello finale.
					string nomeFileRisultante = PathUtil.nomeCompletoRisultante( fotoModif );

					// Eventualmente creo la cartella che contiene i files modificati (.modif)
					PathUtil.creaCartellaRisultanti( fotoModif );

					if( File.Exists( nomeFileRisultante ) )
						File.Delete( nomeFileRisultante );
					File.Move( dopo.FullName, nomeFileRisultante );
				}
			}

			return modificate;
		}
	}
}
