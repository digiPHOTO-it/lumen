using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

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

		static readonly int DPI_PROVINO = 96;

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			long calcW, calcH;
			ResizeCorrezione resizeCorrezione = (ResizeCorrezione)correzione;
			ResizeCorrettore.calcolaEsattaWeH( immagineSorgente, resizeCorrezione.latoMax, out calcW, out calcH );

			ImmagineWic immagineWic = (ImmagineWic)immagineSorgente;

			BitmapFrame bitmapFrame = Resize( immagineWic.bitmapSource, calcW, calcH, DPI_PROVINO );
			return new ImmagineWic( bitmapFrame );
		}

		/// <summary>
		/// Vedere articolo:
		/// http://rongchaua.net/blog/c-wpf-fast-image-resize/
		/// </summary>
		private static BitmapFrame FastResize( BitmapSource bitmapSource, long nWidth, long nHeight ) {
			TransformedBitmap tbBitmap = new TransformedBitmap( bitmapSource, new ScaleTransform( nWidth / bitmapSource.Width, nHeight / bitmapSource.Height, 0, 0 ) );
			return BitmapFrame.Create( tbBitmap );
		}


		/// <summary>
		/// Vedere articolo:
		/// http://weblogs.asp.net/bleroy/archive/2009/12/10/resizing-images-from-the-server-using-wpf-wic-instead-of-gdi.aspx
		/// </summary>
		private static BitmapFrame Resize( BitmapSource bitmapSource, long ww, long hh, int dpi ) {
			double newW = ww / bitmapSource.Width * 96 / bitmapSource.DpiX;
			double newH = hh / bitmapSource.Height * 96 / bitmapSource.DpiY;
			var target = new TransformedBitmap( bitmapSource, new ScaleTransform( newW, newH, 0, 0 ) );
			return BitmapFrame.Create( target );
		}


		private static byte [] ToByteArray( BitmapFrame bfResize ) {
			using( MemoryStream msStream = new MemoryStream() ) {
				PngBitmapEncoder pbdDecoder = new PngBitmapEncoder();
				pbdDecoder.Frames.Add( bfResize );
				pbdDecoder.Save( msStream );
				return msStream.ToArray();
			}
		}

		/// <summary>
		///  Crea un bitmapFrame partendo da uno stream binario in ingresso
		/// </summary>
		/// <param name="streamPhoto">Strem di byte in entrata</param>
		/// <returns>un BitmapFrame per poterlo anche visualizzare</returns>
		private static BitmapFrame ReadBitmapFrame( Stream streamPhoto ) {
			BitmapDecoder bdDecoder = BitmapDecoder.Create( streamPhoto, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None );
			return bdDecoder.Frames [0];
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
				if( immagine.ww > latoMax ) {
					calcW = latoMax;
					calcH = (long)(calcW / immagine.rapporto);
				}
			} else {
				if( immagine.hh > latoMax ) {
					calcH = latoMax;
					calcW = (long)(calcH * immagine.rapporto);
				}
			}
		}	
	}
}
