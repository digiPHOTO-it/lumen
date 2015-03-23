using System;
using System.Collections.Generic;
using System.Linq;
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
using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Digiphoto.Lumen.Licensing;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Applicazione;

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

		private RegistryLicense registryLicense;

		#endregion


		public static LumenApplication Instance {
			get {
				return _instance;
			}
		}

		private string _msgPossoFermare;
		private bool _possoFermare;
		public bool possoFermare
		{
			get
			{
				_possoFermare = true;
				foreach (string chiave in _serviziAvviati.Keys)
				{
					
					if(!_serviziAvviati[chiave].possoChiudere()){
						_possoFermare = false;
						_msgPossoFermare = "\n " + _serviziAvviati[chiave].msgPossoChiudere();
						_giornale.Info(_serviziAvviati[chiave].msgPossoChiudere());
					}
				}
				return _possoFermare;
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
			avvia( false, (string)null, true );
		}


		// TODO questa dovrebbe essere internal usata solo dalla configurazione
		public void avvia( bool autoSistema, string connectionString, bool stopSeMancaLicenza ) {

		/*
		 * Purtoppo non posso mettere questo controllo perché gli Test-Case si inciampano qui.
		 * Sembra che i vari test case siano eseguiti in più thread dello stesso processo.
		 * 
			if( avviata == true )
				throw new InvalidOperationException( "L'applicazione Lumen è già stata avviata" );
		*/
			// Configuro il logger
			log4net.Config.XmlConfigurator.Configure();
			

			avviaConfigurazione( autoSistema );

			using( new UnitOfWorkScope( false, connectionString ) ) {

				Configurazione.infoFissa = StartupUtil.forseCreaInfoFisse();

				creaStato();

				avviaServizi();

				if( autoSistema )
					creaAlcuniDatiDiDefault();

			}

			caricaDatiLicenza();


			// Controllo la licenza
			if( !this.haveValidLicense )
				if( stopSeMancaLicenza )
					throw new LicenseNotFoundException();

			avviata = true;

			_bus.Publish( "primo" );

			_giornale.Info( "L'applicazione è avviata." );
		}

		private void creaAlcuniDatiDiDefault() {

			// Devo creare un fotografo pre-stabilito per assegnare le foto modificate con GIMP
			IEntityRepositorySrv<Fotografo> repo = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografo>>();
			Fotografo artista = repo.getById( Configurazione.ID_FOTOGRAFO_DEFAULT );
			if( artista == null ) {
				artista = new Fotografo();
				artista.id = Configurazione.ID_FOTOGRAFO_DEFAULT;
				artista.umano = true;
				artista.attivo = true;
				artista.cognomeNome = artista.id;
				artista.iniziali = "XY";
				artista.note = "default operator";
				repo.addNew( artista );
				repo.saveChanges();
			}
		}

		private void creaStato() {
			
			stato = new Stato();
			stato.giornataLavorativa = StartupUtil.calcolaGiornataLavorativa();
			stato.isWindowPubblicaVisibile = Configurazione.UserConfigLumen.isWindowPubblicaVisibile;
			stato.isSlideShowRunning = false;
		}

		/**
		 * Faccio un controllo che tutto sia a posto e che il programma possa partire
		 */
		private void avviaConfigurazione( bool autoSistema ) {
			
			_configurazione = new Configurazione( autoSistema );

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

			foreach( String key in this.configurazione.nomiServizi.Keys ) {

				Type interfaccia = null;

				int posIniz = key.IndexOf( "<" );
				if( posIniz > 0 ) {
					// Devo gestire i generic type
					int posFine = key.IndexOf( ">", posIniz + 1 );
					if( posFine > 0 ) {
						String genericStr = key.Substring( posIniz+1, posFine-posIniz-1 );
						Type generic = Type.GetType( genericStr );
						if( generic == null )
							generic = Type.GetType( genericStr + ",Digiphoto.Lumen.Core" );
						if( generic == null )
							generic = Type.GetType( genericStr + ",Digiphoto.Lumen.Model" );

						String interfaceName = key.Substring( 0, posIniz );

						String tutto = interfaceName + "`1";

						interfaccia = Type.GetType( tutto );
						if( interfaccia == null )
							interfaccia = Type.GetType( tutto + ",Digiphoto.Lumen.Core" );
						if( interfaccia == null )
							interfaccia = Type.GetType( tutto + ",Digiphoto.Lumen.Model" );

						interfaccia = interfaccia.MakeGenericType( generic );
					}


				} else {

					interfaccia = Type.GetType( key );
				}


				creaAggiungiAvviaServizio( interfaccia );
			}

			// Questo servizio nasce
			IVolumeCambiatoSrv vcs = getServizioAvviato<IVolumeCambiatoSrv>();
			if( vcs != null ) {
				// TODO forse si può sporstare dentro la IMPL
				vcs.attesaBloccante = false;
				vcs.attesaEventi();
			}

		}

		public T creaServizio<T>() where T : IServizio {
			return (T) _servizioFactory.creaServizio( typeof(T) );
		}

		private IServizio creaAggiungiAvviaServizio( Type type ) {
			IServizio srv = _servizioFactory.creaServizio( type );
			string key = ServizioFactory.calcFullName( type );
			_serviziAvviati.Add( key, srv );
			srv.start();
			return srv;
		}

		private T creaAggiungiAvviaServizio<T>() where T : IServizio {
			return (T)creaAggiungiAvviaServizio( typeof( T ) );
		}


		public void aggiungiAscoltatoreServizioBus( IServizio obj ) {
			IObservable<Messaggio> observable = _bus.Observe<Messaggio>();
			observable.Subscribe( obj );
		}

		public void ferma()
		{
			_giornale.Info( "L'applicazione sta per essere fermata. Ora spengo tutto." );

			foreach( string chiave in _serviziAvviati.Keys )
				_serviziAvviati [chiave].Dispose();

			_serviziAvviati.Clear();

			if( registryLicense != null )
				registryLicense.Dispose();

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
			if( _serviziAvviati.ContainsKey( mioFullName ) )
				return _serviziAvviati[mioFullName];
			else {
				_giornale.Warn( "Servizio non avviato: " + mioFullName );
				return null;
			}
		}

		public T getServizioAvviato<T>() {
			return (T)getServizioAvviato( ServizioFactory.calcFullName( typeof(T) ) );
		}


		public IList<StampanteInstallata> stampantiInstallate {
			get {
				return getServizioAvviato<IStampantiInstallateSrv>().stampantiInstallate;
			}
		}

		void caricaDatiLicenza() {

			registryLicense = LicenseUtil.createCurrentLicense();

			if( haveValidLicense )
				_giornale.Info( "Caricata licenza ed è valida" );
			else
				_giornale.Warn( "Licenza non valida" );
		}

		public bool haveValidLicense {
			get {
				return LicenseUtil.isValida( registryLicense );
			}
		}

		public int numGiorniScadenzaLicenza {
			get {
				return registryLicense != null ? registryLicense.DaysLeft : 0;
			}
		}
	}
}
