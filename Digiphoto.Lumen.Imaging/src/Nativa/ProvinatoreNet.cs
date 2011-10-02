using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using log4net;
using Digiphoto.Lumen.Imaging;

namespace Digiphoto.Lumen.Imaging.Nativa {


	public class ProvinatoreNet : Provinatore, IProvinatore {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ProvinatoreNet ) );

		public ProvinatoreNet() {
		}


		//public Image creaImageProvino( Image imageOriginale ) {

		//   Image thumbnail = imageOriginale.GetThumbnailImage( 100, 100, _callbackProvinaturaAbort, System.IntPtr.Zero );
		//   return thumbnail;
		//}

		//public Image creaImageProvino( string nomeFileOrig ) {
		//   return creaImageProvino( new Bitmap( nomeFileOrig ) );
		//}



		public bool DACANC_thumbnailCallback() {
			_giornale.Warn( @"E' stata annullata la provinatura. Come è possibile?" );
			return true;
		}


		public override Immagine creaProvino( Immagine immagineGrande ) {

			this.immagine = immagineGrande;

			ImmagineNet immagineNet = (ImmagineNet)immagineGrande;
			Image imageGrande = immagineNet.image;
			Image imagePiccola = imageGrande.GetThumbnailImage( calcW, calcH, null, System.IntPtr.Zero );

			Immagine immaginePiccola = new ImmagineNet( imagePiccola );
			return immaginePiccola;
		}


	}
}
