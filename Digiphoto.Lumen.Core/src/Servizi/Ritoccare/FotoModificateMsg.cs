using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Ritoccare {


	/// <summary>
	///  Mi indica che una o più foto sono state modificate
	///  e quindi, chi le ha in pancia le deve rinfrescare
	/// </summary>
	public class FotoModificateMsg : Messaggio {

		public FotoModificateMsg( object sender, Fotografia f ) : this( sender ) {
			add( f );
		}

		public FotoModificateMsg( object sender ) : base( sender ) {
			this.fotos = new List<Fotografia>();
		}

		public List<Fotografia> fotos {
			get;
			set;
		}

		public void add( Fotografia f ) {
			fotos.Add( f );
		}
	}

}
