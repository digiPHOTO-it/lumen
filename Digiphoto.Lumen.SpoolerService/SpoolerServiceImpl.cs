using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Servizi.Vendere;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Digiphoto.Lumen.Services {

	public partial class SpoolerServiceImpl : ISpoolerService {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( SpoolerServiceImpl ) );


		public SpoolerServiceImpl() {

#if DEBUG
			// Siccome in debug mi avvalgo del truschino di Visual Studio per avviare il servizio, faccio questo trucco solo per il debug.
			// Normalmente deve essere l'applicazione Host che avvia e termina l'infrastruttura di Lumen
			if( LumenApplication.Instance.avviata == false ) {
				LumenApplication.Instance.avvia();
			}
#endif
		}

		~SpoolerServiceImpl() {

#if DEBUG
			// Siccome in debug mi avvalgo del truschino di Visual Studio per avviare il servizio, faccio questo trucco solo per il debug.
			// Normalmente deve essere l'applicazione Host che avvia e termina l'infrastruttura di Lumen
			if( LumenApplication.Instance.avviata == true ) {
				LumenApplication.Instance.ferma();
			}
#endif
		}

		// List<IVenditoreSrv> venditori = new List<IVenditoreSrv>();

		public void EseguireStampe( char td, Guid guid ) {

			try {

				using( new UnitOfWorkScope() ) {

					using( IVenditoreSrv venditore = LumenApplication.Instance.creaServizio<IVenditoreSrv>() ) {

						if( td == 'C' ) {
							venditore.RistampareCarrello( guid );
						} else if( td == 'R' ) {
							venditore.RistampareRigaCarrello( guid );
						} else
							throw new InvalidOperationException( "Param td invalido = " + td );
					}
				}

			} catch( Exception ee ) {
				_giornale.Error( ee );
				throw ee;
			}

		}



		public String About() {
			var xx = typeof( SpoolerServiceImpl ).Assembly.GetName();
			return xx.Name + " ver " + xx.Version;
		}

	}

}
