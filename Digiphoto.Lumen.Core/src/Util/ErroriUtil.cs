using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Util {

	public static class ErroriUtil {


		public static string estraiMessage( Exception ee ) {
			string msg = null;
			do {
				if( ee.InnerException == null )
					msg = ee.Message;
				else
					ee = ee.InnerException;
			} while( msg == null );
			return msg;
		}
	}
}
