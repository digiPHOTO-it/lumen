using Digiphoto.Lumen.Model;
using System.Linq;


namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	public class CalcolatorePromoStessaFotoSuFile : ICalcolatorePromozione {

		public Carrello Applica( Carrello cin, Promozione _promo, ContestoDiVendita contestoDiVendita ) {


			PromoStessaFotoSuFile promo = (PromoStessaFotoSuFile) _promo;

			// Vediamo se esiste una foto con il rispettivo file.
			foreach( RigaCarrello r in cin.righeCarrello ) {

				if( r.prezzoNettoTotale > 0 &&  r.discriminator == RigaCarrello.TIPORIGA_STAMPA ) {

					RigaCarrello rigaFile = cin.righeCarrello.SingleOrDefault( r2 => r2.isTipoMasterizzata && r2.fotografia == r.fotografia );
					if( rigaFile != null ) {
						// trovato il file che corrisponde a questa foto.
						rigaFile.prezzoLordoUnitario = promo.prezzoFile;
					}
				}
			}
			
			return null;
		}

	}
}
