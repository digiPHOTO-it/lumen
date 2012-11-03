
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

/* DACANC
		private RelayCommand _commandHistoryInformazioniUtente;
		public ICommand commandHistoryInformazioniUtente {
			get {
				if( _commandHistoryInformazioniUtente == null ) {
					_commandHistoryInformazioniUtente = new RelayCommand( param => esguireHistoryInformazioniUtente(),
															  param => true,
															  false );
				}
				return _commandHistoryInformazioniUtente;
			}
		}
*/	
		

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

		}
#endregion Eventi

	}
}
