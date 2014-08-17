using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.SelfService {

	//  Esempio di definizione routing preso da qui:
	//  http://www.asp.net/web-api/overview/web-api-routing-and-actions/create-a-rest-api-with-attribute-routing

	[RoutePrefix( "api/selfservice" )]
	public class SelfServiceController : ApiController {

#if NONUSATO
		// Typed lambda expression for Select() method. 
		private static readonly Expression<Func<Fotografia, FotografiaDto>> AsFotografiaDto =
			x => new FotografiaDto {
				numero = x.numero,
				nomeFotografo = x.fotografo.cognomeNome,
				giornata = x.giornata
			};
#endif

		[Route( "fotografie/{pagina:int}" )]
		public IEnumerable<FotografiaDto> GetFotografie( int pagina ) {

			const int TAKE = 3;
			
			IFotoExplorerSrv fotoExplorerSrv = LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();

			Paginazione pag = new Paginazione {
				skip = (pagina - 1) * TAKE,
				take = TAKE
			};

			ParamCercaFoto param = new ParamCercaFoto() {
				paginazione = pag,
				idratareImmagini = false
			};

			fotoExplorerSrv.cercaFoto( param );

			List<FotografiaDto> fotografieDtos = new List<FotografiaDto>();
			foreach( Fotografia f in  fotoExplorerSrv.fotografie) {

				FotografiaDto dto = new FotografiaDto {
					id = f.id,
					numero = f.numero,
					nomeFotografo = f.fotografo.cognomeNome,
					giornata = f.giornata
				};
				fotografieDtos.Add( dto );
			}

			return fotografieDtos;
		}


		[Route( "{guid:Guid}/image" )]
		public HttpResponseMessage GetImage( Guid guid ) {

			HttpResponseMessage response = null;
			Fotografia fotografia = null;

			try {

				using( new UnitOfWorkScope() ) {
					IEntityRepositorySrv<Fotografia> fotoRepos = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografia>>();
					fotografia = fotoRepos.getById( guid );
				}

				Stream dataStream;
				long? dimensione;

				// TODO decidere quale immagine tornare: quella piccola o quella grande
				AiutanteFoto.idrataImmaginiFoto( fotografia, IdrataTarget.Provino );
				IImmagine immagine = fotografia.imgProvino;
//				IImmagine immagine = AiutanteFoto.idrataImmagineGrande( fotografia );


				byte[] bytearray = immagine.getBytes();
				dataStream = new MemoryStream( bytearray );
				dimensione = bytearray.Length;

				response = new HttpResponseMessage( HttpStatusCode.OK );
				response.Content = new StreamContent( dataStream );
				response.Content.Headers.ContentType = new MediaTypeHeaderValue( "image/jpeg" );
				response.Content.Headers.ContentLength = dimensione;

			} catch( Exception ee ) {
				Console.WriteLine( ee );
				response = new HttpResponseMessage( HttpStatusCode.InternalServerError );
			} finally {
				if( fotografia != null )
					AiutanteFoto.disposeImmagini( fotografia );
			}

			return response;
		}
		
	}
}
