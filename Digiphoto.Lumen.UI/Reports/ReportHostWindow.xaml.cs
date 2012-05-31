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
using Microsoft.Reporting.WinForms;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using System.Data.Common;
using Digiphoto.Lumen.Servizi.Reports;
using System.Collections;

namespace Digiphoto.Lumen.UI.Reports {
	/// <summary>
	/// Interaction logic for ReportHostWindow.xaml
	/// </summary>
	public partial class ReportHostWindow : Window {

		public ReportHostWindow() {
			InitializeComponent();
		}

		public string reportPath {
			get {
				return viewerInstance.LocalReport.ReportPath;
			}
			set {
				viewerInstance.LocalReport.ReportPath = value;
			}
		}

		public void impostaDataSource( IEnumerable datiBuoni ) {
			ReportDataSource ds = new ReportDataSource( "DataSet1", datiBuoni );
			viewerInstance.LocalReport.DataSources.Add( ds );
		}

		public void renderReport() {
			viewerInstance.RefreshReport();
		}


		List<RigaReportVendite> DACANC___caricaDati() {

			List<RigaReportVendite> vendite = new List<RigaReportVendite>();
			using( new UnitOfWorkScope() ) {


				RigaReportVendite r1 = new RigaReportVendite {
					giornata = DateTime.Today,
					totFotoStampate = 35,
					totDischettiMasterizzati = 3,
					totIncassoCalcolato = 200,
					totIncassoDichiarato = 210
				};

				RigaReportVendite r2 = new RigaReportVendite {
					giornata = DateTime.Today.AddDays( -3 ),
					totFotoStampate = 12,
					totDischettiMasterizzati = 2,
					totIncassoCalcolato = 100,
					totIncassoDichiarato = 99
				};

				vendite.Add( r1 );
				vendite.Add( r2 );

			}
			return vendite;
		}

	

	}
}
