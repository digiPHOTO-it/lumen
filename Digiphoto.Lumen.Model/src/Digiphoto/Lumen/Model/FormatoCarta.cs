using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( FormatoCarta ) )]
	public class FormatoCarta : Prodotto {

		/// <summary>
		/// P = piccolo
		/// M = medio
		/// G = grande
		/// </summary>
		[Column("st_grandezza")]
		public char grandezza {
			get; set;
		}
	}

}
