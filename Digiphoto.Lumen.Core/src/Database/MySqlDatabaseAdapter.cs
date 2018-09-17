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


		#endregion Proprietà

		#region Metodi



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
		/// i DDL li ho messi nel progetto : Model
		/// </summary>
		/// <param name="nomeRisorsa"></param>
		private void eseguiDDL( string nomeRisorsa ) {

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
				if( ! ncs.EndsWith( ";" ) )
					ncs += ";";
				ncs += "Allow User Variables=True;";
				conn.ConnectionString = ncs;

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

					var s = string.Format( "cmd=%02d esito=%d : %s", ++conta, nRowsAff, comando.Substring( 0, Math.Min( comando.Length, 40 ) ) );
					_giornale.Debug( s );

				}


				conn.Close();
			}
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

		#endregion Metodi
	}
}
