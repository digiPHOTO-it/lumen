using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Core;
using log4net;

namespace Digiphoto.Lumen.Servizi.Ricerca {
	
	internal class RicercatoreSrvImpl : ServizioImpl, IRicercatoreSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( RicercatoreSrvImpl ) );

		ParamCercaFoto _paramRicercaFoto;

		public RicercatoreSrvImpl() {
		}

		public List<Fotografia> cerca( ParamCercaFoto param ) {
			
			this._paramRicercaFoto = param;

			_giornale.Debug( "Parametri di ricerca:\n" + param );

			IQueryable<Fotografia> query = creaQuery();

			// Eventuale paginazione dei risultati
			if( param.paginazione != null )
				query = query.Skip( param.paginazione.skip ).Take( param.paginazione.take );

			// Eventuale debug della query
			if( _giornale.IsDebugEnabled ) {
				ObjectQuery<Fotografia> oq = (ObjectQuery<Fotografia>)query;
				string sql = oq.ToTraceString();
				_giornale.Debug( sql );
			}

			return query.ToList();
		}

		private IQueryable<Fotografia> creaQuery() {

			
			IQueryable<Fotografia> query = from ff in this.objectContext.Fotografie
										   orderby ff.dataOraAcquisizione, ff.numero
										   select ff;

			// ----- Filtro eventi
			if( _paramRicercaFoto.eventi != null ) {
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				var listaIds = from le in _paramRicercaFoto.eventi
							   select le.id;
				query = query.Where( ff => listaIds.Contains( ff.evento.id ) );
			}


			// ----- Filtro fotografo
			if( _paramRicercaFoto.fotografi != null ) {
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				var listaIds = from le in _paramRicercaFoto.fotografi
							   select le.id;
				query = query.Where( ff => listaIds.Contains( ff.fotografo.id ) );
			}

			// ----- numeri di fotogramma
			if( _paramRicercaFoto.numeriFotogrammi != null )
				query = query.Where( ff => _paramRicercaFoto.numeriFotogrammi.Contains( ff.numero ) );

			// ----- fasi del giorno (la Enum non prevede il Contains. Devo trasformarla in una array di interi
			if( _paramRicercaFoto.fasiDelGiorno != null ) {
				IEnumerable<short> fasiInt = from p in _paramRicercaFoto.fasiDelGiorno
											 select Convert.ToInt16( p );
				query = query.Where( ff => fasiInt.Contains( (short)ff.faseDelGiorno ) );
			}

			// ----- Didascalia (le didascalie le memorizziamo solo in maiuscolo)
			if( _paramRicercaFoto.didascalia != null )
				query = query.Where( ff => ff.didascalia.Contains( _paramRicercaFoto.didascalia.ToUpper() ) );


			// ----- Giornata Inizio
			if( _paramRicercaFoto.giornataIniz != null )
				query = query.Where( ff => ff.giornata >= _paramRicercaFoto.giornataIniz );

			// ----- Giornata Fine
			if( _paramRicercaFoto.giornataFine != null )
				query = query.Where( ff => ff.giornata <= _paramRicercaFoto.giornataFine );

			return query;
		}

	}
}
