using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.UI.Pubblico;
using Digiphoto.Lumen.UI.Pubblico.GestioneGeometria;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Digiphoto.Lumen.UI.Converters {

	public class ProiettoreAreaConverter : IMultiValueConverter {

		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {

			double ret = Double.NaN;

			WpfScreen scr = values[0] as WpfScreen;

			// Grandezza della griglia dove visualizzo lo schermino blu virtuale


			// Se il parametro è minuscolo, allora devo gestire 
			if( values.Length > 1 ) {

				GestoreFinestrePubbliche gfp = values[1] as GestoreFinestrePubbliche;

				WpfScreen scrSS = gfp.getScreenSlideShow();

				// Se lo schermo dove è presente lo ss non è quello che sto elaborando, allora esco
				if( scrSS == null || scrSS.deviceEnum != scr.deviceEnum )
					return (double)0;


				Int32Rect dest = new Int32Rect( 0, 0, 200, 200 );
				ProiettoreArea p = new ProiettoreArea( dest );

				Int32Rect source = new Int32Rect( (int)scrSS.WorkingArea.Left, (int)scrSS.WorkingArea.Top, (int)scrSS.WorkingArea.Width, (int)scrSS.WorkingArea.Height );
				p.autoCentra = true;

				Proiezione proiezione = p.calcola( source );

				// Ora uso l'area risultante, come destinazione per la dimensione dello SS
				ProiettoreArea p2 = new ProiettoreArea( proiezione.dest );
				Int32Rect ssSorg = new Int32Rect( gfp.geomSS.Left, gfp.geomSS.Top, gfp.geomSS.Width, gfp.geomSS.Height );
				Proiezione proiezione2 = p2.calcola( ssSorg );

				if( "l".Equals( parameter ) )
					ret = proiezione2.dest.X;
				if( "T".Equals( parameter ) )
					ret = proiezione2.dest.Y;
				if( "W".Equals( parameter ) )
					ret = proiezione2.dest.Width;
				if( "H".Equals( parameter ) )
					ret = proiezione2.dest.Height;

			} else {

				Int32Rect dest = new Int32Rect( 0, 0, 200, 200 );
				ProiettoreArea p = new ProiettoreArea( dest );

				Int32Rect source = new Int32Rect( (int)scr.WorkingArea.Left, (int)scr.WorkingArea.Top, (int)scr.WorkingArea.Width, (int)scr.WorkingArea.Height );
				p.autoCentra = true;

				Proiezione proiezione = p.calcola( source );

				if( "L".Equals( parameter ) )
					ret = proiezione.dest.X;
				if( "T".Equals( parameter ) )
					ret = proiezione.dest.Y;
				if( "W".Equals( parameter ) )
					ret = proiezione.dest.Width;
				if( "H".Equals( parameter ) )
					ret = proiezione.dest.Height;
			}



			return ret;
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}


	}
}
