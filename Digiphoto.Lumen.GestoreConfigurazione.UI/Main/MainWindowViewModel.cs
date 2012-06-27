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

namespace Digiphoto.Lumen.GestoreConfigurazione.UI
{

	public enum PassoWiz {

		Login = 0,
		MotoreDb = 1,
		PuntoVendita = 2,
		CartaEStampanti = 3,
		PreferenzeUtente = 4,
		Riservato = 5
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
			passo = PassoWiz.Login;
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
			// La carico da disco, non uso quella statica già caricata dentro Configurazione.
			this.cfg = Configurazione.caricaUserConfig();

			// Se è nullo, significa che è la prima volta che parte il programma, ma non è ancora stata avviata la configurazione.
			if( this.cfg == null ) {
				this.cfg = Configurazione.creaUserConfig();
				Configurazione.UserConfigLumen = this.cfg;
			}

			// Mi parcheggio il Motore di database precedente per fare dei ragionamenti
			motorePrecedente = cfg.motoreDatabase;
        }

        private void saveUserConfig()
        {
			UserConfigSerializer.serializeToFile( cfg );
            salvaConfigDB();
        }

		// TODO rivedere. non so se serve piu
        private void salvaConfigDB()
        {
			AppDomain.CurrentDomain.SetData( "DataDirectory", cfg.cartellaDatabase );
		}

        private void caricaListaMasterizzatori()
        {
            // purtoppo pare che rimpiazzare il reference con uno nuovo, causa dei problemi.
            // Non posso istanziare nuovamente la lista, ma la devo svuotare e ripopolare.
            listaMasterizzatori.Clear();
            BurnerSrvImpl burner = new BurnerSrvImpl();
            burner.start();
            foreach (IDiscRecorder2 masterizzatore in burner.listaMasterizzatori())
            {
                listaMasterizzatori.Add(""+masterizzatore.VolumePathNames.GetValue(0));
            }   
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
					OnPropertyChanged( "isPassoMotoreDb" );
					OnPropertyChanged( "isPassoPuntoVendita" );
					OnPropertyChanged( "isPassoCartaEStampanti" );
					OnPropertyChanged( "isPassoPreferenzeUtente" );
					OnPropertyChanged( "isPassoRiservato" );
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

		public bool possoStepIndietro {
			get {
				return loginEffettuato && passo > 0;
			}
		}

		public bool possoStepAvanti {
			get {
				return loginEffettuato && passo < PassoWiz.Riservato;
			}
		}

		void apriTutto() {

			// Se è la prima volta, avvio tutto
			if( !LumenApplication.Instance.avviata )
				LumenApplication.Instance.avvia( true, qualeConnectionString );


			// Se è la prima volta, avvio tutto
			impostaConnectionStringFittizzia();
			if( this.unitOfWorkScope == null )
				this.unitOfWorkScope = new UnitOfWorkScope( false, qualeConnectionString );

			// carico un pò di dati dal database se per caso esiste già.
			loadDataContextFromDb();
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
                    _loginCommand = new RelayCommand(param => this.login(), param => loginEffettuato == false );
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

			ConnectionStringSettings giusta = ConfigurationManager.ConnectionStrings[qualeConnectionString];
			int quanti = ConfigurationManager.ConnectionStrings.Count;

			Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );

			// Mi creo in memoria una connection string anche per me ma senza salvarla
			var test = config.ConnectionStrings.ConnectionStrings["LumenEntities"];
			if( test == null ) {
				test = new ConnectionStringSettings( "LumenEntities", giusta.ConnectionString );
				config.ConnectionStrings.ConnectionStrings.Add( test );
			} else {
				test.ConnectionString = giusta.ConnectionString;
			}
			config.Save();

			int quanti2 = ConfigurationManager.ConnectionStrings.Count;
			ConfigurationManager.RefreshSection( "connectionStrings" );
			int quanti3 = ConfigurationManager.ConnectionStrings.Count;
		}

		private void eseguiCambioMotore() {

			const string nomeExe = "Digiphoto.Lumen.UI.exe";

			impostaConnectionStringFittizzia();
			ConnectionStringSettings csGiusta = ConfigurationManager.ConnectionStrings [qualeConnectionString];


			FileInfo [] filesInfo;

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
			
			foreach( FileInfo fileInfo in filesInfo ) {

				// Get the application configuration file.
				Configuration config = ConfigurationManager.OpenExeConfiguration( fileInfo.FullName );
				if( ! config.HasFile )
					continue;
				// Get the connection strings section.
				ConnectionStringsSection csSection = config.ConnectionStrings;

				csSection.ConnectionStrings["LumenEntities"].ConnectionString = csGiusta.ConnectionString;
				config.Save();
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
				else
					throw new ArgumentException( "quale cartella : non riconosciuto" );
			}
        }

        private void testConnection()
        {
            salvaConfigDB();
			impostaConnectionStringFittizzia();

			DbUtil dbUtil = new DbUtil( cfg );
			string msgErrore;
            if (dbUtil.verificaSeDatabaseUtilizzabile( out msgErrore ))
            {
                dialogProvider.ShowMessage("OK\nConnessione al database riuscita","Test Connection");
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
			StampanteAbbinata sa = new StampanteAbbinata( selettoreStampantiInstallateViewModel.stampanteSelezionata, selettoreFormatoCartaViewModel.formatoCartaSelezionato );
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
			string errore = Configurazione.getMotivoErrore( cfg );
			int qquanti = ConfigurationManager.ConnectionStrings.Count;

			if( errore != null ) {
				dialogProvider.ShowError( "Configurazione non valida.\nImpossibile salvare!\n\nMotivo errore:\n" + errore, "ERRORE", null );
			} else {

				try {

					int quanti = saveInfoFisse();
					
					saveUserConfig();

					string msg = "Configurazione utente salvata";
					if( quanti > 0 )
						msg += "\nConfigurazione PdV salvata";

					dialogProvider.ShowMessage( msg, "OK" );

				} catch( Exception ee ) {
					dialogProvider.ShowError( ee.Message, "ERRORE salvataggio", null );
				}
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
            String administratorPasswordMd5 = Md5.MD5GenerateHash(LoginPassword.ToString());
            if (administratorPasswordMd5.Equals(Properties.Settings.Default.psw))
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
        #endregion

    }
}
