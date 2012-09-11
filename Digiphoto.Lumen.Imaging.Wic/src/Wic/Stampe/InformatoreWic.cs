using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi.Stampare;
using System.Printing;
using System.Windows.Controls;

namespace Digiphoto.Lumen.Imaging.Wic.Stampe {

	public class InformatoreWic : IInformatore {

		PrintServer _printServer;
		PrintQueue _printQueue;
		PrintCapabilities _printCapabilities;
		PrintDialog _printDialog;

		public InformatoreWic() {
			// Uso me stesso, cioè il computer su cui sto girando.
			_printServer = new PrintServer();
		}

		public void load( string nomeStampante ) {

			_printQueue = _printServer.GetPrintQueue( nomeStampante );

			_printCapabilities = _printQueue.GetPrintCapabilities();

			_printDialog = new PrintDialog();
			_printDialog.PrintQueue = _printQueue;
		}

		public float rapporto {
			get {
				return (float) (_printDialog.PrintableAreaWidth / _printDialog.PrintableAreaHeight);
			}
		}

		public void Dispose() {
			if( _printQueue != null ) {
				_printQueue.Dispose();
				_printQueue = null;
			}

			if( _printServer != null ) {
				_printServer.Dispose();
				_printServer = null;
			}
		}
	}
}
