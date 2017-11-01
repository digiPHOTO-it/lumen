using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	/// <summary>
	/// Questa inner class serve ad effettuare una ricerca dicotomica della pagina in cui si trova un fotogramma.
	/// Uso una inner-class perché ho bisogno di utilizzare alcuni metodi di questa classe padre
	/// </summary>

	public class RicercatorePaginaDicotomicoPosiz {

		private string where { get; set; }

		private string orderBy { get; set; }

		private uint numFotogrammaDaric { get; set; }

		public uint paginaAttuale { get; private set; }

		public uint paginaMin { get; set; }
		public uint paginaMax { get; set; }

		private ushort ampiezzaPagina { get; set; }

		private List<Object> sqlParam;

		public RicercatorePaginaDicotomicoPosiz( ParamCercaFoto paramCercaFoto ) {

			// Siccome in questa ricerca i parametri non cambiano mai,
			// E' inutile che perdo tempo ad ogni iterazione a fare la stessa cosa.
			// Li preparo prima e li passo alla funzione ricorsiva
			sqlParam = new List<object>();
			where = RicercatoreSrvImpl.creaQuerySQLWhere( paramCercaFoto, ref sqlParam );

			// Stessa cosa per l'order-by
			orderBy = RicercatoreSrvImpl.creaQuerySQLOrderBy( paramCercaFoto );

			// Per default, imposto il limite ad un valore impossibile. Non posso effettuare una ricerca in questo modo
			paginaMin = 1;
			paginaMax = uint.MaxValue;

			ampiezzaPagina = (ushort) paramCercaFoto.paginazione.take;
		}

		public uint cercaPagina( uint numFotogrammaDaric ) {

			this.numFotogrammaDaric = numFotogrammaDaric;

			var pagina = cercaPaginaRicorsivo( paginaMin, paginaMax );

			return pagina;
		}

		/// <summary>
		/// Qualcuno deve averlo già inizializzato a priori
		/// </summary>
		private LumenEntities objectContext {
			get {
				return UnitOfWorkScope.currentDbContext;
			}
		}

		private System.Data.Entity.Database database {
			get {
				return objectContext.Database;
			}
		}


		/// <summary>
		/// Metodo ricorsivo dicotomico.
		/// Dati i due estremi, mi posiziono in mezzo e ricavo il numero minimo e massimo di fotogramma in quella pagina.
		/// Poi mi sposto nella metà di sinistra o di destra in base al risultato.
		/// Continuo a cercare di metà in metà fino a che non trovo il fotogramma desiderato.
		/// </summary>
		/// <param name="limiteInf"></param>
		/// <param name="limiteSup"></param>
		/// <param name="param"></param>
		/// <param name="numDaRic"></param>
		/// <returns>il numero della pagina in cui ho trovato il fotogramma richiesto</returns>

		private uint cercaPaginaRicorsivo( uint limiteInf, uint limiteSup ) {

			StringBuilder sql = new StringBuilder();

			uint middlePage = (uint)((limiteInf + limiteSup) / 2);

			// Se sono fuori dai range ammissibili, significa che il numero di foto da ricecare non è stato trovato
			if( middlePage < limiteInf || limiteSup < 0 )
				return 0;

			sql.Append( "Select min(q.numero) minimo, max(q.numero) massimo" );
			sql.Append( RicercatoreSrvImpl.SEPAR );
			sql.Append( "from (" );
			sql.Append( RicercatoreSrvImpl.SEPAR );

			StringBuilder innerSql = new StringBuilder();

			innerSql.Append( "select f.numero " );
			innerSql.Append( RicercatoreSrvImpl.SEPAR );
			innerSql.Append( " from fotografie f " );
			innerSql.Append( RicercatoreSrvImpl.SEPAR );

			// .. clausola where
			innerSql.Append( where );

			innerSql.Append( RicercatoreSrvImpl.SEPAR );

			// .. clausola order-by
			innerSql.Append( orderBy );

			uint skip = ampiezzaPagina * (middlePage - 1);
			innerSql.AppendFormat( " LIMIT {0} , {1} ", skip, ampiezzaPagina );
			innerSql.Append( RicercatoreSrvImpl.SEPAR );

			sql.Append( innerSql );

			sql.Append( ") q" );

			var query2 = database.SqlQuery<AppoNumNum>( sql.ToString(), sqlParam.ToArray() );
			AppoNumNum appo = query2.ToList() [0];

			//  Vediamo se nella pagina attuale ho trovato il numero di foto che sto cercando
			if( appo.minimo <= numFotogrammaDaric && appo.massimo >= numFotogrammaDaric )
				// Trovato
				return middlePage;
			else {

				if( appo.massimo < numFotogrammaDaric ) {
					// salgo
					limiteInf = (ushort)(middlePage + 1);
				} else {
					// Scendo
					limiteSup = (ushort)(middlePage - 1);
				}

				return cercaPaginaRicorsivo( limiteInf, limiteSup );
			}
		}
	}

}
