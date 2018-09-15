using Digiphoto.Lumen.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Database {

	public class MySqlDatabaseAdapter : AbstractDatabaseAdapter {

		public MySqlDatabaseAdapter( UserConfigLumen cfg ) : base ( cfg ) {

			

		}

		protected override DbConnection createConnection() {

			

			return base.createConnection();
		}

		public override string provider {
			get {
				return "MySql.Data.MySqlClient";
			}
		}

		public override void creareNuovoDatabase() {
			// TODO
			throw new NotImplementedException();
		}

		public override bool possoCreareNuovoDatabase {
			get {
				// TODO
				return testConnessione() == true &&
					isSchemaEsistente() == false;
			}
		}


		private bool isSchemaEsistente() {

			bool esiste = false;

			using( DbConnection conn = createConnection() ) {

				try {

					conn.Open();

					string sql = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'lumen'";

					var cmd = conn.CreateCommand();
					cmd.CommandText = sql;
					cmd.CommandType = CommandType.Text;
					object nome = cmd.ExecuteScalar();

					conn.Close();

					esiste = (nome != null);

				} catch( Exception ee ) {
					esiste = false;
				}
			}

			return esiste;
		}

		protected override string sostutireSegnapostoConnectionString( string cs ) {

			return cs.Replace( "|ServerName|", this.cfg.dbNomeServer );
		}
	}
}
