using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Configuration;
using Digiphoto.Lumen.Servizi.Masterizzare.MyBurner;
using IMAPI2.Interop;
using System.IO;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.GestoreConfigurazione.UI.Util;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.UI;
using Digiphoto.Lumen.UI.Mvvm;
using log4net;
using System.Data.Entity.Validation;
using System.Text;
using Digiphoto.Lumen.Servizi.Ricostruzione;
using Digiphoto.Lumen.GestoreConfigurazione.UI.Licenze;
using Digiphoto.Lumen.Licensing;

namespace Digiphoto.Lumen.GestoreConfigurazione.UI
{

	public enum PassoWiz {

		Login = 0,
		MotoreDb = 1,
		PuntoVendita = 2,
		CartaEStampanti = 3,
		PreferenzeUtente = 4,
		Riservato = 5,
		Licenza = 6
	}

    public class MainWindowViewModel : ClosableWiewModel
    {

        public MainWindowViewModel()
        {
            //Blocco l'interfaccia fino al login
			loginEffettuato = false;
            listaMasterizzatori = new ObservableCollection<String>();

            caricaListaMasterizzatori();

            loadUserConfig();

			licenseEditorViewModel = new LicenseEditorViewModel();

			passo = PassoWiz.Login;

			this.abilitoShutdown = false;  // NON permetto all'utente di scegliere se spegnere il computer.
        }

		public LicenseEditorViewModel licenseEditorViewModel {
			get;
			private set;
		}

		private IDictionary<string, string> _modiAzzeramentoNumeroFoto;
		public IDictionary<string,string> modiAzzeramentoNumeroFoto {
			get {
				if( _modiAzzeramentoNumeroFoto == null ) {
					_modiAzzeramentoNumeroFoto = new Dictionary<string,string>();
					_modiAzzeramentoNumeroFoto.Add( "M", "Mai" );
					_modiAzzeramentoNumeroFoto.Add( "G", "Giornaliero" );
					_modiAzzeramentoNumeroFoto.Add( "S", "Settimanale" );
				}
				return _modiAzzeramentoNumeroFoto;
			}
		}

		public UserConfigLumen cfg {
			get;
			set;
		}

		public IEnumerable<ModoVendita> modoVenditaValues {
			get {
				return Enum.GetValues( typeof(ModoVendita) ).Cast<ModoVendita>();
			}
		}

		public IEnumerable<MotoreDatabase> motoriDatabasesValues {
			get {
				return Enum.GetValues( typeof( MotoreDatabase ) ).Cast<MotoreDatabase>();
			}
		}

		private UnitOfWorkScope unitOfWorkScope {
			get;
			set;
		}

		private InfoFissa _infoFissa;
		public InfoFissa infoFissa {
			get {
				return _infoFissa;
			}
			set {
				if( _infoFissa != value ) {
					_infoFissa = value;
					OnPropertyChanged( "infoFissa" );
				}
			}
		}

	
		public bool notMasterizzaDirettamente {
			get {
				return ! cfg.masterizzaDirettamente;
			}
			set {
				cfg.masterizzaDirettamente = !(value);
			}
		}

		public MotoreDatabase? motorePrecedente {
			get;
			set;
		}

        private void loadUserConfig()
        {
			_giornale.Debug( "Carico eventuale configurazione utente già presente" );

			// La carico da disco, non uso quella statica già caricata dentro Configurazione.
			this.cfg = Configurazione.caricaUserConfig();

			// Se è nullo, significa che è la prima volta che parte il programma, ma non è ancora stata avviata la configurazione.
			if( this.cfg == null ) {

				_giornale.Debug( "La configurazione utente non esite. La creo adesso in memoria." );

				this.cfg = Configurazione.creaUserConfig();
				Configurazione.UserConfigLumen = this.cfg;
			}

			// Mi parcheggio il Motore di database precedente per fare dei ragionamenti
			motorePrecedente = cfg.motoreDatabase;
        }

        private void saveUserConfig()
        {
			_giornale.Debug( "Devo salvare la configurazione utente su file xml" );

			UserConfigSerializer.serializeToFile( cfg );
			_giornale.Info( "Salvata la configurazione utente su file xml" );

            salvaConfigDB();
        }

		void saveRegistry() {
			_giornale.Debug( "Salvo nel registry: cod licenza = " + licenseEditorViewModel.codiceLicenza );
			LicenseUtil.writeCurrentLicenseKey( licenseEditorViewModel.codiceLicenza );
		}

		// TODO rivedere. non so se serve piu
        private void salvaConfigDB()
        {
			AppDomain.CurrentDomain.SetData( "DataDirectory", cfg.cartellaDatabase );
		}

        private void caricaListaMasterizzatori()
        {
			_giornale.Debug( "Carico lista masterizzatori" );

            // purtoppo pare che rimpiazzare il reference con uno nuovo, causa dei problemi.
            // Non posso istanziare nuovamente la lista, ma la devo svuotare e ripopolare.
            listaMasterizzatori.Clear();
            BurnerSrvImpl burner = new BurnerSrvImpl();
            burner.start();
            foreach (IDiscRecorder2 masterizzatore in burner.listaMasterizzatori())
            {
                listaMasterizzatori.Add(""+masterizzatore.VolumePathNames.GetValue(0));
            }

			_giornale.Debug( "Trovati " + listaMasterizzatori.Count + " masterizzatori" );
        }

		PassoWiz _passo;
		public PassoWiz passo {
			get {
				return _passo;
			}
			set {
				if( _passo != value ) {
					_passo = value;
					OnPropertyChanged( "passo" );

					foreach( string name in Enum.GetNames( typeof(PassoWiz) ) ) {
						if( name == "Login" )
							continue;  // TODO far diventare il login come passo 1. poi togliere.

						OnPropertyChanged( "isPasso" + name );
					}
/*
					OnPropertyChanged( "isPassoMotoreDb" );
					OnPropertyChanged( "isPassoPuntoVendita" );
					OnPropertyChanged( "isPassoCartaEStampanti" );
					OnPropertyChanged( "isPassoPreferenzeUtente" );
					OnPropertyChanged( "isPassoRiservato" );
 */
					OnPropertyChanged( "possoStepAvanti" );
					OnPropertyChanged( "possoStepIndietro" );
					OnPropertyChanged( "possoApplicare" );
				}
			}
		}

		public bool isPassoMotoreDb {
			get {
				return passo == PassoWiz.MotoreDb;
			}
		}

		public bool isPassoPuntoVendita {
			get {
				return passo == PassoWiz.PuntoVendita;
			}
		}

		public bool isPassoCartaEStampanti {
			get {
				return passo == PassoWiz.CartaEStampanti;
			}
		}

		public bool isPassoPreferenzeUtente {
			get {
				return passo == PassoWiz.PreferenzeUtente;
			}
		}

		public bool isPassoRiservato {
			get {
				return passo == PassoWiz.Riservato;
			}
		}

		public bool isPassoLicenza {
			get {
				return passo == PassoWiz.Licenza;
			}
		}

		public bool possoStepIndietro {
			get {
				return loginEffettuato && passo > PassoWiz.MotoreDb;
			}
		}

		public bool possoStepAvanti {
			get {
				return loginEffettuato
					&& passo < PassoWiz.Licenza
					&& File.Exists( Path.Combine( cfg.cartellaDatabase, cfg.dbNomeDbPieno ) );
			}
		}

		void apriTutto() {

			dialogProvider.attenderePrego = true;

			// Se è la prima volta, avvio tutto
			if( !LumenApplication.Instance.avviata )
				LumenApplication.Instance.avvia( true, qualeConnectionString );


			// Se è la prima volta, avvio tutto
			impostaConnectionStringFittizzia();
			if( this.unitOfWorkScope == null )
				this.unitOfWorkScope = new UnitOfWorkScope( false, qualeConnectionString );

			// carico un pò di dati dal database se per caso esiste già.
			loadDataContextFromDb();

			dialogProvider.attenderePrego = false;
		}

		void chiudiTutto() {

			// Se sono tornato qui chiudo tutto
			if( this.unitOfWorkScope != null ) {
				this.unitOfWorkScope.Dispose();
				this.unitOfWorkScope = null;
			}

			// Se sono tornato qui chiudo tutto
			if( LumenApplication.Instance.avviata )
				LumenApplication.Instance.ferma();
		}

        private SelettoreFormatoCartaViewModel selettoreFormatoCartaViewModel = null;

        private SelettoreFormatoCartaAbbinatoViewModel selettoreFormatoCartaAbbinatoViewModel = null;

        private SelettoreStampantiInstallateViewModel selettoreStampantiInstallateViewModel = null;


		/// <summary>
		/// Ora che ho stabilito quale db usare, carico i dati necessari dal db.
		/// </summary>
        private void loadDataContextFromDb() {
           
			selettoreFormatoCartaViewModel = new SelettoreFormatoCartaViewModel();
            
			selettoreFormatoCartaAbbinatoViewModel = new SelettoreFormatoCartaAbbinatoViewModel( cfg.stampantiAbbinate );

            selettoreStampantiInstallateViewModel = new SelettoreStampantiInstallateViewModel();
			

            DataContextFormatoCarta = selettoreFormatoCartaViewModel;

            DataContextAbbinamenti = selettoreFormatoCartaAbbinatoViewModel;

            DataContextStampantiInstallate = selettoreStampantiInstallateViewModel;

            OnPropertyChanged("DataContextFormatoCarta");
            OnPropertyChanged("DataContextAbbinamenti");
            OnPropertyChanged("DataContextStampantiInstallate");

			infoFisseRepository = LumenApplication.Instance.creaServizio<IEntityRepositorySrv<InfoFissa>>();
			infoFisseRepository.start();

			// -- carico anche le informazioni fisse che possono essere modificate
			infoFissa = infoFisseRepository.getById( "K" );
        }

		private void ricostruireDb() {

			bool procediPure = false;

			dialogProvider.ShowConfirmation( "Questa operazione potrebbe durare parecchi minuti.\nSei sicuro di voler cominciare l'analisi?", "Fase 1 : analisi foto", 
				(sino) => {
					procediPure = sino;
				} );
			if( !procediPure )
				return;


			IDbRebuilderSrv rebuilderSrv = LumenApplication.Instance.creaServizio<IDbRebuilderSrv>();

			rebuilderSrv.analizzare();

			if( rebuilderSrv.necessarioRicostruire ) {

				StringBuilder msg = new StringBuilder( "L'analisi ha riscontrato alcuni dati mancanti nel database:" );
				if( rebuilderSrv.contaFotografiMancanti > 0 )
					msg.Append( "\nFotografi mancanti = " + rebuilderSrv.contaFotografiMancanti );
				if( rebuilderSrv.contaFotoMancanti > 0 )
					msg.Append( "\nFotografie mancanti = " + rebuilderSrv.contaFotoMancanti );
				if( rebuilderSrv.contaFotografieSenzaImmagini > 0 )
					msg.Append( "\nFoto senza immagini = " + rebuilderSrv.contaFotografieSenzaImmagini );
				msg.Append( "\n" );
				msg.Append( "\nAvviando la ricostruzione verranno rigenerati" );
				msg.Append( "\ni dati mancanti." );
				msg.Append( "\n\nSei sicuro di voler proseguire nella ricostruzione?" );

				procediPure = false;
				dialogProvider.ShowConfirmation( msg.ToString(), "Analisi terminata",
					(sino) => {
						procediPure = sino;
					} );
				if( !procediPure )
					return;

				rebuilderSrv.ricostruire();

				msg.Clear();
				msg.Append( "Sono state apportate le seguenti modifiche:" );
				msg.Append( "\nAggiunti  " + rebuilderSrv.contaFotografiAggiunti + " fotografi." );
				msg.Append( "\nAggiunte  " + rebuilderSrv.contaFotoAggiunte + " fotografie." );
				msg.Append( "\nEliminate " + rebuilderSrv.contaFotoEliminate + " fotografie." );
				dialogProvider.ShowMessage( msg.ToString(), "Ricostruzione terminata" );

			} else {
				dialogProvider.ShowMessage( "Nessuna ricostruzione necessaria.", "Analisi terminata" );
			}

		}

		bool possoLogin {
			get {
				 bool posso = loginEffettuato == false;
#if (! DEBUG)
				if( posso ) 
					posso = LoginPassword != null;
#endif
				return posso;
			}
		}

        #region Proprietà

		private bool _loginEffettuato;
		public bool loginEffettuato
        {
            get
            {
				return _loginEffettuato;
            }
            set{
				if (_loginEffettuato != value)
                {
					_loginEffettuato = value;
					OnPropertyChanged("loginEffettuato");
                }
            }
        }

		public bool loginDaEffettuare {
			get {
				return !loginEffettuato;
			}
		}

        public SelettoreFormatoCartaAbbinatoViewModel DataContextAbbinamenti
        {
            get;
            set;
        }

        public SelettoreFormatoCartaViewModel DataContextFormatoCarta
        {
            get;
            set;
        }

        public SelettoreStampantiInstallateViewModel DataContextStampantiInstallate
        {
            get;
            set;
        }

		private String _dataSource;
		public String DataSource 
		{
			get 
			{
				return _dataSource;
			}
			set 
			{
				if( _dataSource != value ) 
				{
					_dataSource = value;
					OnPropertyChanged( "DataSource" );
				}
			}
		}

		public String ConnectionString 
		{
			get
			{
				return ConfigurationManager.ConnectionStrings [qualeConnectionString].ToString();
			}
		}

		private string qualeConnectionString {
			get {
				if( cfg.motoreDatabase == MotoreDatabase.SqLite )
					return "LumenEntities-sqLite";
				else if( cfg.motoreDatabase == MotoreDatabase.SqlServerCE )
					return "LumenEntities-sqlCE";
				else if( cfg.motoreDatabase == MotoreDatabase.SqlServer )
					return "LumenEntities-sqlServer";
				else
					return null;
			}
		}

		public bool possoCambiareCartellaDb
		{
			get
			{
				return cfg.motoreDatabase == MotoreDatabase.SqLite || cfg.motoreDatabase == MotoreDatabase.SqlServerCE;
			}
		}

        public FormatoCarta formatoCartaSelezionato
        {
            get;
            set;
        }

        private string _loginPassword;
        public string LoginPassword
        {
            get
            {
                return _loginPassword;
            }

            set
            {
                _loginPassword = value;
                OnPropertyChanged("LoginPassword");
            }
        }

        private String _nuovaPassword;
        public String NuovaPassword
        {
            get
            {
                return _nuovaPassword;
            }

            set
            {
                _nuovaPassword = value;
                OnPropertyChanged("NuovaPassword");
            }
        }

        private String _nuovaPassword2;
        public String NuovaPassword2
        {
            get
            {
                return _nuovaPassword2;
            }

            set
            {
                _nuovaPassword2 = value;
                OnPropertyChanged("NuovaPassword2");
            }
        }

        /// <summary>
        /// Tutti i formatiCarta da visualizzare
        /// </summary>
        public ObservableCollection<String> listaMasterizzatori
        {
            get;
            set;
        }

		/// <summary>
		/// Questo mi permette di leggere/scrivere le info fisse
		/// </summary>
		private IEntityRepositorySrv<InfoFissa> infoFisseRepository {
			get;
			set;
		}

		public bool canCreateDatabase {
			get {
				// Per poter creare il database, non deve esistere
				DbUtil dbUtil = new DbUtil( cfg );
				return ! dbUtil.isDatabasEsistente;
			}
		}

		public bool canTestConnection {
			get {
				DbUtil dbUtil = new DbUtil( cfg );
				return dbUtil.isDatabasEsistente;				
			}
		}

		public bool possoCambiareMotoreDb {
			get {
				return loginEffettuato ? true : false;
			}
		}

		public bool possoApplicare {
			get {
				return loginEffettuato && passo > PassoWiz.MotoreDb;
			}
		}

		public bool possoAnnullare {
			get {
				return loginEffettuato;
			}
		}

		public bool possoRicostruireDb {
			get {
				return canTestConnection;
			}
		}

        #endregion

        #region Comandi

        private RelayCommand _commandStepIndietro;
        public ICommand commandStepIndietro
        {
            get
            {
                if (_commandStepIndietro == null)
                {
                    _commandStepIndietro = new RelayCommand(param => this.stepIndietro(), param => possoStepIndietro );
                }
                return _commandStepIndietro;
            }
        }

        private RelayCommand _annullaCommand;
        public ICommand annullaCommand
        {
            get
            {
                if (_annullaCommand == null)
                {
                    _annullaCommand = new RelayCommand(param => this.annulla(), param => possoAnnullare );
                }
                return _annullaCommand;
            }
        }

        private RelayCommand _applicaCommand;
        public ICommand applicaCommand
        {
            get
            {
                if (_applicaCommand == null)
                {
                    _applicaCommand = new RelayCommand(param => this.applica(), param => possoApplicare );
                }
                return _applicaCommand;
            }
        }

        private RelayCommand _commandStepAvanti;
        public ICommand commandStepAvanti
        {
            get
            {
                if (_commandStepAvanti == null)
                {
                    _commandStepAvanti = new RelayCommand(param => this.stepAvanti(), param => possoStepAvanti );
                }
                return _commandStepAvanti;
            }
        }

		private RelayCommand _scegliereCartellaCommand;
		public ICommand scegliereCartellaCommand {
			get {
				if( _scegliereCartellaCommand == null ) {
					_scegliereCartellaCommand = new RelayCommand( quale => scegliereCartella( quale as string ) );
				}
				return _scegliereCartellaCommand;
			}
		}


        private RelayCommand _cambiareMotoreDataBaseCommand;
        public ICommand cambiareMotoreDataBaseCommand
        {
            get
            {
                if (_cambiareMotoreDataBaseCommand == null)
                {
					_cambiareMotoreDataBaseCommand = new RelayCommand( param => this.cambiareMotoreDatabase() );
                }
                return _cambiareMotoreDataBaseCommand;
            }
        }

        private RelayCommand _abbinaCommand;
        public ICommand abbinaCommand
        {
            get
            {
                if (_abbinaCommand == null)
                {
                    _abbinaCommand = new RelayCommand(param => this.abbinaButton());
                }
                return _abbinaCommand;
            }
        }

        private RelayCommand _rimuoviAbbinamentoCommand;
        public ICommand rimuoviAbbinamentoCommand
        {
            get
            {
                if (_rimuoviAbbinamentoCommand == null)
                {
                    _rimuoviAbbinamentoCommand = new RelayCommand(param => this.rimuoviAbbinamento());
                }
                return _rimuoviAbbinamentoCommand;
            }
        }

        private RelayCommand _testConnectionCommand;
        public ICommand testConnectionCommand
        {
            get
            {
                if (_testConnectionCommand == null)
                {
                    _testConnectionCommand = new RelayCommand(param => this.testConnection(), p => canTestConnection );
                }
                return _testConnectionCommand;
            }
        }

        private RelayCommand _loginCommand;
        public ICommand loginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(param => this.login(),
													 param => possoLogin );
                }
                return _loginCommand;
            }
        }

        private RelayCommand _createDataBaseCommand;
        public ICommand CreateDataBaseCommand
        {
            get
            {
                if (_createDataBaseCommand == null)
                {
                    _createDataBaseCommand = new RelayCommand(param => this.createDataBase(), p => canCreateDatabase );
                }
                return _createDataBaseCommand;
            }
        }

        private RelayCommand _cambiaPasswordCommand;
        public ICommand cambiaPasswordCommand
        {
            get
            {
                if (_cambiaPasswordCommand == null)
                {
                    _cambiaPasswordCommand = new RelayCommand(param => this.cambiaPassword());
                }
                return _cambiaPasswordCommand;
            }
        }

		private RelayCommand _commandRicostruireDb;
		public ICommand commandRicostruireDb {
			get {
				if( _commandRicostruireDb == null ) {
					_commandRicostruireDb = new RelayCommand( param => this.ricostruireDb(),
						                                      param => possoRicostruireDb,
															  false );
				}
				return _commandRicostruireDb;
			}
		}

        #endregion

        #region esecuzioneComandi

        private void stepIndietro()
        {
			--passo;

			// Quando ritorno sulla prima pagina, chiudo tutto perché si può modificare il database.
			if( passo == PassoWiz.MotoreDb )
				chiudiTutto();
        }

        private void stepAvanti()
        {
			if( forseEseguiCambioMotore() == false )
				return;

			++passo;

			// Dalla pagina 1 sto andando sulla 2
			if( passo == PassoWiz.PuntoVendita )
				apriTutto();
        }


		private bool forseEseguiCambioMotore() {

			if( passo != PassoWiz.MotoreDb )
				return true;

			if( motorePrecedente == cfg.motoreDatabase )
				return true;

			bool prosegui = false;

			dialogProvider.ShowConfirmation( "La modifica del motore del database viene eseguita adesso. Confermi?", "Cambio motore database",
				( sino ) => {
					prosegui = sino;
				} );

			if( prosegui ) {

				try {
					eseguiCambioMotore();
					prosegui = true;
				} catch( Exception ee ) {
					dialogProvider.ShowError( "Impossibile modificare il motore del database.\n" + ee.Message, "ERRORE", null );
				}
			}

			return prosegui;
		}

		/// <summary>
		/// In base al motore di database selezionato in questo momento, mi creo una 
		/// connection string di nome "LumenEntities" in memoria, senza salvarla su disco nel .config
		/// </summary>
		void impostaConnectionStringFittizzia() {

			_giornale.Debug( "qq-1" );
			ConnectionStringSettings giusta = ConfigurationManager.ConnectionStrings[qualeConnectionString];
			_giornale.Debug( "qq-2   : giusta=" + giusta );
			int quanti = ConfigurationManager.ConnectionStrings.Count;
			_giornale.Debug( "qq-3   : quanti=" + quanti );

			Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
			_giornale.Debug( "qq-4   :  config=" + config );

			// Mi creo in memoria una connection string anche per me ma senza salvarla
			var test = config.ConnectionStrings.ConnectionStrings["LumenEntities"];
			if( test == null ) {
				test = new ConnectionStringSettings( "LumenEntities", giusta.ConnectionString );
				config.ConnectionStrings.ConnectionStrings.Add( test );
			} else {
				test.ConnectionString = giusta.ConnectionString;
			}
			_giornale.Debug( "qq-5  :  test=" + test );

			try {

				config.Save( ConfigurationSaveMode.Modified );

				_giornale.Debug( "qq-6   :   salvataggio ok" );

				int quanti2 = ConfigurationManager.ConnectionStrings.Count;
				ConfigurationManager.RefreshSection( "connectionStrings" );
				int quanti3 = ConfigurationManager.ConnectionStrings.Count;
				_giornale.Debug( "qq-7" );
			} catch( Exception ee ) {
				_giornale.Warn( "Impossibile salvare .config", ee );
			}

			_giornale.Debug( "Imposto questa connection string: " + test.ConnectionString );
		}

		private void eseguiCambioMotore() {

			const string nomeExe = "Digiphoto.Lumen.UI.exe";

			impostaConnectionStringFittizzia();
			ConnectionStringSettings csGiusta = ConfigurationManager.ConnectionStrings [qualeConnectionString];


			FileInfo [] filesInfo = null;

#if (DEBUG)
			// Probabilmente sono in debug.
			DirectoryInfo dInfo = new DirectoryInfo( Directory.GetCurrentDirectory() );
			DirectoryInfo padre = dInfo.Parent.Parent.Parent;   // Mi trovo nella cartella:  lumen\Digiphoto.Lumen.GestoreConfigurazione.UI\bin\Debug e devo risalire nella cartella dove c'è la solution. Salgo di 3.
			filesInfo = padre.GetFiles( nomeExe, SearchOption.AllDirectories );
#else
			if( File.Exists( nomeExe ) ) {
				FileInfo f = new FileInfo( nomeExe );
				filesInfo = new FileInfo [] { f };
			}
#endif

			if( filesInfo != null ) {
				foreach( FileInfo fileInfo in filesInfo ) {

					// Get the application configuration file.
					Configuration config = ConfigurationManager.OpenExeConfiguration( fileInfo.FullName );
					if( !config.HasFile )
						continue;
					// Get the connection strings section.
					ConnectionStringsSection csSection = config.ConnectionStrings;

					csSection.ConnectionStrings ["LumenEntities"].ConnectionString = csGiusta.ConnectionString;
					config.Save();
				}
			}

		}

		private void scegliereCartella( string quale ) {

			string appo = PathUtil.scegliCartella();
			if( appo != null ) {

				if( quale.Equals( "mask", StringComparison.CurrentCultureIgnoreCase ) )
					cfg.cartellaMaschere = appo;
				else if( quale.Equals( "burn", StringComparison.CurrentCultureIgnoreCase ) )
					cfg.defaultChiavetta = appo;
				else if( quale.Equals( "foto", StringComparison.CurrentCultureIgnoreCase ) )
					cfg.cartellaFoto = appo;
				else if( quale.Equals( "db", StringComparison.CurrentCultureIgnoreCase ) )
					cfg.cartellaDatabase = appo;
				else if( quale.Equals( "spot", StringComparison.CurrentCultureIgnoreCase ) )
					cfg.cartellaPubblicita = appo;
				else if (quale.Equals("crop", StringComparison.CurrentCultureIgnoreCase))
					cfg.cartellaMaschereCrop = appo;
				else
					throw new ArgumentException( "quale cartella : non riconosciuto" );
			}
        }

        private void testConnection()
        {
			_giornale.Debug( "Devo provare a connettermi al db" );

            salvaConfigDB();
			_giornale.Debug( "stop1" );

			impostaConnectionStringFittizzia();
			_giornale.Debug( "stop2" );

			DbUtil dbUtil = new DbUtil( cfg );
			_giornale.Debug( "stop3" );

			string msgErrore;
            if (dbUtil.verificaSeDatabaseUtilizzabile( out msgErrore ))
            {
				_giornale.Debug( "stop4" );
				dialogProvider.ShowMessage( "OK\nConnessione al database riuscita", "Test Connection" );
				_giornale.Info( "Connessione al db riuscita. Tutto ok" );
            }
            else
            {
				dialogProvider.ShowError( msgErrore, "Connessione fallita", null );
            }
        }


        private void selezionaDataSource()
        {

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = ; // Default file name
            //dlg.DefaultExt = ".config"; // Default file extension
            switch (cfg.motoreDatabase)
            {
                case MotoreDatabase.SqlServerCE:
                    dlg.Filter = "Config File (.sdf)|*.sdf"; // Filter files by extension
                    break;
                case MotoreDatabase.SqLite:
                    dlg.Filter = "Config File (.sqlite)|*.sqlite"; // Filter files by extension
                    break;
                case MotoreDatabase.SqlServer:
					dialogProvider.ShowMessage( "Il Data Sorce prevede un Server", "Avviso" );
                    break;
            }

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                DataSource = dlg.FileName;
                
				OnPropertyChanged("DataSource");
				OnPropertyChanged("ConnectionString");
			}
		}

		private void cambiareMotoreDatabase() {

			switch ( cfg.motoreDatabase )
            {
				case MotoreDatabase.SqlServerCE:
					cfg.dbNomeDbVuoto = "dbVuoto.sdf";
					cfg.dbNomeDbPieno = "database.sdf";
                    break;
                case MotoreDatabase.SqLite:
					cfg.dbNomeDbVuoto = "dbVuoto.sqlite";
					cfg.dbNomeDbPieno = "database.sqlite";
                    break;
                case MotoreDatabase.SqlServer:
					cfg.dbNomeDbVuoto = null;
					cfg.dbNomeDbPieno = null;
					dialogProvider.ShowMessage( "Il Data Sorce prevede un Server", "Avviso" );
                    break;
            }

			OnPropertyChanged( "possoCambiareCartellaDb" );
			OnPropertyChanged( "ConnectionString" );
        }

		private String abbinamentiLoaded {
			get {
				return cfg.stampantiAbbinate;
			}
		}

        private void abbinaButton()
        {
			// Aggiungo un nuovo elemento alla collezione
			StampanteAbbinata sa = StampantiAbbinateUtil.create( selettoreStampantiInstallateViewModel.stampanteSelezionata, selettoreFormatoCartaViewModel.formatoCartaSelezionato );
			selettoreFormatoCartaAbbinatoViewModel.formatiCartaAbbinati.Add( sa );
				
			// Aggiorno la stringa serializzata
			string s = selettoreFormatoCartaAbbinatoViewModel.formatiCartaAbbinati.serializzaToString();
			cfg.stampantiAbbinate = s;
        }

        private void rimuoviAbbinamento()
        {
			selettoreFormatoCartaAbbinatoViewModel.removeSelected();

			// Aggiorno la stringa serializzata
			string s = selettoreFormatoCartaAbbinatoViewModel.formatiCartaAbbinati.serializzaToString();
			cfg.stampantiAbbinate = s;
		}

        private void annulla()
        {
			bool esciPure = false;

			dialogProvider.ShowConfirmation( "Sei sicuro di voler uscire senza salvare?", "Uscita", 			
				(confermato) => {
					  esciPure = confermato;
				  } );

			if( esciPure ) {
				chiudiTutto();
				this.CloseCommand.Execute( null );
			}
        }

        private void applica()
        {

			creaEventualiCartelleMancanti();


			string errore = Configurazione.getMotivoErrore( cfg );
			int qquanti = ConfigurationManager.ConnectionStrings.Count;

			if( errore != null ) {
				dialogProvider.ShowError( "Configurazione non valida.\nImpossibile salvare!\n\nMotivo errore:\n" + errore, "ERRORE", null );
			} else {

				try {

					int quanti = saveInfoFisse();
					
					saveUserConfig();

					saveRegistry();


					inserisciEventualePubblicitaLumen();

					inserisciEventualiMaschereCropLumen();

					string msg = "Configurazione utente salvata";
					if( quanti > 0 )
						msg += "\nConfigurazione PdV salvata";

					bool lanciaPrg = false;

					dialogProvider.ShowConfirmation(msg + "\nLanciare applicazione Lumen ?", "Uscita", (
						confermato) =>
					{
						lanciaPrg = confermato;
					});

					if( lanciaPrg )
						System.Diagnostics.Process.Start( "Digiphoto.Lumen.UI.exe" );

					this.OnRequestClose();	

				} catch( DbEntityValidationException dbeve ) {
					
					// Cerco di dare un messaggio di errore più preciso
					_giornale.Warn( "Salvataggio configurazione pdv fallito", dbeve );

					StringBuilder msg = new StringBuilder();
					foreach( var valRes in dbeve.EntityValidationErrors ) {
						if( valRes.IsValid == false )
							foreach( var erro in valRes.ValidationErrors ) {
								msg.Append( "\n* Proprietà " + erro.PropertyName + " non valida!\n   " + erro.ErrorMessage );
							}
					}

					dialogProvider.ShowError( msg.ToString(), dbeve.Message, null );

				} catch( Exception ee ) {
					// Messaggio di errore generico
					_giornale.Warn( "Salvataggio configurazione fallito", ee );
					dialogProvider.ShowError( ee.Message, "ERRORE salvataggio", null );
				}
			}
        }

		private void creaEventualiCartelleMancanti() {

			// Provo a creare la cartella se non esiste
			try {
				if( !Directory.Exists( cfg.cartellaPubblicita ) ) {
					Directory.CreateDirectory( cfg.cartellaPubblicita );
					_giornale.Info( "Creata cartella pubblicità: " + cfg.cartellaPubblicita );
				}
			} catch( Exception ee ) {
				_giornale.Error( "crea cartella pubblicita", ee );
			}

			// Provo a creare la cartella se non esiste
			try
			{
				if (!Directory.Exists(cfg.cartellaMaschereCrop))
				{
					Directory.CreateDirectory(cfg.cartellaMaschereCrop);
					_giornale.Info("Creata cartella maschere crop: " + cfg.cartellaMaschereCrop);
				}
			}
			catch (Exception ee)
			{
				_giornale.Error("crea cartella maschere crop", ee);
			}

		}

		private int saveInfoFisse() {

			// TODO: importante gestire la concorrenza.
			//       Che succede se io qui salvo e da un'altro pc qualcuno sta scaricando
			//       le foto che incrementano il numero dell'ultimo rullino ?

			infoFisseRepository.update( ref _infoFissa );
			return infoFisseRepository.saveChanges();
		}

        private void createDataBase()
        {

			DbUtil qdbUtil = new DbUtil( cfg );

            salvaConfigDB();
            // Se non esiste la cartella per il database, allora la creo.
			qdbUtil.creaCartellaPerDb();

            // Controllo il database. Se non esiste nessuna impostazione diversa, lo creo.
			qdbUtil.copiaDbVuotoSuDbDiLavoro();

			dialogProvider.ShowMessage( "DataBase creato con successo\n" + qdbUtil.nomeFileDbPieno, "Avviso" );
        }

        private void login()
        {
			bool indovinata = false;
#if (DEBUG)
			indovinata = true;
#else
            String administratorPasswordMd5 = Md5.MD5GenerateHash(LoginPassword.ToString());
			indovinata = administratorPasswordMd5.Equals( Properties.Settings.Default.psw );
#endif

			if( indovinata )
            {
                loginEffettuato = true;
				stepAvanti();
            }
            else
            {
				dialogProvider.ShowMessage( "Password ERROR", "Avviso" );
				loginEffettuato = false;
            }

        }

        private void cambiaPassword()
        {
            if (NuovaPassword == NuovaPassword2)
            {
                Properties.Settings.Default.psw = Md5.MD5GenerateHash(NuovaPassword);
                Properties.Settings.Default.Save();
				dialogProvider.ShowMessage( "Password Cambiata", "Avviso" );
            }
            else
            {
				dialogProvider.ShowError( "Le Password sono Diverse", "Avviso", null );
            }
            
        }

		/// <summary>
		/// Se la cartella delle pubblicità non contiene neanche una immagine,
		/// ne metto una io di default
		/// </summary>
		private void inserisciEventualePubblicitaLumen() {
			
			// Non gestisco la pubblicità
			if( cfg.intervalliPubblicita <= 0 )
				return;

			// La cartella non esiste oppure non è scrivibile.
			if( ! PathUtil.isCartellaScrivibile( cfg.cartellaPubblicita ) )
				return;

			// Se ci sono già dei files, non faccio nulla.
			if( Directory.EnumerateFiles( cfg.cartellaPubblicita ).Count() > 0 )
				return;

			// Copio la mia foto di default
			try {
				const string nomeSpot = "Lumen-spot.jpg";
				string nomeDest = Path.Combine( cfg.cartellaPubblicita, nomeSpot );
				string nomeSrc = Path.Combine( "Images", nomeSpot );
				File.Copy( nomeSrc, nomeDest );
				_giornale.Debug( "Preparato spot pubblicitario di default" );
		
			} catch( Exception ee ) {
				_giornale.Error( "Impossibile copiare spot pubblicitario di default: ", ee );
				throw;
			}
		}

		/// <summary>
		/// Se la cartella delle pubblicità non contiene neanche una immagine,
		/// ne metto una io di default
		/// </summary>
		private void inserisciEventualiMaschereCropLumen()
		{

			// La cartella non esiste oppure non è scrivibile.
			if (!PathUtil.isCartellaScrivibile(cfg.cartellaMaschereCrop))
				return;

			// Se ci sono già dei files, non faccio nulla.
			if (Directory.EnumerateFiles(cfg.cartellaMaschereCrop).Count() > 0)
				return;

			// Copio le mie foto di default
			try
			{
				DirectoryInfo info = new DirectoryInfo("Crops");

				FileInfo[] file = info.GetFiles();

				DirectoryInfo[] directory = info.GetDirectories();

				foreach (FileInfo var in file)
				{
					var.CopyTo(Path.Combine(cfg.cartellaMaschereCrop,var.Name));
				}

				_giornale.Debug("Preparato maschere per il crop di default");

			}
			catch (Exception ee)
			{
				_giornale.Error("Impossibile copiare maschere di crop di default: ", ee);
				throw;
			}
		}

		private void copyResource( string resourceName, string file ) {
			using( Stream resource = GetType().Assembly
											  .GetManifestResourceStream( resourceName ) ) {
				if( resource == null ) {
					throw new ArgumentException( "resourceName", "No such resource" );
				}
				using( Stream output = File.OpenWrite( file ) ) {
					resource.CopyTo( output );
				}
			}
		}

        #endregion

    }
}
