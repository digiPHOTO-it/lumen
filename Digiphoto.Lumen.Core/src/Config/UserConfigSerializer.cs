using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Digiphoto.Lumen.Config;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Config
{
    public static class UserConfigSerializer 
    {

		private static String userConfigFileName =  @"\user.config"; 

		public static void serializeToFile( UserConfigLumen userConfig)
		{
			ConfigSerializer.serializeToFile<UserConfigLumen>(userConfig, userConfigFileName);
		}

		public static UserConfigLumen deserialize()
		{
			return ConfigSerializer.deserialize<UserConfigLumen>(userConfigFileName);
		}

		public static bool esisteUserConfig
		{
			get
			{
				return ConfigSerializer.esisteConfig(userConfigFileName);
			}
		}

    }
}
