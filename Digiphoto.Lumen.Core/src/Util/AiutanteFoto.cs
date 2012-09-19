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

namespace Digiphoto.Lumen.Util {
	
	public enum IdrataTarget {
		Tutte = 0xff,
		Originale = 0x02,
		Provino = 0x04,
		Risultante = 0x08
	}

	public static class AiutanteFoto {

		private static readonly ILog _giornale = LogManager.GetLogger(typeof(AiutanteFoto));

		public static void disposeImmagini( Fotografia foto ) {
			disposeImmagini( foto, IdrataTarget.Tutte );
		}

		public static void disposeImmagini( Fotografia foto, IdrataTarget quale ) {

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
		public static void idrataImmaginiFoto( Fotografia foto ) {
			idrataImmaginiFoto( foto, IdrataTarget.Tutte );
		}

		/** 
		 * Devo caricare gli attributi transienti della fotografia 
		 */
		public static void idrataImmaginiFoto( Fotografia foto, IdrataTarget target ) {
			idrataImmaginiFoto( foto, target, false );
		}

		public static void idrataImmaginiFoto( Fotografia foto, IdrataTarget target, bool forzatamente ) {

			System.Diagnostics.Debug.Assert( foto != null );  // Non deve succedere. Punto e basta.

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();

			try {	        
		
				//
				if( forzatamente || foto.imgProvino == null )
					if( (target & IdrataTarget.Provino) != 0 )
						foto.imgProvino = gis.load( PathUtil.nomeCompletoProvino( foto ) );

				//
				if( forzatamente || foto.imgOrig == null )
					if( (target & IdrataTarget.Originale) != 0 )
						foto.imgOrig = gis.load( PathUtil.nomeCompletoFoto( foto ) );

				//
				if( forzatamente || foto.imgRisultante == null )
					if( (target & IdrataTarget.Risultante) != 0 )
						foto.imgRisultante = gis.load( PathUtil.nomeCompletoRisultante( foto ) );
				
			} catch (Exception ee) {
				// Se non riesco a caricare una immagine, non posso farci niente qui. Devo tirare dritto.
				_giornale.Warn( "Impossibile caricare immagine della foto " + foto, ee );
			}

		}


		/// <summary>
		/// Devo stampare la foto.
		/// Quindi mi serve indietro una immagine grande, che può essere l'originale, oppure quella modificata.
		/// L'immagine modificata, però potrebbe ancora non essere stata calcolata. In tal caso ci penso io.
		/// 
		/// </summary>
		/// <param name="foto"></param>
		public static void idrataImmagineDaStampare( Fotografia foto ) {

			// Ho delle correzioni che non sono ancora state applicate. Lo faccio adesso.
			if( foto.imgRisultante == null && foto.correzioniXml != null ) {
				// Se il file esiste già su disco, allora uso quello.
				if( ! File.Exists( PathUtil.nomeCompletoRisultante( foto ) ) )
	 				creaRisultanteFoto( foto );
			}

			IdrataTarget quale = foto.imgRisultante != null ? IdrataTarget.Risultante : IdrataTarget.Originale;

			AiutanteFoto.idrataImmaginiFoto( foto, quale );
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

			AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Risultante );
			immagine = foto.imgRisultante;

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
		/// Creo il Provino, oppure la Risultante di una foto e scrivo l'immagine su disco nel file indicato
		/// </summary>
		/// <param name="nomeFileSorgente">E' il nome del file della foto vera, grande</param>
		/// <param name="foto"></param>
		private static void creaCacheFotoSuDisco( Fotografia foto, IdrataTarget quale, string nomeFileOriginale ) {

			_giornale.Debug( "Creo provino foto n."  + foto.numero + "  target=" + quale );

			Debug.Assert( quale == IdrataTarget.Provino || quale == IdrataTarget.Risultante );

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();

			// Carico l'immagine grande originale (solo la prima volta)
			if( foto.imgOrig == null ) {
				_giornale.Debug( "carico immagine originale da disco: " + nomeFileOriginale );
				foto.imgOrig = gis.load( nomeFileOriginale );
			}


			IImmagine immagineDestinazione = null; 
			if( quale == IdrataTarget.Provino ) {
				// Scale per ridimensionare
				immagineDestinazione = gis.creaProvino( foto.imgOrig );   // creo una immagine più piccola
				PathUtil.creaCartellaProvini( foto );
			} else if( quale == IdrataTarget.Risultante ) {
				immagineDestinazione = (IImmagine) foto.imgOrig.Clone();  // creo un duplicato
				PathUtil.creaCartellaRisultanti( foto );   // se non esiste la cartelle delle modificate, la creo al volo
			} else 
				throw new ArgumentException( "target per creazione cache non gestito : " + quale );


			// applico eventuali correzioni
			if( foto.correzioniXml != null ) {
				CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>( foto.correzioniXml );
				_giornale.Debug( "Su questa foto esistono correzioni. Le applico" );
				immagineDestinazione = gis.applicaCorrezioni( immagineDestinazione, correzioni );
			}

			// Salvo su disco l'immagine risultante
			string nomeFileDest = PathUtil.nomeCompletoFile( foto, quale );
			gis.save( immagineDestinazione, nomeFileDest );

			_giornale.Debug( "Ho ricreato il file immagine di cache: " + nomeFileDest );

			// Eventualmente chiudo l'immagine precedente (per pulizia)
			AiutanteFoto.disposeImmagini( foto, quale );

			// Modifico la foto che mi è stata passata.
			if( quale == IdrataTarget.Provino ) 
				foto.imgProvino = immagineDestinazione;
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

		/// <summary>
		/// Mi dice se la foto in esame è una maschera generata tramite fotoritocco.
		/// </summary>
		/// <param name="f"></param>
		/// <returns>true = è una maschera</returns>
		/// <returns>false = è una foto normale</returns>
		public static bool isMaschera( Fotografia f ) {
			return f.fotografo != null && Configurazione.ID_FOTOGRAFO_ARTISTA.Equals( f.fotografo.id );
		}
	}
}
