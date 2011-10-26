using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects;
using Digiphoto.Lumen.Database;

namespace Digiphoto.Lumen.Servizi.Ricerca {
	
	internal class RicercatoreSrvImpl : ServizioImpl, IRicercatoreSrv {

		ParamRicercaFoto _paramRicercaFoto;
		List<ObjectParameter> _objectParameters;

		public RicercatoreSrvImpl() {
			_objectParameters = new List<ObjectParameter>();
		}

		public IList<Fotografia> cerca( ParamRicercaFoto param ) {

			this._paramRicercaFoto = param;

			String esql = creaEsqlQuery();


			ObjectQuery<Fotografia> query = new ObjectQuery<Fotografia>( esql, this.objectContext, MergeOption.NoTracking );

			// Aggiungo tutti i parametri che mi ero preparato
			foreach( ObjectParameter p in _objectParameters )
				query.Parameters.Add( p );

			return query.ToList<Fotografia>();
		}

		private string creaEsqlQuery() {

			StringBuilder esql = new StringBuilder( @"SELECT VALUE f" );
			esql.Append( "\nFROM LumenEntities.Fotografie as f" );
			esql.Append( "\nWHERE 1=1" );


			// ----- Selezioni

			if( _paramRicercaFoto.giornataIniz != null ) {
				esql.Append( "\nAND f.dataOraAcquisizione >= @giornataIniz" );
				_objectParameters.Add( new ObjectParameter( "giornataIniz", (DateTime)_paramRicercaFoto.giornataIniz ) );
			} 
			
			if( _paramRicercaFoto.giornataFine != null ) {
				esql.Append( "\nAND f.dataOraAcquisizione <= @giornataFine" );
				_objectParameters.Add( new ObjectParameter( "giornataFine", (DateTime)_paramRicercaFoto.giornataFine ) );
			}

			// ----- Ordinamento
			esql.Append( "\norder by f.dataOraAcquisizione, f.numero" );


			return esql.ToString();
		}

	}
}
