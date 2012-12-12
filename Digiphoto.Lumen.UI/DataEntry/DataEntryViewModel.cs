using System;
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

			entitySource.Source = passoCaricaDati();

			collectionView = (BindingListCollectionView)entitySource.View;

			status = DataEntryStatus.View;
		}

		#region Proprietà

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

			TEntity entita = (TEntity)collectionView.CurrentItem;

			// Eseguo!
			switch( nuovoStatus ) {

				case DataEntryStatus.View:
					if( collectionView.IsEditingItem && collectionView.CanCancelEdit )
						collectionView.CancelEdit();
					if( collectionView.IsAddingNew )
						collectionView.CancelNew();
					break;

				case DataEntryStatus.Edit:
					passoPreparaEdit( entita );
					collectionView.EditItem( entita );
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
					cancella( entita );
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

		protected virtual object passoCaricaDati() {
			return entityRepositorySrv.getAll();
		}

		protected virtual void passoPreparaEdit( TEntity entita ) {
			// A disposizione per override
		}


		void cancella( TEntity entita ) {

			bool prosegui = false;

			dialogProvider.ShowConfirmation( "L'elemento verrà cancellato in modo definitivo.\nConfermi la cancellazione ?", "Cancellazione",
				( sino ) => {
					prosegui = sino;
				} );

			if( prosegui ) {

				try {
					collectionView.Remove( entita ); // rimuovo dalla collection
					entityRepositorySrv.delete( entita );  // rimuovo dal database
					int quanti = entityRepositorySrv.saveChanges();
					if( quanti != 1 )
						dialogProvider.ShowError( "Cancellati " + quanti + " entità", "Attenzione", null );
				} catch( Exception ee ) {
					_giornale.Error( ErroriUtil.estraiMessage( ee ), ee );
					dialogProvider.ShowError( ErroriUtil.estraiMessage( ee ), "Cancellazione fallita", null );
					entityRepositorySrv.refresh( entita );  // Devo rileggere lo stato, altrimenti rimane unchanged e mi sampre errore
				}
			}
		}

		#endregion Metodi

	}


}
