
using System;

namespace Digiphoto.Lumen.OnRide.UI.Config
{
    public static class UserConfigSerializer 
    {

		private static String userConfigFileName =  @"\onride-user.config"; 

		public static void serializeToFile( UserConfigOnRide userConfig)
		{
			Lumen.Config.ConfigSerializer.serializeToFile<UserConfigOnRide>( userConfig, userConfigFileName );
		}

		public static UserConfigOnRide deserialize()
		{
			return Lumen.Config.ConfigSerializer.deserialize<UserConfigOnRide>( userConfigFileName );
		}

		public static bool esisteUserConfig
		{
			get
			{
				return Lumen.Config.ConfigSerializer.esisteConfig( userConfigFileName );
			}
		}

    }
}
