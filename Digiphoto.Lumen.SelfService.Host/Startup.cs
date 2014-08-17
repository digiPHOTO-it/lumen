using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;

namespace Digiphoto.Lumen.SelfService.Host {
	
	public class Startup {

		Type valuesControllerType = typeof( SelfServiceController );

		public void Configuration( IAppBuilder appBuilder ) {

			HttpConfiguration config = new HttpConfiguration();

			//  Enable attribute based routing
			//  http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
			config.MapHttpAttributeRoutes();
/*
			config.Routes.MapHttpRoute( 
				name: "DefaultApi", 
				routeTemplate: "lumen/selfservice/{controller}/{id}", 
				defaults: new {
					id = RouteParameter.Optional
				} 
			);
*/
			appBuilder.UseWebApi( config );
		}
	}
}
