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

			stampaSoloSelezionate = true;

			paramStampaProvini.numeroColonne = Configurazione.UserConfigLumen.numColoneProvini;
			paramStampaProvini.numeroRighe = Configurazione.UserConfigLumen.numRigheProvini;
			paramStampaProvini.macchiaProvini = Configurazione.UserConfigLumen.macchiaProvini;
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

		public bool stampaTuttaLaGallery
		{
			get;
			set;
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

		public int totoleFotoGallery
		{
			get;
			set;
		}

		public int totaleFotoSelezionate
		{
			get;
			set;
		}

		#endregion

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
					if (paramStampaProvini.numeroColonne < 0)
					{
						paramStampaProvini.numeroColonne = 0;
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
					if (paramStampaProvini.numeroRighe < 0)
					{
						paramStampaProvini.numeroRighe = 0;
					}
					break;
			}
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

	}
}
