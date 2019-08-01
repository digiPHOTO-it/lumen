
using System;
using System.IO;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.SelfService.SlideShow.Config
	{
    public static class UserConfigSerializer 
    {

		private static String userConfigFileName =  @"\slideshow-user.config";

		private static String configPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "digiPHOTO.it", "Lumen" );


		public static void serializeToFile( UserConfig userConfig)
		{
			serializeToFile<UserConfig>( userConfig, userConfigFileName );
		}

		public static void serializeToFile<T>( T config, string configFilePath ) {
			if( Directory.Exists( configPath ) == false )
				Directory.CreateDirectory( configPath );

			TextWriter writer = new StreamWriter( configPath + configFilePath, false );
			XmlSerializer x = new XmlSerializer( typeof( T ) );
			x.Serialize( writer, config );
			writer.Close();
		}

		public static UserConfig deserialize()
		{
			return deserialize<UserConfig>( userConfigFileName );
		}

		public static T deserialize<T>( string configFilePath ) {
			T configXML = default( T );

			if( esisteConfig( configFilePath ) ) {

				// A FileStream is needed to read the XML document.
				XmlSerializer x = new XmlSerializer( typeof( T ) );
				FileStream fs = new FileStream( configPath + configFilePath, FileMode.Open );
				configXML = (T)x.Deserialize( fs );
				fs.Close();
			}

			return configXML;
		}

		public static bool esisteConfig( string configFilePath ) {
			return File.Exists( configPath + configFilePath );
		}

		public static bool esisteUserConfig
		{
			get
			{
				return esisteConfig( userConfigFileName );
			}
		}

    }
}
