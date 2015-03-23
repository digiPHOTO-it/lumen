using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Digiphoto.Lumen.SelfService.WebUI {

	public partial class RicercaFotografo : System.Web.UI.Page {

		public List<FotografoDto> listaFotografiDto {
			get {
				return (List<FotografoDto>)Session["listaFotografiDto"];
			}
			set {
				Session["listaFotografiDto"] = value;
			}
		}

		protected void Page_Load( object sender, EventArgs e ) {

			if( !IsPostBack ) {
				if( listaFotografiDto == null )
					caricaFotografi();

				DataBind();
			}
		}

		protected void linkButtonNavIndietro_Click( object sender, EventArgs e ) {
			navigaIndietro();
		}

		protected void linkButtonNavAvanti_Click( object sender, EventArgs e ) {
			paramRicerca.idFotografo = null;
			navigaAvanti();
		}

		HttpClient _httpClient = new HttpClient();

		public string getImage( string idFotografo ) {
			return Util.baseAddress + "/api/fotografi/" + idFotografo + "/immagine";
		}

		private void navigaIndietro() {
			Response.Redirect( "Default.aspx" );
		}

		private void navigaAvanti() {
			Response.Redirect( "~/RicercaGiorno.aspx" );
		}

		bool caricaFotografi() {

			// Devo caricare la lista dei fotografi
			bool ret = false;

			string url = Util.baseAddress + "/api/fotografi";
			HttpResponseMessage response = _httpClient.GetAsync( url ).Result;
			if( response.StatusCode == HttpStatusCode.OK ) {
				listaFotografiDto = response.Content.ReadAsAsync<List<FotografoDto>>().Result;
				ret = true;
			} else {
				// TODO
			}

			return ret;
		}

		protected void ImageFotografo_Click( object sender, ImageClickEventArgs e ) {

			ImageButton imageButton = (ImageButton)sender;
//			ParamRicerca p = (ParamRicerca)Session["paramRicerca"];
//			p.idFotografo = imageButton.CommandArgument;
			paramRicerca.idFotografo = imageButton.CommandArgument;
			navigaAvanti();
		}

		public ParamRicerca paramRicerca {
			get {
				return (ParamRicerca)Session["paramRicerca"];
			}
		}

	}
}