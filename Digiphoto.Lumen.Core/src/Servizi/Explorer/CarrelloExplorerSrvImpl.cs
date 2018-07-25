using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.ComponentModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Eventi;
using System.Threading;
using Digiphoto.Lumen.Util;
using log4net;
using Digiphoto.Lumen.Core.Database;
using System.Collections.ObjectModel;

namespace Digiphoto.Lumen.Servizi.Explorer {

	public class CarrelloExplorerSrvImpl : ServizioImpl, ICarrelloExplorerSrv
	{

		#region Proprietà

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( CarrelloExplorerSrvImpl ) );

		public ICollection<Carrello> carrelli
		{
			get;
			private set;
		}

		public void setCarrelloCorrente( Guid id ) {			
			carrelloCorrente = carrelli.Single( c => c.id == id );
		}

		public Carrello carrelloCorrente { get; set; }

		#endregion

		public CarrelloExplorerSrvImpl() : base() {
			carrelli = new Collection<Carrello>();
		}

		/** Eseguo il caricamento dei carrelli richiesti */
		public void cercaCarrelli( ParamCercaCarrello param ) {

			// Per prima cosa azzero il carrello corrente
			this.carrelli = null;

			_giornale.Debug( "ricerco il carrello: " + param );

			using( IRicercatoreSrv ricercaSrv = LumenApplication.Instance.creaServizio<IRicercatoreSrv>() ) {
				ricercaSrv.start();
				carrelli = ricercaSrv.cerca( param );

				_giornale.Debug( "trovati: " + (carrelli == null ? -1 : carrelli.Count) );
			}
		}

		protected override void Dispose( bool disposing ) {

			base.Dispose( disposing );
		}

	}
}
