
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
				string appo = String.IsNullOrEmpty(Configurazione.UserConfigLumen.DescrizionePuntoVendita) ? "pdv " + Configurazione.UserConfigLumen.CodicePuntoVendita : Configurazione.UserConfigLumen.DescrizionePuntoVendita;
				ReportParameter p3 = new ReportParameter( "nomePdv", appo );

				ReportParameter [] repoParam = { p1, p2, p3 };
				rhw.viewerInstance.LocalReport.SetParameters( repoParam );


				rhw.renderReport();
				rhw.ShowDialog();
			}
		}


	}
}
