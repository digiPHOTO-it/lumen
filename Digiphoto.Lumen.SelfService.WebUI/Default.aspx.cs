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
			impostaLingua( null ); // la prendo dalla sessione
			base.InitializeCulture();
		}

		protected void ImageButtonFlag_Click( object sender, ImageClickEventArgs e ) {
			string id = ((WebControl)sender).ID;
			string lang = id.Substring( id.Length - 2 ).ToLower();
			impostaLingua( lang );

			Response.Redirect( "Default.aspx" );
		}

		void impostaLingua( string lingua ) {
			
			if( lingua != null ) {
				Session["linguaSelezionata"] = lingua;
			}

			if( Session["linguaSelezionata"] != null ) {
				string linguaSelezionata = (string)Session["linguaSelezionata"];
				try {
					UICulture = linguaSelezionata;
					Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( linguaSelezionata );
					Thread.CurrentThread.CurrentUICulture = new CultureInfo( linguaSelezionata );
				} catch( Exception ) {
					UICulture = "it-IT";
				}
			}
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