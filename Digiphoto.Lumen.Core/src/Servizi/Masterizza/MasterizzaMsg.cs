﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.Masterizzare
{

    public enum Fase
    {
		Attesa,
        InizioCopia,
        CopiaCompletata,
		ErroreMedia,
		ErroreSpazioDisco
    };

    public class MasterizzaMsg : Messaggio
    {
		public MasterizzaMsg( object sender ) : base( sender ) {
		}

        public Fase fase { get; set; }
        public int fotoAggiunta { get; set; }
        public int totFotoAggiunte { get; set; }
        public int totFotoNonAggiunte { get; set; }
        public string result { get; set; }
        public int progress { get; set; }
		public string cartella { get; set; }
	}
}
