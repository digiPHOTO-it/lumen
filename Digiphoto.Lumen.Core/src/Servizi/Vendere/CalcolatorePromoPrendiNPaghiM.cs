using Digiphoto.Lumen.Model;
using System.Linq;
using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	public class CalcolatorePromoPrendiNPaghiM : ICalcolatorePromozione {

		private Dictionary<string, int> _mappaDaElargireStampe = new Dictionary<string, int>();
		private Dictionary<string, int> _mappaTotStampe;
		private int _daElargireFile;
		private PromoPrendiNPaghiM _promoPrendiNPaghiM;

		public Carrello Applica( Carrello cin, Promozione promo, PromoContext contestoDiVendita ) {

			this._promoPrendiNPaghiM = (PromoPrendiNPaghiM)promo;

			#region carico totali x grandezza

			// per prima cosa, carico la mappa con i totali delle stampe raggruppate per grandezza p,m,g

			var qq = cin.righeCarrello
				.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA )
				.GroupBy( r => r.formatoCarta.grandezza )
				.Select( ff => new { grandez = ff.Key, conta = ff.Sum( r => r.quantita ) } );

			_mappaTotStampe = qq.ToDictionary( t => t.grandez, t => t.conta );

			#endregion carico totali x grandezza


			valorizzaMappeWork();

			// Ora agisco sul carrello

			foreach( var key in _mappaDaElargireStampe.Keys ) {

				int quante = _mappaDaElargireStampe[key];

				// Ora itero le righe del carrellocon quel tipo
				var righe = cin.righeCarrello
					.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA && r.formatoCarta.grandezza == key )
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

			// ---

			#region calc elargizioni omogenee

			// Ora determino per ogni grandezza se casca in promozione e quante ne posso elargire
			List<string> keys = new List<string>( _mappaTotStampe.Keys );
			foreach( var kk in keys ) {
				var qta = _mappaTotStampe[kk];

				if( qta >= _promoPrendiNPaghiM.qtaDaPrendere ) {

					// Ok la quantità è sufficiente per cascare nella promo.

					// Ora determino quante foto di quella misura posso elargire.
					var multiploRegali = ((int)(qta / _promoPrendiNPaghiM.qtaDaPrendere));
					var qtaElarg = multiploRegali * (_promoPrendiNPaghiM.qtaDaPrendere - _promoPrendiNPaghiM.qtaDaPagare);
					_mappaDaElargireStampe.Add( kk, qtaElarg );

					// Decurto la quantità elargita dal totale delle foto di quella grandezza,
					// perché con il rimanente posso anche determinare una offerta mista.
					int newQta = _mappaTotStampe[kk] - (multiploRegali * _promoPrendiNPaghiM.qtaDaPrendere);
					_mappaTotStampe[kk] = newQta;
				}
			}

			#endregion calc elargizioni omogenee

			// ---

			#region calc elargizioni miste

			// adesso sommo i rimasugli
			int qtaRimas = _mappaTotStampe.Values.Sum();
			if( qtaRimas >= _promoPrendiNPaghiM.qtaDaPrendere ) {
				// Ok posso ancora regalare qualcosa . 
				var multiploRegali = ((int)(qtaRimas / _promoPrendiNPaghiM.qtaDaPrendere));
				var qtaElarg = multiploRegali * (_promoPrendiNPaghiM.qtaDaPrendere - _promoPrendiNPaghiM.qtaDaPagare);

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

		}

	}
}
