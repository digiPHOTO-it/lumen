using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Core.Database;
using log4net;
using System.Configuration;
using System.Data.EntityClient;
using System.IO;
using Digiphoto.Lumen.Servizi.Vendere;
using System.Windows.Forms;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Config  {

	public sealed class Configurazione {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(Configurazione) );
		private IDictionary<String, String> _nomiServizi;
		bool _autoSistemazione;


		// Codice del fotografo usato per il fotoritocco
		public static readonly string ID_FOTOGRAFO_ARTISTA = "_Photo_Retouch";
		public static readonly string companyName = "digiPHOTO.it";  // si potrebbero leggere dall'Assembly Info
		public static readonly string applicationName = "Lumen";     // si potrebbero leggere dall'Assembly Info

		DbUtil _dbUtil;

		public static string cartellaBaseFoto {
			get {
				string p = Environment.GetFolderPath( Environment.SpecialFolder.CommonPictures );
				return Path.Combine( p, applicationName, "Foto" );
			}
		}

		static Configurazione() {
			UserConfigLumen = caricaUserConfig();
		}

		public static UserConfigLumen caricaUserConfig() {

			// Carico i settaggi che ho appoggiato su un xml esterno
			return UserConfigSerializer.deserialize();
		}


		internal Configurazione() : this( true ) {
		}

		internal Configurazione( bool autoSistemazione ) {

			// Per prima cosa controllo se ho i settaggi. Altrimenti fallisco.
			if( autoSistemazione == false && UserConfigLumen == null )
				throw new ConfigurazioneMancanteException( "Configurazione utente non trovata" );

			_dbUtil = new DbUtil();

			caricaMappaNomiServizi();

			_autoSistemazione = autoSistemazione;

			sostituisciSegnapostoDataDirectoryPerConnectionString();

			if( autoSistemazione ) {
				autoSistemaPerPartenzaDiDefault();
			} else
				verificheConfruenza();

			// Alcuni settaggi sono statici perché non vogliamo che siano cambiati
			// Altri settaggi invece sono di istanza perché così si possono anche modificare al volo (senza renderli persistenti)
			valorizzaSettaggiNonStatici();

			//Valorizzo la compressione del numero della foto
			Fotografia.compNumFoto = UserConfigLumen.compNumFoto;
		}


		///
		/// <summary>
		/// Le foto sono memorizzate in una cartella che chiamiamo Repository.
		/// Questo repository ha un percorso di base, e poi una struttura variabile
		/// che comprende il giorno in cui ... e l'operatore che ha scattato le foto.
		/// </summary>
		public static string cartellaRepositoryFoto {
			get {
				if( String.IsNullOrEmpty( UserConfigLumen.cartellaFoto ) )
					return (Path.Combine( cartellaAppData, "Foto" ));
				else
					return UserConfigLumen.cartellaFoto;
				}
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
		private static void sostituisciSegnapostoDataDirectoryPerConnectionString() {

			// Ora che ho deciso dove sta il database, sostituisco la cartella nella stringa di connessione.
			sostituisciSegnapostoDataDirectoryPerConnectionString( UserConfigLumen.cartellaDatabase );
		}

		private static void sostituisciSegnapostoDataDirectoryPerConnectionString( string cartella ) {
			AppDomain.CurrentDomain.SetData( "DataDirectory", cartella );
		}
		
		private void autoSistemaPerPartenzaDiDefault() {

			_giornale.Debug( "La configurazione attuale non è sufficiente. Devo sistemarla con valori di default" );

			if( UserConfigLumen == null ) {
				UserConfigLumen = creaUserConfig();
			} else {
				// Sistemare eventuali parametri nuovi di release future..
			}

			// ---

			// Se non esiste la cartella per il database, allora la creo.
			_dbUtil.creaCartellaPerDb();

			// Controllo il database. Se non esiste nessuna impostazione diversa, lo creo.
			if( ! _dbUtil.isDatabasEsistente )
				_dbUtil.copiaDbVuotoSuDbDiLavoro();


			// ----
			// Se non esiste la cartella dove mettere i rullini, allora la creo
			if( !Directory.Exists( Configurazione.cartellaBaseFoto ) ) {
				_giornale.Debug( "La cartella contenente le foto non usabile. La creo!\n" + cartellaBaseFoto );

				DirectoryInfo dInfo = Directory.CreateDirectory( cartellaBaseFoto );

				_giornale.Info( "Creata cartella per contenere le foto:\n" + cartellaBaseFoto );
			}
			

		}

		public static Config.UserConfigLumen creaUserConfig() {

			UserConfigLumen userConfig = new UserConfigLumen();

			// Alcuni default NON naturali. Quelli naturali non li nomino.

			// Il più importante è il motore del database
			userConfig.motoreDatabase = MotoreDatabase.SqLite;
			userConfig.dbNomeDbVuoto = "dbvuoto.sqlite";
			userConfig.dbNomeDbPieno = "database.sqlite";

			userConfig.cartellaDatabase = decidiCartellaDatabase();
			userConfig.autoZoomNoBordiBianchi = true;
			userConfig.modoVendita = ModoVendita.Carrello;

			// Le foto e le maschere le metto nella CommonPictures
			string pp = Environment.GetFolderPath( Environment.SpecialFolder.CommonPictures );
			userConfig.cartellaFoto = Path.Combine( pp, applicationName, "Foto" );
			userConfig.cartellaMaschere = Path.Combine( pp, applicationName, "Maschere" );
			userConfig.cartellaMaschereCrop = Path.Combine( userConfig.cartellaMaschere, "Crop" );
			userConfig.cartellaPubblicita = Path.Combine( pp, applicationName, "Pubblicita" );
			
			userConfig.estensioniGrafiche = ".jpg;.jpeg;.png;.tif;.tiff";
			userConfig.editorImmagini = "MSPAINT.EXE";
			userConfig.masterizzaDirettamente = false;
			userConfig.millisIntervalloSlideShow = 2500;
			userConfig.oraCambioGiornata = "05:00";

			userConfig.numRigheProvini = 6;
			userConfig.numColoneProvini = 4;
			userConfig.paginazioneRisultatiGallery = 500;

			userConfig.maxNumFotoMod = 25;
			userConfig.lungFIFOFotoMod = 50;
			userConfig.autoRotazione = true;

			// Geometria di default per lo slideShow
			creaGeometriaSlideShowSDefault(userConfig);

			return userConfig;
		}

		public static void creaGeometriaSlideShowSDefault(UserConfigLumen userConfig)
		{
			userConfig.deviceEnum = 0;

			userConfig.screenHeight = 1024;
			userConfig.screenWidth = 1280;

			userConfig.slideHeight = 400;
			userConfig.slideWidth = 400;
			userConfig.slideLeft = (userConfig.screenWidth - userConfig.slideWidth) / 2; // 312
			userConfig.slideTop = (userConfig.screenHeight - userConfig.slideHeight) / 2; // 440

			userConfig.slideBoundsX = 0;
			userConfig.slideBoundsY = 0;
		}

		bool isValida() {
			return getMotivoErrore() != null;
		}

		/**
		 * Se la configurazione non è utilizzabile per poter far partire correttamente il programma,
		 * allora ritorno una stringa con il motivo.
		 * Altrimenti ritorno null se va tutto bene.
		 */
		public String getMotivoErrore() {

			// Controllo che esista il database vuoto. Mi serve in caso di copia iniziale
			if( !System.IO.File.Exists( _dbUtil.nomeFileDbVuoto ) ) {
				return "il Database template\n" + UserConfigLumen.dbNomeDbVuoto + "\nnon usabile. Probabile installazione rovinata";
			}

			return getMotivoErrore( UserConfigLumen );
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

		public static void SalvaUserConfig() {
			UserConfigSerializer.serializeToFile( UserConfigLumen );
		}

		/// <summary>
		/// E' la cartella dove vengono memorizzati i dati dell'applicazione.
		/// Per esempio ci mettiamo il database.
		/// </summary>
		public static string cartellaAppData {
			get {
				string cd = Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData );
				return (Path.Combine( cd, companyName, applicationName ));
			}
		}

		// TODO rinominare. Non Foto ma Fotografo
        public const String suffissoCartellaFoto = ".Fot";

        public const String suffissoCartellaGiorni = ".Gio";

		/// <summary>
		///  Ritorno un vetto di stringhe contenente le estesioni grafiche ammesse e che riconosco.
		/// </summary>
		public static string [] estensioniGraficheAmmesse {
			get {
				return UserConfigLumen.estensioniGrafiche.Split(';');
			}
		}

		private EditorEsternoConfig _editorEsternoConfig;
		public EditorEsternoConfig editorEsternoConfig {
			get {

				if( _editorEsternoConfig == null ) {

					_editorEsternoConfig = new EditorEsternoConfig();

					if( String.IsNullOrEmpty( UserConfigLumen.editorImmagini ) ||
						File.Exists( UserConfigLumen.editorImmagini ) == false ) {
						_editorEsternoConfig.commandLine = "MSPAINT";
						_editorEsternoConfig.gestisceMultiArgs = false;
					} else {
						_editorEsternoConfig.commandLine = UserConfigLumen.editorImmagini;
						_editorEsternoConfig.gestisceMultiArgs = UserConfigLumen.editorImmaginiMultiArgs;
					}

				}

				return _editorEsternoConfig;
			}
		}

		void valorizzaSettaggiNonStatici() {

			// Gli stampigli sulla foto volendo si possono modificare
			Stampigli s;
			s.giornata =  UserConfigLumen.stampiglioGiornata;
			s.operatore = UserConfigLumen.stampiglioOperatore;
			s.numFoto = UserConfigLumen.stampiglioNumFoto;
			stampigli = s;
		}


		public Stampigli stampigli {
			get;
			private set;
		}

		public static UserConfigLumen UserConfigLumen {
			get;
			set;
		}


		private static string decidiCartellaDatabase() {

			string ret = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ), Configurazione.companyName, Configurazione.applicationName );

			ret = Environment.ExpandEnvironmentVariables( ret );

			return ret;
		}

		/// <summary>
		///  Mi dice se 
		/// </summary>
		/// <param name="userConfig"></param>
		/// <returns></returns>
		public static bool isUserConfigValida( UserConfigLumen userConfig ) {
			return getMotivoErrore( userConfig ) == null;
		}

		public static string getMotivoErrore( UserConfigLumen userConfig ) {

			DbUtil mioDbUtil = new DbUtil( userConfig );

			// Controllo che esista e che sia valido anche il database vero di lavoro
			string msgErrore;
			if( ! mioDbUtil.verificaSeDatabaseUtilizzabile( out msgErrore ) )
				return "Database di lavoro\n" + mioDbUtil.nomeFileDbPieno + "\n" + msgErrore;

			// Controllo che la cartella contenente le foto esista e sia scrivibile
			if( !Directory.Exists( userConfig.cartellaFoto ) ) {
				return( "Cartella foto inesistente: " + userConfig.cartellaFoto );
			}

			if( userConfig.cartellaPubblicita != null && Directory.Exists( userConfig.cartellaPubblicita ) == false )
				return ("Cartella pubblicità inesistente: " + userConfig.cartellaPubblicita);

			return null;
		}


		/// <summary>
		/// Mi dice se devo attivare le personalizzazioni fuori standard per ciccio
		/// </summary>
		public static bool isFuoriStandardCiccio {
			get {
				return UserConfigLumen.fuoriStandard != null && UserConfigLumen.fuoriStandard.Equals( "CICCIO", StringComparison.CurrentCultureIgnoreCase );
			}
		}

		public static InfoFissa infoFissa {
			get;
			internal set;
		}
	}
}
