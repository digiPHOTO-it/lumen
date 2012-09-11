using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.Management;

namespace Digiphoto.Lumen.Servizi.Stampare
{
    public class StampantiInstallateSrvImpl : ServizioImpl, IStampantiInstallateSrv
    {

        public StampantiInstallateSrvImpl() {
			caricaStampanti();
		}


		private void caricaStampanti() {

			stampantiInstallate = new List<StampanteInstallata>();

			// Use the ObjectQuery to get the list of configured printers
			ObjectQuery oquery = new ObjectQuery( "SELECT * FROM Win32_Printer" );

			ManagementObjectSearcher mosearcher = new ManagementObjectSearcher( oquery );

			ManagementObjectCollection moc = mosearcher.Get();


			foreach( ManagementObject mo in moc ) {

				// Solo per debug
				// debugManagementObject( mo );

				string nomePorta = null;

				string nomeStampante = (string)mo ["Name"];
				try {
					nomePorta = (string)mo ["PortName"];
				} catch( Exception ) {
				}
				
				StampanteInstallata stp = StampanteInstallata.CreateStampanteInstallata( nomeStampante, nomePorta );
				stampantiInstallate.Add( stp );
			}
		}


		/// <summary>
		/// Solo per debug. MI serve per visualizzare i nomi delle proprietà dentro la mappa
		/// </summary>
		/// <param name="mo"></param>
		private void debugManagementObject( ManagementObject mo ) {
			PropertyDataCollection pdc = mo.Properties;
			foreach( PropertyData pd in pdc ) {
				string pn = pd.Name;
				object dato = pd.Value;
			}
		}

        public StampanteInstallata getStampanteInstallataByString(String nomeStampante)
        {
            foreach(StampanteInstallata stampanteInstallata in stampantiInstallate)
            {
                if (stampanteInstallata.NomeStampante.Equals(nomeStampante))
                {
                    return stampanteInstallata;
                }
            }
            return null;
        }

		public IList<StampanteInstallata> stampantiInstallate {
			get;
			private set;
		}
    }
}
