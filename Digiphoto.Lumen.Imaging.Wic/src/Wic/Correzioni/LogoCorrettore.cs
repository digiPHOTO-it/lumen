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

	public class LogoCorrettore : Correttore {

		private ZoomCorrettore _zoomCorrettore;
		private RuotaCorrettore _ruotaCorrettore;

		public override IImmagine applica( IImmagine immagineSorgente, Correzione correzione ) {

			Logo logoCorrezione = (Logo)correzione;

			// Questo è il nome del file completo su disco della immagine del logo
			string nomeCompletoLogo = PathUtil.nomeCompletoLogo( logoCorrezione );
			if( ! File.Exists( nomeCompletoLogo ) ) {
				throw new FileNotFoundException( nomeCompletoLogo );
			}

			// Costruisco l'immagine di overlay
			ImmagineWic imgLogo = new ImmagineWic( nomeCompletoLogo );

			// Ricavo la bitmap sorgente
			BitmapSource bmpSorgente = ((ImmagineWic)immagineSorgente).bitmapSource;

			// -- inizio ad applicare le trasformazioni al logo
			IImmagine immagineLogoModificato = imgLogo;

			//
			// :: rotazione
			//

			if( logoCorrezione.rotazione != null ) {
				if( _ruotaCorrettore == null )
					_ruotaCorrettore = new RuotaCorrettore();
				immagineLogoModificato = _ruotaCorrettore.applica( immagineLogoModificato, logoCorrezione.rotazione );
			}

			//
			// :: zoom
			//

			if( logoCorrezione.zoom != null ) {
				if( _zoomCorrettore == null )
					_zoomCorrettore = new ZoomCorrettore();
				immagineLogoModificato = _zoomCorrettore.applica( immagineLogoModificato, logoCorrezione.zoom );
			}

			BitmapSource bitmapSourceLogoModificato = ((ImmagineWic)immagineLogoModificato).bitmapSource;

			//
			// :: traslazione
			//

			// Queste sono le coordinate su cui disegnare il logo
			Rect posiz = calcolaCoordinateLogo( bmpSorgente.PixelWidth, bmpSorgente.PixelHeight, immagineLogoModificato.ww, immagineLogoModificato.hh, logoCorrezione );

			// Creo due immagini disegnabili con i rettangoli delle immagini interessate
				// 1. Tutta la foto di sfondo
			Rect rectSotto = new Rect( 0, 0, bmpSorgente.PixelWidth, bmpSorgente.PixelHeight );
			ImageDrawing drawingSotto = new ImageDrawing( bmpSorgente, rectSotto );

				// 2. Il riquando ricalcolato del logo
			Rect rectSopra = new Rect( posiz.Left, posiz.Top, posiz.Width, posiz.Height );
			ImageDrawing drawingSopra = new ImageDrawing( bitmapSourceLogoModificato, rectSopra );

			// Adesso ho entrambe le immagini. Le sovrappongo usando un gruppo.
			DrawingGroup myDrawingGroup = new DrawingGroup();
			myDrawingGroup.Children.Add( drawingSotto );
			myDrawingGroup.Children.Add( drawingSopra );
			// Disegno il gruppo in una nuova immagine.
			Image myImage = new Image();
			myImage.Source = new DrawingImage( myDrawingGroup );

			int w = (int)rectSotto.Width;
			int h = (int)rectSotto.Height;
			// Renderizzo l'immagine finita, in una bitmap in modo da poterla ritornare.
			RenderTargetBitmap rtb = new RenderTargetBitmap( w, h, 96d, 96d, PixelFormats.Default );

			DrawingVisual dv = new DrawingVisual();
			using( DrawingContext ctx = dv.RenderOpen() ) {
				ctx.DrawDrawing( myDrawingGroup );
			}
			rtb.Render( dv );

			return new ImmagineWic( rtb );
		}

		/// <summary>
		/// Mi dice se il logo in questione, va posizionato in modo automatico (nei 4 angoli)
		/// oppure se è posizionato in modo manuale, cioè l'utente lo ha spostato a mano.
		/// </summary>
		/// <param name="logo"></param>
		/// <returns></returns>
		public static bool isLogoPosizionatoManualmente( Logo logo ) {
			return logo.traslazione != null;
		}

		public static Rect calcolaCoordinateLogo( int wi, int hi, int wl, int hl, Logo logoCorrezione ) {

			Rect posiz = Rect.Empty;

			// La traslazione devo gestirla in modo indiretto.
			// Infatti questa ha 
			// Chiamando il suo correttore non funziona. Ed è anche ovvio. Come faccio a traslare una immagine su se stessa ? Ci vuole un contenitore di riferimento.
			if( isLogoPosizionatoManualmente( logoCorrezione ) ) {
				// Posizionamento manuale. Devo riproporzionare le coordinate alla immagine di destinazione
				// TODO occorre valorizzare opportunamente "posiz".
			} else {
				// Posizionamento automatico. Calcolo io la posizione in base all'angolo specificato
				posiz = calcolaCoordinateLogoAutomatiche( wi, hi, wl, hl, logoCorrezione );
			}

			return posiz;
		}


		public static Rect calcolaCoordinateLogoAutomatiche( int wi, int hi, int wl, int hl, Logo logoCorrezione ) {

			// calcolo un margine del 5% da lasciare a sinistra/destra - alto/basso
			int percMargine, pixMargineW, pixMargineH;
			float f_rappoLogo;
			percMargine = 5;  // 5 percento di spazio tra il logo ed il bordo

			double retL;
			double retT;
			double retW;
			double retH;

			// Trasformo la percentuale in pixel
			pixMargineW = wi / 100 * percMargine;
			pixMargineH = hi / 100 * percMargine;

			f_rappoLogo = (float)wl / (float)hl;

			// --- riproporziono il logo all'immagine.
			if( f_rappoLogo >= 1 ) {
				retW = Math.Max( wi, hi ) * ((short)logoCorrezione.pcCopri) / 100;
				retH = retW / f_rappoLogo;
			} else {
				retH = Math.Max( wi, hi ) * ((short)logoCorrezione.pcCopri) / 100;
				retW = retH * f_rappoLogo;
			}

			if( logoCorrezione.posiz == Logo.PosizLogo.NordOvest || logoCorrezione.posiz == Logo.PosizLogo.SudOvest ) {
				// Ovest (sinistra)
				retL = pixMargineW;
			} else {
				// Est (destra)
				retL = wi - retW - pixMargineW;
			}

			if( logoCorrezione.posiz == Logo.PosizLogo.NordOvest || logoCorrezione.posiz == Logo.PosizLogo.NordEst ) {
				// Nord (alto)
				retT = pixMargineH;
			} else {
				// Sud (basso)
				retT = hi - retH - pixMargineH;
			}

			return new Rect( retL, retT, retW, retH );
		}



		public static Logo creaLogoDefault() {
			Logo logo = new Logo();
			logo.nomeFileLogo = Configurazione.UserConfigLumen.logoNomeFile;
			logo.pcCopri = Configurazione.UserConfigLumen.logoPercentualeCopertura;
			return logo;
		}
	}
}
