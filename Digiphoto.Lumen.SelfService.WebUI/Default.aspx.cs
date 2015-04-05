using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Digiphoto.Lumen.SelfService.WebUI {
	public partial class Index : System.Web.UI.Page {

		protected override void InitializeCulture() {

			UICulture = Util.ImpostaLingua( Session ); // la prendo dalla sessione
			base.InitializeCulture();
		}

		protected void ImageButtonFlag_Click( object sender, ImageClickEventArgs e ) {
			string lang = ((ImageButton)sender).CommandArgument;
			UICulture = Util.ImpostaLingua( Session, lang );
			Response.Redirect( "Default.aspx" );
		}


		protected void linkButtonNavAvanti_Click( object sender, EventArgs e ) {
			naviagaAvanti();
		}

		void naviagaAvanti() {
			// Azzero i parametri di ricerca
			paramRicerca = new ParamRicerca();
			Response.Redirect( "~/RicercaFotografo.aspx" );
		}

		public ParamRicerca paramRicerca {
			get {
				return (ParamRicerca)Session["paramRicerca"];
			}
			set {
				Session["paramRicerca"] = value;
			}
		}

	}
}