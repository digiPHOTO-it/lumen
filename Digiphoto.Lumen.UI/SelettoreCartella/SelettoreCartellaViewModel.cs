using System;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Applicazione;
using System.Collections.ObjectModel;
using System.IO;

namespace Digiphoto.Lumen.UI {

	public class SelettoreCartellaViewModel : ViewModelBase, IObserver<VolumeCambiatoMessaggio> {

		public SelettoreCartellaViewModel() {

			caricaElencodischiRimovibili();
		}

		private ObservableCollection<DriveInfo> _dischiRimovibili;
		public ObservableCollection<DriveInfo> dischiRimovibili {
			get {
				return _dischiRimovibili;
			}
			private set {
				_dischiRimovibili = value;
			}
		}

		private void caricaElencodischiRimovibili() {

			DriveInfo [] dischi = volumeCambiatoSrv.GetDrivesUsbAttivi();
			_dischiRimovibili = new ObservableCollection<DriveInfo>( dischi );
		}
	

		IVolumeCambiatoSrv volumeCambiatoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IVolumeCambiatoSrv>();
			}
		}

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( VolumeCambiatoMessaggio value ) {
			// Se è stata inserita una chiavetta usb, mi arriva il messaggio e mi serve per ricaricare la lista.
			caricaElencodischiRimovibili();
		}
	}
}
