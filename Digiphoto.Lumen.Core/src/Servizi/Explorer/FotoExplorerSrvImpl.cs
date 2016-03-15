using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.ComponentModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Eventi;
using System.Threading;
using Digiphoto.Lumen.Util;
using log4net;
using Digiphoto.Lumen.Core.Database;
using System.Transactions;
using Digiphoto.Lumen.Database;
using System.Data.Entity.Core.Objects;
using System.Data;
using MemBus;

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

		/** Eseguo il caricamento delle foto richieste */
		public void cercaFoto( ParamCercaFoto param ) {

			// Per prima cosa azzero la gallery corrente e rilascio la memoria eventualmente utilizzata dalle foto
			if( fotografie != null ) {
				foreach( Fotografia f in fotografie )
					AiutanteFoto.disposeImmagini( f, IdrataTarget.Tutte );

				FormuleMagiche.rilasciaMemoria();
			}
			fotografie = null;

			IRicercatoreSrv ricercaSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			fotografie = ricercaSrv.cerca( param );


			if( param.idratareImmagini ) {
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
				foreach( Fotografia foto in this.fotografie ) {
					AiutanteFoto.disposeImmagini( foto );
				}
			} finally {
			}

			base.Dispose( disposing );
		}


		public bool  modificaMetadatiFotografie( IEnumerable<Fotografia> fotografie, MetadatiFoto metadati ) {
			bool esito = false;
			using (TransactionScope transaction = new TransactionScope()) {
				try
				{
					// riattacco l'entità che non si sa mai
					if (metadati.evento != null)
					{
						//UnitOfWorkScope.CurrentObjectContext.Eventi.Attach(metadati.evento);
						Evento e = metadati.evento;
						OrmUtil.forseAttacca<Evento>( ref e );
					}

					foreach (Fotografia fotografia in fotografie)
					{
                        String didascaliaOld = fotografia.didascalia != null ? fotografia.didascalia : "empty";
                        String faseDelGiornoOld =  fotografia.faseDelGiornoString != null ? fotografia.faseDelGiornoString : "empty";
                        String eventoOld = fotografia.evento != null ? fotografia.evento.ToString() : "empty";

                        String didascaliaNew = metadati.isDidascaliaEnabled ? metadati.didascaliaString : didascaliaOld;
                        String faseDelGiornoNew = metadati.isFaseDelGiornoEnabled ? metadati.faseDelGiornoString : faseDelGiornoOld;
                        String eventoNew = metadati.isEventoEnabled ? metadati.eventoString : eventoOld;

                        StringBuilder msg = new StringBuilder();
						msg.AppendFormat("Modificati metadati: {0} da: didascalia:{1} giornata:{2} evento:{3} in didascalia:{4} giornata:{5} evento:{6}",
							fotografia.numero + " " + fotografia.nomeFile,
                            didascaliaOld,
                            faseDelGiornoOld,
                            eventoOld,
                            didascaliaNew,
                            faseDelGiornoNew,
                            eventoNew
                            );
						modificaMetadatiFotografie(fotografia, metadati);

						_giornale.Info(msg);
					}

					UnitOfWorkScope.currentDbContext.SaveChanges();
					_giornale.Debug("Modifica metadati salvataggio eseguito. Ora committo la transazione");

					transaction.Complete();
					_giornale.Info("Commit metadati andato a buon fine");

					esito = true;
				} catch( Exception eee ) {
					esito = false;
					_giornale.Error("Impossibile salvare il carrello", eee);
				}
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
			OrmUtil.forseAttacca<Fotografia>( ref f );

			//Consento la modifica anche di valori nulli
			//if( !String.IsNullOrWhiteSpace( metadati.didascalia ) )
			if (metadati.isDidascaliaEnabled)
            {
                if (metadati.didascalia != null)
                    foto.didascalia = metadati.didascalia.Trim();
                else
                    foto.didascalia = null;
            }
				
			else {
				if( forzaNullo )
					foto.didascalia = null;
			}

			if(metadati.isFaseDelGiornoEnabled)
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

			if( metadati.isEventoEnabled)
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
	}
}
