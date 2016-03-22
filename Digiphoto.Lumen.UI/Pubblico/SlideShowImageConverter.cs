using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Digiphoto.Lumen.UI.Pubblico {

	public class SlideShowImageConverter : IMultiValueConverter {

		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {

			Fotografia fotografia = (Fotografia)values[0];
			SlideShowViewModel ssViewModel = (SlideShowViewModel)values[1];

			IdrataTarget quale;
			if( ssViewModel.slideShowRighe == 1 && ssViewModel.slideShowColonne == 1 ) {

				quale = AiutanteFoto.qualeImmagineDaStampare( fotografia );
				AiutanteFoto.idrataImmagineDaStampare( fotografia );

			} else {
				quale = IdrataTarget.Provino;
			}

			IImmagine immagine = AiutanteFoto.getImmagineFoto( fotografia, quale );

			ImageSource imageSource = ( (ImmagineWic)immagine).bitmapSource as ImageSource;
			return imageSource;

		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
