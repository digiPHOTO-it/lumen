﻿
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( ProdottoFile ) )]
	public class ProdottoFile : Prodotto {

		public ProdottoFile() {
			this.tipologia = RigaCarrello.TIPORIGA_MASTERIZZATA;
		}
	}

}
