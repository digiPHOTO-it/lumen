using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Windows.Media.Imaging;
using System.IO;
using Digiphoto.Lumen.Config;
using System.Windows.Media;

namespace Digiphoto.Lumen.Imaging.Wic {

	/// <summary>
	/// Crea i provini.
	/// L'algoritmo è stato preso da qui:
	/// http://weblogs.asp.net/bleroy/archive/2009/12/10/resizing-images-from-the-server-using-wpf-wic-instead-of-gdi.aspx
	/// </summary>
	public class ProvinatoreWic : Provinatore {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ProvinatoreWic ) );

		//public override IImmagine creaImmagine( string nomeFile ) {

		//    // Per essere più veloce, uso un BitmapFrame (almeno ci provo)
		//    byte [] bytes = File.ReadAllBytes( nomeFile );
		//    Stream stream = new MemoryStream( bytes );
		//    BitmapFrame bitmapFrame = ReadBitmapFrame(stream);

		//    return new ImmagineWic( bitmapFrame );
		//}

		public override IImmagine creaProvino( IImmagine immagineGrande ) {

			this.immagine = immagineGrande;

			// Valorizza le due proprietà che mi servono per lanciare la Resize
			calcolaEsattaWeH();

			BitmapSource bmSource = ((ImmagineWic)immagineGrande).bitmapSource;

			BitmapFrame bfPiccola = FastResize( bmSource, calcW, calcH );
			
			// byte [] baResize = ToByteArray( bfPiccola );

			return new ImmagineWic( bfPiccola );
		}

		private static BitmapFrame FastResize( BitmapSource bmSource, int nWidth, int nHeight ) {
			TransformedBitmap tbBitmap = new TransformedBitmap( bmSource, new ScaleTransform( nWidth / bmSource.Width, nHeight / bmSource.Height, 0, 0 ) );
			return BitmapFrame.Create( tbBitmap );
		}

		private static BitmapFrame FastResize( BitmapFrame bfPhoto, int nWidth, int nHeight ) {
			TransformedBitmap tbBitmap = new TransformedBitmap( bfPhoto, new ScaleTransform( nWidth / bfPhoto.Width, nHeight / bfPhoto.Height, 0, 0 ) );
			return BitmapFrame.Create( tbBitmap );
		}



	}
}
