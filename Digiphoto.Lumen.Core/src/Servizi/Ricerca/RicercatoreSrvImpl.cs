using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Core;
using log4net;
using Digiphoto.Lumen.Database;

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
				_giornale.Debug( query.ToString() );
			}

			return query.ToList();
		}

		public ICollection<Carrello> cerca(ParamCercaCarrello param)
		{
			//IQueryable<Carrello> query = from ff in this.objectContext.Carrelli.Include("righeCarrello")
			//							   select ff;


			_giornale.Debug("Parametri di ricerca:\n" + param);

			IQueryable<Carrello> query = creaQueryEntita(param);

			// Devo usare prima tutto se no dopo non me lo ricollega più!!! Perchè ho chiuso la connessione?!?!?!?!?!!?
			foreach (Carrello c in query.ToList())
			{
				System.Diagnostics.Trace.WriteLine("\n\n*** Carrello = " + c.id + " " + c.giornata);

				foreach (RigaCarrello r in c.righeCarrello)
				{
					System.Diagnostics.Trace.WriteLine("\n\t" + r.GetType().Name + " " + r.id + " " + r.descrizione);

					if (r is RiCaFotoStampata)
					{

						RiCaFotoStampata rfs = r as RiCaFotoStampata;
						System.Diagnostics.Trace.WriteLine("\t\tFotografo     = " + rfs.fotografo);
						System.Diagnostics.Trace.WriteLine("\t\tFormato Carta = " + rfs.formatoCarta);
						System.Diagnostics.Trace.WriteLine("\t\tFotografia    = " + rfs.fotografia);
						if (rfs.fotografia != null)
							System.Diagnostics.Trace.WriteLine("\t\tDataOra = " + rfs.fotografia.dataOraAcquisizione);
					}
					if (r is RiCaDiscoMasterizzato)
					{
						RiCaDiscoMasterizzato rdm = r as RiCaDiscoMasterizzato;
						System.Diagnostics.Trace.WriteLine("\t\tTot. foto masterizzate = " + rdm.totFotoMasterizzate);
					}
				}
			}

			// Eventuale paginazione dei risultati
			if (param.paginazione != null)
			{
				query = query.Skip(param.paginazione.skip).Take(param.paginazione.take);
			}

			// Eventuale debug della query
			if (_giornale.IsDebugEnabled)
			{
				_giornale.Debug(query.ToString());
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

			IQueryable<Fotografia> query = from ff in this.objectContext.Fotografie.Include( "fotografo" ).Include( "evento" )
										   orderby ff.dataOraAcquisizione, ff.numero
										   select ff;
			
			
			// ----- Filtro eventi
			// Siccome non esiste la WhereIn, me la sono creata io. 
			// Seguire questa discussione : http://social.msdn.microsoft.com/Forums/en-US/adodotnetentityframework/thread/095745fe-dcf0-4142-b684-b7e4a1ab59f0
			if( param.eventi != null ) {
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				IEnumerable<Guid> listaIds = from le in param.eventi select le.id;
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
			if( ! String.IsNullOrWhiteSpace( param.didascalia ) )
				query = query.Where( ff => ff.didascalia.Contains( param.didascalia ) );

			// ----- Giornata Inizio
			if( param.giornataIniz != null )
				query = query.Where( ff => ff.giornata >= param.giornataIniz );

			// ----- Giornata Fine
			if( param.giornataFine != null )
				query = query.Where( ff => ff.giornata <= param.giornataFine );

			return query;
		}

		private IQueryable<Carrello> creaQueryEntita(ParamCercaCarrello param)
		{
			IQueryable<Carrello> query = from ff in this.objectContext.Carrelli.Include("righeCarrello")
										   select ff;

			//Filtro solo i carrelli che non sono stati venduti
			if (param.isVenduto != null)
			{
				if(param.isVenduto==false){
					query = query.Where(ff => ff.venduto != true);
				}
			}

			// ----- Filtro fotografo
			if (param.fotografi != null)
			{
				// Siccome ancora linq non supporta confronto con entità, devo estrarre gli id
				var listaIds = from le in param.fotografi
							   select le.id;

				query = query.Where(ff => listaIds.Equals(ff.righeCarrello.OfType<RiCaFotoStampata>().First<RiCaFotoStampata>().fotografo));
			}

			// ----- fasi del giorno (la Enum non prevede il Contains. Devo trasformarla in una array di interi
			if (param.fasiDelGiorno != null && param.fasiDelGiorno.Count > 0)
			{
				IEnumerable<short> fasiInt = from p in param.fasiDelGiorno
											 select Convert.ToInt16(p);

				query = query.Where(ff => fasiInt.Equals(ff.righeCarrello.OfType<RiCaFotoStampata>().First<RiCaFotoStampata>().fotografia.faseDelGiorno));
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

	}
}
