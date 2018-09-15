using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Util;
using log4net;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;

namespace Digiphoto.Lumen.Core.Database {

	public abstract class AbstractDatabaseAdapter : IDatabaseAdapter {

		protected const string VERSIONE_DB_COMPATIBILE = "1.3";

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( AbstractDatabaseAdapter ) );

		private DbProviderFactory factory;

		private bool _inizializzato = false;

		public AbstractDatabaseAdapter( UserConfigLumen cfg ) {

			this.cfg = cfg;

			_giornale.Debug( "Devo ricavare la factory di questo provider = " + provider );

			factory = DbProviderFactories.GetFactory( provider );

			if( factory == null )
				_giornale.Error( "DbProviderFactory non trovata per provider = " + provider );

		}

		public void init() {
			// creaConnectionStringGiusta();
			_inizializzato = true;
		}

		/// <summary>
		/// Questo metodo, deve creare la connection string giusta, ovvero quella che si chiama "LumenEntities"
		/// che sarà quella che verrà utilizzata da entity framework per lavorare per davvero.
		/// Questi adapter, invece usano delle connection-string template
		/// </summary>
		protected abstract string sostutireSegnapostoConnectionString( string cs );

		


		protected void creaConnectionStringGiusta() {

			string qualeConnectionString = "LumenEntities-" + cfg.motoreDatabase;
			ConnectionStringSettings giusta = ConfigurationManager.ConnectionStrings[qualeConnectionString];

			int quanti = ConfigurationManager.ConnectionStrings.Count;

			// Qui apro la configurazione dell'eseguibile che mi sta invocando
			Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );

			string nuovaCS = sostutireSegnapostoConnectionString( giusta.ConnectionString );

			// Mi creo connection string
			var test = config.ConnectionStrings.ConnectionStrings["LumenEntities"];
			if( test == null ) {
				test = new ConnectionStringSettings( "LumenEntities", nuovaCS );
				config.ConnectionStrings.ConnectionStrings.Add( test );
			} else {
				config.ConnectionStrings.ConnectionStrings.Remove( test );
				test.ConnectionString = nuovaCS;
				config.ConnectionStrings.ConnectionStrings.Add( test );
			}


			config.Save( ConfigurationSaveMode.Modified );


			// int quanti2 = ConfigurationManager.ConnectionStrings.Count;
			ConfigurationManager.RefreshSection( "connectionStrings" );
			// int quanti3 = ConfigurationManager.ConnectionStrings.Count;
			
			_giornale.Debug( "Imposto questa connection string: " + test.ConnectionString );
		}


		public UserConfigLumen cfg {
			get;
			private set;
		}

		public abstract string provider {
			get;
		}


		public String providerConnectionString {
			get {
				string key = "LumenEntities-" + this.cfg.motoreDatabase;
				return ConfigurationManager.ConnectionStrings[key].ConnectionString;
			}
		}

		/// <summary>
		/// Se il database già esiste, allora NON posso crearne un'altro
		/// Altrimenti rischierei di eliminare quello che esiste già.
		/// </summary>
		public abstract bool possoCreareNuovoDatabase {
			get;
		}

		protected virtual DbConnection createConnection() {

			if( !_inizializzato )
				throw new InvalidOperationException( "Adapter non inizializzato" );

			DbConnection conn = factory.CreateConnection();

			conn.ConnectionString = sostutireSegnapostoConnectionString( providerConnectionString );

			// var quale = "LumenEntities-" + cfg.motoreDatabase;
			// var cs = ConfigurationManager.ConnectionStrings[quale];
			_giornale.Debug( "connessione al db creata. Ora apro la apro con questa connection string: " + conn.ConnectionString );


			return conn;
		}


		public bool verificaSeDatabaseUtilizzabile( out string msgErrore ) {
			bool usabile = false;
			msgErrore = null;

			try {

				using( DbConnection conn = createConnection() )  {


					// var sssss = AppDomain.CurrentDomain.GetData( "ServerName" );

					// conn.ConnectionString = "LumenEntities";

					conn.Open();


					string sql = "select versioneDbCompatibile from InfosFisse";
					using( DbCommand comm = conn.CreateCommand() ) {
						comm.CommandText = sql;
						using( DbDataReader rdr = comm.ExecuteReader() ) {
							if( rdr.Read() ) {

								string dbVersion = rdr.GetString( 0 );
								rdr.Close();

								// Prima di testare la versione, eseguo upgrade
								string versioneAttuale = eventualiUpgradeBaseDati( conn, dbVersion );


								_giornale.Info( "Controllo versione DB: Attuale=" + versioneAttuale + " ; Richiesta=" + VERSIONE_DB_COMPATIBILE );
								decimal dVerAttuale = decimal.Parse( versioneAttuale, CultureInfo.InvariantCulture );
								decimal dVerRichiesta = decimal.Parse( VERSIONE_DB_COMPATIBILE, CultureInfo.InvariantCulture );
								if( dVerAttuale < dVerRichiesta ) {
									msgErrore = "Schema del Database non aggiornato. Richiesta aggiornamento alla versione " + VERSIONE_DB_COMPATIBILE;
								} else
									usabile = true;
							} else {
								msgErrore = "Mancano informazioni fisse. Lanciare il Configuratore";
							}
						}
					}

					_giornale.Info( "OK il database risulta utilizzabile" );
				}
			} catch( Exception ee ) {
				_giornale.Error( "verifica se db utilizzabile fallita", ee );
				msgErrore = ee.Message;
			}

			return usabile;
		}

		protected virtual string eventualiUpgradeBaseDati( DbConnection conn, string dbVersion ) {
			return VERSIONE_DB_COMPATIBILE;
		}

		public abstract void creareNuovoDatabase();

		public bool testConnessione() {

			bool tuttoBene = false;

			try {

				using( DbConnection conn = createConnection() ) {

					conn.Open();
					string appo = conn.ServerVersion;
					_giornale.Debug( "db server version = " + appo );
					conn.Close();

					tuttoBene = (appo != null);
				}

			} catch( Exception ee ) {
				// Non segnalo errore, perché questo metodo è solo di test
				_giornale.Debug( "connessione al db fallita", ee );
				tuttoBene = false;
			}

			return tuttoBene;	
		}


		public void impostaConnectionStringFittizzia( string nomeExe ) {

			string key = "LumenEntities-" + this.cfg.motoreDatabase;
			ConnectionStringSettings giusta = ConfigurationManager.ConnectionStrings[key];


			Configuration config = ConfigurationManager.OpenExeConfiguration( nomeExe );
			if( !config.HasFile ) {
				_giornale.Debug( "Non trovato .config dell'exe: " + nomeExe );
				return;
			}


			// Creo o aggiorno la connessione con il nome corretto
			var test = config.ConnectionStrings.ConnectionStrings["LumenEntities"];
			if( test == null ) {

				DbUtil dbUtil = new DbUtil( cfg );
				test = new ConnectionStringSettings( "LumenEntities", giusta.ConnectionString );
				config.ConnectionStrings.ConnectionStrings.Add( test );
			} else {
				test.ConnectionString = sostutireSegnapostoConnectionString( giusta.ConnectionString );
			}

			test.ProviderName = provider;

			config.Save( ConfigurationSaveMode.Modified );

			// Se l'eseguibile è quello corrente, rinfreso i settaggi
			ConfigurationManager.RefreshSection( "connectionStrings" );

			_giornale.Debug( "Imposto questa connection string: " + test.ConnectionString + " per exe: " + nomeExe );

		}

	}
}
