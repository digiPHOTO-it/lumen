using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace Digiphoto.Lumen.Util
{
    public class UserConfigXML
    {

        public static String PathUserConfigLumen
        {
            get
            {
                return pathUserConfigLumen();
            }
        }

        public static String PathUserConfigConfiguratore
        {
            get
            {
                return pathUserConfigConfiguratore();
            }
        }

        private static String pathUserConfigLumen()
        {
            //Calcolo il percorso in cui vengono memorizzati i settaggi utente
            String userConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            String userConfigFilePath = userConfigPath + @"\digiPHOTO.it\";

            String filePath = "";

            if (!Directory.Exists(userConfigFilePath))
            {
                return filePath;
            }

            String[] listUserConfigFilePath = Directory.GetDirectories(userConfigFilePath);

            foreach (String path in listUserConfigFilePath)
            {
                String dirName = Path.GetFileName(path);
                // Filtro su Digiphoto.Lumen.UI potrebbe essere necessario filtrare sulla data di creazione
                // ma fose con un MSI installer non serve; se cambio la versione del programma devo cambare 1.0.0.0
                if (dirName.Substring(0, 18).Equals("Digiphoto.Lumen.UI"))
                {
                    filePath = path + @"\1.0.0.0\user.config";
                }
            }
            return filePath;
        }

        private static String pathUserConfigConfiguratore()
        {
            //Calcolo il percorso in cui vengono memorizzati i settaggi utente
            String userConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            String userConfigFilePath = userConfigPath + @"\digiPHOTO.it\";

            String filePath = "";

            if (!Directory.Exists(userConfigFilePath))
            {
                return filePath;
            }

            String[] listUserConfigFilePath = Directory.GetDirectories(userConfigFilePath);

            foreach (String path in listUserConfigFilePath)
            {
                String dirName = Path.GetFileName(path);
                // Filtro su Digiphoto.Lumen.UI potrebbe essere necessario filtrare sulla data di creazione
                // ma fose con un MSI installer non serve; se cambio la versione del programma devo cambare 1.0.0.0

                if (dirName.Substring(0, 25).Equals("Digiphoto.Lumen.GestoreCo"))
                {
                    filePath = path + @"\1.0.0.0\user.config";
                }
            }
            return filePath;
        }

        public static String getPropertiesValue(String file, String properties)
        {
            XmlDocument myXmlDocument = new XmlDocument();
            if (file.Equals(""))
            {
                //MessageBox.Show("Devi eseguire Lumen prima", "Avviso");
                Environment.Exit(0);
            }
            myXmlDocument.Load(file);

            XmlNode node;
            node = myXmlDocument.DocumentElement;

            foreach (XmlNode node1 in node.ChildNodes)
            {
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    foreach (XmlNode node3 in node2.ChildNodes)
                    {
                        foreach (XmlNode node4 in node3.Attributes)
                        {
                            if (node4.InnerText.Equals(properties))
                            {
                                return node3.InnerText;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static void setPropertiesValue(String file, String properties, String value)
        {
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(file);

            XmlNode node;
            // configuration
            node = myXmlDocument.DocumentElement;
            // userSettings
            foreach (XmlNode node1 in node.ChildNodes)
            {
                // Digiphoto.Lumen.Properties.Settings
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    // setting
                    foreach (XmlNode node3 in node2.ChildNodes)
                    {
                        // setting name=
                        foreach (XmlNode node4 in node3.Attributes)
                        {
                            if (node4.InnerText.Equals(properties))
                            {
                                // value
                                foreach (XmlNode node5 in node3.ChildNodes)
                                {
                                    node5.InnerText = value;
                                }
                            }
                        }
                    }
                }
            }
            myXmlDocument.Save(file);
        }
    }
}
