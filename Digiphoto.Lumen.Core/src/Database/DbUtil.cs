using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Data.EntityClient;
using System.IO;
using log4net;
using System.Reflection;
using System.Data;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Config;
using System.Windows.Forms;

namespace Digiphoto.Lumen.Core.Database {

	public class DbUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(DbUtil) );

		public string nomeFileDbVuoto;
		public string nomeFileDbPieno;

		private UserConfigLumen _userConfig;

		public string cartellaDatabase {
			get {
				return _userConfig.dbCartella;
			}
			
		}

		// Per default lo costruisco con la configurazione statica
		public DbUtil() : this( Configurazione.UserConfigLumen ) {
		}

		/** Costruttore statico */
		public DbUtil( UserConfigLumen cfg ) {

			_userConfig = cfg;

			// determino il nome del file del database vuoto (il template di partenza)
			String doveSono = Assembly.GetExecutingAssembly().Location ;

			string appPath = Path.GetDirectoryName( doveSono );
			nomeFileDbVuoto = Path.Combine(appPath, Configurazione.UserConfigLumen.dbNomeDbVuoto);

			nomeFileDbPieno = Path.Combine(cartellaDatabase, Configurazione.UserConfigLumen.dbNomeDbPieno);
		}

		/**
		 * Provo a connettermi fisicamente al database ed eseguo una verifica dello stesso.
		 * Se il db è inutilizzabile, spaccato, o semplicemente assente, allora ritorno
		 * 
		 * return TRUE se riesco a connettermi ed il db è buono ed utilizzabile.
		 *        FALSE se è spaccato oppure non è presente
		 */
		public  bool verificaSeDatabaseUtilizzabile() {

			bool esiste = false;

			try {
				string conString = providerConnectionString;
				_giornale.Debug( "provo connessione db = " + conString );
				using( SqlCeEngine objCeEngine = new SqlCeEngine( conString ) ) {
					esiste = objCeEngine.Verify();
				}
			} catch( Exception ee ) {
				_giornale.Error( "verifica se db utilizzabile fallita", ee );
			}

			return esiste;
		}

		public bool isDatabasEsistente {
			get {
				return System.IO.File.Exists( nomeFileDbPieno );
			}
		}

		/** Controllo se non esiste la cartella dove risiederà il database, allora la creo 
		 */
		public void creaCartellaPerDb() {
			// Controllo se esiste la cartella di base dei dati dell'applicazione
			if( !Directory.Exists( cartellaDatabase ) ) {
				_giornale.Info( "Creo la cartella dei dati perchè non esiste:\r\n" + cartellaDatabase );
				Directory.CreateDirectory( cartellaDatabase );
			}
		}

		/**
		 * Siccome la connectionString dichiarata nella configurazione è quella nel formato dell'Entity Framework,
		 * io ho bisogno di avere solo la dichiarazione del datasource.
		 * La estraggo con una apposita utilità:
		 */
		public String providerConnectionString {
			get {
				string entityConnectionString = ConfigurationManager.ConnectionStrings ["LumenEntities"].ConnectionString;
                //string entityConnectionString = UserConfigLumen.UserConfigConnectionString;
				return ExtractConnectionStringFromEntityConnectionString( entityConnectionString );
			}
		}

		private string ExtractConnectionStringFromEntityConnectionString( string entityConnectionString ) {
			// create a entity connection string from the input
			EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder( entityConnectionString );

			// read the db connectionstring
			return entityBuilder.ProviderConnectionString;
		}

		/**
		 * Copia il db vuoto su quello pieno.
		 * Per qualsiasi problema, ed anche se il file di destinazione esiste gia,
		 * viene sollevata una eccezione
		 */
		public void copiaDbVuotoSuDbDiLavoro() {

			if( !File.Exists( nomeFileDbPieno ) ) {
				_giornale.Info( @"Il database di lavoro\r\n" + nomeFileDbPieno + "\r\nnon esiste. Lo creo partendo dal template vuoto" );

				File.Copy( nomeFileDbVuoto, nomeFileDbPieno );

				_giornale.Debug( "ok copia vuoto -> pieno riuscita" );
			} else {
				throw new InvalidOperationException( "Il database " + nomeFileDbPieno + " esiste già. Copia fallita" );
			}
		
		}

		public Fotografo loadFotografoById( string idFotografo ) {

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
			return dbContext.Fotografi.SingleOrDefault<Fotografo>( ff => ff.id == idFotografo );
		}

	}
}
