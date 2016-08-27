using System;
using System.Windows;
using Digiphoto.Lumen.Config;
using log4net;
using System.Windows.Forms;
using Digiphoto.Lumen.UI.Pubblico.GestioneGeometria;

namespace Digiphoto.Lumen.UI.Pubblico {
	/// <summary>
	/// Interaction logic for SlideShowWindow.xaml
	/// </summary>
	public partial class SlideShowWindow : Window {

		private static readonly ILog _giornale = LogManager.GetLogger(typeof(SlideShowWindow));

		/// <summary>
		/// Questa proprieta mi dice se la finestra è stata spostata o ridimensionata.
		/// Mi servirà in chiusura per memorizzare la posizione nella geometria corrente
		/// (non quella della configurazione ma quella corrente)
		/// </summary>
		public bool posizionamentoInCorso {
			get; set;
		}
		
		private bool spostata { 
			get; set; 
		}

		public SlideShowWindow() {

			InitializeComponent();

			// Titolo della finestra
			if( String.IsNullOrEmpty( Configurazione.infoFissa.descrizPuntoVendita ) )
				this.Title = "Slide Show - digiPHOTO Lumen";
			else
				this.Title = "Slide Show - " + Configurazione.infoFissa.descrizPuntoVendita;


			// creo ed associo il datacontext 
			this.DataContext = new SlideShowViewModel();

			// Eventi di spostameto e ridimensionamento della finestra
			LocationChanged += windowSlideShow_LocationChanged;
			SizeChanged += windowSlideShow_SizeChanged;

			// Evento di chiusura innescato dal viewmodel
			EventHandler handler = null;
			handler = delegate {
				_slideShowViewModel.RequestClose -= handler;
		
				this.Close();
				
				this.DataContext = null;
			};

			_slideShowViewModel.RequestClose += handler;

			spostata = false;
        }

		#region Proprieta

		private SlideShowViewModel _slideShowViewModel {
			get {
				return (SlideShowViewModel)this.DataContext;
			}
		}

		#endregion Proprieta

		#region Eventi

		protected override void OnClosed( EventArgs e ) {

			if( _slideShowViewModel != null ) {
				_slideShowViewModel.Dispose();
				DataContext = null;
			}



			base.OnClosed( e );
		}
		
		private void windowSlideShow_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {

			// Rimuovo listener per pulizia
			LocationChanged -= windowSlideShow_LocationChanged;
			SizeChanged -= windowSlideShow_SizeChanged;

			// evito di incappare in loop di ridimensionamenti di chiusura finestra
			this.posizionamentoInCorso = true;

			// Memorizzo la geometria per la prossima apertura
			if( spostata ) {
				_slideShowViewModel.memorizzarePosizioneFinestra();
				spostata = false;
			}

			// Se mi hanno premuto X per chiudere la finestra, fermo lo show
			if( _slideShowViewModel != null ) {
				_slideShowViewModel.stop();
				_slideShowViewModel.Dispose();
				DataContext = null;
			}
		}
		

		private void windowSlideShow_SizeChanged( object sender, SizeChangedEventArgs e ) {
			if( IsLoaded && posizionamentoInCorso == false && DataContext != null )
				spostata = true;
		}

		private void windowSlideShow_LocationChanged(object sender, EventArgs e) {
			if( IsLoaded && posizionamentoInCorso == false && DataContext != null )
				spostata = true;
		}

		#endregion Eventi


#if false

		/// <summary>
		/// Controllo che la geometria della finestra sia posizionabile in un monitor.
		/// </summary>
		private void gestisciPosizione()
		{
			// Per prima cosa, provo a posizionare la finestra nella posizione indicata in configurazione
			// Mi copio in locale la geometria della finestra per poterla modificare con lo spostamento
			geoCurrent = (GeometriaFinestra)Configurazione.UserConfigLumen.geometriaFinestraSlideShow.Clone();


			WpfScreen scrn = WpfScreen.GetScreenFrom( geoCurrent.deviceEnum );
			if( scrn == null )
				scrn = WpfScreen.GetFirstScreen();  // Prendo quello di default

			// this.WindowStartupLocation = WindowStartupLocation.Manual;

            _giornale.Debug( "SlideShow Primary Device Info:\n " + WpfScreen.Primary.ToDebugString() );

			_giornale.Debug( "SlideShow Configuration Device Info:\n " + scrn.ToDebugString() );

			
			
			if ( geoCurrent.deviceEnum != scrn.deviceEnum )
			{
				_giornale.Debug("Il monitor impostato non è stato trovato!!!");

				geoCurrent.deviceEnum = WpfScreen.Primary.deviceEnum;
			}

			if (!verificaProiettabile( scrn, geoCurrent ) )
			{
				_giornale.Debug("I valori calcolati non sono ammissibili utilizzo quelli di default");

				this.geoCurrent = Configurazione.creaGeometriaSlideShowDefault();

			}

            //Se ho messo il full screen resetto i Top e Left
            if ( geoCurrent.fullScreen )
            {
                Screen s = (Screen)Screen.AllScreens.GetValue( geoCurrent.deviceEnum );
                System.Drawing.Rectangle r = s.WorkingArea;
				geoCurrent.Top = r.Top;
				geoCurrent.Left = r.Left;
            }
            else if ( geoCurrent.screenHeight != (int)scrn.WorkingArea.Height || geoCurrent.screenWidth != (int)scrn.WorkingArea.Width)
			{
                _giornale.Debug("Ricalcolo la geometria dello slideShow in base al nuovo monitor");
				_giornale.Debug("*** VALORI VECCHI ***");
				_giornale.Debug( geoCurrent.ToDebugString() );

				geoCurrent.Top = geoCurrent.Top <= 0 ? 0 : (int)(geoCurrent.Top * scrn.WorkingArea.Height / geoCurrent.screenHeight);
				geoCurrent.Left = geoCurrent.Left <= 0 ? 0 : (int)(geoCurrent.Left * scrn.WorkingArea.Width / geoCurrent.screenWidth);

				geoCurrent.Height = (int)(geoCurrent.Height * scrn.WorkingArea.Height / geoCurrent.screenHeight);
				geoCurrent.Width = (int)(geoCurrent.Width * scrn.WorkingArea.Width / geoCurrent.screenWidth);

                Screen s = (Screen)Screen.AllScreens.GetValue(geoCurrent.deviceEnum);
                System.Drawing.Rectangle r = s.WorkingArea;

				geoCurrent.screenHeight = (int)scrn.WorkingArea.Height;
				geoCurrent.screenWidth = (int)scrn.WorkingArea.Width;

				_giornale.Debug("*** VALORI RICALCOLATI ***");
				_giornale.Debug( geoCurrent.ToDebugString() );
			}

			GestoreFinestrePubbliche.risposizionaFinestra( this, geoCurrent );

			// Il salvataggio della posizione, lo farei solo su richiesta esplicita dell'utente
			
			if (salvaNuoviValori)
			{
				_giornale.Debug("Devo salvare i nuovi valori ricalcolati");
				_giornale.Debug("Devo salvare la configurazione utente su file xml");
				UserConfigSerializer.serializeToFile(cfg);
				_giornale.Info("Salvata la configurazione utente su file xml");
			}
		}
#endif

	}
}
