using System;
using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using System.Windows.Input;
using System.Windows;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.Servizi.EntityRepository;

namespace Digiphoto.Lumen.UI {

	public abstract class SelettoreMetadatiViewModel : ViewModelBase {

		public SelettoreMetadatiViewModel() {

			// Questo è l'oggetto che contiene le proprietà che vado ad editare
			metadati = new MetadatiFoto();

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoViewModel = new SelettoreEventoViewModel();
		}


		#region Astratti

		protected abstract IEnumerable<Fotografia> getElementiSelezionati();

		protected abstract IEnumerator<Fotografia> getEnumeratorElementiSelezionati();

		public abstract int countFotografieSelezionate {
			get;
		}

		public abstract bool isAlmenoUnaFotoSelezionata {
			get;
		}

		#endregion Astratti


		#region Proprieta

		/// <summary>
		/// In questa proprietà ci sono i 3 valori che l'utente può modificare con anche i flag per dire queli sono interessati
		/// </summary>
		private MetadatiFoto _metadati;
		public MetadatiFoto metadati {
			get {
				return _metadati;
			}
			private set {
				if( _metadati != value ) {
					_metadati = value;
					OnPropertyChanged( "metadati" );
				}
			}
		}

		public FaseDelGiorno[] fasiDelGiorno {
			get {
				return FaseDelGiornoUtil.fasiDelGiorno;
			}
		}

		public SelettoreEventoViewModel selettoreEventoViewModel {
			get;
			private set;
		}

		private bool _faseDelgiornoEnabled;
		public bool FaseDelGiornoEnabled {
			get {
				return _faseDelgiornoEnabled;
			}

			set {
				if( _faseDelgiornoEnabled != value ) {
					_faseDelgiornoEnabled = value;
					OnPropertyChanged( "FaseDelGiornoEnabled" );
				}
			}
		}

		private bool _eventoEnabled;
		public bool EventoEnabled {
			get {
				return _eventoEnabled;
			}

			set {
				if( _eventoEnabled != value ) {
					_eventoEnabled = value;
					OnPropertyChanged( "EventoEnabled" );
				}
			}
		}

		IFotoExplorerSrv fotoExplorerSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}

		public bool possoCaricareStatoMetadati {
			get {
				return true;
			}
		}

		/// <summary>
		/// Se ho almeno una foto selezionata, ed ho scelto almeno una spunta per imporre il dato ...
		/// allora posso applicare.
		/// Amche se il dato da imporre è vuoto, va bene lo stesso, perché signigica che voglio eliminare la didascalia.
		/// </summary>
		public bool possoApplicareMetadati {
			get {
				return isAlmenoUnaFotoSelezionata
						&& metadati != null
						&& metadati.isAlmenoUnoUsato;
			}
		}

		public bool possoEliminareMetadati {
			get {
				return possoApplicareMetadati;
			}
		}

		#endregion Proprieta


		#region Metodi

		protected virtual void applicareMetadati() {

			// Questo attributo, non riesco a puntarlo direttamente nei metadati, ma risiede nel vm del suo componente.
			// Lo copio qui adesso.
			metadati.evento = selettoreEventoViewModel.eventoSelezionato;


			// Ricavo l'Evento dall'apposito componente di selezione.
			// Tutti gli altri attributi sono bindati direttamente sulla struttura MetadatiFoto.
			if( fotoExplorerSrv.modificaMetadatiFotografie( getElementiSelezionati(), metadati ) ) {
				dialogProvider.ShowMessage( "Metadati Modificati correttamente", "AVVISO" );
			} else {
				dialogProvider.ShowError( "Errore modifica metadati", "ERRORE", null );
			}

			MetadatiMsg msg = new MetadatiMsg( this );
			msg.fase = Fase.Completata;
			LumenApplication.Instance.bus.Publish( msg );

			// Svuoto ora i metadati per prossime elaborazioni
			metadati = new MetadatiFoto();
		}



		protected virtual void eliminareMetadati() {
			bool procediPure = false;

			String metadatiToDelete = "";
			//Verifico quali metadati devono essere eliminati
			if( metadati.usoDidascalia ) {
				metadati.didascalia = null;
				metadatiToDelete += "\nDidascalia";
			}

			if( metadati.usoEvento ) {
				metadati.evento = null;
				metadatiToDelete += "\nEvento";
			}

			if( metadati.usoFaseDelGiorno ) {
				metadati.faseDelGiorno = null;
				metadatiToDelete += "\nFase del Giorno";
			}

			dialogProvider.ShowConfirmation( "Sei sicuro di voler eliminare i seguenti metadati" + metadatiToDelete + "\ndelle " + countFotografieSelezionate + " fotografie selezionate?", "Eliminazione metadati",
								  ( confermato ) => {
									  procediPure = confermato;
								  } );

			if( !procediPure )
				return;

			if( fotoExplorerSrv.modificaMetadatiFotografie( getElementiSelezionati(), metadati ) ) {
				dialogProvider.ShowMessage( "Metadati Modificati correttamente", "AVVISO" );
			} else {
				dialogProvider.ShowError( "Errore modifica metadati", "ERRORE", null );
			}

			// Svuoto ora i metadati
			metadati = new MetadatiFoto();

			//dialogProvider.ShowMessage("Eliminati i metadati delle " + selettoreMetadatiView.FotografiaCWP.SelectedItems.Count + " fotografie selezionate!", "Operazione eseguita");
			MetadatiMsg msg = new MetadatiMsg( this );
			msg.fase = Fase.Completata;
			LumenApplication.Instance.bus.Publish( msg );
		}


		/// <summary>
		/// Carico lo stato dei metadati prendendolo dalle foto selezionate
		/// </summary>
		protected void caricareStatoMetadati() {

			bool didascalieDiscordanti = false;
			string didascaliaNew = null;

			bool eventiDiscordanti = false;
			Guid? eventoIdNew = null;  // Non posso usare l'oggetto "Evento" perché entity framework sulle entità non idratate mi torna null anche se non è vero :-(
			Evento eventoNew = null;

			bool fasiDelGiornoDiscordanti = false;
			short? faseDelGiornoNew = null;

			IEnumerator<Fotografia> itera = getEnumeratorElementiSelezionati();
			int conta = 0;
			while( itera.MoveNext() ) {

				Fotografia f = itera.Current;
				++conta;

				// -- didascalia

				if( String.IsNullOrWhiteSpace( f.didascalia ) ) {
					if( didascaliaNew != null )
						didascalieDiscordanti = true;
				} else {
					if( f.didascalia != didascaliaNew && conta > 1 )
						didascalieDiscordanti = true;
				}
				didascaliaNew = String.IsNullOrWhiteSpace( f.didascalia ) ? null : f.didascalia;

				// -- evento

				if( f.evento_id == null ) {
					if( eventoIdNew != null )
						eventiDiscordanti = true;
				} else {
					if( f.evento_id != eventoIdNew && conta > 1 )
						eventiDiscordanti = true;
				}
				eventoIdNew = f.evento_id;
				if( f.evento != null )
					eventoNew = f.evento;  // Se la foto è staccata, questo è null (mentre il suo ID è valorizzato)

				// -- fase del giorno

				if( f.faseDelGiorno == null ) {
					if( faseDelGiornoNew != null )
						fasiDelGiornoDiscordanti = true;
				} else {
					if( f.faseDelGiorno != faseDelGiornoNew && conta > 1 )
						fasiDelGiornoDiscordanti = true;
				}
				faseDelGiornoNew = f.faseDelGiorno;

			}


			// -- ora travaso i dati che sono concordanti
			MetadatiFoto metadatiNew = new MetadatiFoto();


			if( didascalieDiscordanti ) {
				metadatiNew.didascalia = null;
				metadatiNew.usoDidascalia = false;
			} else {
				metadatiNew.didascalia = String.IsNullOrWhiteSpace( didascaliaNew ) ? null : didascaliaNew;
				metadatiNew.usoDidascalia = (metadatiNew.didascalia != null);
			}

			if( eventiDiscordanti ) {
				metadatiNew.evento = null;
				metadatiNew.usoEvento = false;
			} else {
				if( eventoIdNew != null ) {
					if( eventoNew == null ) {
						// L'oggetto era staccato. Lo rileggo
						// Devo ricaricare l'oggetto evento dall'ID	
						IEntityRepositorySrv<Evento> repo = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Evento>>();
						metadatiNew.evento = repo.getById( eventoIdNew );
					} else
						metadatiNew.evento = eventoNew;
				}
				metadatiNew.usoEvento = (metadatiNew.evento != null);
			}
			selettoreEventoViewModel.eventoSelezionato = metadatiNew.evento;  // ribalto questo valore nel vm del componente perché è li che sta il valore buono

			if( fasiDelGiornoDiscordanti ) {
				metadatiNew.faseDelGiorno = null;
				metadatiNew.usoFaseDelGiorno = false;
			} else {
				if( faseDelGiornoNew != null )
					metadatiNew.faseDelGiorno = FaseDelGiornoUtil.getFaseDelGiorno( (short)faseDelGiornoNew );
				metadatiNew.usoFaseDelGiorno = (metadatiNew.faseDelGiorno != null);
			}

			this.metadati = metadatiNew;
		}

		#endregion Metodi

		#region Comandi

		private RelayCommand _applicareMetadatiCommand;
		public ICommand applicareMetadatiCommand {
			get {
				if( _applicareMetadatiCommand == null ) {
					_applicareMetadatiCommand = new RelayCommand( p => applicareMetadati(),
																  p => possoApplicareMetadati, false );
				}
				return _applicareMetadatiCommand;
			}
		}

		private RelayCommand _eliminareMetadatiCommand;
		public ICommand eliminareMetadatiCommand {
			get {
				if( _eliminareMetadatiCommand == null ) {
					_eliminareMetadatiCommand = new RelayCommand( p => eliminareMetadati(),
																  p => possoEliminareMetadati, false );
				}
				return _eliminareMetadatiCommand;
			}
		}

		private RelayCommand _caricareStatoMetadatiCommand;
		public ICommand caricareStatoMetadatiCommand {
			get {
				if( _caricareStatoMetadatiCommand == null ) {
					_caricareStatoMetadatiCommand = new RelayCommand( p => caricareStatoMetadati(),
																	  p => true, false );
				}
				return _caricareStatoMetadatiCommand;
			}
		}

		#endregion Comandi
	}
}