using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.Explorer
{

    public enum Fase
    {
		Completata,
		Errore
    };

	public class MetadatiMsg : Messaggio
    {
		public MetadatiMsg( object sender ) : base( sender ) {
		}

        public Fase fase { get; set; }
    }
}
