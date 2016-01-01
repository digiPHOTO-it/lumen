using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI.Dialogs.SelezionaStampante
{
	public class SelezionaStampanteDialogViewModel : ViewModelBase
	{

		public SelezionaStampanteDialogViewModel()
		{
			caricaStampantiAbbinate();

		}

		private SelezionaStampanteDialog _selezionaStampanteDialog;

		public SelezionaStampanteDialogViewModel(SelezionaStampanteDialog selezionaStampanteDialog): this()
		{
			this._selezionaStampanteDialog = selezionaStampanteDialog;
		}

		#region Proprietà

		public IList<StampanteAbbinata> stampantiAbbinate
		{
			get;
			private set;
		}

		public FormatoCarta formatoCarta
		{
			get;
			private set;
		}

		public string nomeStampante {
			get;
			private set;
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
			StampanteAbbinata stampanteAbbinata = (StampanteAbbinata)objStampanteAbbinata;
			formatoCarta = stampanteAbbinata.FormatoCarta;
			nomeStampante = stampanteAbbinata.StampanteInstallata.NomeStampante;
			_selezionaStampanteDialog.DialogResult = true;
			_selezionaStampanteDialog.Hide();
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
	}
}
