using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using  System.Data.Entity.Core.Objects.DataClasses;
using log4net;
using System.Transactions;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Applicazione;
using  System.Data.Entity.Core.Objects;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Windows.Forms;
using System.Data.Common;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Config;
using System.ComponentModel;
using System.Data.Entity;
using Digiphoto.Lumen.Util;
using System.Data.Entity.Core;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Vendere {

	internal class GestoreCarrello : IDisposable, INotifyPropertyChanged {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( GestoreCarrello ) );

		#region Costruttore
		protected internal GestoreCarrello() {
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

		public bool isEmpty
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
				return carrello.righeCarrello.Count( r => r.discriminator == Carrello.TIPORIGA_MASTERIZZATA );
			}
		}

		/// <summary>
		/// Mi dice quante foto da stampare ci sono nel carrello 
		/// </summary>
		public int sommatoriaQtaFotoDaStampare {
			get {
				return carrello.righeCarrello.Where( r => r.discriminator == Carrello.TIPORIGA_STAMPA ).Sum( rfs => rfs.quantita );
			}
		}

		public decimal sommatoraPrezziFotoDaStampare {
			get {
				return carrello.righeCarrello.Where( r => r.discriminator == Carrello.TIPORIGA_STAMPA ).Sum( rfs => rfs.prezzoNettoTotale );
			}
		}

		public string spazioFotoDaMasterizzate
		{
			get
			{
				long totalLength = 0;
				foreach (RigaCarrello riga in carrello.righeCarrello.Where(r => r.discriminator == Carrello.TIPORIGA_MASTERIZZATA))
                {
                    String filePath = (Configurazione.cartellaRepositoryFoto + Path.DirectorySeparatorChar + riga.fotografia.nomeFile);
					totalLength+= new FileInfo( filePath ).Length;
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
				return carrello.righeCarrello.Where( r => r.discriminator == Carrello.TIPORIGA_STAMPA ).Sum( r => r.prezzoNettoTotale ) + (carrello.prezzoDischetto != null ? (decimal)carrello.prezzoDischetto : (decimal)0);
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
			carrello.righeCarrello = new EntityCollection<RigaCarrello>();
			//Metto un'intestazione automatica per distinguere il carrello autogenerato dagli altri
			// scarrello.intestazione = "Auto";
			isStatoModifica = false;
		}

		/// <summary>
		/// Carico da disco un carrello esistente
		/// </summary>
		/// <param name="idCarrello"></param>
		public void caricaCarrello( Guid idCarrello )
		{
			creaMioDbContext();
			this.carrello = mioDbContext.Carrelli.Single( r => r.id == idCarrello );

			// Questo mi serve per caricare le associazioni, fisto che sto schifo di EF non è in grado di farlo da solo.
			mioObjContext.LoadProperty( this.carrello, c => c.righeCarrello );
			mioObjContext.LoadProperty( this.carrello, c => c.incassiFotografi );
			foreach( IncassoFotografo ff in carrello.incassiFotografi ) {
				mioObjContext.LoadProperty( ff, c => c.fotografo );
			}
			foreach( RigaCarrello rr in carrello.righeCarrello ) {
				mioObjContext.LoadProperty( rr, r => r.fotografia );
				if( rr.formatoCarta != null )
					mioObjContext.LoadProperty( rr, r => r.formatoCarta );
			}

			// Se il prezzo del carrello non è stato cambiato a mano, lo azzero in questo modo se lo risalverò, mi verrà aggiornato il totale nuovamente.
			if( prezzoNettoTotale == carrello.totaleAPagare )
				carrello.totaleAPagare = null;

			isStatoModifica = true;
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
			else if( carrello.prezzoDischetto == null && sommatoriaFotoDaMasterizzare > 0 ) {
				errore = "manca il prezzo del dischetto";
			}

			return errore;
		}

		public void removeRiga( RigaCarrello rigaDacanc ) {
				
			// Rimuovo l'elemento dalla collezione. Essendo una relazione identificante, EF si preoccua di rimuovere anche da disco la riga.
			bool test = carrello.righeCarrello.Remove( rigaDacanc );
			if( !test ) {
				_giornale.Error( "Si è cercato di cancellare una riga dal carrello che non esiste. Probabilmente la riga era già stata eliminata dal carrello in precedenza" );
				throw new LumenException( "La riga non è presente nel carrello" );
			}

		}

		public void aggiungiRiga( RigaCarrello riga ) {

			if( riga.fotografia == null )
				throw new ArgumentNullException( "nella RigaCarrello è obbligatoria la Fotografia" );
			if( riga.fotografo == null )
				throw new ArgumentNullException( "nella RigaCarrello è obbligatorio il Fotografo" );
			if( riga.discriminator == Carrello.TIPORIGA_STAMPA )
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
				riga.carrello_id = this.carrello.id;

				carrello.righeCarrello.Add( riga );

			} else {
				// TODO non è molto mvvm. Da migliorare.
				MessageBox.Show("La fotografia è già stata caricata nel carrello\nModificare la quantita","Avviso");
			}	
		}

		public void abbandonaCarrello() {
			throw new NotImplementedException();
		}


		/**
		 * Salvo il carrello corrente (se era transiente, viene valorizzata la Guid della chiave primaria
		 * Se qualcosa va storto viene sollevata una eccezione.
		 */
		public void salva() {

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
				if( rigaCarrello.carrello_id == Guid.Empty )
					rigaCarrello.carrello_id = carrello.id;
			}

			foreach( IncassoFotografo incassoFotografo in carrello.incassiFotografi ) {
				if( incassoFotografo.id == Guid.Empty )
					incassoFotografo.id = Guid.NewGuid();
				if( incassoFotografo.carrello_id == Guid.Empty )
					incassoFotografo.carrello_id = carrello.id;
			}

			// Se il carrello è nuovo, lo aggiungo al set.
			if( isStatoModifica == false )
				mioDbContext.Carrelli.Add( carrello );

			// Ora sistemo il totale a pagare. Lo valorizzo soltanto se è vuoto. 
			// Se l'utente ha valorizzato a mano il totale a pagare, lo lascio invariato.
			if( carrello.totaleAPagare == null)
				//Prima non avevo la gestione dei valori nulli.
				//carrello.totaleAPagare <= 0 )
				carrello.totaleAPagare = prezzoNettoTotale;

			int quanti = mioDbContext.SaveChanges();

			if( quanti <= 0 ) {
				string msg = "Il carrello non è stato aggiornato. Nessun record salvato";
				_giornale.Error( msg );
				throw new InvalidOperationException( msg );
			}

			string msg2 = "Registrato carrello id = " + carrello.id + " totale a pagare = " + carrello.totaleAPagare + " con " + carrello.righeCarrello.Count + " righe";
			_giornale.Info( msg2 );
		}

		/// <summary>
		/// Mi dice se la stessa foto è già nel carrello con lo stesso discriminator
		/// </summary>
		private static bool isStessaFotoInCarrello( Carrello carrello, RigaCarrello riga )
		{
			foreach( RigaCarrello r in carrello.righeCarrello ) {
				if( r.fotografia.id == riga.fotografia.id && r.discriminator == riga.discriminator )
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

			if( isCarrelloTransient ) {

				// Giornata lavorativa
				carrello.giornata = LumenApplication.Instance.stato.giornataLavorativa;

				// Tempo di creazione
				carrello.tempo = DateTime.Now;
			}

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
					if( r.discriminator == Carrello.TIPORIGA_STAMPA ) {
						r.totFogliStampati = r.quantita;
					}
					if( r.discriminator == Carrello.TIPORIGA_MASTERIZZATA ) {
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


					if( r.discriminator == Carrello.TIPORIGA_STAMPA ) {
						inca.incasso += r.prezzoNettoTotale;
						inca.incassoStampe += r.prezzoNettoTotale;
						inca.contaStampe += r.quantita;
					} else if( r.discriminator == Carrello.TIPORIGA_MASTERIZZATA ) {
						// Il prezzo di queste righe è zero. Calcolo tutto alla fine sul totale
						inca.contaMasterizzate += r.quantita;  // fisso = 1
					}
				}

				if( r.discriminator == Carrello.TIPORIGA_MASTERIZZATA ) {
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
			if( riga.discriminator == Carrello.TIPORIGA_STAMPA ) {

				decimal _localSconto = riga.sconto != null ? (decimal)riga.sconto : 0;

				valore = riga.quantita * (riga.prezzoLordoUnitario - _localSconto);
			}

			return valore;
		}

		public void Dispose() {

			// Se ho delle fotografie caricate, rilascio le immagini
			if( carrello != null && carrello.righeCarrello != null ) {
				foreach( RigaCarrello riga in carrello.righeCarrello )
					AiutanteFoto.disposeImmagini( riga.fotografia );
			}

			// Se il carrello è stato modificato nel db o aggiunto al db ma non ancora committato, allora devo "tornare indietro"
			if( carrello != null && isCarrelloTransient == false ) {

				OrmUtil.rinuncioAlleModifiche( carrello, mioDbContext );

				carrello = null;
			}



			// Distruggo anche il contesto. In questo modo riparto pulito per il prossimo carrello.
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

						if( r.discriminator == Carrello.TIPORIGA_STAMPA ) {
							r.descrizione = marca + "Storno " + r.totFogliStampati + " fogli";
							r.totFogliStampati = 0;
						}

						if( r.discriminator == Carrello.TIPORIGA_MASTERIZZATA ) {
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

					if( r.discriminator == Carrello.TIPORIGA_MASTERIZZATA ) {
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
		}
	}

	public static class Format
	{
		static string[] sizeSuffixes = {
        "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		public static string ByteSize(long size)
		{
			const string formatTemplate = "{0}{1:0.#} {2}";

			if (size == 0)
			{
				return string.Format(formatTemplate, null, 0, sizeSuffixes[0]);
			}

			var absSize = Math.Abs((double)size);
			var fpPower = Math.Log(absSize, 1000);
			var intPower = (int)fpPower;
			var iUnit = intPower >= sizeSuffixes.Length
				? sizeSuffixes.Length - 1
				: intPower;
			var normSize = absSize / Math.Pow(1000, iUnit);

			return string.Format(
				formatTemplate,
				size < 0 ? "-" : null, normSize, sizeSuffixes[iUnit]);
		}
	}
}
