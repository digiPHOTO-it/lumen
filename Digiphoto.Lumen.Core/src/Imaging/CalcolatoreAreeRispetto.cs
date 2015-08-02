using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Digiphoto.Lumen.Imaging {
	
	/// <summary>
	/// Questo calcolatore serve per dimensionare dei rettangoli che 
	/// vengono disegnati SOPRA alla Image della foto, per nascondere l'area che 
	/// verrà ritagliata durate la stampa.
	/// </summary>
	public static class CalcolatoreAreeRispetto {

		public enum Fascia {
			FasciaA = 'a',
			FasciaB = 'b'
		}

		public enum Dimensione {
			Left	= 'L',
			Top		= 'T',
			Width	= 'W',
			Height	= 'H'
		}

		public struct Bordi {

			public bool bottom {
				get;
				set;
			}
			public bool left {
				get;
				set;
			}
			public bool right {
				get;
				set;
			}
			public bool top {
				get;
				set;
			}
		}

		public struct Geo {

			public double w {
				get;
				set;
			}

			public double h {
				get;
				set;
			}
			
		}

		public struct Rettangolo {
			public double l {
				get;
				set;
			}
			public double t {
				get;
				set;
			}
			public double w {
				get;
				set;
			}
			public double h {
				get;
				set;
			}
		}

		public static Bordi calcolcaLatiBordo( Fascia qualeFascia, float ratioCarta, Geo imageGeo, Geo imageActualGeo ) {

			double w = calcolaDimensione( qualeFascia, Dimensione.Width, ratioCarta, imageGeo, imageActualGeo );
			double h = calcolaDimensione( qualeFascia, Dimensione.Height, ratioCarta, imageGeo, imageActualGeo );

			// TODO stabilire con Ciccio se meglio vedere area microscopica oppure non vederla per nulla.
#if false
			if( w < 5 || h < 5 )
				return null;
#endif

			Bordi bo = new Bordi();

			if( qualeFascia == Fascia.FasciaA ) {
				if( w > h )
					bo.bottom = true;
				else
					bo.right = true;
			} else if( qualeFascia == Fascia.FasciaB ) {
				if( w > h )
					bo.top = true;
				else
					bo.left = true;
			}

			return bo;
		}

		public static Rect calcolaDimensioni( Fascia qualeFascia, float ratioCarta, Geo imageGeo ) {

			double l = calcolaDimensione( qualeFascia, Dimensione.Left,		ratioCarta, imageGeo, imageGeo );
			double t = calcolaDimensione( qualeFascia, Dimensione.Top,		ratioCarta, imageGeo, imageGeo );
			double w = calcolaDimensione( qualeFascia, Dimensione.Width,	ratioCarta, imageGeo, imageGeo );
			double h = calcolaDimensione( qualeFascia, Dimensione.Height,	ratioCarta, imageGeo, imageGeo );

			return new Rect( l, t, w, h );
		}

		public static double calcolaDimensione( Fascia qualeFascia, Dimensione qualeDimens, float ratioCarta, Geo imageGeo, Geo imageActualGeo ) {

			if( ratioCarta == 0 ) // probabilmente non è indicata nemmeno una stampante
				return 0d;

			if( imageGeo.w == 0d || imageGeo.h == 0d )
				return 0d;

			if( imageActualGeo.w == 0d || imageActualGeo.h == 0d )
				return 0d;

			// -----

			float ratioImage = (float)(imageActualGeo.w / imageActualGeo.h);

			// I due rapporti mi indicano che sia la foto che l'area stampabile siano nella stesso verso (Orizzontali o Verticali entrambi)
			if( ratioImage < 1 && ratioCarta >= 1 || ratioImage >= 1 && ratioCarta < 1 ) {
				// non sono dello stesso verso. lo giro!
				ratioCarta = (1f / ratioCarta);
			}

			// Se la carta è più grande della foto (inteso come rapporto), allora devo tagliare sopra.
			// Questo mi garantisce il corretto funzionamento indipendentemente dal verso della foto.
			// Quindi le due bande colorate saranno sopra e sotto in orizzontale
			// Nel caso contrario saranno a sinistra e destra in verticale.
			bool eseguoTaglioOrizz = (ratioCarta > ratioImage);

			// -----

			// Ora creo due rettangoli per poter usare il ProiettoreArea
			Int32Rect fotoSorgente = new Int32Rect( 0, 0, (int)imageActualGeo.w, (int)imageActualGeo.h );

			// creo un area dello stesso orientamento
			int cartaW = (int)(1000 * ratioCarta);
			int cartaH = (int)(cartaW / ratioCarta);
			Int32Rect stampanteFinta = new Int32Rect( 0, 0, cartaW, cartaH );

			// Eseguo proiezione per capire dove e quanto verrà tagliata la foto originale.
			ProiettoreArea proiettore = new ProiettoreArea( stampanteFinta, true );
			Proiezione proiezione = proiettore.calcola( fotoSorgente );

			// -----
			//
			// Ora finalmente calcolo il valore da ritornare
			//
			// -----
			double ret = 0d;

			// Width
			if( qualeDimens == Dimensione.Width ) {

				if( eseguoTaglioOrizz )
					ret = (double)proiezione.sorg.Width;
				else
					ret = (double)proiezione.sorg.X;
			}

			// Height
			if( qualeDimens == Dimensione.Height ) {

				if( eseguoTaglioOrizz )
					ret = (double)proiezione.sorg.Y;
				else
					ret = (double)proiezione.sorg.Height;
			}

			// Left (del canvas)
			if( qualeDimens == Dimensione.Left ) {
				double offsetX = (imageGeo.w - imageActualGeo.w) / 2;
				if( qualeFascia == Fascia.FasciaB && eseguoTaglioOrizz == false )
					offsetX += (imageActualGeo.w - proiezione.sorg.X);
				ret = offsetX;
			}

			// Top (del canvas)
			if( qualeDimens == Dimensione.Top ) {

				double offsetY = (imageGeo.h - imageActualGeo.h) / 2;
				if( qualeFascia == Fascia.FasciaB && eseguoTaglioOrizz == true )
					offsetY += (imageActualGeo.h - proiezione.sorg.Y);
				ret = offsetY;
			}

			return ret;
		}

		/// <summary>
		/// Conversione di tipo giusto per pulizia di lettura del codice
		/// </summary>
		/// <param name="strFascia">Può valere:  a, b</param>
		/// <returns></returns>
		public static Fascia parseFascia( char strFascia ) {
			return (Fascia)strFascia;
		}

		/// <summary>
		/// Conversione di tipo giusto per pulizia di lettura del codice
		/// </summary>
		/// <param name="strDimensione">Può valere:  L,T,W,H</param>
		/// <returns></returns>
		public static Dimensione parseDimensione( char strDimensione ) {
			return (Dimensione)strDimensione;
		}
	}
}
