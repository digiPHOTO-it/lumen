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

        private String calcolaPathUserConfig()
        {
            //Calcolo il percorso in cui vengono memorizzati i settaggi utente
            String userConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            String userConfigFilePath = userConfigPath + @"\digiPHOTO.it\";

            String[] listUserConfigFilePath = Directory.GetDirectories(userConfigFilePath);

            String filePath = "";

            foreach(String path in listUserConfigFilePath){
                String dirName = Path.GetFileName(path);
                // Filtro su Digiphoto.Lumen.UI potrebbe essere necessario filtrare sulla data di creazione
                // ma fose con un MSI installer non serve; se cambio la versione del programma devo cambare 1.0.0.0
                if (dirName.Substring(0, 18).Equals("Digiphoto.Lumen.UI"))
                {
                    filePath = path + @"\1.0.0.0\user.config";
                }
            }

            return filePath;
        }

        private void loadUserConfig()
        {
            String file = calcolaPathUserConfig();

            CodicePuntoVendita = getPropertiesValue(file, "codicePuntoVendita");

            DescrizionePuntoVendita = getPropertiesValue(file, "descrizionePuntoVendita");

            GiorniDeleteFoto = int.Parse(getPropertiesValue(file, "giorniDeleteFoto"));

            CartellaFoto = getPropertiesValue(file, "cartellaFoto");

            EraseFotoMemoryCard = Boolean.Parse(getPropertiesValue( file, "eraseFotoMemoryCard" ));

            ProiettaDiapo = Boolean.Parse(getPropertiesValue(file, "proiettaDiapo"));

            ModoVendita = short.Parse(getPropertiesValue(file, "modoVendita"));

            DestMasterizzaMasterizzatore = (getPropertiesValue(file, "destMasterizza")).Equals("0") ? true : false;

            DestMasterizzaCartella = !DestMasterizzaMasterizzatore;

            MasterizzatoreSelezionato = getPropertiesValue(file, "defaultMasterizzatore");

            DefaultChiavetta = getPropertiesValue(file, "defaultChiavetta");

            ConnectionString = getPropertiesValue(file, "connectionString");

            MotoreDataBase = parseConnectionStringToDriver(ConnectionString);

            NomeDbPieno = getPropertiesValue(file, "dbNomeDbPieno");

			DbCartella = getPropertiesValue( file, "dbCartella" );
            DataSource = DbCartella + @"\" + NomeDbPieno;

			string appo;
			appo = getPropertiesValue( file, "stampiglioGiornata" );
			stampiglioGiornata = appo == null ? false : Boolean.Parse( appo );
			appo = getPropertiesValue( file, "stampiglioOperatore" );
			stampiglioOperatore = appo == null ? false : Boolean.Parse( appo );
			appo = getPropertiesValue( file, "stampiglioNumFoto" );
			stampiglioNumFoto = appo == null ? false : Boolean.Parse( appo );
        }

        private void saveUserConfig()
        {
            String file = calcolaPathUserConfig();

            setPropertiesValue(file, "codicePuntoVendita", CodicePuntoVendita);

            setPropertiesValue(file, "descrizionePuntoVendita", DescrizionePuntoVendita);

            setPropertiesValue(file, "giorniDeleteFoto", ""+GiorniDeleteFoto);

            setPropertiesValue(file, "cartellaFoto", CartellaFoto);

            setPropertiesValue(file, "eraseFotoMemoryCard", ""+EraseFotoMemoryCard);

            setPropertiesValue(file, "proiettaDiapo", ""+ProiettaDiapo);

            ModoVendita = short.Parse(getPropertiesValue(file, "modoVendita"));

            if (DestMasterizzaMasterizzatore)
            {
                setPropertiesValue(file, "destMasterizza", "0");
            }
            else
            {
                setPropertiesValue(file, "destMasterizza", "1");
            }

            setPropertiesValue(file, "defaultMasterizzatore", "" + MasterizzatoreSelezionato);

            setPropertiesValue(file, "defaultChiavetta", DefaultChiavetta);

            salvaConfigDB();

        }

        private void salvaConfigDB()
        {
            String file = calcolaPathUserConfig();

            setPropertiesValue(file, "connectionString", ConnectionString);

            String stringDbCartella = Path.GetDirectoryName(DataSource);

            String stringDbNomePieno = Path.GetFileName(DataSource);

            setPropertiesValue(file, "dbCartella", DbCartella);

            setPropertiesValue(file, "dbNomeDbPieno", stringDbNomePieno);

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

        #region XML_DOCUMENT

        private String getPropertiesValue(String file, String properties){
            XmlDocument myXmlDocument = new XmlDocument();
            if(file.Equals("")){
                MessageBox.Show("Devi eseguire Lumen prima","Avviso");
                Environment.Exit(0);
            }
            myXmlDocument.Load(file);

            XmlNode node;
            node = myXmlDocument.DocumentElement;

            foreach (XmlNode node1 in node.ChildNodes)
            {
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    foreach (XmlNode node3 in node2.ChildNodes)
                    {
                        foreach (XmlNode node4 in node3.Attributes)
                        {
                            if (node4.InnerText.Equals(properties))
                            {
                                return node3.InnerText;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void setPropertiesValue(String file, String properties, String value)
        {
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(file);

            XmlNode node;
            // configuration
            node = myXmlDocument.DocumentElement;
            // userSettings
            foreach (XmlNode node1 in node.ChildNodes)
            {
                // Digiphoto.Lumen.Properties.Settings
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    // setting
                    foreach (XmlNode node3 in node2.ChildNodes)
                    {
                        // setting name=
                        foreach (XmlNode node4 in node3.Attributes)
                        {
                            if (node4.InnerText.Equals(properties))
                            {
                                // value
                                foreach (XmlNode node5 in node3.ChildNodes)
                                {
                                    node5.InnerText = value;
                                }
                            }
                        }
                    }
                }
            }
            myXmlDocument.Save(file);
        }

        #endregion

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

        public int GiorniDeleteFoto
        {
            get;
            set;
        }

        public String CodicePuntoVendita
        {
            get;
            set;
        }

        public String DescrizionePuntoVendita
        {
            get;
            set;
        }

        public String CartellaFoto
        {
            get;
            set;
        }

        public bool EraseFotoMemoryCard
        {
            get;
            set;
        }

        public bool ProiettaDiapo
        {
            get;
            set;
        }

        public short ModoVendita
        {
            get;
            set;
        }

        public Boolean DestMasterizzaMasterizzatore
        {
            get;
            set;
        }

        public Boolean DestMasterizzaCartella
        {
            get;
            set;
        }

        public String DefaultChiavetta
        {
            get;
            set;
        }

        public int MotoreDataBase
        {
            get;
            set;
        }

        public string DbCartella
        {
            get;
            set;
        }

        public string DataSource
        {
            get;
            set;
        }

        public string NomeDbPieno
        {
            get;
            set;
        }

        public String ConnectionString
        {
            get;
            set;
        }

        public bool DbCartellaButton
        {
            get;
            set;
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


		public bool stampiglioOperatore {
			get;
			set;
		}

		public bool stampiglioGiornata {
			get;
			set;
		}

		public bool stampiglioNumFoto {
			get;
			set;
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
            // Notifico i cambiamenti
            OnPropertyChanged("CodicePuntoVendita");
            OnPropertyChanged("DescrizionePuntoVendita");
            OnPropertyChanged("GiorniDeleteFoto");
            OnPropertyChanged("CartellaFoto");
            OnPropertyChanged("EraseFotoMemoryCard");
            OnPropertyChanged("ProiettaDiapo");
            OnPropertyChanged("ModoVendita");
            OnPropertyChanged("DestMasterizzaMasterizzatore");
            OnPropertyChanged("DestMasterizzaCartella");
            OnPropertyChanged("MasterizzatoreSelezionato");
            OnPropertyChanged("DestMasterizzaCartella");
            OnPropertyChanged("DefaultChiavetta");
            OnPropertyChanged("MotoreDataBase");
            OnPropertyChanged("DataSorce");
            OnPropertyChanged("ConnectionString");
        }

        private void applica()
        {
            saveUserConfig();
            System.Windows.MessageBox.Show("configurazione Salvata", "Avviso");
            Digiphoto.Lumen.Config.Configurazione.PrimoAvvioConfiguratore = false;
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
