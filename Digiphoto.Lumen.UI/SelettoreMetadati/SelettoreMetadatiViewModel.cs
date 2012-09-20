using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using System.Windows.Input;
using System.Windows;

namespace Digiphoto.Lumen.UI
{
	public class SelettoreMetadatiViewModel : ViewModelBase
	{

		private SelettoreMetadatiView selettoreMetadatiView = null;

		public SelettoreMetadatiViewModel(SelettoreMetadatiView selettoreMetadatiView):this()
		{
			this.selettoreMetadatiView = selettoreMetadatiView;
		}

		internal SelettoreMetadatiViewModel()
		{
			metadati = new MetadatiFoto();

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoMetadato = new SelettoreEventoViewModel();
		}

		#region Servizi

		IFotoExplorerSrv fotoExplorerSrv
		{
			get
			{
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}

		#endregion

		#region Proprieta


		public MultiSelectCollectionView<Fotografia> fotografieMCW
		{
			get
			{
				if(selettoreMetadatiView.MyItemsSource is MultiSelectCollectionView<Fotografia>)
				{
					return (MultiSelectCollectionView<Fotografia>)selettoreMetadatiView.MyItemsSource;
				}

				return null;
			}
		}

		public IList<Fotografia> fotografieCW
		{
			get
			{
				if (fotografieMCW != null)
				{
					return fotografieMCW.SelectedItems.ToList<Fotografia>();
				}

				return null;
			}
		}

		public MetadatiFoto metadati
		{
			get;
			private set;
		}

		public FaseDelGiorno[] fasiDelGiorno
		{
			get
			{
				return FaseDelGiornoUtil.fasiDelGiorno;
			}
		}

		public SelettoreEventoViewModel selettoreEventoMetadato
		{
			get;
			private set;
		}

		private bool _didascaliaEnabled;
		public bool DidascaliaEnabled
		{
			get
			{
				return _didascaliaEnabled;
			}

			set
			{
				if (_didascaliaEnabled!=value)
				{
					_didascaliaEnabled = value;
					OnPropertyChanged("DidascaliaEnabled");
				}
			}
		}

		private bool _giornataEnabled;
		public bool GiornataEnabled
		{
			get
			{
				return _giornataEnabled;
			}

			set
			{
				if (_giornataEnabled != value)
				{
					_giornataEnabled = value;
					OnPropertyChanged("GiornataEnabled");
				}
			}
		}

		private bool _eventoEnabled;
		public bool EventoEnabled
		{
			get
			{
				return _eventoEnabled;
			}

			set
			{
				if (_eventoEnabled != value)
				{
					_eventoEnabled = value;
					OnPropertyChanged("EventoEnabled");
				}
			}
		}

		#endregion

		#region Controlli

		public bool isAlmenoUnaSelezionata
		{
			get
			{
				if (fotografieCW != null &&
					fotografieCW.Count > 0)
				{
					controllaMetadati();
					return true;
				}
				return false;
			}
		}

		public bool possoApplicareMetadati
		{
			get
			{
				return isAlmenoUnaSelezionata &&
						DidascaliaEnabled &&
						GiornataEnabled &&
						EventoEnabled;
			}
		}

		public bool possoEliminareMetadati
		{
			get
			{
				return isAlmenoUnaSelezionata &&
						DidascaliaEnabled && 
						GiornataEnabled && 
						EventoEnabled;
			}
		}

		#endregion Controlli

		#region Metodi

		void applicareMetadati()
		{

			// Ricavo l'Evento dall'apposito componente di selezione.
			// Tutti gli altri attributi sono bindati direttamente sulla struttura MetadatiFoto.
			metadati.evento = selettoreEventoMetadato.eventoSelezionato;
			metadati.didascalia = selettoreMetadatiView.didascalia.Text;
			if (selettoreMetadatiView.fasiDelGiorno.SelectedItem != null)
			{
				metadati.faseDelGiorno = (FaseDelGiorno)selettoreMetadatiView.fasiDelGiorno.SelectedItem;
			}

			if (fotoExplorerSrv.modificaMetadatiFotografie(fotografieCW, metadati))
			{
				dialogProvider.ShowMessage("Metadati Modificati correttamente","AVVISO");
			}

			// Svuoto ora i metadati per prossime elaborazioni
			metadati = new MetadatiFoto();
			selettoreEventoMetadato.eventoSelezionato = null;
			OnPropertyChanged("metadati");
		}

		void eliminareMetadati()
		{
			bool procediPure = false;
			dialogProvider.ShowConfirmation("Sei sicuro di voler eliminare i metadati\ndelle " + fotografieCW.Count + " fotografie selezionate?", "Eliminazione metadati",
								  (confermato) =>
								  {
									  procediPure = confermato;
								  });

			if (!procediPure)
				return;

			if(fotoExplorerSrv.modificaMetadatiFotografie(fotografieCW, new MetadatiFoto()))
			{
				dialogProvider.ShowMessage("Metadati Modificati correttamente", "AVVISO");
			}

			// Svuoto ora i metadati
			metadati = new MetadatiFoto();
			selettoreEventoMetadato.eventoSelezionato = null;
			//dialogProvider.ShowMessage("Eliminati i metadati delle " + selettoreMetadatiView.FotografiaCWP.SelectedItems.Count + " fotografie selezionate!", "Operazione eseguita");
		}

		private void controllaMetadati()
		{
			DidascaliaEnabled = true;
			GiornataEnabled = true;
			EventoEnabled = true;

			List<string> listDidascalie = new List<string>();
			List<string> listGiornata = new List<string>();
			List<string> listEvento = new List<string>();

			foreach (Fotografia fot in fotografieCW)
			{
				listDidascalie.Add(fot.didascalia);
				listGiornata.Add(fot.faseDelGiornoString);
				if (fot.evento!=null)
				{
					listEvento.Add(fot.evento.descrizione);
				}
			}

			if (listDidascalie.Distinct().Count() != 1)
			{
				DidascaliaEnabled = false;
			}

			if (listGiornata.Distinct().Count() != 1)
			{
				GiornataEnabled = false;
			}

			if (listEvento.Distinct().Count() > 1)
			{
				EventoEnabled = false;
			}
		}

		#endregion

		#region Comandi

		private RelayCommand _applicareMetadatiCommand;
		public ICommand applicareMetadatiCommand
		{
			get
			{
				if (_applicareMetadatiCommand == null)
				{
					_applicareMetadatiCommand = new RelayCommand(p => applicareMetadati(),
																  p => possoApplicareMetadati, false);
				}
				return _applicareMetadatiCommand;
			}
		}

		private RelayCommand _eliminareMetadatiCommand;
		public ICommand eliminareMetadatiCommand
		{
			get
			{
				if (_eliminareMetadatiCommand == null)
				{
					_eliminareMetadatiCommand = new RelayCommand(p => eliminareMetadati(),
																  p => possoEliminareMetadati, false);
				}
				return _eliminareMetadatiCommand;
			}
		}

		#endregion Comandi
	}
}
