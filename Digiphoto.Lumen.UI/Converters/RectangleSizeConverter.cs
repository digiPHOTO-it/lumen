using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Digiphoto.Lumen.Imaging;

namespace Digiphoto.Lumen.UI.Converters {

	/// <summary>
	/// Questo converter serve per dimensionare dei rettangoli che 
	/// vengono disegnati SOPRA alla Image della foto, per nascondere l'area che 
	/// verrà ritagliata durate la stampa.
	/// </summary>
	public class RectangleSizeConverter : IMultiValueConverter {

		/// <summary>
		/// Abbiamo due rettangoli che servono per "coprire" la zona non stampabile della foto. Chiamiamoli a) e b)
		/// Questi rettangoli hanno 4 valori di posizionamento (L, T, W, H) che sono dipendenti dalla grandezza
		/// della foto, e dal suo orientamento.
		/// 
		/// </summary>
		/// <param name="values">
		///   0)  il ratio dell'area stampabile di riferimento per calcolare il taglio.
		///   1)  Image.Width        = è la larghezza del componente Image (che contiene la foto)
		///   2)  Image.Height       = è la altezza   del componente Image (che contiene la foto)
		///   3)  Image.ActualWidth  = è la larghezza effettiva della foto (dovuta allo stretch=Uniform)
		///   4)  Image.ActualHeight = è la altezza   effettiva della foto (dovuta allo stretch=Uniform)
		/// </param>
		/// <param name="targetType"></param>
		/// <param name="parameter">
		///   Una stringa separata da punti e virgola:
		///   esempio:    a,W    
		///   significa che sto trattando il rettangolo a) e che voglio avere in output la Width del rettangolo a)
		/// </param>
		/// <param name="culture"></param>
		/// <returns></returns>

		public object Convert( object [] values, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			string[] pezzi = ((string)parameter).Split( ';' );

			CalcolatoreAreeRispetto.Fascia qualeFascia = (CalcolatoreAreeRispetto.Fascia)( pezzi[0][0] );  // Può valere:  a, b
			CalcolatoreAreeRispetto.Dimensione qualeDimens = (CalcolatoreAreeRispetto.Dimensione)( pezzi[1][0] );  // Può valere:  W, H, L, T
			string flagBordi = null;
			if( pezzi.Length > 2 )
				flagBordi = pezzi[2];        // Può valere "T" oppure niente


			// Ricavo dal vettore dei parametri delle variabili con un nome più chiaro.
			float ratioCarta = (float)values[0];

			CalcolatoreAreeRispetto.Geo imageGeo = new CalcolatoreAreeRispetto.Geo();
			imageGeo.w = (double)values[1];
			imageGeo.h = (double)values[2];

			CalcolatoreAreeRispetto.Geo imageActualGeo = new CalcolatoreAreeRispetto.Geo();
			imageActualGeo.w = (double)values[3];
			imageActualGeo.h = (double)values[4];


			object ret = null;

			if( flagBordi == "T" ) {
				CalcolatoreAreeRispetto.Bordi bordo = CalcolatoreAreeRispetto.calcolcaLatiBordo( qualeFascia, ratioCarta, imageGeo, imageActualGeo );
				Thickness t = new Thickness();
				if( bordo.left )
					t.Left = spessoreLinea;
				if( bordo.top )
					t.Top = spessoreLinea;
				if( bordo.bottom )
					t.Bottom = spessoreLinea;
				if( bordo.right )
					t.Right = spessoreLinea;

				ret = t;

			} else
				ret = CalcolatoreAreeRispetto.calcolaDimensione( qualeFascia, qualeDimens, ratioCarta, imageGeo, imageActualGeo );

			return ret;
		}

		private const int spessoreLinea = 2;

#if false
spostato tutto in apposita classe CalcolaAreaRispetto
		private object calcolcaLatiBordo( string qualeFascia, object[] values ) {

			double w = calcolaDimensione( qualeFascia, "W", values );
			double h = calcolaDimensione( qualeFascia, "H", values );

// TODO stabilire con Ciccio se meglio vedere area microscopica oppure non vederla per nulla.
#if false
			if( w < 5 || h < 5 )
				return null;
#endif

			Thickness t = new Thickness( 0, 0, 0, 0 );

			if( qualeFascia == "a" ) {
				if( w > h )
					t.Bottom = spessoreLinea;
				else
					t.Right = spessoreLinea;
			} else if( qualeFascia == "b" ) {
				if( w > h )
					t.Top = spessoreLinea;
				else
					t.Left = spessoreLinea;
			}

			return t;
		}

		private double calcolaDimensione( string qualeFascia, string qualeDimens, object[] values ) {

			// Ricavo dal vettore dei parametri delle variabili con un nome più chiaro.
			float ratioCarta = (float)values[0];

			if( ratioCarta == 0 ) // probabilmente non è indicata nemmeno una stampante
				return 0d;

			double imageW = (double)values [1];
			double imageH = (double)values [2];
			double imageActualW = (double)values [3];
			double imageActualH = (double)values [4];

			if( imageW == 0d || imageH == 0d )
				return 0d;

			if( imageActualW == 0d || imageActualH == 0d )
				return 0d;

			// -----

			float ratioImage = (float)(imageActualW / imageActualH);

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
			Int32Rect fotoSorgente = new Int32Rect( 0, 0, (int)imageActualW, (int)imageActualH );
			
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
			if( qualeDimens == "W" ) {

				if( eseguoTaglioOrizz )
					ret = (double)proiezione.sorg.Width;
				else
					ret = (double)proiezione.sorg.X;
			}

			// Height
			if( qualeDimens == "H" ) {

				if( eseguoTaglioOrizz )
					ret = (double)proiezione.sorg.Y;
				else
					ret = (double)proiezione.sorg.Height;
			}

			// Left (del canvas)
			if( qualeDimens == "L" ) {
				double offsetX = (imageW - imageActualW) / 2;
				if( qualeFascia == "b" && eseguoTaglioOrizz == false )
					offsetX += (imageActualW - proiezione.sorg.X);
				ret = offsetX;
			}

			// Top (del canvas)
			if( qualeDimens == "T" ) {

				double offsetY = (imageH - imageActualH) / 2;
				if( qualeFascia == "b" && eseguoTaglioOrizz == true )
					offsetY += (imageActualH - proiezione.sorg.Y);
				ret = offsetY;
			}

			return ret;
		}
#endif

		public object [] ConvertBack( object value, Type [] targetTypes, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
