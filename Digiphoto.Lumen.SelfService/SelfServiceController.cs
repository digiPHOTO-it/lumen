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
using Digiphoto.Lumen.Config;
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

	[RoutePrefix( "api/fotografie" )]
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

		/// <summary>
		/// Estra tutte le foto (senza filtri) 
		/// non so se può servire ma in futuro, chissà.
		/// </summary>
		/// <example>
		/// http://localhost:9000/api/fotografie
		/// </example>
		/// <returns></returns>
		[Route( "" )]
		public IEnumerable<FotografiaDto> GetFotografie() {
			return GetFotografie( null, null, null );
		}


		/// <summary>
		/// Ricava una foto (dto) dato il suo id
		/// </summary>
		/// <example>
		/// http://localhost:9000/api/fotografie/e2bf55cc-9366-4857-b9ea-61309a846683
		/// </example>
		/// <param name="guid"></param>
		/// <returns>Un dto contenente i dati della fotografia</returns>
		[Route( "{guid:Guid}" )]
		public FotografiaDto GetFotografia( Guid guid ) {

			FotografiaDto dto = null;
			using( new UnitOfWorkScope() ) {
				IEntityRepositorySrv<Fotografia> fotoRepos = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografia>>();
				Fotografia f = fotoRepos.getById( guid );
				if( f != null )
					dto = creaFotografiaDto( f );
			}
			return dto;
		}

		/// <example>
		/// http://localhost:9000/api/fotografie/giorno/2014-12-26
		/// </example>
		[Route( "giorno/{giorno:datetime}" )]
		public IEnumerable<FotografiaDto> GetFotografie( DateTime giorno ) {
			return GetFotografie( giorno, null , null );
		}

		[Route( "giorno/{giorno:datetime}/pag/{pagina:int}" )]
		public IEnumerable<FotografiaDto> GetFotografie( DateTime giorno, int pagina ) {
			return GetFotografie( giorno, null, pagina );
		}

		/// <example>
		/// http://localhost:9000/api/fotografie/pag/3
		/// </example>
		[Route( "pag/{pagina:int}" )]
		public IEnumerable<FotografiaDto> GetFotografie( int pagina ) {
			return GetFotografie( null, null, pagina );
		}
		


/*
		[Route( "giorno/{giorno:datetime?}/pag/{pagina:int?}" )]
		public IEnumerable<FotografiaDto> GetFotografie( DateTime? giorno, int? pagina ) {
			return GetFotografie( giorno, null, pagina );
		}
*/
		public IEnumerable<FotografiaDto> GetFotografie( DateTime? giorno, string idFotografo, int? pagina ) {

			// TODO casino: occorre spostare sul web
			const int TAKE = 12;

			System.Diagnostics.Trace.WriteLine( "Inizio query ricerca foto: " + DateTime.Now.ToString() + "  pag: " + pagina );

			IFotoExplorerSrv fotoExplorerSrv = LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();

			Paginazione pag = null;
			if( pagina != null )
				pag = new Paginazione {
					skip = ((int)pagina - 1) * TAKE,
					take = TAKE
				};

			ParamCercaFoto param = new ParamCercaFoto() {
				paginazione = pag,
				idratareImmagini = false,
				evitareJoinEvento = true
			};

			// Eventuale filtro per giorno
			if( giorno != null ) {
				param.giornataIniz = giorno;
				param.giornataFine = giorno;
			}

			// Eventuale filtro per fotografo
			if( idFotografo != null ) {

				Fotografo fotografo;
				using( new UnitOfWorkScope() ) {

					IEntityRepositorySrv<Fotografo> repo = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografo>>();
					fotografo = repo.getById( idFotografo );
					// Controllo di sicurezza: mi è stato passato un codice fotografo inesistente
				}
				if( fotografo == null )
					return null;  
				Fotografo[] fotografi = { fotografo };
				param.fotografi = fotografi;
			}

			fotoExplorerSrv.cercaFoto( param );

			System.Diagnostics.Trace.WriteLine( "Fine query ricerca foto: " + DateTime.Now.ToString() );

			List<FotografiaDto> fotografieDtos = new List<FotografiaDto>();
			foreach( Fotografia f in fotoExplorerSrv.fotografie ) {

				FotografiaDto dto = new FotografiaDto {
					id = f.id,
					numero = f.numero,
					nomeFotografo = f.fotografo.cognomeNome,
					giornata = f.giornata
				};
				fotografieDtos.Add( dto );
			}

			System.Diagnostics.Trace.WriteLine( "Fine idratazione dto: " + DateTime.Now.ToString() );

			return fotografieDtos;
		}

		private static FotografiaDto creaFotografiaDto( Fotografia f ) {
			FotografiaDto dto = new FotografiaDto {
					id = f.id,
					numero = f.numero,
					nomeFotografo = f.fotografo.cognomeNome,
					giornata = f.giornata
				};
			return dto;
		}

		/// <summary>
		/// Ritorna il provino della immagine indicata dal ID.
		/// Questa immagine è subito visualizzabile nel browser.
		/// </summary>
		/// <example>
		/// http://localhost:9000/api/fotografie/e2bf55cc-9366-4857-b9ea-61309a846683/provino
		/// </example>
		/// <param name="guid"></param>
		/// <returns></returns>
		[Route( "{guid:Guid}/provino" )]
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

		//
		// ********************************
		//

		/// <summary>
		/// Tutte le fotografie di un fotografo
		/// </summary>
		/// <param name="idFotografo"></param>
		/// <returns></returns>
		[Route( "~/api/fotografi/{idFotografo}/fotografie" )]
		public IEnumerable<FotografiaDto> GetFotografieDelFotografo( string idFotografo ) {
			return GetFotografie( null, idFotografo, null );
		}

		[Route( "~/api/fotografi/{idFotografo}/fotografie/giorno/{giorno:datetime}" )]
		public IEnumerable<FotografiaDto> GetFotografieDelFotografo( string idFotografo, DateTime giorno ) {
			return GetFotografie( giorno, idFotografo, null );
		}

		[Route( "~/api/fotografi/{idFotografo}/fotografie/giorno/{giorno:datetime}/pag/{pagina:int}" )]
		public IEnumerable<FotografiaDto> GetFotografieDelFotografo( string idFotografo, DateTime giorno, int pagina ) {
			return GetFotografie( giorno, idFotografo, pagina );
		}

		[Route( "~/api/fotografi/{idFotografo}/fotografie/pag/{pagina:int}" )]
		public IEnumerable<FotografiaDto> GetFotografieDelFotografo( string idFotografo, int pagina ) {
			return GetFotografie( null, idFotografo, pagina );
		}

		[Route( "~/api/fotografi/{onlyWithImg:bool=false}" )]
		public IEnumerable<FotografoDto> GetFotografi( bool onlyWithImg ) {

			List<FotografoDto> fotografiDtos = new List<FotografoDto>();
			using( new UnitOfWorkScope() ) {
				IEntityRepositorySrv<Fotografo> repo = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografo>>();
				var fff = repo.getAll().Where( f => f.attivo == true && f.umano == true );
				foreach( Fotografo f in fff ) {

					// Eventuale controllo per filtrare solo i fotografi con immagine
					if( onlyWithImg )
						if( !esisteImmagineFotografo( f ) )
							continue;

					FotografoDto dto = new FotografoDto {
						id = f.id,
						cognomeNome = f.cognomeNome
					};
					fotografiDtos.Add( dto );
				}
			}

			return fotografiDtos;
		}

		[Route( "~/api/fotografi/{id}/immagine" )]
		public HttpResponseMessage GetImmagineFotografo( string id ) {

			HttpResponseMessage response = null;

			try {

				Fotografo fotografo = null;

				using( new UnitOfWorkScope() ) {
					IEntityRepositorySrv<Fotografo> repo = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografo>>();
					fotografo = repo.getById( id );
				}


				// dataStream = new MemoryStream( fotografo.immagine );
				// Stream dataStream = caricaImmagineFotografo( fotografo );
				var bytes = caricaBytesImgFotografo( fotografo );
				if( bytes != null ) {
					MemoryStream dataStream = new MemoryStream( bytes );

					long? dimensione;
					dimensione = dataStream.Length;
					// dimensione = fotografo.immagine.Length;
					response = new HttpResponseMessage( HttpStatusCode.OK );
					response.Content = new StreamContent( dataStream );
					response.Content.Headers.ContentType = new MediaTypeHeaderValue( "image/jpeg" );
					response.Content.Headers.ContentLength = dimensione;
				}
				

			} catch( Exception ee ) {
				Console.WriteLine( ee );
				response = new HttpResponseMessage( HttpStatusCode.InternalServerError );
			} 

			return response;
		}

		/// <summary>
		/// TODO questa modalità è temporanea. Queste immagini andranno nel db
		/// </summary>
		private Stream caricaImmagineFotografo( Fotografo f ) {

			Stream stream = null;
			var nomeFile = nomeFileImgFotografo( f );
			if( nomeFile != null )
				stream = new FileStream( nomeFile, FileMode.Open, FileAccess.Read );

			return stream;
		}

		private byte[] caricaBytesImgFotografo( Fotografo f ) {

			var nomeFile = nomeFileImgFotografo( f );
			if( nomeFile != null )
				return File.ReadAllBytes( nomeFile );
			else
				return null;
		}

		private bool esisteImmagineFotografo( Fotografo f ) {
			string nomeFile = nomeFileImgFotografo( f );
			return File.Exists( nomeFile );
		}

		private string nomeFileImgFotografo( Fotografo f ) {
			DirectoryInfo di = new DirectoryInfo( Configurazione.UserConfigLumen.cartellaMaschere );
			var folder = Path.Combine( di.Parent.FullName, "Fotografi" );
			var nomeFile = Path.Combine( folder, f.id + ".jpg" );

			return File.Exists( nomeFile ) ? nomeFile : null;
		}

	}
}
