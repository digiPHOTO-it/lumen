using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Validation;
using log4net;

namespace Digiphoto.Lumen.Util {

	public static class ErroriUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ErroriUtil ) );

		public static string estraiMessage( Exception ee ) {

			string msg = null;

			do {

				if( ee is DbEntityValidationException ) {

					StringBuilder sb = new StringBuilder();
					DbEntityValidationException ev = (DbEntityValidationException)ee;
					foreach( DbEntityValidationResult res in ev.EntityValidationErrors ) {
						if( !res.IsValid ) {
							foreach( DbValidationError erro in res.ValidationErrors ) {
								_giornale.Debug( "property non valida: " + erro.PropertyName + ". Motivo=" + erro.ErrorMessage );
								sb.Append( "Propietà non valida: " + erro.PropertyName + ". Motivo=" + erro.ErrorMessage + "\n" );
							}
						}
					}
					msg = sb.ToString();
				} else {
					if( ee.InnerException == null ) {
						msg = ee.Message;
					} else {
						ee = ee.InnerException;
					}
				}
			
			} while( msg == null );
				
			return msg;
		}
	}
}
