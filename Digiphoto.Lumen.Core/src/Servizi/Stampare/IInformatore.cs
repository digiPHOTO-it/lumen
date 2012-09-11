using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Servizi.Stampare {


	/// <summary>
	/// Serve a reperire informazioni sulla stampante (o sulle stampanti)
	/// </summary>
	public interface IInformatore : IDisposable {

		void load( string nomeStampante );

		/**
		 *  E' dato dalla larghezza / altezza  
		 *  rapp = ww / hh
		 */
		float rapporto {
			get;
		}
	}
}
