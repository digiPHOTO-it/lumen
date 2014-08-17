using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Microsoft.Owin.Hosting;

//   Esempio preso da qui:
//   http://www.asp.net/web-api/overview/hosting-aspnet-web-api/use-owin-to-self-host-web-api


namespace Digiphoto.Lumen.SelfService.Host {

	public class Program {
		
		static void Main( string[] args ) {

			LumenApplication app = LumenApplication.Instance;
			app.avvia();

			string baseAddress = "http://tirannos:9000/";
			
			// Start OWIN host
			using( WebApp.Start<Startup>( url: baseAddress ) ) {

				bool autoTest = false;

				if( autoTest ) {


					// creo il client http
					HttpClient client = new HttpClient();
					//				var response = client.GetAsync( baseAddress + "lumen/selfservice/getall" ).Result;


					HttpResponseMessage response = client.GetAsync( baseAddress + "api/selfservice/fotografie/2" ).Result;
					List<FotografiaDto> lista = response.Content.ReadAsAsync<List<FotografiaDto>>().Result;
					foreach( FotografiaDto fotoDto in lista ) {
						Console.WriteLine( "num=" + fotoDto.numero + " data=" + fotoDto.giornata + " oper=" + fotoDto.nomeFotografo );
					}

					Console.WriteLine( "----" );

					Guid guid = lista[0].id;
					String url = baseAddress + "api/selfservice/" + guid.ToString() + "/image";
					response = client.GetAsync( url ).Result;
					Console.WriteLine( response );
					var data = response.Content.ReadAsByteArrayAsync().Result;
					using( var ms = new MemoryStream( data ) ) {
						using( var fs = File.Create( "c:\\tmp\\pollo.jpeg" ) ) {
							ms.CopyTo( fs );
						}
					}
				}

				Console.WriteLine( "* * * * ATTESA * * * " );
				Console.ReadLine();
			}

			app.ferma();
		}
	}
}
