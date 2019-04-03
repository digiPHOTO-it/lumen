using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi.Stampare;
using System.Printing;
using System.Windows.Controls;
using System.Text.RegularExpressions;

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
            //http://stackoverflow.com/questions/1018001/is-there-a-net-way-to-enumerate-all-available-network-printers
            var match = Regex.Match(nomeStampante, @"(?<machine>\\\\.*?)\\(?<queue>.*)");
            if (match.Success)
            {
                _printQueue = new PrintServer(match.Groups["machine"].Value).GetPrintQueue(match.Groups["queue"].Value);
            }
            else
            {
                _printQueue = _printServer.GetPrintQueue( nomeStampante );
            }

			try {
				_printCapabilities = _printQueue.GetPrintCapabilities();
			} catch( Exception ) {
				// Le stampanti shinko in rete non supportano questa operazione
			}

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
