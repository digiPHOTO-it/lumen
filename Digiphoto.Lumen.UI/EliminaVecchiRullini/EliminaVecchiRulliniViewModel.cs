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
		}

		#region Proprietà

		public string testoEsplicativo {
			get {

				if( abilitaClean ) {
					return "La ricerca delle foto vecchie da cancellare si fermerà al giorno : " + eliminaFotoVecchieSrv.giornoFineAnalisi.ToString( "dd/MM/yyyy" ) + ".\r\nE' possibile poi confermare singolarmente le giornate da eliminare.\r\nPremere il pulsante per iniziare la ricerca";
				} else {
					return "Avviso:\r\nLa cancellazione delle foto vecchie non è stata prevista nella configurazione.\r\nPer abilitare questa funzionalità lanciare il programma 'Gestore Configurazione'\r\ne impostare il parametro : 'N° giorni elimina foto'.";
				}
			}
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

		private MessageBoxResult chiediConfermaEliminazionePath(String path)
		{

			StringBuilder msg = new StringBuilder("Confermi la cancellazione di tutte le foto del giorno :\r\n"+
													path + "\r\nL'operazione è irreversibile.\r\nLe foto eliminate non potranno più essere recuperate");
			MessageBoxResult procediPure = MessageBoxResult.Cancel;
			dialogProvider.ShowConfirmationAnnulla(msg.ToString(), "Eliminazione foto",
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
				return IsInDesignMode ? true : Configurazione.infoFissa.numGiorniEliminaFoto > 0;
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
					_cleanCommand = new RelayCommand(param => clean(), param => abilitaClean ); // La unit-of-work la gestisco dentro.
				}
				return _cleanCommand;
			}
		}

		#endregion
	}
}
