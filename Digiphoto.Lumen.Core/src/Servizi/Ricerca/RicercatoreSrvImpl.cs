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


		public RicercatoreSrvImpl() {
		}

		public List<Fotografia> cerca( ParamCercaFoto param ) {
			
			_giornale.Debug( "Parametri di ricerca:\n" + param );

			IQueryable<Fotografia> query = creaQueryEntita( param );

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

		/// <summary>
		/// Eseguo la stessa query che faccio per le fotografie,
		/// ma mi faccio tornare soltanto i nomi dei files.
		/// </summary>
		public List<string> cercaNomi( ParamCercaFoto param ) {
			IQueryable<string> queryNomi = creaQueryNomi( param );
			return queryNomi.ToList();
		}


		/// <summary>
		///  Creo la query per cercare le foto, però invece che tornarmi 
		///  le Fotografia, mi faccio ritornare solo i nomi dei files.
		///  Mi servirà per lo slide-show, dove non voglio tenermi tutte le
		///  immagini in memoria, ma le carico una alla volta.
		/// </summary>
		/// <returns></returns>
		private IQueryable<string> creaQueryNomi( ParamCercaFoto param ) {

			var qEntita = creaQueryEntita( param );
			var q2 = from ff in qEntita
					 select ff.nomeFile;

			return q2;
		}



		private IQueryable<Fotografia> creaQueryEntita( ParamCercaFoto param ) {

			IQueryable<Fotografia> query = from ff in this.objectContext.Fotografie
										   orderby ff.dataOraAcquisizione, ff.numero
										   select ff;
			// ----- Filtro eventi
			if( param.eventi != null ) {
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				var listaIds = from le in param.eventi
							   select le.id;
				query = query.Where( ff => listaIds.Contains( ff.evento.id ) );
			}

			// ----- Filtro fotografo
			if( param.fotografi != null ) {
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				var listaIds = from le in param.fotografi
							   select le.id;
				query = query.Where( ff => listaIds.Contains( ff.fotografo.id ) );
			}

			// ----- numeri di fotogramma
			if( param.numeriFotogrammi != null )
				query = query.Where( ff => param.numeriFotogrammi.Contains( ff.numero ) );

			// ----- fasi del giorno (la Enum non prevede il Contains. Devo trasformarla in una array di interi
			if( param.fasiDelGiorno != null && param.fasiDelGiorno.Count > 0 ) {
				IEnumerable<short> fasiInt = from p in param.fasiDelGiorno
											 select Convert.ToInt16( p );
				query = query.Where( ff => fasiInt.Contains( (short)ff.faseDelGiorno ) );
			}

			// ----- Didascalia (le didascalie le memorizziamo solo in maiuscolo)
			if( param.didascalia != null )
				query = query.Where( ff => ff.didascalia.Contains( param.didascalia.ToUpper() ) );

			// ----- Giornata Inizio
			if( param.giornataIniz != null )
				query = query.Where( ff => ff.giornata >= param.giornataIniz );

			// ----- Giornata Fine
			if( param.giornataFine != null )
				query = query.Where( ff => ff.giornata <= param.giornataFine );

			return query;
		}

	}
}
