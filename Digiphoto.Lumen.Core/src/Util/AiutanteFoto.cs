using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Imaging.Correzioni;
using log4net;
using System.IO;
using Digiphoto.Lumen.Config;
using System.Diagnostics;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Eventi;
using System.Text.RegularExpressions;

namespace Digiphoto.Lumen.Util {
	
	public enum IdrataTarget {
		Tutte = 0xff,
		Originale = 0x02,
		Provino = 0x04,
		Risultante = 0x08
	}

	public static class AiutanteFoto {

		private static readonly ILog _giornale = LogManager.GetLogger(typeof(AiutanteFoto));

		private static AreaRispetto areaRispetto;

		static AiutanteFoto() {
			float ratio = Convert.ToSingle( CoreUtil.evaluateExpression( Configurazione.UserConfigLumen.expRatioAreaDiRispetto ) );
			areaRispetto = new AreaRispetto( ratio );
		}

		public static void disposeImmagini( Fotografia foto ) {
			disposeImmagini( foto, IdrataTarget.Tutte );
		}

		public static void disposeImmagini( Fotografia foto, IdrataTarget quale ) {

			// evito errori
			if( foto == null )
				return;

			if( quale == IdrataTarget.Tutte || quale == IdrataTarget.Originale ) {
				if( foto.imgOrig != null ) {
					foto.imgOrig.Dispose();
					foto.imgOrig = null;
				}
			}

			if( quale == IdrataTarget.Tutte || quale == IdrataTarget.Provino ) {
				if( foto.imgProvino != null ) {
					foto.imgProvino.Dispose();
					foto.imgProvino = null;
				}
			}

			if( quale == IdrataTarget.Tutte || quale == IdrataTarget.Risultante ) {
				if( foto.imgRisultante != null ) {
					foto.imgRisultante.Dispose();
					foto.imgRisultante = null;
				}
			}
		}


		/// <summary>
		/// Idrato tutte le immagini associate alla foto.
		/// Per decidere più puntualmente usare l'altro overload dove si indica il target
		/// </summary>
		public static string idrataImmaginiFoto( Fotografia foto ) {
			return idrataImmaginiFoto( foto, IdrataTarget.Tutte );
		}

		/** 
		 * Devo caricare gli attributi transienti della fotografia 
		 */
		public static string idrataImmaginiFoto( Fotografia foto, IdrataTarget target ) {
			return idrataImmaginiFoto( foto, target, false );
		}

		public static string idrataImmaginiFoto( Fotografia foto, IdrataTarget target, bool forzatamente ) {

			System.Diagnostics.Debug.Assert( foto != null );  // Non deve succedere. Punto e basta.

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			gis.idrataImmaginiFoto( foto, target, forzatamente );

			return PathUtil.nomeCompletoFile( foto, target );
		}

		public static IdrataTarget qualeImmagineDaStampare( Fotografia foto ) {

			// Se ho delle correzioni, allora devo usare il file Risultante. Se non ho modifiche, allora uso l'originale.
			IdrataTarget quale = (foto.correzioniXml == null ? IdrataTarget.Originale : IdrataTarget.Risultante);
			return quale;
		}

		/// <summary>
		/// Devo stampare la foto.
		/// Quindi mi serve indietro una immagine grande, che può essere l'originale, oppure quella modificata.
		/// L'immagine modificata, però potrebbe ancora non essere stata calcolata. In tal caso ci penso io.
		/// 
		/// </summary>
		/// <param name="foto"></param>
		/// <returns>il nome del file interessato su disco</returns>
		public static string idrataImmagineDaStampare( Fotografia foto ) {

			IdrataTarget quale = qualeImmagineDaStampare( foto );

			// Ho delle correzioni che non sono ancora state applicate. Lo faccio adesso.
			if( foto.imgRisultante == null && foto.correzioniXml != null ) {
				// Se il file esiste già su disco, allora uso quello.
				if( ! File.Exists( PathUtil.nomeCompletoRisultante( foto ) ) )
	 				creaRisultanteFoto( foto );
			}

			AiutanteFoto.idrataImmaginiFoto( foto, quale, true );

			return PathUtil.nomeCompletoFile( foto, quale );
		}

		/// <summary>
		/// Voglio ricavare l'immagine grande.
		///  Se la foto in esame possiede una immagine grande risultante (quindi modificata) prendo quella.
		///  Se invece non esiste l'immagine modificata, prendo quella grande originale.
		///  L'immagine in questione viene anche idratata
		/// </summary>
		/// <param name="foto"></param>
		/// <returns></returns>
		public static IImmagine idrataImmagineGrande( Fotografia foto ) {
			
			IImmagine immagine = null;

			if (esisteFileRisultante(foto))
			{
				AiutanteFoto.idrataImmaginiFoto(foto, IdrataTarget.Risultante);
				immagine = foto.imgRisultante;
			}

			// Se non l'ho trovata, prendo l'immagine grande originale
			if( immagine == null ) {
				AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Originale );
				immagine = foto.imgOrig;
			}
			return immagine;
		}

		/// <summary>
		/// Crea fisicamente su disco il file con il JPG del provino della foto. Se esiste viene sovrascritto.
		/// </summary>
		/// <param name="foto">la foto di riferimento</param>
		public static void creaProvinoFoto( Fotografia foto ) {
			creaProvinoFoto( PathUtil.nomeCompletoFoto( foto ), foto );
		}

		/// <summary>
		/// Crea fisicamente su disco il file con il JPG della risultante della foto. Se esiste viene sovrascritto.
		/// Se non esistono correzioni, me ne frego e scrivo comunque. Verrà un duplicato della Originale.
		/// </summary>
		/// <param name="foto">la foto di riferimento</param>
		public static void creaRisultanteFoto( Fotografia foto ) {
			creaCacheFotoSuDisco( foto, IdrataTarget.Risultante );
		}

		/// <summary>
		/// Creo il profino partendo dalla immagine indicata nel nome del file.
		/// </summary>
		/// <param name="nomeFilePartenza">Attenzione: questo è il nome del file di partenza da cui creare il provino</param>
		/// <param name="foto"></param>
		public static void creaProvinoFoto( string nomeFilePartenza, Fotografia foto ) {
			creaCacheFotoSuDisco( foto, IdrataTarget.Provino, nomeFilePartenza );
		}

		private static void creaCacheFotoSuDisco( Fotografia foto, IdrataTarget quale ) {
			creaCacheFotoSuDisco( foto, quale, PathUtil.nomeCompletoFoto(foto) );
		}

		/// <summary>
		/// Creo il Provino, e/o la Risultante di una foto e scrivo l'immagine su disco nel file indicato
		/// </summary>
		/// <param name="nomeFileSorgente">E' il nome del file della foto vera, grande</param>
		/// <param name="foto"></param>
		private static void creaCacheFotoSuDisco( Fotografia foto, IdrataTarget quale, string nomeFileOriginale ) {

			_giornale.Debug( "Creo provino foto n."  + foto.numero + "  target=" + quale );

			Debug.Assert( quale == IdrataTarget.Provino || quale == IdrataTarget.Risultante );

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			IFotoRitoccoSrv fr = LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();

			// Carico l'immagine grande originale (solo la prima volta)
			bool caricataOrig = false;
			if( foto.imgOrig == null ) {
				_giornale.Debug( "carico immagine originale da disco: " + nomeFileOriginale );
				foto.imgOrig = gis.load( nomeFileOriginale );
				caricataOrig = true;
			}

			// carico eventuali correzioni
			CorrezioniList correzioni = null;
			if( foto.correzioniXml != null )
				correzioni = SerializzaUtil.stringToObject<CorrezioniList>( foto.correzioniXml );

			// Se devo crere il provino ma la foto contiene la correzione di Zoom, 
			// devo passare dalla foto grande, altrimenti perde di qualità
			bool devoPassareDallaGrande = false;
			if( quale == IdrataTarget.Risultante )
				devoPassareDallaGrande = true;
			if( correzioni != null && correzioni.Contains( typeof(Zoom) ) )
				devoPassareDallaGrande = true;


			// Se richiesto nella configurazione, scrivo direttamente sul provino le righe tratteggiate di rispetto area stampabile
			bool aggiungiCorrezioneAreaRispetto = false;
			if( Configurazione.UserConfigLumen.imprimereAreaDiRispetto ) {
				if( quale == IdrataTarget.Provino ) {

					if( correzioni == null )
						correzioni = new CorrezioniList();
					if( !correzioni.Contains( typeof( AreaRispetto ) ) )
						aggiungiCorrezioneAreaRispetto = true;
				}
			}



			// Cancello il file Risultante da disco perchè tanto sta per cambiare.
			IImmagine immagineDestinazione = null;
			foto.imgRisultante = null;
			string nomeFileRisultante = PathUtil.nomeCompletoFile( foto, IdrataTarget.Risultante );
			if( File.Exists( nomeFileRisultante ) )
				File.Delete( nomeFileRisultante );

			// Eventuale creazione delle cartelle di destinazione (potrebbero non esistere)
			PathUtil.creaCartellaProvini( foto );
			PathUtil.creaCartellaRisultanti( foto );

			// OK partiamo!
			if( devoPassareDallaGrande )
				immagineDestinazione = (IImmagine)foto.imgOrig.Clone();		// creo un duplicato della Originale per poi lavorarci
			else
				immagineDestinazione = gis.creaProvino( foto.imgOrig );		// creo una immagine più piccola

			// applico eventuali correzioni
			if( correzioni != null ) {

				IdrataTarget tempQuale = quale;
				if( devoPassareDallaGrande && quale == IdrataTarget.Provino )
					tempQuale = IdrataTarget.Risultante;

				if( aggiungiCorrezioneAreaRispetto && tempQuale == IdrataTarget.Provino )
					correzioni.Add( areaRispetto );
				
				immagineDestinazione = fr.applicaCorrezioni( immagineDestinazione, correzioni, tempQuale );

				// NO : non funziona sempre bene.
				// Se sto facendo un provino che prevede lo zoom, devo passare dalla immagine grande,
				// quindi sono obbligato a ricalcolare la Risultante e quindi rimpicciolirla.
				// quindi per essere efficiente, salvo la Risultante che ho già pronta (cosi risparmio tempo dopo)
				if( devoPassareDallaGrande && quale == IdrataTarget.Provino ) {

					gis.save( immagineDestinazione, nomeFileRisultante );
					foto.imgRisultante = immagineDestinazione;

					// Poi la ritaglio per fare il provino buono.
					immagineDestinazione = gis.creaProvino( immagineDestinazione );

					// Aggiungo l'area di rispetto al provino
					if( aggiungiCorrezioneAreaRispetto )
						immagineDestinazione = fr.applicaCorrezione( immagineDestinazione, areaRispetto );
				}
			}

			// Salvo su disco l'immagine di destinazione
			string nomeFileDest = PathUtil.nomeCompletoFile( foto, quale );
			gis.save( immagineDestinazione, nomeFileDest );

			_giornale.Debug( "Ho ricreato il file immagine di cache: " + nomeFileDest );

			// Eventualmente chiudo l'immagine grande se l'avevo aperta io.
			// Il provino, invece lo lascio aperto (non so se mi causerà problemi di memoria)
			if( caricataOrig )
				AiutanteFoto.disposeImmagini( foto, IdrataTarget.Originale );

			// Modifico la foto che mi è stata passata.
			if( quale == IdrataTarget.Provino ) {
				foto.imgProvino = immagineDestinazione;
			}
			if( quale == IdrataTarget.Risultante )
				foto.imgRisultante = immagineDestinazione;
		}
		

		/// <summary>
		/// Mi dice se esiste già un file con una foto risultante già calcolata
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public static bool esisteFileRisultante( Fotografia foto ) {
			string fileName = PathUtil.nomeCompletoRisultante( foto );
			return File.Exists( fileName );
		}

		public static bool IsValidFotografoId( string idFotografo ) {

			bool valido;

			if( String.IsNullOrEmpty( idFotografo ) || idFotografo.Contains( ' ' ) )
				valido = false;
			else {
				valido = true;
				foreach( char c in Path.GetInvalidFileNameChars() ) {
					if( idFotografo.Contains( c ) ) {
						valido = false;
						break;
					}
				}
			}

			return valido;
		}

		public static string nomeCartellaImmaginiFotografi {
			get {
				DirectoryInfo di = new DirectoryInfo( Configurazione.UserConfigLumen.cartellaMaschere );
				return  Path.Combine( di.Parent.FullName, "Fotografi" );
			}
		}

		public static string nomeFileImgFotografo( Fotografo f ) {
			return nomeFileImgFotografo( f, false );
		}

		public static string nomeFileImgFotografo( Fotografo f, bool seNonEsisteNull ) {
			if( f == null || f.id == null ) {
				return null;
			} else {
				var nomeFile = Path.Combine(nomeCartellaImmaginiFotografi, f.id + ".jpg");
				return seNonEsisteNull && !File.Exists(nomeFile) ? null : nomeFile;
			}
		}


		public static IImmagine getImmagineFoto( Fotografia f, IdrataTarget quale ) {

			IImmagine immagine = null;

			switch( quale ) {
				case IdrataTarget.Originale:
					immagine = f.imgOrig;
					break;
				case IdrataTarget.Risultante:
					immagine = f.imgRisultante;
					break;
				case IdrataTarget.Provino:
					immagine = f.imgProvino;
					break;
			}

			return immagine;
		}


	}
}
