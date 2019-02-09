using Digiphoto.Lumen.Model;
using System.Linq;
using System.Collections.Generic;
using log4net;

namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	public class CalcolatorePromoPrendiNPaghiM : ICalcolatorePromozione {

		private Dictionary<string, int> _mappaDaElargireStampe;
		private Dictionary<string, int> _mappaTotStampe;
		private int _daElargireFile;
		private PromoPrendiNPaghiM _promoPrendiNPaghiM;

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( CalcolatorePromoPrendiNPaghiM ) );

		public Carrello Applica( Carrello cin, Promozione promo, PromoContext contestoDiVendita ) {

			// Se ho già applicato la promo 3 (ingrandimento) allora questa non deve funzionare
			if( contestoDiVendita.promoApplicate.Any( p => p.id == 3 ) ) {
				_giornale.Debug( "Non applico la " + promo.GetType().Name + " perché è già stata applicata la promo 3" );
				return cin;
			}

			this._promoPrendiNPaghiM = (PromoPrendiNPaghiM)promo;
			bool elargito = false;

			#region Elaboro Stampe

			if( promo.attivaSuStampe ) {

				_mappaDaElargireStampe = new Dictionary<string, int>();

				#region carico totali x grandezza

				// per prima cosa, carico la mappa con i totali delle stampe 
				// raggruppate per grandezza p,m,g
				// e che siano a prezzo pieno (cioè che non abbiano già uno sconto)

				var qq = cin.righeCarrello
					.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA &&
							r.sconto == null )
					.GroupBy( r => ((FormatoCarta)r.prodotto).grandezza )
					.Select( ff => new { grandez = ff.Key, conta = ff.Sum( r => r.quantita ) } );

				_mappaTotStampe = qq.ToDictionary( t => t.grandez, t => t.conta );

				#endregion carico totali x grandezza

				valorizzaMappeWork();

				foreach( var key in _mappaDaElargireStampe.Keys ) {

					int quante = _mappaDaElargireStampe[key];

					// Ora itero le righe del carrellocon quel tipo
					var righe = cin.righeCarrello
						.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA && ((FormatoCarta)r.prodotto).grandezza == key )
						.Take( quante );

					foreach( RigaCarrello riga in righe ) {
						riga.sconto = riga.prezzoLordoUnitario;
						elargito = true;
					}
				}

			}

			#endregion Elaboro Stampe


			#region Elaboro Masterizzate

			if( promo.attivaSuFile ) {

				// Conto quante masterizzate sono a prezzo pieno
				var qta = cin.righeCarrello
					.Where( r => r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA &&
							r.sconto == null )
					.Sum( r => r.quantita );

				var multiploRegali = ((int)(qta / _promoPrendiNPaghiM.qtaDaPrendere));
				var qtaElarg = multiploRegali * (_promoPrendiNPaghiM.qtaDaPrendere - _promoPrendiNPaghiM.qtaDaPagare);

				_giornale.Debug( "devo elargire " + qtaElarg + " file digitali" );

				// Ora itero le righe del carrello con quel tipo
				var righeOma = cin.righeCarrello
					.Where( r => r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA &&
							r.sconto == null );

				foreach( RigaCarrello riga in righeOma ) {

					if( qtaElarg >= riga.quantita ) {
						riga.sconto = riga.prezzoLordoUnitario;
						qtaElarg -= riga.quantita;
						elargito = true;
					}
				}
			}

			#endregion Elaboro Masterizzate

			// Aggiungo la promo alla lista di quelle elargite
			if( elargito && contestoDiVendita.promoApplicate.Contains( promo ) == false )
				contestoDiVendita.promoApplicate.Add( promo );

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
