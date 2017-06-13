using Digiphoto.Lumen.Applicazione;
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
using Digiphoto.Lumen.Servizi.Reports.ConsumoCarta;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.UI.DataEntry.DEGiornata;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.UI.DataEntry.DEFotografo;
using Digiphoto.Lumen.UI.DataEntry.DEEvento;
using System.Windows;
using Digiphoto.Lumen.UI.Pubblico;
using System.Text;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using System.IO;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.UI.Preferenze;
using static System.Environment;
using System.Net.Mail;
using Digiphoto.Lumen.Core.Servizi.Utilita;

namespace Digiphoto.Lumen.UI {

	class MainWindowViewModel : ClosableWiewModel, IObserver<Messaggio> {

		public MainWindowViewModel() {

			lastUsedTestConfig();

			// Tengo un massimo di elementi in memoria per evitare consumi eccessivi
			informazioniUtente = new RingBuffer<InformazioneUtente>( 30 );

			carrelloViewModel = new CarrelloViewModel();
			fotoGalleryViewModel = new FotoGalleryViewModel();

			selettoreStampantiInstallateViewModel = new SelettoreStampantiInstallateViewModel();
			DataContextStampantiInstallate = selettoreStampantiInstallateViewModel;

			// Ascolto i messaggi
			IObservable<Messaggio> observable = LumenApplication.Instance.bus.Observe<Messaggio>();
			observable.Subscribe( this );

			Messaggio msgInit = new Messaggio( this );
			msgInit.showInStatusBar = true;
			msgInit.descrizione = "Nessun messaggio";
			msgInit.esito = 0;

			LumenApplication.Instance.bus.Publish( msgInit );

			caricaElencoDischiRimovibili();

			this.abilitoShutdown = true;  // permetto all'utente di scegliere se spegnere il computer.
		}

		private void ejectUsb() {
			//Recupero solo la lettera...
			char letter = ejectUsbItem.Name.ToCharArray()[0];

			if( UsbEjectWithExe.usbEject( letter ) )
				dialogProvider.ShowMessage( "Chiavetta rimossa con successo", "Eject Usb" );
			else
				dialogProvider.ShowMessage( "Errore rimozione chiavetta", "Eject Usb Errore" );

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

		private SelettoreStampantiInstallateViewModel selettoreStampantiInstallateViewModel = null;

		public SelettoreStampantiInstallateViewModel DataContextStampantiInstallate {
			get;
			set;
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

		private String _numFotoFase;
		public String numFotoFase {
			get {
				return _numFotoFase;
			}
			private set {
				if( _numFotoFase != value ) {
					_numFotoFase = value;
					OnPropertyChanged( "numFotoFase" );
				}
			}
		}

		private bool _numFotoFaseVisibility;
		public bool numFotoFaseVisibility {
			get {
				return _numFotoFaseVisibility;
			}
			private set {
				if( _numFotoFaseVisibility != value ) {
					_numFotoFaseVisibility = value;
					OnPropertyChanged( "numFotoFaseVisibility" );
				}
			}
		}

		public Boolean compNumFotoVisibility {
			get {
				return Configurazione.UserConfigLumen.compNumFoto;
			}
		}

		public SlideShowViewModel slideShowViewModel {
			get {
				return ((App)Application.Current).gestoreFinestrePubbliche.slideShowViewModel;
			}
		}

		public Boolean eliminaFotoEnabled {
			get {
				return IsInDesignMode ? true : Configurazione.infoFissa.numGiorniEliminaFoto > 0;
			}
		}

		public ObservableCollectionEx<DriveInfo> dischiRimovibili {
			get;
			private set;
		}

		private DriveInfo _ejectUsbItem;
		public DriveInfo ejectUsbItem {
			get {
				return _ejectUsbItem;
			}
			set {
				if( _ejectUsbItem != value ) {
					_ejectUsbItem = value;
					OnPropertyChanged( "ejectUsbItem" );
				}
			}
		}

		public bool possoEjectUsb {
			get {
				bool posso = true;

				if( posso && ejectUsbItem == null )
					posso = false;

				return posso;
			}
		}

		#endregion Prorietà

		#region Comandi

		private RelayCommand _uscireCommand;
		public ICommand uscireCommand {
			get {
				if( _uscireCommand == null ) {
					_uscireCommand = new RelayCommand( param => uscire( "SHUTDOWN".Equals( param ) ), param => true, false );
				}
				return _uscireCommand;
			}
		}


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

		private RelayCommand _visualizzareLogCommand;
		public ICommand visualizzareLogCommand {
			get {
				if( _visualizzareLogCommand == null ) {
					_visualizzareLogCommand = new RelayCommand( param => visualizzareLog(), param => true );
				}
				return _visualizzareLogCommand;
			}
		}

		private RelayCommand _spedireLogCommand;
		public ICommand spedireLogCommand {
			get {
				if( _spedireLogCommand == null ) {
					_spedireLogCommand = new RelayCommand( param => spedireLog(), param => true );
				}
				return _spedireLogCommand;
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

		private RelayCommand _reportProvvigioniCommand;
		public ICommand reportProvvigioniCommand {
			get {
				if( _reportProvvigioniCommand == null ) {
					_reportProvvigioniCommand = new RelayCommand( param => reportProvvigioni(),
															  param => true,
															  false );
				}
				return _reportProvvigioniCommand;
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
		public ICommand ejectUsbCommand {
			get {
				if( _ejectUsbCommand == null ) {
					_ejectUsbCommand = new RelayCommand( param => ejectUsb(),
															  param => possoEjectUsb,
															  false );
				}
				return _ejectUsbCommand;
			}
		}

		private RelayCommand _eseguireRefreshCommand;
		public ICommand eseguireRefreshCommand {
			get {
				if( _eseguireRefreshCommand == null ) {
					_eseguireRefreshCommand = new RelayCommand( p => eseguireRefresh() );
				}
				return _eseguireRefreshCommand;
			}
		}

		private RelayCommand _modificarePreferenzeCommand;
		public ICommand modificarePreferenzeCommand {
			get {
				if( _modificarePreferenzeCommand == null ) {
					_modificarePreferenzeCommand = new RelayCommand( nn => modificarePreferenze(), nn => true, false );
				}
				return _modificarePreferenzeCommand;
			}
		}

		#endregion Comandi

		#region Metodi

		private void uscire( bool eseguiShutdown ) {
			//Controllo se posso fermare l'applicazione
			if( !LumenApplication.Instance.possoFermare ) {
				bool procediPure = false;
				string msg = "Attenzione:" +
					"\nCi sono delle operazioni che non sono concluse." +
					"\nForzando l'uscita si rischia la perdita di dati." +
					"\nSi consiglia di chiudere correttamante i servizi rimasti in sospeso." +
					"\nSei sicuro che vuoi forzare l'uscita dal programma ?";

				dialogProvider.ShowConfirmation( msg, "Avviso", ( sino ) => {
					procediPure = sino;
				} );
				if( !procediPure )
					return;
			}

			if( eseguiShutdown )
				this.shutdownConfermato = true;     // mi ha già detto che vuole spegnere
			else
				this.abilitoShutdown = false;       // mi ha già detto che NON vuole spegnere


			((App)App.Current).gestoreFinestrePubbliche.chiudereTutteLeFinestre();

		}


		private void reportVendite() {

			ParamRangeGiorni paramRangeGiorni = richiediParametriRangeGiorni();
			if( paramRangeGiorni == null )
				return;

			Servizi.Vendere.IVenditoreSrv srv = LumenApplication.Instance.getServizioAvviato<Servizi.Vendere.IVenditoreSrv>();

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

			ReportParameter[] repoParam = { p1, p2, p3 };
			rhw.viewerInstance.LocalReport.SetParameters( repoParam );

			_giornale.Debug( "Impostati i parametri del report: " + paramRangeGiorni.dataIniz + " -> " + paramRangeGiorni.dataFine );

			rhw.renderReport();

			_giornale.Debug( "render del report" );
			rhw.ShowDialog();

			_giornale.Info( "Completato il report delle vendite DAL" + paramRangeGiorni.dataIniz + " -> " + paramRangeGiorni.dataFine );
		}
		

		private void visualizzareLog(){

			// Per adesso apro con l'editor di default del sistema
			// TODO bisognerebbe fare una lista dei file 
			// TODO questo è solo uno dei file di log che si possono configurare in log4net. possono essercene altri oppure cambiare nome. Per ora ok cosi
			String nomeFileLog = Path.Combine( Environment.GetFolderPath( SpecialFolder.LocalApplicationData ), "digiPHOTO.it", "Lumen", "Log", "lumenUI-log.txt" );
			if( File.Exists( nomeFileLog ) )
				System.Diagnostics.Process.Start( nomeFileLog );
			else
				dialogProvider.ShowError( nomeFileLog, "File non esistente!", null );

		}

		private void spedireLog() {

			bool procediPure = true;

			dialogProvider.ShowConfirmation( "Assicurarsi di avere la connessione alla rete.\nVuoi inviare il giornale al team di assistenza ?",
				"Richiesta conferma",
				  ( confermato ) => {
					  procediPure = confermato;
				  } );

			if( !procediPure )
				return;

			// Siccome questo servizio non è sempre indispensabile (anzi quasi mai), lo istanzio solo alla necessità
			using( IUtilitaSrv utilitaSrv = LumenApplication.Instance.creaServizio<IUtilitaSrv>() ) {

				bool esitoOk = utilitaSrv.inviaLog();
				if( esitoOk ) {
					this.trayIconProvider.showInfo( "Ok", "Log inviato", 5000 );
				} else {
					dialogProvider.ShowError( "Invio log fallito.\nControllare connessione di rete ", "Errore", null );
				}
			}
		}			

		ParamRangeGiorni richiediParametriRangeGiorni() {

			ParamRangeGiorni paramRangeGiorni = null;

			RangeGiorniDialog d = new RangeGiorniDialog();
			bool? esito = d.ShowDialog();

			if( esito == true ) {
				paramRangeGiorni = new ParamRangeGiorni();
				paramRangeGiorni.dataIniz = d.giornoIniz;
				paramRangeGiorni.dataFine = d.giornoFine;
			}

			d.Close();
			return paramRangeGiorni;
		}

		private void reportConsumoCarta()
		{
			ParamRangeGiorni paramRangeGiorni = richiediParametriRangeGiorni();
			if( paramRangeGiorni == null )
				return;

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

		private void reportProvvigioni() {

			ParamRangeGiorni paramRangeGiorni = richiediParametriRangeGiorni();
			if( paramRangeGiorni == null )
				return;

			Servizi.Vendere.IVenditoreSrv srv = LumenApplication.Instance.getServizioAvviato<Servizi.Vendere.IVenditoreSrv>();
			List<RigaReportProvvigioni> righe = srv.creaReportProvvigioni( paramRangeGiorni );

			ReportHostWindow rhw = new ReportHostWindow();
			rhw.impostaDataSource( righe );
			rhw.reportPath = ".\\Reports\\ReportProvvigioni.rdlc";

			// Imposto qualche parametro da stampare nel report
			ReportParameter p1 = new ReportParameter( "dataIniz", paramRangeGiorni.dataIniz.ToString() );
			ReportParameter p2 = new ReportParameter( "dataFine", paramRangeGiorni.dataFine.ToString() );
			string appo = String.IsNullOrEmpty( Configurazione.infoFissa.descrizPuntoVendita ) ? "pdv " + Configurazione.infoFissa.idPuntoVendita : Configurazione.infoFissa.descrizPuntoVendita;
			ReportParameter p3 = new ReportParameter( "nomePdv", appo );

			ReportParameter[] repoParam = { p1, p2, p3 };
			rhw.viewerInstance.LocalReport.SetParameters( repoParam );

			rhw.renderReport();
			rhw.ShowDialog();
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

		public BitmapSource statusSlideShowImage {

			get {
				// Decido qual'è la giusta icona da caricare per mostrare lo stato dello slide show (Running, Pause, Empty)

				// Non so perchè ma se metto il percorso senza il pack, non funziona. boh eppure sono nello stesso assembly.
				string uriTemplate = @"pack://application:,,,/Digiphoto.Lumen.UI;component/Resources/##-16x16.png";
				Uri uri = null;

				if( slideShowViewModel != null ) {
					if( slideShowViewModel.isRunning )
						uri = new Uri( uriTemplate.Replace( "##", "ssRunning" ) );

					if( slideShowViewModel.isPaused )
						uri = new Uri( uriTemplate.Replace( "##", "ssPause" ) );

					if( slideShowViewModel.isEmpty )
						uri = new Uri( uriTemplate.Replace( "##", "ssEmpty" ) );

					return new BitmapImage( uri );
				} else
					return null;
			}
		}

		private void lastUsedTestConfig()
		{
			_giornale.Debug("Carico eventuale configurazione last-used già presente");

			// Verifico se ho il file di configurazione last-used nel caso lo creo
			if (Configurazione.caricaLastUsedConfig() == null)
			{
				_giornale.Debug("La configurazione last-used non esite. La creo adesso su file.");
				Configurazione.LastUsedConfigLumen = Configurazione.creaLastUsedConfig();
				LastUsedConfigSerializer.serializeToFile(Configurazione.LastUsedConfigLumen);
			}
		}

		private void eseguireRefresh() {

			// Pubblico un messaggio di eseguire un refesh
			var msgRefresh = new Digiphoto.Lumen.Core.Eventi.RefreshMsg( this );
			msgRefresh.descrizione = "REFRESH";
			LumenApplication.Instance.bus.Publish( msgRefresh );

		}

		private void modificarePreferenze() {
			PreferenzeWindow window = new PreferenzeWindow();
			window.DataContext = new PreferenzeViewModel();
			window.ShowDialog();
		}

		#endregion Metodi

		#region Eventi

		protected override void OnRequestClose() {
			
			// Faccio la dispose di tutti i viewmodel che ho istanziato io.

			if( selettoreStampantiInstallateViewModel != null ) {
				selettoreStampantiInstallateViewModel.Dispose();
				selettoreStampantiInstallateViewModel = null;
            }

			if( fotoGalleryViewModel != null ) {
				fotoGalleryViewModel.Dispose();
				fotoGalleryViewModel = null;
			}

			if( carrelloViewModel != null ) {
				carrelloViewModel.Dispose();
				carrelloViewModel = null;
			}

			base.OnRequestClose();
		}

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
					App.Current.Dispatcher.BeginInvoke(
							new Action( () => {
								dialogProvider.ShowError( sm.lavoroDiStampa.ToString(), "Lavoro di stampa fallito", null );
							}
						) );
				}
			}

			if (msg is ScaricoFotoMsg)
			{
				ScaricoFotoMsg sm = (ScaricoFotoMsg)msg;
				if(sm.fase == FaseScaricoFoto.InizioScarico){
					numFotoFaseVisibility = true;
					numFotoFase = String.Format( "Scaricate = {0:000}", 0 );
				}else if(sm.fase == FaseScaricoFoto.Scaricamento) {
					numFotoFaseVisibility = true;
					numFotoFase = String.Format( "Scaricate = {0:000}", sm.esitoScarico.totFotoScaricateProg );
				}
				else if (sm.fase == Digiphoto.Lumen.Servizi.Scaricatore.FaseScaricoFoto.Provinatura)
				{
					numFotoFase = String.Format("Provinate = {0:000}/{1:000}", sm.esitoScarico.totFotoProvinateProg, sm.esitoScarico.totFotoScaricate );
				} else if(sm.fase == FaseScaricoFoto.FineLavora){
					numFotoFaseVisibility = false;
				}
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

			if( msg is CambioStatoMsg ) {
				if( msg.sender is SlideShowViewModel ) {
					// Devo aggiornare lo stato della icona dello slide show
					CambioStatoMsg csm = (CambioStatoMsg)msg;
					SlideShowStatus nuovoStato = (SlideShowStatus)csm.nuovoStato;
					OnPropertyChanged( "statusSlideShowImage" );
				}
			}

		}
#endregion Eventi

	}
}
