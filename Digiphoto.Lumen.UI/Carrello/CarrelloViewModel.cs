using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using System.ComponentModel;
using System.Windows.Data;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using System.Windows.Forms;
using System.Drawing;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Vendere;
using System.Windows.Input;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Core.Database;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Servizi.Masterizzare;

namespace Digiphoto.Lumen.UI
{
    public class CarrelloViewModel : ViewModelBase, IObserver<MasterizzaMsg> 
    {
		private Digiphoto.Lumen.Servizi.Masterizzare.Fase StatoMasterizzazione = Fase.CopiaCompletata;

		// Mi indica se il carrello con qui sto lavorando è stato caricato dal db
		private bool isCarrelloCaricato = false;

        public CarrelloViewModel()
        {
            if (IsInDesignMode)
            {
            }
            else
			{
				IObservable<MasterizzaMsg> observable = LumenApplication.Instance.bus.Observe<MasterizzaMsg>();
				observable.Subscribe(this);

				Digiphoto.Lumen.Model.Carrello carrello = venditoreSrv.carrello;
				RigheCarrelloCv = CollectionViewSource.GetDefaultView(carrello.righeCarrello);
				OnPropertyChanged("RigheCarrelloCv");

				IntestazioneCarrello = carrello.intestazione;

				if (carrello.giornata == null || carrello.giornata.Equals(DateTime.MinValue))
					carrello.giornata = LumenApplication.Instance.stato.giornataLavorativa;

				GiornataCarrello = carrello.giornata;
				
				decimal prezzoTotaleIntero = 0;
				foreach(RiCaFotoStampata riCaFotoStampata in venditoreSrv.carrello.righeCarrello){
					prezzoTotaleIntero += riCaFotoStampata.prezzoNettoTotale;
				}

				PrezzoTotaleIntero = prezzoTotaleIntero;
				ScontoApplicato = "0%";

				IsControlVisible = "Hidden";
			}
		}

        #region Proprietà

		/// <summary>
		///  Uso questa particolare collectionView perché voglio tenere traccia nel ViewModel degli N-elementi selezionati.
		///  Sottolineo N perché non c'è supporto nativo per questo. Vedere README.txt nel package di questa classe.
		/// </summary>
		public ICollectionView RigheCarrelloCv
		{
			get;
			private set;
		}

		public RigaCarrello RigheCarrelloCvSelected
		{
			get;
			private set;
		}

		public ICollectionView Carrelli
		{
			get;
			private set;
		}

		public String _isControlVisible;
		public String IsControlVisible
		{
			get
			{
				return _isControlVisible;
			}
			set
			{
				_isControlVisible = value;
				OnPropertyChanged("IsControlVisible");
			}
		}

		private int _totaleFotoCarrelloMemorizzato;
		public int TotaleFotoCarrelloMemorizzato
		{
			get
			{
				return _totaleFotoCarrelloMemorizzato;
			}
			set
			{
				if (_totaleFotoCarrelloMemorizzato != value)
				{
					_totaleFotoCarrelloMemorizzato = value;
					OnPropertyChanged("TotaleFotoCarrelloMemorizzato");
				}
			}
		}

		private String _fotografoCarrelloMemorizzato;
		public String FotografoCarrelloMemorizzato
		{
			get
			{
				//Digiphoto.Lumen.Model.Carrello carrello = Carrelli.CurrentItem as Digiphoto.Lumen.Model.Carrello;
				//_fotografoAlbum = carrello.intestazione;
				return _fotografoCarrelloMemorizzato;
			}
			set
			{
				if (_fotografoCarrelloMemorizzato != value)
				{
					_fotografoCarrelloMemorizzato = value;
					OnPropertyChanged("FotografoCarrelloMemorizzato");
				}
			}
		}

		private String _nomeCarrelloMemorizzato;
		public String NomeCarrelloMemorizzato
		{
			get
			{
				return _nomeCarrelloMemorizzato;
			}
			set
			{
				if (_nomeCarrelloMemorizzato != value)
				{
					_nomeCarrelloMemorizzato = value;
					OnPropertyChanged("NomeCarrelloMemorizzato");
				}
			}
		}

		private decimal? _totalePrezzoCarrelloMemorizzato;
		public decimal? TotalePrezzoCarrelloMemorizzato
		{
			get
			{
				return _totalePrezzoCarrelloMemorizzato;
			}
			set
			{
				if (_totalePrezzoCarrelloMemorizzato != value)
				{
					_totalePrezzoCarrelloMemorizzato = value;
					OnPropertyChanged("TotalePrezzoCarrelloMemorizzato");
				}
			}
		}

		private IVenditoreSrv venditoreSrv
		{
			get
			{
				return (IVenditoreSrv)LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			}
		}

		private decimal _prezzoTotaleIntero;
		public decimal PrezzoTotaleIntero
		{
			get
			{
				return _prezzoTotaleIntero;
			}
			set
			{
				if (value != _prezzoTotaleIntero)
				{
					_prezzoTotaleIntero = value;
					OnPropertyChanged("PrezzoTotaleIntero");
				}
			}
		}

		private decimal _prezzoTotaleForfettario;
		public decimal PrezzoTotaleForfettario
		{
			get
			{
				return _prezzoTotaleForfettario;
			}
			set
			{
				if (value != _prezzoTotaleForfettario)
				{
					_prezzoTotaleForfettario = value;

					if (PrezzoTotaleIntero!=0)
					{
						ScontoApplicato = ((PrezzoTotaleIntero - _prezzoTotaleForfettario) / PrezzoTotaleIntero).ToString("P1");
					}
					OnPropertyChanged("PrezzoTotaleForfettario");
				}
			}
		}

		private String _masterizzazionePorgress;
		public String MasterizzazionePorgress
		{
			get
			{
				return _masterizzazionePorgress;
			}
			set
			{
				if (value != _masterizzazionePorgress)
				{
					_masterizzazionePorgress = value;
					OnPropertyChanged("MasterizzazionePorgress");
				}
			}
		}

		private string _scontoApplicato;
		public string ScontoApplicato
		{
			get
			{
				return _scontoApplicato;
			}
			set
			{
				if (value != _scontoApplicato)
				{
					_scontoApplicato = value;
					OnPropertyChanged("ScontoApplicato");
				}
			}
		}

		private string _intestazioneCarrello;
		public string IntestazioneCarrello
		{
			get
			{
				return _intestazioneCarrello;
			}
			set
			{
				if (value != _intestazioneCarrello)
				{
					_intestazioneCarrello = value;
					OnPropertyChanged("IntestazioneCarrello");
				}
			}
		}

		private DateTime _giornataCarrello;
		public DateTime GiornataCarrello
		{
			get
			{
				return _giornataCarrello;
			}
			set
			{
				if (value != _giornataCarrello)
				{
					_giornataCarrello = value;
					OnPropertyChanged("GiornataCarrello");
				}
			}
		}

		public bool abilitaOperazioniCarrello
		{
			get
			{
				if (IsInDesignMode)
					return true;

				bool posso = true;

				// Verifico che i dati minimi siano stati indicati
				if (posso && RigheCarrelloCv.IsEmpty)
					posso = false;

				return posso;
			}
		}

		public bool abilitaEliminaCarrello
		{
			get
			{
				if (IsInDesignMode)
					return true;

				bool posso = true;

				// Verifico che i dati minimi siano stati indicati
				// Elimino solo se il carrello è stato caricato in caso contrario è transiente e quindi
				//posso fare svuota
				if (posso && (RigheCarrelloCv.IsEmpty || !isCarrelloCaricato))
					posso = false;

				return posso;
			}
		}

		public BitmapSource StatusStatoMasterizzazioneImage
		{
			get
			{
				// Decido qual'è la giusta icona da caricare per mostrare lo stato dello slide show (Running, Pause, Empty)

				// Non so perchè ma se metto il percorso senza il pack, non funziona. boh eppure sono nello stesso assembly.
				string uriTemplate = @"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/##-16x16.png";

				Uri uri = null;

				if (StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.InizioCopia)
					uri = new Uri(uriTemplate.Replace("##", "ssBurn"));

				if (StatoMasterizzazione == Digiphoto.Lumen.Servizi.Masterizzare.Fase.CopiaCompletata)
					uri = new Uri(uriTemplate.Replace("##", "ssCompleate"));

				return new BitmapImage(uri);	
			}
		}

        #endregion

        #region Metodi

		public void updateGUI()
		{
			Digiphoto.Lumen.Model.Carrello carrello = venditoreSrv.carrello;
			RigheCarrelloCv = CollectionViewSource.GetDefaultView(carrello.righeCarrello);
			OnPropertyChanged("RigheCarrelloCv");

			foreach (RigaCarrello riga in carrello.righeCarrello)
			{
				if (riga.descrizione.Equals("Masterizzato Dischetto"))
				{
					IsControlVisible = "Visible";
				}
				else
				{
					IsControlVisible = "Hidden";
				}
			}
		}

        /// <summary>
        /// Devo mandare in stampa le foto selezionate
        /// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
        /// per la stampa: il formato carta e la stampante
        /// </summary>
        private void vendere()
        {
			venditoreSrv.carrello.totaleAPagare = PrezzoTotaleForfettario;
			venditoreSrv.carrello.intestazione = IntestazioneCarrello;
			venditoreSrv.carrello.giornata = GiornataCarrello;
			venditoreSrv.confermaCarrello();
			updateGUI();
        }

        private void calcolaTotali()
        {
			decimal prezzoTotaleIntero = 0;
			foreach (RigaCarrello rigaCarrello in venditoreSrv.carrello.righeCarrello)
			{
				//Verifico che la riga sia di tipo RiCaFotoStampata
				RiCaFotoStampata riCaFotoStampata = rigaCarrello as RiCaFotoStampata;
				if(riCaFotoStampata!=null){
					riCaFotoStampata.prezzoNettoTotale = riCaFotoStampata.quantita * riCaFotoStampata.prezzoLordoUnitario;
					prezzoTotaleIntero += riCaFotoStampata.prezzoNettoTotale;
				}
				
			}
			PrezzoTotaleIntero = prezzoTotaleIntero;
			PrezzoTotaleForfettario = prezzoTotaleIntero;
        }

		private void salvaCarrello()
		{
			venditoreSrv.carrello.totaleAPagare = PrezzoTotaleForfettario;
			venditoreSrv.carrello.intestazione = IntestazioneCarrello;
			venditoreSrv.carrello.giornata = GiornataCarrello;
			venditoreSrv.salvaCarrello();
			dialogProvider.ShowMessage("Carrello Salvato Correttamente", "Avviso");
			updateGUI();
		}

		private void eliminaCarrello()
		{
			Digiphoto.Lumen.Model.Carrello carrello = Carrelli.CurrentItem as Digiphoto.Lumen.Model.Carrello;			 
			venditoreSrv.removeCarrello(carrello);
			updateGUI();
		}

		private void eliminaRiga()
		{
			RiCaFotoStampata riCaFotoStampata = RigheCarrelloCv.CurrentItem as RiCaFotoStampata;
			//Testo il cast se è riuscito allora la riga è di tipo RiCaFotoStampata altrimenti e di tipo RiCaDiscoMasterizzato
			if (riCaFotoStampata != null)
			{
				venditoreSrv.removeRigaCarrello(RigheCarrelloCv.CurrentItem as RiCaFotoStampata);
			}
			else
			{
				venditoreSrv.removeRigaCarrello(RigheCarrelloCv.CurrentItem as RiCaDiscoMasterizzato);
			}
			updateGUI();
		}

		private void caricaCarrelloSelezionato()
		{
			Digiphoto.Lumen.Model.Carrello carrello = Carrelli.CurrentItem as Digiphoto.Lumen.Model.Carrello;			 
			RiCaFotoStampata ric = carrello.righeCarrello as RiCaFotoStampata;

			RigheCarrelloCv = CollectionViewSource.GetDefaultView(carrello.righeCarrello);
			OnPropertyChanged("RigheCarrelloCv");

			IntestazioneCarrello = carrello.intestazione;
			GiornataCarrello = carrello.giornata;
			isCarrelloCaricato = true;
			updateGUI();
		}

		private void creaNuovoCarrello()
		{
			venditoreSrv.creaNuovoCarrello();
			updateGUI();
		}

		private void svuotaCarrello()
		{
			venditoreSrv.abbandonaCarrello();
			updateGUI();
		}

		private void updateDatiCarrello()
		{
			Digiphoto.Lumen.Model.Carrello carrello = Carrelli.CurrentItem as Digiphoto.Lumen.Model.Carrello;
			TotaleFotoCarrelloMemorizzato = carrello.righeCarrello.Count;
			TotalePrezzoCarrelloMemorizzato = carrello.totaleAPagare;
			NomeCarrelloMemorizzato = carrello.intestazione;
			 
			RiCaFotoStampata ric = carrello.righeCarrello as RiCaFotoStampata;
			if(ric!=null){
				FotografoCarrelloMemorizzato = ric.fotografo.iniziali;
			}
			updateGUI();
		}

        #endregion

        #region Comandi

		private RelayCommand _updateGUICommand;
		public ICommand updateGUICommand
		{
			get
			{
				if (_updateGUICommand == null)
				{
					_updateGUICommand = new RelayCommand(param => updateGUI());
				}
				return _updateGUICommand;
			}
		}

		private RelayCommand _updateDatiCarrello;
		public ICommand UpdateDatiCarrello
		{
			get
			{
				if (_updateDatiCarrello == null)
				{
					_updateDatiCarrello = new RelayCommand(param => updateDatiCarrello());
				}
				return _updateDatiCarrello;
			}
		}

		private RelayCommand _creaNuovoCarrelloCommand;
		public ICommand creaNuovoCarrelloCommand
		{
			get
			{
				if (_creaNuovoCarrelloCommand == null)
				{
					_creaNuovoCarrelloCommand = new RelayCommand(param => creaNuovoCarrello(), param => abilitaOperazioniCarrello, false);
				}
				return _creaNuovoCarrelloCommand;
			}
		}

		private RelayCommand _eliminaCarrelloCommand;
		public ICommand eliminaCarrelloCommand
		{
			get
			{
				if (_eliminaCarrelloCommand == null)
				{
					_eliminaCarrelloCommand = new RelayCommand(param => eliminaCarrello(), param => abilitaEliminaCarrello, true);
				}
				return _eliminaCarrelloCommand;
			}
		}

		private RelayCommand _salvaCarrelloCommand;
		public ICommand salvaCarrelloCommand
		{
			get
			{
				if (_salvaCarrelloCommand == null)
				{
					// Non salvo alla fine perchè già salvo all'interno del metodo
					_salvaCarrelloCommand = new RelayCommand(param => salvaCarrello(), param => abilitaOperazioniCarrello, true);
				}
				return _salvaCarrelloCommand;
			}
		}

		private RelayCommand _svuotaCarrelloCommand;
		public ICommand svuotaCarrelloCommand
		{
			get
			{
				if (_svuotaCarrelloCommand == null)
				{
					_svuotaCarrelloCommand = new RelayCommand(param => svuotaCarrello(), param => abilitaOperazioniCarrello, true);
				}
				return _svuotaCarrelloCommand;
			}
		}

		private RelayCommand _eliminaRigaCommand;
		public ICommand eliminaRigaCommand
		{
			get
			{
				if (_eliminaRigaCommand == null)
				{
					_eliminaRigaCommand = new RelayCommand(param => eliminaRiga(), param => abilitaOperazioniCarrello, true);
				}
				return _eliminaRigaCommand;
			}
		}
		
		private RelayCommand _vendereCommand;
		public ICommand vendereCommand
		{
			get
			{
				if (_vendereCommand == null)
				{
					_vendereCommand = new RelayCommand(param => vendere(), param => abilitaOperazioniCarrello, false);
				}
				return _vendereCommand;
			}
		}

        private RelayCommand _calcolaTotali;
        public ICommand CalcolaTotali
        {
            get
            {
                if (_calcolaTotali == null)
                {
                    _calcolaTotali = new RelayCommand(param => calcolaTotali());
                }
                return _calcolaTotali;
            }
        }

        #endregion


		public void OnCompleted()
		{
			throw new NotImplementedException();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}

		public void OnNext(MasterizzaMsg msg)
		{
			/*
			System.Diagnostics.Trace.WriteLine("");
			System.Diagnostics.Trace.WriteLine("[TotFotoNonAggiunte]: " + msg.totFotoNonAggiunte);
			System.Diagnostics.Trace.WriteLine("[TotFotoAggiunte]: " + msg.totFotoAggiunte);
			System.Diagnostics.Trace.WriteLine("[Esito]: " + msg.esito);
			System.Diagnostics.Trace.WriteLine("[FotoAggiunta]: " + msg.fotoAggiunta);
			System.Diagnostics.Trace.WriteLine("[Fase]: " + msg.fase);
			System.Diagnostics.Trace.WriteLine("[Result]: " + msg.result);
			System.Diagnostics.Trace.WriteLine("[Progress]: " + msg.progress);
			 */ 

			StatoMasterizzazione = msg.fase;
			if (msg.result == null)
			{
				MasterizzazionePorgress = msg.progress + " %";
			}
			else
			{
				MasterizzazionePorgress = msg.result;
			}
			OnPropertyChanged("StatusStatoMasterizzazioneImage");
		}
	}
}
