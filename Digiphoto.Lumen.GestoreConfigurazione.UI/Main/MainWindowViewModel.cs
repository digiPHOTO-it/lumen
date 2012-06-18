using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using System.Configuration;
using Digiphoto.Lumen.Servizi.Masterizzare.MyBurner;
using IMAPI2.Interop;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Servizi.Masterizzare;
using System.Windows.Forms;
using System.Data.EntityClient;
using System.Data.SqlServerCe;
using System.Data.SQLite;
using System.Data.SqlClient;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using System.Security;
using Digiphoto.Lumen.GestoreConfigurazione.UI.Util;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Servizi.Vendere;

namespace Digiphoto.Lumen.GestoreConfigurazione.UI
{
    public class MainWindowViewModel : ClosableWiewModel
    {

        public MainWindowViewModel()
        {
            //Blocco l'interfaccia fino al login
			Abilitato = false;
            listaMasterizzatori = new ObservableCollection<String>();
            caricaListaMasterizzatori();
            loadUserConfig();
            setGui();
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

	
		public bool notMasterizzaDirettamente {
			get {
				return ! cfg.masterizzaDirettamente;
			}
			set {
				cfg.masterizzaDirettamente = !(value);
			}
		}

        private void loadUserConfig()
        {
			// La carico da disco, non uso quella statica già caricata dentro Configurazione.
			this.cfg = Configurazione.caricaUserConfig();
        }

        private void saveUserConfig()
        {
			UserConfigSerializer.serializeToFile( cfg );
            salvaConfigDB();
        }

		// TODO rivedere. non so se serve piu
        private void salvaConfigDB()
        {
			AppDomain.CurrentDomain.SetData( "DataDirectory", cfg.dbCartella );
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


        private void setGui()
        {
            rwButton = false;
            fwButton = false;
            applicaButton = false;
            annullaButton = false;
            switch (SelectedTabControlIndex)
            {    
                case 0:
                    configurazione = true;
                    CartaEStampanti = false;
                    PreferenzeUtente = false;
                    Riservato = true;
                    rwButton = false;
                    fwButton = false;
                    applicaButton = false;
                    annullaButton = false;
					DbCartellaButton = true;
                    if (LumenApplication.Instance.avviata)
                    {
                        LumenApplication.Instance.ferma();
                    }
                    break;
                case 1:
                    configurazione = false;
                    CartaEStampanti = true;
                    PreferenzeUtente = false;
                    Riservato = true;
                    rwButton = true;
                    fwButton = true;
                    applicaButton = false;
                    annullaButton = true;
                    LumenApplication.Instance.avvia();
                    using (new UnitOfWorkScope())
                    {
						creaAlcuniDatiDiDefault();
                        loadDataContext();
                    }
                    break;
                case 2:
                    configurazione = false;
                    CartaEStampanti = false;
                    PreferenzeUtente = true;
                    Riservato = true;
                    rwButton = true;
                    fwButton = true;
                    applicaButton = true;
                    annullaButton = true;
                    break;
                case 3:
                    configurazione = false;
                    CartaEStampanti = false;
                    PreferenzeUtente = false;
                    Riservato = true;
                    rwButton = true;
                    fwButton = false;
                    applicaButton = true;
                    annullaButton = true;
                    break;
            }
            OnPropertyChanged("configurazione");
            OnPropertyChanged("CartaEStampanti");
            OnPropertyChanged("PreferenzeUtente");
            OnPropertyChanged("Riservato");
            OnPropertyChanged("rwButton");
            OnPropertyChanged("fwButton");
            OnPropertyChanged("applicaButton");
            OnPropertyChanged("annullaButton");
        }

		private void creaAlcuniDatiDiDefault() {

			// Devo creare un fotografo pre-stabilito per assegnare le foto modificate con GIMP
			IEntityRepositorySrv<Fotografo> repo = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografo>>();
			Fotografo artista = repo.getById( Configurazione.ID_FOTOGRAFO_ARTISTA );
			if( artista == null ) {
				artista = new Fotografo();
				artista.id = Configurazione.ID_FOTOGRAFO_ARTISTA;
				artista.umano = false;
				artista.attivo = true;
				artista.cognomeNome = "Photo Retouch";
				artista.iniziali = "XY";
				artista.note = "used for masks and frames";
				repo.addNew( artista );
				repo.update(artista);
			}
		}


        private SelettoreFormatoCartaViewModel selettoreFormatoCartaViewModel = null;

        private SelettoreFormatoCartaAbbinatoViewModel selettoreFormatoCartaAbbinatoViewModel = null;

        private SelettoreStampantiInstallateViewModel selettoreStampantiInstallateViewModel = null;


        private void loadDataContext(){
           
			selettoreFormatoCartaViewModel = new SelettoreFormatoCartaViewModel();
            
			selettoreFormatoCartaAbbinatoViewModel = new SelettoreFormatoCartaAbbinatoViewModel( cfg.stampantiAbbinate );

            selettoreStampantiInstallateViewModel = new SelettoreStampantiInstallateViewModel();
			

            DataContextFormatoCarta = selettoreFormatoCartaViewModel;

            DataContextAbbinamenti = selettoreFormatoCartaAbbinatoViewModel;

            DataContextStampantiInstallate = selettoreStampantiInstallateViewModel;

            OnPropertyChanged("DataContextFormatoCarta");
            OnPropertyChanged("DataContextAbbinamenti");
            OnPropertyChanged("DataContextStampantiInstallate");
        }

        #region Proprietà

		private bool _abilitato;
		public bool Abilitato
        {
            get
            {
				return _abilitato;
            }
            set{
				if (_abilitato != value)
                {
					_abilitato = value;
					OnPropertyChanged("Abilitato");
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

        public int SelectedTabControlIndex
        {
            get;
            set;
        }

        public bool configurazione
        {
            get;
            set;
        }

        public bool CartaEStampanti
        {
            get;
            set;
        }

        public bool PreferenzeUtente
        {
            get;
            set;
        }

        public bool Riservato
        {
            get;
            set;
        }

        public bool rwButton
        {
            get;
            set;
        }

        public bool fwButton
        {
            get;
            set;
        }

        public bool applicaButton
        {
            get;
            set;
        }

        public bool annullaButton
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
				return ConfigurationManager.ConnectionStrings [cfg.qualeConnectionString].ToString();
			}
		}

		private bool _dbCartellaButton;
		public bool DbCartellaButton
		{
			get
			{
				return _dbCartellaButton;
			}
			set
			{
				if (_dbCartellaButton != value)
				{
					_dbCartellaButton = value;
					OnPropertyChanged("DbCartellaButton");
				}
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

		public bool canCreateDatabase {
			get {
				// Per poter creare il database, non deve esistere
				DbUtil dbUtil = new DbUtil( cfg );
				return ! dbUtil.isDatabasEsistente;
			}
		}

		public bool possoCambiareMotoreDb {
			get {
				return Abilitato ? true : false;
			}
		}

        #endregion

        #region Comandi

        private RelayCommand _rwButtonCommand;
        public ICommand rwButtonCommand
        {
            get
            {
                if (_rwButtonCommand == null)
                {
                    _rwButtonCommand = new RelayCommand(param => this.rwControlTab());
                }
                return _rwButtonCommand;
            }
        }

        private RelayCommand _annullaCommand;
        public ICommand annullaCommand
        {
            get
            {
                if (_annullaCommand == null)
                {
                    _annullaCommand = new RelayCommand(param => this.annulla());
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
                    _applicaCommand = new RelayCommand(param => this.applica());
                }
                return _applicaCommand;
            }
        }

        private RelayCommand _fwButtonCommand;
        public ICommand fwButtonCommand
        {
            get
            {
                if (_fwButtonCommand == null)
                {
                    _fwButtonCommand = new RelayCommand(param => this.fwControlTab());
                }
                return _fwButtonCommand;
            }
        }

        private RelayCommand _cartellaRulliniCommand;
        public ICommand cartellaRulliniCommand
        {
            get
            {
                if (_cartellaRulliniCommand == null)
                {
                    _cartellaRulliniCommand = new RelayCommand(param => this.selezionaCartellaRullini());
                }
                return _cartellaRulliniCommand;
            }
        }

        private RelayCommand _desMasterizzaCartellaCommand;
        public ICommand desMasterizzaCartellaCommand
        {
            get
            {
                if (_desMasterizzaCartellaCommand == null)
                {
                    _desMasterizzaCartellaCommand = new RelayCommand(param => this.selezionaCartellaMasterizza());
                }
                return _desMasterizzaCartellaCommand;
            }
        }

        private RelayCommand _dbCartellaCommand;
        public ICommand DbCartellaCommand
        {
            get
            {
                if (_dbCartellaCommand == null)
                {
                    _dbCartellaCommand = new RelayCommand(param => this.selezionaDbCartella());
                }
                return _dbCartellaCommand;
            }
        }


        private RelayCommand _motoreDataBaseCommand;
        public ICommand MotoreDataBaseCommand
        {
            get
            {
                if (_motoreDataBaseCommand == null)
                {
                    _motoreDataBaseCommand = new RelayCommand(param => this.abilitaDataSorceButton());
                }
                return _motoreDataBaseCommand;
            }
        }

        private RelayCommand _abbinaCommand;
        public ICommand abbinaCommand
        {
            get
            {
                if (_abbinaCommand == null)
                {
                    _abbinaCommand = new RelayCommand(param => this.abbinaButton() , param => true, false );
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
                    _testConnectionCommand = new RelayCommand(param => this.testConnection());
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
                    _loginCommand = new RelayCommand(param => this.login());
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

        private void rwControlTab()
        {
            if (SelectedTabControlIndex > 0)
            {
                SelectedTabControlIndex--;
                OnPropertyChanged("SelectedTabControlIndex");
                setGui();
            }
        }

        private void fwControlTab()
        {
            if (SelectedTabControlIndex < 4)
            {
                SelectedTabControlIndex++;
                OnPropertyChanged("SelectedTabControlIndex");
                setGui();
            }
        }

        private void selezionaCartellaRullini(){
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                cfg.cartellaFoto = dlg.SelectedPath;
            }    
        }

        private void selezionaCartellaMasterizza()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                cfg.defaultChiavetta = dlg.SelectedPath;
                OnPropertyChanged("cfg.defaultChiavetta");
            }    
        }

        private void testConnection()
        {
            salvaConfigDB();

			DbUtil dbUtil = new DbUtil( cfg );

            if (dbUtil.verificaSeDatabaseUtilizzabile())
            {
                dialogProvider.ShowMessage("OK\nConnessione al database riuscita","Test Connection");
            }
            else
            {
				dialogProvider.ShowMessage( "ERRORE CONNESSIONE", "Test Connection" );
            }
        }

        private void selezionaDbCartella()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                cfg.dbCartella = dlg.SelectedPath;
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

        private void abilitaDataSorceButton()
        {
				
			switch ( cfg.motoreDatabase )
            {
                case MotoreDatabase.SqlServerCE:
					cfg.dbNomeDbVuoto = "dbVuoto.sdf";
					cfg.dbNomeDbPieno = "database.sdf";
                    DbCartellaButton = true;
                    break;
                case MotoreDatabase.SqLite:
					cfg.dbNomeDbVuoto = "dbVuoto.sqlite";
					cfg.dbNomeDbPieno = "database.sqlite";
                    DbCartellaButton = true;
                    break;
                case MotoreDatabase.SqlServer:
					cfg.dbNomeDbVuoto = null;
					cfg.dbNomeDbPieno = null;
					dialogProvider.ShowMessage( "Il Data Sorce prevede un Server", "Avviso" );
                    DbCartellaButton = false;
                    break;
            }

            OnPropertyChanged("DbCartellaButton");
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

			if( esciPure )
				this.CloseCommand.Execute( null );
        }

        private void applica()
        {
			string errore = Configurazione.getMotivoErrore( cfg );

			if( errore != null ) {
				dialogProvider.ShowError( "Configurazione non valida.\nImpossibile salvare!\n\nMotivo errore:\n" + errore, "ERRORE", null );
			} else {
				saveUserConfig();
				dialogProvider.ShowMessage( "OK\nConfigurazione Salvata", "Avviso" );
			}
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
                Abilitato = true;
				fwButton = true;
				annullaButton = true;
				OnPropertyChanged("fwButton");
				OnPropertyChanged("annullaButton");
            }
            else
            {
				dialogProvider.ShowMessage( "Password ERROR", "Avviso" );
				Abilitato = false;
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
