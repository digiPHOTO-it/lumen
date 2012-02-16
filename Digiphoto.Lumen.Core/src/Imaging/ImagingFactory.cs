using System;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Servizi.Ritoccare;

namespace Digiphoto.Lumen.Imaging {

	/** Questa classe serve ad istanziare le giuste classi che implementano la parte grafica */
	public class ImagingFactory {

		private static volatile ImagingFactory _instance;
		private static object m_syncRoot = new object();

		public static ImagingFactory Instance {

			get {
				if( _instance == null ) {
					lock( m_syncRoot ) {
						if( _instance == null ) {
							_instance = new ImagingFactory();
						}
					}
				}

				return _instance;
			}
		}

		private ImagingFactory() {
		}


		public ICorrettoreFactory creaCorrettoreFactory() {

			object ooo = Activator.CreateInstance( "Digiphoto.Lumen.Imaging.Wic", "Digiphoto.Lumen.Imaging.Wic.Correzioni.CorrettoreFactory" ).Unwrap();

			// TODO sostituire con un setting
			return (ICorrettoreFactory)ooo;

		}


		public IEsecutoreStampa creaStampatore( string nomeStampante ) {

			object ooo = Activator.CreateInstance( "Digiphoto.Lumen.Imaging.Nativa", "Digiphoto.Lumen.Imaging.Nativa.EsecutoreStampaNet" ).Unwrap();

			// TODO sostituire con un setting
			return (IEsecutoreStampa)ooo;
		}

	}
}