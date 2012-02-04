using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects.DataClasses;
using Digiphoto.Lumen.Core.Database;
using log4net;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Eventi;
using System.Transactions;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Servizi.Masterizzare;

namespace Digiphoto.Lumen.Servizi.Vendere {

	public class VenditoreSrvImpl : ServizioImpl, IVenditoreSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( VenditoreSrvImpl ) );

#region Proprietà

		private GestoreCarrello gestoreCarrello {
			get;
			set;
		}

		public Carrello carrello {
			get {
				return gestoreCarrello.carrello;
			}
		}

		public ModoVendita modoVendita {
			get;
			set;
		}

		ISpoolStampeSrv spoolStampeSrv {
			get {
				return (ISpoolStampeSrv)LumenApplication.Instance.getServizioAvviato<ISpoolStampeSrv>();
			}
		}

		int contaMessaggiInCoda {
			get;
			set;
		}


#endregion

		private Object thisLock = new Object();

		public VenditoreSrvImpl() : base() {

			// istanzio il gestore del carrello e creo subito un carrello nuovo per iniziare a lavorare subito.
			gestoreCarrello = new GestoreCarrello();
			gestoreCarrello.creaNuovo();

			modoVendita = Digiphoto.Lumen.Config.Configurazione.modoVendita;
			
			contaMessaggiInCoda = 0;

		}

		


		/** 
		 * Per ogni foto indicata, creo una nuova riga di carrello
		 */
		public void aggiungiStampe( IEnumerable<Fotografia> fotografie, Stampare.ParamStampaFoto param ) {
			foreach( Fotografia foto in fotografie ) {
				AiutanteFoto.idrataImmaginiFoto( foto );
				carrello.righeCarrello.Add( creaRiCaFotoStampata( foto, param ) );
			}
		}

		
		public void creaNuovoCarrello() {

			if( modoVendita != ModoVendita.Carrello )
				throw new InvalidOperationException( "La modalità di vendita non è impostata su 'Carrello'" );

			abbandonaCarrello();   // se ce n'era uno già apero, lo rimuovo

			gestoreCarrello.creaNuovo();
		}


		public void confermaCarrello() {

			_giornale.Debug( "carrello valido. Inizio operazioni di produzione" );

			//
			// Siccome l'esito della stampa e della masterizzazione lo riceverò più tardi 
			// ed in modo asincrono, in questo momento non posso fare altro che dare per scontato
			// che andrà tutto bene.
			// Quindi memorizzo il carrello intero. Poi gestirò i problemi (sperando che non ce ne siano).
			//

			using( TransactionScope transaction = new TransactionScope() ) {

				try {

					aggiornaTotFotoMasterizzate();

					// Poi salvo il carrello
					gestoreCarrello.salva();

				} catch( Exception eee ) {
					_giornale.Error( "Impossibile salvare il carrello", eee );
					// Purtoppo devo andare avanti lo stesso, perché non posso permettermi 
					// di non fare uscire le foto. Altrimenti i clienti in fila si arrabbiano
					// ed il commesso non può incassare.
					// In ogni caso è un errore grave.  >>> THE SHOW MUST GO ON !  <<<
				}

				transaction.Complete();
			}

			// Prima lancio le stampe
			eventualeStampa();

			// Poi lancio la masterizzazione
			eventualeMasterizzazione();

		}

		//
		// <summary>
		// conto quante foto sono pronte per essere masterizzate e le scrivo nella riga 
		// del carrello.
		// Riporto anche il prezzo del cd sulla riga.
		// </summary>
		//
		private void aggiornaTotFotoMasterizzate() {
			// Sistemo il numero eventuale di foto masterizzate
			if( _masterizzaSrvImpl != null ) {
				foreach( RigaCarrello r in gestoreCarrello.carrello.righeCarrello ) {
					if( r is RiCaDiscoMasterizzato ) {
						RiCaDiscoMasterizzato rdm = (RiCaDiscoMasterizzato)r;
						rdm.totFotoMasterizzate = (short)_masterizzaSrvImpl.fotografie.Count;
						
						// Sto attento a non sovrascrivere con una informazione vuota.
						if( _masterizzaSrvImpl.prezzoForfaittario != null )
							rdm.prezzoLordoUnitario = _masterizzaSrvImpl.prezzoForfaittario;
						break;
					}
				}
			}
		}

		private void eventualeStampa() {

			// Se non ho righe nel carrello da stampare, allora esco.
			if( carrello == null || carrello.righeCarrello.Count == 0 )
				return;

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			int conta = 0;
			foreach( RigaCarrello riga in carrello.righeCarrello ) {
				
				if( riga is RiCaFotoStampata ) {
					
					RiCaFotoStampata riCaFotoStampata = (RiCaFotoStampata)riga;
					
					++conta;

					// Creo nuovamente i parametri di stampa perché potrebbero essere cambiati nell GUI
					ParamStampaFoto paramStampaFoto = creaParamStampaFoto( riCaFotoStampata );
					spoolStampeSrv.accodaStampa( riCaFotoStampata.fotografia, paramStampaFoto );					
				}
			}
		}

		/**
		 * Crea una nuova riga carrello da stampare in base ai parametri di stampa richiesti
		 */
		private RiCaFotoStampata creaRiCaFotoStampata( Fotografia fotografia, ParamStampaFoto param ) {

			RiCaFotoStampata r = new RiCaFotoStampata();
			r.id = Guid.NewGuid();
			r.fotografia = fotografia;
			r.formatoCarta = param.formatoCarta;
			r.descrizione = "Stampe formato " + r.formatoCarta.descrizione;
			r.fotografo = fotografia.fotografo;
			r.prezzoLordoUnitario = param.formatoCarta.prezzo;
			r.quantita = param.numCopie;
			r.prezzoNettoTotale = r.prezzoLordoUnitario * r.quantita;
			return r;
		}

		private ParamStampaFoto creaParamStampaFoto( RiCaFotoStampata riCaFotoStampata ) {
			ParamStampaFoto param = new ParamStampaFoto();
			param.autoRuota = true;
			param.autoZoomNoBordiBianchi = true;
			param.formatoCarta = riCaFotoStampata.formatoCarta;
			param.numCopie = riCaFotoStampata.quantita;
			param.nomeStampante = "doPDF v7";    // TODO definire la stampa

			// Questa informazione mi serve nella callback
			System.Diagnostics.Debug.Assert( ! riCaFotoStampata.id.Equals( Guid.Empty ) );
			param.idRigaCarrello = riCaFotoStampata.id;
			return param;
		}



		/**
		 * Siccome il carrello in questione viene chiuso prima che termini la masterizzazione,
		 * tramite il suo id, è possibile andare a sistemare eventuali problemi.
		 */
		private void eventualeMasterizzazione() {

			if( masterizzaSrv == null )
				return;

			if( masterizzaSrv.fotografie.Count <= 0 )
				return;

			_masterizzaSrvImpl.start();

			_masterizzaSrvImpl.masterizza( carrello.id );
		}



		public override void OnNext( Messaggio messaggio ) {
			
			base.OnNext( messaggio );

			lock( thisLock ) {

				++contaMessaggiInCoda;

				try {

					if( messaggio is StampatoMsg )
						gestioneEsitoStampa( ((StampatoMsg)messaggio).lavoroDiStampa );
					else if( messaggio is MasterizzaMsg )
						gestioneEsitoMasterizzazione( (MasterizzaMsg)messaggio );

				} catch( Exception ee ) {
					_giornale.Error( "gestione messaggio " + messaggio, ee );
				} finally {
					--contaMessaggiInCoda;
				}
			}
		}

		private void gestioneEsitoMasterizzazione( MasterizzaMsg masterizzaMsg ) {

			if( masterizzaMsg.fase != Fase.CopiaCompletata )
				return;

			if( masterizzaMsg.esito == Esito.Ok )
				return;

			// TODO gestire lo storno
			// Vado a correggere questa riga
			using( GestoreCarrello altroGestoreCarrello = new GestoreCarrello() ) {
				altroGestoreCarrello.stornoMasterizzate( (Guid)masterizzaMsg.senderTag, (short)masterizzaMsg.totFotoAggiunte, (short)masterizzaMsg.totFotoNonAggiunte );
			}

		}

		/**
		 * E' stata completata una stampa. Devo gestirne l'esito
		 */
		private void gestioneEsitoStampa( LavoroDiStampa lavoroDiStampa ) {

			if( false && lavoroDiStampa.fotografia != null ) {
				try {
					// Prima che sia troppo tardi devo rilasciare le immagini (altrimenti rimangono loccate)
					if( lavoroDiStampa.fotografia.imgOrig != null )
						lavoroDiStampa.fotografia.imgOrig.Dispose();

					if( lavoroDiStampa.fotografia.imgProvino != null )
						lavoroDiStampa.fotografia.imgProvino.Dispose();

					if( lavoroDiStampa.fotografia.imgRisultante != null )
						lavoroDiStampa.fotografia.imgRisultante.Dispose();

				} catch( Exception ee ) {
					_giornale.Error( "Impossibile rilasciare immagini dopo stampa", ee );
			
					// Devo andare avanti lo stesso perché devo notificare tutti
				}
			}

			// Se la stampa è stata completata correttamente, non faccio niente. Sono già a posto.
			// TODO rimettere a posto con ==
			if( lavoroDiStampa.esitostampa == EsitoStampa.Ok )
				return;

			_giornale.Error( "il lavoro di stampa non è andato a buon fine: " + lavoroDiStampa.ToString() );

			// Vado a correggere questa riga
			using( GestoreCarrello altroGestoreCarrello = new GestoreCarrello() ) {
				altroGestoreCarrello.stornoRiga( lavoroDiStampa.param.idRigaCarrello );
			}

		}

		public override void stop() {

			// Rimango in attesa fino a che ho dei messaggi da elaborare
			while( contaMessaggiInCoda > 0 )
				System.Threading.Thread.Sleep( 2000 );

			base.stop();
		}

		public void abbandonaCarrello() {

			if( gestoreCarrello != null )
				gestoreCarrello.Dispose();
			
			gestoreCarrello = new GestoreCarrello();

			if( _masterizzaSrvImpl != null ) {
				_masterizzaSrvImpl.Dispose();
				_masterizzaSrvImpl = null;
			}
		}

		/**
		 * Questo servizio mi tengo io la implementazione perché voglio chiamare io il metodo masterizza.
		 * E non voglio che venga chiamato da fuori.
		 */
		private MasterizzaSrvImpl _masterizzaSrvImpl;
		public IMasterizzaSrv masterizzaSrv {
			get {
				return _masterizzaSrvImpl;
			}
		}


		public void aggiungiMasterizzate( IEnumerable<Fotografia> fotografie ) {

			// Istanzio il servizio di masterizzazione, ma io uso la Impl.
			if( _masterizzaSrvImpl == null ) {
				// Siccome non voglio che si chiami il metodo masterizza da fuori, faccio una forzatura.
				// Uso io direttamente la impl internamente.
				_masterizzaSrvImpl = (MasterizzaSrvImpl)LumenApplication.Instance.creaServizio<IMasterizzaSrv>();

				carrello.righeCarrello.Add( creaRiCaDiscoMasterizzato() );
			}

			// Aggiungo le foto alla lista
			_masterizzaSrvImpl.addFotografie( fotografie );
		}


		// creo anche una riga nel carrello (UNA SOLA)
		private RiCaDiscoMasterizzato creaRiCaDiscoMasterizzato() {

			RiCaDiscoMasterizzato r = new RiCaDiscoMasterizzato();
			r.id = Guid.NewGuid();
			r.quantita = 1;
			r.descrizione = "Masterizzato Dischetto";
			return r;
		}


		/// <summary>
		/// In base alla configurazione, ed altre variabili di stato/lavoro,
		/// creo i parametri di stampa di default
		/// </summary>
		/// <returns></returns>
		public ParamStampaFoto creaParamStampaFoto() {

			ParamStampaFoto p = new ParamStampaFoto();
			p.autoRuota = true;    // non ha senso stampare una foto orizzontale nella carta verticale
			p.numCopie = 1;
			p.autoZoomNoBordiBianchi = configurazione.autoZoomNoBordiBianchi;

			// TODO la stampante dovrei prendere quella di default di windows.
			return p;
		}
	}
}