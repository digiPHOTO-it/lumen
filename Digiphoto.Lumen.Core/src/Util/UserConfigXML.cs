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
		//Calcolo il percorso in cui vengono memorizzati i settaggi utente
		private static String userConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),".Digiphoto");

		private static String userConfigFilePath = userConfigPath + @"\user.config"; 

		private static UserConfigXML userConfigXML = null;

		private UserConfigXML()
		{
			createUserConfigFile();
		}

		public static UserConfigXML Instance
		{
			get 
			{
				if (userConfigXML == null)
				{
					userConfigXML = new UserConfigXML();
				}
				return userConfigXML;
			}
		}

		private static void createUserConfigFile()
		{
			if (!Directory.Exists(userConfigPath))
			{
				Directory.CreateDirectory(userConfigPath);
				creaXmlFile();
			}
			else
			{
				if (!File.Exists(userConfigFilePath)) 
				{
					creaXmlFile();
				}
			}
		}

        public String getPropertiesValue(String file, String properties)
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

		public String getPropertiesValue(String properties)
		{
			return getPropertiesValue(userConfigFilePath, properties);
		}

        public void setPropertiesValue(String file, String properties, String value)
        {
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(file);

            XmlNode node;
            // configuration
            node = myXmlDocument.DocumentElement;

			bool addNewNode = true;

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
									addNewNode = false;
                                }
                            }
                        }
                    }

					// Non ho aggiornato quindi il nodo non esiste lo aggiungo
					if (addNewNode)
					{
						XmlNode newNode;

						XmlAttribute attributeName, attributeSerializeAs;

						newNode = AppendChildNode(node2, "setting", null);

						attributeName = node2.OwnerDocument.CreateAttribute("name");

						attributeName.Value = properties;

						newNode.Attributes.Append(attributeName);

						attributeSerializeAs = node2.OwnerDocument.CreateAttribute("serializeAs");

						attributeSerializeAs.Value = "String";

						newNode.Attributes.Append(attributeSerializeAs);

						AppendChildNode(newNode, "value", value);
					}
                }
            }
	
            myXmlDocument.Save(file);
        }

		public void setPropertiesValue(String properties, String value)
		{
			setPropertiesValue(userConfigFilePath, properties, value); 
		}

		private static void creaXmlFile()
		{
			// Create XML document

			XmlDocument doc = new XmlDocument();

			// Create and attach root node

			XmlNode configurationNode = doc.CreateElement("configuration");

			doc.AppendChild(configurationNode);

			// Create and attach version

			XmlNode version = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");

			doc.InsertBefore(version, configurationNode);

			XmlNode userSettingsNode = configurationNode.OwnerDocument.CreateElement("userSettings");

			configurationNode.AppendChild(userSettingsNode);

			XmlNode settingsNode = configurationNode.OwnerDocument.CreateElement("Digiphoto.Lumen.Properties.Settings");

			userSettingsNode.AppendChild(settingsNode);

			doc.Save(userConfigFilePath);
		}

		private XmlNode AppendChildNode(XmlNode Parent, string ChildName, string ChildValue)
		{
			XmlNode node = Parent.OwnerDocument.CreateElement(ChildName);

			node.InnerText = ChildValue;

			Parent.AppendChild(node);

			return node;
		}


    }
}
