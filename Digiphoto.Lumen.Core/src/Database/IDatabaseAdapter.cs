namespace Digiphoto.Lumen.Core.Database {

	interface IDatabaseAdapter {

		void init();

		string provider {
			get;
		}

		string providerConnectionString {
			get;
		}

		bool possoCreareNuovoDatabase {
			get;
		}

		bool verificaSeDatabaseUtilizzabile( out string msgErrore );


		/// <summary>
		/// Eseguo una connessione di test al database;
		/// </summary>
		/// <returns>true se riesco a connettermi</returns>
		bool testConnessione();

		/// <summary>
		/// Se possibile crea il db vuoto in condizioni di partenza.
		/// Se il db esiste già oppure non può essere creato, allora solleva eccezione.
		/// </summary>
		void creareNuovoDatabase();

		//		void eventualiUpgradeBaseDati( string versioneAttuale );

		/// <summary>
		/// Apro il file .config di configurazione dell'eseguibile indicato nel parametro.
		/// Prendo la stringa di connessione che riguarda il motore attuale,
		/// e la salvo con il nome "LumenEntities" risolvendo eventuali segnaposto per parametri.
		/// </summary>
		/// <param name="nomeExe"></param>
		void impostaConnectionStringFittizzia( string nomeExe );
	}

}
