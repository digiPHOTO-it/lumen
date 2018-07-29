using Digiphoto.Lumen.SelfService.WebUI.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Digiphoto.Lumen.SelfService.WebUI.Controllers {

	

	public class HomeController : Controller {

		
		private Nullable<bool> _esisteLogo = null;
		private byte[] imageLogo {
			get;
			set;
		}


		public ActionResult GetLogo() {

			if( _esisteLogo == null ) {

				using( SelfServiceClient selfServiceClient = new SelfServiceClient() ) {

					selfServiceClient.Open();

					imageLogo = selfServiceClient.getImageLogo();

					selfServiceClient.Close();
				}

				_esisteLogo = (imageLogo != null);
			}

			if( _esisteLogo == true )
				return File( imageLogo, "image/png" );
			else
				return null;
		}



		public ActionResult Index() {

			return View();
		}

		public ActionResult About() {
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact() {
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}