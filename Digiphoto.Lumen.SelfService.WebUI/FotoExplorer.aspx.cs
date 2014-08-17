using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Digiphoto.Lumen.SelfService.WebUI {


	public partial class FotoExplorer : System.Web.UI.Page {

		#region session properties

		public int numPagina {
			get {
				return (int)Session["numPagina"];
			}
			set {
				Session["numPagina"] = value;
			}
		}

		public List<FotografiaDto> listaFotografieDto {
			get {
				return (List<FotografiaDto>)Session["listaFotografieDto"];
			}
			set {
				Session["listaFotografieDto"] = value;
			}
		}

		public int indexFotoCorrente {
			get {
				return (int)Session["indexFotoCorrente"];
			}
			set {
				Session["indexFotoCorrente"] = value;
			}
		}

		#endregion



		public FotografiaDto fotoCorrente {
			get {
				return (listaFotografieDto == null || indexFotoCorrente < 0) ? null : listaFotografieDto[indexFotoCorrente];
			}
		}
		
		protected void Page_Load( object sender, EventArgs e ) {

			if( listaFotografieDto == null ) {
				// Inizializzazione. Parto dal primo risultato
				numPagina = 0;
				spostamentoAvanti();
			}

		}

		string baseAddress = "http://tirannos:9000/";  // TODO parametrizzare

		public string urlImmagineCorrente {
			get {
				return fotoCorrente == null ? null : baseAddress + "api/selfservice/" + fotoCorrente.id.ToString() + "/image";
			}
		}

		protected void buttonNext_Click( object sender, EventArgs e ) {
			spostamentoAvanti();
			Page.DataBind();
		}

		protected void buttonPrev_Click( object sender, EventArgs e ) {
			spostamentoIndietro();
		}

		/// <summary>
		/// Mi sposto di una foto.
		/// </summary>
		/// <param name="direz">+1 = foto successiva ; -1 = foto precedente </param>
		void spostamentoAvanti() {

			if( listaFotografieDto != null && indexFotoCorrente < listaFotografieDto.Count - 1 ) {
				++indexFotoCorrente;
			} else {
				if( caricaPaginaCacheFoto( +1 ) ) {
					// Devo chiamare la prossima pagina
					indexFotoCorrente = 0;
				} else {
					// Non ci sono più pagine. Rimango fermo
				}
			}
			Page.DataBind();
		}

		void spostamentoIndietro() {

			if( listaFotografieDto != null && indexFotoCorrente > 0 ) {
				--indexFotoCorrente;
			} else {
				if( caricaPaginaCacheFoto( -1 ) ) {
					// Devo chiamare la prossima pagina
					indexFotoCorrente = listaFotografieDto.Count-1;  // ultima della pagina precedente (sto andando indietro)
				} else {
					// Non ci sono più pagine. Rimango fermo
				}
			}
			Page.DataBind();
		}



		bool caricaPaginaCacheFoto( int direz ) {

			bool ret = false;
			int proxPagina = numPagina + direz;

			HttpClient client = new HttpClient();
			String reqUrl = baseAddress + "api/selfservice/fotografie/" + proxPagina;
			HttpResponseMessage response = client.GetAsync( reqUrl ).Result;
			if( response.StatusCode == HttpStatusCode.OK ) {
				numPagina = proxPagina;
				listaFotografieDto = response.Content.ReadAsAsync<List<FotografiaDto>>().Result;
				ret = true;
			} else {
				// TODO
			}

			return ret;
		}


	}
}