using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi {

	public interface IServizio : IDisposable, IObserver<Messaggio> {
	
		void start();

		void stop();
	}

}
