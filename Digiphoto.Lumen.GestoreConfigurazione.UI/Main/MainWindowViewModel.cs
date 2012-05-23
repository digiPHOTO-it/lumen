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

        private void loadUserConfig()
        {
            CodicePuntoVendita = UserConfigLumen.CodicePuntoVendita;

			UserConfigLumen.CodicePuntoVendita = "Test";

			DescrizionePuntoVendita = UserConfigLumen.DescrizionePuntoVendita;

			GiorniDeleteFoto = UserConfigLumen.GiorniDeleteFoto;

			CartellaFoto = UserConfigLumen.CartellaFoto;

			EraseFotoMemoryCard = UserConfigLumen.EraseFotoMemoryCard;

			ProiettaDiapo = UserConfigLumen.ProiettaDiapo;

			ModoVendita = (short)UserConfigLumen.ModoVendita;

			DestMasterizzaMasterizzatore = (UserConfigLumen.DestMasterizza).Equals("0") ? true : false;

            DestMasterizzaCartella = !DestMasterizzaMasterizzatore;

			MasterizzatoreSelezionato = UserConfigLumen.DefaultMasterizzatore;

			DefaultChiavetta = UserConfigLumen.DefaultChiavetta;

			ConnectionString = ConfigurationManager.ConnectionStrings["LumenEntities"].ConnectionString;

            MotoreDataBase = parseConnectionStringToDriver(ConnectionString);

			NomeDbPieno = UserConfigLumen.DbNomeDbPieno;

			DbCartella = UserConfigLumen.DbCartella;

            DataSource = DbCartella + @"\" + NomeDbPieno;
        }

        private void saveUserConfig()
        {
            UserConfigLumen.CodicePuntoVendita = CodicePuntoVendita;

			UserConfigLumen.DescrizionePuntoVendita = DescrizionePuntoVendita;

			UserConfigLumen.GiorniDeleteFoto = GiorniDeleteFoto;

			UserConfigLumen.CartellaFoto = CartellaFoto;

			UserConfigLumen.EraseFotoMemoryCard = EraseFotoMemoryCard;

			UserConfigLumen.ProiettaDiapo = ProiettaDiapo;

            ModoVendita = (short)UserConfigLumen.ModoVendita;

            if (DestMasterizzaMasterizzatore)
            {
				UserConfigLumen.DestMasterizza = "0";
            }
            else
            {
				UserConfigLumen.DestMasterizza = "1";
            }

			UserConfigLumen.DefaultMasterizzatore = MasterizzatoreSelezionato;

			UserConfigLumen.DefaultChiavetta = DefaultChiavetta;

            salvaConfigDB();

        }

        private void salvaConfigDB()
        {
            String stringDbCartella = Path.GetDirectoryName(DataSource);

            String stringDbNomePieno = Path.GetFileName(DataSource);

            UserConfigLumen.DbCartella = DbCartella;

            UserConfigLumen.DbNomeDbPieno = stringDbNomePieno;

			AppDomain.CurrentDomain.SetData( "DataDirectory", DbCartella );
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

        public static int parseConnectionStringToDriver(String connectionString)
        {
            var entityConnString = new EntityConnectionStringBuilder(connectionString);
            //entityConnString.Metadata = "res://*/Model2.csdl|res://*/Model2.ssdl|res://*/Model2.msl";
            //entityConnString.Provider = "System.Data.SQLite";
            //entityConnString.ProviderConnectionString = sqliteConnString.ConnectionString;
            int index = 0;
            //currentProvider = entityConnString.Provider;
            switch (entityConnString.Provider)
            {
                case "System.Data.SqlServerCe.4.0":
                    SqlCeConnectionStringBuilder sqlCeConnString = new SqlCeConnectionStringBuilder(entityConnString.ProviderConnectionString);
                    //textBoxDataSource.Text = sqlCeConnString.DataSource;
                    index = 0;
                    break;
                case "System.Data.SQLite":
                    var sqliteConnString = new SQLiteConnectionStringBuilder(entityConnString.ProviderConnectionString);
                    //textBoxDataSource.Text = sqliteConnString.DataSource;
                    index = 1;
                    break;
                case "System.Data.SQServer":
                    var sqlConnString = new SqlConnectionStringBuilder(entityConnString.ProviderConnectionString);
                    //textBoxDataSource.Text = sqlConnString.DataSource;
                    index = 2;
                    break;
            }
            return index;
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

        private StampantiAbbinateSrvImpl stampantiAbbinateSrvImpl = null;

        private void loadDataContext(){
            selettoreFormatoCartaViewModel = new SelettoreFormatoCartaViewModel();
            selettoreFormatoCartaAbbinatoViewModel = new SelettoreFormatoCartaAbbinatoViewModel();
            selettoreStampantiInstallateViewModel = new SelettoreStampantiInstallateViewModel();
            stampantiAbbinateSrvImpl  = new StampantiAbbinateSrvImpl();

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

		private int _giorniDeleteFoto;
        public int GiorniDeleteFoto
        {
			get
			{
				return _giorniDeleteFoto;
			}
			set
			{
				if(_giorniDeleteFoto!=value){
					_giorniDeleteFoto = value;
					OnPropertyChanged("GiorniDeleteFoto");
				}
			}
        }

		private String _codicePuntoVendita;
		public String CodicePuntoVendita
		{
			get
			{
				return _codicePuntoVendita;
			}
			set
			{
				if (_codicePuntoVendita != value)
				{
					_codicePuntoVendita = value;
					OnPropertyChanged("CodicePuntoVendita");
				}
			}
		}

		private String _descrizionePuntoVendita;
		public String DescrizionePuntoVendita
		{
			get
			{
				return _descrizionePuntoVendita;
			}
			set
			{
				if (_descrizionePuntoVendita != value)
				{
					_descrizionePuntoVendita = value;
					OnPropertyChanged("DescrizionePuntoVendita");
				}
			}
		}

		private String _cartellaFoto;
		public String CartellaFoto
		{
			get
			{
				return _cartellaFoto;
			}
			set
			{
				if (_cartellaFoto != value)
				{
					_cartellaFoto = value;
					OnPropertyChanged("CartellaFoto");
				}
			}
		}

		private Boolean _eraseFotoMemoryCard;
		public Boolean EraseFotoMemoryCard
		{
			get
			{
				return _eraseFotoMemoryCard;
			}
			set
			{
				if (_eraseFotoMemoryCard != value)
				{
					_eraseFotoMemoryCard = value;
					OnPropertyChanged("EraseFotoMemoryCard");
				}
			}
		}

		private Boolean _proiettaDiapo;
		public Boolean ProiettaDiapo
		{
			get
			{
				return _proiettaDiapo;
			}
			set
			{
				if (_proiettaDiapo != value)
				{
					_proiettaDiapo = value;
					OnPropertyChanged("ProiettaDiapo");
				}
			}
		}

		private short _modoVendita;
		public short ModoVendita
		{
			get
			{
				return _modoVendita;
			}
			set
			{
				if (_modoVendita != value)
				{
					_modoVendita = value;
					OnPropertyChanged("ModoVendita");
				}
			}
		}

		private Boolean _destMasterizzaMasterizzatore;
		public Boolean DestMasterizzaMasterizzatore
		{
			get
			{
				return _destMasterizzaMasterizzatore;
			}
			set
			{
				if (_destMasterizzaMasterizzatore != value)
				{
					_destMasterizzaMasterizzatore = value;
					OnPropertyChanged("DestMasterizzaMasterizzatore");
				}
			}
		}

		private String _defaultChiavetta;
		public String DefaultChiavetta
		{
			get
			{
				return _defaultChiavetta;
			}
			set
			{
				if (_defaultChiavetta != value)
				{
					_defaultChiavetta = value;
					OnPropertyChanged("DefaultChiavetta");
				}
			}
		}

		private Boolean _destMasterizzaCartella;
		public Boolean DestMasterizzaCartella
		{
			get
			{
				return _destMasterizzaCartella;
			}
			set
			{
				if (_destMasterizzaCartella != value)
				{
					_destMasterizzaCartella = value;
					OnPropertyChanged("DestMasterizzaCartella");
				}
			}
		}

		private int _motoreDataBase;
		public int MotoreDataBase
		{
			get
			{
				return _motoreDataBase;
			}
			set
			{
				if (_motoreDataBase != value)
				{
					_motoreDataBase = value;
					OnPropertyChanged("MotoreDataBase");
				}
			}
		}

		private String _dbCartella;
		public String DbCartella
		{
			get
			{
				return _dbCartella;
			}
			set
			{
				if (_dbCartella != value)
				{
					_dbCartella = value;
					OnPropertyChanged("DbCartella");
				}
			}
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
				if (_dataSource != value)
				{
					_dataSource = value;
					OnPropertyChanged("DataSource");
				}
			}
		}

		private string _nomeDbPieno;
        public string NomeDbPieno
        {
            get
			{
				return _nomeDbPieno;
			}
            set
			{
				if(_nomeDbPieno != value){
					_nomeDbPieno = value;
					OnPropertyChanged("NomeDbPieno");
				}
			}
        }


		private String _connectionString;
		public String ConnectionString
		{
			get
			{
				return _connectionString;
			}
			set
			{
				if (_connectionString != value)
				{
					_connectionString = value;
					OnPropertyChanged("ConnectionString");
				}
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

        /// <summary>
        /// Il Masterizzatore attualmente selezionato
        /// </summary>
        String _masterizzatoreSelezionato;
        public String MasterizzatoreSelezionato
        {
            get
            {
                return _masterizzatoreSelezionato;
            }
            set
            {
                if (value != _masterizzatoreSelezionato)
                {
                    _masterizzatoreSelezionato = value;
                    OnPropertyChanged("MasterizzatoreSelezionato"); 
                }
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
                    _createDataBaseCommand = new RelayCommand(param => this.createDataBase());
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
                CartellaFoto = dlg.SelectedPath;
                OnPropertyChanged("CartellaFoto");
            }    
        }

        private void selezionaCartellaMasterizza()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DefaultChiavetta = dlg.SelectedPath;
                OnPropertyChanged("DefaultChiavetta");
            }    
        }

        private void testConnection()
        {
            salvaConfigDB();
			
            if (DbUtil.verificaSeDatabaseUtilizzabile())
            {
                MessageBox.Show("OK\n--- Funziona solo per CE ---","Test Connection");
            }
            else
            {
                MessageBox.Show("ERRORE CONNESSIONE","Test Connection");
            }
        }

        private void selezionaDbCartella()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DbCartella = dlg.SelectedPath;
            }   
            var entityConnString = new EntityConnectionStringBuilder(ConnectionString);
            switch (MotoreDataBase)
            {
                case 0:
                    entityConnString.Provider = "System.Data.SqlServerCe.4.0";

                    //entityConnString.ProviderConnectionString = sqliteConnString.ConnectionString;
                    SqlCeConnectionStringBuilder sqlCeConnString = new SqlCeConnectionStringBuilder(entityConnString.ProviderConnectionString);
                    sqlCeConnString.DataSource = DbCartella + @"\" + NomeDbPieno;
                    entityConnString.ProviderConnectionString = sqlCeConnString.ConnectionString;
                    ConnectionString = entityConnString.ConnectionString;
                    break;
                case 1:
                    entityConnString.Provider = "System.Data.SQLite";
                    var sqliteConnString = new SQLiteConnectionStringBuilder(entityConnString.ProviderConnectionString);
                    sqliteConnString.DataSource = DbCartella + @"\" + NomeDbPieno;
                    entityConnString.ProviderConnectionString = sqliteConnString.ConnectionString;
                    ConnectionString = entityConnString.ConnectionString;
                    break;
                case 2:
                    entityConnString.Provider = "System.Data.SQServer";
                    var sqlConnString = new SqlConnectionStringBuilder(entityConnString.ProviderConnectionString);
                    sqlConnString.DataSource = DataSource;
                    entityConnString.ProviderConnectionString = sqlConnString.ConnectionString;
                    ConnectionString = entityConnString.ConnectionString;
                    break;
            }
            OnPropertyChanged("DbCartella");
            OnPropertyChanged("ConnectionString");
        }

        private void selezionaDataSource()
        {

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = ; // Default file name
            //dlg.DefaultExt = ".config"; // Default file extension
            switch (MotoreDataBase)
            {
                case 0:
                    dlg.Filter = "Config File (.sdf)|*.sdf"; // Filter files by extension
                    break;
                case 1:
                    dlg.Filter = "Config File (.sqlite)|*.sqlite"; // Filter files by extension
                    break;
                case 2:
                    System.Windows.MessageBox.Show("Il Data Sorce prevede un Server", "Avviso");
                    break;
            }

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                DataSource = dlg.FileName;
                // Creo la nuova stringa di connessione
                var entityConnString = new EntityConnectionStringBuilder(ConnectionString);

                //entityConnString.Metadata = "res://*/Model2.csdl|res://*/Model2.ssdl|res://*/Model2.msl";

                switch (MotoreDataBase)
                {
                    case 0:
                        entityConnString.Provider = "System.Data.SqlServerCe.4.0";

                        //entityConnString.ProviderConnectionString = sqliteConnString.ConnectionString;
                        SqlCeConnectionStringBuilder sqlCeConnString = new SqlCeConnectionStringBuilder(entityConnString.ProviderConnectionString);
                        sqlCeConnString.DataSource = DbCartella + @"\" + NomeDbPieno;
                        entityConnString.ProviderConnectionString = sqlCeConnString.ConnectionString;
                        ConnectionString = entityConnString.ConnectionString;
                        break;
                    case 1:
                        entityConnString.Provider = "System.Data.SQLite";
                        var sqliteConnString = new SQLiteConnectionStringBuilder(entityConnString.ProviderConnectionString);
                        sqliteConnString.DataSource = DbCartella + @"\" + NomeDbPieno;
                        entityConnString.ProviderConnectionString = sqliteConnString.ConnectionString;
                        ConnectionString = entityConnString.ConnectionString;
                        break;
                    case 2:
                        entityConnString.Provider = "System.Data.SQServer";
                        var sqlConnString = new SqlConnectionStringBuilder(entityConnString.ProviderConnectionString);
                        sqlConnString.DataSource = DataSource;
                        entityConnString.ProviderConnectionString = sqlConnString.ConnectionString;
                        ConnectionString = entityConnString.ConnectionString;
                        break;
                }
            }
            OnPropertyChanged("DataSource");
            OnPropertyChanged("ConnectionString");
        }

        private void abilitaDataSorceButton()
        {
            switch (MotoreDataBase)
            {
                case 0:
                    NomeDbPieno = "database.sdf";
                    DbCartellaButton = true;
                    break;
                case 1:
                    NomeDbPieno = "database.sqlite";
                    DbCartellaButton = true;
                    break;
                case 2:
                    System.Windows.MessageBox.Show("Il Data Sorce prevede un Server", "Avviso");
                    DbCartellaButton = false;
                    break;
            }
            OnPropertyChanged("NomeDbPieno");
            OnPropertyChanged("DbCartellaButton");
        }

        private void abbinaButton()
        {
            using (new UnitOfWorkScope())
            {
                stampantiAbbinateSrvImpl.addAbbinamento(new StampanteAbbinata(selettoreStampantiInstallateViewModel.stampanteSelezionata, selettoreFormatoCartaViewModel.formatoCartaSelezionato));
                stampantiAbbinateSrvImpl.updateAbbinamento();
                selettoreFormatoCartaAbbinatoViewModel = null;
                selettoreFormatoCartaAbbinatoViewModel = new SelettoreFormatoCartaAbbinatoViewModel();
                DataContextAbbinamenti = selettoreFormatoCartaAbbinatoViewModel;
            }
            OnPropertyChanged("DataContextAbbinamenti");
        }

        private void rimuoviAbbinamento()
        {
            using (new UnitOfWorkScope())
            {
                stampantiAbbinateSrvImpl.removeAbbinamentoByIndex(Convert.ToInt32(selettoreFormatoCartaAbbinatoViewModel.SelectedAbbinamentoIndex));
                stampantiAbbinateSrvImpl.updateAbbinamento();
                selettoreFormatoCartaAbbinatoViewModel = null;
                selettoreFormatoCartaAbbinatoViewModel = new SelettoreFormatoCartaAbbinatoViewModel();
                DataContextAbbinamenti = selettoreFormatoCartaAbbinatoViewModel;
            }
            OnPropertyChanged("DataContextAbbinamenti");
        }

        private void annulla()
        {
            loadUserConfig();
        }

        private void applica()
        {
            saveUserConfig();
            System.Windows.MessageBox.Show("configurazione Salvata", "Avviso");
        }

        private void createDataBase()
        {
            salvaConfigDB();
            // Se non esiste la cartella per il database, allora la creo.
            DbUtil.creaCartellaPerDb();
            // Controllo il database. Se non esiste nessuna impostazione diversa, lo creo.
            DbUtil.copiaDbVuotoSuDbDiLavoro();
            MessageBox.Show("DataBase creato con successo in \n " + DbUtil.cartellaDatabase, "Avviso");
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
                MessageBox.Show("Password OK", "Avviso");
            }
            else
            {
                MessageBox.Show("Password ERROR", "Avviso");
				Abilitato = false;
            }

        }

        private void cambiaPassword()
        {
            if (NuovaPassword == NuovaPassword2)
            {
                Properties.Settings.Default.psw = Md5.MD5GenerateHash(NuovaPassword);
                Properties.Settings.Default.Save();
                MessageBox.Show("Password Cambiata", "Avviso");
            }
            else
            {
                MessageBox.Show("Le Password sono Diverse", "Avviso");
            }
            
        }
        #endregion

    }
}
