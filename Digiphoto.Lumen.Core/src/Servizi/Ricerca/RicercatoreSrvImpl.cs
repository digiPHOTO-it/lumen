using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Util;
using Digiphoto.Lumen.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	internal class RicercatoreSrvImpl : ServizioImpl, IRicercatoreSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( RicercatoreSrvImpl ) );
		internal static string SEPAR = "\r\n";

		public RicercatoreSrvImpl() {
		}

		public List<Fotografia> cerca( ParamCercaFoto param ) {

			// è stato chiesto un numero specifico di foto ma con intorno prima e dopo
			int pagina = gestisciRicercaPuntualePerNumeroEdIntorno( param );
			if( pagina > 0 )
				// mi posiziono sulla pagina che contiene la foto desiderata
				param.paginazione.skip = (pagina -1) * param.paginazione.take;
			// in ogni caso devo pulire il numerino perché al prossmo giro diventa una ricerca paginata normale	
			param.numeroConIntorno = 0;


			return cercaVeloceSQL( param );
		}

		private List<Fotografia> cercaVeloceSQL( ParamCercaFoto param ) {

			_giornale.Debug( "Parametri di ricerca:\n" + param );

			List<object> sqlParam = null;
			
			String sql = creaQuerySQL( param, false, ref sqlParam );

			var query = objectContext.Database.SqlQuery<Guid>( sql, sqlParam.ToArray() );

			DateTime t1 = DateTime.Now;
			_giornale.Debug( "Tempo1: " + t1.ToString( "mm:ss:fff" ) );

			var listaIds = query.ToList();

			DateTime t2 = DateTime.Now;
			_giornale.Debug( "Tempo2: " + t2.ToString( "mm:ss:fff" ) );

			// 
			// -----
			//       Adesso carico gli oggetti per davvero
			// -----
			//

			var listaFoto = caricaFotografieDaIds( listaIds, param );

			DateTime t3 = DateTime.Now;
			TimeSpan elapsed = t3.Subtract( t1 );
			_giornale.Debug( "Tempo3: " + t3.ToString( "mm:ss:fff" ) + " tot millis=" + elapsed.TotalMilliseconds);



			return listaFoto;
		}

		private String creaQuerySQL( ParamCercaFoto param, bool soloCount, ref List<Object> paramOut ) {

			// Creo la lista vuota dei parametri. Poi la andrò a riempire
			paramOut = new List<object>();

			StringBuilder sql = new StringBuilder();
			
			#region Select

			sql.Append( "SELECT " );
			if( soloCount )
				sql.Append( "count( f.id )" );
			else
				sql.Append( "f.id" );
			sql.Append( SEPAR );

			#endregion Select

			#region From

			sql.Append( "FROM Fotografie f " );
			sql.Append( SEPAR );

			#endregion From

			/*
				Le join probabilmente non servono in questa prima fase

						sql.Append( "inner join Fotografi o \n" );
						sql.Append( "        on o.id = f.fotografo_id \n" );

						if( !param.evitareJoinEvento ) {
							sql.Append( "inner join Eventi e \n" );
							sql.Append( "        on e.id = f.evento_id \n" );
						}
			*/

			#region Where
		
			string where = creaQuerySQLWhere( param, ref paramOut );
			sql.Append( where );

			#endregion Where

			// Nella query normale devo ordinare e paginare, mentre invece se sto solo facendo la conta dei record mi risparmio questa fatica cosi vado piu veloce
			if( ! soloCount ) {

				#region Order-By

				string orderBy = creaQuerySQLOrderBy( param );
				sql.Append( orderBy );

				#endregion Order-By

				#region Paginazione

				// Paginazione
				if( param.paginazione != null && param.paginazione.isEmpty == false ) {
					sql.Append( " LIMIT " );
					sql.Append( param.paginazione.skip );
					sql.Append( " , " );
					sql.Append( param.paginazione.take );
					sql.Append( SEPAR );
				}

				#endregion Paginazione
			}

#if DEBUG
			// Eventuale debug della query
			if( _giornale.IsDebugEnabled ) {
				_giornale.Debug( sql.ToString() );
			}
#endif

			return sql.ToString();
		}

		internal static string creaQuerySQLWhere( ParamCercaFoto param, ref List<Object> paramOut ) {
			
			StringBuilder sql = new StringBuilder();

			sql.Append( " where 1=1" );
			sql.Append( SEPAR );

			#region Where - Eventi

			// Eventi
			if( param.eventi != null ) {
				if( param.eventi.Length == 1 ) {
					sql.Append( "AND f.evento_id = {" + paramOut.Count + "}" );
					paramOut.Add( (string)param.eventi.ElementAt( 0 ).id.ToString() );  // Forzatura: devo convertirlo in stringa
				} else {
					sql.Append( "AND f.evento_id in ( " );
					foreach( var p in param.eventi ) {
						sql.Append( "{" + paramOut.Count + "}," );
						paramOut.Add( (string)p.ToString() );   // Forzatura: devo convertirlo in stringa
					}
					sql.Replace( ',', ')', sql.Length - 1, 1 );  // Rimuovo l'ultima virgola di troppo e la sostituisco con la parentesi chiusa
				}
				sql.Append( SEPAR );
			}

			#endregion Where - Eventi


			#region Where - ScaricoCard

			// ----- Filtro scarico card
			if( param.scarichiCard != null ) {
				IEnumerable<DateTime> listaDate = from le in param.scarichiCard select le.tempo;
				if( listaDate.Count() == 1 ) {
					sql.Append( "AND f.dataOraAcquisizione = {" + paramOut.Count + "}" );
					paramOut.Add( listaDate.ElementAt( 0 ) );
				} else {
					sql.Append( "AND dataOraAcquisizione in ( " );
					foreach( var d in listaDate ) {
						sql.Append( "{" + paramOut.Count + "}," );
						paramOut.Add( d );
					}
					sql.Replace( ',', ')', sql.Length - 1, 1 );  // Rimuovo l'ultima virgola di troppo e la sostituisco con la parentesi chiusa
				}
				sql.Append( SEPAR );
			}

			#endregion Where - ScaricoCard

			#region Where - Fotografo

			// ----- Filtro fotografo
			if( param.fotografi != null ) {
				if( param.fotografi.Length == 1 ) {
					sql.Append( "AND f.fotografo_id = {" + paramOut.Count + "}" );
					paramOut.Add( param.fotografi.ElementAt( 0 ).id );
				} else {
					sql.Append( "AND f.fotografo_id in ( " );
					foreach( var oo in param.fotografi ) {
						sql.Append( "{" + paramOut.Count + "}," );
						paramOut.Add( oo.id );
					}
					sql.Replace( ',', ')', sql.Length - 1, 1 );  // Rimuovo l'ultima virgola di troppo e la sostituisco con la parentesi chiusa
				}
				sql.Append( SEPAR );
			}

			#endregion Where - Fotografo

			#region Where - ids Foto

			// --- id delle fotografie
			if( param.idsFotografie != null ) {
				sql.Append( "AND f.id in ( " );
				foreach( var id in param.idsFotografie ) {
					sql.Append( "{" + paramOut.Count + "}," );
					paramOut.Add( (string)id.ToString() );
				}
				sql.Replace( ',', ')', sql.Length - 1, 1 );  // Rimuovo l'ultima virgola di troppo e la sostituisco con la parentesi chiusa
				sql.Append( SEPAR );
			}

			#endregion Where - ids Foto

			#region Where - numerifotogrammi

			// --- numeri di fotogrammi
			if( param.numeriFotogrammi != null ) {
				int [] range = FotoRangeUtil.rangeToString( param.numeriFotogrammi );

				// Testo se ho un range o una lista
				if( param.numeriFotogrammi.Contains( '-' ) ) {
					int estInf = range [0];
					int estSup = range [1];

					// ----- RANGE di fotogramma
					sql.Append( "AND f.numero >= {" + paramOut.Count + "}" );
					sql.Append( SEPAR );
					paramOut.Add( estInf );

					if( range [1] > 0 ) {
						sql.Append( "AND f.numero <= {" + paramOut.Count + "}" );
						paramOut.Add( estSup );
						sql.Append( SEPAR );
					}

				} else {
					// ----- numeri di fotogramma
					sql.Append( "AND f.numero in ( " );
					foreach( var nn in range ) {
						sql.Append( "{" + paramOut.Count + "}," );
						paramOut.Add( nn );
					}
					sql.Replace( ',', ')', sql.Length - 1, 1 );  // Rimuovo l'ultima virgola di troppo e la sostituisco con la parentesi chiusa
					sql.Append( SEPAR );
				}
			}

			#endregion Where - numerifotogrammi

			#region Where - Fasi del giorno

			// ----- fasi del giorno (la Enum non prevede il Contains. Devo trasformarla in una array di interi
			if( param.fasiDelGiorno != null && param.fasiDelGiorno.Count > 0 ) {
				IEnumerable<short> fasiInt = from p in param.fasiDelGiorno
											 select Convert.ToInt16( p );
				sql.Append( "AND f.faseDelGiorno in ( " );
				foreach( var nn in fasiInt ) {
					sql.Append( "{" + paramOut.Count + "}," );
					paramOut.Add( nn );
				}
				sql.Replace( ',', ')', sql.Length - 1, 1 );  // Rimuovo l'ultima virgola di troppo e la sostituisco con la parentesi chiusa
				sql.Append( SEPAR );
			}

			#endregion Where - Fasi del giorno


			#region Where - Didascalia

			// ----- Didascalia (le didascalie le memorizziamo solo in maiuscolo)
			if( !String.IsNullOrWhiteSpace( param.didascalia ) && param.didascalia != "%" ) {

				if( "(VUOTA)".Equals( param.didascalia, StringComparison.CurrentCultureIgnoreCase ) ) {
					// Caso particolare : voglio solo i nulli.
					sql.Append( "AND f.didascalia is null " );
				} else if( "(PIENA)".Equals( param.didascalia, StringComparison.CurrentCultureIgnoreCase ) ) {
					// Caso particolare : voglio solo i NON nulli.
					sql.Append( "AND f.didascalia is not null " );
				} else {
					sql.Append( "AND f.didascalia like " );
					sql.Append( "{" + paramOut.Count + "}" );
					paramOut.Add( param.didascalia );
					sql.Append( SEPAR );
				}
			}

			#endregion Where - Didascalia

			#region Date

			// ----- Giornata Inizio
			if( param.giornataIniz != null ) {
				sql.Append( "AND f.giornata >= " );
				sql.Append( "{" + paramOut.Count + "}" );
				paramOut.Add( param.giornataIniz );
				sql.Append( SEPAR );
			}

			// ----- Giornata Fine
			if( param.giornataFine != null ) {
				sql.Append( "AND f.giornata <= " );
				sql.Append( "{" + paramOut.Count + "}" );
				paramOut.Add( param.giornataFine );
				sql.Append( SEPAR );
			}

			#endregion Date

			// TODO

			// TODO ..... altri parametri mancanti

			sql.Append( " " );

			return sql.ToString();
		}

		internal static string creaQuerySQLOrderBy( ParamCercaFoto param ) {

			StringBuilder sql = new StringBuilder();


			// Query normale

			// ----- Ordinamento
			if( param.ordinamento != null ) {
				sql.Append( "ORDER BY " );

				sql.Append( "f.dataOraAcquisizione " );
				sql.Append( param.ordinamento );

				sql.Append( ", " );

				sql.Append( "f.numero " );
				sql.Append( param.ordinamento );

				// forse non serve. provo a far risparmiare tempo
				sql.Append( ", f.id " );
			}



			sql.Append( " " );

			return sql.ToString();
		}


		/// <summary>
		/// Data una lista di Id che sono stati ritornati dalla query,
		/// adesso carico gli oggetti Fotografia con le associazioni necessarie
		/// </summary>
		/// <param name="listaIds"></param>
		/// <returns></returns>
		private List<Fotografia> caricaFotografieDaIds( List<Guid> idsFotografie, ParamCercaFoto param ) {

			// Porto in join il fotografo tanto mi servirà per visualizzarne i dati nelle tooltip delle foto
			var qq2 = this.objectContext.Fotografie.Include( "fotografo" );

			// Porto in join gli eventi solo se richiesto esplicitamente
			if( ! param.evitareJoinEvento )
				qq2 = qq2.Include( "evento" );

			// Eseguo la query solo con gli ID e spero cosi di essere più veloce
			IQueryable<Fotografia> query2 = qq2.AsQueryable();
			query2 = query2.Where( ff => idsFotografie.Contains( ff.id ) );

			// ----- Ordinamento
			if( param.ordinamento != null ) {
				if( param.ordinamento == Ordinamento.Asc )
					query2 = query2.OrderBy( ff => ff.dataOraAcquisizione ).ThenBy( ff => ff.numero );
				if( param.ordinamento == Ordinamento.Desc )
					query2 = query2.OrderByDescending( ff => ff.dataOraAcquisizione ).ThenByDescending( ff => ff.numero );
			}

			_giornale.Debug( "Sto per eseguire la query EF per oggetti completi Fotografia" );

			var lista = query2.ToList(); ;

			_giornale.Debug( "Eseguita la query EF per oggetti completi Fotografia" );

			return lista;
		}

		int gestisciRicercaPuntualePerNumeroEdIntorno( ParamCercaFoto param ) {

			if( param.numeroConIntorno <= 0 )
				return -1;

			var v = param.numeriFotogrammi.Split( '-' );
			int dalNum = Convert.ToInt32( v[0] );
			int alNum = Convert.ToInt32( v[1] );


			
			// ---

			List<object> sqlParam = new List<object>();
			sqlParam.Add( param.numeroConIntorno );
			sqlParam.Add( param.giornataIniz ); // la fine è sempre uguale all'inizio. ne basta uno solo di parametro.

			string sql = "Select count(*) from fotografie f where f.numero = {0} and giornata = {1}";

			var query = objectContext.Database.SqlQuery<int>( sql, sqlParam.ToArray() );

			var quanti = (long) query.Single();

			// Se non esiste la foto che sto cercando, allora esco subito, cosi non perdo tempo nella ricerca dicotomica a vuoto
			if( quanti == 0 )
				return -1;


			// Ora so che la foto c'è.
			// Ora devo scoprire a quale pagina si trova la foto interessata. Probabilmente è nel mezzo
			// parto dal mezzo con una ricerca dicotomica

			sql = "Select count(*) from fotografie f where f.numero between {0} and {1} and giornata = {2}";

			sqlParam = new List<object>();
			sqlParam.Add( dalNum.ToString() );
			sqlParam.Add( alNum.ToString() );
			sqlParam.Add( param.giornataIniz );

			query = objectContext.Database.SqlQuery<int>( sql, sqlParam.ToArray() );

			int totale = (int)query.Single();

			ushort totPagine = (ushort) ( (totale / param.paginazione.take) );
			if( (totale % param.paginazione.take) != 0 )
				++totPagine;

			int numeroDaric = param.numeroConIntorno;
			var pag = ricercaPaginaDicotomicaIntorno( 1, totPagine, sqlParam, param );

			return pag;
		}

		private ushort ricercaPaginaDicotomicaIntorno( ushort limiteInf, ushort limiteSup, List<object> sqlParam, ParamCercaFoto param ) {

			StringBuilder sql = new StringBuilder();

			ushort middlePage = (ushort) ((limiteInf + limiteSup) / 2);

			if( middlePage < limiteInf || limiteSup < 0 )
				throw new InvalidOperationException( "impossibile" );

			sql.Append( "Select min(q.numero) minimo, max(q.numero) massimo" );
			sql.Append( SEPAR );
			sql.Append( "from (" );
			sql.Append( SEPAR );
			sql.Append( "\tselect f.numero" );
			sql.Append( SEPAR );
			sql.Append( "\tfrom fotografie f" );
			sql.Append( SEPAR );
			sql.Append( "\twhere 1=1" );
			sql.Append( SEPAR );
			if( sqlParam.Count > 0 )
				sql.Append( "\t\tand f.numero between {0} and {1}" );
			sql.Append( SEPAR );
			sql.Append( "\t\tand f.giornata = {2}" );
			sql.Append( SEPAR );

			string orderBy = creaQuerySQLOrderBy( param );
			sql.Append( orderBy );

			sql.Append( "\tLIMIT " );
			int skip = param.paginazione.take * (middlePage - 1);
			sql.Append( Convert.ToString( skip ) );
			sql.Append( " , " );
			sql.Append( param.paginazione.take );
			sql.Append( SEPAR );
			sql.Append( ") q" );

			var query = objectContext.Database.SqlQuery<AppoNumNum>( sql.ToString(), sqlParam.ToArray() );
			AppoNumNum appo = query.ToList()[0];

			//  Vediamo se nella pagina attuale ho trovato il numero di foto che sto cercando
			if( appo.minimo <= param.numeroConIntorno && appo.massimo >= param.numeroConIntorno )
				// Trovato
				return middlePage;
			else {
				
				if( appo.massimo < param.numeroConIntorno ) {
					// salgo
					limiteInf = (ushort) (middlePage + 1);
				} else {
					// Scendo
					limiteSup = (ushort) (middlePage - 1);
				}

				return ricercaPaginaDicotomicaIntorno( limiteInf, limiteSup, sqlParam, param );
			}
		}

		public int ricercaPaginaDelFotogramma( int numFotogrammaDaric, int paginaMin, int paginaMax, ParamCercaFoto param ) {

			RicercatorePaginaDicotomicoPosiz ricer = new RicercatorePaginaDicotomicoPosiz( param );

			ricer.paginaMin = paginaMin;
			ricer.paginaMax = paginaMax;

			return ricer.cercaPagina( numFotogrammaDaric );
		}

		public ICollection<Carrello> cerca(ParamCercaCarrello param)
		{

			_giornale.Debug( "Parametri di ricerca:\n" + param );

			IQueryable<Carrello> query = creaQueryEntita( param );

			// Devo usare prima tutto se no dopo non me lo ricollega più!!! Perchè ho chiuso la connessione?!?!?!?!?!!?
			foreach (Carrello c in query.ToList())
			{
				// Per caricare una collezione sarebbe più figo cosi:
				// UnitOfWorkScope.currentDbContext.Entry( c ).Collection( k => k.righeCarrello ).Load();


				System.Diagnostics.Trace.WriteLine("\n\n*** Carrello = " + c.id + " " + c.giornata);

				foreach (RigaCarrello r in c.righeCarrello)
				{
					System.Diagnostics.Trace.WriteLine("\n\t" + r.GetType().Name + " " + r.id + " " + r.descrizione);

					System.Diagnostics.Trace.WriteLine( "\t\tFotografo     = " + r.fotografo );
					System.Diagnostics.Trace.WriteLine( "\t\tFotografia    = " + r.fotografia );
					if( r.fotografia != null )
						System.Diagnostics.Trace.WriteLine( "\t\tDataOra = " + r.fotografia.dataOraAcquisizione );

					if (r.discriminator == RigaCarrello.TIPORIGA_STAMPA )
					{
						System.Diagnostics.Trace.WriteLine("\t\tFormato Carta = " + r.formatoCarta);
					}
					if( r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA )
					{
					}
				}
			}

			// Eventuale paginazione dei risultati
			if (param.paginazione != null)
			{
				query = query.Skip(param.paginazione.skip).Take(param.paginazione.take);
			}

#if DEBUG
			// Eventuale debug della query
			if (_giornale.IsDebugEnabled)
			{
				_giornale.Debug(query.ToString());
			}
#endif
			_giornale.Debug( "Eseguita query ricerca carrelli" );
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

			var qq = this.objectContext.Fotografie.Include( "fotografo" );			

			if( !param.evitareJoinEvento )
				qq = qq.Include( "evento" );

			var qq2 = qq;

			IQueryable<Fotografia> query = qq2.AsQueryable();
			
			// ----- Filtro eventi
			// Siccome non esiste la WhereIn, me la sono creata io. 
			// Seguire questa discussione : http://social.msdn.microsoft.com/Forums/en-US/adodotnetentityframework/thread/095745fe-dcf0-4142-b684-b7e4a1ab59f0
			if( param.eventi != null ) {
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				IEnumerable<Guid> listaIds = from le in param.eventi select le.id;
				query = query.Where( ff => listaIds.Contains( ff.evento.id ) );
			}


			// ----- Filtro scarico card
			if( param.scarichiCard != null ) {
				IEnumerable<DateTime> listaDate = from le in param.scarichiCard select le.tempo;
				query = query.Where( ff => listaDate.Contains( ff.dataOraAcquisizione ) );
			}

			// ----- Filtro fotografo
			if( param.fotografi != null ) {
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				var listaIds = from le in param.fotografi
							   select le.id;
				query = query.Where( ff => listaIds.Contains( ff.fotografo.id ) );
			}

			if( param.idsFotografie != null ) {
				query = query.Where( ff => param.idsFotografie.Contains( ff.id ) );
			}

			if (param.numeriFotogrammi != null)
			{
				int[] range = FotoRangeUtil.rangeToString(param.numeriFotogrammi);

				// Testo se ho un range o una lista
				if (param.numeriFotogrammi.Contains('-'))
				{
					int estInf = range[0];
					int estSup = range[1]; 

					// ----- RANGE di fotogramma
					query = query.Where(ff => ff.numero >= estInf);
					if (range[1]>0)
					{
						query = query.Where(ff => ff.numero <= estSup);
					}
				}
				else
				{
					// ----- numeri di fotogramma
					query = query.Where(ff => range.Contains(ff.numero));
				}
			}

			// ----- fasi del giorno (la Enum non prevede il Contains. Devo trasformarla in una array di interi
			if( param.fasiDelGiorno != null && param.fasiDelGiorno.Count > 0 ) {
				IEnumerable<short> fasiInt = from p in param.fasiDelGiorno
											 select Convert.ToInt16( p );
				query = query.Where( ff => fasiInt.Contains( (short)ff.faseDelGiorno ) );
			}

			// ----- Didascalia (le didascalie le memorizziamo solo in maiuscolo)
			if( !String.IsNullOrWhiteSpace( param.didascalia ) && param.didascalia != "%" ) {


				if( param.didascalia.Contains( "%" ) ) {

					string dida;

                    if( param.didascalia.StartsWith( "%" ) && param.didascalia.EndsWith( "%" ) ) {
						// Cerco nel mezzo
						dida = param.didascalia.Substring( 1, param.didascalia.Length - 2 );
                        query = query.Where( ff => ff.didascalia.Contains( dida ) );
					} else {

						if( param.didascalia.StartsWith( "%" ) ) {
							dida = param.didascalia.Substring( 1 );
							query = query.Where( ff => ff.didascalia.EndsWith( dida ) );
						} else if( param.didascalia.EndsWith( "%" ) ) {
							dida = param.didascalia.Substring( 0, param.didascalia.Length - 1 );
							query = query.Where( ff => ff.didascalia.StartsWith( dida ) );
						}
					}
				} else {
					query = query.Where( ff => ff.didascalia.Equals( param.didascalia ) );
				}
			}

			// ----- Giornata Inizio
			if( param.giornataIniz != null )
				query = query.Where( ff => ff.giornata >= param.giornataIniz );

			// ----- Giornata Fine
			if( param.giornataFine != null )
				query = query.Where( ff => ff.giornata <= param.giornataFine );

			// ----- Ordinamento
			if( param.ordinamento != null ) {
				if( param.ordinamento == Ordinamento.Asc )
					query = query.OrderBy( ff => ff.dataOraAcquisizione ).ThenBy( ff => ff.numero );
				if( param.ordinamento == Ordinamento.Desc )
					query = query.OrderByDescending( ff => ff.dataOraAcquisizione ).ThenByDescending( ff => ff.numero );
			}

			return query;
		}

		private IQueryable<Carrello> creaQueryEntita(ParamCercaCarrello param)
		{
			IQueryable<Carrello> query = from ff in this.objectContext.Carrelli.Include("righeCarrello")
										 orderby ff.tempo descending
										 select ff;


			// ----- ID : ricerca puntuale per ID
			if( param.carrelloId != Guid.Empty )
				query = query.Where( cc => cc.id == param.carrelloId );

			//Filtro solo i carrelli che non sono stati venduti
			if (param.isVenduto != null)
			{
				if(param.isVenduto==false){
					query = query.Where(ff => ff.venduto != true);
				}
			}

			// Filtro solo i carrelli visibili per il self service
			if( param.soloSelfService != null ) {
				query = query.Where( cc => cc.visibileSelfService == param.soloSelfService );
			}

			// ----- Filtro fotografo
			if (param.fotografi != null)
			{
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				var listaIds = from le in param.fotografi
							   select le.id;

				query = query.Where(ff => listaIds.Equals(ff.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA ).First<RigaCarrello>().fotografo));
			}

			// ----- fasi del giorno (la Enum non prevede il Contains. Devo trasformarla in una array di interi
			if (param.fasiDelGiorno != null && param.fasiDelGiorno.Count > 0)
			{
				IEnumerable<short> fasiInt = from p in param.fasiDelGiorno
											 select Convert.ToInt16(p);

				query = query.Where(ff => fasiInt.Equals(ff.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA ).First<RigaCarrello>().fotografia.faseDelGiorno));
			}

			// ----- Intestazione 
			if (!String.IsNullOrWhiteSpace(param.intestazione))
				query = query.Where(ff => ff.intestazione.ToLower().Contains(param.intestazione.ToLower()));

			// ----- Giornata Inizio
			if (param.giornataIniz != null)
				query = query.Where(ff => ff.giornata >= param.giornataIniz);

			// ----- Giornata Fine
			if (param.giornataFine != null)
				query = query.Where(ff => ff.giornata <= param.giornataFine);



			return query;
		}

		public int conta( ParamCercaFoto param ) {

			// Clono i parametri per evitare alcuni rallentamenti che nella count non hanno senso
			ParamCercaFoto param2 = param.deepCopy<ParamCercaFoto>();
			param2.ordinamento = null;

			List<object> sqlParam = null;
			String sql = creaQuerySQL( param2, true, ref sqlParam );

			var query = objectContext.Database.SqlQuery<int>( sql, sqlParam.ToArray() );

			_giornale.Debug( "Conta1: " + DateTime.Now.ToString( "mm:ss:fff" ) );

			var quanti = query.Single();

			_giornale.Debug( "Conta2: " + DateTime.Now.ToString( "mm:ss:fff" ) );

			return quanti;
		}



	
	}



}
