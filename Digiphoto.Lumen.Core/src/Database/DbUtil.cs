using System;
using System.Linq;
using log4net;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Config;
using System.Configuration;

namespace Digiphoto.Lumen.Core.Database {

	public class DbUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( DbUtil ) );


		private UserConfigLumen _userConfig;

		private IDatabaseAdapter _databaseAdapter;



		// Per default lo costruisco con la configurazione statica
		public DbUtil() : this( Configurazione.UserConfigLumen ) {
		}

		/** Costruttore */
		public DbUtil( UserConfigLumen cfg ) {

			_userConfig = cfg;

			if( cfg.motoreDatabase == MotoreDatabase.SqLite )
				this._databaseAdapter = new SqLiteDatabaseAdapter( cfg );
			else if( cfg.motoreDatabase == MotoreDatabase.MySQL )
				this._databaseAdapter = new MySqlDatabaseAdapter( cfg );
			else
				throw new NotImplementedException( "motore database non gestito: " + cfg.motoreDatabase );

			// Inizializzo alcuni aspetti della connection string
			this._databaseAdapter.init();
		}

		/**
		 * Provo a connettermi fisicamente al database ed eseguo una verifica dello stesso.
		 * Se il db è inutilizzabile, spaccato, o semplicemente assente, allora ritorno
		 * 
		 * return TRUE se riesco a connettermi ed il db è buono ed utilizzabile.
		 *        FALSE se è spaccato oppure non è presente
		 */
		public bool verificaSeDatabaseUtilizzabile( out string msgErrore ) {

			return _databaseAdapter.verificaSeDatabaseUtilizzabile( out msgErrore );
		}

		/// <summary>
		/// La connessione al db è diversa tra sqlite e mysql.
		/// In sqlite mi connetto se il db esiste (quindi esiste anche lo schema).
		/// In mysql mi connetto se esiste il db ma NON esiste lo schema.
		/// </summary>
		/// <returns></returns>
		public bool testConessione() {
			return _databaseAdapter.testConnessione();
		}


		public void impostaConnectionStringFittizzia( string nomeExe ) {
			_databaseAdapter.impostaConnectionStringFittizzia( nomeExe );
		}

		public String providerConnectionString {
			get {
				return this._databaseAdapter.providerConnectionString;
			}
		}

		public String provider {
			get {
				return this._databaseAdapter.provider;
			}
		}

		public void creareNuovoDatabase() {
			_databaseAdapter.creareNuovoDatabase();
		}

		public bool possoCreareNuovoDatabase {
			get {
				return _databaseAdapter.possoCreareNuovoDatabase;
			}
		}


	}
}
