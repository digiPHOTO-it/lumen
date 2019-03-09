using Digiphoto.Lumen.SelfService.WebUI.Models;
using Digiphoto.Lumen.SelfService.WebUI.ServiceReference1;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Mvc;

namespace Digiphoto.Lumen.SelfService.WebUI.Controllers
{
    public class CarrelloController : Controller
    {
		#region Proprietà

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

		#endregion Proprietà


		/// <summary>
		/// Scarico tutte le foto con uno zip.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// 
		// GET: Carrello/ScaricareZip/5
		[HttpGet]
		public ActionResult ScaricareZip( Guid id ) {

			Paniere paniere = (Paniere) Session["paniere"];

			if( paniere.carrelloDto.id != id )
				throw new InvalidOperationException( "sessione non valida" );

			List<FileInfo> filesDaZippare = new List<FileInfo>();

			foreach( FotografiaDto f in paniere.listaFotografieDto ) {

				byte [] bytes = selfServiceClient.getImage( f.id );

				String fullName = Path.Combine( System.IO.Path.GetTempPath(), "foto-" + f.id + ".jpg" );

				if( ! filesDaZippare.Any( i => i.FullName == fullName ) ) {
					System.IO.File.WriteAllBytes( fullName, bytes );
					filesDaZippare.Add( new FileInfo( fullName ) );
				}
			}

			if( filesDaZippare.Count == 0 ) {
				return View( "Error", "Non trovata nessuna immagine da zippare" );
			}

			// Creo lo zip
			String zipName = Path.Combine( Path.GetTempPath(), "carrello-" + id + ".zip" );
			if( System.IO.File.Exists( zipName ) )
				System.IO.File.Delete( zipName );
			using( ZipArchive zip = ZipFile.Open( zipName, ZipArchiveMode.Create ) ) {
				foreach( FileInfo finfo in filesDaZippare ) {
					zip.CreateEntryFromFile( finfo.FullName, finfo.Name );
					finfo.Delete();
				}
			}




			FileContentResult result = new FileContentResult( System.IO.File.ReadAllBytes( zipName ), "application/zip" );
			result.FileDownloadName = "carrello-" + id + ".zip";
			return result;
		}




		// GET: Carrello/Details/5
		public ActionResult Details( String id )
        {
			CarrelloDto dto = null;
			Guid guid;
			bool isGuid = Guid.TryParse( id, out guid );

			if( isGuid )
				dto = selfServiceClient.getCarrello( guid );
			else
				dto = selfServiceClient.getCarrello2( id.ToUpper() );

			if( dto == null || dto.isVenduto == false ) {
				string msg = "Carrello non trovato, oppure NON ancora venduto, oppure non visibile per il SelfService. ID = " + id;
				return View( "Error", model:msg );   // questa sintassi serve a risolvere un problema: esiste un overload con 2 stringhe: https://stackoverflow.com/questions/18273416/the-view-or-its-master-was-not-found-or-no-view-engine-supports-the-searched-loc#31245642
			}

			FotografiaDto [] lista = selfServiceClient.getListaFotografie( dto.id );

			// creo il modello per visualizzare la view
			var paniere = new Paniere {
				carrelloDto = dto,
				listaFotografieDto = lista,
				fileinfoFoto = new System.IO.FileInfo[lista.Length]
			};
				
			Session["paniere"] = paniere;

			return View( "Details", paniere );
        }

		[HttpGet]
		public ActionResult GetImage( Guid fotografiaId ) {

			using( SelfServiceClient selfServiceClient = new SelfServiceClient() ) {

				selfServiceClient.Open();

				byte[] imageFoto = selfServiceClient.getImage( fotografiaId );

				selfServiceClient.Close();

				FileContentResult result = new FileContentResult( imageFoto, "image/jpg" );
				result.FileDownloadName = "foto-" + fotografiaId + ".jpg";
				return result;
			}
		}
	}
}
