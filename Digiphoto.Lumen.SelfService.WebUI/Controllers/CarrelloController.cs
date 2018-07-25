using Digiphoto.Lumen.SelfService.WebUI.Models;
using Digiphoto.Lumen.SelfService.WebUI.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Digiphoto.Lumen.SelfService.WebUI.Controllers
{
    public class CarrelloController : Controller
    {

		private SelfServiceClient _selfServiceClient;
		public SelfServiceClient selfServiceClient {

			get {
				if( _selfServiceClient == null ) {
					_selfServiceClient = new SelfServiceClient();
					_selfServiceClient.Open();
				}
				return _selfServiceClient;
			}
		}



		// GET: Carrello
		public ActionResult Index()
        {
            return View();
        }

		public ActionResult DownloadFoto( Guid id ) {

			// selfServiceClient.downloadZipLongOperation( id, null );

			return View();
		}

		// GET: Carrello/Details/5
		public ActionResult Details( Guid id )
        {
			Paniere paniere = new Paniere();

// 			paniere.carrelloDto = selfServiceClient.getCarrello( id );
			
			paniere.listaFotografieDto = selfServiceClient.getListaFotografie( id );


			int contaRighe = paniere.listaFotografieDto.Length;
			paniere.fileinfoFoto = new System.IO.FileInfo[contaRighe];

				
			Session["paniere"] = paniere;

			return View( "Details", paniere );
        }

        // GET: Carrello/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Carrello/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Carrello/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Carrello/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Carrello/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Carrello/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
