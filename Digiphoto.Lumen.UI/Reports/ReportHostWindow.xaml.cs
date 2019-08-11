using System.Windows;
using System.Collections;
using Microsoft.Reporting.WinForms;

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
		
	}

}
