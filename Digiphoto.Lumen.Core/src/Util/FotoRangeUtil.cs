using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model.Util;
using Digiphoto.Lumen.Config;
using System.Collections;

namespace Digiphoto.Lumen.UI.Util
{
	public class FotoRangeUtil
	{

		public static int[] rangeToString(string value)
		{
			List<int> range = new List<int>();

			try
			{

				if (Configurazione.UserConfigLumen.compNumFoto)
				{
					if (value.Contains('-'))
					{
						IEnumerable<int> v = ((string)value).Split('-').Select(nn => CompNumFoto.getIntValue(nn) );
						return v.ToArray<int>();
					}
					else
					{
						IEnumerable<int> v = ((string)value).Split(',').Select(nn => CompNumFoto.getIntValue(nn) );
						return v.ToArray<int>();
					}
				}
				else
				{
					if (value.Contains('-'))
					{
						return value.Split('-').Select(nn => (nn.Equals("") ? 0 : Convert.ToInt32(nn))).ToArray();
					} 
					else
					{
						return value.Split(',').Select(nn => (nn==null ? 0 : Convert.ToInt32(nn))).ToArray();
					}
				}

			}
			catch (Exception ee )
			{
				throw new ArgumentOutOfRangeException( "Intervallo numerico non valido", ee );
			}

		}
	}
}
