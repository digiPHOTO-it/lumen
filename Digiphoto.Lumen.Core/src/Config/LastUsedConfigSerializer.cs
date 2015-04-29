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
    public static class LastUsedConfigSerializer
    {
		private static String lastUsedConfigFileName = @"\lastUsed.config"; 

		public static void serializeToFile(LastUsedConfigLumen lastUsedConfig)
		{
			ConfigSerializer.serializeToFile<LastUsedConfigLumen>(lastUsedConfig, lastUsedConfigFileName);
		}

		public static LastUsedConfigLumen deserialize()
		{
			return ConfigSerializer.deserialize<LastUsedConfigLumen>(lastUsedConfigFileName);
		}

		public static bool esisteLasetUsedConfig
		{
			get
			{
				return ConfigSerializer.esisteConfig(lastUsedConfigFileName);
			}
		}
    }
}
