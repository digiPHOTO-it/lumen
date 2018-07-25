using Digiphoto.Lumen.SelfService.WebUI.ServiceReference1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Digiphoto.Lumen.SelfService.WebUI.Models {

	public class Paniere {

		public CarrelloDto carrelloDto {
			get;
			set;
		}

		public FotografiaDto[] listaFotografieDto {
			get;
			set;
		}

		public FileInfo[] fileinfoFoto;
	}
}