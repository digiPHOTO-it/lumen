using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Config
{
    
    public class ConfigurazioneUserConfigLumen
    {
        private static String pathUserConfig = UserConfigXML.PathUserConfigLumen;

        public static String UserConfigConnectionString
        {
            get
            {
                return UserConfigXML.getPropertiesValue(pathUserConfig, "connectionString");
            }

            set
            {
                UserConfigXML.setPropertiesValue(pathUserConfig, "connectionString", value);
            }
        }

        public static bool PrimoAvvioLumen
        {
            get
            {
                return Boolean.Parse(UserConfigXML.getPropertiesValue(pathUserConfig, "primoAvvio"));
            }
            set
            {
                UserConfigXML.setPropertiesValue(pathUserConfig, "primoAvvio", ""+value);
            }
        }

        public static String stampantiAbbinate
        {
            get
            {
                return UserConfigXML.getPropertiesValue(pathUserConfig, "stampantiAbbinate");
            }
            set
            {
                UserConfigXML.setPropertiesValue(pathUserConfig, "stampantiAbbinate", value);
            }
        }

        public static String dbNomeDbVuoto
        {
            get
            {
                return UserConfigXML.getPropertiesValue(pathUserConfig, "dbNomeDbVuoto");
            }
            set
            {
                UserConfigXML.setPropertiesValue(pathUserConfig, "dbNomeDbVuoto", value);
            }
        }

        public static String dbNomeDbPieno
        {
            get
            {
                return UserConfigXML.getPropertiesValue(pathUserConfig, "dbNomeDbPieno");
            }
            set
            {
                UserConfigXML.setPropertiesValue(pathUserConfig, "dbNomeDbPieno", value);
            }
        }

        public static String dbCartella
        {
            get
            {
                return UserConfigXML.getPropertiesValue(pathUserConfig, "dbCartella");
            }
            set
            {
                UserConfigXML.setPropertiesValue(pathUserConfig, "dbCartella", value);
            }
        }
        


    }
}
