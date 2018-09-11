using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Util {

	public static class NetworkUtil {

		public static bool isInternetConnectionActive() {
			return (new Ping().Send( "www.digiphoto.it" ).Status == IPStatus.Success);
		}

	}
}
