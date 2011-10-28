using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Core;

namespace Digiphoto.Lumen.Servizi.Ricerca {
	
	internal class RicercatoreSrvImpl : ServizioImpl, IRicercatoreSrv {

		ParamRicercaFoto _paramRicercaFoto;
		List<ObjectParameter> _objectParameters;

		public RicercatoreSrvImpl() {
			_objectParameters = new List<ObjectParameter>();
		}

		public List<Fotografia> cerca( ParamRicercaFoto param ) {

			this._paramRicercaFoto = param;

			String esql = creaEsqlQuery();
			ObjectQuery<Fotografia> query = new ObjectQuery<Fotografia>( esql, this.objectContext, MergeOption.NoTracking );

			// Aggiungo tutti i parametri che mi ero preparato
			foreach( ObjectParameter p in _objectParameters )
				query.Parameters.Add( p );

			return query.ToList<Fotografia>();
		}

		private string creaEsqlQuery() {

			StringBuilder esql = new StringBuilder( "\n SELECT VALUE f" );
			esql.Append( "\n FROM LumenEntities.Fotografie as f" );
			esql.Append( "\n WHERE 1=1" );


			// ----- Selezioni

			if( _paramRicercaFoto.giornataIniz != null ) {
				esql.Append( "\n AND f.dataOraAcquisizione >= @giornataIniz" );
				_objectParameters.Add( new ObjectParameter( "giornataIniz", (DateTime)_paramRicercaFoto.giornataIniz ) );
			} 
			
			if( _paramRicercaFoto.giornataFine != null ) {
				esql.Append( "\n AND f.dataOraAcquisizione <= @giornataFine" );
				_objectParameters.Add( new ObjectParameter( "giornataFine", (DateTime)_paramRicercaFoto.giornataFine ) );
			}

			if( _paramRicercaFoto.didascalia != null ) {
				esql.Append( "\n AND f.didascalia like @didascalia" );
				_objectParameters.Add( new ObjectParameter( "didascalia", "%" + _paramRicercaFoto.didascalia + "%" ) );
			}

			aggiungiWhereFotografi( esql );

			aggiungiWhereEventi( esql );

			aggiungiWhereNumeriFotogrammi( esql );

			// ----- Ordinamento
			esql.Append( "\n ORDER BY f.dataOraAcquisizione, f.numero" );

			// ----- Eventuale paginazione
			if( _paramRicercaFoto.paginazioneLimit > 0 ) {
				esql.Append( "\n SKIP @skip LIMIT @limit" );
				_objectParameters.Add( new ObjectParameter( "skip", _paramRicercaFoto.paginazioneSkip ) );
				_objectParameters.Add( new ObjectParameter( "limit", _paramRicercaFoto.paginazioneLimit ) );
			}

			return esql.ToString();
		}

		/** 
		 * Siccome ho una lista di ids fotografo, devo creare una IN.
		 * Siccome quello sfigato di EF non gestisce le IN, me la devo costruire
		 * da solo.
		 * Speriamo non ci siano dei limiti
		 */
		private void aggiungiWhereFotografi( StringBuilder esql ) {

			if( _paramRicercaFoto.fotografi == null || _paramRicercaFoto.fotografi.Length == 0 )
				return;

			var listaIds = from ff in _paramRicercaFoto.fotografi
						   select ff.id;

			esql.Append( "\n AND f.fotografo.id in { " );

			int conta = 0;
			foreach( var idFotografo in listaIds ) {
				
				string parN = "idFotografo" + conta;	

				if( conta++ > 0 )
					esql.Append( ", " );
				esql.Append( "@" );
				esql.Append( parN );

				_objectParameters.Add( new ObjectParameter( parN, idFotografo ) );
			}

			esql.Append( " }" );
		}

		private void aggiungiWhereEventi( StringBuilder esql ) {

			if( _paramRicercaFoto.eventi == null || _paramRicercaFoto.eventi.Length == 0 )
				return;

			// Siccome non si riesce a testare l'entità, estraggo gli id;
			var listaIds = from ee in _paramRicercaFoto.eventi
						  select ee.id;

			// Quindi sono più di uno: creo una IN fatta in casa.
			esql.Append( "\n AND f.evento.id in { " );

			int conta = 0;
			foreach( var idEvento in listaIds ) {

				string parN = "idEvento" + conta;

				if( conta++ > 0 )
					esql.Append( ", " );
				esql.Append( "@" );
				esql.Append( parN );

				_objectParameters.Add( new ObjectParameter( parN, idEvento ) );
			}

			esql.Append( " }" );
		}

		private void aggiungiWhereNumeriFotogrammi( StringBuilder esql ) {

			if( _paramRicercaFoto.numeriFotogrammi == null )
				return;
			if( _paramRicercaFoto.numeriFotogrammi.Length == 0 )
				return;

			if( _paramRicercaFoto.numeriFotogrammi.Length == 1 ) {
				esql.Append( "\n AND f.numero = @numeroFotogramma" );
				_objectParameters.Add( new ObjectParameter( "numeroFotogramma", _paramRicercaFoto.numeriFotogrammi[0] ) );
			} else {

				// Quindi sono più di uno: creo una IN fatta in casa.

				esql.Append( "\n AND f.numero in { " );

				for( int ii = 0; ii < _paramRicercaFoto.numeriFotogrammi.Length; ii++ ) {

					string parN = "@numeroFotogramma" + ii;

					if( ii > 0 )
						esql.Append( ", " );
					esql.Append( parN );

					_objectParameters.Add( new ObjectParameter( parN.Substring( 1 ), _paramRicercaFoto.numeriFotogrammi[ii] ) );
				}

				esql.Append( " }" );
			}
		}


		private void aggiungiWhereFaseDelGiorno( StringBuilder esql ) {

			if( _paramRicercaFoto.fasiDelGiorno == null )
				return;
			if( _paramRicercaFoto.fasiDelGiorno.Length == 0 )
				return;

			if( _paramRicercaFoto.fasiDelGiorno.Length == 1 ) {
				esql.Append( "\n AND f.faseDelGiorno = @faseDelGiorno" );
				_objectParameters.Add( new ObjectParameter( "faseDelGiorno", _paramRicercaFoto.fasiDelGiorno[0] ) );
			} else {

				// Quindi sono più di uno: creo una IN fatta in casa.
				esql.Append( "\n AND f.faseDelGiorno in { " );

				for( int ii = 0; ii < _paramRicercaFoto.fasiDelGiorno.Length; ii++ ) {

					string parN = "@faseDelGiorno" + ii;

					if( ii > 0 )
						esql.Append( ", " );
					esql.Append( parN );

					_objectParameters.Add( new ObjectParameter( parN.Substring( 1 ), _paramRicercaFoto.fasiDelGiorno[ii] ) );
				}

				esql.Append( " }" );
			}
		}
	}
}
