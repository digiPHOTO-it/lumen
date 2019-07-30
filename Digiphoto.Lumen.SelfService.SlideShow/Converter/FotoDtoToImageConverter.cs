using Digiphoto.Lumen.SelfService.SlideShow.Main;
using Digiphoto.Lumen.SelfService.SlideShow.SelfServiceReference;
using Digiphoto.Lumen.UI.Pubblico;
using log4net;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.SelfService.SlideShow.Converters {

	/// <summary>
	/// Questo converter serve per due scopi:
	/// 1) per estrarre da un oggetto sorgente, la sua relativa immagine (jpg o png)
	/// 2) per decidere se presentare la foto ad alta qualità oppure il provino in base alla situazione
	/// </summary>
	/// 
	public class FotoDtoToImageConverter : IMultiValueConverter {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( FotoDtoToImageConverter ) );

		public static bool richiedeAltaQualita( short numRighe, short numColonne ) {
			return false;
			// TODO
			// return FotoGalleryViewModel.vediAltaQualita( numRighe, numColonne );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="values">Un array di oggetti cosi: composto:</br>
		///   indice = 0 : un oggetto Fotografia
		///   indice = 1 : un viewModel (o una qualsiasi classe) che implementi l'interfaccia IContenitoreGriglia
		/// </param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>una ImageSource</returns>
		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {

			ImageSource imageSource = null;


			try {

				FotografiaDto fotografiaDto = (FotografiaDto)values[0];
				SlideShowWindowViewModel vm = (SlideShowWindowViewModel)values[1];

				imageSource = vm.GetBitmap( fotografiaDto );

			} catch( Exception ee ) {
				_giornale.Error( "estrazione immagine fallita", ee );
				// Alcune immagini possono essere rovinate o mancanti. Devo proseguire.
				imageSource = null;
			}

			return imageSource;
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
