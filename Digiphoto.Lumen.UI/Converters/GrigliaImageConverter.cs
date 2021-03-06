﻿using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Gallery;
using Digiphoto.Lumen.UI.Pubblico;
using Digiphoto.Lumen.Util;
using log4net;
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

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( GrigliaImageConverter ) );

		public static bool richiedeAltaQualita( short numRighe, short numColonne ) {
			return FotoGalleryViewModel.vediAltaQualita( numRighe, numColonne );
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

				Fotografia fotografia = (Fotografia)values[0];
				IContenitoreGriglia vmContenitoreGriglia = (IContenitoreGriglia)values[1];

				IdrataTarget quale;

				// Se sto visualizzando una sola foto, oppure due affiancate su di una riga .... scelgo alta qualita
				if( vmContenitoreGriglia != null
					&& richiedeAltaQualita( vmContenitoreGriglia.numRighe, vmContenitoreGriglia.numColonne ) ) { 

					// ALTA QUALITA (più lento)
					quale = AiutanteFoto.qualeImmagineDaStampare( fotografia );
					AiutanteFoto.idrataImmagineDaStampare( fotografia );

				} else {
					// BASSA QUALITA (più veloce)
					quale = IdrataTarget.Provino;
				}


				IImmagine immagine = AiutanteFoto.getImmagineFoto( fotografia, quale );
				if( immagine != null )
					imageSource = ((ImmagineWic)immagine).bitmapSource as ImageSource;

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
