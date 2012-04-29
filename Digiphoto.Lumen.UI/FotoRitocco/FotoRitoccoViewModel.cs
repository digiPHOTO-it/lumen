using System.Linq;
using Digiphoto.Lumen.UI.Mvvm;
using System.ComponentModel;
using System.Windows.Data;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ritoccare;
using System.Windows.Input;
using System.Collections.Generic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Util;
using System;
using Digiphoto.Lumen.Core;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.Windows.Media;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.UI.Adorners;
using System.Windows;
using System.Windows.Documents;
using System.IO;
using Digiphoto.Lumen.Config;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace Digiphoto.Lumen.UI.FotoRitocco {


	public class FotoRitoccoViewModel : ViewModelBase, IObserver<FotoDaModificareMsg> {

		public delegate void EditorModeChangedEventHandler( object sender, EditorModeEventArgs args );
		public event EditorModeChangedEventHandler editorModeChangedEvent;

		
		public FotoRitoccoViewModel() {

			if( IsInDesignMode ) {
				// caricare qualche foto a casaccio
			} else {

				// Mi sottoscrivo per ascoltare i messaggi di richiesta di modifica delle foto.
				IObservable<FotoDaModificareMsg> observable = LumenApplication.Instance.bus.Observe<FotoDaModificareMsg>();
				observable.Subscribe( this );


				fotografieDaModificare = new List<Fotografia>();
				fotografieDaModificareCW = new MultiSelectCollectionView<Fotografia>( fotografieDaModificare );

				modalitaEdit = ModalitaEdit.DefaultFotoRitocco;
			}

			resetEffetti();
		}

		#region Fields

		private CroppingAdorner _croppingAdorner;
		FrameworkElement _felCur = null;
		Brush _brOriginal;

		#endregion


		#region Proprietà

		private Transform _trasformazioneCorrente;
		public Transform trasformazioneCorrente {
			get {
				return _trasformazioneCorrente;
			}
			set {
				if( _trasformazioneCorrente != value ) {
					_trasformazioneCorrente = value;
					OnPropertyChanged( "trasformazioneCorrente" );

					forseInizioModifiche();
				}	
			}
		}

		public MultiSelectCollectionView<Fotografia> fotografieDaModificareCW {
			get;
			set;
		}

		public IFotoRitoccoSrv fotoRitoccoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}

		public bool isAlmenoUnaFotoSelezionata {
			get {
				return contaSelez > 0;
			}
		}

		int contaSelez {
			get {
				return fotografieDaModificareCW != null ? fotografieDaModificareCW.SelectedItems.Count : 0;
			}
		}

		/// <summary>
		/// Le foto selezionate sono in fase di modifica
		/// </summary>
		private bool _modificheInCorso;
		public bool modificheInCorso {
			get {
				return _modificheInCorso;
			}
			private set {
				if( _modificheInCorso != value ) {
					_modificheInCorso = value;
					OnPropertyChanged( "modificheInCorso" );
				}
			}
		}

		public bool possoSalvareCorrezioni {
			get {
				return modificheInCorso == true;
			}
		}

		public bool possoRifiutareCorrezioni {
			get {
				return possoSalvareCorrezioni;
			}
		}

		public bool possoApplicareCorrezione {
			get {
				return isAlmenoUnaFotoSelezionata;
			}
		}

		public FaseDelGiorno [] fasiDelGiorno {
			get {
				return FaseDelGiornoUtil.fasiDelGiorno;
			}
		}

		public FaseDelGiorno? faseDelGiorno {
			get;
			set;
		}

		public bool possoSalvareMetadati {
			get {
				return true;   // TODO (solo se ho qualcosa da fare)
			}
		}

		public List<ShaderEffectBase> effetti {
			get;
			set;
		}

		private ShaderEffect _effettoCorrente;
		public ShaderEffect effettoCorrente {
			get {
				return _effettoCorrente;
			}
			set {
				if( _effettoCorrente != value ) {
					_effettoCorrente = value;
					OnPropertyChanged( "effettoCorrente" );

					forseInizioModifiche();
				}
			}
		}


		/// <summary>
		///  Cerco se esiste lo specifico effetto nella lista di tutti gli effetti
		/// </summary>
		public LuminositaContrastoEffect luminositaContrastoEffect {

			get {
				LuminositaContrastoEffect ret = null;

				if( effetti != null ) {

					foreach( ShaderEffectBase effetto in effetti ) {
						if( effetto is LuminositaContrastoEffect ) {
							ret = effetto as LuminositaContrastoEffect;
							break;
						}
					}
				}

				return ret;
			}
		}

		public bool isSepiaChecked {
			get {
				return  effetti != null && effetti.Exists( e => e is SepiaEffect );
			}
		}

		public bool isGrayscaleChecked {
			get {
				return effetti != null && effetti.Exists( e => e is GrayscaleEffect );
			}
		}

		public bool possoAttivareSelector {
			get {
				 return (selectorAttivo == false && contaSelez == 1);
			}
		}


		public bool selectorAttivo {
			get {
				return _croppingAdorner != null;
			}
		}

		/*
			// Faccio una prova scema: TODO togliere.
			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
			fotografieDaModificare = dbContext.Fotografie.Top( Convert.ToString( 10 ) ).ToList();
			foreach( Fotografia foto in fotografieDaModificare )
				Digiphoto.Lumen.Util.AiutanteFoto.idrataImmaginiFoto( foto );
 */
		

		private List<Fotografia> _fotografieDaModificare;
		public List<Fotografia> fotografieDaModificare {
			get {
				return _fotografieDaModificare;
			}
			set {
				_fotografieDaModificare = value;
			}
		}

		public ListCollectionView maschereCW {
			private set;
			get;
		}

		public bool isGestioneMaschereDisattiva {
			get {
				return !isGestioneMaschereAttiva;
			}
		}

		public bool isGestioneMaschereAttiva {
			get {
				return modalitaEdit == ModalitaEdit.GestioneMaschere;
			}
		}

		BitmapImage _mascheraAttiva;
		public BitmapImage mascheraAttiva {
			get {
				return _mascheraAttiva;
			}
			set {
				if( _mascheraAttiva != value ) {
					_mascheraAttiva = value;
					OnPropertyChanged( "mascheraAttiva" );
				}
			}
		}

		ModalitaEdit _modalitaEdit;
		public ModalitaEdit modalitaEdit {
			get {
				return _modalitaEdit;
			}
			set {
				if( _modalitaEdit != value ) {
					_modalitaEdit = value;
					OnPropertyChanged( "modalitaEdit" );
					OnPropertyChanged( "isGestioneMaschereAttiva" );
					OnPropertyChanged( "isGestioneMaschereDisattiva" );
					onEditorModeChanged( new EditorModeEventArgs( modalitaEdit ) );
				}
			}
		}
		
		public bool listBoxImmaginiDaModificareEnabled {
			get {
				return modificheInCorso || modalitaEdit == ModalitaEdit.GestioneMaschere;
			}
		}

		public bool possoModificareConEditorEsterno {
			get {
				return isAlmenoUnaFotoSelezionata &&
					   modalitaEdit == ModalitaEdit.DefaultFotoRitocco &&
					   (!modificheInCorso);
			}
		}

		private bool _isTuttoBloccato;
		public bool isTuttoBloccato {
			get {
				return _isTuttoBloccato;
			}
			set {
				if( _isTuttoBloccato != value ) {
					_isTuttoBloccato = value;
					OnPropertyChanged( "isTuttoBloccato" );
				}
			}
		}

		#endregion Proprietà


		#region Comandi

		private RelayCommand _grayScaleCommand;
		public ICommand grayScaleCommand {
			get {
				if( _grayScaleCommand == null ) {
					_grayScaleCommand = new RelayCommand( param => this.grayScale( (bool)param ),
														param => possoApplicareCorrezione,
														true );
				}
				return _grayScaleCommand;
			}
		}

		private RelayCommand _ruotareCommand;
		public ICommand ruotareCommand {
			get {
				if( _ruotareCommand == null ) {
					_ruotareCommand = new RelayCommand( sGradi => this.ruotare( Convert.ToInt16(sGradi) ),
														sGradi => this.possoApplicareCorrezione,
														true );
				}
				return _ruotareCommand;
			}
		}

		private RelayCommand _tornareOriginaleCommand;
		public ICommand tornareOriginaleCommand {
			get {
				if( _tornareOriginaleCommand == null ) {
					_tornareOriginaleCommand = new RelayCommand( param => this.tornareOriginale(),
														gradi => this.possoApplicareCorrezione,
														true );
				}
				return _tornareOriginaleCommand;
			}
		}

		private RelayCommand _sepiaCommand;
		public ICommand sepiaCommand {
			get {
				if( _sepiaCommand == null ) {
					_sepiaCommand = new RelayCommand( param => this.sepia( (bool)param ),
													  param => this.possoApplicareCorrezione );
				}
				return _sepiaCommand;
			}
		}

		private RelayCommand _flipCommand;
		public ICommand flipCommand {
			get {
				if( _flipCommand == null ) {
					_flipCommand = new RelayCommand( p => this.flip(),
													 p => this.possoApplicareCorrezione );
				}
				return _flipCommand;
			}
		}

		private RelayCommand _salvareCorrezioniCommand;
		public ICommand salvareCorrezioniCommand {
			get {
				if( _salvareCorrezioniCommand == null ) {
					_salvareCorrezioniCommand = new RelayCommand( param => salvareCorrezioni(),
													  param => this.possoSalvareCorrezioni,
													  true );
				}
				return _salvareCorrezioniCommand;
			}
		}

		private RelayCommand _rifiutareCorrezioniCommand;
		public ICommand rifiutareCorrezioniCommand {
			get {
				if( _rifiutareCorrezioniCommand == null ) {
					_rifiutareCorrezioniCommand = new RelayCommand( param => rifiutareCorrezioni(),
													  param => this.possoRifiutareCorrezioni,
													  true );
				}
				return _rifiutareCorrezioniCommand;
			}
		}

		private RelayCommand _salvareMetadatiCommand;
		public ICommand salvareMetadatiCommand {
			get {
				if( _salvareMetadatiCommand == null ) {
					_salvareMetadatiCommand = new RelayCommand( param => salvareMetadati(),
													  param => this.possoSalvareMetadati,
													  true );
				}
				return _salvareMetadatiCommand;
			}
		}

		private RelayCommand _attivareSelectorCommand;
		public ICommand attivareSelectorCommand {
			get {
				if( _attivareSelectorCommand == null ) {
					_attivareSelectorCommand = new RelayCommand( param => this.attivareSelector( (FrameworkElement)param ),
					                                             param => possoAttivareSelector);
				}
				return _attivareSelectorCommand;
			}
		}

		private RelayCommand _cropoareCommand;
		public ICommand croppareCommand {
			get {
				if( _cropoareCommand == null ) {
					_cropoareCommand = new RelayCommand( p => this.croppare(), p => possoCroppare );
				}
				return _cropoareCommand;
			}
		}

		private RelayCommand _caricareMaschereCommand;
		public ICommand caricareMaschereCommand {
			get {
				if( _caricareMaschereCommand == null ) {
					_caricareMaschereCommand = new RelayCommand( param => caricareMaschere( (string)param ) );
				}
				return _caricareMaschereCommand;
			}
		}

		private RelayCommand _attivareMascheraCommand;
		public ICommand attivareMascheraCommand {
			get {
				if( _attivareMascheraCommand == null ) {
					_attivareMascheraCommand = new RelayCommand( param => attivareMaschera( param ) );
				}
				return _attivareMascheraCommand;
			}
		}

		private RelayCommand _modificareConEditorEsternoCommand;
		public ICommand modificareConEditorEsternoCommand {
			get {
				if( _modificareConEditorEsternoCommand == null ) {
					_modificareConEditorEsternoCommand = new RelayCommand( p => modificareConEditorEsterno(),
																		   p => possoModificareConEditorEsterno );
				}
				return _modificareConEditorEsternoCommand;
			}
		}
	
		#endregion Comandi

		// ******************************************************************************************************
		// ******************************************************************************************************

		#region Metodi

		/// <summary>
		/// La prima volta che inizio a toccare una foto,
		/// devo salvarmi le correzioni attuali di tutte quelle che stanno per essere modificate.
		/// Mi serve per gestire eventuale rollback
		/// </summary>
		private void forseInizioModifiche() {
			if( !modificheInCorso ) {
				// devo fare qualcosa al primo cambio di stato ?
			}
			modificheInCorso = true;
		}

		/// <summary>
		/// Aggiungo la correzione a tutte le foto selezionate
		/// </summary>
		private void addCorrezione( Correzione correzione ) {
			
			forseInizioModifiche();

			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.addCorrezione( f, correzione );
		}

		private void ruotare( int pGradi ) {
			addCorrezione( new Ruota() { gradi = pGradi } );
		}

		private void grayScale( bool addRemove ) {
			if( addRemove )
				addCorrezione( new BiancoNero() );
			else
				removeCorrezione( typeof( BiancoNero ) );
		}

		private void tornareOriginale() {

			// elimino tutti gli effetti creati
			resetEffetti();
			
			// per ogni foto elimino le correzioni e ricreo il provino partendo dall'originale.
			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.tornaOriginale( f );			
		}

		private void sepia( bool addRemove ) {
			if( addRemove )
				addCorrezione( new Sepia() );
			else
				removeCorrezione( typeof( Sepia ) );
		}

		private void removeCorrezione( Type type ) {
						
			forseInizioModifiche();

			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.removeCorrezione( f, type );
		}

		private void flip() {
			addCorrezione( new Specchio() );
		}

		private void salvareCorrezioni() {

			
			// Purtoppo gli shader effects sono gestiti a parte
			// Vado ad aggiungerli solo al momento di applicare per davvero
			foreach( ShaderEffectBase effetto in effetti )
				addCorrezione( convertiInCorrezione( effetto ) );

			// Purtoppo anche la trasformazione di rotazione, è gestita a parte.
			if( trasformazioneCorrente is RotateTransform ) {
				Ruota rc = new Ruota();
				rc.gradi = (float) ((RotateTransform)trasformazioneCorrente).Angle;
				addCorrezione( rc );
			}

			// Ormai che li ho acquisiti, li svuoto
			resetEffetti();

			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.salvaCorrezioniTransienti( f );

			// Ora che ho persistito, concludo "dicamo cosi" la transazione, faccio una specie di commit.
			modificheInCorso = false;
		}

		private void rifiutareCorrezioni() {
			resetEffetti();
			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.undoCorrezioniTransienti( f );
			modificheInCorso = false;
		}

		private void salvareMetadati() {
			// TODO
		}

		void resetEffetti() {

			effettoCorrente = null;
			trasformazioneCorrente = null;
			attivareSelector( null );  // Spegno eventuale selettore

			if( effetti == null ) {
				// Creo gli effetti vuoti
				effetti = new List<ShaderEffectBase>();
			} else {
				// Questo serve anche per rimettere gli slider nella posiziona di default
				foreach( ShaderEffectBase effetto in effetti ) {
					effetto.reset();
					BindingOperations.ClearAllBindings( effetto );
				}
				effetti.Clear();

				// Questo rimette a posto i bottoni toggle BN e SEPIA
				OnPropertyChanged( "isSepiaChecked" );
				OnPropertyChanged( "isGrayscaleChecked" );
			}


			// anche le maschere
			mascheraAttiva = null;

			// Spengo maschere
			modalitaEdit = ModalitaEdit.DefaultFotoRitocco;
		}

		private Correzione convertiInCorrezione( ShaderEffectBase effetto ) {

			Correzione ret = null;

			if( effetto is LuminositaContrastoEffect ) {
				ret = new Luce {
					luminosita = ((LuminositaContrastoEffect)effetto).Brightness,
					contrasto = ((LuminositaContrastoEffect)effetto).Contrast
				};

			}

			return ret;
		}

		public bool forseCambioTrasformazioneCorrente( Type type ) {

			bool creatoNuovo = false;

			// Controllo se la trasformazione corrente è già quello attuale non faccio niente.
			if( trasformazioneCorrente == null || trasformazioneCorrente.GetType() != type ) {
				trasformazioneCorrente = (Transform) Activator.CreateInstance( type );
				creatoNuovo = true;
			}

			return creatoNuovo;
		}

		/// <summary>
		///  Imposto un eventuale nuovo effetto.
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true se l'ho creato davvero</returns>
		public bool forseCambioEffettoCorrente( Type type ) {

			bool creatoNuovo = false;

			// Controllo se l'effetto corrente è già quello attuale non faccio niente.
			if( effettoCorrente != null && effettoCorrente.GetType() == type )
				return false;

			// Se l'effetto indicato è già in lista non faccio niente, altrimenti lo creo
			bool trovato = false;
			foreach( ShaderEffectBase effetto in effetti ) {
				if( effetto.GetType() == type ) {
					trovato = true;
					effettoCorrente = effetto;
					break;
				}
			}

			if( !trovato ) {
				ShaderEffectBase nuovo = (ShaderEffectBase)Activator.CreateInstance( type );
				effetti.Add( nuovo );
				effettoCorrente = nuovo;
				creatoNuovo = true;
			}

			return creatoNuovo;
		}




		/// <summary>
		/// Attivo il selettore sulla immagine corrente
		/// </summary>
		private void attivareSelector( FrameworkElement imageToCrop ) {

			if( imageToCrop != null ) {
				AddCropToElement( imageToCrop );
				_brOriginal = _croppingAdorner.Fill;
				RefreshCropImage();
			} else {
				RemoveCropFromCur();
			}
		}

		private void AddCropToElement( FrameworkElement fel ) {
			if( _felCur != null ) {
				RemoveCropFromCur();
			}
			Rect rcInterior = new Rect(
				fel.ActualWidth * 0.2,
				fel.ActualHeight * 0.2,
				fel.ActualWidth * 0.6,
				fel.ActualHeight * 0.6 );
			AdornerLayer aly = AdornerLayer.GetAdornerLayer( fel );
			_croppingAdorner = new CroppingAdorner( fel, rcInterior );
			aly.Add( _croppingAdorner );
			// Questa è una anteprima che non mi serve
			// imgCrop.Source = _croppingAdorner.BpsCrop();
			_croppingAdorner.CropChanged += CropChanged;
			_felCur = fel;

			SetClipColorGrey();

		}


		private void SetClipColorGrey() {
			if( _croppingAdorner != null ) {
				Color clr = Colors.Black;
				clr.A = 140;
				_croppingAdorner.Fill = new SolidColorBrush( clr );
			}
		}

		private void RefreshCropImage() {
			if( _croppingAdorner != null ) {
				Rect rc = _croppingAdorner.ClippingRectangle;

				string testo = string.Format(
					"Clipping Rectangle: ({0:N1}, {1:N1}, {2:N1}, {3:N1})",
					rc.Left,
					rc.Top,
					rc.Right,
					rc.Bottom );

				// Questa è una anteprima che non mi serve
				// imgCrop.Source = _clp.BpsCrop();
			}
		}

		private void CropChanged( Object sender, RoutedEventArgs rea ) {
			RefreshCropImage();
		}

		private void RemoveCropFromCur() {

			// Ho remmato le istruzioni di sicurezza.
			// bisogna evitare che si arrivi qu


			if( _felCur != null ) {
				AdornerLayer aly = AdornerLayer.GetAdornerLayer( _felCur );
				if( aly != null )
					aly.Remove( _croppingAdorner );
			}

			// Spengo tutto
			if( _croppingAdorner != null )
				_croppingAdorner.CropChanged -= CropChanged;
			_croppingAdorner = null;
			_felCur = null;
			_brOriginal = null;
		}


		void croppare() {

			Crop cropCorrezione = new Crop();
			// Questo è il rettangolo da tagliare
			cropCorrezione.x = (int) _croppingAdorner.ClippingRectangle.X;
			cropCorrezione.y = (int) _croppingAdorner.ClippingRectangle.Y;
			cropCorrezione.w = (int) _croppingAdorner.ClippingRectangle.Width;
			cropCorrezione.h = (int) _croppingAdorner.ClippingRectangle.Height;

			// Queste sono le dimensioni dell'immagine di riferimento per la geometria di cui sopra.
			cropCorrezione.imgWidth = (int) _croppingAdorner.ActualWidth;
			cropCorrezione.imgHeight = (int) _croppingAdorner.ActualHeight;

			addCorrezione( cropCorrezione );

			attivareSelector( null );  // Spengo il selector tanto ormai ho tagliato
		}

		bool possoCroppare {
			get {
				return selectorAttivo;
			}
		}

		void addFotoDaModificare( Fotografia f ) {

			if( this.fotografieDaModificare.Contains( f ) == false ) {
				AiutanteFoto.idrataImmaginiFoto( IdrataTarget.Provino, f );
				//			fotografieDaModificareCW.AddNewItem( f );
				this.fotografieDaModificare.Add( f );
			}
		}

		List<BitmapImage> loadMaschereDaDisco() {

			List<BitmapImage> maschere = new List<BitmapImage>();

			// Faccio giri diversi per i vari formati grafici che sono indicati nella configurazione (jpg, tif)
			foreach( string estensione in Configurazione.estensioniGraficheAmmesse ) {

				string [] files = Directory.GetFiles( Configurazione.cartellaMaschere, searchPattern: estensione, searchOption: SearchOption.AllDirectories );

				// trasferisco tutti i files elencati
				foreach( string nomeFileSrc in files ) {
					BitmapImage msk = new BitmapImage( new Uri( nomeFileSrc ) );
					maschere.Add( msk );
				}
			}

			return maschere;
		}

		/*
		List<FileInfo> caricaMaschere() {

			List<FileInfo> filesInfoMaschere = new List<FileInfo>();

			// Faccio giri diversi per i vari formati grafici che sono indicati nella configurazione (jpg, tif)
			foreach( string estensione in Configurazione.estensioniGraficheAmmesse ) {

				string [] files = Directory.GetFiles( Configurazione.cartellaMaschere, searchPattern: estensione, searchOption: SearchOption.AllDirectories );

				// trasferisco tutti i files elencati
				foreach( string nomeFileSrc in files )
					filesInfoMaschere.Add( new FileInfo(nomeFileSrc) );
			}

			return filesInfoMaschere;
		}
		*/


		/// <summary>
		///  verso:   H = solo orizzontali
		///           V = solo verticali
		///           T = tutte
		///           N = nessuna (svuota)
		/// </summary>
		/// <param name="verso"></param>
		private void caricareMaschere( string verso ) {

			if( verso == "T" ) {
				maschereCW = new ListCollectionView( loadMaschereDaDisco() );
			} else if( verso == "N" ) {
				maschereCW = null;
			} else {

				maschereCW.Filter = obj => {

					BitmapImage bmp = obj as BitmapImage;
					if( verso == "V" )
						return bmp.Width <= bmp.Height;
					else if( verso == "H" )
						return bmp.Width >= bmp.Height;
					else
						return true;  // Qui è impossibile. Per sicurezza includo tutto
				};
			}
			OnPropertyChanged( "maschereCW" );
		}

		void attivareMaschera( object p ) {
				
			BitmapImage msk = (BitmapImage)p;
			mascheraAttiva = msk;


			// cambio stato. Vado in modalità di editing maschere
			modalitaEdit = ModalitaEdit.GestioneMaschere;

			// Mi serve per accendere i pulsanti di rifiuta e salva
			forseInizioModifiche();
		}


		void modificareConEditorEsterno() {

			try {
				isTuttoBloccato = true;

				// Accodo le stampe da modificare
				fotoRitoccoSrv.modificaConProgrammaEsterno( fotografieDaModificareCW.SelectedItems.ToArray() );

			} finally {
				isTuttoBloccato = false;
			}
		}

		#endregion Metodi

		// **********************************************************************************************
		// **********************************************************************************************

		#region Gestori Eventi

		protected void onEditorModeChanged( EditorModeEventArgs e ) {
			if( editorModeChangedEvent != null )
				editorModeChangedEvent( this, e );
		}

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( FotoDaModificareMsg msg ) {

			// Ecco che sono arrivate delle nuove foto da modificare
			// Devo aggiungerle alla lista delle esistenti
			foreach( Fotografia f in msg.fotosDaModificare )
				addFotoDaModificare( f );

			fotografieDaModificareCW = new MultiSelectCollectionView<Fotografia>( fotografieDaModificare );
			OnPropertyChanged( "fotografieDaModificareCW" );
		}

		#endregion Gestori Eventi

	}
}
