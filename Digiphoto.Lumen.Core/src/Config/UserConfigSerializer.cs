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
    public static class UserConfigSerializer
    {
		//Calcolo il percorso in cui vengono memorizzati i settaggi utente
		private static String userConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Configurazione.companyName, Configurazione.applicationName );

		private static String userConfigFilePath = userConfigPath + @"\user.config"; 

		public static void serializeToFile( UserConfigLumen userConfig )
		{
			if( Directory.Exists( userConfigPath ) == false )
				Directory.CreateDirectory( userConfigPath );

			TextWriter writer = new StreamWriter(userConfigFilePath, false);
			XmlSerializer x = new XmlSerializer( typeof(UserConfigLumen) );
			x.Serialize( writer, userConfig );
			writer.Close();
		}

		public static UserConfigLumen deserialize()
		{
			UserConfigLumen userConfigXML = null;

			if( esisteUserConfig ) {

				// A FileStream is needed to read the XML document.
				XmlSerializer x = new XmlSerializer( typeof( UserConfigLumen ) );
				FileStream fs = new FileStream( userConfigFilePath, FileMode.Open );
				userConfigXML = (Config.UserConfigLumen)x.Deserialize( fs );
				fs.Close();
			} 

			return userConfigXML;
		}

		/// <summary>
		///  Mi dice soltanto se il file con la configurazione utente è presente su disco
		/// </summary>
		public static bool esisteUserConfig {
			get {
				return File.Exists( userConfigFilePath );
			}
		}
    }
}
