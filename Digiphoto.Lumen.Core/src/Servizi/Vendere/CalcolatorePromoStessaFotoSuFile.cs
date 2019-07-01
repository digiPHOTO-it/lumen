using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Vendere;
using log4net;
using System.Linq;


namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	public class CalcolatorePromoStessaFotoSuFile : ICalcolatorePromozione {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( CalcolatorePromoStessaFotoSuFile ) );

		public Carrello Applica( Carrello cin, Promozione _promo, PromoContext contestoDiVendita ) {

			PromoStessaFotoSuFile promo = (PromoStessaFotoSuFile) _promo;

			bool elargito = false;

			// Vediamo se esiste una foto con il rispettivo file.
			foreach( RigaCarrello r in cin.righeCarrello ) {

				if( r.prezzoNettoTotale > 0 && r.discriminator == RigaCarrello.TIPORIGA_STAMPA ) { 

					RigaCarrello rigaFile = cin.righeCarrello.SingleOrDefault( r2 => r2.isTipoMasterizzata && r2.fotografia == r.fotografia );
					if( rigaFile != null ) {
						// trovato il file che corrisponde a questa foto.
						rigaFile.sconto = rigaFile.prezzoLordoUnitario - promo.prezzoFile;

						rigaFile.prezzoNettoTotale = GestoreCarrello.calcValoreRiga( rigaFile );
						elargito = true;

						_giornale.Debug( "Elargita per foto num. " + rigaFile.fotografia.numero );
					}
				}
			}

			// Aggiungo la promo alla lista di quelle elargite
			if( elargito && contestoDiVendita.promoApplicate.Contains( promo ) == false )
				contestoDiVendita.promoApplicate.Add( promo );

			return cin;
		}

	}
}
