using Digiphoto.Lumen.Model;
using System.Linq;
using System.Collections.Generic;
using log4net;
using Digiphoto.Lumen.Database;
using System;

namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	public class CalcolatorePromoProdXProd : ICalcolatorePromozione {

		private Dictionary<string, int> _mappaDaElargireStampe = new Dictionary<string, int>();
		private Dictionary<string, int> _mappaTotStampe;
		private int _daElargireFile;

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( CalcolatorePromoProdXProd ) );

		public Carrello Applica( Carrello cin, Promozione promo, PromoContext contestoDiVendita ) {

			PromoProdXProd _promoProdXProd = (PromoProdXProd)promo;

			try {
				OrmUtil.forseAttacca<PromoProdXProd>( ref _promoProdXProd );
			} catch( Exception ) {
			}

			// NOTA: non controllo più il flag di applicazione stampe/file che c'è sul database.
			//       tanto è il prodotto di innesco che mi guida.

			var qta = cin.righeCarrello
				.Where( r => r.prezzoLordoUnitario > 0 && r.prodotto.Equals( _promoProdXProd.prodottoInnesco ) )
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

			bool elargito = false;

			// -- Ora itero le righe del carrello con quel tipo e prendo per prima le righe a prezzo pieno.

			var righeOma1 = cin.righeCarrello
				.Where( r => r.prodotto.Equals( _promoProdXProd.prodottoElargito ) && r.sconto == null );
			var righeOma2 = cin.righeCarrello
				.Where( r => r.prodotto.Equals( _promoProdXProd.prodottoElargito ) && r.sconto != null ).OrderByDescending( r => r.sconto );

			// Faccio due giri : 1) righe non ancora scontate. 2) righe scontate
			for( int ii=1; ii<2; ii++ ) {
				var righeOma = (ii == 1 ? righeOma1 : righeOma2);
				foreach( RigaCarrello riga in righeOma ) {
					if( qtaElarg >= riga.quantita ) {
						riga.sconto = riga.prezzoLordoUnitario;
						qtaElarg -= riga.quantita;
						elargito = true;
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
