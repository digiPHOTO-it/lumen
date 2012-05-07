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
			if( foto.imgOrig != null )
				foto.imgOrig.Dispose();
			if( foto.imgProvino != null )
				foto.imgProvino.Dispose();
			if( foto.imgRisultante != null )
				foto.imgRisultante.Dispose();
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
				_giornale.Warn( "Impossibile caricare immagine della foto " + foto );
			}

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

		public static void creaProvinoFoto( Fotografia foto ) {
			creaProvinoFoto( PathUtil.nomeCompletoFoto( foto ), foto );
		}

		public static void creaProvinoFoto( string nomeFileFoto, Fotografia foto ) {

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();

			// Carico l'immagine grande originale (solo la prima volta)
			if( foto.imgOrig == null )
				foto.imgOrig = gis.load( nomeFileFoto );

			// Scale per ridimensionare
			IImmagine immaginePiccola = gis.creaProvino( foto.imgOrig );

			// applico eventuali correzioni
			if( foto.correzioniXml != null ) {
				CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>( foto.correzioniXml );
				immaginePiccola = gis.applicaCorrezioni( immaginePiccola, correzioni );
			}

			// Se avevo già un provino caricato, qui lo vado a sovrascrivere, quindi devo rilasciarlo
			if( foto.imgProvino != null )
				foto.imgProvino.Dispose();

			// Salvo su disco l'immagine risultante
			foto.imgProvino = immaginePiccola;
			gis.save( immaginePiccola, PathUtil.nomeCompletoProvino( foto ) );
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


	}
}
