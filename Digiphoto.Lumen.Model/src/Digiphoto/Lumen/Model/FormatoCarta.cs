﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( FormatoCarta ) )]
	public class FormatoCarta : Prodotto {

		public FormatoCarta() {
			tipologia = RigaCarrello.TIPORIGA_STAMPA;
		}

		/// <summary>
		/// P = piccolo
		/// M = medio
		/// G = grande
		/// 
		/// serve per il calcolo delle promozioni
		/// </summary>
		[Column("st_grandezza")]
		[Required]
		public string grandezza {
			get; set;
		}
	}

}
