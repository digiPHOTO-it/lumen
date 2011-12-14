using System;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Stampare;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public class StampatoMsg : Messaggio {

		public LavoroDiStampa lavoroDiStampa {
			get;
			private set;
		}

		public StampatoMsg( LavoroDiStampa lavoro ) : base( "Stampa completata" ) {
			this.lavoroDiStampa = lavoro;
		}

	}
}
