﻿using Digiphoto.Lumen.Config;
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

	public class SqLiteDatabaseAdapter : AbstractDatabaseAdapter {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( SqLiteDatabaseAdapter ) );

		private const string SCHEMA_NAME = null;
		private const string PROVIDER = "System.Data.SQLite";

		private string nomeFileDbVuoto;
		private string nomeFileDbPieno;
		private string cartellaDatabase;

		public SqLiteDatabaseAdapter( UserConfigLumen cfg ) : base( cfg ) {

			// determino il nome del file del database vuoto (il template di partenza)
			String doveSono = Assembly.GetExecutingAssembly().Location;

			string appPath = Path.GetDirectoryName( doveSono );

			if( cfg.dbNomeDbVuoto == null )
				nomeFileDbVuoto = null;
			else
				nomeFileDbVuoto = Path.Combine( appPath, cfg.dbNomeDbVuoto );

			if( cfg.dbNomeDbPieno == null )
				nomeFileDbPieno = null;
			else
				nomeFileDbPieno = Path.Combine( cfg.cartellaDatabase, cfg.dbNomeDbPieno );

			cartellaDatabase = cfg.cartellaDatabase;

		}

		#region Proprietà

		private string nomeCompleto {
			get {
				return Path.Combine( cfg.cartellaDatabase, cfg.dbNomeDbPieno );
			}
		}

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

		private bool isDatabasEsistente {
			get {
				FileInfo fi = new FileInfo( nomeFileDbPieno );
				return fi.Exists && fi.Length > 0;
			}
		}

		/// <summary>
		/// Se il database già esiste, allora NON posso crearne un'altro
		/// Altrimenti rischierei di eliminare quello che esiste già.
		/// </summary>
		public override bool possoCreareNuovoDatabase {
			get {
				return Directory.Exists( cartellaDatabase ) && (!isDatabasEsistente);
			}
		}

		#endregion Proprietà

		#region Metodi

		protected override DbConnection createConnection() {

			FileInfo fi = new FileInfo( nomeCompleto );
			if( fi.Exists == false || fi.Length == 0 )
				throw new LumenException( "Il database non esiste. Occorre crearlo" );


			// Sostituisco il segnaposto
			AppDomain.CurrentDomain.SetData( "DataDirectory", cfg.cartellaDatabase );

			return base.createConnection();
		}

		/// <summary>
		/// Usando una connection string "template", creo la connection string vera ovvero quella che
		/// si chiama "LumenEntities" che è quella che utilizzerà EntityFramework
		/// </summary>
		protected override string sostutireSegnapostoConnectionString( string cs, bool definitiva ) {

			// Sostituisco il segnaposto nella connection string
			// AppDomain.CurrentDomain.SetData( "DataDirectory", cartellaDatabase );

			return cs.Replace( "|DataDirectory|", cartellaDatabase );
		}

		protected override string eventualiUpgradeBaseDati( DbConnection conn, string versioneAttuale ) {

			base.eventualiUpgradeBaseDati( conn, versioneAttuale );

			if( versioneAttuale == "2.1" ) {

				// Rinomino la correzione "Maschera" in "Mascheratura"
				string sql = @"update AzioniAutomatiche
								set correzioniXml = replace( correzioniXml, 'type=""Maschera""', 'type=""Mascheratura""' )";

				var cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				cmd.CommandType = CommandType.Text;
				int conta = cmd.ExecuteNonQuery();

				string sql2 = @"update InfosFisse set versioneDbCompatibile = '" + VERSIONE_DB_COMPATIBILE + "'";
				cmd.CommandText = sql2;
				var conta2 = cmd.ExecuteNonQuery();

				_giornale.Debug( "Aggiornati " + conta + " record. Aggiornamento db " + VERSIONE_DB_COMPATIBILE );

				versioneAttuale = VERSIONE_DB_COMPATIBILE;
			}

			return versioneAttuale;
		}

		public override void creareNuovoDatabase() {

			// Se non esiste la cartella per il database, allora la creo.
			creaCartellaPerDb();

			// Copio il database template su quello di destinazione vero.
			copiaDbVuotoSuDbDiLavoro();
		}

		/** Controllo se non esiste la cartella dove risiederà il database, allora la creo */
		private void creaCartellaPerDb() {
			// Controllo se esiste la cartella di base dei dati dell'applicazione
			if( !Directory.Exists( cartellaDatabase ) ) {
				_giornale.Info( "Creo la cartella dei dati perchè non usabile:\r\n" + cartellaDatabase );
				Directory.CreateDirectory( cartellaDatabase );
			}
		}

		/**
		 * Copia il db vuoto su quello pieno.
		 * Per qualsiasi problema, ed anche se il file di destinazione esiste gia,
		 * viene sollevata una eccezione
		 */
		private void copiaDbVuotoSuDbDiLavoro() {

			FileInfo fi = new FileInfo( nomeFileDbPieno );

			if( ! fi.Exists || fi.Length == 0 ) {
				_giornale.Info( @"Il database di lavoro\r\n" + nomeFileDbPieno + "\r\nnon esiste. Lo creo partendo dal template vuoto" );

				bool overwrite = (fi.Exists && fi.Length == 0);
				File.Copy( nomeFileDbVuoto, nomeFileDbPieno, overwrite );

				_giornale.Debug( "copia db vuoto -> pieno riuscita" );

				PathUtil.AddFileSecurity( nomeFileDbPieno, FileSystemRights.WriteData | FileSystemRights.WriteData, AccessControlType.Allow );

			} else {
				throw new InvalidOperationException( "Il database " + nomeFileDbPieno + " esiste già. Copia fallita" );
			}

		}

		#endregion Metodi

	}
}