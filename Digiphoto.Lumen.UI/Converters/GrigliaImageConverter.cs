using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Pubblico;
using Digiphoto.Lumen.Util;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Digiphoto.Lumen.UI.Converters {

	/// <summary>
	/// Questo converter serve per due scopi:
	/// 1) per estrarre da un oggetto sorgente, la sua relativa immagine (jpg o png)
	/// 2) per decidere se presentare la foto ad alta qualità oppure il provino in base alla situazione
	/// </summary>
	/// 
	public class GrigliaImageConverter : IMultiValueConverter {

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

			Fotografia fotografia = (Fotografia)values[0];
			IContenitoreGriglia vmContenitoreGriglia = (IContenitoreGriglia)values[1];

			IdrataTarget quale;

			// Se sto visualizzando una sola foto, oppure due affiancate su di una riga .... scelgo alta qualita
			if( vmContenitoreGriglia != null 
				&& vmContenitoreGriglia.numRighe == 1 
				&& (vmContenitoreGriglia.numColonne == 1 || vmContenitoreGriglia.numColonne == 2) ) {

				// ALTA QUALITA (più lento)
				quale = AiutanteFoto.qualeImmagineDaStampare( fotografia );
				AiutanteFoto.idrataImmagineDaStampare( fotografia );

			} else {
				// BASSA QUALITA (più veloce)
				quale = IdrataTarget.Provino;
			}

			ImageSource imageSource = null;
            IImmagine immagine = AiutanteFoto.getImmagineFoto( fotografia, quale );
			if( immagine != null )
				imageSource = ((ImmagineWic)immagine).bitmapSource as ImageSource; 

			return imageSource;
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
