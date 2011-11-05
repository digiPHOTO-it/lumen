using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Imaging.Nativa.Correzioni {

	public class BiancoNeroCorrettore : Correttore {

		public BiancoNeroCorrettore() {
		}

		private static ColorMatrix _colorMatrix = new ColorMatrix( new float[][]{
			new float[] {0.299f,	0.299f,	0.299f,	0,	0},
			new float[] {0.587f,	0.587f,	0.587f,	0,	0},
			new float[] {0.114f,	0.114f,	0.114f,	0,	0},
			new float[] {0,			0,		0,		1,	0},
			new float[] {0,			0,		0,		0,	1}});


		public override Immagine applica( Immagine immagineSorgente, Correzione correzione ) {

			Image imageSorgente = ((ImmagineNet)immagineSorgente).image;

			ImageAttributes ia = new ImageAttributes();
			ia.SetColorMatrix( _colorMatrix );

			Image imageDest = new Bitmap( imageSorgente.Width, imageSorgente.Height );
			using( Graphics g = Graphics.FromImage( imageDest ) ) {

				// Definisco l'area di destinazione (è uguale a quella sorgente)
				Rectangle rr = new Rectangle( 0, 0, imageSorgente.Width, imageSorgente.Height );
				g.DrawImage( imageSorgente, rr,
					0, 0, imageSorgente.Width, imageSorgente.Height,
					GraphicsUnit.Pixel, ia );
			}

			return( new ImmagineNet( imageDest ) );
		}

	}
}
