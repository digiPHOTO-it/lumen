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
		private static String userConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".Digiphoto");

		private static String userConfigFilePath = userConfigPath + @"\user.config"; 

		private static UserConfigXML userConfigXML = null;

		private XmlDocument myXmlDocument = null;

		private Dictionary<string, string> dictionary =  new Dictionary<string, string>();

		private UserConfigXML()
		{
			//Controllo se ce il Config File e in caso contrario lo creo
			createUserConfigFile();

			myXmlDocument = new XmlDocument();
			if (userConfigFilePath.Equals(""))
			{
				Environment.Exit(0);
			}
			myXmlDocument.Load(userConfigFilePath);

			//Carico il Dizzionario
			loadDictionary();
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

		private void loadDictionary()
		{
			XmlNode node = myXmlDocument.DocumentElement;

			foreach (XmlNode node1 in node.ChildNodes)
			{
				foreach (XmlNode node2 in node1.ChildNodes)
				{
					foreach (XmlNode node3 in node2.ChildNodes)
					{
						foreach (XmlNode node4 in node3.Attributes)
						{
							if (!node4.InnerText.Equals("String"))
							{
								dictionary.Add(node4.InnerText, node3.InnerText);
							}
						}
					}
				}
			}
		}

		public void saveDictionary()
		{
			foreach (KeyValuePair<string, string> value in dictionary)
			{
				Console.WriteLine("{0}, {1}",value.Key,value.Value);

				XmlDocument myXmlDocument = new XmlDocument();
				myXmlDocument.Load(userConfigFilePath);
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
								if (node4.InnerText.Equals(value.Key))
								{
									// value
									foreach (XmlNode node5 in node3.ChildNodes)
									{
										node5.InnerText = value.Value;
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

							attributeName.Value = value.Key;

							newNode.Attributes.Append(attributeName);

							attributeSerializeAs = node2.OwnerDocument.CreateAttribute("serializeAs");

							attributeSerializeAs.Value = "String";

							newNode.Attributes.Append(attributeSerializeAs);

							AppendChildNode(newNode, "value", value.Value);
						}
					}
				}
				myXmlDocument.Save(userConfigFilePath);
			}
		}

        public String getPropertiesValue(String properties)
        {
            XmlNode node = myXmlDocument.DocumentElement;

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

        public void setPropertiesValue(String properties, String value)
        {
			setPropertiesValueDictionary(properties,value);
			XmlDocument myXmlDocument = new XmlDocument();
			myXmlDocument.Load(userConfigFilePath);
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
			myXmlDocument.Save(userConfigFilePath);
        }

		public String getPropertiesValueDictionary(String properties)
		{
			String ret = null;
			dictionary.TryGetValue(properties,out ret);
			return ret;
		}

		public void setPropertiesValueDictionary(String properties, String value)
		{
			dictionary[properties]=value;
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
