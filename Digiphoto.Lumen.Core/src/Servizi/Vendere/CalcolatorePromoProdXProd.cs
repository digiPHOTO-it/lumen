using Digiphoto.Lumen.Model;
using System.Linq;
using System.Collections.Generic;
using log4net;

namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	public class CalcolatorePromoProdXProd : ICalcolatorePromozione {

		private Dictionary<string, int> _mappaDaElargireStampe = new Dictionary<string, int>();
		private Dictionary<string, int> _mappaTotStampe;
		private int _daElargireFile;
		private PromoProdXProd _promoProdXProd;

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( CalcolatorePromoProdXProd ) );

		public Carrello Applica( Carrello cin, Promozione promo, PromoContext contestoDiVendita ) {

			this._promoProdXProd = (PromoProdXProd)promo;
	
			// Non entro in promozione: esco subito
			if( !promo.attivaSuStampe )
				return cin;
			
			var qta = cin.righeCarrello
				.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA &&
						r.prodotto.Equals( _promoProdXProd.prodottoInnesco ) )
				.Sum( r2 => r2.quantita );

			// Non entro in promozione: esco subito
			if( qta < _promoProdXProd.qtaInnesco )
				return cin;


			// Ok la quantità è sufficiente per cascare nella promo.
			// Ora agisco sul carrello

			// Ora determino quante foto di quella misura posso elargire.
			var multiploRegali = ((int)(qta / _promoProdXProd.qtaInnesco));
			var qtaElarg = multiploRegali * (_promoProdXProd.qtaElargito);
			_giornale.Debug( "devo elargire " + qtaElarg + " omaggio" );

			// Ora itero le righe del carrello con quel tipo
			var righeOma = cin.righeCarrello
				.Where( r => r.prodotto.Equals( _promoProdXProd.prodottoElargito ) );

			bool elargito = false;
			foreach( RigaCarrello riga in righeOma ) {

				if( qtaElarg >= riga.quantita ) {
					riga.sconto = riga.prezzoLordoUnitario;
					qtaElarg -= riga.quantita;

					elargito = true;
				}
			}

			// Aggiungo la promo alla lista di quelle elargite
			if( elargito && contestoDiVendita.promoApplicate.Contains( promo ) == false )
				contestoDiVendita.promoApplicate.Add( promo );

			return cin;
		}



	}
}
