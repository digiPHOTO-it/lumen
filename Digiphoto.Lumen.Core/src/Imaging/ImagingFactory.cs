using System;

namespace Digiphoto.Lumen.Imaging {

	/** Questa classe serve ad istanziare le giuste classi che implementano la parte grafica */
	public class ImagingFactory {
	
		 private static volatile ImagingFactory _instance;
		 private static object m_syncRoot = new object();
    
		 public static ImagingFactory Instance {
    
			  get {
					if (_instance == null ) {
						 lock (m_syncRoot) {
							  if ( _instance == null ) {
									_instance = new ImagingFactory();
							  }
						 }
					}
            
					return _instance;
			  }
		 }
    
		 private ImagingFactory() {
		 }


		 public IProvinatore creaProvinatore() {
			 // TODO sostituire con un setting
			 return (IProvinatore) Activator.CreateInstance( "Digiphoto.Lumen.Imaging", "ProvinatoreNet" );
		 }
	


	}
}