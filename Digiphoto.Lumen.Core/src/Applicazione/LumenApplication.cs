using System;
using System.Collections.Generic;

using log4net;
using Digiphoto.Lumen.Servizi;
using MemBus;
using MemBus.Configurators;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Imaging;
using log4net.Config;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Servizi.Ritoccare;

namespace Digiphoto.Lumen.Applicazione {


	public sealed class LumenApplication {

		#region Proprietà

		public static readonly LumenApplication _instance = new LumenApplication();
		private static readonly ILog _giornale = LogManager.GetLogger( typeof(LumenApplication) );
		private readonly IBus _bus = BusSetup.StartWith<Conservative>().Construct();
		
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

/*
 * Purtoppo non posso mettere questo controllo perché gli Test-Case si inciampano qui.
 * Sembra che i vari test case siano eseguiti in più thread dello stesso processo.
 * 
			if( avviata == true )
				throw new InvalidOperationException( "L'applicazione Lumen è già stata avviata" );

*/
			// Configuro il logger
			XmlConfigurator.Configure();

			avviaConfigurazione();

			using( new UnitOfWorkScope() ) {

				StartupUtil.forseCreaInfoFisse();

				creaStato();

				avviaServizi();
			}

			avviata = true;

			_bus.Publish( "primo" );

			_giornale.Info( "L'applicazione è avviata." );
		}

		private void creaStato() {
			
			stato = new Stato();
			stato.giornataLavorativa = StartupUtil.calcolaGiornataLavorativa();
			stato.isWindowPubblicaVisibile = Properties.Settings.Default.isWindowPubblicaVisibile;
			stato.isSlideShowRunning = false;
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

		/**
		 * - Istanzio i servizi,
		 * - li aggiungo in una lista interna che mi tengo io per gestire i servizi.
		 * - Avvio i servizi appena creati
		 */
		private void avviaServizi() {

			// Creo la mappa dei servizi
			_serviziAvviati = new Dictionary<string, IServizio>();

			// Creo i servizi

			//
			IVolumeCambiatoSrv vcs = creaAggiungiAvviaServizio<IVolumeCambiatoSrv>();
			vcs.attesaBloccante = false;
			vcs.attesaEventi();
			//
			creaAggiungiAvviaServizio<IScaricatoreFotoSrv>();
			//
			creaAggiungiAvviaServizio<IGestoreImmagineSrv>();
			//
			creaAggiungiAvviaServizio<IFotoRitoccoSrv>();
			//
			creaAggiungiAvviaServizio<IFotoExplorerSrv>();
			//
			creaAggiungiAvviaServizio<ICarrelloExplorerSrv>();
			//
			creaAggiungiAvviaServizio<ISpoolStampeSrv>();
			//
			creaAggiungiAvviaServizio<IVenditoreSrv>();

			creaAggiungiAvviaServizio<IEntityRepositorySrv<Fotografo>>();

			creaAggiungiAvviaServizio<IEntityRepositorySrv<Evento>>();

            creaAggiungiAvviaServizio<IEntityRepositorySrv<FormatoCarta>>();

		}

		public T creaServizio<T>() where T : IServizio {
			return (T) _servizioFactory.creaServizio( typeof(T) );
		}

		private T creaAggiungiAvviaServizio<T>() where T : IServizio {
			T srv = creaServizio<T>();
			string key = ServizioFactory.calcFullName( typeof( T ) );
			_serviziAvviati.Add( key, srv );
			srv.start();
			return srv;
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


		public IBus bus {
			get {
				return _bus;
			}
		}

		//
		// <summary>
		//  il mioFullName viene composto dal metodo ServizioFactory.calcFullName()
		// </summary>
		//
		public IServizio getServizioAvviato( string mioFullName ) {
			return _serviziAvviati [mioFullName];
		}

		public T getServizioAvviato<T>() {
			return (T)getServizioAvviato( ServizioFactory.calcFullName( typeof(T) ) );
		}

	}
}
