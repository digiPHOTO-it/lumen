using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Vendere {


	/// <summary>
	///  Mi indica che sono stata aggiunte delle foto nel carrello
	///  devo ricaricare l'interfaccia
	/// </summary>
	public class GestoreCarrelloMsg : Messaggio
	{
		public enum Fase
		{
			UpdateCarrello,
			LoadCarrelloSalvato,
			ErroreMasterizzazione
		};

		public GestoreCarrelloMsg( object sender ) : base( sender ) {
		}

		public Fase fase { get; set; }
	}

}
