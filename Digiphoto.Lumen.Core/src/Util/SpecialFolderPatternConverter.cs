using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Util {
	
	public class SpecialFolderPatternConverter : log4net.Util.PatternConverter {
		
		override protected void Convert(System.IO.TextWriter writer, object state)  {

			string quale = base.Option;

			string cartella;
			if( quale.Equals( "Temp" ) )
				cartella = System.IO.Path.GetTempPath();
			else { 
				Environment.SpecialFolder specialFolder = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder),base.Option, true);
				cartella = Environment.GetFolderPath( specialFolder );
			}
		    writer.Write( cartella );
		}
  }
}

