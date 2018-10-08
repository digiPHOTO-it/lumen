using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Model.Util
{
	public class CompNumFoto
	{
		private const string CharList = "0123456789ABCDEFGHIJKLMNPQRSTUVWXYZ";

		public static String getStringValue(long input)
		{
			if (input < 0) throw new ArgumentOutOfRangeException("input", input, "input cannot be negative");

			char[] clistarr = CharList.ToCharArray();
			var result = new Stack<char>();
			while (input != 0)
			{
				result.Push(clistarr[input % CharList.Length]);
				input /= CharList.Length;
			}
			return new string(result.ToArray()).ToUpper();
		}

		public static Int64 getLongValue(string input)
		{
			var reversed = input.ToLower().Reverse();
			long result = 0;
			int pos = 0;
			foreach (char c in reversed)
			{
				result += CharList.IndexOf(c) * (long)Math.Pow(CharList.Length, pos);
				pos++;
			}
			return result;
		}

		public static int getIntValue( string input ) {
			var reversed = input.ToLower().Reverse();
			int result = 0;
			int pos = 0;
			foreach( char c in reversed ) {
				result += CharList.IndexOf( c ) * (int)Math.Pow( CharList.Length, pos );
				pos++;
			}
			return result;
		}

		public static string getRandomString( int len ) {

			StringBuilder buf = new StringBuilder();
			Random rnd = new Random();
			int pos;
			do {
				pos = rnd.Next( 0, CharList.Length-1 );
				buf.Append( CharList[pos] );
			} while( buf.Length < len );

			return buf.ToString();
		}
			 
	}
}
