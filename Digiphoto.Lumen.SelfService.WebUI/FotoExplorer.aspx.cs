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


	public partial class FotoExplorer : System.Web.UI.Page {

		#region Proprietà

		#region Session Properties


		public ParamRicerca paramRicerca {
			get {
				return (ParamRicerca)Session["paramRicerca"];
			}
			set {
				Session["paramRicerca"] = value;
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

		/// <summary>
		/// Acceso significa che le foto scorrono da sole.
		/// </summary>
		public bool autoPlay {
			get {
				return Session["autoPlay"] == null ? false : (bool)Session["autoPlay"];
			}
			set {
				if( Session["autoPlay"] == null || (bool)Session["autoPlay"] != value ) {
					Session["autoPlay"] = value;
					Page.DataBind();
				}
			}
		}

		#endregion Session Properties

		public FotografiaDto fotoCorrente {
			get {
				return (listaFotografieDto == null || listaFotografieDto.Count == 0 || indexFotoCorrente < 0) ? null : listaFotografieDto[indexFotoCorrente];
			}
		}


		public string urlImmagineCorrente {
			get {
				return fotoCorrente == null ? null : Util.baseAddress + "api/fotografie/" + fotoCorrente.id.ToString() + "/provino";
			}
		}

		public int totImmagini {
			get {
				return listaFotografieDto == null ? 0 : listaFotografieDto.Count;
			}
		}

		public bool possoAndareAvanti {
			get {
				// TODO dovrei sapere quante foto in tutto mi ritorna la query, ma per ora non lo so. Da fare
				return totImmagini > 0 && !autoPlay;
			}
		}

		public bool possoAndareIndietro {
			get {
				return    totImmagini > 0 
					   && (paramRicerca.numPagina > 1 || indexFotoCorrente > 0)
					   && !autoPlay;
			}
		}

		public bool possoAutoPlay {
			get {
				return totImmagini > 0;
			}
		}


		#endregion Proprietà



		protected void Page_Load( object sender, EventArgs e ) {

			if( ! IsPostBack )  {
				init();
			}

		}

		private void init() {

			listaFotografieDto = null;
			indexFotoCorrente = 0;

			// Posso arrivare su questa pagina direttamente, oppure navigando i parametri. In questo caso li trovo già pronti
			if( paramRicerca == null )
				paramRicerca = new ParamRicerca();

			// Vado sulla prima foto (se esiste)
			andareAvanti();
		}


		protected void buttonNext_Click( object sender, EventArgs e ) {
			andareAvanti();
		}

		protected void buttonPrev_Click( object sender, EventArgs e ) {
			andareIndietro();
		}

		protected void buttonHome_Click( object sender, EventArgs e ) {
			Response.Redirect( "Default.aspx" );
		}

		

		/// <summary>
		/// Mi sposto di una foto.
		/// </summary>
		/// <param name="direz">+1 = foto successiva ; -1 = foto precedente </param>
		private bool andareAvanti() {

			bool andato = false;

			if( listaFotografieDto != null && indexFotoCorrente < totImmagini - 1 ) {
				++indexFotoCorrente;
				andato = true;
			} else {
				andato = caricaPaginaCacheFoto( +1 );
				if( andato ) {
					// Devo chiamare la prossima pagina
					indexFotoCorrente = 0;
				} else {
					// Non ci sono più pagine. Rimango fermo
				}
			}
			Page.DataBind();

			return andato;
		}

		private void andareIndietro() {

			if( listaFotografieDto != null && indexFotoCorrente > 0 ) {
				--indexFotoCorrente;
			} else {
				if( caricaPaginaCacheFoto( -1 ) ) {
					// Devo chiamare la prossima pagina
					indexFotoCorrente = totImmagini-1;  // ultima della pagina precedente (sto andando indietro)
				} else {
					// Non ci sono più pagine. Rimango fermo
				}
			}
			Page.DataBind();
		}



		bool caricaPaginaCacheFoto( int direz ) {

			bool ret = false;
			int proxPagina = paramRicerca.numPagina + direz;

			StringBuilder reqUrl = new StringBuilder( Util.baseAddress );
			reqUrl.Append( "api/" );

			// Compongo l'url della richiesta
			if( paramRicerca.idFotografo != null ) {
				reqUrl.Append( "fotografi/" );
				reqUrl.Append( paramRicerca.idFotografo );
				reqUrl.Append( "/" );
			}

			reqUrl.Append( "fotografie/" );

			if( paramRicerca.giorno != null ) {
				reqUrl.Append( "giorno/" );
				reqUrl.Append( ((DateTime)paramRicerca.giorno).ToString( "yyyy-MM-dd" ) );
				reqUrl.Append( "/" );
			}

			if( proxPagina != 0 ) {
				reqUrl.Append( "pag/" );
				reqUrl.Append( proxPagina );
				reqUrl.Append( "/" );
			}

					
			HttpClient _httpClient = new HttpClient();
			HttpResponseMessage response = _httpClient.GetAsync( reqUrl.ToString() ).Result;
			if( response.StatusCode == HttpStatusCode.OK ) {
				paramRicerca.numPagina = proxPagina;
				listaFotografieDto = response.Content.ReadAsAsync<List<FotografiaDto>>().Result;
				ret = true;
			} else {
				// TODO
			}

			return ret;
		}

		protected void timerAutoPlay_Tick( object sender, EventArgs e ) {
			// Se non riesco andare avanti, mi fermo
			if( !andareAvanti() ) {
				autoPlay = false;
			}
		}

		protected void buttonAutoPlay_Click( object sender, EventArgs e ) {
			autoPlay = !autoPlay;
		}


	}
}