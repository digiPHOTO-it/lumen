using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.OnRide.UI.Model;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace Digiphoto.Lumen.OnRide.UI {

	public class MainWindowViewModel : ViewModelBase, IObserver<ScaricoFotoMsg> {

		protected new static readonly ILog _giornale = LogManager.GetLogger( typeof( MainWindowViewModel ) );

		public MainWindowViewModel() {

			// TODO parametrizzare
			this.cartellaOnRide = @"D:\OnRideIn";

			using( new UnitOfWorkScope() ) {

				// Carico il fotografo dal db (in questo caso non è umano, ma di tipo automatico)
				fotografoOnRide = UnitOfWorkScope.currentDbContext.Fotografi.FirstOrDefault( f => f.umano == false && f.attivo == true );
				if( fotografoOnRide == null )
					throw new LumenException( "Nessun fotografo per OnRide" );
			}

			init();
		}

		private void init() {

			// Ascolto messaggio
			IObservable<ScaricoFotoMsg> observable = LumenApplication.Instance.bus.Observe<ScaricoFotoMsg>();
			observable.Subscribe( this );

			caricareItems();
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


		private void caricareItems() {

			fotoItems = new ObservableCollectionEx<FotoItem>();

			try {

				// Prendo le estensioni ammesse dalla configurazione
				string[] estensioni = Configurazione.UserConfigLumen.estensioniGrafiche.Split( ';' );
				foreach( string estensione in estensioni )	{
					DirectoryInfo dirInfo = new DirectoryInfo( cartellaOnRide );
					foreach( FileInfo fileInfo in dirInfo.GetFiles( "*" + estensione ) ) {

						// Istanzio elemento della lista
						FotoItem fotoItem = new FotoItem();
						fotoItem.fileInfo = fileInfo;
						fotoItem.daTaggare = true;

						// carico eventuale testo già associato
						if( File.Exists( fotoItem.nomeFileTag ) ) {
							fotoItem.tag = File.ReadAllText( fotoItem.nomeFileTag );
						}

						fotoItems.Add( fotoItem );
					}
				}

				fotoItemsCW = new ListCollectionView( fotoItems );
				OnPropertyChanged( "fotoItemsCW" );

			} catch( Exception ee ) {
				_giornale.Error( "lista files in " + cartellaOnRide, ee );
			}

		}


		void acquisireTutteLeFoto() {

			bool ricomincia;

			do {
				ricomincia = false;

				for( int ii = 0; ii < fotoItemsCW.Count; ii++ ) {

					FotoItem fotoItem = fotoItems[ii];
					// Se richiesta la cancellazione da disco, la elimino 
					if( fotoItem.daEliminare ) {
						if( eliminareFoto( fotoItem ) ) {
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

		private bool eliminareFoto( FotoItem fotoItem ) {

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

		public void OnError( Exception error ) {
		}

		public void OnCompleted() {
		}

		#endregion Messaggi

		bool possoAcquisireFoto {
				get {
					return fotoItemsCW.Count > 0;
			}
		}

		#region Comandi

		private RelayCommand _acquisireFotoCommand;
		public ICommand acquisireFotoCommand {
			get {
				if( _acquisireFotoCommand == null ) {

					_acquisireFotoCommand = new RelayCommand( param => acquisireTutteLeFoto(),
					                                          param => possoAcquisireFoto );

				}
				return _acquisireFotoCommand;
			}
		}

		private RelayCommand _caricareItemsCommand;
		public ICommand caricareItemsCommand {
			get {
				if( _caricareItemsCommand == null ) {

					_caricareItemsCommand = new RelayCommand( param => caricareItems(),
															  param => true );

				}
				return _caricareItemsCommand;
			}
		}

		#endregion Comandi
	}
}
