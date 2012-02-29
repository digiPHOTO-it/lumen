
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media;
using Digiphoto.Lumen.Util;
using System.Windows.Media.Imaging;
using System;
namespace Digiphoto.Lumen.UI {

	class MainWindowViewModel : ClosableWiewModel {

		public MainWindowViewModel() {

            selettoreStampantiInstallateViewModel = new SelettoreStampantiInstallateViewModel();

            selettoreFormatoCartaViewModel = new SelettoreFormatoCartaViewModel();

            selettoreFormatoCartaAbbinatoViewModel = new SelettoreFormatoCartaAbbinatoViewModel();

        }

        public SelettoreStampantiInstallateViewModel selettoreStampantiInstallateViewModel
        {
            get;
            private set;
        }

        public SelettoreFormatoCartaViewModel selettoreFormatoCartaViewModel
        {
            get;
            private set;
        }

        public SelettoreFormatoCartaAbbinatoViewModel selettoreFormatoCartaAbbinatoViewModel
        {
            get;
            private set;
        }
	}
}
