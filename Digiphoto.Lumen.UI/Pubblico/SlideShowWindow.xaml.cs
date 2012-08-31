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
using log4net;
using System.Windows.Forms;
using Digiphoto.Lumen.UI.Pubblico.GestioneGeometria;
using System.Configuration;

namespace Digiphoto.Lumen.UI.Pubblico {
	/// <summary>
	/// Interaction logic for SlideShowWindow.xaml
	/// </summary>
	public partial class SlideShowWindow : Window {

		private static readonly ILog _giornale = LogManager.GetLogger(typeof(SlideShowWindow));

		private UserConfigLumen cfg = Configurazione.UserConfigLumen;

		SlideShowViewModel _slideShowViewModel;

		public SlideShowWindow() {

			InitializeComponent();

if( 1 == 1 ) {  // TODO
			// creo ed associo il datacontext
			_slideShowViewModel = new SlideShowViewModel();
			this.DataContext = _slideShowViewModel;

				// verifico che lo slideShow sia proiettabile in una zona visibile.
				gestisciPosizione();
}
		}

		protected override void OnClosed( EventArgs e ) {

			_slideShowViewModel.Dispose();
			base.OnClosed( e );
		}

		private void windowSlideShow_LocationChanged(object sender, EventArgs e)
		{	
			cfg.deviceEnum = WpfScreen.GetScreenFrom(this).deviceEnum;

			GestSlideShowViewModel.pSSG.deviceEnum = WpfScreen.GetScreenFrom(this).deviceEnum;
			WpfScreen scrn = WpfScreen.GetScreenFrom(cfg.deviceEnum);
			cfg.slideBoundsX = (int)scrn.DeviceBounds.Location.X;
			cfg.slideBoundsY = (int)scrn.DeviceBounds.Location.Y;
		}

		private void gestisciPosizione()
		{
			WpfScreen scrn = WpfScreen.GetScreenFrom(cfg.deviceEnum);

			this.WindowStartupLocation = WindowStartupLocation.Manual;

			_giornale.Debug(String.Format("SlideShow Info:\n DeviceName.{0}, \n workingArea={1}, \n displayResolution={2}, \n isPrimary={3}", scrn.DeviceName, scrn.WorkingArea, scrn.DeviceBounds, scrn.IsPrimary));

			bool salvaNuoviValori = false;

			if (cfg.deviceEnum != scrn.deviceEnum)
			{
				_giornale.Debug("Il monitor impostato non è stato trovato!!!");
				cfg.deviceEnum = 0;

				salvaNuoviValori = true;
			}

			if (!verificaProiettabile(scrn, cfg))
			{
				_giornale.Debug("I valori calcolati non sono ammissibili utilizzo quelli di default");

				Configurazione.creaGeometriaSlideShowSDefault(Configurazione.UserConfigLumen);
				cfg = Configurazione.UserConfigLumen;

				salvaNuoviValori = true;
			}

			if (cfg.screenHeight != (int)scrn.WorkingArea.Height || cfg.screenWidth != (int)scrn.WorkingArea.Width)
			{
				_giornale.Debug("Ricalcolo la geometria dello slideShow in base al nuovo monitor");
				_giornale.Debug("*** VALORI VECCHI ***");
				_giornale.Debug("deviceEnum: " + cfg.deviceEnum);
				_giornale.Debug("slideHeigth: " + cfg.slideHeight + " slideWidth: " + cfg.slideWidth);
				_giornale.Debug("screenHeight: " + cfg.screenHeight + " screenWidth: " + cfg.screenWidth);
				_giornale.Debug("slideTop: " + cfg.slideTop + " slideLeft: " + cfg.slideLeft);

				cfg.slideHeight = (int)(cfg.slideHeight * scrn.WorkingArea.Height / cfg.screenHeight);
				cfg.slideWidth = (int)(cfg.slideWidth * scrn.WorkingArea.Width / cfg.screenWidth);

				cfg.slideTop = cfg.slideTop <= 0 ? 0 : (int)(cfg.slideTop * scrn.WorkingArea.Height / cfg.screenHeight);
				cfg.slideLeft = cfg.slideLeft <= 0 ? 0 : (int)(cfg.slideLeft * scrn.WorkingArea.Width / cfg.screenWidth);

				cfg.screenHeight = (int)scrn.WorkingArea.Height;
				cfg.screenWidth = (int)scrn.WorkingArea.Width;

				_giornale.Debug("*** VALORI RICALCOLATI ***");
				_giornale.Debug("deviceEnum: " + 0);
				_giornale.Debug("slideHeigth: " + cfg.slideHeight + " slideWidth: " + cfg.slideWidth);
				_giornale.Debug("screenHeight: " + cfg.screenHeight + " screenWidth: " + cfg.screenWidth);
				_giornale.Debug("slideTop: " + cfg.slideTop + " slideLeft: " + cfg.slideLeft);

				salvaNuoviValori = true;
			}

			GestSlideShowViewModel.riposiziona();

			if (salvaNuoviValori)
			{
				_giornale.Debug("Devo salvare i nuovi valori ricalcolati");
				_giornale.Debug("Devo salvare la configurazione utente su file xml");
				UserConfigSerializer.serializeToFile(cfg);
				_giornale.Info("Salvata la configurazione utente su file xml");
			}
		}

		private bool verificaProiettabile(WpfScreen scr, UserConfigLumen cfg)
		{
			bool proiettabile = true;

			if(cfg.fullScreen)
			{
				return true;
			}

			if (proiettabile && cfg.slideLeft < 0)
			{
				proiettabile = false;
			}

			if (proiettabile && cfg.slideTop < 0)
			{
				proiettabile = false;
			}

			if (proiettabile && cfg.deviceEnum == 0 && (cfg.slideHeight + cfg.slideTop > scr.WorkingArea.Height || cfg.slideWidth + cfg.slideLeft > scr.WorkingArea.Width))
			{
				proiettabile = false;
			}

			if (proiettabile && cfg.deviceEnum >= 1 && (cfg.slideHeight + cfg.slideTop - cfg.slideBoundsY > scr.WorkingArea.Height || cfg.slideWidth + cfg.slideLeft - cfg.slideBoundsX > scr.WorkingArea.Width))
			{
				proiettabile = false;
			}

			return proiettabile;
		}
	}
}
