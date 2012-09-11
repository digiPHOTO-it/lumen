using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public static class StampantiAbbinateUtil {

		public static StampantiAbbinateCollection deserializza( String strStampantiAbbinate ) {
			return new StampantiAbbinateCollection( deserializzaList( strStampantiAbbinate ) );
		}

		public static List<StampanteAbbinata> deserializzaList( String strStampantiAbbinate ) {

			List<StampanteAbbinata> list = new List<StampanteAbbinata>();
			if( String.IsNullOrEmpty( strStampantiAbbinate ) ) {
				return list;
			}

			char [] sepC = new char [] { '#' };
			String [] righe = strStampantiAbbinate.Split( sepC, StringSplitOptions.RemoveEmptyEntries );

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
			for( int i = 0; i < righe.Length; i++ ) {

				string [] campi = righe [i].Split( ';' );

				Guid idFormatoCarta = new Guid( campi[0] );
				String stampante = campi[1];

				FormatoCarta formatoCarta = dbContext.FormatiCarta.FirstOrDefault( f => f.id == idFormatoCarta );

				if( formatoCarta != null ) {					
					IStampantiInstallateSrv stampantiInstallateSrv = LumenApplication.Instance.getServizioAvviato<IStampantiInstallateSrv>();
					StampanteInstallata stampanteInstallata = stampantiInstallateSrv.getStampanteInstallataByString( stampante );


					list.Add( create(stampanteInstallata, formatoCarta) );


				}
			}

			// Ordino la lista per il valore di ordinamento impostato nel database nel formato carta.

			list.Sort( StampanteAbbinata.CompareByImportanza );
			return list;
		}

		/// <summary>
		/// metodo factory
		/// </summary>
		/// <param name="stampanteInstallata"></param>
		/// <param name="formatoCarta"></param>
		/// <returns></returns>
		public static StampanteAbbinata create( StampanteInstallata stampanteInstallata, FormatoCarta formatoCarta ) {
			StampanteAbbinata stpAbbi = new StampanteAbbinata( stampanteInstallata, formatoCarta );
			stpAbbi.ratio = determinaRatio( stampanteInstallata.NomeStampante );
			return stpAbbi;
		}

		public static string serializzaToString( StampantiAbbinateCollection collection ) {

			StringBuilder stampantiAbbinateString = new StringBuilder();
			foreach( StampanteAbbinata stampanteAbbinata in collection ) {
				stampantiAbbinateString.Append( stampanteAbbinata.FormatoCarta.id );
				stampantiAbbinateString.Append( ";" );
				stampantiAbbinateString.Append( stampanteAbbinata.StampanteInstallata.NomeStampante );
				stampantiAbbinateString.Append( "#" );
			}

			return stampantiAbbinateString.ToString();
		}


		private static float determinaRatio( string nomeStampante ) {

			float ratio = 0f;
			using( IInformatore informatore = Imaging.ImagingFactory.Instance.creaInformatore( nomeStampante ) ) {
				ratio = informatore.rapporto;
			}
			return ratio;
		}
				
	}
}
