using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Core.DatiDiEsempio {
	
	internal class ParoleCasuali {
	
		public enum Varianti {
			Maiuscole = 0x01,
			Minuscole = 0x02,
			Numeri = 0x04
		}

		private Varianti _varianti;
		private int _maxLen;
		private Random _random;

		public ParoleCasuali() : this( 50 ) {
		}

		public ParoleCasuali( int maxLen ) : this( maxLen, Varianti.Maiuscole ) {
		}

		public ParoleCasuali( int maxLen, Varianti v ) {
			_maxLen = maxLen;
			_varianti = v;
			_random = new Random();
		}

		public string genera() {
			return genera( _maxLen );
		}

		public string genera( int max ) {

			StringBuilder builder = new StringBuilder();
			char ch ;

			for(int i=0; i<max; i++) {
				int q = Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65));
				ch = Convert.ToChar(q) ;
				builder.Append(ch);
			}
			return builder.ToString();
		}

	}
}
