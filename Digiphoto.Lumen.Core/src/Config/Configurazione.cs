using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Database;
using log4net;
using System.Configuration;
using System.Data.EntityClient;
using System.IO;

namespace Digiphoto.Lumen.Config  {

	internal class Configurazione {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(Configurazione) );
		private IDictionary<String, String> _nomiServizi;
		bool _autoSistemazione;

		private DateTime _dataLegale;

		/**
		 * La data legale, rappresenta la giornata lavorativa e non quella "solare" del calendario.
		 * Infatti è possibile che le foto acquisite alle 2 di notte vadano nel giorno precedente.
		 */

		public DateTime dataLegale {
			get {
				return _dataLegale;
			}

			private set {
				_dataLegale = value;
			}
		}

		public static string cartellaBaseFoto {
			get {
				return Path.Combine( cartellaAppData, "Foto" );
			}
		}
		
		internal Configurazione() : this( true ) {
		}

		internal Configurazione( bool autoSistemazione ) {
		
			caricaMappaNomiServizi();

			_autoSistemazione = autoSistemazione;

			sostituisciSegnapostoDataDirectoryPerConnectionString();

			dataLegale = calcolaDataLegale();


			if( autoSistemazione ) {
				autoSistemaPerPartenzaDiDefault();
			}

			verificheConfruenza();
		}

		/**
		 * Le foto sono memorizzate in una cartella che chiamiamo Repository.
		 * Questo repository ha un percorso di base, e poi una struttura variabile
		 * che comprende il giorno in cui ... e l'operatore che ha scattato le foto.
		 */
		public string getCartellaRepositoryFoto() {

			if( String.IsNullOrEmpty( Properties.Settings.Default.cartellaFoto ) )
				return (Path.Combine( cartellaAppData, "Foto" ));
			else
				return Properties.Settings.Default.cartellaFoto;
		}

		/**
		 * La data legale, rappresenta la giornata lavorativa.
		 * Anche se le foto sono state scattate il giorno 2 all'una di notte, 
		 * queste appartengono alla giornata lavorativa del giorno 1.
		 * L'orario vero in cui fare il cambio della giornata, è scritto nei settaggi
		 */
		private DateTime calcolaDataLegale() {

			DateTime dataLegale = DateTime.Today;
			string oraCambioGiornata = Properties.Settings.Default.oraCambioGiornata;

			if( ! String.IsNullOrEmpty( oraCambioGiornata ) ) { 
				DateTime adesso = DateTime.Now;
				DateTime ieri = DateTime.Today.AddDays( -1 );

				string [] pezzi = oraCambioGiornata.Split( ':' );
				if( pezzi.Length == 2 ) {

					if( adesso.Hour < Int16.Parse(pezzi[0]) )
						dataLegale = ieri;
					else if( adesso.Hour == Int16.Parse(pezzi[0]) && adesso.Minute < Int16.Parse(pezzi[1]) )
						dataLegale = ieri;
				}
			}

			return dataLegale;
		}

		/** Faccio un controllo. Se tutto a posto sto zitto, altrimenti sollevo un eccezione */
		private void verificheConfruenza() {
			String motivoErrore = getMotivoErrore();
			if( motivoErrore != null ) {
				_giornale.Warn( motivoErrore );
				throw new ConfigurazioneNonValidaException( "Mancano dati fondamentali per l'avvio del programma:\n" + motivoErrore );
			}
		}

		public IDictionary<String,String> nomiServizi {
			get { return _nomiServizi; }
		}

		/**
		 * Per non camblare il nome della cartella nella ConnectionString,
		 * devo usare un segnaposto che ora vado a sostituire
		 */
		private void sostituisciSegnapostoDataDirectoryPerConnectionString() {

			// Ora che ho deciso dove sta il database, sostituisco la cartella nella stringa di connessione.
			AppDomain.CurrentDomain.SetData( "DataDirectory", DbUtil.cartellaDatabase );
		}

		
		private void autoSistemaPerPartenzaDiDefault() {

			_giornale.Debug( "La configurazione attuale non è sufficiente. Devo sistemarla con valori di default" );

			// Se non esiste la cartella per il database, allora la creo.
			DbUtil.creaCartellaPerDb();

			// Controllo il database. Se non esiste nessuna impostazione diversa, lo creo.
			DbUtil.copiaDbVuotoSuDbDiLavoro();


			// ----
			// Se non esiste la cartella dove mettere i rullini, allora la creo
			if( !Directory.Exists( Configurazione.cartellaBaseFoto ) ) {
				_giornale.Debug( "La cartella contenente le foto non esiste. La creo!\n" + cartellaBaseFoto );

				DirectoryInfo dInfo = Directory.CreateDirectory( cartellaBaseFoto );

				_giornale.Info( "Creata cartella per contenere le foto:\n" + cartellaBaseFoto );
			}
			

		}

		bool isValida() {
			return getMotivoErrore() != null;
		}

		/**
		 * Se la configurazione non è utilizzabile per poter far partire correttamente il programma,
		 * allora ritorno una stringa con il motivo.
		 * Altrimenti ritorno null se va tutto bene.
		 */
		String getMotivoErrore() {

			// Controllo che esista il database vuoto. Mi serve in caso di copia iniziale
			if( !System.IO.File.Exists( DbUtil.nomeFileDbVuoto ) ) {
				return "il Database template\n" + DbUtil.nomeFileDbVuoto + "\nnon esiste. Probabile installazione rovinata";
			}

			// Controllo che esista e che sia valido anche il database vero di lavoro
			if( !DbUtil.verificaSeDatabaseUtilizzabile() )
				return "Database di lavoro\n" + DbUtil.nomeFileDbPieno + "\nnon trovato, oppure non utilizzabile.";

			// Controllo che la cartella contenente le foto esista e sia scrivibile
			if( !Directory.Exists( cartellaBaseFoto ) ) {
				return( "Cartella foto inesistente: " + cartellaBaseFoto );
			}

			// TODO verificare se la cartella delle foto è scrivibile.

			// tutto bene
			return null;
		}


		private void caricaMappaNomiServizi() {

			_nomiServizi = new Dictionary<String, String>();

			StartupServiziConfigSection section = (StartupServiziConfigSection)ConfigurationManager.GetSection( "StartupServizi" );

			if( section != null ) {
				ServiziCollection items = section.ServiziItems;
				foreach( ServizioElement item in items ) {
					_nomiServizi.Add( item.Interfaccia, item.Implementazione );
				}
			}
		}


		public static string cartellaAppData {
			get {
				string cd = Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData );
				return (Path.Combine( cd, "digiPHOTO", "Lumen" ));
			}
		}

	
	}
}
