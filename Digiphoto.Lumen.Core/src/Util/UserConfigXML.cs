using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Util
{
    public class UserConfigXML
    {
		//Calcolo il percorso in cui vengono memorizzati i settaggi utente
		private static String userConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".digiPhoto");

		private static String userConfigFilePath = userConfigPath + @"\user.config"; 

		private static UserConfigXML userConfigXML = null;

		private UserConfigLumen userConfigLumen = new UserConfigLumen();

		private UserConfigXML()
		{
			Configurazione.UserConfigLumen = userConfigLumen;
			//Controllo se ce il Config File e in caso contrario lo creo
			createUserConfigFile();

			if (userConfigFilePath.Equals(""))
			{
				Environment.Exit(0);
			}

			//Carico il file di configurazione
			loadConfigFromXml();
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

		public void SaveUserConfig()
		{
			TextWriter writer = new StreamWriter(userConfigFilePath);
			System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(userConfigLumen.GetType());
			x.Serialize(writer, Configurazione.UserConfigLumen);
			writer.Close();
		}

		private void createUserConfigFile()
		{
			if (!Directory.Exists(userConfigPath))
			{
				Directory.CreateDirectory(userConfigPath);
				SaveUserConfig();
			}
			else
			{
				if (!File.Exists(userConfigFilePath))
				{
					SaveUserConfig();
				}
			}
		}

		private void loadConfigFromXml()
		{
			// A FileStream is needed to read the XML document.
			System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(userConfigLumen.GetType());
			FileStream fs = new FileStream(userConfigFilePath, FileMode.Open);
			Config.UserConfigLumen userConfigXML = (Config.UserConfigLumen)x.Deserialize(fs);
			fs.Close();
			Configurazione.UserConfigLumen = userConfigXML;
		}
    }
}
