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
        private IList<StampanteInstallata> listStampantiInstallate = null;

        public StampantiInstallateSrvImpl(){
            listStampantiInstallate = new List<StampanteInstallata>();
            List<ManagementObject> arrayStampantiInstallate = new List<ManagementObject>();
            ManagementScope objScope = new ManagementScope(ManagementPath.DefaultPath); //For the local Access
            objScope.Connect();

            SelectQuery selectQuery = new SelectQuery();
            selectQuery.QueryString = "Select * from win32_Printer";
            ManagementObjectSearcher MOS = new ManagementObjectSearcher(objScope, selectQuery);
            ManagementObjectCollection MOC = MOS.Get();
            foreach (ManagementObject mo in MOC)
            {
                try
                {
                    listStampantiInstallate.Add(new StampanteInstallata(mo["Name"].ToString(), " [" + mo["Location"].ToString().ToUpper() + "]"));
                }
                catch (NullReferenceException ex)
                {
                    listStampantiInstallate.Add(new StampanteInstallata(mo["Name"].ToString(), " [...]"));
                }
            }
        }

        public StampanteInstallata stampanteInstallataByString(String nomeStampante)
        {
            foreach(StampanteInstallata stampanteInstallata in listStampantiInstallate)
            {
                if (stampanteInstallata.NomeStampante.Equals(nomeStampante))
                {
                    return stampanteInstallata;
                }
            }
            return null;
        }

        public IList<StampanteInstallata> listaStampantiInstallate()
        {
            return listStampantiInstallate;
        }
    }
}
