using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using System.Threading;
using Digiphoto.Lumen.Util;
using log4net;
using Digiphoto.Lumen.Core.Database;
using System.Transactions;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Core.Eventi;

namespace Digiphoto.Lumen.Servizi.Explorer {

	public class FotoExplorerSrvImpl : ServizioImpl, IFotoExplorerSrv {

		#region Proprietà

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( FotoExplorerSrvImpl ) );


		public List<Fotografia> fotografie {
			get;
			private set;
		}

		public Fotografia fotoCorrente { get; set; }

		/** Alcuni attributi della foto, sono transienti e devo gestirli io a mano */
		private Thread _threadIdrata;

		#endregion

		public FotoExplorerSrvImpl() : base() {
			fotografie = new List<Fotografia>();
		}

		~FotoExplorerSrvImpl() {

			// avviso se il thread di copia è ancora attivo
			if( _threadIdrata != null && _threadIdrata.IsAlive ) {
				_giornale.Warn( "Il thread di caricamento foto è ancora attivo. Non è stata fatta la Dispose del servizio.\nProababilmente il programma si inchioderà" );
			}
		}

		/// <summary>
		//  azzero la gallery corrente e rilascio la memoria eventualmente utilizzata dalle foto
		/// </summary>
		private void svuotaGalleryCorrente() {
			
			if( fotografie != null ) {
				foreach( Fotografia f in fotografie )
					AiutanteFoto.disposeImmagini( f, IdrataTarget.Tutte );

				FormuleMagiche.rilasciaMemoria();
			}
			fotografie = null;
		}

		private void idratareImmaginiGallery() {

			if( fotografie != null ) {
				bool bloccoTuttoQuindiPeggioro = false;
				if( bloccoTuttoQuindiPeggioro ) {
					// Idrato le foto nello stesso thread della UI
					idrataImmaginiFoto();
				} else {
					// idrato le immagini in un thread separato
					_threadIdrata = new Thread( idrataImmaginiFoto );
					_threadIdrata.Start();
				}
			}

		}

		/** Eseguo il caricamento delle foto richieste */
		public void cercaFoto( ParamCercaFoto param ) {

			svuotaGalleryCorrente();

			IRicercatoreSrv ricercaSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			fotografie = ricercaSrv.cerca( param );

			if( param.idratareImmagini )
				idratareImmaginiGallery();
		}

		/// <summary>
		/// Occhio questo metodo non idrata le foto.
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public IList<Fotografia> cercaFotoTutte( ParamCercaFoto param ) {

			IRicercatoreSrv ricercaSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			return ricercaSrv.cerca( param );
		}


		/** Idrato in modo asincrono gli attributi delle immagini che ho caricato */
		private void idrataImmaginiFoto() {

			foreach( Fotografia fotografia in fotografie ) {

				// Per essere più veloce, idrato solo l'immagine del provino.
				AiutanteFoto.idrataImmaginiFoto( fotografia, IdrataTarget.Provino );
				// TODO forse occorre lanciare un evento di foto caricata ??? 
				//      essendo la collezione bindabile, forse non ce ne sarà bisogno..... 
				//      vedremo. Per ora risparmio fatica.
			}

			// Lancio un messaggio che dice che è stata portata a termine una nuova ricerca
			LumenApplication.Instance.bus.Publish( new RicercaModificataMessaggio( this ) );

			// Mando anche un messaggio di refresh per i provini che ho liberato
			LumenApplication.Instance.bus.Publish( new RefreshMsg( this ) );
		}

		protected override void Dispose( bool disposing ) {

			try {

				// Se il tread di copia è ancora vivo, lo uccido
				if( _threadIdrata != null ) {
					if( _threadIdrata.IsAlive )
						_threadIdrata.Abort();
					else
						_threadIdrata.Join();
				}
			} finally {
			}

			try {
				if( this.fotografie != null )
					foreach( Fotografia foto in this.fotografie ) {
						AiutanteFoto.disposeImmagini( foto );
				}
			} finally {
			}

			base.Dispose( disposing );
		}


		public bool  modificaMetadatiFotografie( IEnumerable<Fotografia> fotografie, MetadatiFoto metadati ) {
			bool esito = false;

			try
			{
				// riattacco l'entità che non si sa mai
				if (metadati.evento != null)
				{
					try {
						Evento e = metadati.evento;
						OrmUtil.forseAttacca<Evento>( ref e );
					} catch( Exception ee ) {
						_giornale.Debug( "Potenziale errore", ee );
					}
				}

				foreach (Fotografia f in fotografie)
				{

					Fotografia fotografia = f;
                    try {
						OrmUtil.forseAttacca( ref fotografia );
					} catch( Exception ee ) {
						_giornale.Debug( "Potenziale errore", ee );
						fotografia = UnitOfWorkScope.currentDbContext.Fotografie.Single( f2 => f2.id == f.id );
					}

#if DEBUG
					String didascaliaNew = null;
					if( metadati.usoDidascalia ) {
						if( ! String.IsNullOrWhiteSpace( metadati.didascalia ) )
							didascaliaNew = metadati.didascalia.TrimEnd().ToUpper();
                    }

					string strDidascaliaOld = fotografia.didascalia;
					string strFaseDelGiornoOld = fotografia.faseDelGiorno == null ? "empty" : FaseDelGiornoUtil.valoreToString( fotografia.faseDelGiorno );
					string strEventoOld = fotografia.evento == null ? "empty" : fotografia.evento.descrizione;

					string strFaseDelGiornoNew = metadati.faseDelGiorno == null ? "empty" : metadati.faseDelGiorno.ToString();
					string strEventoNew = metadati.evento == null ? "empty" : metadati.evento.descrizione;

					String msg = String.Format( "Modificati metadati: {0} da: dida:{1} faseGG:{2} evento:{3} in dida:{4} faseGG:{5} evento:{6}",
							fotografia.ToString(),
							strDidascaliaOld,
							strFaseDelGiornoOld,
							strEventoOld,
							didascaliaNew,
							strFaseDelGiornoNew,
							strEventoNew
					);
#endif

					modificaMetadatiFotografie(fotografia, metadati);

#if DEBUG
					_giornale.Debug( msg );
#endif

				}

				UnitOfWorkScope.currentDbContext.SaveChanges();
				_giornale.Debug("Modifica metadati salvataggio eseguito. Ora committo la transazione");

				_giornale.Info("Commit metadati andato a buon fine");

				esito = true;
			} catch( Exception eee ) {
				_giornale.Error( "Modifica metadati", eee );
				esito = false;
				_giornale.Error("Impossibile modificare metadati", eee);
			}

			return esito;
		}

		private void modificaMetadatiFotografie( Fotografia foto, MetadatiFoto metadati ) {

			// Se tutti i metadati sono nulli, allora forzo l'eliminazione degli stessi.
			// Se invece almeno uno è pieno, setto solo quello (cioè vado in aggiunta a quelli eventualmente
			// già esistenti sulla foto
			bool forzaNullo = metadati.isEmpty();

			modificaMetadatiFotografie( foto, metadati, forzaNullo );
		}

		private void modificaMetadatiFotografie( Fotografia foto, MetadatiFoto metadati, bool forzaNullo ) {

			// L'entità è sicuramente staccata
			//UnitOfWorkScope.CurrentObjectContext.Fotografie.Attach( foto );

			Fotografia f = foto;
			try {
				OrmUtil.forseAttacca<Fotografia>( ref f );
			} catch( Exception ) {
			}

			//Consento la modifica anche di valori nulli
			//if( !String.IsNullOrWhiteSpace( metadati.didascalia ) )
			if (metadati.usoDidascalia)
            {
                if( String.IsNullOrWhiteSpace( metadati.didascalia ) )
					foto.didascalia = null;
                else
					foto.didascalia = metadati.didascalia.Trim().ToUpper();  // pulisco spazi e converto in maiuscolo
			} else {
				if( forzaNullo )
					foto.didascalia = null;
			}

			if(metadati.usoFaseDelGiorno)
            {
                if (metadati.faseDelGiorno != null)
                    foto.faseDelGiorno = (short)metadati.faseDelGiorno;
                else
                    foto.faseDelGiorno = null;
            }
				
			else {
				if( forzaNullo )
					foto.faseDelGiorno = null;
			}

			if( metadati.usoEvento)
				foto.evento = metadati.evento;
			else {
				if( forzaNullo )
					foto.evento = null;
			}

			OrmUtil.cambiaStatoModificato( f );
		}

		public IEnumerable<ScaricoCard> loadUltimiScarichiCards() {

			DateTime giornoLim = DateTime.Today.AddDays( -6 );
			return UnitOfWorkScope.currentDbContext.ScarichiCards.Include( "fotografo" ).Where( sc => sc.giornata >= giornoLim ).OrderByDescending( sc => sc.tempo );
		}

		public int contaFoto( ParamCercaFoto paramCercaFoto ) {

			IRicercatoreSrv ricercaSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			return ricercaSrv.conta( paramCercaFoto );
		}

		public IEnumerable<Guid> caricaFotoDalCarrello() {

			IVenditoreSrv venditoreSrv = LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			return venditoreSrv.enumeraIdsFoto();
		}

		public Fotografia get( Guid id ) {

			Fotografia foto = null;	
			
			// Prima guardo se ce l'ho in pancia io
			foto = fotografie.FirstOrDefault( ff => ff.id == id );

			if( foto == null ) {
				foto = UnitOfWorkScope.currentDbContext.Fotografie.SingleOrDefault( f => f.id == id );

				// Stacco l'oggetto altrimenti sarebbe a carico del chiamante.
				if( foto != null )
					OrmUtil.forseStacca<Fotografia>( ref foto );
			}

			return foto;
		} 
	}
}
