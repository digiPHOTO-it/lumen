using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.VolumeCambiato;

namespace Digiphoto.Lumen.UI {

	internal class SelettoreCartellaViewModel : ViewModelBase {


		public IVolumeCambiatoSrv scaricatoreFotoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IScaricatoreFotoSrv>();
			}
		}

	}
}
