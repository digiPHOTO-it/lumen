using System;
using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.Model;
using System.Data.Entity.Core.Objects.DataClasses;
using log4net;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Applicazione;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Config;
using System.ComponentModel;
using Digiphoto.Lumen.Util;
using System.Data.Entity.Core;
using System.IO;
using System.Data.Entity.Validation;
using System.Data.Entity;
using Digiphoto.Lumen.Servizi.Stampare;

namespace Digiphoto.Lumen.Servizi.Vendere {

#if DEBUG
	public
#else
	internal
#endif
	class GestoreCarrello : IDisposable, INotifyPropertyChanged {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( GestoreCarrello ) );

		#region Costruttore

#if DEBUG
	public
#else
	protected internal 
#endif
		GestoreCarrello() {
			// Provo a lavorare con un mio contesto separato dagli altri, per vedere se risolvo i problemi di attacca/stacca
			creaMioDbContext();
		}

		~GestoreCarrello() {
			if( mioDbContext != null ) {
				// Qui non dovrebbe mai capitare.
				_giornale.Warn( "Nel distruttore di GestoreCarrello, in context è ancora vivo. Come mai ?" );
			}
		}
		#endregion Costruttore



		#region Propietà
		private Carrello _carrello;
		public Carrello carrello {
			get {
				return _carrello;
			}
			private set {
				if( _carrello != value ) {
					_carrello = value;
					OnPropertyChanged( "carrello" );
				}
			}
		}

		/// <summary>
		/// True  = sono in modifica
		/// False = sono in inserimento 
		/// </summary>
		public bool isStatoModifica {
			get;
			private set;
		}

		public bool isPossibileSalvare {
			get {
				return isCarrelloValido;
			}
		}

		public bool isCarrelloModificato { 
			get; 
			private set; 
		}

		public bool isCarrelloTransient {
			get {
				return isStatoModifica == false && (carrello.id == null || carrello.id.Equals( Guid.Empty ));
			}
		}

		public bool isCarrelloValido {
			get {
				return (msgValidaCarrello() == null);
			}
		}

		public bool isPossibileModificareCarrello {
			get {
				return ! carrello.venduto;
			}
		}

		public bool isCarrelloVuoto
		{
			get
			{
				return carrello.righeCarrello.Count == 0;
			}
		}

		/// <summary>
		/// Ritorno una percenuale di sconto intera calcolata in base al prezzo del carrello calcolato automaticamente, 
		/// confrontato con il totaleAPagare impostato eventualmente dall'utente.
		/// Esempio: se il carrello costa 100 euro e il totale a pagare è 80, allora ho fatto uno sconto del 20%
		/// </summary>
		public short? scontoApplicato {
			get {
				if (carrello.totaleAPagare == 0 || 
					prezzoNettoTotale == 0 ||
					carrello.totaleAPagare == null)
					return null;

				decimal x = (100 * (decimal)carrello.totaleAPagare) / prezzoNettoTotale;

				if( x > short.MaxValue )
					x = short.MaxValue;
				if( x < short.MinValue )
					x = short.MinValue;

				return (short)(100 - x);
			}
		}

		/// <summary>
		/// Mi dice quante foto da masterizzare ci sono nel carrello 
		/// </summary>
		public int sommatoriaFotoDaMasterizzare {
			get {
				return carrello.righeCarrello.Count( r => r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA );
			}
		}

		/// <summary>
		/// Mi dice quante foto da stampare ci sono nel carrello 
		/// </summary>
		public int sommatoriaQtaFotoDaStampare {
			get {
				return carrello.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA ).Sum( rfs => rfs.quantita );
			}
		}

		public decimal sommatoraPrezziFotoDaStampare {
			get {
				return carrello.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA ).Sum( rfs => rfs.prezzoNettoTotale );
			}
		}

		public string spazioFotoDaMasterizzate
		{
			get
			{
				long totalLength = 0;
				foreach (RigaCarrello riga in carrello.righeCarrello.Where(r => r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA))
                {
					if( riga.fotografia != null ) {
						String filePath = (Configurazione.cartellaRepositoryFoto + Path.DirectorySeparatorChar + riga.fotografia.nomeFile);
						totalLength += new FileInfo( filePath ).Length;
					}
				}

				//long size = totalLength / 1024;

				return Format.ByteSize(totalLength);

				//return string.Format( "{0} MB", size / 1024 );

			}
		}


		/// <summary>
		/// Questo è il totale aritmetico del carrello. E' dato dalla somma di tutte le righe con stampe + il prezzo globale del dischetto (se esiste)
		/// </summary>
		public Decimal prezzoNettoTotale {

			get {
				return carrello.righeCarrello.Where( r => r.discriminator == RigaCarrello.TIPORIGA_STAMPA ).Sum( r => r.prezzoNettoTotale ) + (carrello.prezzoDischetto != null ? (decimal)carrello.prezzoDischetto : (decimal)0);
			}
		}

		/// <summary>
		/// Per evitare i problemi di attacca/stacca e duplicati ID in sessione,
		/// provo a tenermi un mio context solo per la gestione del carrello.
		/// Questo context non viene chiuso mai, quindi gli oggetti dovrebbero non staccarsi mai 
		/// evitando i problemi noti.
		/// </summary>
		private LumenEntities _mioDbContext;
		private LumenEntities mioDbContext {
			get {
				return _mioDbContext;
			}
			set {
				_mioDbContext = value;
			}
		}

		private ObjectContext mioObjContext {
			get {
				return mioDbContext == null ? null : ((IObjectContextAdapter)mioDbContext).ObjectContext;
			}
		}

		#endregion Proprietà


		#region Metodi
		private void creaMioDbContext() {
			
			// Rilascio eventuale context precedente
			if( mioDbContext != null ) {
				mioDbContext.Dispose();
				mioDbContext = null;
			}

			mioDbContext = new LumenEntities();
		}

		public void creaNuovo() {

			creaMioDbContext();

			carrello = new Carrello();
			carrello.giornata = LumenApplication.Instance.stato.giornataLavorativa;

			carrello.righeCarrello = new EntityCollection<RigaCarrello>();
			//Metto un'intestazione automatica per distinguere il carrello autogenerato dagli altri
			// scarrello.intestazione = "Auto";
			isStatoModifica = false;
			isCarrelloModificato = false;
		}

		/// <summary>
		/// Carico da disco un carrello esistente
		/// </summary>
		/// <param name="idCarrello"></param>
		public void caricaCarrello( Guid idCarrello )
		{
			creaMioDbContext();

			this.carrello = mioDbContext.Carrelli
				.Include( c1 => c1.righeCarrello )
				.Include( c2 => c2.incassiFotografi )
				.Where( r => r.id == idCarrello )
				.Single();
			
			foreach( IncassoFotografo ff in carrello.incassiFotografi ) {
				mioObjContext.LoadProperty( ff, c => c.fotografo );
				mioObjContext.Detach( ff.fotografo );
			}

			foreach( RigaCarrello rr in carrello.righeCarrello ) {

				if( rr.fotografia != null ) {
					mioObjContext.LoadProperty( rr, r => r.fotografia );
					mioObjContext.Detach( rr.fotografia );
				}

				if( rr.formatoCarta != null ) {
					mioObjContext.LoadProperty( rr, r => r.formatoCarta );
					mioObjContext.Detach( rr.formatoCarta );
				}
			}

			// Se il prezzo del carrello non è stato cambiato a mano, lo azzero in questo modo se lo risalverò, mi verrà aggiornato il totale nuovamente.
			if( prezzoNettoTotale == carrello.totaleAPagare )
				carrello.totaleAPagare = null;

			isStatoModifica = true;
			isCarrelloModificato = false;
		}

		/**
		 * Mi dice se il carrello corrente è valido e pronto per essere salvato.
		 * Ritorna NULL se tutto ok.
		 * Altrimenti la stringa con il messaggio di errore
		 */
		public string msgValidaCarrello() {

			string errore = null;

			if( carrello == null )
				errore = "Carrello nullo";
			else if( carrello.righeCarrello.Count == 0 )
				errore = "Carrello senza righe";
			else if( carrello.giornata == DateTime.MinValue )
				errore = "Giornata vuota";
			else if( carrello.tempo == DateTime.MinValue )
				errore = "tempo non indicato";
			else if( carrello.prezzoDischetto == null && sommatoriaFotoDaMasterizzare > 0 )
				errore = "manca il prezzo del dischetto";
			else {
				// Controllo che sulle righe da stampare ci sia il nome della stampante e che sia valida
				foreach( var riga in carrello.righeCarrello ) {
					if( riga.discriminator == RigaCarrello.TIPORIGA_STAMPA )
						if( String.IsNullOrEmpty( riga.nomeStampante ) ) {
							errore = "In alcune righe non è stato indicato il formato carta o la stampante";
							break;
						} else {
							// Verifico che la stampante sia ancora presente come nome nel computer
							var stampantiInstallateSrv = LumenApplication.Instance.getServizioAvviato<IStampantiInstallateSrv>();
							var stampanteInstallata = stampantiInstallateSrv.getStampanteInstallataByString( riga.nomeStampante );
							if( stampanteInstallata == null ) {
								errore = "Stampante " + riga.nomeStampante + " non esistente";
								break;
							}
						}
				}
			}

			return errore;
		}

		public void removeRiga( RigaCarrello rigaDacanc ) {

			EntityState stato = OrmUtil.getEntityState( rigaDacanc, mioDbContext );

			// Rimuovo l'elemento dalla collezione. 
			// Non so perché, ma essendo una relazione identificante, perché EF NON si preoccua di rimuovere anche da disco la riga da solo ??
			// Dovrebbe chiamare la delete sul db, ma non lo fa! ...
			bool test = carrello.righeCarrello.Remove( rigaDacanc );
			if( !test ) {
				_giornale.Error( "Si è cercato di cancellare una riga dal carrello che non esiste. Probabilmente la riga era già stata eliminata dal carrello in precedenza" );
				throw new LumenException( "La riga non è presente nel carrello" );
			}

			// ... per quanto sopra, se l'oggetto era persistente, devo preoccuparmi io di rimuoverlo anche dal dbcontext
			if( isStatoModifica == true && rigaDacanc.id != Guid.Empty ) {
				if( stato == EntityState.Detached || stato == EntityState.Modified || stato == EntityState.Unchanged ) {
					mioDbContext.RigheCarrelli.Remove( rigaDacanc );
				}
			}

			isCarrelloModificato = true;
		} 

		public void aggiungiRiga( RigaCarrello riga ) {
			
			if( riga.fotografia == null )
				throw new ArgumentNullException( "nella RigaCarrello è obbligatoria la Fotografia" );
			if( riga.fotografo == null )
				throw new ArgumentNullException( "nella RigaCarrello è obbligatorio il Fotografo" );
			if( riga.discriminator == RigaCarrello.TIPORIGA_STAMPA )
				if( riga.formatoCarta == null )
					throw new ArgumentNullException( "nella RigaCarrello da stampare è obbligatorio il FormatoCarta" );


			if (!isStessaFotoInCarrello(_carrello, riga)) {

				// Rileggo le associazioni in questo modo gli oggetti vengono riattaccati al context corrente.
				riga.fotografia = mioDbContext.Fotografie.Single( r => r.id == riga.fotografia.id );
				riga.fotografo = mioDbContext.Fotografi.Single( f => f.id == riga.fotografo.id );
				if( riga.formatoCarta != null )
					riga.formatoCarta = mioDbContext.FormatiCarta.Single( c => c.id == riga.formatoCarta.id );

				// Non so perché ma per gestire le associazioni identificanti, (e quindi il cascade dal master al child) occorre sfruttare un attributo con l'ID del padre.
				// In pratica la FK del figlio deve essere parte della PK (del figlio) quindi una chiave composta (che brutto).
				// Se non ci credi leggi qui:
				// http://jamesheppinstall.wordpress.com/2013/06/08/managing-parent-and-child-collection-relationships-in-entity-framework-what-is-an-identifying-relationship-anyway/
				riga.carrello = this.carrello;
//				riga.carrello_id = this.carrello.id;
				// Però se faccio cosi, non mi funziona piu l'aggiunta di righe ad un carrello esistente !!

				carrello.righeCarrello.Add( riga );

				isCarrelloModificato = true;

            } else {
				throw new ArgumentException( "La fotografia è già stata caricata nel carrello\r\nModificare la quantità\r\nRiga non aggiunta" );
			}	
		}

		internal void elimina( Carrello carrelloDacanc ) {

			if( ! carrelloDacanc.Equals( this.carrello ) ) {
				OrmUtil.forseAttacca<Carrello>( ref carrelloDacanc, mioDbContext );
			}

			if( carrelloDacanc.venduto )
				throw new InvalidOperationException( "Carrello venduto. Impossibile cancellare" );

			mioDbContext.Carrelli.Remove( carrelloDacanc );

			mioDbContext.SaveChanges();

			isCarrelloModificato = false;
		}

		public void abbandonaCarrello() {
			throw new NotImplementedException();
		}


		/**
		 * Salvo il carrello corrente (se era transiente, viene valorizzata la Guid della chiave primaria
		 * Se qualcosa va storto viene sollevata una eccezione.
		 */
		public void salvare() {

			if( carrello.venduto == true )
				throw new InvalidOperationException( "Il carrello attuale non è modificabile" );

			salvare( false );
		}

		public void vendere() {
			salvare( true );
		}

		public void salvare( bool vendere ) {

			carrello.venduto = vendere;

			// Sistemo un pò di campi ma NON quelli di chiave primaria!

			completaAttributiMancanti( true );

			if( ! isPossibileSalvare )
				throw new InvalidOperationException( "Impossibile salvare carrello : " + msgValidaCarrello() );

			// Sistemo tutti gli attributi di chiave transienti
			if( carrello.id == Guid.Empty )
				carrello.id = Guid.NewGuid();

			foreach(RigaCarrello rigaCarrello in carrello.righeCarrello){

				if( rigaCarrello.id == Guid.Empty )
					rigaCarrello.id = Guid.NewGuid();
				/* ERRATO !!!!
				if( rigaCarrello.carrello_id == Guid.Empty )
					rigaCarrello.carrello_id = carrello.id;           
				*/
            }

			foreach( IncassoFotografo incassoFotografo in carrello.incassiFotografi ) {
				if( incassoFotografo.id == Guid.Empty )
					incassoFotografo.id = Guid.NewGuid();
				/* ERRATO
				if( incassoFotografo.carrello_id == Guid.Empty )
					incassoFotografo.carrello_id = carrello.id;
				*/
			}

            // Se il carrello è nuovo, lo aggiungo al set.
            if (isStatoModifica == false)
                mioDbContext.Carrelli.Add(carrello);

			// Ora sistemo il totale a pagare. Lo valorizzo soltanto se è vuoto. 
			// Se l'utente ha valorizzato a mano il totale a pagare, lo lascio invariato.
			if( carrello.totaleAPagare == null)
				//Prima non avevo la gestione dei valori nulli.
				//carrello.totaleAPagare <= 0 )
				carrello.totaleAPagare = prezzoNettoTotale;

			try {

				int quanti = mioDbContext.SaveChanges();

				if( quanti <= 0 ) {
					string msg = "Il carrello non è stato aggiornato. Nessun record salvato";
					_giornale.Error( msg );
					throw new InvalidOperationException( msg );
				}


				isCarrelloModificato = false;

			} catch( DbEntityValidationException dbEx ) {

				// Rimetto a posto lo stato
				carrello.venduto = false;

				_giornale.Error( "Salvataggio carrello fallito. Entità non validata", dbEx );
				foreach( var validationErrors in dbEx.EntityValidationErrors ) {
					foreach( var validationError in validationErrors.ValidationErrors ) {
						_giornale.Debug( String.Format( "Property: {0} Error: {1}",
												validationError.PropertyName,
												validationError.ErrorMessage ) );
					}
				}
				throw dbEx;

			} catch( Exception ex ) {

				// Rimetto a posto lo stato
				carrello.venduto = false;

				_giornale.Error( "Salvataggio carrello fallito.", ex );
				throw ex;
			}

			string msg2 = "Registrato carrello id = " + carrello.id + " totale a pagare = " + carrello.totaleAPagare + " con " + carrello.righeCarrello.Count + " righe";
			_giornale.Info( msg2 );
		}

		/// <summary>
		/// Mi dice se la stessa foto è già nel carrello con lo stesso discriminator
		/// </summary>
		private static bool isStessaFotoInCarrello( Carrello carrello, RigaCarrello riga )
		{
			return isStessaFotoInCarrello(carrello, riga, riga.discriminator);
		}

        public static bool isStessaFotoInCarrello(Carrello carrello, RigaCarrello riga, string discriminator)
        {
            foreach (RigaCarrello r in carrello.righeCarrello)
            {
                if (r.fotografia.id == riga.fotografia.id && r.discriminator == discriminator)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Questo metodo pubblico per poter ricalcolare il totale durante la gestione del carrello.
        /// </summary>
        public void ricalcolaDocumento() {
			completaAttributiMancanti( false );
		}
		public void ricalcolaDocumento( bool ancheProvvigioni ) {
			completaAttributiMancanti( ancheProvvigioni );
		}

		private void completaAttributiMancanti( bool ancheProvvigioni ) {

			// Tempo di creazione o ultima modifica: lo aggiorno sempre.
			// In questo modo, nella gestione del selfservice, il carrello passerà in cima alla lista.
			carrello.tempo = DateTime.Now;

			// La giornata contabile la impongo sempre
			// anche perché se sto vendendo un carrello vecchio, deve andare a finire nella cassa di oggi.
			carrello.giornata = LumenApplication.Instance.stato.giornataLavorativa;

			if( ancheProvvigioni ) {
				// Gestico lo spaccato degli incassi per singolo fotografo
				if( carrello.incassiFotografi == null )
					carrello.incassiFotografi = new EntityCollection<IncassoFotografo>();
				else {
					// Svuoto per il ricalcolo
					foreach( IncassoFotografo inf in carrello.incassiFotografi ) {
						inf.incasso = 0;
						inf.incassoStampe = 0;
						inf.incassoMasterizzate = 0;
						inf.contaStampe = 0;
						inf.contaMasterizzate = 0;
						inf.provvigioni = null;
					}
				}
			}

			carrello.totMasterizzate = 0;

			// :: loop su tutte le righe
			foreach( RigaCarrello r in carrello.righeCarrello ) {

				// ricalcolo il valore della riga
				r.prezzoNettoTotale = calcValoreRiga( r );

				// Se ho venduto il carrello, valorizzo i fogli stampati con la quantità
				if( carrello.venduto ) {
					if( r.discriminator == RigaCarrello.TIPORIGA_STAMPA ) {
						r.totFogliStampati = r.quantita;
					}
					if( r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA ) {
						carrello.totMasterizzate += r.quantita; // Sarà sempre = 1 per forza;
					}
				}

				if( ancheProvvigioni ) {
					// Ora valorizzo lo spaccato provvigioni
					IncassoFotografo inca = carrello.incassiFotografi.SingleOrDefault( ii => ii.fotografo.id.Equals( r.fotografo.id ) );
					if( inca == null ) {
						inca = new IncassoFotografo();
						inca.id = Guid.Empty;  // lo lascio volutamente vuoto. Lo valorizzero soltanto un attimo prima di persisterlo.
						inca.carrello = carrello;
						inca.fotografo = r.fotografo;
						carrello.incassiFotografi.Add( inca );
					}


					if( r.discriminator == RigaCarrello.TIPORIGA_STAMPA ) {
						inca.incasso += r.prezzoNettoTotale;
						inca.incassoStampe += r.prezzoNettoTotale;
						inca.contaStampe += r.quantita;
					} else if( r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA ) {
						// Il prezzo di queste righe è zero. Calcolo tutto alla fine sul totale
						inca.contaMasterizzate += r.quantita;  // fisso = 1
					}
				}

				if( r.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA ) {
					carrello.totMasterizzate += r.quantita;
				}
			}

			if( ancheProvvigioni ) {

				// Elimino le provvigioni di eventuali fotografi che prima erano nel carrello ed ora non ci sono più.
				// Occhio questa espressione seguente, funziona solo grazie ad una estensione di Linq che è in questa classe: MyIEnumerableExtensions
				// Di natura, il compilatore mi darebbe errore (senza l'estensione)
				IEnumerable<Fotografo> fotografiBuoni = carrello.righeCarrello.Select( r => r.fotografo ).Distinct( f => f.id );
				//
				bool riprova;
				do {
					riprova = false;
					foreach( IncassoFotografo ii in carrello.incassiFotografi ) {
						if( !fotografiBuoni.Contains( ii.fotografo ) ) {
							carrello.incassiFotografi.Remove( ii );
							riprova = true;
							break;
						}
					}
				} while( riprova );


				// Devo sistemare l'incasso del dischetto diviso per quante foto sono state masterizzate per ogni fotografo
				if( carrello.prezzoDischetto != null && carrello.totMasterizzate > 0 ) {
					foreach( IncassoFotografo ii in carrello.incassiFotografi ) {
						decimal mioIncasso = ii.contaMasterizzate * (decimal)carrello.prezzoDischetto / carrello.totMasterizzate;
						ii.incasso += mioIncasso;
						ii.incassoMasterizzate = Math.Round( mioIncasso, 2 );
					}
				}
			}
		}

		/** 
		 * Calcolo il valore della riga
		 */
		public static decimal calcValoreRiga( RigaCarrello riga ) {

			decimal valore = 0;

			// Le righe masterizzate non le conteggio. Sono a valore 0
			if( riga.isTipoStampa ) {

				decimal _localSconto = riga.sconto != null ? (decimal)riga.sconto : 0;

				valore = riga.quantita * (riga.prezzoLordoUnitario - _localSconto);
			}

			return valore;
		}

		/// <summary>
		/// Uso tutti try-catch perché è importante che io arrivi a fare la dispose del DbContext,
		/// altrimenti mi rimangono le entità tracciate.
		/// </summary>
		public void Dispose() {

			// Se ho delle fotografie caricate, rilascio le immagini
			if( carrello != null && carrello.righeCarrello != null ) {
				foreach( RigaCarrello riga in carrello.righeCarrello )
					try {
						AiutanteFoto.disposeImmagini( riga.fotografia );
					} catch( Exception ) {
					}
			}

			// Se il carrello è stato modificato nel db o aggiunto al db ma non ancora committato, allora devo "tornare indietro"
			if( carrello != null && isCarrelloTransient == false ) {

				try {
					OrmUtil.rinuncioAlleModifiche( carrello, mioDbContext );
				} catch( Exception ) {
				}

				carrello = null;
			}

			isCarrelloModificato = false;

			// Distruggo anche il contesto. In questo modo riparto pulito per il prossimo carrello, ma sopattutto devo rilasciare le entità che sono "tracciate" da questo context
			this.mioDbContext.Dispose();
			this.mioDbContext = null;
		}

		/**
		 * Se una stampa è andata male, 
		 * tramite la riga che è fallita, ricarico il carrello relativo 
		 * e lo vado a stornare.
		 */
		public void stornoRiga( Guid idRigaCarrello ) {

			if( carrello != null )
				throw new InvalidOperationException( "Esiste già un carrello caricato" );

			
			using( new UnitOfWorkScope( true ) ) {

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

				var query = from c in dbContext.Carrelli.Include( "righeCarrello" )
							where c.righeCarrello.Any( r => r.id == idRigaCarrello )
							select c;

				carrello = query.SingleOrDefault();

				if( carrello == null )
					throw new ObjectNotFoundException( "La riga carrello con id = " + idRigaCarrello + " non è usabile" );

				// Carattere speciale che non c'è sulla tastiera per evitare cancellazioni fraudolente
				char marca = '\u0251';

				// Devo individuare qual'è la riga da modificare
				foreach( RigaCarrello r in carrello.righeCarrello ) {
					if( r.id == idRigaCarrello ) {

						short qtaPrec = r.quantita;
						decimal totRigaPrec = r.prezzoNettoTotale;

						r.quantita = 0;

						if( r.isTipoStampa ) {
							r.descrizione = marca + "Storno " + r.totFogliStampati + " fogli";
							r.totFogliStampati = 0;
                            Fotografia f = r.fotografia;
                            int contaStampa = (int)f.contaStampata + (int)r.quantita;
                            f.contaStampata = (short)(contaStampa > 0 ? contaStampa : 0);

                            ObjectContext objContext = ((IObjectContextAdapter)mioDbContext).ObjectContext;
                            objContext.ObjectStateManager.ChangeObjectState(f, EntityState.Modified);
                        }

						if( r.isTipoMasterizzata ) {
							r.descrizione = marca + "Storno foto masterizzate";
							r.quantita = 0;
						}

						completaAttributiMancanti( false );
						break;
					}
				}

				dbContext.SaveChanges();
			}
		}



		internal void stornoMasterizzate( Guid idCarrello, short totFotoOk, short totFotoErrate ) {
			
			_giornale.Debug( "Devo stornare " + totFotoErrate + " foto masterizzate dal carrello " + idCarrello + " perché qualcosa è andato storto" );

			if( carrello != null )
				throw new InvalidOperationException( "Esiste già un carrello caricato" );

			using( new UnitOfWorkScope( true ) ) {

				LumenEntities dbContext = UnitOfWorkScope.currentDbContext;

				var query = from c in dbContext.Carrelli.Include( "righeCarrello" )
							where c.id == idCarrello
							select c;

				carrello = query.SingleOrDefault();

				if( carrello == null )
					throw new ObjectNotFoundException( "Il carrello con id = " + idCarrello + " non è usabile" );

				// Carattere speciale che non c'è sulla tastiera per evitare cancellazioni fraudolente
				char marca = '\u0251';

				// Devo individuare qual'è la riga da modificare
				foreach( RigaCarrello r in carrello.righeCarrello ) {

					if( r.isTipoMasterizzata ) {
						// Se non ho masterizzato nulla, azzero il totale riga e poi abbasso il totale documento
						r.descrizione = marca + "Storno foto masterizzate";
						r.quantita = 0;
					}
				}
				completaAttributiMancanti( true );
				dbContext.SaveChanges();
			}
		}

		#endregion Metodi

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property on this object has a new value.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The property that has a new value.</param>
		protected virtual void OnPropertyChanged( string propertyName ) {
			
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if( handler != null ) {
				var e = new PropertyChangedEventArgs( propertyName );
				handler( this, e );
			}
		}

		#endregion // INotifyPropertyChanged Members


		internal void rimpiazzaFotoInRiga( Guid idFotoDaRefresh ) {
			RigaCarrello riga = carrello.righeCarrello.Where( r => r.fotografia.id == idFotoDaRefresh ).SingleOrDefault();
			if( riga != null ) {
				((IObjectContextAdapter)mioDbContext).ObjectContext.Refresh( RefreshMode.StoreWins, riga.fotografia );
			}

			isCarrelloModificato = true;
		}



		public bool possoClonareCarrello {
			get {
				return isStatoModifica;
            }
		}

		// TODO forse sarebbe più consono chiamare il clona su tutti i componenti ?? (carrello, righe, provvigioni)
		public void clonareCarrello() {

			if( possoClonareCarrello == false )
				throw new InvalidOperationException("nessun carrello caricato");

			Carrello c = new Carrello();
//			c.giornata = DateTime.Today;
//			c.tempo = DateTime.Now;
			c.intestazione = carrello.intestazione;
			c.note = carrello.note;
			c.prezzoDischetto = carrello.prezzoDischetto;
			c.totaleAPagare = carrello.totaleAPagare;
			c.totMasterizzate = carrello.totMasterizzate;
			c.visibileSelfService = carrello.visibileSelfService;

			c.righeCarrello = new List<RigaCarrello>();
			foreach( RigaCarrello r in carrello.righeCarrello ) {
				RigaCarrello r2 = new RigaCarrello();
				r2.carrello = c;
				r2.bordiBianchi = r.bordiBianchi;
				r2.descrizione = r.descrizione;
				r2.discriminator = r.discriminator;
				r2.formatoCarta = r.formatoCarta;
				r2.fotografia = r.fotografia;
				r2.fotografo = r.fotografo;
				r2.nomeStampante = r.nomeStampante;
				r2.prezzoLordoUnitario = r.prezzoLordoUnitario;
				r2.prezzoNettoTotale = r.prezzoNettoTotale;
				r2.quantita = r.quantita;
				r2.sconto = r.sconto;
				c.righeCarrello.Add( r2 );
			}
			
			mioObjContext.Detach(carrello);
			carrello = c;

			isStatoModifica = false;
			isCarrelloModificato = true;
		}

		public void spostaRigaCarrello( RigaCarrello rigaCarrello, bool remove ) {

			if( remove )
				carrello.righeCarrello.Remove( rigaCarrello );

			if( rigaCarrello.isTipoStampa ) {
				rigaCarrello.discriminator = RigaCarrello.TIPORIGA_MASTERIZZATA;
				rigaCarrello.quantita = 1;
				rigaCarrello.formatoCarta = null;
				rigaCarrello.totFogliStampati = 0;
				rigaCarrello.bordiBianchi = null;
				rigaCarrello.prezzoLordoUnitario = 0;
				rigaCarrello.prezzoNettoTotale = 0;
				rigaCarrello.nomeStampante = null;
				rigaCarrello.sconto = null;
			} else if( rigaCarrello.isTipoMasterizzata ) {
				//Quando sposto la riga setto di default i bordi bianchi a false
				rigaCarrello.bordiBianchi = false;
				rigaCarrello.discriminator = RigaCarrello.TIPORIGA_STAMPA;
			} else {
				_giornale.Warn( "Errore è stata spostata una riga senza dicriminator" );
			}
			aggiungiRiga( rigaCarrello );

			ricalcolaDocumento( true );
		}



		public static class Format {
			static string[] sizeSuffixes = {
		"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

			public static string ByteSize( long size ) {
				const string formatTemplate = "{0}{1:0.#} {2}";

				if( size == 0 ) {
					return string.Format( formatTemplate, null, 0, sizeSuffixes[0] );
				}

				var absSize = Math.Abs( (double)size );
				var fpPower = Math.Log( absSize, 1000 );
				var intPower = (int)fpPower;
				var iUnit = intPower >= sizeSuffixes.Length
					? sizeSuffixes.Length - 1
					: intPower;
				var normSize = absSize / Math.Pow( 1000, iUnit );

				return string.Format(
					formatTemplate,
					size < 0 ? "-" : null, normSize, sizeSuffixes[iUnit] );
			}
		}
	}
}
