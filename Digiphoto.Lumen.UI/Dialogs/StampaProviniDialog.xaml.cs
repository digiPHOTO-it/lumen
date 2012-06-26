using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.UI.Mvvm;
using System.ComponentModel;

namespace Digiphoto.Lumen.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for StampaProviniDialog.xaml
	/// </summary>
	public partial class StampaProviniDialog : Window
	{

		public StampaProviniDialog()
		{
			InitializeComponent();

			DataContext = this;

			caricaStampantiAbbinate();

			paramStampaProvini = new ParamStampaProvini();

			stampaSoloSelezionate = true;

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
		private void caricaStampantiAbbinate() {

			string ss = Configurazione.UserConfigLumen.stampantiAbbinate;
			this.stampantiAbbinate = StampantiAbbinateUtil.deserializza( ss );
		}


		private void stampare(object objStampanteAbbinata)
		{
			StampanteAbbinata stampanteAbbinata = (StampanteAbbinata)objStampanteAbbinata;
			paramStampaProvini.formatoCarta = stampanteAbbinata.FormatoCarta;
			paramStampaProvini.nomeStampante = stampanteAbbinata.StampanteInstallata.NomeStampante;
			this.DialogResult = true;
			this.Hide();
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
