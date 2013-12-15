using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects.DataClasses;
using log4net;
using System.Transactions;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Applicazione;
using System.Data.Objects;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Windows.Forms;
using System.Data.Common;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.src.Database;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Servizi.Vendere {
	
	internal class GestoreCarrello : IDisposable {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( GestoreCarrello ) );

#region Propietà
		private Carrello _carrello;
		public Carrello carrello {
			get {
				return _carrello;
			}
			private set {
				_carrello = value;
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
				return  isCarrelloValido;
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

#endregion


		protected internal GestoreCarrello() {
		}

		public void creaNuovo() {
			carrello = new Carrello();
			carrello.righeCarrello = new EntityCollection<RigaCarrello>();
			//Metto un'intestazione automatica per distinguere il carrello autogenerato dagli altri
			// scarrello.intestazione = "Auto";
			isStatoModifica = false;

			GestoreCarrelloMsg msg = new GestoreCarrelloMsg(this);
			msg.fase = Digiphoto.Lumen.Servizi.Vendere.GestoreCarrelloMsg.Fase.CreatoNuovoCarrello;
			LumenApplication.Instance.bus.Publish(msg);
		}

		/// <summary>
		/// Carico da disco un carrello esistente
		/// </summary>
		/// <param name="idCarrello"></param>
		public void caricaCarrello( Guid idCarrello)
		{
			this.carrello = UnitOfWorkScope.CurrentObjectContext.Carrelli.Include( "righeCarrello" ).Single( r => r.id == idCarrello );

	//		Digiphoto.Lumen.Database.OrmUtil.vediEntitaInCache( UnitOfWorkScope, typeof( Carrello ) );
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
			else {
			}

			return errore;
		}

		public void aggiungiRiga( RigaCarrello riga ) {
			if (!rigaIsInCarrello(_carrello, riga))
			{
				// Prima di aggiungere la riga al carrello, provo a riattaccarlo. Non si sa mai.
				if( isStatoModifica )
				{
					Digiphoto.Lumen.Database.OrmUtil.forseAttacca<Carrello>( "Carrelli", ref _carrello );	
				}
				carrello.righeCarrello.Add( riga );
			}else
			{
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
			completaAttributiMancanti();
			
			if( ! isPossibileSalvare )
				throw new InvalidOperationException( "Impossibile salvare carrello : " + msgValidaCarrello() );

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;


			// Loop per tutte le righe



			if( isCarrelloTransient ) {
				carrello.id = Guid.NewGuid();

				foreach(RigaCarrello rigaCarrello in carrello.righeCarrello){
					rigaCarrello.id = Guid.NewGuid();
					Fotografia f = rigaCarrello.fotografia;
					OrmUtil.forseAttacca<Fotografia>( "Fotografie", ref f );
					FormatoCarta fc = rigaCarrello.formatoCarta;
					OrmUtil.forseAttacca<FormatoCarta>( "FormatiCarta", ref fc );
					Fotografo fo = rigaCarrello.fotografo;
					OrmUtil.forseAttacca<Fotografo>( "Fotografi", ref fo );
				}

				dbContext.Carrelli.Add( carrello );
			} else {

				// Sono in variazione. Riattacco il carrello e tutti i grafi interni
				Digiphoto.Lumen.Database.OrmUtil.forseAttacca<Carrello>( "Carrelli", ref _carrello );

				
				dbContext.ObjectContext.ObjectStateManager.ChangeObjectState( carrello, EntityState.Modified );

				foreach( RigaCarrello rc in carrello.righeCarrello ) {

					// Devo capire quali righe sono in modifica e quali sono state aggiunte
					ObjectStateEntry entry;
					bool esiste = dbContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry( rc, out entry );

					if( Guid.Empty.Equals( rc.id ) ) {
						// E' una riga nuova
						rc.id = Guid.NewGuid();
						if( esiste && entry.State != EntityState.Added )
							dbContext.ObjectContext.ObjectStateManager.ChangeObjectState( rc, EntityState.Added );
					} else {
						// la riga esiste. Forzo la modifica
						if( esiste && entry.State != EntityState.Modified )
							dbContext.ObjectContext.ObjectStateManager.ChangeObjectState( rc, EntityState.Modified );
					}
				}
			}

			string appo = CustomExtensions.ToTraceString( dbContext.ObjectContext );
			_giornale.Debug( appo );


			// Ora sistemo 


			int quanti = dbContext.SaveChanges();

			if( quanti <= 0 ) {
				string msg = "salvato carrello ma nessun record aggiornato. Possibile problema di EntityFramework";
				_giornale.Warn( msg );
				throw new InvalidOperationException( msg );
			}

			// non so perché ma se salvo un carrello di 1 riga mi dice che sono stati modificati 6 record
			if( quanti == carrello.righeCarrello.Count || quanti == carrello.righeCarrello.Count * 2 ) {
				// ok
			} else {
				string msg = "carrello id = " + carrello.id + " righe= " + carrello.righeCarrello.Count + " salvate=" + quanti;
				_giornale.Warn( msg );
			}

			string msg2 = "Registrato carrello id = " + carrello.id + " totale a pagare = " + carrello.totaleAPagare + " con " + carrello.righeCarrello.Count + " righe";
			_giornale.Info( msg2 );
		}

		private static bool rigaIsInCarrello(Carrello carrello, RigaCarrello riga)
		{
			return carrello.righeCarrello.Any( r => r.fotografia.id == riga.fotografia.id );
		}

		/// <summary>
		/// Sistemo qualcosa ma NON le chiavi primarie
		/// </summary>
		private void completaAttributiMancanti() {

			if( isCarrelloTransient ) {

				// Giornata lavorativa
				carrello.giornata = LumenApplication.Instance.stato.giornataLavorativa;

				// Tempo di creazione
				carrello.tempo = DateTime.Now;
			}


			decimal totaleAPagare = 0;

			if( carrello.prezzoDischetto != null )
				totaleAPagare += (decimal)carrello.prezzoDischetto;

			carrello.totMasterizzate = 0;

			// :: loop su tutte le righe
			foreach( RigaCarrello r in carrello.righeCarrello ) {

				// ricalcolo il valore della riga
				r.prezzoNettoTotale = calcValoreRiga( r );

				// totalizzo il totale a pagare.
				totaleAPagare += r.prezzoNettoTotale;

				// Se ho venduto il carrello, valorizzo i fogli stampati con la quantità
				if( carrello.venduto ) {
					if( r.discriminator == Carrello.TIPORIGA_STAMPA ) {
						r.totFogliStampati = r.quantita;
					}
					if( r.discriminator == Carrello.TIPORIGA_MASTERIZZATA ) {
						carrello.totMasterizzate += r.quantita; // Sarà sempre = 1 per forza;
					}
				}
			}

			carrello.totaleAPagare = totaleAPagare;
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

			// Se il carrello è stato modificato nel db o aggiunto al db ma non ancora committato, allora devo "tornare indietro"
			if( carrello != null && isCarrelloTransient == false ) {

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

				// Se il carrello non è stato salvato, allora torno indietro.
				if (carrello is IEntityWithKey)
				{
					ObjectStateEntry stateEntry = dbContext.ObjectContext.ObjectStateManager.GetObjectStateEntry( ((IEntityWithKey)carrello).EntityKey );

					if( stateEntry.State == EntityState.Modified )
						dbContext.ObjectContext.Refresh( RefreshMode.StoreWins, carrello );

					if( stateEntry.State == EntityState.Added )
						dbContext.Carrelli.Remove( carrello );
				}
				carrello = null;
			}
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

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

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

						// Abbasso il totale del carrello. Se per qualche motivo vado sotto zero, livello a zero.
						carrello.totaleAPagare -= totRigaPrec;
						if( carrello.totaleAPagare < 0 )
							carrello.totaleAPagare = 0;


						completaAttributiMancanti();
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

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

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

						completaAttributiMancanti();
					}
				}
				dbContext.SaveChanges();
			}
		}
	}
}
