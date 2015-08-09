using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;


namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	/// <summary>
	/// Serve per plottare sopra l'immagine del provino, le linee di rispetto dell'area stampabile
	/// </summary>
	public class AreaRispettoCorrettore : Correttore {

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			// Ricavo la bitmap sorgente
			BitmapSource bmpSorgente = ((ImmagineWic)immagineSorgente).bitmapSource;

			AreaRispetto areaRispetto = (AreaRispetto)correzione;


			CalcolatoreAreeRispetto.Geo imageGeo = new CalcolatoreAreeRispetto.Geo();

			// Calcolo la fascia A
			imageGeo.w = immagineSorgente.ww;
			imageGeo.h = immagineSorgente.hh;
			Rect rettangoloA = CalcolatoreAreeRispetto.calcolaDimensioni( CalcolatoreAreeRispetto.Fascia.FasciaA, areaRispetto.ratio, imageGeo );
			CalcolatoreAreeRispetto.Bordi bordiA = CalcolatoreAreeRispetto.calcolcaLatiBordo( CalcolatoreAreeRispetto.Fascia.FasciaA, areaRispetto.ratio, imageGeo, imageGeo );

			// Calcolo la fascia B
			imageGeo.w = immagineSorgente.ww;
			imageGeo.h = immagineSorgente.hh;
			Rect rettangoloB = CalcolatoreAreeRispetto.calcolaDimensioni( CalcolatoreAreeRispetto.Fascia.FasciaB, areaRispetto.ratio, imageGeo );
			CalcolatoreAreeRispetto.Bordi bordiB = CalcolatoreAreeRispetto.calcolcaLatiBordo( CalcolatoreAreeRispetto.Fascia.FasciaB, areaRispetto.ratio, imageGeo, imageGeo );


			BitmapSource bmpFinale = null;

			if( areaRispetto.costringi ) {
				
				int w = 0, h = 0;

				// Devo fare un resize che preservi il canale alpha (ovviamente l'immagine potrà risultare distorta, perché non devo preservare le proporzioni
				if( bordiA.right || bordiA.left || bordiB.right || bordiB.left ) {
					// Taglio verticale: la foto è orizzontale
					w = immagineSorgente.ww - (int)rettangoloA.Width - (int)rettangoloB.Width;
					h = immagineSorgente.hh;
				}
				if( bordiA.top || bordiA.bottom || bordiB.top || bordiB.bottom ) {
					// Tagli orizzontali: la foto è verticale
					h = immagineSorgente.hh - (int)rettangoloA.Height - (int)rettangoloB.Height;
					w = immagineSorgente.ww;
				}

				bmpFinale = ResizeCorrettore.Resize( bmpSorgente, w, h, (int)bmpSorgente.DpiX );
				
			} else {

				// Create a DrawingVisual/Context to render with
				DrawingVisual drawingVisual = new DrawingVisual();
				Rect rect = new Rect( 0, 0, immagineSorgente.ww, immagineSorgente.hh );

				RenderTargetBitmap renderTargetBitmap = null;

				Pen pennaRossa = new Pen( Brushes.Red, 2 );
				pennaRossa.DashStyle = DashStyles.Dash; // tratteggio

				using( DrawingContext drawingContext = drawingVisual.RenderOpen() ) {

					drawingContext.DrawImage( bmpSorgente, rect );

					// Prima linea di rispetto : calcolo i due punti
					Point p1, p2, p3, p4;
					calcolaPunti( bordiA, rettangoloA, out p1, out p2 );
					calcolaPunti( bordiB, rettangoloB, out p3, out p4 );

					// Disegno le due linee di rispetto
					drawingContext.DrawLine( pennaRossa, p1, p2 );
					drawingContext.DrawLine( pennaRossa, p3, p4 );
				}

				renderTargetBitmap = new RenderTargetBitmap( (int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default );
				renderTargetBitmap.Render( drawingVisual );
				bmpFinale = renderTargetBitmap;
			}

			if( bmpFinale.CanFreeze )
				bmpFinale.Freeze();

			return new ImmagineWic( bmpFinale );
		}

		void calcolaPunti( CalcolatoreAreeRispetto.Bordi bordi, Rect rettangolo, out Point p0, out Point p1 ) {

			p0 = new Point();
			p1 = new Point();

			if( bordi.left == true ) {
				p0.X = rettangolo.Left;
				p0.Y = rettangolo.Top;
				//
				p1.X = p0.X;
				p1.Y = rettangolo.Top + rettangolo.Height;
			}

			if( bordi.top == true ) {
				p0.X = rettangolo.Left;
				p0.Y = rettangolo.Top;
				//
				p1.X = p0.X + rettangolo.Width;
				p1.Y = p0.Y;
			}

			if( bordi.right == true ) {
				p0.X = rettangolo.Width;
				p0.Y = rettangolo.Top;
				//
				p1.X = p0.X;
				p1.Y = p0.Y + rettangolo.Height;
			}

			if( bordi.bottom == true ) {
				p0.X = rettangolo.Left;
				p0.Y = rettangolo.Top + rettangolo.Height;
				//
				p1.X = p0.X + rettangolo.Width;
				p1.Y = p0.Y;
			}
		}


	}
}
