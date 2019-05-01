using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.OnRide.UI.Config;
using Digiphoto.Lumen.OnRide.UI.Model;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Core.Servizi.Impronte;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.UI;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;

namespace Digiphoto.Lumen.OnRide.UI {

	public class MainWindowViewModel : ClosableWiewModel, IObserver<ScaricoFotoMsg> {

		protected new static readonly ILog _giornale = LogManager.GetLogger( typeof( MainWindowViewModel ) );

		public MainWindowViewModel( IDialogProvider dialogProvider ) {

			this.dialogProvider = dialogProvider;

			Init();
		}

		#region Proprietà

		private Maschera _maschera;
		public Maschera maschera {
			get {
				return _maschera;
			}
			private set {

				if( value != _maschera ) {
					_maschera = value;
					OnPropertyChanged( "maschera" );
					OnPropertyChanged( "nomeCompletoMaschera" );
					OnPropertyChanged( "bytesProvinoMaschera" );
				}
			}
		}

		private bool _isMascheraAttiva;
		public bool isMascheraAttiva {
			get {
				return _isMascheraAttiva;
			}
			set {

				if( value != _isMascheraAttiva ) {
					_isMascheraAttiva = value;
					OnPropertyChanged( "isMascheraAttiva" );
				}
			}
		}

		private int _totFotoAcquisite;
		public int totFotoAcquisite {
			get {
				return _totFotoAcquisite;
			}
			private set {
				if( _totFotoAcquisite != value ) {
					_totFotoAcquisite = value;
					OnPropertyChanged( "totFotoAcquisite" );
				}
			}
		}

		private int _totImpronteAcquisite;
		public int totImpronteAcquisite {
			get {
				return _totImpronteAcquisite;
			}
			private set {
				if( _totImpronteAcquisite != value ) {
					_totImpronteAcquisite = value;
					OnPropertyChanged( "totImpronteAcquisite" );
				}
			}
		}
		

		public string nomeCompletoMaschera {
			get {
				return maschera == null ? null : Path.Combine( PathUtil.getCartellaMaschera( FiltroMask.MskSingole ), maschera.nomeFile );
			}
		}

		public byte[] bytesProvinoMaschera {

			get {
				return maschera == null ? null : maschera.imgProvino.getBytes();
			}
		}

		public bool isRunning {
			get {
				return fileSystemWatcher != null ? fileSystemWatcher.EnableRaisingEvents : false;
			}
		}

		private FileSystemWatcher fileSystemWatcher {
			get;
			set;
		}

		public Fotografo fotografoOnRide {
			get;
			private set;
		}

		public string cartellaOnRide {
			get;
			set;
		}

		public ListCollectionView fotoItemsCW {
			get;
			private set;
		}

		public IScaricatoreFotoSrv scaricatoreFotoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IScaricatoreFotoSrv>();
			}
		}

		public IFotoRitoccoSrv fotoRitoccoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}

		public IGestoreImmagineSrv gestoreImmagineSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			}
		}

		public IEntityRepositorySrv<Fotografo> fotografiReporitorySrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografo>>();
			}
		}

		private ObservableCollectionEx<FotoItem> fotoItems {

			get;
			set;
		}

		bool possoAcquisireFoto {
			get {
				return fotoItemsCW.Count > 0;
			}
		}

		public UserConfigOnRide userConfigOnRide {
			get;
			private set;
		}

		public string scannerImpronteNome {
			set; get;
		}

		private bool _scannerImprontePresente;
		public bool scannerImprontePresente {
			get {
				return _scannerImprontePresente;
			}
			set {
				if( _scannerImprontePresente != value ) {
					_scannerImprontePresente = value;
					OnPropertyChanged( "scannerImprontePresente" );
				}
			}
		}

		private string _fileNameBmpImpronta;
		public string fileNameBmpImpronta {
			get {
				return _fileNameBmpImpronta;
			}
			set {
				if( _fileNameBmpImpronta != value ) {
					_fileNameBmpImpronta = value;
					OnPropertyChanged( "fileNameBmpImpronta" );
				}
			}
		}

		#endregion Proprietà

		#region Metodi

		private void Init() {

			this.cartellaOnRide = Configurazione.UserConfigLumen.cartellaOnRide;
			if( !Directory.Exists( cartellaOnRide ) ) {
				var msg = "Cartella per modalità OnRide non valida: " + cartellaOnRide;
				_giornale.Error( msg );
				throw new LumenException( msg );
			}

			// Devo caricare le preferenze
			string nomeFileMsk = null;
			try {
				userConfigOnRide = Config.UserConfigSerializer.deserialize();
				if( userConfigOnRide != null ) {
					this.isMascheraAttiva = userConfigOnRide.isMascheraAttiva;
					nomeFileMsk = userConfigOnRide.nomeMaschera;
					_giornale.Info( "Caricate preferenze utente" );
				} else
					userConfigOnRide = new UserConfigOnRide();

			} catch( Exception ee ) {
				_giornale.Error( "Caricamento preferenze utente", ee );
			}

			using( new UnitOfWorkScope() ) {

				if( userConfigOnRide.idFotografo != null ) {
					// Lo carico
					fotografoOnRide = UnitOfWorkScope.currentDbContext.Fotografi.SingleOrDefault( f => f.id == userConfigOnRide.idFotografo && f.umano == false && f.attivo == true );
				}

				if( fotografoOnRide == null ) {
					// Carico il fotografo dal db (in questo caso non è umano, ma di tipo automatico)
					int conta = UnitOfWorkScope.currentDbContext.Fotografi.Count( f => f.umano == false && f.attivo == true );
					if( conta == 1 ) {
						fotografoOnRide = UnitOfWorkScope.currentDbContext.Fotografi.Single( f => f.umano == false && f.attivo == true );
						userConfigOnRide.idFotografo = fotografoOnRide.id;
					} else if( conta == 0 ) {
						var msg = "Nessun fotografo attivo per modalità OnRide (automatico)";
						_giornale.Error( msg );
						throw new LumenException( msg );
					} 

				}

				if( fotografoOnRide == null ) {
					// se la configurazione è vuota, allora devo permettere all'utente di inserirla.	
				}

				// Se ho deciso di usare lo scanner, lo inizializzo
				if( userConfigOnRide.scannerImpronteGestito ) {
					refreshStatoScanner();
				}

			}

			// Ascolto messaggio
			IObservable<ScaricoFotoMsg> observable = LumenApplication.Instance.bus.Observe<ScaricoFotoMsg>();
			observable.Subscribe( this );

			CaricareItems();

			CaricareMaschera( nomeFileMsk );

			CreaFileSystemWatcher();

			// inizio ad ascoltare file in arrivo
			if( cambiareStatoCommand.CanExecute( "start" ) )
				cambiareStatoCommand.Execute( "start" );
			else
				dialogProvider.ShowError( "Completare la configurazione\r\nquindi premere il pulsante RUN\r\n" + validaConfigurazione(), "Configurazione non valida", null );
		}


		/// <summary>
		/// Creo un ascoltatore che mi notifica quando viene creato un file in una determinata cartella.
		/// </summary>
		[PermissionSet( SecurityAction.Demand, Name = "FullTrust" )]
		void CreaFileSystemWatcher() {

			if( fileSystemWatcher != null ) {
				fileSystemWatcher.Dispose();
				fileSystemWatcher = null;
			}

			fileSystemWatcher = new FileSystemWatcher( cartellaOnRide );
			fileSystemWatcher.Filter = ""; // Ascolto tutti i tipi di file perché non posso specificare più di una estensione. Li filtro io dopo
			fileSystemWatcher.Created += new FileSystemEventHandler( OnNuovaFotoArrivata );

			_giornale.Info( "Inizio monitoraggio cartella : " + cartellaOnRide );
		}

		public void CaricareMaschera( string nomeFileMsk ) {

			List<Maschera> maschere = fotoRitoccoSrv.caricaListaMaschere( FiltroMask.MskSingole );
			Maschera mskTemp = maschere.SingleOrDefault( m => m.nomeFile == nomeFileMsk );

			if( mskTemp != null ) {

				try {
					// idrato sia l'immaginetta del provino che quella grande, perché mi servono le dimensioni.
					gestoreImmagineSrv.idrataMaschera( mskTemp, true );

					// ok la maschera esiste ed è idratata. La assegno
					this.maschera = mskTemp;

					isMascheraAttiva = (this.maschera != null);

				} catch( Exception ee ) {
					_giornale.Warn( "Maschera " + nomeFileMsk + "  non caricata", ee );
				}
			}

		}

		/// <summary>
		/// Ricarico la lista degli items da disco, analizzando tutti i file contenuti nella cartella
		/// </summary>
		private void CaricareItems() {
/*
			if( fotoItems != null )
				fotoItems.CollectionChanged -= this.collezioneCambiata;
*/
			fotoItems = new ObservableCollectionEx<FotoItem>();

//			fotoItems.CollectionChanged += this.collezioneCambiata;


			try {

				// Prendo le estensioni ammesse dalla configurazione
				string[] estensioni = Configurazione.UserConfigLumen.estensioniGrafiche.Split( ';' );
				foreach( string estensione in estensioni ) {
					DirectoryInfo dirInfo = new DirectoryInfo( cartellaOnRide );
					foreach( FileInfo fileInfo in dirInfo.GetFiles( "*" + estensione ) ) {

						// Istanzio elemento della lista
						FotoItem fotoItem = new FotoItem( fileInfo );

						// carico eventuale testo già associato
						if( File.Exists( fotoItem.nomeFileTag ) ) {
							fotoItem.tag = File.ReadAllText( fotoItem.nomeFileTag );
						}

						if( userConfigOnRide.runningMode == RunningMode.Presidiato ) {
							fotoItems.Add( fotoItem );

						} else if( userConfigOnRide.runningMode == RunningMode.Automatico ) {
							fotoItem.daTaggare = false;
							acquisireUnaFoto( fotoItem );
						}

					}
				}

				// Ora che ho riempito la collezione, creo la sua VIEW
				// fotoItemsCW = new ListCollectionView( fotoItems );
				fotoItemsCW = (ListCollectionView)CollectionViewSource.GetDefaultView( fotoItems );
				OnPropertyChanged( "fotoItemsCW" );

			} catch( Exception ee ) {
				_giornale.Error( "lista files in " + cartellaOnRide, ee );
			}

		}

		void AcquisireTutteLeFoto() {

			// --- Prima chiedo conferma sulle eliminazioni

			int quanteDaEliminare = fotoItems.Count( f => f.daEliminare );
			int quanteSenzaTag = fotoItems.Count( f => f.daTaggare == false );

			bool procediPure = true;

			if( userConfigOnRide.runningMode == RunningMode.Presidiato ) {

				string msgConferma = "Sei sicuro di volere\r\n";
				if( quanteDaEliminare > 0 )
					msgConferma += "distruggere " + quanteDaEliminare + " foto\r\n";
				if( quanteSenzaTag > 0 )
					msgConferma += "acquisire " + quanteSenzaTag + " foto senza il tag\r\n";
				msgConferma += "?";

				if( quanteDaEliminare > 0 || quanteSenzaTag > 0 )
					dialogProvider.ShowConfirmation( msgConferma, "Attenzione",
						  ( confermato ) => {
							  procediPure = confermato;
						  } );
			}

			if( !procediPure )
				return;

			// ---

			bool ricomincia;

			do {
				ricomincia = false;

				for( int ii = 0; ii < fotoItemsCW.Count; ii++ ) {

					FotoItem fotoItem = fotoItems[ii];
					// Se richiesta la cancellazione da disco, la elimino 
					if( fotoItem.daEliminare ) {
						if( EliminareFoto( fotoItem ) ) {
							ricomincia = true;
							break;
						}
					} else {
						if( fotoItem.daTaggare == false || String.IsNullOrWhiteSpace( fotoItem.tag ) == false ) {
							acquisireUnaFoto( fotoItem );
							ricomincia = true;
							break;
						}
					}
				}

			} while( ricomincia );

		}

		private bool EliminareFoto( FotoItem fotoItem ) {

			bool eliminata = false;

			try {
				if( File.Exists( fotoItem.nomeFileTag ) )
					File.Delete( fotoItem.nomeFileTag );
			} catch( Exception ee ) {
				_giornale.Error( "Rimozione file tag: " + fotoItem.nomeFileTag, ee );
			}

			try {

				if( File.Exists( fotoItem.fileInfo.FullName ) )
					File.Delete( fotoItem.fileInfo.FullName );

				// Ora che ho rimosso i file dal disco, elimino anche l'elemento dalla collection
				fotoItemsCW.Remove( fotoItem );

				eliminata = true;

			} catch( Exception ee ) {
				_giornale.Error( "Rimozione file foto: " + fotoItem.fileInfo.Name, ee );
			}

			return eliminata;
		}

		/**
		 * Ciclo su tutti i file in attesa, e li carico nel db
		 */
		void acquisireUnaFoto( FotoItem fotoItem ) {

			ParamScarica paramScarica = new ParamScarica();

			// paramScarica.cartellaSorgente = cartellaOnRide;
			paramScarica.nomeFileSingolo = fotoItem.fileInfo.FullName;

			// Fotografo a cui attribuire le foto
			paramScarica.flashCardConfig.idFotografo = fotografoOnRide.id;
			paramScarica.flashCardConfig.didascalia = fotoItem.tag;

			paramScarica.eliminaFilesSorgenti = true;

			// Eventuale maschera automatica
			if(isMascheraAttiva )
				paramScarica.mascheraAuto = maschera;

			// Fase del giorno
			if( String.IsNullOrWhiteSpace(userConfigOnRide.orarioSeparaMatPom ) == false ) {
				
				FaseDelGiorno? faseDelGiorno = null;

				DateTime creation = File.GetCreationTime( fotoItem.fileInfo.FullName );
					
				string strCreation = creation.ToString( "HH:mm" );
				
				if( strCreation.CompareTo( userConfigOnRide.orarioSeparaMatPom ) < 0 )
					faseDelGiorno = FaseDelGiorno.Mattino;
				if( strCreation.CompareTo( userConfigOnRide.orarioSeparaMatPom ) >= 0 )
					faseDelGiorno = FaseDelGiorno.Pomeriggio;

				paramScarica.faseDelGiorno = faseDelGiorno;
			}

			try {

				scaricatoreFotoSrv.scarica( paramScarica );

				++totFotoAcquisite;

			} catch( Exception ee ) {
				_giornale.Error( "scarica foto", ee );
			}

		}

		void cambiareStato( string nuovo ) {

			IImpronteSrv impronteSrv = LumenApplication.Instance.getServizioAvviato<IImpronteSrv>();

			if( nuovo == "stop" ) {

				fileSystemWatcher.EnableRaisingEvents = false;

				if( userConfigOnRide.scannerImpronteGestito && scannerImprontePresente )
					impronteSrv.stop();
			}

			if( nuovo == "start" ) {

				fileSystemWatcher.EnableRaisingEvents = true;

				if( userConfigOnRide.scannerImpronteGestito && scannerImprontePresente ) {
					if( impronteSrv.statoRun == Servizi.StatoRun.Stopped )
						impronteSrv.start();
					impronteSrv.Listen( OnImprontaAcquisita );
				}
			}

			OnPropertyChanged( "isRunning" );
		}

		/// <summary>
		/// Se non ci sono errori ritorno null
		/// Altrimenti ritorno stringa con errore.
		/// </summary>
		/// <param name="avvisa"></param>
		/// <returns></returns>
		private bool possoCambiareStato( string newStato, bool avvisa = false ) {

			if( newStato != "start" )
				return true;

			string msgError = null;
			using( new UnitOfWorkScope() ) {
				msgError = validaConfigurazione();
			}

			if( avvisa && msgError != null )
				dialogProvider.ShowError( cartellaOnRide, msgError, null );

			return (msgError == null);
		}

		private string validaConfigurazione() {

			String msgError = null;

			if( Directory.Exists( cartellaOnRide ) == false )
				msgError = "Cartella Onrdide inesistente";

			if( String.IsNullOrWhiteSpace( userConfigOnRide.idFotografo ) )
				msgError = "Fotografo OnRide non impostato";
			else {
				Fotografo f = fotografiReporitorySrv.getById( userConfigOnRide.idFotografo );
				if( f == null )
					msgError = "Fotografo OnRide non valido";
			}

			return msgError;
		}

		void OnImprontaAcquisita( object sender, ScansioneEvent eventArgs ) {

			++totImpronteAcquisite;
			fileNameBmpImpronta = eventArgs.bmpFileName;
			_giornale.Info( "Impronta acquisita. Valida =  " + eventArgs.isValid + eventArgs.bmpFileName );

		}

		void refreshStatoScanner() {

			scannerImpronteNome = null;
			scannerImprontePresente = false;
//			scannerImpronteAbilitato = false;

			String nomeScanner = UnitOfWorkScope.currentDbContext.InfosFisse.Single().scannerImpronte;

			string[] modelliScannerGestiti = { "ZK4500" };

			if( modelliScannerGestiti.Contains( nomeScanner ) ) {
				scannerImpronteNome = nomeScanner;
//				scannerImpronteAbilitato = true;
			}

			// Ora provo a dialogare con lo scanner
			IImpronteSrv impronteSrv = LumenApplication.Instance.getServizioAvviato<IImpronteSrv>();

			if( ! userConfigOnRide.scannerImpronteGestito ) {

				impronteSrv.stop();
				scannerImprontePresente = false;

			} else {

				try {

					if( impronteSrv.statoRun == Servizi.StatoRun.Stopped )
						impronteSrv.start();

					bool connesso = impronteSrv.testScannerConnected();
					if( connesso ) {
						scannerImprontePresente = true;
						_giornale.Info( "Scanner presente : " + impronteSrv.infoScanner );
					} else
						_giornale.Warn( "scanner impronte non presente" );
					
				} catch( Exception ee ) {
					_giornale.Error( "lettura da scanner", ee );
					scannerImprontePresente = false;
				}
			}
		}

		#endregion Metodi

		#region Messaggi

		public void OnNext( ScaricoFotoMsg msgScaricoFotoMsg ) {

			if( msgScaricoFotoMsg.fase == FaseScaricoFoto.FineScarico ) {

				
//			} else if( msgScaricoFotoMsg.fase == FaseScaricoFoto.FineLavora ) {

				// Ho finito. Se andato a buon fine, devo fare delle cose:

				// se è andato bene, il file jpg me lo ha cancellato il servizio di acquisizione. Controllo per sicurezza
				if( msgScaricoFotoMsg.esito == Eventi.Esito.Ok ) {

					if( msgScaricoFotoMsg.esitoScarico.totFotoScaricate < 1 ) {
						_giornale.Warn( "scaricate < 1 . Come mai ?" );
					} else {

						// Ricavo il Item della lista
						string nomeFileFoto = msgScaricoFotoMsg.sorgente;
						var item = fotoItems.FirstOrDefault( i => i.fileInfo.FullName == nomeFileFoto );

						string nomeFileTag = item == null ? nomeFileFoto + ".tag.txt" : item.nomeFileTag;

						// Elimino l'oggetto dalla collezione
						if( item == null )
							_giornale.Warn( "Item non trovato in lista. Come mai ?" );
						else
							fotoItemsCW.Remove( item );

						// OK il file è stato scaricato correttamente. Se esiste, lo rimuovo
						// il file dovrebbe essere sparito
						if( File.Exists( nomeFileFoto ) ) {
							_giornale.Warn( "il file non è rimasto nella cartella di input. Come mai ?" );
							File.Delete( nomeFileFoto );
						}

						if( File.Exists( nomeFileTag ) )
							File.Delete( nomeFileTag );
					}
				}
			}
		}

		private void scegliereCartella() {
			// TODO
		}

		public void OnError( Exception error ) {
		}

		public void OnCompleted() {
		}

		private void OnNuovaFotoArrivata( object sender, FileSystemEventArgs e ) {
			// Controllo se il file ha una estensione tra quelle supportate

			string[] estensioniAmmesse = Configurazione.UserConfigLumen.estensioniGrafiche.Split( ';' );

			FileInfo finfo = new FileInfo( e.FullPath );
			if( !estensioniAmmesse.Contains( finfo.Extension.ToLower() ) )
				return;


			// Se non attendo un briciolo, il file è sempre loccato perché deve essere ancora copiato
			// Thread.Sleep( 100 );

			const int maxAttesa	= 10000;	// Massimo numero di secondi che sono disposto ad attendere. Poi lo dichiaro non usabile.
			const int singolaPausa = 1000;	// Ampiezza della singola pausa (sleep)
			int aspetta = 0;

			do {
				bool isLocked = FileUtil.IsFileLocked( finfo );
				if( isLocked ) {
					aspetta += singolaPausa;
					Thread.Sleep( singolaPausa );
				} else
					aspetta = maxAttesa*2;		// ok file buono e arrivato: provoco l'uscita dal loop

			} while( aspetta < maxAttesa );


			// Se il file è ancora loccato, non posso aggiungerlo, devo ignorarlo. Occorrerà un refresh
			bool possoAggiungere = true;

			if( aspetta != (maxAttesa * 2) ) {
				possoAggiungere = false;        // il file è ancora loccato
				_giornale.Warn( "File ancora loccato: " + finfo.Name + ". Impossibile processarlo adesso" );
			}

			// controllo se il file è troppo piccolo, (esempio zero) non è buono.
			if( possoAggiungere ) {
				// Ok è arrivata una immagine per davvero. Creo un item da accodare
				finfo.Refresh();
				if( finfo.Length <= 50 ) {
					possoAggiungere = false;        // Dimensione del file non corretta
					_giornale.Warn( "File con dimensione errata: " + finfo.Name + ". Non lo considero" );
				}
			}

			// --
			if( possoAggiungere ) {

				FotoItem fotoItem = new FotoItem( finfo );

				if( userConfigOnRide.runningMode == RunningMode.Presidiato ) {

					try {
						fotoItems.Add( fotoItem );
					} catch( Exception ee ) {
						_giornale.Error( "Aggingo foto alla lista", ee );
					}

				} else if( userConfigOnRide.runningMode == RunningMode.Automatico ) {

					// La processo senza il tag
					fotoItem.daTaggare = false;
					acquisireUnaFoto( fotoItem );
				}

			}
		}

		protected override void OnRequestClose() {

			// Faccio la dispose di tutti i viewmodel che ho istanziato io.

			// Devo salvare le preferenze
			try {
				// UserConfigOnRide pref = new UserConfigOnRide();
				userConfigOnRide.isMascheraAttiva = this.isMascheraAttiva;
				userConfigOnRide.nomeMaschera = maschera == null ? null : maschera.nomeFile;
				Config.UserConfigSerializer.serializeToFile( userConfigOnRide );
			} catch( Exception ee ) {
				_giornale.Error( "Salvataggio preferenze", ee );
			}


			base.OnRequestClose();
		}

		#endregion Messaggi


		#region Comandi

		
		private RelayCommand _acquisireFotoCommand;
		public ICommand acquisireFotoCommand {
			get {
				if( _acquisireFotoCommand == null ) {

					_acquisireFotoCommand = new RelayCommand( param => AcquisireTutteLeFoto(),
					                                          param => possoAcquisireFoto );

				}
				return _acquisireFotoCommand;
			}
		}

		private RelayCommand _caricareItemsCommand;
		public ICommand caricareItemsCommand {
			get {
				if( _caricareItemsCommand == null ) {

					_caricareItemsCommand = new RelayCommand( param => CaricareItems(),
															  param => true );

				}
				return _caricareItemsCommand;
			}
		}

		private RelayCommand _scegliereCartellaCommand;
		public ICommand scegliereCartellaCommand {
			get {
				if( _scegliereCartellaCommand == null ) {
					_scegliereCartellaCommand = new RelayCommand( quale => scegliereCartella() );
				}
				return _scegliereCartellaCommand;
			}
		}

		private RelayCommand _cambiareStatoCommand;
		public ICommand cambiareStatoCommand {
			get {
				if( _cambiareStatoCommand == null ) {
					_cambiareStatoCommand = new RelayCommand( 
						newStato => cambiareStato( (string)newStato ),
						newStato => possoCambiareStato( (string)newStato ) );
				}
				return _cambiareStatoCommand;
			}
		}

		private RelayCommand _refreshScannerCommand;
		public ICommand refreshScannerCommand {
			get {
				if( _refreshScannerCommand == null ) {
					_refreshScannerCommand = new RelayCommand(
						newStato => refreshStatoScanner(),
						newStato => true,
						false );
				}
				return _refreshScannerCommand;
			}
		}

		
		#endregion Comandi
	}
}
