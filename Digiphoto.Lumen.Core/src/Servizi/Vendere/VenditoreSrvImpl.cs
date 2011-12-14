using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects.DataClasses;
using Digiphoto.Lumen.Database;
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

		IMasterizzaSrv masterizzaSrv = null;

		/**
		 * Null non serve.
		 * false = da fare ma non ancora fatta
		 * true = da fare e già finita.
		 */
		private bool? faseMasterizzazioneCompletata;
		private bool? faseStampaCompletata;


#endregion

		private Object thisLock = new Object();

		public VenditoreSrvImpl() : base() {

			gestoreCarrello = new GestoreCarrello();
			modoVendita = Digiphoto.Lumen.Config.Configurazione.modoVendita;
			contaMessaggiInCoda = 0;
		}

		
		public void aggiungiMasterrizzate( IList<Fotografia> fotografie ) {
			throw new NotImplementedException();
		}

		/** 
		 * Per ogni foto indicata, creo una nuova riga di carrello
		 */
		public void aggiungiStampe( IList<Fotografia> fotografie, Stampare.ParamStampaFoto param ) {
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
			// eventualeMasterizzazione();

		}

		private void eventualeStampa() {

			faseStampaCompletata = null;

			// Se non ho righe nel carrello da stampare, allora esco.
			if( carrello == null || carrello.righeCarrello.Count == 0 )
				return;

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			int conta = 0;
			foreach( RigaCarrello riga in carrello.righeCarrello ) {
				
				if( riga is RiCaFotoStampata ) {
					
					RiCaFotoStampata riCaFotoStampata = (RiCaFotoStampata)riga;
					
					++conta;
					if( faseStampaCompletata == null )
						faseStampaCompletata = false;

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
			param.autoZoomToFit = true;
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
		private void eventualeMasterizzazione( Guid idCarrello ) {

			throw new NotImplementedException();

			faseMasterizzazioneCompletata = null;

			if( masterizzaSrv == null )
				return;

			if( masterizzaSrv.Count <= 0 )
				return;

			// Ok ho delle foto da masterizzare
			faseMasterizzazioneCompletata = false;

// TODO da fare!	
			masterizzaSrv.confermaVendita( 12m );

			// .masterizza( mioMamsterizzazioneEventHandler );
		}







		public void mioMamsterizzazioneEventHandler( MasterizzaMsg msg ) {

			faseMasterizzazioneCompletata = true;

			// Se l'operazione è andata bene, sono già a posto. Ero stato ottimista prima.
			if( msg.esito == Esito.Ok )
				return;

			// TODO gestire l'esito (e soprattutto la guid del carrello che mi ha generato

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

				} finally {
					--contaMessaggiInCoda;
				}
			}
		}

		private void gestioneEsitoMasterizzazione( MasterizzaMsg masterizzaMsg ) {
			throw new NotImplementedException();
		}

		/**
		 * E' stata completata una stampa. Devo gestirne l'esito
		 */
		private void gestioneEsitoStampa( LavoroDiStampa lavoroDiStampa ) {

			if( lavoroDiStampa.fotografia != null ) {
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
		}
	}
}