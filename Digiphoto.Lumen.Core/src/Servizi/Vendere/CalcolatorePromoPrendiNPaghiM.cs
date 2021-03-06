﻿using Digiphoto.Lumen.Model;
using System.Linq;
using System.Collections.Generic;
using log4net;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.Vendere;

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
					.Where( r => r.isTipoStampa /* && r.sconto == null*/ )
					.GroupBy( r => ((FormatoCarta)r.prodotto).grandezza )
					.Select( ff => new { grandez = ff.Key, conta = ff.Sum( r => r.quantita ) } );

				_mappaTotStampe = qq.ToDictionary( t => t.grandez, t => t.conta );

				#endregion carico totali x grandezza

				valorizzaMappeWork();

				foreach( var key in _mappaDaElargireStampe.Keys ) {

					// Questo è il numero di copie che devo scontare
					int quante = _mappaDaElargireStampe[key];
					var prezzoUnitario = cin.righeCarrello.FirstOrDefault( r => r.isTipoStampa && ((FormatoCarta)r.prodotto).grandezza == key ).prezzoLordoUnitario;
//						UnitOfWorkScope.currentDbContext.FormatiCarta.FirstOrDefault( fc => fc.grandezza == key && fc.attivo == true ).prezzo;
					decimal totDaScontare = quante * prezzoUnitario;

					// Ora itero le righe del carrellocon quel tipo su cui devo spalmare lo sconto
					var righe = cin.righeCarrello
						.Where( r => r.isTipoStampa && ((FormatoCarta)r.prodotto).grandezza == key );

					foreach( RigaCarrello riga in righe ) {

						var impRig = riga.quantita * riga.prezzoLordoUnitario;
						if( totDaScontare <= impRig ) {
							riga.sconto = totDaScontare;
							totDaScontare = 0;
						} else {
							riga.sconto = impRig;
							totDaScontare -= impRig;
						}

						riga.prezzoNettoTotale = GestoreCarrello.calcValoreRiga( riga );
						elargito = true;
						if( totDaScontare <= 0 )
							break;
					}
				}

			}

			#endregion Elaboro Stampe


			#region Elaboro Masterizzate

			if( promo.attivaSuFile ) {

				// Conto quante masterizzate sono a prezzo pieno
				var qta = cin.righeCarrello
					.Where( r => r.isTipoMasterizzata /* && r.sconto == null */ )
					.Sum( r => r.quantita );

				var multiploRegali = ((int)(qta / _promoPrendiNPaghiM.qtaDaPrendere));
				var qtaElarg = multiploRegali * (_promoPrendiNPaghiM.qtaDaPrendere - _promoPrendiNPaghiM.qtaDaPagare);

				_giornale.Debug( "devo elargire " + qtaElarg + " file digitali" );

				// Carico due liste: Prima quella dei file a prezzo pieno, poi quella dei file eventualmente già scontati
				var righeOmaPP = cin.righeCarrello.Where( r => r.isTipoMasterizzata && r.sconto == null );
				var righeOmaPS = cin.righeCarrello.Where( r => r.isTipoMasterizzata && r.sconto != null );

				// Faccio due giri di elargizione
				for( int ii = 1; ii <= 2; ii++ ) {

					foreach( RigaCarrello riga in (ii == 1 ? righeOmaPP : righeOmaPS) ) {

						if( qtaElarg >= riga.quantita ) {
							riga.sconto = riga.prezzoLordoUnitario;
							qtaElarg -= riga.quantita;
							riga.prezzoNettoTotale = GestoreCarrello.calcValoreRiga( riga );
							elargito = true;
						}
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
