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
using System.Text.RegularExpressions;

namespace Digiphoto.Lumen.Core.Database {

	public abstract class AbstractDatabaseAdapter : IDatabaseAdapter {

		protected const string VERSIONE_DB_COMPATIBILE = "5";

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
		protected abstract string sostutireSegnapostoConnectionString( string cs, bool definitiva = false );

		protected abstract string schemaName {
			get;
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

		public abstract bool possoDistruggereDatabase {
			get;
		}

		protected virtual DbConnection createConnection() {

			if( !_inizializzato )
				throw new InvalidOperationException( "Adapter non inizializzato" );

			DbConnection conn = factory.CreateConnection();

			conn.ConnectionString = sostutireSegnapostoConnectionString( providerConnectionString );

			// var quale = "LumenEntities-" + cfg.motoreDatabase;
			// var cs = ConfigurationManager.ConnectionStrings[quale];
			_giornale.Debug( "connessione al db creata. Ora la apro con questa connection string: " + conn.ConnectionString );


			return conn;
		}


		public bool verificaSeDatabaseUtilizzabile( out string msgErrore ) {
			bool usabile = false;
			msgErrore = null;

			try {

				using( DbConnection conn = createConnection() )  {

					conn.Open();

					// Compongo il nome della tabella eventualmente con il prefisso dello schema
					string sql = "select versioneDbCompatibile from ";
					if( schemaName != null )
						sql += schemaName + ".";
					sql += "InfosFisse";

					using( DbCommand comm = conn.CreateCommand() ) {
						comm.CommandText = sql;
						using( DbDataReader rdr = comm.ExecuteReader() ) {
							if( rdr.Read() ) {

								string dbVersion = rdr.GetString( 0 );
								rdr.Close();

								_giornale.Info( "Controllo versione DB: Attuale=" + dbVersion + " ; Richiesta=" + VERSIONE_DB_COMPATIBILE );

								// Prima di testare la versione, eseguo upgrade
								string versioneAttuale = eventualiUpgradeBaseDati( conn, dbVersion );

								decimal dVerAttuale = decimal.Parse( versioneAttuale, CultureInfo.InvariantCulture );
								decimal dVerRichiesta = decimal.Parse( VERSIONE_DB_COMPATIBILE, CultureInfo.InvariantCulture );
								if( dVerAttuale < dVerRichiesta ) {
									msgErrore = "Schema del Database non aggiornato. Richiesta aggiornamento alla versione " + VERSIONE_DB_COMPATIBILE;
								} else
									usabile = true;
							} else {
								// Mancano informazioni fisse. Verranno create quando parte entity framework
								usabile = true;
							}
						}
					}

					_giornale.Info( "OK il database risulta utilizzabile" );
				}
			} catch( Exception ee ) {
				_giornale.Info( "verifica se db utilizzabile fallita", ee );
				msgErrore = ee.Message;
			}

			return usabile;
		}

		protected abstract string eventualiUpgradeBaseDati( DbConnection conn, string dbVersion );
			
		public abstract void creareNuovoDatabase();

		public abstract void distruggereDatabase();

		public bool verificareConnessione() {

			bool tuttoBene = false;

			try {

				using( DbConnection conn = createConnection() ) {

					conn.Open();
					string appo = conn.ServerVersion;
					_giornale.Info( "db server version = " + appo );
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


		public void impostaConnectionStringGiusta( string nomeExe ) {

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
				test.ConnectionString = sostutireSegnapostoConnectionString( giusta.ConnectionString, true );
			}

			test.ProviderName = provider;

			config.Save( ConfigurationSaveMode.Modified );

			// Se l'eseguibile è quello corrente, rinfreso i settaggi
			ConfigurationManager.RefreshSection( "connectionStrings" );

			_giornale.Debug( "Imposto questa connection string: " + test.ConnectionString + " per exe: " + nomeExe );

		}

		// La classe reale, potrebbe correggermi la connection string per eventuali particolarità
		protected abstract string eventualeCorrezioneConnectionString( String connectionString );


		/// <summary>
		/// i DDL li ho messi nel progetto : Model
		/// </summary>
		/// <param name="nomeRisorsa"></param>
		protected void eseguiDDL( string nomeRisorsa ) {

			var aa = typeof( Digiphoto.Lumen.Model.LumenEntities ).Assembly;

			string fileContents = null;
			using( var stream = aa.GetManifestResourceStream( nomeRisorsa ) ) {
				TextReader tr = new StreamReader( stream );
				fileContents = tr.ReadToEnd();
			}

			if( fileContents == null )
				throw new ConfigurazioneNonValidaException( "DDL non trovato: " + nomeRisorsa );



			using( DbConnection conn = createConnection() ) {

				// Questo parametro serve a consentire i parametri iniziali dello script (quelli con la chiocciola)
				var ncs = conn.ConnectionString;
				conn.ConnectionString = eventualeCorrezioneConnectionString( ncs );

				conn.Open();


				Regex regRem = new Regex( @"^--\s.*\n", RegexOptions.Multiline );

				// Separo i singoli comandi
				char[] sep = { ';', '\n' };
				string[] comandi = Regex.Split( fileContents, ";\r\n" );
				int conta = 0;

				for( int ii = 0; ii < comandi.Length; ii++ ) {

					// Estraggo il comando, eliminando le righe di commento. Funzionerebbe ugualmente, ma cosi è più bello il log
					var comando = regRem.Replace( comandi[ii], "" );

					if( string.IsNullOrWhiteSpace( comando ) )
						continue;

					DbCommand cmd = conn.CreateCommand();
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = comando;

					int nRowsAff = cmd.ExecuteNonQuery();

					var s = string.Format( "cmd={0:00} affect={1} : {2}", ++conta, nRowsAff, comando.Substring( 0, Math.Min( comando.Length, 40 ) ) );
					_giornale.Debug( s );

				}

				conn.Close();
			}
		}

	}
}
