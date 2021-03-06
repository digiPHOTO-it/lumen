﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Applicazione;
using System.Windows.Data;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Eventi;
using System.ComponentModel;

namespace Digiphoto.Lumen.UI.DataEntry {


	public enum DataEntryStatus {

		View,   // sto visualizzando le entità esistenti
		New, // sto creando una nuova entità
		Edit,   // sto editanto l'entità corrente 
		Delete  // sto per cancellare l'entità corrente
	};

	/// <summary>
	/// Preso spunto da qui:
	/// http://msdn.microsoft.com/en-us/vstudio/dd776540.aspx
	/// poi riadattato al pattern mvvm e generalizzato per poterlo riusare.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class DataEntryViewModel<TEntity> : ViewModelBase where TEntity : class {


		public DataEntryViewModel() {

			entityRepositorySrv = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<TEntity>>();

			entitySource = new CollectionViewSource();

			IEnumerable<TEntity> dati = passoCaricaDati();

			var bindingList = new BindingList<TEntity>( dati.ToList() );

			entitySource.Source = bindingList;


			collectionView = (BindingListCollectionView)entitySource.View;

			status = DataEntryStatus.View;
		}

		#region Proprietà

		public TEntity entitaCorrente {
			get {
				return collectionView == null ? null : (TEntity)collectionView.CurrentItem;
			}
		}


		protected IEntityRepositorySrv<TEntity> entityRepositorySrv {
			get;
			private set;
		}

		public BindingListCollectionView collectionView {
			get;
			private set;
		}

		public CollectionViewSource entitySource {
			get;
			private set;
		}

		private DataEntryStatus _status;
		public DataEntryStatus status {
			get {
				return _status;
			}
			private set {
				if( _status != value ) {
					_status = value;
					OnPropertyChanged( "status" );
					OnPropertyChanged( "isKeyEditabile" ); // TODO si potrebbe fare una dipendenza tra property come questo esempio: http://www.codeproject.com/Articles/375192/WPF-The-calculated-property-dependency-problem nello step 4.
					OnPropertyChanged( "canEditFields" );
					OnPropertyChanged( "possoSalvare" );
					OnPropertyChanged( "isLovEnabled" );
				}
			}
		}

		public bool isKeyEditabile {
			get {
				return status == DataEntryStatus.New;
			}
		}

		/// <summary>
		/// Mi dice se posso editare i campi (cioè gli attributi)
		/// </summary>
		public bool canEditFields {
			get {
				return status == DataEntryStatus.Edit || status == DataEntryStatus.New;
			}
		}

		/// <summary>
		/// Mi dice se la Lista di Valori (cioè la datagrid con tutti i record)
		/// e' abilitata oppure no. Infatti quando sono in modifica di un record, 
		/// non posso spostarmi.
		/// </summary>
		public bool isLovEnabled {
			get {
				return !canEditFields;
			}
		}
		

		#endregion Proprietà

		#region Comandi

		private RelayCommand _commandSpostamento;
		public ICommand commandSpostamento {
			get {
				if( _commandSpostamento == null ) {
					_commandSpostamento = new RelayCommand( param => spostamento( param as string ),
															param => possoSpostamento( param as string ),
															false );
				}
				return _commandSpostamento;
			}
		}

		private RelayCommand _commandCambiareStatus;
		public ICommand commandCambiareStatus {
			get {
				if( _commandCambiareStatus == null ) {
					_commandCambiareStatus = new RelayCommand( param => cambiareStatus( param as string ),
															  param => possoCambiareStatus( param as string ),
															  false );
				}
				return _commandCambiareStatus;
			}
		}

		private RelayCommand _commandSalvare;
		public ICommand commandSalvare {
			get {
				if( _commandSalvare == null ) {
					_commandSalvare = new RelayCommand( param => salvare(),
														param => possoSalvare,
														false );
				}
				return _commandSalvare;
			}
		}

		#endregion

		#region Metodi

		private void spostamento( string p ) {

			switch( p ) {
				case "First":
					collectionView.MoveCurrentToFirst();
					break;
				case "Last":
					collectionView.MoveCurrentToLast();
					break;
				case "Next":
					collectionView.MoveCurrentToNext();
					break;
				case "Previous":
					if( collectionView.CurrentPosition > 0 )
						collectionView.MoveCurrentToPrevious();
					break;
			}

			OnPropertyChanged( "entitaCorrente" );
		}

		bool possoSpostamento( string dove ) {

			bool posso = true;

			switch( dove ) {

				case "Previous":
					if( collectionView.CurrentPosition <= 0 )
						posso = false;
					break;

				case "Next":
					if( collectionView.CurrentPosition >= collectionView.Count )
						posso = false;
					break;
			}

			return posso;
		}

		void cambiareStatus( string nomeNuovoStato ) {
			DataEntryStatus nuovoStatus = (DataEntryStatus)Enum.Parse( typeof( DataEntryStatus ), nomeNuovoStato );
			cambiareStatus( nuovoStatus );
		}

		void cambiareStatus( DataEntryStatus nuovoStatus ) {

			if( !possoCambiareStatus( nuovoStatus ) )
				throw new ArgumentException( "nuovo stato incompatibile: " + nuovoStatus );

			// Eseguo!
			switch( nuovoStatus ) {

				case DataEntryStatus.View:
					if( collectionView.IsEditingItem && collectionView.CanCancelEdit )
						collectionView.CancelEdit();
					if( collectionView.IsAddingNew ) {
						collectionView.CancelNew();
					}
					break;

				case DataEntryStatus.Edit:
					passoPreparaEdit( entitaCorrente );
					collectionView.EditItem( entitaCorrente );
					break;

				case DataEntryStatus.New:
					System.Diagnostics.Debug.Assert( collectionView.CanAddNew );
					TEntity nuova = (TEntity)collectionView.AddNew();
					passoPreparaAddNew( nuova );

					// Purtroppo i campi che sono a video non si rinfrescano con i valori di default.
					// Per ovviare, mi devo spostare e poi ritornare sul nuovo record.
					collectionView.MoveCurrentToFirst();
					collectionView.MoveCurrentTo( nuova );
					break;

				case DataEntryStatus.Delete:
					cancella( entitaCorrente );
					nuovoStatus = DataEntryStatus.View;
					break;
			}

			status = nuovoStatus;
		}

		bool possoCambiareStatus( string nomeNuovoStatus ) {
			DataEntryStatus nuovoStatus = (DataEntryStatus)Enum.Parse( typeof( DataEntryStatus ), nomeNuovoStatus );
			return possoCambiareStatus( nuovoStatus );
		}

		bool possoCambiareStatus( DataEntryStatus nuovoStatus ) {

			bool posso = false;

			if( nuovoStatus == status )
				return false;  // Che senso ha ?? esco subito.

			switch( nuovoStatus ) {

				case DataEntryStatus.New:
					posso = status == DataEntryStatus.View;
					break;

				case DataEntryStatus.Edit:
					posso = collectionView.CurrentItem != null && status == DataEntryStatus.View;
					break;

				case DataEntryStatus.Delete:
					posso = collectionView.CurrentItem != null && collectionView.CanRemove && status == DataEntryStatus.View;
					break;

				case DataEntryStatus.View:
					posso = true;
					break;
			}

			return posso;
		}

		void salvare() {

			if( !possoSalvare )
				throw new InvalidOperationException( "Impossibile salvare in questo momento" );

			TEntity entita = null;

			try {

				if( collectionView.IsAddingNew )
					collectionView.CommitNew();
				if( collectionView.IsEditingItem )
					collectionView.CommitEdit();

				entita = (TEntity)collectionView.CurrentItem;

				switch( status ) {

					case DataEntryStatus.New:
						entityRepositorySrv.addNew( entita );
						break;

					case DataEntryStatus.Edit:
						// TODO qui occorrerebbe gestire un controllo di concorrenza !!! Per ora salto
						// entityRepositorySrv.update( ref entita );
						break;

					case DataEntryStatus.Delete:
						// entityRepositorySrv.delete( entita );
						break;

					case DataEntryStatus.View:
						break;
				}

				passoPrimaDiSalvare( entita );

				entityRepositorySrv.saveChanges();

				_giornale.Debug( "salvata entità: " + entita );
				dialogProvider.ShowMessage( "Ok salvataggio riuscito\n" + entita, "Info" );

				passoDopoSalvato( entita );

				cambiareStatus( DataEntryStatus.View );

			} catch( Exception eee ) {
				_giornale.Error( "Salvataggio entità " + entita, eee );
				dialogProvider.ShowError( ErroriUtil.estraiMessage( eee ), "ERRORE", null );
			}

		}

		public bool possoSalvare {
			get {
				return status == DataEntryStatus.New || status == DataEntryStatus.Edit;
			}
		}

		protected virtual void passoPreparaAddNew( TEntity entita ) {
			// A disposizione per override
		}

		protected virtual void passoPrimaDiSalvare( TEntity entita ) {
			// A disposizione per override
		}

		protected virtual void passoDopoSalvato( TEntity entita ) {
			// A disposizione per override
		}

		protected virtual IEnumerable<TEntity> passoCaricaDati() {
			return entityRepositorySrv.getAll();
		}

		protected virtual void passoPreparaEdit( TEntity entita ) {
			// A disposizione per override
		}


		protected virtual bool cancella( TEntity entita ) {

			bool prosegui = false, esito = false;
			
			dialogProvider.ShowConfirmation( "L'elemento verrà cancellato in modo definitivo.\nConfermi la cancellazione ?", "Cancellazione",
				( sino ) => {
					prosegui = sino;
				} );

			if( prosegui ) {

				try {
					if( !collectionView.Contains( entita ) )
						throw new LumenException( "elemento da cancellare non trovato in lista" );

					// Prima di rimuovere dall'elenco devo spegnere l'elemento corrente
					// se non sposto il puntatore all'elemento corrente, non posso poi toglierlo dalla collection osservabile.
					if( collectionView.CurrentPosition > 0 )
						collectionView.MoveCurrentToPosition( 0 );
					else
						collectionView.MoveCurrentToNext();

					collectionView.Remove( entita );		// rimuovo dalla collection
					entityRepositorySrv.delete( entita );	// rimuovo dal database
					int quanti = entityRepositorySrv.saveChanges();

					if( quanti == 1 ) {
						esito = true;


						dialogProvider.ShowMessage( "L'elemento è stato cancellato correttamente.", "Cancellazione riuscita" );

					} else
						dialogProvider.ShowError( "Cancellati " + quanti + " entità", "Attenzione", null );

				} catch( Exception ee ) {
					_giornale.Error( ErroriUtil.estraiMessage( ee ), ee );
					dialogProvider.ShowError( ErroriUtil.estraiMessage( ee ), "Cancellazione fallita", null );
					entityRepositorySrv.refresh( entita );  // Devo rileggere lo stato, altrimenti rimane unchanged e mi da sampre errore
				}
			}
			return esito;
		}

		#endregion Metodi

	}


}
