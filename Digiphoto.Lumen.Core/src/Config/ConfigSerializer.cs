using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Digiphoto.Lumen.Config;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Config
{
    public static class ConfigSerializer
    {
		//Calcolo il percorso in cui vengono memorizzati i settaggi utente
		private static String configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Configurazione.companyName, Configurazione.applicationName);

		public static void serializeToFile<T>(T config, string configFilePath)
		{
			if (Directory.Exists(configPath) == false)
				Directory.CreateDirectory(configPath);

			TextWriter writer = new StreamWriter(configPath + configFilePath, false);
			XmlSerializer x = new XmlSerializer(typeof(T));
			x.Serialize(writer, config);
			writer.Close();
		}

		public static T deserialize<T>(string configFilePath)
		{
			T configXML = default(T);

			if (esisteConfig(configFilePath))
			{

				// A FileStream is needed to read the XML document.
				XmlSerializer x = new XmlSerializer(typeof(T));
				FileStream fs = new FileStream(configPath + configFilePath, FileMode.Open);
				configXML = (T)x.Deserialize(fs);
				fs.Close();
			}

			return configXML;
		}

		/// <summary>
		///  Mi dice soltanto se il file con la configurazione utente è presente su disco
		/// </summary>
		public static bool esisteConfig(string configFilePath)
		{
			return File.Exists(configPath + configFilePath);
		}
    }
}
