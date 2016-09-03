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
		public SelettoreMetadatiViewModel(SelettoreMetadati selettoreMetadatiView):this()
		{
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

        /// <summary>
        ///  Uso questa particolare collectionView perché voglio tenere traccia nel ViewModel degli N-elementi selezionati.
        ///  Sottolineo N perché non c'è supporto nativo per questo. Vedere README.txt nel package di questa classe.
        /// </summary>
        private MultiSelectCollectionView<Fotografia> _fotografieMCW;
        public MultiSelectCollectionView<Fotografia> fotografieMCW
        {
            get
            {
                return _fotografieMCW;
            }
            set
            {
                if (_fotografieMCW != value)
                {
                    _fotografieMCW = value;
                    OnPropertyChanged("fotografieMCW");
                }
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

					foreach (Fotografia fot in fotografieCW)
					{
						if (fot.evento != null)
						{
							//Serve a selezzionare l'evento dal menu rapido
							selettoreEventoMetadato.eventoSelezionato = fot.evento;
						}
					}

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

                DidascaliaEnabled = false;
                GiornataEnabled = false;
                EventoEnabled = false;

                return false;
			}
		}

		public bool possoApplicareMetadati
		{
			get
			{
				return isAlmenoUnaSelezionata &&
						(DidascaliaEnabled ||
						GiornataEnabled ||
						EventoEnabled);
			}
		}

		public bool possoEliminareMetadati
		{
			get
			{
				return isAlmenoUnaSelezionata &&
						(DidascaliaEnabled || 
						GiornataEnabled || 
						EventoEnabled);
			}
		}

        public bool isDidascaliaChecked
        {
            get
            {
                return metadati.isDidascaliaEnabled;
            }
            set
            {
                metadati.isDidascaliaEnabled = value;
            }
        }

        public bool isFasidelGiornoChecked
        {
            get
            {
                return metadati.isFaseDelGiornoEnabled;
            }
            set
            {
                metadati.isFaseDelGiornoEnabled = value;
            }
        }

        public bool isEventiChecked
        {
            get
            {
                return metadati.isEventoEnabled;
            }
            set
            {
                metadati.isEventoEnabled = value;
            }
        }

        #endregion Controlli

        #region Metodi

        void applicareMetadati()
		{

			// Ricavo l'Evento dall'apposito componente di selezione.
			// Tutti gli altri attributi sono bindati direttamente sulla struttura MetadatiFoto.
			metadati.evento = selettoreEventoMetadato.eventoSelezionato;
			if (this.fotografieCW.First<Fotografia>() != null)
			{
                metadati.didascalia = this.fotografieCW.First<Fotografia>().didascalia;
                if (this.fotografieCW.First<Fotografia>().faseDelGiorno!=null)
                {
                    metadati.faseDelGiorno = FaseDelGiornoUtil.getFaseDelGiorno((short)this.fotografieCW.First<Fotografia>().faseDelGiorno);
                }
			}

			if (fotoExplorerSrv.modificaMetadatiFotografie(fotografieCW, metadati))
			{
				dialogProvider.ShowMessage("Metadati Modificati correttamente","AVVISO");
			}
			else
			{
				dialogProvider.ShowError("Errore modifica metadati", "ERRORE",null);
			}

			MetadatiMsg msg = new MetadatiMsg(this);
			msg.fase = Fase.Completata;
			LumenApplication.Instance.bus.Publish(msg);

			// Svuoto ora i metadati per prossime elaborazioni
			metadati = new MetadatiFoto();
			selettoreEventoMetadato.eventoSelezionato = null;

			OnPropertyChanged("metadati");
            OnPropertyChanged("isDidascaliaChecked");
            OnPropertyChanged("isFasidelGiornoChecked");
            OnPropertyChanged("isEventiChecked");

            deselezionareTutto();
		}

		void eliminareMetadati()
		{
			bool procediPure = false;

            String metadatiToDelete = "";
            //Verifico quali metadati devono essere eliminati
            if (metadati.isDidascaliaEnabled)
            {
                metadati.didascalia = null;
                metadatiToDelete += "\nDidascalia";
            }

            if (metadati.isEventoEnabled)
            {
                metadati.evento = null;
                metadatiToDelete += "\nEvento";
            }

            if (metadati.isFaseDelGiornoEnabled)
            {
                metadati.faseDelGiorno = null;
                metadatiToDelete += "\nFase del Giorno";
            }

            dialogProvider.ShowConfirmation("Sei sicuro di voler eliminare i seguenti metadati"+ metadatiToDelete+"\ndelle " + fotografieCW.Count + " fotografie selezionate?", "Eliminazione metadati",
								  (confermato) =>
								  {
									  procediPure = confermato;
								  });

			if (!procediPure)
				return;

			if(fotoExplorerSrv.modificaMetadatiFotografie(fotografieCW, metadati))
			{
				dialogProvider.ShowMessage("Metadati Modificati correttamente", "AVVISO");
			}
            else
            {
                dialogProvider.ShowError("Errore modifica metadati", "ERRORE", null);
            }

            // Svuoto ora i metadati
            metadati = new MetadatiFoto();
            selettoreEventoMetadato.eventoSelezionato = null;
			//dialogProvider.ShowMessage("Eliminati i metadati delle " + selettoreMetadatiView.FotografiaCWP.SelectedItems.Count + " fotografie selezionate!", "Operazione eseguita");
			MetadatiMsg msg = new MetadatiMsg(this);
			msg.fase = Fase.Completata;
			LumenApplication.Instance.bus.Publish(msg);

            OnPropertyChanged("metadati");
            OnPropertyChanged("isDidascaliaChecked");
            OnPropertyChanged("isFasidelGiornoChecked");
            OnPropertyChanged("isEventiChecked");

            deselezionareTutto();
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
				if (fot.didascalia!=null)
					listDidascalie.Add(fot.didascalia);

				if (fot.faseDelGiornoString!=null)
					listGiornata.Add(fot.faseDelGiornoString);
				if (fot.evento!=null)
				{
					//Serve a selezzionare l'evento dal menu rapido
					//selettoreEventoMetadato.eventoSelezionato = fot.evento;
					listEvento.Add(fot.evento.descrizione);
				}
			}

            //Consento la modifica di tutto!!
            /*
			if (listDidascalie.Distinct().Count() > 1)
			{
				DidascaliaEnabled = false;
			}

			if (listGiornata.Distinct().Count() > 1)
			{
				GiornataEnabled = false;
			}

			if (listEvento.Distinct().Count() > 1)
			{
				EventoEnabled = false;
			}
			 */

            if (!isDidascaliaChecked)
            {
                DidascaliaEnabled = false;
            }

            if (!isFasidelGiornoChecked)
            {
                GiornataEnabled = false;
            }

            if (!isEventiChecked)
            {
                EventoEnabled = false;
            }
        }

		private void deselezionareTutto()
		{
			accendiSpegniTutto(false);
		}

		private void selezionareTutto()
		{
			accendiSpegniTutto(true);
		}

		/// <summary>
		/// Accendo o Spengo tutte le selezioni
		/// </summary>
		private void accendiSpegniTutto(bool selez)
		{
			if (fotografieMCW == null)
				return;

			if (selez)
				fotografieMCW.selezionaTutto();
			else
				fotografieMCW.deselezionaTutto();

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
