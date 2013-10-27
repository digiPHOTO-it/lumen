using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Imaging;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	public class CropCorrettore : Correttore {

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			BitmapSource bitmapSource = ((ImmagineWic)immagineSorgente).bitmapSource;

			Crop cropCorrezione = (Crop)correzione;
			Int32Rect rect = calcolaRettangolo( cropCorrezione, bitmapSource.PixelWidth, bitmapSource.PixelHeight );
			
			var croppedImage = new CroppedBitmap( bitmapSource, rect );

			BitmapSource newBitmap = (BitmapSource)croppedImage;
			
			return new ImmagineWic( newBitmap );
		}

		private Int32Rect calcolaRettangolo( Crop cropCorrezione, int imgWidth, int imgHeight ) {
			
			// Rettangolo di crop
			Int32Rect croppingRect = new Int32Rect(); 
			croppingRect.X = cropCorrezione.x;
			croppingRect.Y = cropCorrezione.y;
			croppingRect.Width = cropCorrezione.w;
			croppingRect.Height = cropCorrezione.h;

			// Immagine sorgente piccola
			Int32Size rectImgSorg = new Int32Size();
			rectImgSorg.Width = cropCorrezione.imgWidth;
			rectImgSorg.Height = cropCorrezione.imgHeight;

			// Immagine di destinazione grande
			Int32Size rectImgDest = new Int32Size();
			rectImgDest.Width = imgWidth;
			rectImgDest.Height = imgHeight;

			return Geometrie.proporziona( croppingRect, rectImgSorg, rectImgDest );
		}

	}
}
