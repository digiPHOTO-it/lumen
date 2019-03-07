using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Core.Database;
using log4net;
using System.Configuration;
using System.IO;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Config  {

	public sealed class Configurazione {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(Configurazione) );
		private IDictionary<String, String> _nomiServizi;
		bool _autoSistemazione;


		// Codice del fotografo creato per default
		public static readonly string ID_FOTOGRAFO_DEFAULT = "Operator1";
		public static readonly string companyName = "digiPHOTO.it";  // si potrebbero leggere dall'Assembly Info
		public static readonly string applicationName = "Lumen";     // si potrebbero leggere dall'Assembly Info
		public static readonly string pathBaseRegLumen = "Software\\" + Configurazione.companyName + "\\" + Configurazione.applicationName;
		public static readonly string nomeLogoDefault = "digiPHOTO-logo.png";
		public static readonly string nomeLogoSSDefault = "Lumen-selfservice-logo.png";
		public static readonly string releaseNickname = "Giotto";

		public static String configPath
		{
			get
			{
				return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Configurazione.companyName, Configurazione.applicationName );
			}
		}



		public static string cartellaBaseFoto {
			get {
				string p = Environment.GetFolderPath( Environment.SpecialFolder.CommonPictures );
				return Path.Combine( p, applicationName, "Foto" );
			}
		}

		static Configurazione() {
			UserConfigLumen = caricaUserConfig();
			LastUsedConfigLumen = caricaLastUsedConfig();
		}

		public static UserConfigLumen caricaUserConfig() {

			// Carico i settaggi che ho appoggiato su un xml esterno
			return UserConfigSerializer.deserialize();
		}

		public static LastUsedConfigLumen caricaLastUsedConfig()
		{

			// Carico i settaggi che ho appoggiato su un xml esterno
			return LastUsedConfigSerializer.deserialize();
		}

		internal Configurazione() : this( true ) {
		}

		internal Configurazione( bool autoSistemazione ) {

			// Per prima cosa controllo se ho i settaggi. Altrimenti fallisco.
			if( autoSistemazione == false && UserConfigLumen == null )
				throw new ConfigurazioneMancanteException( "Configurazione utente non trovata" );

			caricaMappaNomiServizi();

			_autoSistemazione = autoSistemazione;


			// sostituisci Segnaposto DataDirectory Per Connection String
			AppDomain.CurrentDomain.SetData( "DataDirectory", UserConfigLumen.cartellaDatabase );


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
				_giornale.Error( motivoErrore );
				throw new ConfigurazioneNonValidaException( "Mancano dati fondamentali per l'avvio del programma:\n" + motivoErrore );
			}
		}

		public IDictionary<String,String> nomiServizi {
			get { return _nomiServizi; }
		}


		private void autoSistemaPerPartenzaDiDefault() {

			_giornale.Debug( "La configurazione attuale non è sufficiente. Devo sistemarla con valori di default" );

			if( UserConfigLumen == null ) {
				UserConfigLumen = creaUserConfig();
			} else {
				// Sistemare eventuali parametri nuovi di release future..
			}

			if (LastUsedConfigLumen == null)
			{
				LastUsedConfigLumen = creaLastUsedConfig();
			}
			else
			{
				// Sistemare eventuali parametri nuovi di release future..
			}

			/*
			 * Il database deve essere a posto in fase di configurazione. Qui è troppo tardi (almeno per mysql)
			DbUtil _dbUtil = new DbUtil();
			if( _dbUtil.possoCreareNuovoDatabase )
				_dbUtil.creareNuovoDatabase();
			*/

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
			// Questo è per MySql
			userConfig.dbNomeServer = "localhost";					// TODO sostituire con LUMEN

			userConfig.cartellaDatabase = decidiCartellaDatabase();
			userConfig.autoZoomNoBordiBianchi = true;
			userConfig.modoVendita = ModoVendita.Carrello;

			// Le foto e le maschere le metto nella CommonPictures
			string pp = Environment.GetFolderPath( Environment.SpecialFolder.CommonPictures );
			userConfig.cartellaFoto = Path.Combine( pp, applicationName, "Foto" );
			userConfig.cartellaMaschere = Path.Combine( pp, applicationName, "Maschere" );
			userConfig.cartellaLoghi = Path.Combine( pp, applicationName, "Loghi" );
			userConfig.cartellaPubblicita = Path.Combine( pp, applicationName, "Pubblicita" );


			userConfig.estensioniGrafiche = ".jpg;.jpeg;.png;.tif;.tiff";
			userConfig.editorImmagini = "MSPAINT.EXE";
			userConfig.masterizzaTarget = MasterizzaTarget.DriveRimovibili;
			userConfig.oraCambioGiornata = "05:00";

			userConfig.numRigheProvini = 6;
			userConfig.numColoneProvini = 4;

			userConfig.maxNumFotoMod = 25;
			userConfig.lungFIFOFotoMod = 50;
			userConfig.autoRotazione = true;

			// Questo è il logo di esempio che verrà distribuito nel pacchetto di installazione.
			userConfig.logoNomeFile = nomeLogoDefault;
			userConfig.logoPercentualeCopertura = 15;

			#region Settagggi per Self-Service

			userConfig.logoNomeFileSelfService = nomeLogoSSDefault;
			userConfig.modoRicercaSS = "fotografi";
			userConfig.filtroFotografiSS = FiltroFotografi.Tutti;

			#endregion Settagggi per Self-Service

			userConfig.sogliaNumFotoConfermaInStampaRapida = 3;  // Se stampo almeno 3 foto chiedo conferma

			// Geometria di default per lo slideShow
			userConfig.geometriaFinestraSlideShow = creaGeometriaSlideShowDefault();

			userConfig.correzioneAltezzaGalleryDueFoto = 50;
			userConfig.tecSogliaStampaProvini = -3;

			userConfig.imprimereAreaDiRispetto = false;
			userConfig.expRatioAreaDiRispetto = "4/3";
			userConfig.stampigliMarginBottom = 20;
			userConfig.stampigliMarginRight = 20;

			// Grandezza del font per stampare gli stampigli sulla foto.
			userConfig.fontSizeStampaFoto = 10;

			// Configurazione delle righe/colonne per ogni stellina della gallery
			userConfig.prefGalleryViste = new Griglia[MAX_STELLINE];
			// 1 stellina
			userConfig.prefGalleryViste[0] = new Griglia { numRighe = 1, numColonne = 1 };
			// 2 stelline
			userConfig.prefGalleryViste[1] = new Griglia { numRighe = 2, numColonne = 4 };
			// 3 stelline
			userConfig.prefGalleryViste[2] = new Griglia { numRighe = 4, numColonne = 6 };

			return userConfig;
		}

		public static Config.LastUsedConfigLumen creaLastUsedConfig()
		{
			LastUsedConfigLumen lastUsedConfig = new LastUsedConfigLumen();

			// Setto i default
			lastUsedConfig.slideShowNumRighe = 1;
			lastUsedConfig.slideShowNumColonne = 2;
			lastUsedConfig.millisIntervalloSlideShow = 2500;
            lastUsedConfig.collassaFiltriInRicercaGallery = true;

			return lastUsedConfig;
		}

		/// <summary>
		/// La prima volta che apro lo S.S. l'utente non ha ancora configurato
		/// la geometria 
		/// </summary>
		/// <returns></returns>
		public static GeometriaFinestra creaGeometriaSlideShowDefault() {
			
			return new GeometriaFinestra() {

				// Imposto dei valori tali per cui ogni monitor di questo mondo può visualizzarli
				deviceEnum = 0,
				fullScreen = false,
				Width = 600,
				Height = 400,
				Left = 20,
				Top = 20
			};

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

#if false
			TODO rivedere questo controllo

			// Controllo che esista il database vuoto. Mi serve in caso di copia iniziale
			if( !System.IO.File.Exists( _dbUtil.nomeFileDbVuoto ) ) {
				return "il Database template\n" + UserConfigLumen.dbNomeDbVuoto + "\nnon usabile. Probabile installazione rovinata";
			}
#endif
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

		public static void salvaAllConfig()
		{
			SalvaUserConfig();
			SalvaLastUsedConfig();
		}

		public static void SalvaUserConfig() {
			UserConfigSerializer.serializeToFile( UserConfigLumen );
		}

		public static void SalvaLastUsedConfig()
		{
			LastUsedConfigSerializer.serializeToFile(LastUsedConfigLumen);
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
		public static readonly int MAX_STELLINE = 3;

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

		public static LastUsedConfigLumen LastUsedConfigLumen
		{
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

			// Controllo che la cartella contenente le foto esista e sia scrivibile
			if( !Directory.Exists( userConfig.cartellaFoto ) ) {
				return( "Cartella foto inesistente: " + userConfig.cartellaFoto );
			}

			if( userConfig.cartellaPubblicita != null && Directory.Exists( userConfig.cartellaPubblicita ) == false )
				return ("Cartella pubblicità inesistente: " + userConfig.cartellaPubblicita);

			if( userConfig.cartellaLoghi != null && Directory.Exists( userConfig.cartellaLoghi ) == false )
				return ("Cartella loghi inesistente: " + userConfig.cartellaLoghi );

			if( userConfig.cartellaLoghi != null ) {
				string nomeLogo = Path.Combine( userConfig.cartellaLoghi, userConfig.logoNomeFile );
				if( !String.IsNullOrWhiteSpace( userConfig.logoNomeFile ) && !File.Exists( nomeLogo ) )
					return  "File logo inesistente" + nomeLogo;
			}

			if( userConfig.imprimereAreaDiRispetto ) {
				// Verifico che la ratio sia != 0

				try {	        
					double ris = CoreUtil.evaluateExpression( userConfig.expRatioAreaDiRispetto ) ;
					if( ris < 1 ) {
						throw new ArgumentException( "valore non valido (es: 4/3)" );
					}

				} catch (Exception ee) {
					return "Rapporto dell'area di rispetto: " + ee.Message;
				}


			}

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
