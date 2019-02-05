using Digiphoto.Lumen.Model;
using System.Linq;
using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	public class CalcolatorePromoProdXProd : ICalcolatorePromozione {

		private Dictionary<string, int> _mappaDaElargireStampe = new Dictionary<string, int>();
		private Dictionary<string, int> _mappaTotStampe;
		private int _daElargireFile;
		private PromoProdXProd _promoProdXProd;

		public Carrello Applica( Carrello cin, Promozione promo, PromoContext contestoDiVendita ) {

			this._promoProdXProd = (PromoProdXProd)promo;

			#region carico totali x prodotto

			// per prima cosa, carico la mappa con i totali delle stampe raggruppate per grandezza p,m,g

			var qq = cin.righeCarrello
				.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA )
				.GroupBy( r => ((FormatoCarta)r.prodotto).grandezza )
				.Select( ff => new { grandez = ff.Key, conta = ff.Sum( r => r.quantita ) } );

			_mappaTotStampe = qq.ToDictionary( t => t.grandez, t => t.conta );

			#endregion carico totali x grandezza


			valorizzaMappeWork();

			// Ora agisco sul carrello

			foreach( var key in _mappaDaElargireStampe.Keys ) {

				int quante = _mappaDaElargireStampe[key];

				// Ora itero le righe del carrellocon quel tipo
				var righe = cin.righeCarrello
					.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA && ((FormatoCarta)r.prodotto).grandezza == key )
					.Take( quante );
				foreach( RigaCarrello riga in righe )
					riga.sconto = riga.prezzoLordoUnitario;

			}

			return cin;
		}

		/// <summary>
		/// distribuisco le quantità delle elargizioni
		/// </summary>
		private void valorizzaMappeWork() {
#if TODO
			// ---

#region calc elargizioni omogenee

			// Ora determino per ogni grandezza se casca in promozione e quante ne posso elargire
			List<string> keys = new List<string>( _mappaTotStampe.Keys );
			foreach( var kk in keys ) {
				var qta = _mappaTotStampe[kk];

				if( qta >= _promoProdXProd.qtaDaPrendere ) {

					// Ok la quantità è sufficiente per cascare nella promo.

					// Ora determino quante foto di quella misura posso elargire.
					var multiploRegali = ((int)(qta / _promoProdXProd.qtaDaPrendere));
					var qtaElarg = multiploRegali * (_promoProdXProd.qtaDaPrendere - _promoProdXProd.qtaDaPagare);
					_mappaDaElargireStampe.Add( kk, qtaElarg );

					// Decurto la quantità elargita dal totale delle foto di quella grandezza,
					// perché con il rimanente posso anche determinare una offerta mista.
					int newQta = _mappaTotStampe[kk] - (multiploRegali * _promoProdXProd.qtaDaPrendere);
					_mappaTotStampe[kk] = newQta;
				}
			}

#endregion calc elargizioni omogenee

			// ---

#region calc elargizioni miste

			// adesso sommo i rimasugli
			int qtaRimas = _mappaTotStampe.Values.Sum();
			if( qtaRimas >= _promoProdXProd.qtaDaPrendere ) {
				// Ok posso ancora regalare qualcosa . 
				var multiploRegali = ((int)(qtaRimas / _promoProdXProd.qtaDaPrendere));
				var qtaElarg = multiploRegali * (_promoProdXProd.qtaDaPrendere - _promoProdXProd.qtaDaPagare);

				bool continua = true;
				string key = "P";
				do {
					if( qtaElarg > 0 ) {

						if( _mappaTotStampe.ContainsKey( key ) && _mappaTotStampe[key] > 0 ) {
							--_mappaTotStampe[key];
							if( _mappaDaElargireStampe.ContainsKey( key ) )
								++_mappaDaElargireStampe[key];
							else
								_mappaDaElargireStampe.Add( key, 1 );

							--qtaElarg;
						} else {

							if( key == "P" )
								key = "M";
							else if( key == "M" )
								key = "G";
							else
								key = null;
						}
					}

					continua = (qtaElarg > 0 && key != null);

				} while( continua );

			}

#endregion calc elargizioni miste

#endif
		}

	}
}
