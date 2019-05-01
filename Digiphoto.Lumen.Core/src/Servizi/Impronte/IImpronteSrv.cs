using Digiphoto.Lumen.Servizi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Servizi.Impronte {

	public delegate void OnImmagineAcquisita( object sender, ScansioneEvent eventArgs );

	public class InfoScanner {

		public string vendor {
			get;
			set;
		}

		public string productName {
			get;
			set;
		}

		public string serialNumber {
			get;
			set;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			if( vendor != null )
				sb.AppendFormat( "Vendor = {0} ; ", vendor );
			if( productName != null )
				sb.AppendFormat( "Product Name = {0} ; ", productName );
			if( serialNumber != null )
				sb.AppendFormat( "Serial Number = {0} ; ", serialNumber );
			return sb.ToString();
		}

	}

	public interface IImpronteSrv : IServizio {

		bool testScannerConnected();

		/// <summary>
		/// Quando lo scanner è connesso, si possono leggere queste info caratteristice del modello
		/// </summary>
		InfoScanner infoScanner {
			get;
		}

#if FALSE
		/// <summary>
		/// Indica la cartella in cui si vogliono memorizzare le immagini delle impronte
		/// </summary>
		string imgFolder {
			set;
		}
#endif

		void Listen( OnImmagineAcquisita callback );

	}
}
