﻿using System;
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

namespace Digiphoto.Lumen.Servizi.Vendere {
	
	internal class GestoreCarrello : IDisposable {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( GestoreCarrello ) );

#region Propietà
		public Carrello carrello {
			get;
			private set;
		}

		public bool isTransient {
			
			get {
				bool trans = true;
				// Get ObjectStateEntry from EntityKey.
				IEntityWithKey ek = (IEntityWithKey)carrello;
				if( ek != null && ek.EntityKey != null ) {
					LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
					ObjectStateEntry stateEntry = dbContext.ObjectStateManager.GetObjectStateEntry( ek.EntityKey );
					EntityState stato = stateEntry.State;

					trans = ((int)stato <= 0);
				}
				return trans;
			}
				
		}

		public bool isPossibileSalvare {
			get {
				return  isCarrelloValido;
			}
		}

		public bool isCarrelloTransient {
			get {
				ObjectStateEntry s = UnitOfWorkScope.CurrentObjectContext.ObjectStateManager.GetObjectStateEntry( carrello );
				return s.State == EntityState.Added;
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
			else if( carrello.totaleAPagare == null )
				errore = "totale a pagare non indicato";
			else {
			}

			return errore;
		}


		public void abbandonaCarrello() {
			throw new NotImplementedException();
		}

		/**
		 * Salvo il carrello e ritorno l'id che ho attribuito
		 */
		public Guid salva() {

			completaAttributiMancanti();


			if( ! isPossibileSalvare )
				throw new InvalidOperationException( "Impossibile salvare carrello : " + msgValidaCarrello() );

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			Guid guidCarrello;

			if( isCarrelloTransient ) {
				carrello.giornata = LumenApplication.Instance.stato.giornataLavorativa;
				carrello.tempo = DateTime.Now;
				guidCarrello = Guid.NewGuid();
				dbContext.Carrelli.AddObject( carrello );

			} else {
				guidCarrello = carrello.id;
			}

			dbContext.SaveChanges();

			_giornale.Info( "Registrato carrello id = " + carrello.id + " totale a pagare = " + carrello.totaleAPagare );

			return guidCarrello;
		}

		private void completaAttributiMancanti() {

			if( isCarrelloTransient ) {

				// Giornata lavorativa
				if( carrello.giornata == null || carrello.giornata.Equals( DateTime.MinValue ) )
					carrello.giornata = LumenApplication.Instance.stato.giornataLavorativa;

				// Tempo di creazione
				if( carrello.tempo == null || carrello.tempo.Equals( DateTime.MinValue ) )
					carrello.tempo = DateTime.Now;

				// Id del carrello
				if( carrello.id == null || carrello.id.Equals( Guid.Empty ) )
					carrello.id = Guid.NewGuid();
			}


			// :: loop su tutte le righe
			decimal totaleAPagare = 0;
			foreach( RigaCarrello r in carrello.righeCarrello ) {

				// Id delle righe
				if( isCarrelloTransient ) 
					if( r.id == null || r.id.Equals( Guid.Empty ) )
						r.id = Guid.NewGuid();

				// ricalcolo il valore della riga
				r.prezzoNettoTotale = calcValoreRiga( r );

				// totalizzo il totale a pagare.
				totaleAPagare += r.prezzoNettoTotale;
			}

			if( carrello.totaleAPagare == null )
				carrello.totaleAPagare = totaleAPagare;



		}

		/** 
		 * Calcolo il valore della riga
		 */
		public static decimal calcValoreRiga( RigaCarrello riga ) {

			decimal _localSconto = riga.sconto != null ? (decimal)riga.sconto : 0;

			decimal valore = riga.quantita * (riga.prezzoLordoUnitario - _localSconto);
			return valore;
		}

		public void Dispose() {

			// Se il carrello è stato modificato nel db o aggiunto al db ma non ancora committato, allora devo "tornare indietro"
			if( carrello != null && isTransient == false ) {

				LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

				// Se il carrello non è stato salvato, allora torno indietro.
				ObjectStateEntry stateEntry = dbContext.ObjectStateManager.GetObjectStateEntry( ((IEntityWithKey)carrello).EntityKey );

				if( stateEntry.State == EntityState.Modified )
					dbContext.Refresh( RefreshMode.StoreWins, carrello );

				if( stateEntry.State == EntityState.Added )
					dbContext.Carrelli.DeleteObject( carrello );

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
					throw new ObjectNotFoundException( "La riga carrello con id = " + idRigaCarrello + " non è esiste" );

				// Carattere speciale che non c'è sulla tastiera per evitare cancellazioni fraudolente
				char marca = '\u0251';

				// Devo individuare qual'è la riga da modificare
				foreach( RigaCarrello r in carrello.righeCarrello ) {
					if( r.id == idRigaCarrello ) {

						short qtaPrec = r.quantita;
						decimal totRigaPrec = r.prezzoNettoTotale;

						r.quantita = 0;

						if( r is RiCaFotoStampata ) {
							RiCaFotoStampata rcfs = (RiCaFotoStampata)r;
							r.descrizione = marca + "Storno " + rcfs.totFogliStampati + " fogli";
							rcfs.totFogliStampati = 0;
						}

						if( r is RiCaDiscoMasterizzato ) {
							RiCaDiscoMasterizzato rcdm = (RiCaDiscoMasterizzato)r;
							r.descrizione = marca + "Storno " + rcdm.totFotoMasterizzate + " foto masterizzate";
							rcdm.totFotoMasterizzate = 0;
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
					throw new ObjectNotFoundException( "Il carrello con id = " + idCarrello + " non è esiste" );

				// Carattere speciale che non c'è sulla tastiera per evitare cancellazioni fraudolente
				char marca = '\u0251';

				// Devo individuare qual'è la riga da modificare
				foreach( RigaCarrello r in carrello.righeCarrello ) {

					short qtaPrec = r.quantita;
					decimal totRigaPrec = r.prezzoNettoTotale;

					if( r is RiCaDiscoMasterizzato ) {
						RiCaDiscoMasterizzato rcdm = (RiCaDiscoMasterizzato)r;
						r.descrizione = marca + "Storno " + rcdm.totFotoMasterizzate + " foto masterizzate";
						rcdm.totFotoMasterizzate -= totFotoErrate;
						if( rcdm.totFotoMasterizzate <= 0 ) {
							rcdm.totFotoMasterizzate = 0;
							
							// Se non ho masterizzato nulla, azzero il totale riga e poi abbasso il totale documento
							r.quantita = 0;

							carrello.totaleAPagare -= totRigaPrec;
							if( carrello.totaleAPagare < 0 )
								carrello.totaleAPagare = 0;

							completaAttributiMancanti();
						}
						dbContext.SaveChanges();
						break;
					}
				}
			}
		}
	}
}
