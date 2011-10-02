using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using log4net;
using System.Configuration;
using Digiphoto.Lumen.Database;
using System.Reflection;
using Digiphoto.Lumen.Servizi;
using System.Runtime.Remoting;
using MemBus;
using MemBus.Configurators;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Applicazione {


	public sealed class LumenApplication : IObserver<String> {

		public static readonly LumenApplication _instance = new LumenApplication();
		private static readonly ILog _giornale = LogManager.GetLogger( typeof(LumenApplication) );
		private readonly IBus _bus = BusSetup.StartWith<Fast>().Construct();
		
		private Configurazione _configurazione;
		
		private IDictionary<String, ServizioImpl> _serviziAvviati = null;
		private ServizioFactory _servizioFactory;



		public static LumenApplication Instance {
			get {
				return _instance;
			}
		}


		public bool avviata {
			get;
			private set;
		}

		/**
		 * Avvio della applicazione. Accendiamo la baracca.
		 */
		public void avvia() {

			avviaConfigurazione();

			avviaServizi();

			avviata = true;

			_bus.Publish( "primo" );

			_giornale.Info( "L'applicazione è avviata." );
		}

		/**\
		 * Faccio un controllo che tutto sia a posto e che il programma possa partire
		 */
		private void avviaConfigurazione() {
			_configurazione = new Configurazione();
			_servizioFactory = new ServizioFactory();
		}

		internal Configurazione configurazione {
			get {
				return _configurazione;
			}
		}

		private void avviaServizi() {

			_serviziAvviati = new Dictionary<string, ServizioImpl>();


			VolumeCambiatoSrvImpl s1  = (VolumeCambiatoSrvImpl) _servizioFactory.creaServizio( typeof(IVolumeCambiatoSrv) );
			_serviziAvviati.Add( typeof( IVolumeCambiatoSrv ).FullName, s1 );
			s1.attesaBloccante = false;
			s1.start();
			s1.attesaEventi();
/*
			ScaricatoreFotoSrvImpl s2 = (ScaricatoreFotoSrvImpl)_servizioFactory.creaServizio( typeof( IScaricatoreFotoSrv ) );
			_serviziAvviati.Add( typeof( IScaricatoreFotoSrv ).FullName, s2 );
			s2.start();
*/
		}

		public IScaricatoreFotoSrv creaScaricatoreFotoSrv() {
			
			ScaricatoreFotoSrvImpl scaricatoreFotoSrvImpl = new ScaricatoreFotoSrvImpl();
	
			// Sottoscrivo questo servizio come asoltatore del bus di eventi
			IObservable<Messaggio> observable = _bus.Observe<Messaggio>();
			observable.Subscribe( scaricatoreFotoSrvImpl );

			return scaricatoreFotoSrvImpl;
		}

		public void aggiungiAscoltatoreServizioBus( IServizio obj ) {
			IObservable<Messaggio> observable = _bus.Observe<Messaggio>();
			observable.Subscribe( obj );
		}

		public void ferma() {
	
			_giornale.Info( "L'applicazione sta per essere fermata. Ora spengo tutto." );

			foreach( string chiave in _serviziAvviati.Keys )
				_serviziAvviati [chiave].Dispose();

			_serviziAvviati.Clear();

			avviata = false;
		}


		public void OnCompleted() {
			throw new NotImplementedException();
		}

		public void OnError( Exception error ) {
			throw new NotImplementedException();
		}

		public void OnNext( string value ) {
			throw new NotImplementedException();
		}

		public IBus bus {
			get {
				return _bus;
			}
		}


		//public IScaricatoreFotoSrv getScaricatoreFotoSrv() {
		//   return (IScaricatoreFotoSrv) _serviziAvviati[ typeof( IScaricatoreFotoSrv ).FullName ];
		//}
	}
}
