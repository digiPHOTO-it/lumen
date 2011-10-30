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
using Digiphoto.Lumen.Imaging;
using log4net.Config;

namespace Digiphoto.Lumen.Applicazione {


	public sealed class LumenApplication : IObserver<String> {

		#region Proprietà

		public static readonly LumenApplication _instance = new LumenApplication();
		private static readonly ILog _giornale = LogManager.GetLogger( typeof(LumenApplication) );
		private readonly IBus _bus = BusSetup.StartWith<Fast>().Construct();
		
		private Configurazione _configurazione;
		
		private IDictionary<String, IServizio> _serviziAvviati = null;

		private ServizioFactory _servizioFactory;

		public Stato stato {
			get;
			private set;
		}

		#endregion



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

			// Configuro il logger
			XmlConfigurator.Configure();

			avviaConfigurazione();

			StartupUtil.forseCreaInfoFisse();

			creaStato();

			avviaServizi();

			avviata = true;

			_bus.Publish( "primo" );

			_giornale.Info( "L'applicazione è avviata." );
		}

		private void creaStato() {
			
			stato = new Stato();
			stato.giornataLavorativa = StartupUtil.calcolaGiornataLavorativa();
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

			_serviziAvviati = new Dictionary<string, IServizio>();


			string s2 = this.GetType().Assembly.FullName;
			Console.WriteLine( s2 );

			IVolumeCambiatoSrv s1  = (VolumeCambiatoSrvImpl) _servizioFactory.creaServizio( typeof(IVolumeCambiatoSrv) );
			_serviziAvviati.Add( typeof( IVolumeCambiatoSrv ).FullName, s1 );
			s1.attesaBloccante = false;
			s1.start();
			s1.attesaEventi();

			IGestoreImmagineSrv gis = (IGestoreImmagineSrv)_servizioFactory.creaServizio( typeof( IGestoreImmagineSrv ) );
			_serviziAvviati.Add( typeof(IGestoreImmagineSrv).FullName , gis );
			gis.start();



/*
			ScaricatoreFotoSrvImpl s2 = (ScaricatoreFotoSrvImpl)_servizioFactory.creaServizio( typeof( IScaricatoreFotoSrv ) );
			_serviziAvviati.Add( typeof( IScaricatoreFotoSrv ).FullName, s2 );
			s2.start();
*/
		}

		public T creaServizio<T>() {

			T srv  = (T) _servizioFactory.creaServizio( typeof(T) );
			return srv;
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


		public IServizio getServizioAvviato( string nome ) {
			return _serviziAvviati [nome];
		}

		/** Ritorno il servizio di gestione dell''immagine */
		public IGestoreImmagineSrv getGestoreImmaginiSrv() {
			return (IGestoreImmagineSrv) getServizioAvviato( typeof(IGestoreImmagineSrv).FullName );
		}

		//public IScaricatoreFotoSrv getScaricatoreFotoSrv() {
		//   return (IScaricatoreFotoSrv) _serviziAvviati[ typeof( IScaricatoreFotoSrv ).FullName ];
		//}
	}
}
