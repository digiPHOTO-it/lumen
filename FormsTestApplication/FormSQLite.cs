using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.EntityClient;
using System.Data.SQLite;
using System.Configuration;
using System.Threading;

namespace FormsTestApplication
{
    public partial class FormSQLite : Form
    {

        private BackgroundWorker backgroundWriteWorker;

        private BackgroundWorker backgroundLoadWorker;

        private EntityConnection entityConnection;

        public FormSQLite()
        {
            InitializeComponent();
            timeLoadText.Text = "5000";
            timeWriteText.Text = "5000";
            percorsoDataBaseField.Text = "Utilizzare Sfoglia per settare il db";
            providerConnectionStringLabel.Text = providerConnectionString;
            entityConnection = new EntityConnection(ConfigurationManager.ConnectionStrings["test3Entities1"].ConnectionString);
        }

        private void initDB()
        {
            test3Entities1 objContext = new test3Entities1(entityConnection);
            Clienti clienti = new Clienti();
            clienti = new Clienti();
            clienti.id = Guid.NewGuid();
            clienti.Nome = "Init" + DateTime.Now;
            objContext.Clienti.AddObject(clienti);
            objContext.SaveChanges();
        }

        private void sfoglia_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                percorsoDataBaseField.Text = openFileDialog.FileName;
            }

            //Imposto la stringa del provider desiderato, in questo cosa SqlServer.

            var sqliteConnString = new SQLiteConnectionStringBuilder();
            sqliteConnString.DataSource = percorsoDataBaseField.Text;
            sqliteConnString.Version = 3;
            System.Diagnostics.Trace.WriteLine("Percorso DataBase" + percorsoDataBaseField.Text);

            //Creo la stringa di connessione dell'entity framework inserendo i file csdl,ssdl,msl,impostando il provider di connessione ( SqlClient ) e impostando //nella proprietà ProviderConnectionString la connectionstring di SqlServer creata precedentemente.

            var entityConnString = new EntityConnectionStringBuilder();
            entityConnString.Metadata = "res://*/Model2.csdl|res://*/Model2.ssdl|res://*/Model2.msl";
            entityConnString.Provider = "System.Data.SQLite";
            entityConnString.ProviderConnectionString = sqliteConnString.ConnectionString;

            //System.Configuration.ConfigurationManager.AppSettings.Set("ConnectionString", entityConnString.ToString());

            //Inizializzo il datamodel impostando come stringa di connessione la proprietà ConnectionString dell'istanza entityConnString
            //EntityConnectionStringBuilder

            providerConnectionStringLabel.Text = entityConnString.ProviderConnectionString;
            System.Diagnostics.Trace.WriteLine("Connection String " + providerConnectionString);

            entityConnection = new EntityConnection(entityConnString.ConnectionString);
            entityConnection.ConnectionString = entityConnString.ConnectionString;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringsSection connSection = (ConnectionStringsSection)config.GetSection("connectionStrings");

            if (connSection != null)
            {
                connSection.ConnectionStrings["test3Entities1"].ConnectionString = entityConnection.ConnectionString;
                config.Save();
                ConfigurationManager.RefreshSection("connectionStrings");
            }
        }

        private void connect_Click(object sender, EventArgs e)
        {
            initDB();
            loadClienti();
        }

        #region Write
        private void write_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("WRITE");
            backgroundWriteWorker = new BackgroundWorker();
            backgroundWriteWorker.WorkerReportsProgress = true;
            backgroundWriteWorker.WorkerSupportsCancellation = true;
            backgroundWriteWorker.DoWork += backgroundWriteWorker_DoWork;
            backgroundWriteWorker.ProgressChanged += backgroundWriteWorker_ProgressChanged;
            backgroundWriteWorker.RunWorkerCompleted += backgroundWriteWorker_RunWorkerCompleted;
            backgroundWriteWorker.RunWorkerAsync();
        }

        private void backgroundWriteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int count = 0;
            System.Diagnostics.Trace.WriteLine("WRITE_DO_WORK");
            while (!backgroundWriteWorker.CancellationPending)
            {
                Thread.Sleep(Int32.Parse(this.timeWriteText.Text));
                backgroundWriteWorker.ReportProgress(count++);
            }
        }

        private void backgroundWriteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("WRITE_PROGRESS_CHANGE");
            using (test3Entities1 dbContext = new test3Entities1(entityConnection))
            {
                Clienti clienti = new Clienti();
                clienti.id = Guid.NewGuid();
                clienti.Nome = descrizioneField.Text + "_" + DateTime.Now + "_" + (Int32.Parse(e.ProgressPercentage.ToString()));
                dbContext.Clienti.AddObject(clienti);
                dbContext.SaveChanges();
            }
        }

        private void backgroundWriteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                System.Diagnostics.Trace.WriteLine("Canceled!");
            }

            else if (!(e.Error == null))
            {
                System.Diagnostics.Trace.WriteLine("Error: " + e.Error.Message);
            }

            else
            {
                System.Diagnostics.Trace.WriteLine("Done!");
            }
        }
        #endregion

        #region Load

        private void load_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("LOAD");
            backgroundLoadWorker = new BackgroundWorker();
            backgroundLoadWorker.WorkerReportsProgress = true;
            backgroundLoadWorker.WorkerSupportsCancellation = true;
            backgroundLoadWorker.DoWork += backgroundLoadWorker_DoWork;
            backgroundLoadWorker.ProgressChanged += backgroundLoadWorker_ProgressChanged;
            backgroundLoadWorker.RunWorkerCompleted += backgroundLoadWorker_RunWorkerCompleted;
            backgroundLoadWorker.RunWorkerAsync();
        }

        private void backgroundLoadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int count = 0;
            System.Diagnostics.Trace.WriteLine("LOAD_DO_WORK");
            while (!backgroundLoadWorker.CancellationPending)
            {
                if ((backgroundLoadWorker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    Thread.Sleep(Int32.Parse(this.timeLoadText.Text));
                    backgroundLoadWorker.ReportProgress(count++);
                }
            }
        }

        private void backgroundLoadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("LOAD_PROGRESS_CHANGE");
            loadClienti();
        }

        private void loadClienti()
        {
            test3Entities1 dbContext = new test3Entities1(entityConnection);
            dataGridView.DataSource = dbContext.Clienti.ToList<Clienti>();
            if (dataGridView.Rows.Count > 1)
            {
                dataGridView.FirstDisplayedCell = dataGridView.Rows[dataGridView.Rows.Count - 1].Cells[0];
            }
        }

        private void backgroundLoadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                System.Diagnostics.Trace.WriteLine("Canceled!");
            }

            else if (!(e.Error == null))
            {
                System.Diagnostics.Trace.WriteLine("Error: " + e.Error.Message);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Done!");
            }
        }

        #endregion

        public void dispose()
        {
            System.Diagnostics.Trace.WriteLine("DISPOSE WORKS");

            if (backgroundWriteWorker.WorkerSupportsCancellation == true)
            {
                backgroundWriteWorker.CancelAsync();
            }

            if (backgroundLoadWorker.WorkerSupportsCancellation == true)
            {
                backgroundLoadWorker.CancelAsync();
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            dispose();
        }

        /**
		 * Siccome la connectionString dichiarata nella configurazione è quella nel formato dell'Entity Framework,
		 * io ho bisogno di avere solo la dichiarazione del datasource.
		 * La estraggo con una apposita utilità:
		 */
        public static String providerConnectionString
        {
            get
            {
                string entityConnectionString = ConfigurationManager.ConnectionStrings["test3Entities1"].ConnectionString;
                return ExtractConnectionStringFromEntityConnectionString(entityConnectionString);
            }
        }

        private static string ExtractConnectionStringFromEntityConnectionString(string entityConnectionString)
        {
            // create a entity connection string from the input
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(entityConnectionString);

            // read the db connectionstring
            return entityBuilder.ProviderConnectionString;
        }
    }
}
