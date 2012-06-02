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

	public static class DbUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(DbUtil) );

		public static readonly string nomeFileDbVuoto;
		public static readonly string nomeFileDbPieno;

		public static string cartellaDatabase {
			get;
			private set;
		}

		/** Costruttore statico */
		static DbUtil() {
			
			// determino il nome del file del database vuoto (il template di partenza)
			String doveSono = Assembly.GetExecutingAssembly().Location ;

			string appPath = Path.GetDirectoryName( doveSono );
			nomeFileDbVuoto = Path.Combine(appPath, Configurazione.UserConfigLumen.DbNomeDbVuoto);

			cartellaDatabase = DbUtil.decidiCartellaDatabase();

			nomeFileDbPieno = Path.Combine(cartellaDatabase, Configurazione.UserConfigLumen.DbNomeDbPieno);
		}

		/**
		 * Provo a connettermi fisicamente al database ed eseguo una verifica dello stesso.
		 * Se il db è inutilizzabile, spaccato, o semplicemente assente, allora ritorno
		 * 
		 * return TRUE se riesco a connettermi ed il db è buono ed utilizzabile.
		 *        FALSE se è spaccato oppure non è presente
		 */
		public static bool verificaSeDatabaseUtilizzabile() {

			bool esiste = false;

			try {
				string conString = DbUtil.providerConnectionString;
				_giornale.Debug( "provo connessione db = " + conString );
				using( SqlCeEngine objCeEngine = new SqlCeEngine( conString ) ) {
					esiste = objCeEngine.Verify();
				}
			} catch( Exception ee ) {
				_giornale.Error( "verifica se db utilizzabile fallita", ee );
			}

			return esiste;
		}

		public static bool isDatabasEsistente() {		
			return System.IO.File.Exists( nomeFileDbPieno );
		}

		/** Controllo se non esiste la cartella dove risiederà il database, allora la creo 
		 */
		public static void creaCartellaPerDb() {
			// Controllo se esiste la cartella di base dei dati dell'applicazione
			if( !Directory.Exists( DbUtil.cartellaDatabase ) ) {
				_giornale.Info( "Creo la cartella dei dati perchè non esiste:\r\n" + DbUtil.cartellaDatabase );
				Directory.CreateDirectory( DbUtil.cartellaDatabase );
			}
		}

		/**
		 * Siccome la connectionString dichiarata nella configurazione è quella nel formato dell'Entity Framework,
		 * io ho bisogno di avere solo la dichiarazione del datasource.
		 * La estraggo con una apposita utilità:
		 */
		public static String providerConnectionString {
			get {
				string entityConnectionString = ConfigurationManager.ConnectionStrings ["LumenEntities"].ConnectionString;
                //string entityConnectionString = UserConfigLumen.UserConfigConnectionString;
				return ExtractConnectionStringFromEntityConnectionString( entityConnectionString );
			}
		}

		private static string ExtractConnectionStringFromEntityConnectionString( string entityConnectionString ) {
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
		public static void copiaDbVuotoSuDbDiLavoro() {

			if( !File.Exists( nomeFileDbPieno ) ) {
				_giornale.Info( @"Il database di lavoro\r\n" + nomeFileDbPieno + "\r\nnon esiste. Lo creo partendo dal template vuoto" );

				
				File.Copy( nomeFileDbVuoto, nomeFileDbPieno );

				_giornale.Debug( "ok copia vuoto -> pieno riuscita" );
			} else
				_giornale.Debug( "Il database di lavoror\r\n" + nomeFileDbPieno + "\r\nesiste già. Uso quello" );
		
		}

		public static Fotografo loadFotografoById( string idFotografo ) {


			Fotografo f = null;

			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;

			f = dbContext.Fotografi.FirstOrDefault<Fotografo>( ff => ff.id == idFotografo );

			return f;
		}



		private static string decidiCartellaDatabase() {

			string ret = null;

			// Decido la cartella dove risiede il database
			//string cd = Properties.Settings.Default.dbCartella;
			string cd = Configurazione.UserConfigLumen.DbCartella;

			if( String.IsNullOrEmpty( cd ) ) {
				ret = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ), "digiPHOTO", "Lumen" );
			} else {
				ret = Environment.ExpandEnvironmentVariables( cd );
			}

			return ret;
		}


	}
}
