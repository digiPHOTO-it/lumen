using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Config;
using System.Windows.Input;
using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Digiphoto.Lumen.Applicazione;
using System.Windows;

namespace Digiphoto.Lumen.UI.EliminaVecchiRullini
{
	public class EliminaVecchiRulliniViewModel : ViewModelBase
	{

		public EliminaVecchiRulliniViewModel()
		{
			this.cfg = Configurazione.UserConfigLumen;
		}

		#region Proprietà

		public UserConfigLumen cfg
		{
			get;
			set;
		}

		#endregion

		#region Servizi

		IEliminaFotoVecchieSrv eliminaFotoVecchieSrv
		{
			get
			{
				return LumenApplication.Instance.getServizioAvviato<IEliminaFotoVecchieSrv>();
			}
		}

		#endregion

		#region Metodi

		public void clean()
		{
			IList<String> listPathFoto = eliminaFotoVecchieSrv.getListaCartelleDaEliminare();

			if (listPathFoto.Count==0)
			{
				dialogProvider.ShowMessage("Non vi sono rullini da eliminare","Avviso");
				return;
			}

			foreach (String path in listPathFoto)
			{
				MessageBoxResult confermato = chiediConfermaEliminazionePath(path);
				if (confermato == MessageBoxResult.Cancel)
				{
					break;
				}
				else if (confermato == MessageBoxResult.Yes)
				{
					eliminaFotoVecchieSrv.elimina(path);
				}
			}
		}

		private void applica()
		{
			string errore = Configurazione.getMotivoErrore(cfg);

			if (errore != null)
			{
				dialogProvider.ShowError("Configurazione non valida.\nImpossibile salvare!\n\nMotivo errore:\n" + errore, "ERRORE", null);
			}
			else
			{
				UserConfigSerializer.serializeToFile(cfg);
				dialogProvider.ShowMessage("OK\nConfigurazione Salvata", "Avviso");
			}
		}

		private MessageBoxResult chiediConfermaEliminazionePath(String path)
		{

			StringBuilder msg = new StringBuilder("Confermare Eliminazione dell path :\n"+
													path + "\nL'operazione è irreversibile");
			MessageBoxResult procediPure = MessageBoxResult.Cancel;
			dialogProvider.ShowConfirmationAnnulla(msg.ToString(), "Richiesta conferma",
				(confermato) =>
				{
					procediPure = confermato;
				});

			return procediPure;
		}

		#endregion

		#region Controlli

		public bool abilitaClean
		{
			get
			{
				bool posso = true;

				if (posso && cfg.giorniDeleteFoto <= 0)
				{
					posso = false;
				}

				return posso;
			}
		}

		public bool abilitaApplica
		{
			get
			{
				bool posso = true;

				if (posso && cfg.giorniDeleteFoto <= 0)
				{
					posso = false;
				}

				return posso;
			}
		}

		#endregion

		#region Comandi

		private RelayCommand _cleanCommand;
		public ICommand cleanCommand
		{
			get
			{
				if (_cleanCommand == null)
				{
					_cleanCommand = new RelayCommand(param => clean(), param => abilitaClean, false);
				}
				return _cleanCommand;
			}
		}

		private RelayCommand _applicaCommand;
		public ICommand applicaCommand
		{
			get
			{
				if (_applicaCommand == null)
				{
					_applicaCommand = new RelayCommand(param => applica(), param => abilitaApplica, false);
				}
				return _applicaCommand;
			}
		}

		#endregion
	}
}
