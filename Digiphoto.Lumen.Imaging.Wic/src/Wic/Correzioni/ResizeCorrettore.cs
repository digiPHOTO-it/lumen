﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using log4net;
using System.Windows;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {


	public enum BitmapScalingMode {
		Unspecified = 0,
		Linear = 1,
		LowQuality = 1,
		HighQuality = 2,
		Fant = 2,
		NearestNeighbor = 3,
	}

	public class ResizeCorrettore : Correttore {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ResizeCorrettore ) );

		static readonly int DPI_PROVINO = 96;

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			_giornale.Debug( "Richiesta correzione: " + correzione.GetType().Name );

			long calcW, calcH;
			Resize resizeCorrezione = (Resize)correzione;
			ResizeCorrettore.calcolaEsattaWeH( immagineSorgente, resizeCorrezione.latoMax, out calcW, out calcH );

			BitmapSource bitmapSource = ((ImmagineWic)immagineSorgente).bitmapSource;


			/* I dpi sono una indicazione per la stampa. 
			 * Non impattano sui dati dell'immagine.
			 * Non impattano sulla dimensione dell'immagine.
			 * Non impattano sulla qualità dell'immagine.
			 * In pratica è solo un "consiglio" per la stampa.
			 * 
			 * Siccome credo che le stampanti termiche/sublimazione se ne fregano di questo parametro (per ovvi motivi)
			 * allora me ne sbatto.
			 * Occorrebbe fare una prova con una laser o con una InkJet per vedere se il risultato stampato cambia.
			 
			int quantiDpi;
			quantiDpi = (int) Math.Max( bitmapSource.DpiX, bitmapSource.DpiY );
			quantiDpi = (int) Math.Max( quantiDpi, DPI_PROVINO );
			*/

			BitmapSource bitmapFrame = Resize( bitmapSource, calcW, calcH, DPI_PROVINO );
			// _giornale.Debug( "effettuato resize" );
			if( bitmapFrame.CanFreeze )
				bitmapFrame.Freeze();
			ImmagineWic immagine = new ImmagineWic( bitmapFrame );
			return immagine;
		}

		/// <summary>
		/// Vedere articolo:
		/// http://weblogs.asp.net/bleroy/archive/2009/12/10/resizing-images-from-the-server-using-wpf-wic-instead-of-gdi.aspx
		/// Strano ma se non tengo conto dei dpi, anche se dico di fare il resize ad una certa larghezza, 
		/// mi crea una bitmap piu grande
		/// </summary>
		public static BitmapSource Resize( BitmapSource bitmapSource, long ww, long hh, int dpi ) {
			double newW = ww / bitmapSource.Width * dpi / bitmapSource.DpiX;
			double newH = hh / bitmapSource.Height * dpi / bitmapSource.DpiY;
			var target = new TransformedBitmap( bitmapSource, new ScaleTransform( newW, newH, 0, 0 ) );
			return BitmapFrame.Create( target );
			//return target;
		}

		/// <summary>
		/// Decide quale lato del provino tagliare e quale invece ricalcolare in proporzione
		/// </summary>
		/// <param name="calcW">Parametro di output che viene valorizzato con la nuova larghezza</param>
		/// <param name="calcH">Parametro di output che viene valorizzato con la nuova altezza</param>
		private static void calcolaEsattaWeH( IImmagine immagine, long latoMax, out long calcW, out long calcH  ) {

			calcW = 0L;
			calcH = 0L;

			if( immagine.orientamento == Orientamento.Orizzontale ) {
				if( immagine.ww > latoMax )
					calcW = latoMax;
				else
					calcW = immagine.ww;
				calcH = (long)(calcW / immagine.rapporto);
			} else {
				if( immagine.hh > latoMax )
					calcH = latoMax;
				else
					calcH = immagine.hh;
				calcW = (long)(calcH * immagine.rapporto);
			}
		}

	}
}
