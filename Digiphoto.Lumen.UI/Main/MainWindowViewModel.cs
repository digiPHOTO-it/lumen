
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media;
using Digiphoto.Lumen.Util;
using System.Windows.Media.Imaging;
using System;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.UI.Dialogs;
using Digiphoto.Lumen.Servizi.Reports;
using System.Collections.Generic;
using Digiphoto.Lumen.UI.Reports;
using Microsoft.Reporting.WinForms;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Logging;
using Digiphoto.Lumen.UI.EliminaVecchiRullini;
using Digiphoto.Lumen.Servizi.Reports.ConsumoCarta;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.UI.DataEntry.DEGiornata;
using Digiphoto.Lumen.UI.DataEntry;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.UI.Main;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.UI.DataEntry.DEFotografo;
using Digiphoto.Lumen.UI.DataEntry.DEEvento;
using System.Windows;
using Digiphoto.Lumen.UI.Pubblico;
using System.Text;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using System.IO;
using Digiphoto.Lumen.Servizi.Scaricatore;

namespace Digiphoto.Lumen.UI {

	class MainWindowViewModel : ClosableWiewModel, IObserver<Messaggio> {

		public MainWindowViewModel() {

			// Tengo un massimo di elementi in memoria per evitare consumi eccessivi
			informazioniUtente = new RingBuffer<InformazioneUtente>( 30 );

			carrelloViewModel = new CarrelloViewModel();
			fotoGalleryViewModel = new FotoGalleryViewModel();

			// Ascolto i messaggi
			IObservable<Messaggio> observable = LumenApplication.Instance.bus.Observe<Messaggio>();
			observable.Subscribe( this );
			
			Messaggio msgInit = new Messaggio(this);
			msgInit.showInStatusBar = true;
			msgInit.descrizione = "Nessun messaggio";
			msgInit.esito = 0;

			LumenApplication.Instance.bus.Publish(msgInit);
			
			caricaElencoDischiRimovibili();

			#if(!DEBUG)
				this.abilitoShutdown = true;  // permetto all'utente di scegliere se spegnere il computer.
			#endif
        }

		private void ejectUsb()
		{
			//Recupero solo la lettera...
			char letter = ejectUsbItem.Name.ToCharArray()[0];

			if(UsbEjectWithExe.usbEject(letter))
				dialogProvider.ShowMessage("Chiavetta rimossa con successo", "Eject Usb");
			else
				dialogProvider.ShowMessage("Errore rimozione chiavetta","Eject Usb Errore");		

			//caricaElencoDischiRimovibili();
        }

		#region Proprietà

		CarrelloViewModel _carrelloViewModel;
		public CarrelloViewModel carrelloViewModel {
			get {
				return _carrelloViewModel;
			}
			private set {
				_carrelloViewModel = value;
			}
		}

		FotoGalleryViewModel _fotoGalleryViewModel;
		public FotoGalleryViewModel fotoGalleryViewModel {
			get {
				return _fotoGalleryViewModel;
			}
			private set {
				_fotoGalleryViewModel = value;
			}
		}


		/// <summary>
		/// Ritorno la testa del buffer circolare
		/// ossia l'ultimo elemento inserito.
		/// </summary>
		public InformazioneUtente ultimaInformazioneUtente {
			get {
				// La Peek non rimuove l'elemento dal buffer. Invece la Pop si.
				return (informazioniUtente != null && informazioniUtente.IsEmpty == false) ? informazioniUtente.HeadElement : null;
			}
		}

		public RingBuffer<InformazioneUtente> informazioniUtente {
			get;
			private set;
		}

		public String numFotoFase
		{
			get;
			private set;
		}

		public Boolean numFotoFaseVisibility
		{
			get;
			private set;
		}

		public SlideShowViewModel slideShowViewModel {
			get {
				return ((App)Application.Current).slideShowViewModel;
			}
		}

		public ObservableCollectionEx<DriveInfo> dischiRimovibili
		{
			get;
			private set;
		}

		private DriveInfo _ejectUsbItem;
		public DriveInfo ejectUsbItem
		{
			get
			{
				return _ejectUsbItem;
			}
			set
			{
				if (_ejectUsbItem != value)
				{
					_ejectUsbItem = value;
					OnPropertyChanged("ejectUsbItem");
				}
			}
		}

		public bool possoEjectUsb
		{
			get
			{
				bool posso = true;

				if (posso && ejectUsbItem == null)
					posso = false;

				return posso;
			}
		}

		#endregion Prorietà

		#region Comandi
		private RelayCommand _reportVenditeCommand;
		public ICommand reportVenditeCommand {
			get {
				if( _reportVenditeCommand == null ) {
					_reportVenditeCommand = new RelayCommand( param => reportVendite(),
						                                      param => true,
															  false );
				}
				return _reportVenditeCommand;
			}
		}

		private RelayCommand _logCommand;
		public ICommand LogCommand {
			get {
				if( _logCommand == null ) {
					_logCommand = new RelayCommand( param => log(),
															  param => true,
															  false );
				}
				return _logCommand;
			}
		}

		private RelayCommand _commandDataEntry;
		public ICommand commandDataEntry {
			get {
				if( _commandDataEntry == null ) {
					_commandDataEntry = new RelayCommand( param => dataEntry( param as string ),
					                                      param => true,
				                                          false );
				}
				return _commandDataEntry;
			}
		}

		private RelayCommand _reportConsumoCartaCommand;
		public ICommand reportConsumoCartaCommand {
			get {
				if( _reportConsumoCartaCommand == null ) {
					_reportConsumoCartaCommand = new RelayCommand( param => reportConsumoCarta(),
															  param => true,
															  false );
				}
				return _reportConsumoCartaCommand;
			}
		}

		private RelayCommand _commandRivelareNumFotoSlideShow;
		public ICommand commandRivelareNumFotoSlideShow {
			get {
				if( _commandRivelareNumFotoSlideShow == null ) {
					_commandRivelareNumFotoSlideShow = new RelayCommand( param => rivelareNumFotoSlideShow(),
															  param => true );
				}
				return _commandRivelareNumFotoSlideShow;
			}
		}

		private RelayCommand _ejectUsbCommand;
		public ICommand ejectUsbCommand
		{
			get {
				if (_ejectUsbCommand == null)
				{
					_ejectUsbCommand = new RelayCommand(param => ejectUsb(),
															  param => possoEjectUsb,
															  false );
				}
				return _ejectUsbCommand;
			}
		}

		#endregion Comandi

		#region Metodi

		private void reportVendite() {

			ParamRangeGiorni paramRangeGiorni = new ParamRangeGiorni();
			IVenditoreSrv srv = LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();

			_giornale.Debug( "Sto per aprire il dialogo con il calendario per richiesta date" );
			RangeGiorniDialog d = new RangeGiorniDialog();
			bool? esito = d.ShowDialog();
			
			if( esito == true ) {
				paramRangeGiorni.dataIniz = d.giornoIniz;
				paramRangeGiorni.dataFine = d.giornoFine;
			}

			d.Close();

			if( esito == true ) {
				List<RigaReportVendite> righe = srv.creaReportVendite( paramRangeGiorni );

				string nomeRpt = ".\\Reports\\ReportVendite.rdlc";
				_giornale.Debug( "devo caricare il report: " + nomeRpt );

				ReportHostWindow rhw = new ReportHostWindow();
				rhw.impostaDataSource( righe );
				rhw.reportPath = nomeRpt;


				// Imposto qualche parametro da stampare nel report
				ReportParameter p1 = new ReportParameter( "dataIniz", paramRangeGiorni.dataIniz.ToString() );
				ReportParameter p2 = new ReportParameter( "dataFine", paramRangeGiorni.dataFine.ToString() );
				string appo = String.IsNullOrEmpty( Configurazione.infoFissa.descrizPuntoVendita ) ? "pdv " + Configurazione.infoFissa.idPuntoVendita : Configurazione.infoFissa.descrizPuntoVendita;
				ReportParameter p3 = new ReportParameter( "nomePdv", appo );

				ReportParameter [] repoParam = { p1, p2, p3 };
				rhw.viewerInstance.LocalReport.SetParameters( repoParam );

				_giornale.Debug( "Impostati i parametri del report: " + paramRangeGiorni.dataIniz + " -> " + paramRangeGiorni.dataFine );

				rhw.renderReport();

				_giornale.Debug( "render del report" );
				rhw.ShowDialog();

				_giornale.Info( "Completato il report delle vendite DAL" + paramRangeGiorni.dataIniz + " -> " + paramRangeGiorni.dataFine );
			}
		}

		private void log(){
			LoggingShowWindows loggingShowWindows = new LoggingShowWindows();
			loggingShowWindows.Show();
		}


		private void reportConsumoCarta()
		{
			ParamRangeGiorni paramRangeGiorni = new ParamRangeGiorni();
			//IVenditoreSrv srv = LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();

			RangeGiorniDialog d = new RangeGiorniDialog();
			bool? esito = d.ShowDialog();

			if (esito == true)
			{
				paramRangeGiorni.dataIniz = d.giornoIniz;
				paramRangeGiorni.dataFine = d.giornoFine;
			}

			d.Close();

			if (esito == true)
			{
				dialogProvider.ShowMessage( "Attualmente questo report conteggia soltanto i provini stampati, e non le fotografie", "Avviso" );

				ReportHostWindow rhw = new ReportHostWindow();
				rhw.impostaDataSource(RigaReportConsumoCarta.righe(paramRangeGiorni));
				rhw.reportPath = ".\\Reports\\ReportConsumoCarta.rdlc";

				// Imposto qualche parametro da stampare nel report
				ReportParameter p1 = new ReportParameter("dataIniz", paramRangeGiorni.dataIniz.ToString());
				ReportParameter p2 = new ReportParameter("dataFine", paramRangeGiorni.dataFine.ToString());
				string appo = String.IsNullOrEmpty(Configurazione.infoFissa.descrizPuntoVendita) ? "pdv " + Configurazione.infoFissa.idPuntoVendita : Configurazione.infoFissa.descrizPuntoVendita;
				ReportParameter p3 = new ReportParameter("nomePdv", appo);

				ReportParameter[] repoParam = { p1, p2, p3 };
				rhw.viewerInstance.LocalReport.SetParameters(repoParam);

				rhw.renderReport();
				rhw.ShowDialog();
			}
		}

		void dataEntry( string nomeEntita ) {

			// TODO sostituire con una factory
			if( nomeEntita == "Giornata" ) {

				WindowGiornata window = new WindowGiornata();
				window.ShowDialog();
			}

			// TODO sostituire con una factory
			if (nomeEntita == "Fotografo")
			{
				WindowFotografo window = new WindowFotografo();
				window.ShowDialog();
			}

			// TODO sostituire con una factory
			if (nomeEntita == "Evento")
			{
				WindowEvento window = new WindowEvento();
				window.ShowDialog();
			}
		}

		private void caricaElencoDischiRimovibili()
		{
			DriveInfo[] dischi;

			if (IsInDesignMode)
			{
				dischi = creaDischiFinti();
			}
			else
			{
				dischi = LumenApplication.Instance.getServizioAvviato<IVolumeCambiatoSrv>().GetDrivesUsbAttivi();
			}
			dischiRimovibili = new ObservableCollectionEx<DriveInfo>(dischi);
			if (dischi.Length > 0)
				ejectUsbItem = dischi[dischi.Length-1];
			OnPropertyChanged("dischiRimovibili");
		}

		private DriveInfo[] creaDischiFinti()
		{
			return new DriveInfo[0];

			// non so perchè ma non funzionano.
			/*
			DriveInfo d1 = new DriveInfo( "C:" );
			d1.VolumeLabel = "DISCO 1";
			DriveInfo d2 = new DriveInfo( "C:" );
			d1.VolumeLabel = "DISCO 2";
			DriveInfo d3 = new DriveInfo( "C:" );
			d1.VolumeLabel = "DISCO 3";

			DriveInfo [] ret = new DriveInfo []  { d1, d2, d3 };
			return ret;
			 */ 
		}

		void rivelareNumFotoSlideShow() {

			StringBuilder sb = new StringBuilder( "Stato: " );
			if( slideShowViewModel.isRunning )
				sb.Append( "RUNNING" );
			else if( slideShowViewModel.isPaused )
				sb.Append( "IN PAUSA" );
			else if( slideShowViewModel.isEmpty )
				sb.Append( "VUOTO" );

			if( slideShowViewModel.numFotoCorrente != null )
				sb.Append( "\nFoto: " + slideShowViewModel.numFotoCorrente );

			dialogProvider.ShowMessage( sb.ToString(), "Stato Slide Show" );
		}

		#endregion Metodi

		#region Eventi
		public void OnCompleted() {
			// throw new NotImplementedException();
		}

		public void OnError( Exception error ) {
			// throw new NotImplementedException();
		}

		public void OnNext( Messaggio msg ) {

			if( msg is StampatoMsg ) {

				StampatoMsg sm = (StampatoMsg)msg;

				if( sm.lavoroDiStampa.esitostampa == EsitoStampa.Errore ) {
					dialogProvider.ShowError( sm.lavoroDiStampa.ToString(), "Lavoro di stampa fallito", null );
				}
			}

			if (msg is ScaricoFotoMsg)
			{
				ScaricoFotoMsg sm = (ScaricoFotoMsg)msg;
				if(sm.fase == FaseScaricoFoto.InizioScarico){
					numFotoFaseVisibility = true;
					numFotoFase = String.Format("Fase: 0/0");
				}else if(sm.fase == FaseScaricoFoto.Scaricamento){
					numFotoFase = String.Format("Fase: 0/{0}", sm.esitoScarico.totFotoScaricateProg);
				}
				else if (sm.fase == Digiphoto.Lumen.Servizi.Scaricatore.FaseScaricoFoto.Provinatura)
				{
					numFotoFase = String.Format("Fase: {0}/{1}", sm.esitoScarico.totFotoProvinateProg, sm.esitoScarico.totFotoScaricate); 
				}else if(sm.fase == FaseScaricoFoto.FineLavora){
					numFotoFaseVisibility = false;
				}
				OnPropertyChanged("numFotoFaseVisibility");
				OnPropertyChanged("numFotoFase");
			}

			if( msg.showInStatusBar ) {
				InformazioneUtente infoUser = new InformazioneUtente( msg.descrizione );
				infoUser.esito = msg.esito;

				App.Current.Dispatcher.BeginInvoke(
					new Action(() =>
					{
						informazioniUtente.Write(infoUser);
						OnPropertyChanged( "ultimaInformazioneUtente" );
						OnPropertyChanged( "informazioniUtente" );
					}
				));

			}

			if (msg is VolumeCambiatoMsg)
			{
				caricaElencoDischiRimovibili();
			}

		}
#endregion Eventi

	}
}
