using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Digiphoto.Lumen.SelfService.WebUI {

	public partial class RicercaOrario : System.Web.UI.Page {

		protected void ButtonGiorno_Click( object sender, EventArgs e ) {
			Button button = (Button)sender;
			short quantiGiorniIndietro = Convert.ToInt16( button.CommandArgument );
			paramRicerca.giorno = DateTime.Today.AddDays( quantiGiorniIndietro * (-1) );
			navigaAvanti();
		}

		protected void linkButtonNavAvanti_Click( object sender, EventArgs e ) {

			if( GiornoIniz.SelectedDate == DateTime.MinValue )
				paramRicerca.giorno = null;
			else {
				// paramRicerca.giorno = Convert.ToDateTime( GiornoIniz.Text );
				paramRicerca.giorno = GiornoIniz.SelectedDate;
			}
			navigaAvanti();
		}

		protected override void InitializeCulture() {
			UICulture = Util.ImpostaLingua( Session ); // la prendo dalla sessione
			base.InitializeCulture();
		}

		protected void linkButtonNavIndietro_Click( object sender, EventArgs e ) {
			navigaIndietro();
		}

		private void navigaIndietro() {
			Response.Redirect( "~/RicercaFotografo.aspx" );
		}

		private void navigaAvanti() {
			paramRicerca.numPagina = 0;
			Response.Redirect( "~/FotoExplorer.aspx" );
		}

		public ParamRicerca paramRicerca {
			get {
				return (ParamRicerca)Session["paramRicerca"];
			}
		}
	}
}