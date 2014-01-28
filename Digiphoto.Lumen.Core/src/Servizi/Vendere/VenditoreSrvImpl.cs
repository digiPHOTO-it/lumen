using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using  System.Data.Entity.Core.Objects.DataClasses;
using Digiphoto.Lumen.Core.Database;
using log4net;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Eventi;
using System.Transactions;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Database;
using System.Windows.Forms;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi.Reports;
using System.Data.Entity;
using  System.Data.Entity.Core.Objects;
using System.Data.Common;

namespace Digiphoto.Lumen.Servizi.Vendere {

	public class VenditoreSrvImpl : ServizioImpl, IVenditoreSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( VenditoreSrvImpl ) );

		public static readonly String INTESTAZIONE_STAMPA_RAPIDA = "Stampa Diretta o Rapida";

		#region Proprietà

		private GestoreCarrello gestoreCarrello {
			get;
			set;
		}

		public Carrello carrello {
			get {
				return gestoreCarrello.carrello;
			}

			set {
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

		public short? scontoApplicato {
			get {
				return gestoreCarrello.scontoApplicato;
			}
		}


		public bool isPossibileSalvareCarrello {
			get {
				return gestoreCarrello.isPossibileSalvare;
			}
		}

		public bool isPossibileVendereCarrello {
			get {
				return gestoreCarrello.isPossibileSalvare;
			}
		}

		public int sommatoriaFotoDaMasterizzare {
			get {
				return gestoreCarrello.sommatoriaFotoDaMasterizzare;
			}
		}

		public int sommatoriaQtaFotoDaStampare {
			get {
				return gestoreCarrello.sommatoriaQtaFotoDaStampare;
			}
		}

		public decimal sommatoriaPrezziFotoDaStampare {
			get {
				return gestoreCarrello.sommatoraPrezziFotoDaStampare;
			}
		}

		public Decimal prezzoNettoTotale {
			get {
				return gestoreCarrello.prezzoNettoTotale;
			}
		}

		public string msgValidaCarrello {
			get {
				return gestoreCarrello.msgValidaCarrello();
			}
		}

		public bool possoAggiungereStampe {
			get {
				return (carrello != null && carrello.venduto == false);
			}
		}

		public bool possoAggiungereMasterizzate {
			get {
				return (carrello != null && carrello.venduto == false);
			}
		}

		#endregion

		#region Fields
		private Object thisLock = new Object();
		private StampantiAbbinateCollection _stampantiAbbinate;
		#endregion Fields


		public VenditoreSrvImpl() : base() {

			// istanzio il gestore del carrello e creo subito un carrello nuovo per iniziare a lavorare subito.
			gestoreCarrello = new GestoreCarrello();

			modoVendita = Configurazione.UserConfigLumen.modoVendita;

			contaMessaggiInCoda = 0;
		}

		public override void start() {

			base.start();

			creaNuovoCarrello();

			// TODO sostituire con la lista che è dentro il servizio spoolsrv
			_stampantiAbbinate = StampantiAbbinateUtil.deserializza( Configurazione.UserConfigLumen.stampantiAbbinate );

		}

		public void ricalcolaProvvigioni() {
			gestoreCarrello.ricalcolaDocumento( true );
		}

		public void ricalcolaTotaleCarrello() {

			// Sistemo i prezzi e i totali documento
			gestoreCarrello.ricalcolaDocumento( false );

			GestoreCarrelloMsg msg = new GestoreCarrelloMsg( this );
			msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.UpdateCarrello;
			LumenApplication.Instance.bus.Publish( msg );
		}

		public void caricaCarrello( Carrello c ) {
			
			gestoreCarrello.caricaCarrello( c.id );

			GestoreCarrelloMsg msg = new GestoreCarrelloMsg( this );
			msg.descrizione = "Caricato carrello dal database: " + c.intestazione;
			msg.showInStatusBar = true;
			msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.LoadCarrelloSalvato;
			LumenApplication.Instance.bus.Publish( msg );
		}

		/** 
		 * Per ogni foto indicata, creo una nuova riga di carrello
		 */
		public void aggiungereStampe( IEnumerable<Fotografia> fotografie, Stampare.ParamStampa param ) {

			if( ! possoAggiungereStampe )
				throw new InvalidOperationException( "Impossibile aggiungere stampe a questo carrello" );

			if (param is ParamStampaFoto)
			{
				foreach (Fotografia foto in fotografie)
				{
					// Non so perchè me devo fare la versione forzata perchè se no non mi idrata i provini del carrello. 
					// Nel caso in cui ricarico una foto che è già stata stampata precedentemente. 
					AiutanteFoto.idrataImmaginiFoto( foto , IdrataTarget.Provino, true);
					gestoreCarrello.aggiungiRiga(creaRiCaFotoStampata(foto, param as ParamStampaFoto));
				}
				// Notifico al carrello l'evento
				ricalcolaTotaleCarrello();
			}
			else if(param is ParamStampaProvini)
			{
				ParamStampaProvini paramStampaProvini = param as ParamStampaProvini;

				// Stampigli
				paramStampaProvini.stampigli = configurazione.stampigli;
				spoolStampeSrv.accodaStampaProvini(fotografie.ToList<Fotografia>(), paramStampaProvini);
			}
		}

		public void creaNuovoCarrello() {

			if( gestoreCarrello != null ) {
				gestoreCarrello.Dispose();
				gestoreCarrello = null;
			}

			gestoreCarrello = new GestoreCarrello();
			gestoreCarrello.creaNuovo();

			GestoreCarrelloMsg msg = new GestoreCarrelloMsg( this );
			msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.CreatoNuovoCarrello;
			LumenApplication.Instance.bus.Publish( msg );
			
			if (_masterizzaSrvImpl != null)
			{
				_masterizzaSrvImpl.Dispose();
				_masterizzaSrvImpl = null;
			}
		}

		public bool salvaCarrello() {
			bool esito = false;


			//
			// Siccome l'esito della stampa e della masterizzazione lo riceverò più tardi 
			// ed in modo asincrono, in questo momento non posso fare altro che dare per scontato
			// che andrà tutto bene.
			// Quindi memorizzo il carrello intero. Poi gestirò i problemi (sperando che non ce ne siano).
			//

			using( TransactionScope transaction = new TransactionScope() ) {

				try {
					// aggiornaTotFotoMasterizzate();

					// Poi salvo il carrello
					gestoreCarrello.salva();
					_giornale.Debug( "salvataggio eseguito. Ora committo la transazione" );

					transaction.Complete();
					_giornale.Info( "commit carrello a buon fine" );

					esito = true;

					pubblicaMessaggio( new Messaggio( this ) {
						esito = Esito.Ok,
						showInStatusBar = true,
						descrizione = "Carrello salvato ok"
					} );

				} catch( Exception eee ) {
					esito = false;
					string msg = ErroriUtil.estraiMessage(eee);
					_giornale.Error( msg, eee );

					pubblicaMessaggio( new Messaggio( this ) {
						esito = Esito.Errore,
						showInStatusBar = true,
						descrizione = "Errore nel salvataggio del carrello"
					} );

				}
			}

			return esito;
		}

		public bool vendereCarrello() {

			_giornale.Debug( "carrello valido. Inizio operazioni di produzione" );

			bool esito = false;

			try {

				carrello.venduto = true;

				esito = salvaCarrello();

				if( !esito )
					carrello.venduto = false;

			} finally {
				// Vado avanti ugualmente
				// Prima lancio le stampe
				eventualeStampa(carrello);

				// Poi lancio la masterizzazione
				eventualeMasterizzazione(carrello);
			}


			Messaggio info = new Messaggio( this, "Vendita completata. Totale a pagare: " + carrello.totaleAPagare );
			info.showInStatusBar = true;
			pubblicaMessaggio( info );

			return esito;
		}


		public void removeRigaCarrello( RigaCarrello rigaCarrello ) {

			gestoreCarrello.removeRiga( rigaCarrello );
			ricalcolaTotaleCarrello();
		}

		public void removeRigheCarrello( string discriminator ) {
			IEnumerable<RigaCarrello> listaDacanc = carrello.righeCarrello.Where( r => r.discriminator == discriminator );
			foreach (RigaCarrello dacanc in listaDacanc.ToArray())
			{
				gestoreCarrello.removeRiga( dacanc );
			}
			ricalcolaTotaleCarrello();
		}


		public void removeCarrello( Carrello carrello ) {
			OrmUtil.forseAttacca<Carrello>( "Carrelli", ref carrello );
			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
			dbContext.Carrelli.Remove( carrello );
		}

		public void spostaRigaCarrello(RigaCarrello rigaCarrello)
		{
			spostaRigaCarrello(rigaCarrello, true);
		}

		public void copiaSpostaRigaCarrello(RigaCarrello rigaCarrello)
		{
			RigaCarrello cloneRiga = new RigaCarrello();
			if (Carrello.TIPORIGA_MASTERIZZATA.Equals(rigaCarrello.discriminator))
			{
				cloneRiga.id = Guid.NewGuid();
				cloneRiga.bordiBianchi = rigaCarrello.bordiBianchi;
				cloneRiga.carrello = rigaCarrello.carrello;
				cloneRiga.carrello_id = rigaCarrello.carrello_id;
				cloneRiga.descrizione = rigaCarrello.descrizione;
				cloneRiga.discriminator = rigaCarrello.discriminator;
				cloneRiga.formatoCarta = rigaCarrello.formatoCarta;
				cloneRiga.fotografia = rigaCarrello.fotografia;
				cloneRiga.fotografo = rigaCarrello.fotografo;
				cloneRiga.id = rigaCarrello.id;
				cloneRiga.nomeStampante = rigaCarrello.nomeStampante;
				cloneRiga.prezzoLordoUnitario = rigaCarrello.prezzoLordoUnitario;
				cloneRiga.prezzoNettoTotale = rigaCarrello.prezzoNettoTotale;
				cloneRiga.quantita = rigaCarrello.quantita;
				cloneRiga.sconto = rigaCarrello.sconto;
				cloneRiga.totFogliStampati = rigaCarrello.totFogliStampati;

			}
			
			if (Carrello.TIPORIGA_STAMPA.Equals(rigaCarrello.discriminator))
			{
				cloneRiga.id = Guid.NewGuid();
				cloneRiga.bordiBianchi = rigaCarrello.bordiBianchi;
				cloneRiga.carrello = rigaCarrello.carrello;
				cloneRiga.carrello_id = rigaCarrello.carrello_id;
				cloneRiga.descrizione = rigaCarrello.descrizione;
				cloneRiga.discriminator = rigaCarrello.discriminator;
				cloneRiga.formatoCarta = rigaCarrello.formatoCarta;
				cloneRiga.fotografia = rigaCarrello.fotografia;
				cloneRiga.fotografo = rigaCarrello.fotografo;
				cloneRiga.id = rigaCarrello.id;
				cloneRiga.nomeStampante = rigaCarrello.nomeStampante;
				cloneRiga.prezzoLordoUnitario = rigaCarrello.prezzoLordoUnitario;
				cloneRiga.prezzoNettoTotale = rigaCarrello.prezzoNettoTotale;
				cloneRiga.quantita = rigaCarrello.quantita;
				cloneRiga.sconto = rigaCarrello.sconto;
				cloneRiga.totFogliStampati = rigaCarrello.totFogliStampati;
			}
			spostaRigaCarrello(cloneRiga, false);
		}

		private void spostaRigaCarrello(RigaCarrello rigaCarrello, bool remove)
		{
			if (remove)
				gestoreCarrello.removeRiga(rigaCarrello);
			if (Carrello.TIPORIGA_STAMPA.Equals(rigaCarrello.discriminator))
			{
				rigaCarrello.discriminator = Carrello.TIPORIGA_MASTERIZZATA;
				rigaCarrello.quantita = 1;
			}
			else if (Carrello.TIPORIGA_MASTERIZZATA.Equals(rigaCarrello.discriminator))
			{
				//Quando sposto la riga setto di default i bordi bianchi a false
				rigaCarrello.bordiBianchi = false;
				rigaCarrello.discriminator = Carrello.TIPORIGA_STAMPA;
			}
			else
			{
				_giornale.Warn("Errorre è stata spostat una riga senza dicriminator");
			}
			gestoreCarrello.aggiungiRiga(rigaCarrello);
			ricalcolaTotaleCarrello();
		}

		private void eventualeStampa(Carrello carrello) {

			// Se non ho righe nel carrello da stampare, allora esco.
			if( carrello == null || carrello.righeCarrello.Count == 0 )
				return;

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			

			int conta = 0;
			foreach( RigaCarrello riga in carrello.righeCarrello ) {

				if( riga.discriminator == Carrello.TIPORIGA_STAMPA ) {

					// Siccome il nome della stampante è un attributo transiente,
					// eventualmente lo assegno. Potrebbe essere null, quando carico un carrello dal db.
					if( riga.nomeStampante == null ) {
						StampanteAbbinata sa = _stampantiAbbinate.FirstOrDefault<StampanteAbbinata>( s => s.FormatoCarta.Equals( riga.formatoCarta ) );
						if( sa != null )
							riga.nomeStampante = sa.StampanteInstallata.NomeStampante;
						else
							_giornale.Warn( "Non riesco a stabilire la stampante di questa carta: " + riga.formatoCarta.descrizione + "(id=" + riga.formatoCarta.id + ")" );
					}

					++conta;

					// Creo nuovamente i parametri di stampa perché potrebbero essere cambiati nell GUI
					ParamStampaFoto paramStampaFoto = creaParamStampaFoto( riga );
					spoolStampeSrv.accodaStampaFoto( riga.fotografia, paramStampaFoto );
				}
			}
		}

		/**
		 * Crea una nuova riga carrello da stampare in base ai parametri di stampa richiesti
		 */
		private RigaCarrello creaRiCaFotoStampata( Fotografia fotografia, ParamStampaFoto param ) {

			RigaCarrello r = new RigaCarrello() {
				discriminator = Carrello.TIPORIGA_STAMPA
			};

			r.id = Guid.Empty;  // Lascio intenzionalmente vuoto. Lo valorizzo alla fine prima di salvare

			// Riattacco un pò di roba altrimenti si incacchia
			OrmUtil.forseAttacca<Fotografia>( "Fotografie", ref fotografia );
			FormatoCarta fc = param.formatoCarta;
			OrmUtil.forseAttacca<FormatoCarta>( "FormatiCarta", ref fc );
			Fotografo fo = fotografia.fotografo;
			OrmUtil.forseAttacca<Fotografo>( "Fotografi", ref fo );

			r.fotografia = fotografia;
			r.fotografo = fotografia.fotografo;
			r.formatoCarta = param.formatoCarta;
			r.nomeStampante = param.nomeStampante;   // Questo è un attributo transiente mi serve solo a runtime.

			r.descrizione = "Stampe formato " + param.formatoCarta.descrizione;

			r.prezzoLordoUnitario = param.formatoCarta.prezzo;
			r.quantita = param.numCopie;
			r.prezzoNettoTotale = r.prezzoLordoUnitario * r.quantita;
			r.bordiBianchi = ! param.autoZoomNoBordiBianchi;

			return r;
		}

		private ParamStampaFoto creaParamStampaFoto( RigaCarrello riCaFotoStampata ) {
			ParamStampaFoto param = new ParamStampaFoto();
			param.autoRuota = true;
			param.autoZoomNoBordiBianchi = ! (bool) riCaFotoStampata.bordiBianchi;
			param.formatoCarta = riCaFotoStampata.formatoCarta;
			param.numCopie = riCaFotoStampata.quantita;
			param.nomeStampante = riCaFotoStampata.nomeStampante;  // Attributo transiente.

			// Stampigli
			param.stampigli = configurazione.stampigli;

			// Questa informazione mi serve nella callback
			System.Diagnostics.Debug.Assert( !riCaFotoStampata.id.Equals( Guid.Empty ) );
			param.idRigaCarrello = riCaFotoStampata.id;
			return param;
		}

		/**
		 * Siccome il carrello in questione viene chiuso prima che termini la masterizzazione,
		 * tramite il suo id, è possibile andare a sistemare eventuali problemi.
		 */
		private void eventualeMasterizzazione(Carrello carrello)
		{
			// Se non ho righe nel carrello da stampare, allora esco.
			if (carrello == null || carrello.righeCarrello.Count == 0)
				return;

			IEnumerable<RigaCarrello> listaDaMast = carrello.righeCarrello.Where(r => r.discriminator == Carrello.TIPORIGA_MASTERIZZATA);
			IList<Fotografia> fotoDaMast = new List<Fotografia>();
			foreach(RigaCarrello riga in listaDaMast){
				fotoDaMast.Add(riga.fotografia);
			}

			if( fotoDaMast.Count <= 0 )
				return;

			masterizzaSrv.addFotografie(fotoDaMast);

			_masterizzaSrvImpl.start();
			_masterizzaSrvImpl.masterizza( carrello.id );
		}

		public void rimasterizza() {
			if( _masterizzaSrvImpl == null )
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
			//using( GestoreCarrello altroGestoreCarrello = new GestoreCarrello() ) {
			//    altroGestoreCarrello.stornoMasterizzate( (Guid)masterizzaMsg.senderTag, (short)masterizzaMsg.totFotoAggiunte, (short)masterizzaMsg.totFotoNonAggiunte );
			//}

		}

		/**
		 * E' stata completata una stampa. Devo gestirne l'esito
		 */
		private void gestioneEsitoStampa( LavoroDiStampa lavoroDiStampa ) {

			LavoroDiStampaFoto lavoroDiStampaFoto = null;
			LavoroDiStampaProvini lavoroDiStampaProvini = null;

			if (lavoroDiStampa is LavoroDiStampaFoto)
			{
				lavoroDiStampaFoto = lavoroDiStampa as LavoroDiStampaFoto;


				bool provaRisparmio = false;
				if (provaRisparmio && lavoroDiStampaFoto.fotografia != null)
				{
					try {
						// Rilascio un pò di memoria
						AiutanteFoto.disposeImmagini( lavoroDiStampaFoto.fotografia, IdrataTarget.Originale );
						AiutanteFoto.disposeImmagini( lavoroDiStampaFoto.fotografia, IdrataTarget.Risultante );

					} catch( Exception ee ) {
						_giornale.Error( "Impossibile rilasciare immagini dopo stampa", ee );
	
						// Devo andare avanti lo stesso perché devo notificare tutti
					}
				}
			}
			else if (lavoroDiStampa is LavoroDiStampaProvini)
			{
				lavoroDiStampaProvini = lavoroDiStampa as LavoroDiStampaProvini;
				foreach(Fotografia foto in lavoroDiStampaProvini.fotografie){
					if (false && foto != null)
					{
						try
						{
							AiutanteFoto.disposeImmagini( foto, IdrataTarget.Originale );
							AiutanteFoto.disposeImmagini( foto, IdrataTarget.Risultante );
						}
						catch (Exception ee)
						{
							_giornale.Error("Impossibile rilasciare immagini dopo stampa", ee);

							// Devo andare avanti lo stesso perché devo notificare tutti
						}
					}
				}
			}

			// Se la stampa è stata completata correttamente, non faccio niente. Sono già a posto.
			// TODO rimettere a posto con ==
			if( lavoroDiStampa.esitostampa == EsitoStampa.Ok )
			{
				if (lavoroDiStampaProvini != null)
				{
					using (new UnitOfWorkScope(true))
					{
						LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
						
						DateTime giornata = DateTime.Today;
						FormatoCarta formatoCarta = lavoroDiStampaProvini.param.formatoCarta;
						OrmUtil.forseAttacca<FormatoCarta>("FormatiCarta", ref formatoCarta);
						//Verifico se sono già stati stampati dei provini all'interno della giornata.
						ConsumoCartaGiornaliero consumoCarta = dbContext.ConsumiCartaGiornalieri.FirstOrDefault(cC => System.DateTime.Equals(cC.giornata,giornata) && cC.formatoCarta.id == formatoCarta.id);
						
						if (consumoCarta != null)
						{
							consumoCarta.diCuiProvini += lavoroDiStampaProvini.param.numPag;
						}
						else
						{
							consumoCarta = new ConsumoCartaGiornaliero();
							consumoCarta.id = Guid.NewGuid();
							consumoCarta.diCuiProvini = lavoroDiStampaProvini.param.numPag;
							consumoCarta.formatoCarta = formatoCarta;
							consumoCarta.giornata = giornata;
							dbContext.ConsumiCartaGiornalieri.Add(consumoCarta);
						}
						dbContext.SaveChanges();
					}
				}
				return;
			}
			_giornale.Error( "il lavoro di stampa non è andato a buon fine: " + lavoroDiStampa.ToString() );

			// Vado a correggere questa riga
			if(lavoroDiStampaFoto != null){
				using( GestoreCarrello altroGestoreCarrello = new GestoreCarrello() ) {
					ParamStampaFoto psf = lavoroDiStampa.param as ParamStampaFoto;
					altroGestoreCarrello.stornoRiga( psf.idRigaCarrello );
				}
			}
			// TODO Fare storno errore provini
			if (lavoroDiStampaProvini != null)
			{
				
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

			creaNuovoCarrello();
		}

		/**
		 * Questo servizio mi tengo io la implementazione perché voglio chiamare io il metodo masterizza.
		 * E non voglio che venga chiamato da fuori.
		 * Il CarrelloViewModel, ora non deve più appoggiarsi direttamente al servizio di masterizzazione.
		 * Ci penso io (VenditoreSrvImpl).
		 * Lasciare private la property.
		 */
		private MasterizzaSrvImpl _masterizzaSrvImpl;
		private IMasterizzaSrv masterizzaSrv {
			get {
				if (_masterizzaSrvImpl == null)
				{
					// Siccome non voglio che si chiami il metodo masterizza da fuori, faccio una forzatura.
					// Uso io direttamente la impl internamente.
					_masterizzaSrvImpl = (MasterizzaSrvImpl)LumenApplication.Instance.creaServizio<IMasterizzaSrv>();
				}
				return _masterizzaSrvImpl;
			}
		}


		public void aggiungereMasterizzate( IEnumerable<Fotografia> fotografie ) {

			if( ! possoAggiungereMasterizzate )
				throw new InvalidOperationException( "Impossibile aggiungere foto da masterizzare a questo carrello" );

			foreach( Fotografia foto in fotografie ) {

				// Aggiungo le foto al carrello
				gestoreCarrello.aggiungiRiga( creaRigaFotoMasterizzata( foto ) );

			}

			// Aggiungo le foto alla lista
// TODO da fare alla fine			_masterizzaSrvImpl.addFotografie( fotografie );
			// Notifico al carrello l'evento
			ricalcolaTotaleCarrello();
		}


		// creo anche una riga nel carrello (UNA SOLA)
		private RigaCarrello creaRigaFotoMasterizzata( Fotografia fotografia ) {

			RigaCarrello r = new RigaCarrello {
				discriminator = Carrello.TIPORIGA_MASTERIZZATA
			};

			r.id = Guid.Empty;  // Lascio intenzionalmente vuoto. Lo valorizzo alla fine prima di salvare
			r.quantita = 1;
			r.descrizione = "Foto masterizzata";
			
			// Riattacco un pò di roba altrimenti si incacchia
			OrmUtil.forseAttacca<Fotografia>( ref fotografia );
			Fotografo fo = fotografia.fotografo;
			OrmUtil.forseAttacca<Fotografo>( ref fo );

			r.fotografia = fotografia;
			r.fotografo = fo;

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
			p.autoZoomNoBordiBianchi = Configurazione.UserConfigLumen.autoZoomNoBordiBianchi;

			// TODO la stampante dovrei prendere quella di default di windows.
			return p;
		}

		public ParamStampaProvini creaParamStampaProvini()
		{

			ParamStampaProvini p = new ParamStampaProvini();
			p.autoRuota = true;    // non ha senso stampare una foto orizzontale nella carta verticale
			p.numCopie = 1;

			// TODO la stampante dovrei prendere quella di default di windows.
			return p;
		}


		public List<RigaReportVendite> creaReportVendite( ParamRangeGiorni p ) {

			Dictionary<DateTime, RigaReportVendite> reportVendite = new Dictionary<DateTime, RigaReportVendite>();

			_giornale.Debug( "Devo preparare il report vendite da " + p.dataIniz + " a " + p.dataFine );

			creaReportVenditeStep0( ref reportVendite, p );   // Foto scattate
			creaReportVenditeStep1( ref reportVendite, p );   // Foto stampate (cioè vendute)
			creaReportVenditeStep2( ref reportVendite, p );   // Dischetti masterizzati
			creaReportVenditeStep3( ref reportVendite, p );   // Totale incasso previsto

			return reportVendite.Values.ToList();
		}

		/// <summary>
		/// Calcolo le foto scattate (cioè quelle acquisite)
		/// </summary>
		void creaReportVenditeStep0( ref Dictionary<DateTime, RigaReportVendite> reportVendite, ParamRangeGiorni p ) {

			// Siccome ho dei valori con il tempo, testo < del giorno dopo.
			DateTime giornoDopo = p.dataFine.AddDays( 1 );

			var queryh = from ff in UnitOfWorkScope.currentDbContext.ScarichiCards
						 where ff.giornata >= p.dataIniz && ff.giornata <= p.dataFine
						 group ff by ff.giornata into grp
						 select new RigaReportVendite {
							 giornata = grp.Key,
							 totFotoScattate = grp.Sum( q => q.totFoto )
						 };

			// Ora ciclo i risultati e li metto nella mappa
			foreach( RigaReportVendite riga in queryh )
				reportVendite.Add( riga.giornata, riga );

			_giornale.Debug( "report vendite: calcolate le foto scattate." );
		}

		/// <summary>
		/// Calcolo le foto stampate
		/// </summary>
		void creaReportVenditeStep1( ref Dictionary<DateTime, RigaReportVendite> reportVendite, ParamRangeGiorni p ) {

			// Qui ho necessità di fare una join tra il carrello e le righe di tipo FotoScattata
			var querya = from cc in UnitOfWorkScope.currentDbContext.Carrelli.Include( "righeCarrello" )
						 from rr in cc.righeCarrello.Where( r => r.discriminator == Carrello.TIPORIGA_STAMPA )
						 where cc.giornata >= p.dataIniz && cc.giornata <= p.dataFine
						       && cc.venduto == true
						 select new {
							 cc,
							 rr
						 };

			// totalizzo le foto stampate divise per formato carta
			var queryb = from dd in querya
						 group dd by new {
							 dd.cc.giornata,
							 dd.rr.formatoCarta.descrizione
						 } into grp
						 select new {
							 gg = grp.Key.giornata,
							 fc = grp.Key.descrizione,
							 fogli = grp.Sum( a => a.rr.totFogliStampati )
						 };

			// Ora ciclo i risultati e creo l'apposita riga
			foreach( var ris in queryb ) {
				RigaReportVendite riga;
				if( reportVendite.ContainsKey( ris.gg ) )
					riga = reportVendite [ris.gg];
				else {
					riga = new RigaReportVendite {
						giornata = ris.gg
					};
					reportVendite.Add( ris.gg, riga );
				}
				// Sommo i campi
				riga.totFotoStampate += (ris.fogli == null ? 0 : (int)ris.fogli);
				// TODO sommare anche i diversi formati carta
			}

			_giornale.Debug( "report vendite: calcolate le foto stampate." );
		}


		/// <summary>
		/// Calcolo i dischetti masterizzati
		/// </summary>
		void creaReportVenditeStep2( ref Dictionary<DateTime, RigaReportVendite> reportVendite, ParamRangeGiorni p ) {

			// Estraggo le righe carrello di tipo disco masterizzato
			var queryc = from cc in UnitOfWorkScope.currentDbContext.Carrelli.Include( "righeCarrello" )
						 from rr in cc.righeCarrello.Where( r => r.discriminator == Carrello.TIPORIGA_MASTERIZZATA )
						 where cc.giornata >= p.dataIniz && cc.giornata <= p.dataFine
						       && cc.venduto == true
						 select new {
							 cc,
							 rr
						 };

			// Calcolo i cd masterizzati divisi per giornata
			var queryd = from dd in queryc
						 group dd by new {
							 dd.cc.giornata
						 } into grp
						 select new {
							 gg = grp.Key.giornata,
							 dvd = grp.Sum( a => a.rr.quantita )
						 };

			// Ora ciclo i risultati e creo l'apposita riga
			foreach( var ris in queryd ) {
				RigaReportVendite riga;
				if( reportVendite.ContainsKey( ris.gg ) )
					riga = reportVendite [ris.gg];
				else {
					riga = new RigaReportVendite {
						giornata = ris.gg
					};
					reportVendite.Add( ris.gg, riga );
				}
				// Sommo i campi
				riga.totDischettiMasterizzati += (int)ris.dvd;
			}

			_giornale.Debug( "report vendite: calcolati i dischetti masterizzati." );
		}

		/// <summary>
		/// Calcolo il totale incasso previsto
		/// </summary>
		void creaReportVenditeStep3( ref Dictionary<DateTime, RigaReportVendite> reportVendite, ParamRangeGiorni p ) {

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			//
			var querye = from cc in dbContext.Carrelli
						 where cc.giornata >= p.dataIniz && cc.giornata <= p.dataFine
						       && cc.venduto == true
						 group cc by cc.giornata into grp
						 select new {
							 gg = grp.Key,
							 inca = grp.Sum( k => k.totaleAPagare )
						 };


			// Ora ciclo i risultati e creo l'apposita riga
			foreach( var ris in querye ) {
				RigaReportVendite riga;
				if( reportVendite.ContainsKey( ris.gg ) )
					riga = reportVendite [ris.gg];
				else {
					riga = new RigaReportVendite {
						giornata = ris.gg
					};
					reportVendite.Add( ris.gg, riga );
				}
				// setto il totale dell'incasso giornaliero
				riga.totIncassoDichiarato = (decimal)ris.inca;
			}

			_giornale.Debug( "report vendite: calcolato l'incasso previsto." );
		}



		public bool isStatoModifica {
			get {
				return gestoreCarrello.isStatoModifica;
			}
		}

		public decimal calcolaIncassoPrevisto( DateTime giornata ) {

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			// Questa sintassi strana nella Sum, serve per gestire un caso rompiscatole. Se il set è vuoto, la somma torna null.
			// vedere qui: http://adventuresinsoftware.com/blog/?p=478
			decimal incasso = dbContext.Carrelli.Where( c => c.giornata == giornata && c.venduto == true ).Sum( p => (decimal ?)p.totaleAPagare) ?? 0;

			_giornale.Debug( "Calcolato incasso previsto giornata= " + giornata + " incasso=" + incasso );

			return incasso;
		}

		public IList<IncassoFotografo> calcolaIncassiFotografiPrevisti( DateTime giornata ) {

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			List<IncassoFotografo> incassiFotografiDelGiorno = new List<IncassoFotografo>();

			var incassi = dbContext.IncassiFotografi.Where( i => i.carrello.giornata == giornata && i.carrello.venduto == true );

			foreach( IncassoFotografo incaLoop in incassi ) {

				IncassoFotografo incaPrec = incassiFotografiDelGiorno.SingleOrDefault( ii => ii.fotografo.id == incaLoop.fotografo.id );
				if( incaPrec == null ) {
					incaPrec = new IncassoFotografo();
					incaPrec.fotografo = incaLoop.fotografo;
					incassiFotografiDelGiorno.Add( incaPrec );
				}

				// Sommo i valori
				incaPrec.contaMasterizzate += incaLoop.contaMasterizzate;
				incaPrec.contaStampe += incaLoop.contaStampe;
				incaPrec.incasso += incaLoop.incasso;
				incaPrec.incassoMasterizzate += incaLoop.incassoMasterizzate;
				incaPrec.incassoStampe += incaLoop.incassoStampe;
			}

			return incassiFotografiDelGiorno;
		}


		/// <summary>
		/// Elimina tutte le righe da masterizzare e azzera tutti i dati del masterizzatore.
		/// </summary>
		public void removeDatiDischetto() {
			this.removeRigheCarrello( Carrello.TIPORIGA_MASTERIZZATA );
			setDatiDischetto( TipoDestinazione.NULLA, null, null );
		}

		public void setDatiDischetto( TipoDestinazione tipoDest, string nomeCartella ) {
			masterizzaSrv.impostaDestinazione( tipoDest, nomeCartella );
		}

		public void setDatiDischetto( TipoDestinazione tipoDest, string nomeCartella, decimal? prezzoDischetto ) {

			setDatiDischetto( tipoDest, nomeCartella );

			// Notifico al carrello l'evento
			if( this.carrello.prezzoDischetto != prezzoDischetto ) {
				this.carrello.prezzoDischetto = prezzoDischetto;
				ricalcolaTotaleCarrello();
			}
		}


		public void rimpiazzaFotoInRiga( RigaCarrello riga, Fotografia fMod ) {

			// Rilascio eventuali immagini precedenti
			AiutanteFoto.disposeImmagini( riga.fotografia, IdrataTarget.Tutte );

			// Rileggo da disco la fotografia
			gestoreCarrello.rimpiazzaFotoInRiga( fMod.id );

			// idrato il provino per visualizzarlo
			AiutanteFoto.idrataImmaginiFoto( riga.fotografia, IdrataTarget.Provino );
		}


	}
}