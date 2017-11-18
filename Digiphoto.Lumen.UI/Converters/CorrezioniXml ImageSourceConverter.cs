using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;
using System.IO;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI.Converters {

	/// <summary>
	/// Questo convertitore mi serve per convertire una immagine intesa come interfaccia IImmagine
	/// in un componente renderizzabile in un controllo Image.
	/// </summary>
	public class CorrezioniXmlImageSourceConverter : IValueConverter  {

		// Una icona che è sempre la stessa
		static ImageSource iconaOverlayComposizione = null;

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			// Qui mi aspetto la stringa xml contenente le correzioni
			if( !(value is String) )
				return null;

			ImageSource imageSource = null;

			if( "ICONACOMPOSIZIONE".Equals( parameter )  ) {
				// se è presente una composizione, allora torno una icona, altrimenti nullo.

				if( iconaOverlayComposizione == null )
					iconaOverlayComposizione = caricaIconaOverlayComposizione();

				// Controllo se dentro le correzioni è presente una composizione orientabile.
				imageSource = determinaSeComposizione( (string)value ) ? iconaOverlayComposizione : null;

			} else if( "MASCHERA".Equals( parameter ) ) {

				// carico proprio la maschera
				imageSource = caricaMascheraDaCorrezioneXml( (String)value );
			}
			
			return imageSource;
		}

		private static ImageSource caricaMascheraDaCorrezioneXml( string correzioneXml ) 
		{
			ImageSource imageSource = caricaImmagineDefault();
			CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>((String)correzioneXml);

			ImmagineWic immagineMaschera = null;
			Mascheratura mascheratura = null;

			if( correzioni != null && correzioni.Contains( typeof( Mascheratura ) ) ) {
				mascheratura = (Mascheratura)correzioni.FirstOrDefault( c => c is Mascheratura );
			}
			if( correzioni != null && correzioni.Contains( typeof( MascheraturaOrientabile ) ) ) {
				MascheraturaOrientabile mo = (MascheraturaOrientabile)correzioni.FirstOrDefault( c => c is MascheraturaOrientabile );
				mascheratura = mo.mascheraturaH ?? mo.mascheraturaV;
			}


			if( mascheratura != null)
			{
				immagineMaschera = new ImmagineWic(Path.Combine(PathUtil.getCartellaMaschera(FiltroMask.MskSingole), mascheratura.nome));
			}

			if (immagineMaschera != null)
			{
				imageSource = ((ImmagineWic)immagineMaschera).bitmapSource as ImageSource;
			}
			
			return imageSource;
		}

		private static ImageSource caricaImmagineDefault()
		{
			BitmapImage image = new BitmapImage();

			try
			{
				image.BeginInit();
				image.UriSource =  new Uri(@"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/cornice_empty-48x48.png");
				image.EndInit();
			}
			catch
			{
				// Qui si potrebbe emettere una warning
			}

			return image;
		}

		private static ImageSource caricaIconaOverlayComposizione() {

			BitmapImage image = new BitmapImage();

			try {
				image.BeginInit();
				image.UriSource = new Uri( @"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/Composizione-16x16.png" );
				image.EndInit();
			} catch {
				// Qui si potrebbe emettere una warning
			}

			return image;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}

		public static bool determinaSeComposizione( string correzioniXml ) {

			bool composizione = false;

			CorrezioniList correzioniList = null;
			MascheraturaOrientabile mascheraturaOrientabile = null;

			// Controllo che l'azione corrente contenga una mascheratura orientabile	
			correzioniList = SerializzaUtil.stringToObject<CorrezioniList>( correzioniXml );
			if( correzioniList != null && correzioniList.Count > 0 ) {
				mascheraturaOrientabile = (MascheraturaOrientabile)correzioniList.SingleOrDefault( mo => mo is MascheraturaOrientabile );
				composizione = (mascheraturaOrientabile != null);
			}
			
			return composizione;
		}
	}
}
