using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.OnRide.UI.Model;
using Digiphoto.Lumen.OnRideUI;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;

namespace Digiphoto.Lumen.OnRide.UI {

	public class MainWindowViewModel : ViewModelBase, IObserver<ScaricoFotoMsg> {


		protected new static readonly ILog _giornale = LogManager.GetLogger( typeof( MainWindowViewModel ) );

		public MainWindowViewModel() {

			Init();
		}
		

		#region Proprietà

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

		private ObservableCollectionEx<FotoItem> fotoItems {

			get;
			set;
		}

		bool possoAcquisireFoto {
			get {
				return fotoItemsCW.Count > 0;
			}
		}

		#endregion Proprietà

		#region Metodi

		private void Init() {

			// TODO leggere dalla configurazione
			this.cartellaOnRide = @"D:\OnRideIn";


			using( new UnitOfWorkScope() ) {

				// Carico il fotografo dal db (in questo caso non è umano, ma di tipo automatico)
				fotografoOnRide = UnitOfWorkScope.currentDbContext.Fotografi.FirstOrDefault( f => f.umano == false && f.attivo == true );
				if( fotografoOnRide == null ) {
					var msg = "Nessun fotografo attivo per modalità OnRide";
					_giornale.Error( msg );
					throw new LumenException( msg );
				}
			}

			// Ascolto messaggio
			IObservable<ScaricoFotoMsg> observable = LumenApplication.Instance.bus.Observe<ScaricoFotoMsg>();
			observable.Subscribe( this );

			CaricareItems();

			CreaFileSystemWatcher();

			// inizio ad ascoltare file in arrivo
			cambiareStatoCommand.Execute( "start" );
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


		/// <summary>
		/// Ricarico la lista degli items da disco, analizzando tutti i file contenuti nella cartella
		/// </summary>
		private void CaricareItems() {

			fotoItems = new ObservableCollectionEx<FotoItem>();

			try {

				// Prendo le estensioni ammesse dalla configurazione
				string[] estensioni = Configurazione.UserConfigLumen.estensioniGrafiche.Split( ';' );
				foreach( string estensione in estensioni )	{
					DirectoryInfo dirInfo = new DirectoryInfo( cartellaOnRide );
					foreach( FileInfo fileInfo in dirInfo.GetFiles( "*" + estensione ) ) {

						// Istanzio elemento della lista
						FotoItem fotoItem = new FotoItem( fileInfo );

						// carico eventuale testo già associato
						if( File.Exists( fotoItem.nomeFileTag ) ) {
							fotoItem.tag = File.ReadAllText( fotoItem.nomeFileTag );
						}

						fotoItems.Add( fotoItem );
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

			string msgConferma = "Sei sicuro di volere\r\n";
			if( quanteDaEliminare > 0 )
				msgConferma += "distruggere " + quanteDaEliminare + " foto\r\n";
			if( quanteSenzaTag > 0 )
				msgConferma += "acquisire " + quanteSenzaTag + " foto senza il tag\r\n";
			msgConferma += "?";

			bool procediPure = true;
			if( quanteDaEliminare > 0 || quanteSenzaTag > 0 )
				dialogProvider.ShowConfirmation( msgConferma, "Attenzione",
					  ( confermato ) => {
						  procediPure = confermato;
					  } );

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


			// Fase del giorno
			FaseDelGiorno faseDelGiorno;
			DateTime creation = File.GetCreationTime( fotoItem.fileInfo.FullName );
			if( creation.Hour > 16 )
				faseDelGiorno = FaseDelGiorno.Sera;
			else if( creation.Hour > 13 )
				faseDelGiorno = FaseDelGiorno.Pomeriggio;
			else
				faseDelGiorno = FaseDelGiorno.Mattino;
			paramScarica.faseDelGiorno = faseDelGiorno;
			
			try {

				scaricatoreFotoSrv.scarica( paramScarica );

			} catch( Exception ee ) {
				_giornale.Error( "scarica foto", ee );
			}

		}

		void cambiareStato( string nuovo ) {

			if( nuovo == "stop" )
				fileSystemWatcher.EnableRaisingEvents = false;
			if( nuovo == "start" )
				fileSystemWatcher.EnableRaisingEvents = true;
			OnPropertyChanged( "isRunning" );
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
				_giornale.Warn( "File ancora loccato: " + finfo.Name + ". Impossibile aggiungerlo alla lista" );
			}

			if( possoAggiungere ) {
				// Ok è arrivata una immagine per davvero. Creo un item da accodare
				finfo.Refresh();
				if( finfo.Length <= 50 ) {
					possoAggiungere = false;        // Dimensione del file non corretta
					_giornale.Warn( "File con dimensione errata: " + finfo.Name + ". Impossibile aggiungerlo alla lista" );
				}
			}

			if( possoAggiungere ) {
			
				try {
					FotoItem fotoItem = new FotoItem( finfo );
					fotoItems.Add( fotoItem );
				} catch( Exception ee ) {
					_giornale.Error( "Aggingo foto alla lista", ee );
				}
			}

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
					_cambiareStatoCommand = new RelayCommand( stato => cambiareStato( (string)stato ) );
				}
				return _cambiareStatoCommand;
			}
		}


		#endregion Comandi
	}
}
