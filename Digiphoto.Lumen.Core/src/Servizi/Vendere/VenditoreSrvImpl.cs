using System;
using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using log4net;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi.Reports;
using System.ComponentModel;

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

		public bool isPossibileModificareCarrello {
			get {
				return gestoreCarrello.isPossibileModificareCarrello;
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

		public string spazioFotoDaMasterizzate
		{
			get
			{
				return gestoreCarrello.spazioFotoDaMasterizzate;
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
				if( modoVendita == ModoVendita.StampaDiretta )
					return true;
				else
					return (carrello != null && carrello.venduto == false);
			}
		}

		public bool possoAggiungereMasterizzate {
			get {
				return (carrello != null && carrello.venduto == false);
			}
		}

		public override bool possoChiudere()
		{
			return gestoreCarrello.isCarrelloVuoto || gestoreCarrello.isCarrelloModificato == false;
		}

		public bool isPossibileClonareCarrello {
			get {
				return gestoreCarrello.possoClonareCarrello;
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

			contaMessaggiInCoda = 0;
		}

		protected override void Dispose( bool disposing ) {

			gestoreCarrello.Dispose(); // Importante: mi serve per rilasciare il DbContext (che altrimenti rimane aperto e tiene sotto scacco le entità)
		}

		public override void start() {

			base.start();

			creareNuovoCarrello();

			// TODO sostituire con la lista che è dentro il servizio spoolsrv
			_stampantiAbbinate = StampantiAbbinateUtil.deserializza( Configurazione.UserConfigLumen.stampantiAbbinate );

		}

		public void ricalcolaProvvigioni() {
			gestoreCarrello.ricalcolaDocumento( true );
		}

		public void ricalcolaTotaleCarrello() {

			// Sistemo i prezzi e i totali documento
			gestoreCarrello.ricalcolaDocumento( false );

			inviaMessaggioValoreCarrelloCambiato();
		}

		private void inviaMessaggioValoreCarrelloCambiato() {
			inviaMessaggioValoreCarrelloCambiato( false );
        }

		/// <summary>
		/// Invio un messaggio sul bus che il prezzo del carrello è cambiato
		/// </summary>
		private void inviaMessaggioValoreCarrelloCambiato( bool cambiateRighe ) {
			GestoreCarrelloMsg msg = new GestoreCarrelloMsg( this );
			msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.UpdateCarrello;
			if( cambiateRighe )
				msg.descrizione = "cambiate-righe";
			LumenApplication.Instance.bus.Publish( msg );
		}

		public void caricareCarrello( Carrello c ) {

			ascoltatorePropertyChangedElimina();

			gestoreCarrello.caricaCarrello( c.id );

			ascoltatorePropertyChangedCrea();


			GestoreCarrelloMsg msg = new GestoreCarrelloMsg( this );
			msg.descrizione = "Caricato carrello dal database: " + c.intestazione;
			msg.showInStatusBar = true;
			msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.LoadCarrelloSalvato;
			LumenApplication.Instance.bus.Publish( msg );
		}

		private void ascoltatorePropertyChangedCrea() {
			// Aggancio un ascoltatore di attributi cambiati
			gestoreCarrello.carrello.PropertyChanged += carrelloCorrente_PropertyChanged;
		}

		private void ascoltatorePropertyChangedElimina() {
			if( gestoreCarrello != null && gestoreCarrello.carrello != null )
				gestoreCarrello.carrello.PropertyChanged -= carrelloCorrente_PropertyChanged;
		}

		/// <summary>
		/// Se viene modificato il prezzo del dischetto, oppure il totale forfettario, rilancio un messaggio di valorizzazione modificata.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void carrelloCorrente_PropertyChanged( object sender, PropertyChangedEventArgs e ) {

			if( e.PropertyName == "prezzoDischetto" || e.PropertyName == "totaleAPagare" ) {
				inviaMessaggioValoreCarrelloCambiato();
            }
		}

		public void aggiungereStampe( Fotografia fotografia, Stampare.ParamStampa param ) {
			Fotografia [] fotos = { fotografia };
			aggiungereStampe( fotos, param );
		}

		/** 
		 * Per ogni foto indicata, creo una nuova riga di carrello
		 */
		public void aggiungereStampe( IEnumerable<Fotografia> fotografie, Stampare.ParamStampa param ) {

			if( ! possoAggiungereStampe )
				throw new InvalidOperationException( "Impossibile aggiungere stampe a questo carrello" );

			Exception laPrimaEccezione = null;

			if (param is ParamStampaFoto)
			{
				foreach (Fotografia foto in fotografie)
				{
					try {

						// Non so perchè me devo fare la versione forzata perchè se no non mi idrata i provini del carrello. 
						// Nel caso in cui ricarico una foto che è già stata stampata precedentemente. 
						AiutanteFoto.idrataImmaginiFoto( foto , IdrataTarget.Provino, true);
						gestoreCarrello.aggiungiRiga(creaRiCaFotoStampata(foto, param as ParamStampaFoto));

					} catch( Exception ee ) {
						_giornale.Error( "Aggingi stampe al carrello", ee );
						if( laPrimaEccezione == null )
							laPrimaEccezione = ee;
					}

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


			if( laPrimaEccezione != null )
				throw laPrimaEccezione;
		}

		public void creareNuovoCarrello() {

			if( gestoreCarrello != null ) {
				ascoltatorePropertyChangedElimina();
				gestoreCarrello.Dispose();
				gestoreCarrello = null;
			}

			modoVendita = Configurazione.UserConfigLumen.modoVendita;

			gestoreCarrello = new GestoreCarrello();
			gestoreCarrello.creaNuovo();
			
			ascoltatorePropertyChangedCrea();

			GestoreCarrelloMsg msg = new GestoreCarrelloMsg( this );
			msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.CreatoNuovoCarrello;
			LumenApplication.Instance.bus.Publish( msg );
			
			if (_masterizzaSrvImpl != null)
			{
				_masterizzaSrvImpl.Dispose();
				_masterizzaSrvImpl = null;
			}
		}

		public string salvareCarrello() {
			return salvareCarrello( false  );
		}

        private string salvareCarrello( bool vendere ) {

			string msgErrore = null;

			//
			// Siccome l'esito della stampa e della masterizzazione lo riceverò più tardi 
			// ed in modo asincrono, in questo momento non posso fare altro che dare per scontato
			// che andrà tutto bene.
			// Quindi memorizzo il carrello intero. Poi gestirò i problemi (sperando che non ce ne siano).
			//

			try {

				// Poi salvo il carrello
				gestoreCarrello.salvare( vendere );

				_giornale.Debug( "salvataggio carrello " + carrello.id + " a buon fine" );

				pubblicaMessaggio( new Messaggio( this ) {
					esito = Esito.Ok,
					showInStatusBar = true,
					descrizione = "Carrello salvato ok"
				} );


			} catch( Exception eee ) {

				msgErrore = ErroriUtil.estraiMessage(eee);
				_giornale.Error( msgErrore, eee );

				pubblicaMessaggio( new Messaggio( this ) {
					esito = Esito.Errore,
					showInStatusBar = true,
					descrizione = "Errore nel salvataggio del carrello:\r\n" + msgErrore
				} );

			}

			return msgErrore;
		}

		public void clonareCarrello() {
			gestoreCarrello.clonareCarrello();
		}


        public string vendereCarrello() {

			_giornale.Debug( "carrello valido. Inizio operazioni di produzione" );

			string msgErrore = null;

			try {

				msgErrore = salvareCarrello( true );

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

			return msgErrore;
		}


		public void eliminareRigaCarrello( RigaCarrello rigaCarrello ) {

			gestoreCarrello.removeRiga( rigaCarrello );

			if (masterizzaSrv != null && masterizzaSrv.fotografie!=null && masterizzaSrv.fotografie.Contains(rigaCarrello.fotografia))
				masterizzaSrv.fotografie.Remove(rigaCarrello.fotografia);

			ricalcolaTotaleCarrello();
		}

		public void eliminareRigheCarrello( string discriminator ) {
			IEnumerable<RigaCarrello> listaDacanc = carrello.righeCarrello.Where( r => r.discriminator == discriminator );
			foreach (RigaCarrello dacanc in listaDacanc.ToArray())
			{
				gestoreCarrello.removeRiga( dacanc );
			}
			ricalcolaTotaleCarrello();
		}


		public void eliminareCarrello( Carrello carrello ) {
			gestoreCarrello.elimina( carrello );
		}

		public void spostareRigaCarrello(RigaCarrello rigaCarrello)
		{
			spostaRigaCarrello(rigaCarrello, true);
		}

        public void spostareTutteRigheCarrello(string discriminator, ParametriDiStampa parametriDiStampa)
        {
            IEnumerable<RigaCarrello> listaDaSpostare = carrello.righeCarrello.Where(r => r.discriminator == discriminator);
			
            string d = RigaCarrello.getDiscriminatorOpposto(discriminator);

            foreach (RigaCarrello rigaDaSpostare in listaDaSpostare.ToArray())
            {
                if(!GestoreCarrello.isStessaFotoInCarrello(carrello, rigaDaSpostare, d))
                {
                    if (RigaCarrello.TIPORIGA_STAMPA.Equals(d))
                    {
                        rigaDaSpostare.formatoCarta = parametriDiStampa.FormatoCarta;
                        rigaDaSpostare.nomeStampante = parametriDiStampa.NomeStampante;
                        rigaDaSpostare.quantita = parametriDiStampa.Quantita;
                        rigaDaSpostare.prezzoLordoUnitario = parametriDiStampa.PrezzoLordoUnitario;
                        rigaDaSpostare.prezzoNettoTotale = parametriDiStampa.PrezzoNettoTotale;
                    }
                    gestoreCarrello.spostaRigaCarrello(rigaDaSpostare, true);
                }
            }
            
            inviaMessaggioValoreCarrelloCambiato(true);
        }

        private void spostaRigaCarrello(RigaCarrello rigaCarrello, bool remove)
        {
            gestoreCarrello.spostaRigaCarrello(rigaCarrello, remove);
            inviaMessaggioValoreCarrelloCambiato(true);
        }

        public void copiaSpostaRigaCarrello( RigaCarrello rigaSorgente, ParametriDiStampa parametriDiStampa )
		{
			RigaCarrello cloneRiga = new RigaCarrello();

			cloneRiga.id = Guid.Empty;
			
			cloneRiga.carrello = rigaSorgente.carrello;
			cloneRiga.descrizione = rigaSorgente.descrizione;
			cloneRiga.discriminator = rigaSorgente.discriminator;

			cloneRiga.fotografia = rigaSorgente.fotografia;
			cloneRiga.fotografo = rigaSorgente.fotografo;
			cloneRiga.sconto = rigaSorgente.sconto;
			cloneRiga.totFogliStampati = rigaSorgente.totFogliStampati;

			//associo il nuovo formato carta alla riga
			if( parametriDiStampa != null ) {
				cloneRiga.bordiBianchi = parametriDiStampa.BordiBianchi;
				cloneRiga.formatoCarta = parametriDiStampa.FormatoCarta;
				cloneRiga.nomeStampante = parametriDiStampa.NomeStampante;
				cloneRiga.quantita = parametriDiStampa.Quantita;
				cloneRiga.prezzoLordoUnitario = parametriDiStampa.PrezzoLordoUnitario;
				cloneRiga.prezzoNettoTotale = parametriDiStampa.PrezzoNettoTotale;
			}

			spostaRigaCarrello( cloneRiga, false );
		}

        public void copiaSpostaTutteRigheCarrello(string discriminaSorg, ParametriDiStampa parametriDiStampa)
        {
            IEnumerable<RigaCarrello> listaDaCopiareSpostare = carrello.righeCarrello.Where(r => r.discriminator == discriminaSorg);

            string discriminaDest = RigaCarrello.getDiscriminatorOpposto(discriminaSorg);

            foreach ( RigaCarrello rigaDaCopiareSpostare in listaDaCopiareSpostare.ToArray() )
            {
                if (!GestoreCarrello.isStessaFotoInCarrello(carrello, rigaDaCopiareSpostare, discriminaDest))
                {
                    copiaSpostaRigaCarrello( rigaDaCopiareSpostare, parametriDiStampa );
                }
            }
        }

		private void eventualeStampa(Carrello carrello) {

			// Se non ho righe nel carrello da stampare, allora esco.
			if( carrello == null || carrello.righeCarrello.Count == 0 )
				return;

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			

			int conta = 0;
			foreach( RigaCarrello riga in carrello.righeCarrello ) {

				if( riga.isTipoStampa ) {

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
				discriminator = RigaCarrello.TIPORIGA_STAMPA
			};

			r.id = Guid.Empty;  // Lascio intenzionalmente vuoto. Lo valorizzo alla fine prima di salvare

			// Riattacco un pò di roba altrimenti si incacchia
			// A volte possono esserci degli errori durante il riattacco. Cerco di evitarli

			try {
				OrmUtil.forseAttacca<Fotografia>( ref fotografia );
			} catch( Exception ) {
			}

			try {
				FormatoCarta fc = param.formatoCarta;
				OrmUtil.forseAttacca<FormatoCarta>( ref fc );
			} catch( Exception ) {
			}

			try {
				Fotografo fo = fotografia.fotografo;
				OrmUtil.forseAttacca<Fotografo>( ref fo );
			} catch( Exception ) {
			}

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

			IEnumerable<RigaCarrello> listaDaMast = carrello.righeCarrello.Where(r => r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA);
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

#if FALSE
				bool provaRisparmio = false;
				if (provaRisparmio && lavoroDiStampaFoto.fotografia != null)
				{
					// Ora che so che è andata bene la stampa, devo incrementare il numero di volte che è stata stampata
					
					try {
						// Rilascio un pò di memoria
						AiutanteFoto.disposeImmagini( lavoroDiStampaFoto.fotografia, IdrataTarget.Originale );
						AiutanteFoto.disposeImmagini( lavoroDiStampaFoto.fotografia, IdrataTarget.Risultante );

					} catch( Exception ee ) {
						_giornale.Error( "Impossibile rilasciare immagini dopo stampa", ee );
	
						// Devo andare avanti lo stesso perché devo notificare tutti
					}
				}
#endif
			} else if (lavoroDiStampa is LavoroDiStampaProvini)
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

			// Se la stampa è stata completata correttamente, aggiorno alcuni dati di consumo
			if( lavoroDiStampa.esitostampa == EsitoStampa.Ok ) {

				if( lavoroDiStampaFoto != null ) {

					Fotografia foto = lavoroDiStampaFoto.fotografia;

					// Devo aggiornare la fotografia sul db.

					using( new UnitOfWorkScope() ) {

						LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
						Fotografia foto1 = dbContext.Fotografie.Single( f => f.id == lavoroDiStampaFoto.fotografia.id );
						foto1.contaStampata += lavoroDiStampaFoto.param.numCopie;
						dbContext.SaveChanges();
                    }

				}

				if( lavoroDiStampaProvini != null ) {
					using( new UnitOfWorkScope() ) {
						LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

						DateTime giornata = DateTime.Today;
						FormatoCarta formatoCarta = lavoroDiStampaProvini.param.formatoCarta;
						OrmUtil.forseAttacca<FormatoCarta>( ref formatoCarta );
						//Verifico se sono già stati stampati dei provini all'interno della giornata.
						ConsumoCartaGiornaliero consumoCarta = dbContext.ConsumiCartaGiornalieri.FirstOrDefault( cC => System.DateTime.Equals( cC.giornata, giornata ) && cC.formatoCarta.id == formatoCarta.id );

						if( consumoCarta != null ) {
							consumoCarta.diCuiProvini += lavoroDiStampaProvini.param.numPag;
						} else {
							consumoCarta = new ConsumoCartaGiornaliero();
							consumoCarta.id = Guid.NewGuid();
							consumoCarta.diCuiProvini = lavoroDiStampaProvini.param.numPag;
							consumoCarta.formatoCarta = formatoCarta;
							consumoCarta.giornata = giornata;
							dbContext.ConsumiCartaGiornalieri.Add( consumoCarta );
						}
						dbContext.SaveChanges();
					}
				}

			} else {

				_giornale.Error( "il lavoro di stampa non è andato a buon fine: " + lavoroDiStampa.ToString() );

				// Vado a correggere questa riga
				if(lavoroDiStampaFoto != null){
					using( GestoreCarrello altroGestoreCarrello = new GestoreCarrello() ) {
						ParamStampaFoto psf = lavoroDiStampa.param as ParamStampaFoto;
						altroGestoreCarrello.stornoRiga( psf.idRigaCarrello );
					}
				}
				if (lavoroDiStampaProvini != null)
				{
					// TODO Fare storno errore provini
				}
			}

		}

		public override void stop() {

			// Rimango in attesa fino a che ho dei messaggi da elaborare
			while( contaMessaggiInCoda > 0 )
				System.Threading.Thread.Sleep( 2000 );

			base.stop();
		}

		public void abbandonareCarrello() {

			if( gestoreCarrello != null ) {
				ascoltatorePropertyChangedElimina();
				gestoreCarrello.Dispose();
			}

			creareNuovoCarrello();
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

			Exception laPrimaEccezione = null;

			foreach( Fotografia foto in fotografie ) {

				try {

					// Aggiungo le foto al carrello
					gestoreCarrello.aggiungiRiga( creaRigaFotoMasterizzata( foto ) );

				} catch( Exception ee ) {

					_giornale.Error( "Aggingi stampe al carrello", ee );

					if( laPrimaEccezione == null )
						laPrimaEccezione = ee;
				}

			}

			// Aggiungo le foto alla lista
// TODO da fare alla fine			_masterizzaSrvImpl.addFotografie( fotografie );
			// Notifico al carrello l'evento
			ricalcolaTotaleCarrello();

			if( laPrimaEccezione != null )
				throw laPrimaEccezione;
		}


		// creo anche una riga nel carrello (UNA SOLA)
		private RigaCarrello creaRigaFotoMasterizzata( Fotografia fotografia ) {

			RigaCarrello r = new RigaCarrello {
				discriminator = RigaCarrello.TIPORIGA_MASTERIZZATA
			};

			r.id = Guid.Empty;  // Lascio intenzionalmente vuoto. Lo valorizzo alla fine prima di salvare
			r.quantita = 1;
			r.descrizione = "Foto masterizzata";

			// Riattacco un pò di roba altrimenti si incacchia
			try {
				OrmUtil.forseAttacca<Fotografia>( ref fotografia );
			} catch( Exception ) {
			}
			r.fotografia = fotografia;

			try {
				Fotografo fo = fotografia.fotografo;
				OrmUtil.forseAttacca<Fotografo>( ref fo );
				r.fotografo = fo;
			} catch( Exception ) {
				r.fotografo = fotografia.fotografo;
			}

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

		public List<RigaReportProvvigioni> creaReportProvvigioni( ParamRangeGiorni p ) {

			_giornale.Debug( "Devo preparare il report provvigioni da " + p.dataIniz + " a " + p.dataFine );

			var queryh = from inf in this.objectContext.IncassiFotografi
						 where 1==1
						   && inf.carrello.venduto == true
			                           && inf.carrello.giornata >= p.dataIniz && inf.carrello.giornata <= p.dataFine
			                         orderby inf.fotografo.cognomeNome
			                         group inf by inf.fotografo.cognomeNome into grp
			                         select new RigaReportProvvigioni {
			                                nomeFotografo = grp.Key,
			                                incasso = grp.Sum( q2 => q2.incasso ),
			                                incassoStampe = grp.Sum( q3 => q3.incassoStampe ),
			                                incassoMasterizzate = grp.Sum( q4 => q4.incassoMasterizzate ),
			                                contaStampe = grp.Sum( q5 => q5.contaStampe ),
			                                contaMasterizzate = grp.Sum( q6 => q6.contaMasterizzate )
			                         };

			return queryh.ToList();
		}


		public List<RigaReportVendite> creaReportVendite( ParamRangeGiorni p ) {

			Dictionary<DateTime, RigaReportVendite> reportVendite = new Dictionary<DateTime, RigaReportVendite>();

			_giornale.Debug( "Devo preparare il report vendite da " + p.dataIniz + " a " + p.dataFine );

			creaReportVenditeStep0( ref reportVendite, p );   // Foto scattate
			creaReportVenditeStep1( ref reportVendite, p );   // Foto stampate (cioè vendute)
			creaReportVenditeStep2( ref reportVendite, p );   // Dischetti masterizzati
			creaReportVenditeStep3( ref reportVendite, p );   // Totale incasso dichiarato nei carrelli
			creaReportVenditeStep4( ref reportVendite, p );   // Chiusura di cassa giorno

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
						 from rr in cc.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA )
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
						 from rr in cc.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA )
						 where cc.giornata >= p.dataIniz && cc.giornata <= p.dataFine
						       && cc.venduto == true
						 select new {
							 cc,
							 rr
						 };

			// Calcolo i cd masterizzati divisi per giornata e totalizzando per carrello (1 carrello = 1 cd)
			var queryd = from dd in queryc
						 group dd by new {
							 dd.cc.giornata,
							 dd.cc.id
						 } into grp
						 select new {
							 gg = grp.Key.giornata,
							 contaMaster = grp.Count()   // 1 riga = 1 foto masterizzata (tanto la qta è sempre uno)
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
				riga.totDischettiMasterizzati += 1;
				riga.totFotoMasterizzate += (int)ris.contaMaster;
			}

			_giornale.Debug( "report vendite: calcolati i dischetti masterizzati." );
		}

		/// <summary>
		/// Calcolo il totale incasso dichiarato nei carrelli
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

			_giornale.Debug( "report vendite: calcolato l'incasso dichiarato." );
		}


		/// <summary>
		/// Arricchisco con eventuale chiusura di cassa
		/// </summary>
		void creaReportVenditeStep4( ref Dictionary<DateTime, RigaReportVendite> reportVendite, ParamRangeGiorni p ) {

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

			foreach( Giornata giornata in dbContext.Giornate.Where( gg => gg.id >= p.dataIniz && gg.id <= p.dataFine ) ) {
				RigaReportVendite riga;
				if( reportVendite.ContainsKey( giornata.id ) )
					riga = reportVendite[giornata.id];
				else {
					riga = new RigaReportVendite {
						giornata = giornata.id
					};
					reportVendite.Add( giornata.id, riga );
				}
				// setto i dati della chiusura
				riga.ccTotIncassoPrevisto = giornata.incassoPrevisto;
				riga.ccTotIncassoDichiarato = giornata.incassoDichiarato;
			}

			_giornale.Debug( "report vendite: elaborate chiusure di cassa" );
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
			this.eliminareRigheCarrello( RigaCarrello.TIPORIGA_MASTERIZZATA );
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

		/// <summary>
		/// Ritorno la lista degli ID delle foto che sono nel carrello
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Guid> enumeraIdsFoto() {

			if( this.carrello == null )
				return null;
			else
				return this.carrello.righeCarrello.Where( rr1 => rr1 != null && rr1.fotografia != null ).Select( rr2 => rr2.fotografia.id );

		}
	}
}