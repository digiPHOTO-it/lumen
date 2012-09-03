
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
namespace Digiphoto.Lumen.UI {

	class MainWindowViewModel : ClosableWiewModel, IObserver<Messaggio> {

		public MainWindowViewModel() {

            selettoreStampantiInstallateViewModel = new SelettoreStampantiInstallateViewModel();

            selettoreFormatoCartaViewModel = new SelettoreFormatoCartaViewModel();

            selettoreFormatoCartaAbbinatoViewModel = new SelettoreFormatoCartaAbbinatoViewModel();

			// Ascolto i messaggi
			IObservable<Messaggio> observable = LumenApplication.Instance.bus.Observe<Messaggio>();
			observable.Subscribe( this );
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

		private void reportVendite() {

			ParamRangeGiorni paramRangeGiorni = new ParamRangeGiorni();
			IVenditoreSrv srv = LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			
			RangeGiorniDialog d = new RangeGiorniDialog();
			bool? esito = d.ShowDialog();
			
			if( esito == true ) {
				paramRangeGiorni.dataIniz = d.giornoIniz;
				paramRangeGiorni.dataFine = d.giornoFine;
			}

			d.Close();

			if( esito == true ) {
				List<RigaReportVendite> righe = srv.creaReportVendite( paramRangeGiorni );

				ReportHostWindow rhw = new ReportHostWindow();
				rhw.impostaDataSource( righe );
				rhw.reportPath = ".\\Reports\\ReportVendite.rdlc";


				// Imposto qualche parametro da stampare nel report
				ReportParameter p1 = new ReportParameter( "dataIniz", paramRangeGiorni.dataIniz.ToString() );
				ReportParameter p2 = new ReportParameter( "dataFine", paramRangeGiorni.dataFine.ToString() );
				string appo = String.IsNullOrEmpty( Configurazione.infoFissa.descrizPuntoVendita ) ? "pdv " + Configurazione.infoFissa.idPuntoVendita : Configurazione.infoFissa.descrizPuntoVendita;
				ReportParameter p3 = new ReportParameter( "nomePdv", appo );

				ReportParameter [] repoParam = { p1, p2, p3 };
				rhw.viewerInstance.LocalReport.SetParameters( repoParam );


				rhw.renderReport();
				rhw.ShowDialog();
			}
		}

		private void log(){
			LoggingShowWindows loggingShowWindows = new LoggingShowWindows();
			loggingShowWindows.Show();
		}

		private RelayCommand _logCommand;
		public ICommand LogCommand
		{
			get
			{
				if (_logCommand == null)
				{
					_logCommand = new RelayCommand(param => log(),
															  param => true,
															  false);
				}
				return _logCommand;
			}
		}


		private void reportConsumoCarta()
		{
			ParamRangeGiorni paramRangeGiorni = new ParamRangeGiorni();
			//IVenditoreSrv srv = LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();

			RangeGiorniDialog d = new RangeGiorniDialog();
			d.noteAggiuntive = "Attenzione: questo report attualmente conteggia solo i fogli dei provini e non le fotografie.";
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

		private RelayCommand _reportConsumoCartaCommand;
		public ICommand reportConsumoCartaCommand
		{
			get
			{
				if (_reportConsumoCartaCommand == null)
				{
					_reportConsumoCartaCommand = new RelayCommand(param => reportConsumoCarta(),
															  param => true,
															  false);
				}
				return _reportConsumoCartaCommand;
			}
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
					dialogProvider.ShowError( sm.lavoroDiStampa.ToString(), "Lavoro di stampa fallito", null );
				}
			}

		}
	}
}
