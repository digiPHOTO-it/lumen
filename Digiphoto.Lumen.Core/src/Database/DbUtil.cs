using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using System.IO;
using log4net;
using System.Reflection;
using System.Data;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Config;
using System.Data.Common;
using System.Security.AccessControl;
using System.Data.Entity.Core.EntityClient;
using System.Globalization;

namespace Digiphoto.Lumen.Core.Database {

	public class DbUtil {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(DbUtil) );
		public const string VERSIONE_DB_COMPATIBILE = "1.3";

		public string nomeFileDbVuoto;
		public string nomeFileDbPieno;

		private UserConfigLumen _userConfig;

		public string cartellaDatabase {
			get {
				return _userConfig.cartellaDatabase;
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

			if( cfg.dbNomeDbVuoto == null )
				nomeFileDbVuoto = null;
			else
				nomeFileDbVuoto = Path.Combine(appPath, cfg.dbNomeDbVuoto);

			if( cfg.dbNomeDbPieno == null )
				nomeFileDbPieno = null;
			else
				nomeFileDbPieno = Path.Combine(cartellaDatabase, cfg.dbNomeDbPieno);
		}

		/**
		 * Provo a connettermi fisicamente al database ed eseguo una verifica dello stesso.
		 * Se il db è inutilizzabile, spaccato, o semplicemente assente, allora ritorno
		 * 
		 * return TRUE se riesco a connettermi ed il db è buono ed utilizzabile.
		 *        FALSE se è spaccato oppure non è presente
		 */
		public bool verificaSeDatabaseUtilizzabile( out string msgErrore ) {

			bool usabile = false;
			msgErrore = null;

			try {

				_giornale.Debug( "Devo ricavare la factory di questo provider = " + provider );

				DbProviderFactory factory = DbProviderFactories.GetFactory( provider );

				_giornale.Debug( "Ok DbProviderFactory trovata. Ora creo la connessione al db" );

				using( DbConnection conn = factory.CreateConnection() ) {

					_giornale.Debug( "connessione al db creata. Ora apro la apro con questa connection string: " + providerConnectionString );

					conn.ConnectionString = providerConnectionString;

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


					// TODO
					// Solo se stiamo usando Sql CE si può abilitare questo controllo
					// Istanziare però l'engine in modo dinamico perché non voglio tenere il reference

					/*

								try {
									string conString = providerConnectionString;
									if( conString.EndsWith( "sdf" ) ) {
										_giornale.Debug( "provo connessione db = " + conString );
										using( SqlCeEngine objCeEngine = new SqlCeEngine( conString ) ) {
											esiste = objCeEngine.Verify();
										}
									}
								} catch( Exception ee ) {
									_giornale.Error( "verifica se db utilizzabile fallita", ee );
								}
					*/
					_giornale.Info( "OK il database risulta utilizzabile" );
				}
			} catch( Exception ee ) {
				_giornale.Error( "verifica se db utilizzabile fallita", ee );
				msgErrore = ee.Message;
			}

			return usabile;
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
				_giornale.Info( "Creo la cartella dei dati perchè non usabile:\r\n" + cartellaDatabase );
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
				// return ExtractConnectionStringFromEntityConnectionString( entityConnectionString );
				return entityConnectionString; // TODO MYSQL sistemare qui
			}
		}

		public String provider {
			get {
				/*
				string entityConnectionString = ConfigurationManager.ConnectionStrings ["LumenEntities"].ConnectionString;
				return ExtractProviderStringFromEntityConnectionString( entityConnectionString );
				*/
				return "MySql.Data.MySqlClient";    // TODO MYSQL qui da sistemare:
			}
		}

		private string ExtractConnectionStringFromEntityConnectionString( string entityConnectionString ) {
			// create a entity connection string from the input
			EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder( entityConnectionString );

			// read the db connectionstring
			return entityBuilder.ProviderConnectionString;
		}

		private string ExtractProviderStringFromEntityConnectionString( string entityConnectionString ) {
			// create a entity connection string from the input
			EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder( entityConnectionString );

			// read the provider
			return entityBuilder.Provider;
		}

		/**
		 * Copia il db vuoto su quello pieno.
		 * Per qualsiasi problema, ed anche se il file di destinazione esiste gia,
		 * viene sollevata una eccezione
		 */
		public void copiaDbVuotoSuDbDiLavoro() {

			if( !File.Exists( nomeFileDbPieno ) ) {
				_giornale.Info( @"Il database di lavoro\r\n" + nomeFileDbPieno + "\r\nnon usabile. Lo creo partendo dal template vuoto" );

				File.Copy( nomeFileDbVuoto, nomeFileDbPieno );

				_giornale.Debug( "ok copia vuoto -> pieno riuscita" );


				PathUtil.AddFileSecurity( nomeFileDbPieno, FileSystemRights.WriteData|FileSystemRights.WriteData, AccessControlType.Allow );

			} else {
				throw new InvalidOperationException( "Il database " + nomeFileDbPieno + " usabile già. Copia fallita" );
			}
		
		}

		public Fotografo loadFotografoById( string idFotografo ) {

			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
			return dbContext.Fotografi.SingleOrDefault<Fotografo>( ff => ff.id == idFotografo );
		}

		private string eventualiUpgradeBaseDati( DbConnection conn, string versioneAttuale ) {

			if( versioneAttuale == "2.1" ) {

				if( 1==0 && Configurazione.UserConfigLumen.motoreDatabase == MotoreDatabase.SqLite ) {

					// Rinomino la correzione "Maschera" in "Mascheratura"
					string sql = @"update AzioniAutomatiche
								  set correzioniXml = replace( correzioniXml, 'type=""Maschera""', 'type=""Mascheratura""' )";

					var cmd = conn.CreateCommand();
					cmd.CommandText = sql;
					cmd.CommandType = CommandType.Text;
					int conta = cmd.ExecuteNonQuery();

					string sql2 = @"update InfosFisse set versioneDbCompatibile = '" + DbUtil.VERSIONE_DB_COMPATIBILE + "'";
					cmd.CommandText = sql2;
					var conta2 = cmd.ExecuteNonQuery();

					_giornale.Debug( "Aggiornati " + conta + " record. Aggiornamento db " + DbUtil.VERSIONE_DB_COMPATIBILE );
				}

				versioneAttuale = DbUtil.VERSIONE_DB_COMPATIBILE;
			}

			return versioneAttuale;
		}
		
	}
}
