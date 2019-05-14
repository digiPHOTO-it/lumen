using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Model;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Database {

	public class MySqlDatabaseAdapter : AbstractDatabaseAdapter {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( MySqlDatabaseAdapter ) );

		private const string SCHEMA_NAME	= "lumen";
		private const string PROVIDER		= "MySql.Data.MySqlClient";


		public MySqlDatabaseAdapter( UserConfigLumen cfg ) : base ( cfg ) {
		}

		#region Proprietà

		public override string provider {
			get {
				return PROVIDER;
			}
		}

		protected override string schemaName {
			get {
				return SCHEMA_NAME;
			}
		}

		public override bool possoCreareNuovoDatabase {
			get {
				return verificareConnessione() == true && testareSchemaEsistente() == false;
			}
		}

		public override bool possoDistruggereDatabase {
			get {
					return verificareConnessione() == true && testareSchemaEsistente() == true;
			}
		}


		#endregion Proprietà

		#region Metodi


		public override void distruggereDatabase() {

			using( DbConnection conn = createConnection() ) {

				conn.Open();

				// Elimino il database
				{
				DbCommand cmd1 = conn.CreateCommand();
				cmd1.CommandType = CommandType.Text;
				cmd1.CommandText = "DROP DATABASE IF EXISTS LUMEN";

				int nRowsAff = cmd1.ExecuteNonQuery();
				_giornale.Info( "drop database lumen. Eliminato tutto. affected = " + nRowsAff );
				}

				// Elimino l'utente specifico
				{
				DbCommand cmd2 = conn.CreateCommand();
				cmd2.CommandType = CommandType.Text;
				cmd2.CommandText = "DROP USER IF EXISTS fotografo";
				int nRowsAff = cmd2.ExecuteNonQuery();
				_giornale.Info( "drop user fototgrafo. Eliminato tutto. affected = " + nRowsAff );
				}

			}
		}

		protected override string eventualeCorrezioneConnectionString( string connectionString ) {

			if( !connectionString.EndsWith( ";" ) )
				connectionString += ";";
			connectionString += "Allow User Variables=True;";

			return connectionString;
		}

		/// <summary>
		/// Ricavo lo script DDL che è presente in un altro assembly della solution,
		/// e lo eseguo sulla connessione mysql attiva.
		/// </summary>
		public override void creareNuovoDatabase() {

			// ---
			_giornale.Debug( "Inizio creazione schema databse MySql tramite DDL" );
			eseguiDDL( "Digiphoto.Lumen.Model.ddl.ddl-create-mysql.sql" );
			_giornale.Info( "Fine creazione schema databse MySql tramite DDL" );

			// ---
			_giornale.Debug( "Inizio creazione schema databse MySql tramite DDL" );
			eseguiDDL( "Digiphoto.Lumen.Model.ddl.ddl-create-mysql-user.sql" );
			_giornale.Info( "Fine creazione utente tramite DDL" );

		}



		/// <summary>
		/// Mi connetto ad database senza uno schema, e leggo le viste di sistema per 
		/// scoprire se lo schema esiste già
		/// </summary>
		/// <returns></returns>
		private bool testareSchemaEsistente() {

			bool esiste = false;

			using( DbConnection conn = createConnection() ) {

				try {

					conn.Open();

					string sql = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '" + SCHEMA_NAME + "'";

					var cmd = conn.CreateCommand();
					cmd.CommandText = sql;
					cmd.CommandType = CommandType.Text;
					object nome = cmd.ExecuteScalar();

					conn.Close();

					esiste = (nome != null);

				} catch( Exception ) {
					esiste = false;
				}
			}

			return esiste;
		}

		/// <summary>
		/// Sostituisco il nome del server, ed aggiungo anche il nome dello schema.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>

		protected override string sostutireSegnapostoConnectionString( string connectionString, bool definitiva ) {

			var cs = connectionString.Replace( "|ServerName|", cfg.dbNomeServer );

			if( definitiva ) {
				cs = String.Format( "server={0};port=3306;database={1};uid=fotografo;pwd=fotografo", cfg.dbNomeServer, schemaName );
			}

			return cs;
		}

		protected override string eventualiUpgradeBaseDati( DbConnection conn, string versioneAttuale ) {

			if( versioneAttuale == "4" ) {
				_giornale.Info( "upgrade base dati da versione " + versioneAttuale + " a " + VERSIONE_DB_COMPATIBILE );
				eseguiDDL( "Digiphoto.Lumen.Model.ddl.ddl-upgrade-mysql-005.sql" );
				_giornale.Info( "Fine upgrade database MySql tramite DDL" );

				versioneAttuale = "5";
			}

			return versioneAttuale;
		}


		#endregion Metodi
	}
}
