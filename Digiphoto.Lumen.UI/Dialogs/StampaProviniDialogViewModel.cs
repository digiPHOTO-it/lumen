using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI.Dialogs
{
	public class StampaProviniDialogViewModel : ViewModelBase
	{

		public StampaProviniDialogViewModel()
		{
			caricaStampantiAbbinate();

			paramStampaProvini = new ParamStampaProvini();
			paramStampaProvini.PropertyChanged += ParamStampaProvini_PropertyChanged;
			stampaSoloSelezionate = true;

			paramStampaProvini.numeroColonne = Configurazione.UserConfigLumen.numColoneProvini;
			paramStampaProvini.numeroRighe = Configurazione.UserConfigLumen.numRigheProvini;
			paramStampaProvini.macchiaProvini = Configurazione.UserConfigLumen.macchiaProvini;

			ricreaMatriceEsempio();
		}

		private void ParamStampaProvini_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == "numeroRighe" || e.PropertyName == "numeroColonne")
				ricreaMatriceEsempio();
		}

		private void ricreaMatriceEsempio() {
			// Se la matrice contiene già il numero di elementi interessato, allora non la sto a ricreare
			if( matriceEsempio == null || matriceEsempio.Length != quanteFotoEsempio )
				matriceEsempio = new byte[quanteFotoEsempio];
		}

		private StampaProviniDialog stampaProviniDialog;

		public StampaProviniDialogViewModel(StampaProviniDialog stampaProviniDialog):this()
		{
			this.stampaProviniDialog = stampaProviniDialog;
		}

		#region Proprietà

		public IList<StampanteAbbinata> stampantiAbbinate
		{
			get;
			private set;
		}

		public ParamStampaProvini paramStampaProvini
		{
			get;
			set;
		}

		private bool _stampaTuttaLaGallery;
        public bool stampaTuttaLaGallery
		{
			get {
				return _stampaTuttaLaGallery;
			}
			set {
				_stampaTuttaLaGallery = value;
				ricreaMatriceEsempio();
            }
		}

		public bool possoModificareWaterMark {
			get {
#if DEBUG
				return true;
#else
				return ( Configurazione.isFuoriStandardCiccio ? false : true );
#endif
			}
		}

		public bool stampaSoloSelezionate
		{
			get
			{
				return !stampaTuttaLaGallery;
			}
			set
			{
				stampaTuttaLaGallery = !(value);
			}
		}

		public int _totoleFotoGallery;
        public int totoleFotoGallery
		{
			get {
				return _totoleFotoGallery;
            }
			set {
				_totoleFotoGallery = value;
				ricreaMatriceEsempio();
            }
		}

		private int _totaleFotoSelezionate;
        public int totaleFotoSelezionate
		{
			get {
				return _totaleFotoSelezionate;
            }
			set {
				_totaleFotoSelezionate = value;
				ricreaMatriceEsempio();
            }
		}


		private int quanteFotoEsempio {
			get {
				int tot = paramStampaProvini.numeroColonne * paramStampaProvini.numeroRighe;
				if (stampaTuttaLaGallery)
					return Math.Min( totoleFotoGallery, tot );
				else
					return Math.Min( totaleFotoSelezionate, tot );
			}
		}

		// Tengo una matrice giusto per bindare un esempio nel form
		private byte[] _matriceEsempio;
		public byte[] matriceEsempio {
			get {
				return _matriceEsempio;
			}
			private set {
				if (_matriceEsempio != value) {
					_matriceEsempio = value;
					OnPropertyChanged("matriceEsempio");
				}
			}
		}

		#endregion Proprietà

		#region Metodi

		/// <summary>
		/// Carico tutti i formati carta che sono abbinati alle stampanti installate
		/// Per ognuno di questi elementi, dovrò visualizzare un pulsante per la stampa
		/// </summary>
		private void caricaStampantiAbbinate()
		{

			string ss = Configurazione.UserConfigLumen.stampantiAbbinate;
			this.stampantiAbbinate = StampantiAbbinateUtil.deserializza(ss);
		}

		private void stampare(object objStampanteAbbinata)
		{
			bool procediPure = false;

			int quante = stampaSoloSelezionate ? totaleFotoSelezionate : totoleFotoGallery;

			int numFotPag = paramStampaProvini.numeroRighe * paramStampaProvini.numeroColonne;

			int numPag = numFotPag > 0 ? (int)Math.Ceiling((decimal)quante / numFotPag) : 1;

			StringBuilder msg = new StringBuilder();
			dialogProvider.ShowConfirmation(msg.AppendFormat("Confermi la stampa di {0} foto ?\n e N° {1} fogli di Provini?", quante, numPag).ToString(),
															"Richiesta conferma",
				  (confermato) =>
				  {
					  procediPure = confermato;
				  });

			if (procediPure)
			{
				StampanteAbbinata stampanteAbbinata = (StampanteAbbinata)objStampanteAbbinata;
				paramStampaProvini.formatoCarta = stampanteAbbinata.FormatoCarta;
				paramStampaProvini.nomeStampante = stampanteAbbinata.StampanteInstallata.NomeStampante;
				stampaProviniDialog.DialogResult = true;
				stampaProviniDialog.Hide();
			}
		}

		private void updateQuantitaColonneCommand(object param)
		{
			String type = (string)param;

			switch (type)
			{
				case "+":
					++paramStampaProvini.numeroColonne;
					break;
				case "-":
					// Se voglio una quantita uguale a 0 elimino la riga
					--paramStampaProvini.numeroColonne;
					if (paramStampaProvini.numeroColonne < 1)
					{
						paramStampaProvini.numeroColonne = 1;
					}
					break;
			}
		}

		private void updateQuantitaRigheCommand(object param)
		{
			String type = (string)param;

			switch (type)
			{
				case "+":
					++paramStampaProvini.numeroRighe;
					break;
				case "-":
					// Se voglio una quantita uguale a 0 elimino la riga
					--paramStampaProvini.numeroRighe;
					if (paramStampaProvini.numeroRighe < 1)
					{
						paramStampaProvini.numeroRighe = 1;
					}
					break;
			}
		}

		private void annulla()
		{
			stampaProviniDialog.Close();
		}

		#endregion


		private RelayCommand _stampareCommand;
		public ICommand stampareCommand
		{
			get
			{
				if (_stampareCommand == null)
				{
					_stampareCommand = new RelayCommand(param => stampare(param), param => true);
				}
				return _stampareCommand;
			}
		}

		private RelayCommand _updateQuantitaColonneCommand;
		public ICommand UpdateQuantitaColonneCommand
		{
			get
			{
				if (_updateQuantitaColonneCommand == null)
				{
					_updateQuantitaColonneCommand = new RelayCommand(param => updateQuantitaColonneCommand(param));
				}
				return _updateQuantitaColonneCommand;
			}
		}

		private RelayCommand _updateQuantitaRigheCommand;
		public ICommand UpdateQuantitaRigheCommand
		{
			get
			{
				if (_updateQuantitaRigheCommand == null)
				{
					_updateQuantitaRigheCommand = new RelayCommand(param => updateQuantitaRigheCommand(param));
				}
				return _updateQuantitaRigheCommand;
			}
		}

		private RelayCommand _annullaCommand;
		public ICommand annullaCommand
		{
			get
			{
				if (_annullaCommand == null)
				{
					_annullaCommand = new RelayCommand(param => this.annulla());
				}
				return _annullaCommand;
			}
		}


	}
}
