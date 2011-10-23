﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using log4net;
using Digiphoto.Lumen.Database;

namespace Digiphoto.Lumen.Applicazione {
	
	internal static class StartupUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( StartupUtil ) );

		/**
		 * La data legale, rappresenta la giornata lavorativa.
		 * Anche se le foto sono state scattate il giorno 2 all'una di notte, 
		 * queste appartengono alla giornata lavorativa del giorno 1.
		 * L'orario vero in cui fare il cambio della giornata, è scritto nei settaggi
		 */
		public static DateTime calcolaGiornataLavorativa() {

			DateTime gionata = DateTime.Today;
			string oraCambioGiornata = Properties.Settings.Default.oraCambioGiornata;

			if( !String.IsNullOrEmpty( oraCambioGiornata ) ) {
				DateTime adesso = DateTime.Now;
				DateTime ieri = DateTime.Today.AddDays( -1 );

				string [] pezzi = oraCambioGiornata.Split( ':' );
				if( pezzi.Length == 2 ) {

					if( adesso.Hour < Int16.Parse( pezzi [0] ) )
						gionata = ieri;
					else if( adesso.Hour == Int16.Parse( pezzi [0] ) && adesso.Minute < Int16.Parse( pezzi [1] ) )
						gionata = ieri;
				}
			}

			return gionata;
		}

		// Se non esistono le informazioni fisse, allora le creo di default
		internal static void forseCreaInfoFisse() {

			using( new UnitOfWorkScope() ) {
			
				LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
				InfoFissa infoFissa = objContext.InfosFisse.SingleOrDefault<InfoFissa>( f => f.id == "K" );

				if( infoFissa == null ) {
					_giornale.Info( "Informazioni fisse non trovate. Le creo con i default" );
					infoFissa = new InfoFissa();
					infoFissa.id = "K";
					objContext.InfosFisse.AddObject( infoFissa );
					objContext.SaveChanges();
				}
			}
		}

	}
}