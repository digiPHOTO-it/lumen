using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Util;
using Digiphoto.Lumen.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

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

#if DEBUG
			// Eventuale debug della query
			if( _giornale.IsDebugEnabled ) {
				_giornale.Debug( query.ToString() );
			}
#endif

			_giornale.Debug( "Eseguita query di ricerca foto." );
			return query.ToList();
		}

		public ICollection<Carrello> cerca(ParamCercaCarrello param)
		{

			_giornale.Debug( "Parametri di ricerca:\n" + param );

			IQueryable<Carrello> query = creaQueryEntita( param );

			// Devo usare prima tutto se no dopo non me lo ricollega più!!! Perchè ho chiuso la connessione?!?!?!?!?!!?
			foreach (Carrello c in query.ToList())
			{
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

			if( param.eventi == null )
				param2.evitareJoinEvento = true;

			IQueryable<Fotografia> query = creaQueryEntita( param2 );
			
			int quanti = query.Count();

			return quanti;
		}
	}
}
