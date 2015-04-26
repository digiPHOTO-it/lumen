using System.Linq;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Data;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ritoccare;
using System.Windows.Input;
using System.Collections.Generic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;
using System;
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
using Digiphoto.Lumen.UI.Main;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Scaricatore;
using System.Text;
using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic.Correzioni;
using Digiphoto.Lumen.UI.SelettoreAzioniRapide;

namespace Digiphoto.Lumen.UI.FotoRitocco {


	public class FotoRitoccoViewModel : ViewModelBase, IObserver<Messaggio>, IAzzioniRapide {

		public delegate void EditorModeChangedEventHandler( object sender, EditorModeEventArgs args );
		public event EditorModeChangedEventHandler editorModeChangedEvent;

		
		public FotoRitoccoViewModel() {

			if( IsInDesignMode ) {
				// caricare qualche foto a casaccio
			} else {

				// Mi sottoscrivo per ascoltare i messaggi di richiesta di modifica delle foto.
				IObservable<Messaggio> observable = LumenApplication.Instance.bus.Observe<Messaggio>();
				observable.Subscribe( this );

				selettoreAzioniRapideViewModel = new SelettoreAzioniRapideViewModel(this);

				fotografieDaModificare = new ObservableCollectionEx<Fotografia>();
				fotografieDaModificareCW = new ListCollectionView( fotografieDaModificare );
	//			selettoreAzioniRapideViewModel.fotografieCW = 
				fotografieDaModificareCW.Filter += fdmViewFilter;

				// Carico le maschere e mi setto in modalità fotoritocco
				this.modalitaEdit = ModalitaEdit.FotoRitocco;
				caricareMaschere( "S" );

				cfg = Configurazione.UserConfigLumen;

	
				// Resetto collezion ed effetti
				svuotareListaDaModificare();

				salvataggioAutomatico = true;  // Per ora lo decido a tavolino. Un domani potrebbe diventare un parametro della configurazione
			}

		}

		//Non ho la selezione multipla nel foto ritocco
		public MultiSelectCollectionView<Fotografia> fotografieCW
		{
			get
			{
				return null;
			}
		}
		

#region Fields

		/// <summary>
		/// Rappresenta l'ordine puntuale delle trasformazioni nella lista
		/// </summary>
		/// 
		public const int TFXPOS_FLIP = 0;
		public const int TFXPOS_ROTATE = 1;
		public const int TFXPOS_ZOOM = 2;
		public const int TFXPOS_TRANSLATE = 3;

		static SkewTransform tfxNulla = new SkewTransform();

		bool _faseRipristinoFoto = false;

#endregion Fields


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
					
					// notifico anche le singole trasformazioni tutte : TODO dovrei segnalare solo quella precedente cambiata e quella corrente
					OnPropertyChanged( "trasformazioneFlip" );
					OnPropertyChanged( "trasformazioneRotate" );
					OnPropertyChanged( "trasformazioneTranslate" );
					OnPropertyChanged( "trasformazioneZoom" );

					// Se non è nulla la trasformazione che mi stanno settando, allora dichiaro che sto iniziando le modifiche alla foto.
					if( ! isTrasformazioneNulla(_trasformazioneCorrente) )
						forseInizioModifiche();
				}	
			}
		}

		public UserConfigLumen cfg
		{
			get;
			set;
		}

		public IFotoRitoccoSrv fotoRitoccoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}

		/// <summary>
		///  Se sono in modalità di fotoritocco, allora ritono la foto in esame
		/// </summary>
		private Fotografia _fotografiaInModifica;
		public Fotografia fotografiaInModifica {

			get {
				return _fotografiaInModifica;
			}

			set {
				if( _fotografiaInModifica != value ) {

					// Operazioni prima della modifica
					onFotografiaInModificaChanged( true );

					_fotografiaInModifica = value;

					// Operazioni dopo della modifica
					onFotografiaInModificaChanged( false );


					OnPropertyChanged( "fotografiaInModifica" );
					OnPropertyChanged( "isModalitaEditFotoRitocco" );
				}
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

		public bool possoApplicareCorrezioni {
			get {
				return possoModificareLaFoto && modificheInCorso == true;
			}
		}

		public bool possoModificareLaFoto {
			get {
				return modalitaEdit == ModalitaEdit.FotoRitocco && isAlmenoUnaFotoSelezionata;
			}
		}

		public bool possoRifiutareCorrezioni {
			get {

				return possoApplicareCorrezioni;
			}
		}

		public bool possoApplicareCorrezione {
			get {
				return isAlmenoUnaFotoSelezionata;
			}
		}

		public bool possoTornareOriginale {
			get {
				return isAlmenoUnaFotoSelezionata;
			}
		}

		public bool isAlmenoUnaFotoSelezionata { 
			get {
				return fotografiaInModifica != null;
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

		public bool possoSfogliarePerFileCornice {
			get {
				return modalitaEdit == ModalitaEdit.GestioneMaschere;
			}
		}

		public List<ShaderEffectBase> effetti {
			get;
			set;
		}

		/// <summary>
		/// Le trasformazioni sono posizionali.
		/// L'ordine è dato dalle define: TFX_xxx
		/// </summary>
		public TransformGroup trasformazioni {
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

					OnPropertyChanged( "effetto1" );
					OnPropertyChanged( "effetto2" );
					OnPropertyChanged( "effetto3" );

					if( _effettoCorrente != null )
						forseInizioModifiche();
				}
			}
		}

		public ShaderEffect effetto1 {
			get {
				return effetti != null && effetti.Count > 0 ? effetti [0] : null;
			}
		}
		public ShaderEffect effetto2 {
			get {
				return effetti != null && effetti.Count > 1 ? effetti [1] : null;
			}
		}
		public ShaderEffect effetto3 {
			get {
				return effetti != null && effetti.Count > 2 ? effetti [2] : null;
			}
		}

		/// <summary>
		/// Larghezza del contenitore (può essere dato dalla cornice, oppure dal ratio della foto stessa)
		/// </summary>
		private double _frpContenitoreW;
		public double frpContenitoreW {
			get {
				return _frpContenitoreW;
			}
			set {
				if( value != _frpContenitoreW ) {
					_frpContenitoreW = value;
					OnPropertyChanged( "frpContenitoreW" );
				}
			}
		}

		/// <summary>
		/// Larghezza del contenitore (può essere dato dalla cornice, oppure dal ratio della foto stessa)
		/// </summary>
		private double _frpContenitoreH;
		public double frpContenitoreH {
			get {
				return _frpContenitoreH;
			}
			set {
				if( value != _frpContenitoreH ) {
					_frpContenitoreH = value;
					OnPropertyChanged( "frpContenitoreH" );
				}
			}
		}

		// Questa informazione mi viene dalla GUI.
		public double frpContenitoreMaxW {
			private get;
			set;
		}

		// Questa informazione mi viene dalla GUI.
		public double frpContenitoreMaxH {
			private get;
			set;
		}



		public Transform trasformazioneFlip {
			get {
				return trasformazioni != null && trasformazioni.Children.Count > TFXPOS_FLIP && trasformazioni.Children[TFXPOS_FLIP] != null ? trasformazioni.Children[TFXPOS_FLIP] : null;
			}
		}

		public Transform trasformazioneRotate {
			get {
				return trasformazioni != null && trasformazioni.Children.Count > TFXPOS_ROTATE && trasformazioni.Children[TFXPOS_ROTATE] is RotateTransform ? trasformazioni.Children[TFXPOS_ROTATE] : null;
			}
		}

		public Transform trasformazioneZoom {
			get {
				return trasformazioni != null && trasformazioni.Children.Count > TFXPOS_ZOOM && trasformazioni.Children[TFXPOS_ZOOM] is ScaleTransform ? trasformazioni.Children[TFXPOS_ZOOM] : null;
			}
		}

		public Transform trasformazioneTranslate {
			get {
				return trasformazioni != null && trasformazioni.Children.Count > TFXPOS_TRANSLATE && trasformazioni.Children[TFXPOS_TRANSLATE] is TranslateTransform ? trasformazioni.Children[TFXPOS_TRANSLATE] : null;
			}
		}

		/// <summary>
		/// Al momento gestisco un solo logo. In futuro potrei gestirne anche più di uno, 
		/// oppure delle ImgOverlay in genere.
		/// </summary>
		private Logo _logo;
		public Logo logo {
			get {
				return _logo;
			}
			set {
				if( _logo != value ) {
					_logo = value;
					OnPropertyChanged( "logo" );
				}
			}
		}

		public LuminositaContrastoEffect luminositaContrastoEffect {

			get {

				LuminositaContrastoEffect ret = null;

				if( effetti != null )
					ret = effetti.FirstOrDefault( effetto => effetto is LuminositaContrastoEffect ) as LuminositaContrastoEffect;

				return ret;
			}
		}
		public DominantiEffect dominantiEffect {

			get {
				DominantiEffect ret = null;

				if( effetti != null ) {

					foreach( ShaderEffectBase effetto in effetti ) {
						if( effetto is DominantiEffect ) {
							ret = effetto as DominantiEffect;
							break;
						}
					}
				}

				return ret;
			}
		}

		public bool isSepiaChecked {
			get {
				return  (effetti != null && effetti.Exists( e => e is SepiaEffect ));
			}
		}

		public bool isFlipChecked {
			get {
				return (trasformazioni != null && trasformazioni.Children.Count > TFXPOS_FLIP && trasformazioni.Children[TFXPOS_FLIP] is ScaleTransform );
			}
		}

		public bool isRotatePiu90Checked {
			get {
				bool esiste = false;
				if( trasformazioni != null && trasformazioni.Children.Count > TFXPOS_ROTATE && trasformazioni.Children[TFXPOS_ROTATE] is RotateTransform )
					esiste = ((RotateTransform)trasformazioni.Children[TFXPOS_ROTATE]).Angle == 90.0d;
				return esiste;
			}
		}

		public bool isRotateMeno90Checked {
			get {
				bool esiste = false;
				if( trasformazioni != null && trasformazioni.Children.Count > TFXPOS_ROTATE && trasformazioni.Children[TFXPOS_ROTATE] is RotateTransform )
					esiste = ((RotateTransform)trasformazioni.Children[TFXPOS_ROTATE]).Angle == -90.0d;
				return esiste;
			}
		}

		public bool isLuminositaChecked {
			get {
				return (effetti != null && effetti.Exists( e => e is LuminositaContrastoEffect && ((LuminositaContrastoEffect)e).Brightness != 0 ));
			}
		}

		public bool isContrastoChecked {
			get {
				return (effetti != null && effetti.Exists( e => e is LuminositaContrastoEffect && ((LuminositaContrastoEffect)e).Contrast != 1 ));
			}
		}

		public bool isGrayscaleChecked {
			get {
				return (effetti != null && effetti.Exists( e => e is GrayscaleEffect ));
			}
		}

		public ListCollectionView fotografieDaModificareCW {
			get;
			private set;
		}

		private ObservableCollection<Fotografia> _fotografieDaModificare;
		public ObservableCollection<Fotografia> fotografieDaModificare {
			get {
				return _fotografieDaModificare;
			}
			set {
				_fotografieDaModificare = value;
			}
		}

		private ObservableCollection<BitmapImage> maschereSingole {
			get;
			set;
		}
		private ObservableCollection<BitmapImage> maschereMultiple {
			get;
			set;
		}

		public ListCollectionView maschereCW {
			private set;
			get;
		}

		public bool isMascheraAttiva {
			get {
				return mascheraAttiva != null;
			}
		}

		public bool isMascheraInattiva {
			get {
				return !isMascheraAttiva;
			}
		}

		public bool isModalitaEditFotoRitocco
		{
			get
			{
				return isAlmenoUnaFotoSelezionata;
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
					OnPropertyChanged( "isMascheraAttiva" );
					OnPropertyChanged( "isMascheraInattiva" );
					OnPropertyChanged( "possoSalvareMaschera" );
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
					OnPropertyChanged( "possoSalvareMaschera" );
					OnPropertyChanged( "fotografiaInModifica" );
					
					forzaRefreshStato();
					
					onEditorModeChanged( new EditorModeEventArgs( modalitaEdit ) );
				}
			}
		}
		
		// gestione paginazione lista foto da modificare
		public int fdmPaginaCorrente {
			get;
			private set;
		}

		public const int fdmFotoPerPagina = 2*4;

		private int _fdmTotPagine;
		public int fdmTotPagine { 
			get {
				return _fdmTotPagine;
			}
			
			private set { 
				if( _fdmTotPagine != value ) {
					_fdmTotPagine = value;
					OnPropertyChanged( "fdmTotPagine" );
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
				return String.IsNullOrEmpty( Configurazione.UserConfigLumen.editorImmagini ) == false &&
					   modalitaEdit == ModalitaEdit.FotoRitocco &&
					   (!modificheInCorso) &&
					   isAlmenoUnaFotoSelezionata;
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

		public int contaFotoInAttesaDiModifica {
			get {
				return fotografieDaModificare != null ? fotografieDaModificare.Count : 0;
			}
		}

		public bool esistonoFotoInAttesaDiModifica {
			get {
				return contaFotoInAttesaDiModifica > 0;
			}
		}

		public bool possoSvuotareListaDaModificare {
			get {
				return esistonoFotoInAttesaDiModifica /* && modificheInCorso == false */;
			}
		}

		public bool possoCaricareMaschere( string param ) {

			if( param == "V" || param == "H" )
				return maschereCW != null && maschereCW.IsEmpty == false;
			else
				return true;  // T=tutto ; N=Niente
		}

		public bool possoSalvareMaschera {
			get {
				return modalitaEdit == ModalitaEdit.GestioneMaschere && mascheraAttiva != null;
			}
		}

		public SelettoreAzioniRapideViewModel selettoreAzioniRapideViewModel
		{
			get;
			set;
		}

		public float ratioAreaStampabile {
			get {
				ISpoolStampeSrv srv = LumenApplication.Instance.getServizioAvviato<ISpoolStampeSrv>();
				return srv == null ? 0f : srv.ratioAreaStampabile;
			}
		}

		/// <summary>
		/// Il controllo classico di crop, Ciccio non lo vuole. Lo tengo per lo standard.
		/// </summary>
		public bool isCropClassicoVisibile {
			get {
				return ! Configurazione.isFuoriStandardCiccio;
			}
		}

		public IGestoreImmagineSrv gestoreImmaginiSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			}
		}

		/// <summary>
		/// Quando passo da una foto ad un altra, salvo automaticamente eventuali modifiche effettuate
		/// </summary>
		public bool salvataggioAutomatico {
			get;
			set;
		}

		private bool _quadroRuotato;
		public bool quadroRuotato {
			get {
				return _quadroRuotato;
			}
			set {
				if( _quadroRuotato != value ) {
					_quadroRuotato = value;
					OnPropertyChanged( "quadroRuotato" );
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
				                                                 param => possoTornareOriginale,
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
					_flipCommand = new RelayCommand( param => this.flip( (bool)param ),
													 param => this.possoApplicareCorrezione );
				}
				return _flipCommand;
			}
		}

		private RelayCommand _zoomareCommand;
		public ICommand zoomareCommand {
			get {
				if( _zoomareCommand == null ) {
					_zoomareCommand = new RelayCommand( sFactor => this.zoomare( Convert.ToDouble(sFactor) ),
														sFactor => this.possoApplicareCorrezione,
														true );
				}
				return _zoomareCommand;
			}
		}

		private RelayCommand _traslareCommand;
		public ICommand traslareCommand {
			get {
				if( _traslareCommand == null ) {
					_traslareCommand = new RelayCommand( sFactor => this.traslare( Convert.ToDouble( sFactor ) ),
														 sFactor => Convert.ToDouble(sFactor) == 0 ? true : possoApplicareCorrezione,
					                                     true );
				}
				return _traslareCommand;
			}
		}


		private RelayCommand _applicareCorrezioniCommand;
		public ICommand applicareCorrezioniCommand {
			get {
				if( _applicareCorrezioniCommand == null ) {
					_applicareCorrezioniCommand = new RelayCommand( param => applicareCorrezioni(),
													  param => this.possoApplicareCorrezioni,
													  true );
				}
				return _applicareCorrezioniCommand;
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

		private RelayCommand _cambiareModalitaEditorCommand;
		public ICommand cambiareModalitaEditorCommand {
			get {
				if( _cambiareModalitaEditorCommand == null ) {
					_cambiareModalitaEditorCommand = new RelayCommand( param => cambiareModalitaEdit( (string)param ), p => true );
				}
				return _cambiareModalitaEditorCommand;
			}
		}

		private RelayCommand _caricareMaschereCommand;
		public ICommand caricareMaschereCommand {
			get {
				if( _caricareMaschereCommand == null ) {
					_caricareMaschereCommand = new RelayCommand( param => caricareMaschere( (string)param ),
						                                         param => possoCaricareMaschere( (string)param ) );
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

		private RelayCommand _svuotareListaDaModificareCommand;
		public ICommand svuotareListaDaModificareCommand {
			get {
				if( _svuotareListaDaModificareCommand == null ) {
					_svuotareListaDaModificareCommand = new RelayCommand( p => svuotareListaDaModificare(),
																		  p => possoSvuotareListaDaModificare );
				}
				return _svuotareListaDaModificareCommand;
			}
		}

		private RelayCommand _commandSfogliarePerFileCornice;
		public ICommand commandSfogliarePerFileCornice {
			get {
				if( _commandSfogliarePerFileCornice == null ) {
					_commandSfogliarePerFileCornice = new RelayCommand( p => sfogliarePerFileCornice(),
																		p => possoSfogliarePerFileCornice );
				}
				return _commandSfogliarePerFileCornice;
			}
		}

		private RelayCommand _resettareValoreEffettoCommand;
		public ICommand resettareValoreEffettoCommand {
			get {
				if( _resettareValoreEffettoCommand == null ) {
					_resettareValoreEffettoCommand = new RelayCommand( propName => resettareValoreEffetto( (string)propName ) );
				}
				return _resettareValoreEffettoCommand;
			}
		}

		private RelayCommand _fdmPaginareCommand;
		public ICommand fdmPaginareCommand {
			get {
				if( _fdmPaginareCommand == null ) {
					_fdmPaginareCommand = new RelayCommand( paramSposta => fdmPaginare( (string)paramSposta ),
															paramSposta => fdmPossoPaginare( (string)paramSposta ) );
				}
				return _fdmPaginareCommand;
			}
		}

		private RelayCommand _aggiungereLogoCommand;
		public ICommand  aggiungereLogoCommand {
			get {
				if( _aggiungereLogoCommand == null ) {
					_aggiungereLogoCommand = new RelayCommand( p => aggiungereLogo(),
															   p => possoModificareLaFoto );
				}
				return _aggiungereLogoCommand;
			}
		}


		private RelayCommand _ruotareQuadroCommand;
		public ICommand ruotareQuadroCommand {
			get {
				if( _ruotareQuadroCommand == null ) {
					_ruotareQuadroCommand = new RelayCommand( p => ruotareQuadro(),
															  p => possoModificareLaFoto );
				}
				return _ruotareQuadroCommand;
			}
		}

		private RelayCommand _commandGotFocus;
		public ICommand commandGotFocus {
			get {
				if( _commandGotFocus == null ) {
					_commandGotFocus = new RelayCommand( p => gotFocus(), p => true );
				}
				return _commandGotFocus;
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
		public void forseInizioModifiche() {

			if( ! _faseRipristinoFoto ) 
				modificheInCorso = true;
		}

		private void ruotare( int pGradi ) {

			bool elimina = false;

			forseCambioTrasformazioneCorrente( TFXPOS_ROTATE );

			// Gestisco lo spegnimento
			if( ((RotateTransform)trasformazioneCorrente).Angle == pGradi )
				elimina = true;
			else
				((RotateTransform)trasformazioneCorrente).Angle = pGradi;

			forseInizioModifiche();

			OnPropertyChanged( "isRotatePiu90Checked" );
			OnPropertyChanged( "isRotateMeno90Checked" );
			OnPropertyChanged( "trasformazioneRotate" );

			// Infine (dopo aver provocato l'aggiornamento dei controlli) eventualmente elimino la trasformazione
			if( pGradi == 0 )
				elimina = true;

			if( elimina )
				removeTrasformazione( TFXPOS_ROTATE );
		}

		private void zoomare( double factor ) {
			forseCambioTrasformazioneCorrente( TFXPOS_ZOOM );
			((ScaleTransform)trasformazioneZoom).ScaleX = factor;
			((ScaleTransform)trasformazioneZoom).ScaleY = factor;
		}

		private void traslare( double value ) {
			forseCambioTrasformazioneCorrente( TFXPOS_TRANSLATE );
			((TranslateTransform)trasformazioneTranslate).X = value;
			((TranslateTransform)trasformazioneTranslate).Y = value;
		}

		private void grayScale( bool addRemove ) {

			if( addRemove ) {
				sepia( false );  // Eventualmente prima spengo la sepia se esiste
				forseCambioEffettoCorrente( typeof( GrayscaleEffect ) );
			} else
				removeEffetto( typeof( GrayscaleEffect ) );

			OnPropertyChanged( "isGrayscaleChecked" );
		}

		private void tornareOriginale() {

			// elimino tutti gli effetti creati
			resetEffettiAndTrasformazioni();
			
			// per ogni foto elimino le correzioni e ricreo il provino partendo dall'originale.
			fotoRitoccoSrv.tornaOriginale( fotografiaInModifica );

			// Devo reidratare la foto originale 
			AiutanteFoto.idrataImmaginiFoto( fotografiaInModifica, IdrataTarget.Originale );

			forzaRefreshStato();
		}

		private void sepia( bool addRemove ) {

			if( addRemove ) {
				grayScale( false );
				forseCambioEffettoCorrente( typeof( SepiaEffect ) );
			}  else
				removeEffetto( typeof( SepiaEffect ) );

			forseInizioModifiche();

			OnPropertyChanged( "isSepiaChecked" );
		}

		private void removeEffetto( Type effettoType ) {

			if( effettoCorrente != null && effettoCorrente.GetType() == effettoType )
				effettoCorrente = null;

			effetti.RemoveAll( e => e.GetType() == effettoType );
			forzaRefreshStato();
		}


		void resettareValoreEffetto( string propertiesName ) {

			ShaderEffectBase effetto = null;

			string [] propertiesNameArray = propertiesName.Split( ';' );

			foreach( var propertyName in propertiesNameArray ) {
		 
				if( propertyName == "Brightness" || propertyName == "Contrast" )
					effetto = effetti.FirstOrDefault( t => t is LuminositaContrastoEffect );
				else if( propertyName == "Red" || propertyName == "Green" || propertyName == "Blue" )
					effetto = effetti.FirstOrDefault( t => t is DominantiEffect );

				if( effetto != null )
					effetto.resetToDefaultValue( propertyName );
			}
		}


		private void removeCorrezione( Type type ) {
						
			forseInizioModifiche();

			fotoRitoccoSrv.removeCorrezione( fotografiaInModifica, type );
		}

		private void flip( bool crea ) {

			if( crea ) {
				forseCambioTrasformazioneCorrente( TFXPOS_FLIP );
				((ScaleTransform)trasformazioneCorrente).ScaleX = -1;
			} else {
				removeTrasformazione( TFXPOS_FLIP );	
			}

			forseInizioModifiche();

			OnPropertyChanged( "isFlipChecked" );
		}


		private void removeTrasformazione( int tfxPosiz ) {

			Transform quale = trasformazioni.Children[tfxPosiz];

			if( trasformazioneCorrente != null && trasformazioneCorrente.Equals( quale ) )
				trasformazioneCorrente = null;

			trasformazioni.Children[tfxPosiz] = tfxNulla;

			forzaRefreshStato();
		}



		private void applicareCorrezioni() {

			fotografiaInModifica.correzioniXml = null;
			fotografiaInModifica.imgProvino.Dispose();
			fotografiaInModifica.imgProvino = null;
			// TODO forse devo anche eliminare da disco il file con la risultante !!!


			// Nel fotoritocco, la maschera viene gestita come una correzione
			if( mascheraAttiva != null ) {
				string nomeFile = Path.GetFileName( mascheraAttiva.UriSource.LocalPath );
				Maschera maschera = new Maschera {
					nome = nomeFile,
					width = mascheraAttiva.PixelWidth,
					height = mascheraAttiva.PixelHeight
				};
				addCorrezione( maschera );
			}


			// Vado ad aggiungerli solo al momento di applicare per davvero
			// Prima tratto gli effetti

			CorrezioniList lista1 = fotoRitoccoSrv.converteInCorrezioni( effetti.AsEnumerable<Object>() );
			foreach( Correzione correz in lista1 )
				addCorrezione( correz );


			// Poi tratto le trasformazioni : occhio sono posizionali
			addCorrezione( TipoCorrezione.Specchio, trasformazioni.Children[TFXPOS_FLIP]      );
			addCorrezione( TipoCorrezione.Ruota,    trasformazioni.Children[TFXPOS_ROTATE]    );
			addCorrezione( TipoCorrezione.Zoom,     trasformazioni.Children[TFXPOS_ZOOM]      );
			addCorrezione( TipoCorrezione.Trasla,   trasformazioni.Children[TFXPOS_TRANSLATE] );

			// IL logo lo metto per ultimo perché potrebbe andare su di una immagine traslata o zoomata
			if( logo != null ) {
				addCorrezione( logo );
			}

			gestoreImmaginiSrv.salvaCorrezioniTransienti( fotografiaInModifica );

			AiutanteFoto.creaProvinoFoto( fotografiaInModifica );

			// Devo informare tutti che questa foto è cambiata
			FotoModificateMsg msg = new FotoModificateMsg( this, fotografiaInModifica );
			LumenApplication.Instance.bus.Publish( msg );

			// Ora che ho persistito, concludo "dicamo cosi" la transazione, faccio una specie di commit.
			// Ormai che li ho acquisiti, li svuoto

			modificheInCorso = false;
		}

		/// <summary>
		/// Aggiungo la correzione a tutte le foto selezionate
		/// </summary>
		private void addCorrezione( Correzione correzione ) {

			// Sul correzione di traslazione, devo riportare due proprietà di front-end
			// Mi serviranno per riproporzionare durante la provinatura, oppure la risultante.
			if( correzione is Trasla && modalitaEdit == ModalitaEdit.FotoRitocco ) {
				((Trasla)correzione).rifW = frpContenitoreW;
				((Trasla)correzione).rifH = frpContenitoreH;
			}

			if( correzione is Zoom )
				((Zoom)correzione).quadroRuotato = this.quadroRuotato;

			fotoRitoccoSrv.addCorrezione( fotografiaInModifica, correzione );
		}

		void addCorrezione( TipoCorrezione qualeTipo, Transform trasformazione ) {

			if( ! isTrasformazioneNulla(trasformazione) ) {
				Correzione ccc = fotoRitoccoSrv.converteInCorrezione( qualeTipo, trasformazione );
				if( ccc != null )
					addCorrezione( ccc );
			}
		}

		/// <summary>
		/// Elimino le correzioni transienti effettuate.
		/// In pratica torno a come è salvata la foto nel db con le correzioni persistenti.
		/// </summary>
		private void rifiutareCorrezioni() {

			bool saveFaseRipristinoFoto = _faseRipristinoFoto; // salvo
			try {
				// Evito di far scattare il trigger di modifiche in corso
				_faseRipristinoFoto = true;

				riposizionaControlliFotoritocco();  //  **  1  **

				forzaRefreshStato();  //  **  2  **

				modificheInCorso = false;

				pubblicaMessaggioEffettiCambiati();  //  **  3 **

			} finally {
				_faseRipristinoFoto = saveFaseRipristinoFoto;
			}

		}

		/// <summary>
		/// Voglio rinunciare a modificare una foto e la tolgo anche dall'elenco di quelle in modifica.
		/// </summary>
		/// <param name="daTogliere"></param>
		internal void rifiutareCorrezioni( Fotografia daTogliere, bool toglila ) {

			if( toglila ) {
				resetEffettiAndTrasformazioni();
			}

			modificheInCorso = false;
		}

		/// <summary>
		/// Siccome alcune azioni avvengono solo nella UI, devo forzare in qualche 
		/// modo l'aggiornamento dello stato dei pulsanti.
		/// </summary>
		public void forzaRefreshStato() {
			
			OnPropertyChanged( "possoTornareOriginale" );
			OnPropertyChanged( "possoApplicareCorrezione" );
			OnPropertyChanged( "possoApplicareCorrezioni" );
			OnPropertyChanged( "possoRifiutareCorrezioni" );
			OnPropertyChanged( "possoModificareConEditorEsterno" );

			OnPropertyChanged( "possoSvuotareListaDaModificare" );

			OnPropertyChanged( "isGrayscaleChecked" );
			OnPropertyChanged( "isSepiaChecked" );
			OnPropertyChanged( "isRotatePiu90Checked" );
			OnPropertyChanged( "isRotateMeno90Checked" );
			OnPropertyChanged( "isFlipChecked" );
			
			OnPropertyChanged( "isLuminositaChecked" );
			OnPropertyChanged( "isContrastoChecked" );

			OnPropertyChanged( "effetto1" );
			OnPropertyChanged( "effetto2" );
			OnPropertyChanged( "effetto3" );
			OnPropertyChanged( "dominantiEffect" );
			OnPropertyChanged( "effettoCorrente" );

			OnPropertyChanged( "trasformazioneFlip" );
			OnPropertyChanged( "trasformazioneRotate" );
			OnPropertyChanged( "trasformazioneZoom" );
			OnPropertyChanged( "trasformazioneTranslate" );
			OnPropertyChanged( "trasformazioneCorrente" );

			// TODO sul torna originale, questo mi fa sparire la foto da modificare e non so perché !!
			OnPropertyChanged( "fotografiaInModifica" );
		}

		public static bool isTrasformazioneNulla( Transform t ) {
			return (t == null || tfxNulla.Equals(t) );
		}

		/// <summary>
		/// Elimino tutti gli effetti e le trasformazioni che sono attivi.
		/// Gli effetti possono essere transienti o caricati da delle correzioni persistenti.
		/// </summary>
		void resetEffettiAndTrasformazioni() {

			if( effetti == null ) {
				// Creo gli effetti vuoti
				effetti = new List<ShaderEffectBase>();
			} else {
				// Questo serve anche per rimettere gli slider nella posiziona di default
				foreach( ShaderEffectBase effetto in effetti ) {
					effetto.resetToDefaultValue();
					BindingOperations.ClearAllBindings( effetto );
				}
				effetti.Clear();
			}

			if( trasformazioni == null ) {
				trasformazioni = new TransformGroup();
			} else {
				// Rimuovo tutti i bindings
				foreach( Transform t in trasformazioni.Children )
					if( isTrasformazioneNulla(t) == false )
						BindingOperations.ClearAllBindings( t );
				// poi pulisco
				trasformazioni.Children.Clear();
			}

			trasformazioni.Children.Add( tfxNulla );
			trasformazioni.Children.Add( tfxNulla );
			trasformazioni.Children.Add( tfxNulla );
			trasformazioni.Children.Add( tfxNulla );

			logo = null;

			// Spengo le proprietà che indicano elementi correnti.
			effettoCorrente = null;
			trasformazioneCorrente = null;
			mascheraAttiva = null;
			quadroRuotato = false;

			modalitaEdit = ModalitaEdit.FotoRitocco;
			modificheInCorso = false;
		}


		public bool forseCambioTrasformazioneCorrente( int quale ) {

			bool creatoNuovo = false;

			if( _faseRipristinoFoto && isTrasformazioneNulla( trasformazioni.Children[quale] ) ) {
				// Se sto ripristinando una foto, e la trasformazione indicata è inefficace, esco subito
				return false;
			}

			// Controllo se la trasformazione corrente è già quello attuale non faccio niente.
			if( trasformazioneCorrente == null || !trasformazioneCorrente.Equals( trasformazioni.Children[quale] ) ) {

				// Se non l'ho trovata, allora la creo
				if( isTrasformazioneNulla(trasformazioni.Children[quale] ) ) {
					trasformazioni.Children[quale] = creaTrasformazione( quale );
					creatoNuovo = true;
				}

				trasformazioneCorrente = trasformazioni.Children[quale];
			}

			return creatoNuovo;
		}
		
		private Transform creaTrasformazione( int quale ) {

			Transform tx;

			if( quale == TFXPOS_FLIP )
				tx = new ScaleTransform();
			else if( quale == TFXPOS_ZOOM )
				tx = new ScaleTransform();
			else if( quale == TFXPOS_ROTATE )
				tx = new RotateTransform();
			else if( quale == TFXPOS_TRANSLATE )
				tx = new TranslateTransform();
			else
				throw new NotSupportedException( "indice di trasformazione non supportato: " + quale );	

			return tx;
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

			effettoCorrente = effetti.FirstOrDefault( e => e.GetType() == type );

			// Se l'effetto indicato è già in lista non faccio niente, altrimenti lo creo
			if( effettoCorrente == null ) {
				ShaderEffectBase nuovo = (ShaderEffectBase)Activator.CreateInstance( type );
				effetti.Add( nuovo );
				effettoCorrente = nuovo;
				creatoNuovo = true;
			}

			return creatoNuovo;
		}

		void addFotoDaModificare( Fotografia f ) {

			// Se la foto è già in lista non faccio nulla.
			if( fotografieDaModificare.Contains( f )  )
				return;

			// Per visualizzare la foto devo caricare il provino da disco
			try {
				AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Provino );
			} catch( Exception ee ) {
				dialogProvider.ShowError( ee.Message, "Errore apertura foto", null );
				return;
			}

			this.fotografieDaModificare.Add( f );

			// Ricalcolo il totale pagine per la paginazione
			int totalPages = fotografieDaModificare.Count / fdmFotoPerPagina;
			if (fotografieDaModificare.Count % fdmFotoPerPagina != 0) {
				totalPages += 1;
			}
			fdmTotPagine = totalPages;


			
		}


		/// <summary>
		/// Carico la collezione con le maschere
		/// </summary>
		void loadMaschereDaDisco( FiltroMask filtro ) {

			if( filtro == FiltroMask.MskSingole ) {
				if( maschereSingole == null )
					maschereSingole = loadMascheraDaDisco2( filtro );
			} else {
				if( maschereMultiple == null )
					maschereMultiple = loadMascheraDaDisco2( filtro );
			}
		}

		private ObservableCollection<BitmapImage> loadMascheraDaDisco2( FiltroMask filtro ) {
				
			ObservableCollection<BitmapImage> maschere = new ObservableCollection<BitmapImage>();

			string[] nomiMiniature = fotoRitoccoSrv.caricaMiniatureMaschere( filtro );
			if( nomiMiniature != null ) {
				foreach( string nomeMiniatura in nomiMiniature ) {

					try {
						maschere.Add( loadMascheraDaDisco( nomeMiniatura ) );

					} catch( Exception ee ) {
						_giornale.Error( "Maschera non caricata", ee );
					}
				}
			}

			return maschere;
		}




		/// <summary>
		/// Ne carica una sola e la ritorna
		/// </summary>
		/// <param name="nomeFileSrc">Il nome completo della maschera da caricare</param>
		/// <returns>una BitmapImage piccolina</returns>
		BitmapImage loadMascheraDaDisco( string nomeFileSrc ) {
			BitmapImage msk = new BitmapImage();
			msk.BeginInit();
			msk.CacheOption = BitmapCacheOption.OnLoad;
			//						msk.CreateOptions = BitmapCreateOptions.DelayCreation;
			msk.DecodePixelWidth = 80;
			msk.UriSource = new Uri( nomeFileSrc );
			msk.EndInit();
			msk.Freeze();
			return msk;
		}

		private void cambiareModalitaEdit( string nuovoModo ) {
			if( nuovoModo == "R" ) {
				this.modalitaEdit = ModalitaEdit.FotoRitocco;
				caricareMaschere( "S" );
			}
			if( nuovoModo == "M" ) {
				this.modalitaEdit = ModalitaEdit.GestioneMaschere;
				caricareMaschere( "M" );
			}
			fotografiaInModifica = null;
			mascheraAttiva = null;
		}


		/// <summary>
		///  verso:   S = Maschere singole
		///           M = Maschere multiple (Composizione)
		///           N = nessuna (svuota)
		/// </summary>
		/// <param name="verso"></param>
		private void caricareMaschere( string verso ) {

			if( verso == "S" ) {
				loadMaschereDaDisco( FiltroMask.MskSingole );
				maschereCW = new ListCollectionView( maschereSingole );
			} else if( verso == "M" ) {
				loadMaschereDaDisco( FiltroMask.MskMultiple );
				maschereCW = new ListCollectionView( maschereMultiple );
			} else {
				maschereCW = null;
			}
			OnPropertyChanged( "maschereCW" );
		}

		void attivareMaschera( object p ) {

			String nomeFile = null;
			String subFolder = null;
			Uri uriMaschera = null;
			string nomeMaschera = null;
			BitmapImage bi = null;

			if( p is Maschera ) {
				if( p == null ) {
					mascheraAttiva = null;
				} else {
					nomeFile = ((Maschera)p).nome;
				}

				subFolder = fotoRitoccoSrv.getCartellaMaschera( modalitaEdit == ModalitaEdit.FotoRitocco ? FiltroMask.MskSingole : FiltroMask.MskMultiple );
				nomeMaschera = Path.Combine( subFolder, nomeFile );

			} else {
				bi = (BitmapImage)p;
				nomeFile = Path.GetFileName( bi.UriSource.LocalPath );

				if( bi.UriSource.LocalPath.Contains( PathUtil.THUMB ) ) {
					// Siccome la bitmap selezionata è solo una thumnail di 80 pixel, rileggo il file vero effettivo.
					subFolder = fotoRitoccoSrv.getCartellaMaschera( modalitaEdit == ModalitaEdit.FotoRitocco ? FiltroMask.MskSingole : FiltroMask.MskMultiple );
					nomeMaschera = Path.Combine( subFolder, nomeFile );
				} else {
					// Questa è una maschera con il path completo selezionato da disco.
					nomeMaschera = Path.GetFullPath( bi.UriSource.LocalPath );
				}

			}
	
			// Le maschere quelle aggiunte al volo, non sono trattate come le altre che hanno una miniatura.
			if( File.Exists( nomeMaschera ) )
				uriMaschera = new Uri( nomeMaschera );
			else {
				_giornale.Error( "Maschera non esistente : " + nomeMaschera );
				return;
			}

			BitmapImage msk = new BitmapImage( uriMaschera );
			mascheraAttiva = msk;

			if( modalitaEdit == ModalitaEdit.FotoRitocco ) {
				// Devo modificare le dimensioni del contenitore.
				frpCalcolaDimensioniContenitore( (float)(msk.Width / msk.Height) );
			}

		}

		// Devo creare una immagine modificata in base
		internal void salvareImmagineIncorniciata(Fotografia fotoOrig, RenderTargetBitmap bitmapIncorniciata)
		{

			BitmapFrame frame = BitmapFrame.Create(bitmapIncorniciata);

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(frame);

			string tempFile = PathUtil.dammiTempFileConEstesione("png");

			// ----- scrivo su disco
			using (FileStream fs = FileUtil.waitForFile( tempFile ))
			{
				encoder.Save(fs);
				fs.Flush();
			}

			// Ora che il file su disco, devo portarlo dentro il database ed acquisirlo come una normale fotografia.
			fotoRitoccoSrv.clonaImmagineIncorniciata(fotoOrig, tempFile);

			// spengo tutto
			resetEffettiAndTrasformazioni();
		}

		void modificareConEditorEsterno() {

			try {
				isTuttoBloccato = true;

				fotoRitoccoSrv.modificaConProgrammaEsterno( fotografiaInModifica );

			} finally {
				isTuttoBloccato = false;
			}
		}

		void svuotareListaDaModificare() {

			// Anche contro il mio parere Ciccio vuole che quando si svuota la lista, si salvi eventualmente la foto ultima rimasta da salvare
			eventualeSalvataggioAutomatico();

			// Prima di svuotare la lista, voglio provare a liberare un pò di memoria che forse è inutile.
			foreach( Fotografia foto in fotografieDaModificare ) {
				// Il provino non posso rilasciarlo perché potrebbe essere visualizzato nella gallery o nel carrello
				AiutanteFoto.disposeImmagini( foto, IdrataTarget.Originale );
				AiutanteFoto.disposeImmagini( foto, IdrataTarget.Risultante );
			}

			fotografiaInModifica = null;

			fotografieDaModificare.Clear();
			fdmPaginaCorrente = 1;
			resetEffettiAndTrasformazioni();

			// Pubblico un messaggio di richiesta cambio pagina. Voglio tornare sulla gallery
			CambioPaginaMsg cambioPaginaMsg = new CambioPaginaMsg( this );
			cambioPaginaMsg.nuovaPag = "GalleryPag";
			LumenApplication.Instance.bus.Publish( cambioPaginaMsg );

		}

		private void sfogliarePerFileCornice() {

			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Devo aggiungere gli asterischi
			string testo = Configurazione.UserConfigLumen.estensioniGrafiche;
			StringBuilder extWithStar = new StringBuilder();
			foreach( string ext in Configurazione.estensioniGraficheAmmesse ) {
				extWithStar.Append( '*' );
				extWithStar.Append( ext );
				extWithStar.Append( ';' );
			}

			extWithStar.Remove( extWithStar.Length-1, 1 );  // tolgo l'ultimo punto e virgola

			dlg.DefaultExt = ".png";
			dlg.Filter = "Images |" + extWithStar;

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();
 
			// Get the selected file name and display in a TextBox
			if( result == true ) {

				try {
					// Carico la immagine e la aggiungo alla lista che sta a video
					BitmapImage bmp = loadMascheraDaDisco( dlg.FileName );
					maschereMultiple.Add( bmp );
				} catch( Exception ee ) {
					_giornale.Warn( "Errore in caricamento maschera : " + dlg.FileName, ee );
					dialogProvider.ShowError( ee.Message, "Imposssibile caricare cornice", null );
				}
			}
		}

		private void riposizionaControlliFotoritocco() {

			if( modalitaEdit == ModalitaEdit.GestioneMaschere ) {
//				frpCalcolaDimensioniContenitore( 0f );
				return;
			}

			// Lavoro solo se ho una sola singola foto selezionata
			if( fotografiaInModifica == null )
				return;

			quadroRuotato = false;
//			bool saveFaseRipristinoFoto = _faseRipristinoFoto; // salvo
			try {
//				_faseRipristinoFoto = true;
			

				// resetto tutti gli effetti e trasformazioni precedenti per resettare i controlli ui.
				resetEffettiAndTrasformazioni();

				Correzione maschera = null;

				// Quando lavoro puntualmente su di una foto, ho bisogno dell'immagine originale 
				// (TODO sarebbe più veloce avere un provino originale)
				AiutanteFoto.idrataImmaginiFoto( fotografiaInModifica, IdrataTarget.Originale );

				if( fotografiaInModifica.correzioniXml != null ) {

					CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>( fotografiaInModifica.correzioniXml );

					// carico Effetti precedenti
					IList<ShaderEffectBase> carEffetti = fotoRitoccoSrv.converteCorrezioni<ShaderEffectBase>( correzioni );
					effetti.AddRange( carEffetti );

					// carico Trasformazioni precedenti
					IList<Transform> carTrasformazioni = fotoRitoccoSrv.converteCorrezioni<Transform>( correzioni );
					caricaTrasformazioni( carTrasformazioni );

					
					// La maschera e il logo devo gestirli in modo separato.
					foreach( Correzione c in correzioni ) {

						if( c is Maschera ) {
							maschera = c;
							attivareMaschera( maschera );   // Questa chiamata già ridimensiona il contenitore giallo.
						}

						if( c is Logo ) {
							logo = (Logo)c;  // Per ora ne gestisco solo uno.
						}

						if( c is Zoom )
							this.quadroRuotato = ((Zoom)c).quadroRuotato;
					}

					// cerco di capire se la foto è verticale, per rigirare il contenitore
					// Ruota rrr = (Ruota)correzioni.FirstOrDefault( c => c is Ruota );
				}



				if( maschera == null && fotografiaInModifica.imgOrig != null ) {
					float ratio = fotografiaInModifica.imgOrig.rapporto;
					if( quadroRuotato )
						ratio = 1f / ratio;
					frpCalcolaDimensioniContenitore( ratio );
				}

			} finally {
//				_faseRipristinoFoto = saveFaseRipristinoFoto;
			}

		}

		void pubblicaMessaggioEffettiCambiati() {
			// Pubblico un messaggio per indicare che ci sono degli effetti cambiati.
			// Tramite questo messaggio, la UI può re-bindare i controlli interessati
			RitoccoPuntualeMsg ritoccoPuntualeMsg = new RitoccoPuntualeMsg( this );
			//			ritoccoPuntualeMsg.senderTag = puntuale;
			LumenApplication.Instance.bus.Publish( ritoccoPuntualeMsg );
		}
		

		/// <summary>
		///  Le trasformazioni devono essere nella posizione giusta
		/// </summary>
		/// <param name="carTrasformazioni"></param>
		private void caricaTrasformazioni( IList<Transform> carTrasformazioni ) {

			// Prima istanzio le trasformazioni vuote...

			// Attenzione l'ordine è importante: deve rispettare il posizionamento dato dalle costanti rispettive.
			trasformazioni.Children = new TransformCollection( 4 );
			trasformazioni.Children.Add( tfxNulla );   // TFXPOS_FLIP
			trasformazioni.Children.Add( tfxNulla );   // TFXPOS_ROTATE
			trasformazioni.Children.Add( tfxNulla );   // TFXPOS_ZOOM
			trasformazioni.Children.Add( tfxNulla );   // TFXPOS_TRANSLATE

			// ... poi sovrascrivo con le eventuali trasformazioni reali.
			foreach( Transform t in carTrasformazioni ) {
				int pos = getPosTrasf( t );
				trasformazioni.Children[pos] = t;
			}
		}

		private int getPosTrasf( Transform t ) {

			int ret;

			if( t is ScaleTransform ) {
				if( ((ScaleTransform)t).ScaleX == -1 )
					ret = TFXPOS_FLIP;
				else
					ret = TFXPOS_ZOOM;
			} else if( t is RotateTransform )
				ret = TFXPOS_ROTATE;
			else if( t is TranslateTransform )
				ret = TFXPOS_TRANSLATE;
			else
				throw new NotSupportedException( "tipo = " + t.GetType() );

			return ret;
		}

		
		// TODO questa operazione dovrebbe essere fatta dalla UI e non da qui!!! spostarla
		//      Inoltre adesso non c'è più tanto spazio, quindi potrebbe cambiare la logica.
		private void frpCalcolaDimensioniContenitore( float ratio ) {

			if( modalitaEdit == ModalitaEdit.FotoRitocco && ratio != 0f ) {
			
				const int MARG = 20;
				Size ris;
				Size s1 = new Size();
				Size s2 = new Size();

				// La GUI mi ha detto qual'è la dimensione massima del contenitore della foto in modifica (cioè  il massimo del rettangolo giallo)
				s1.Width = frpContenitoreMaxW - MARG;
				s1.Height = s1.Width / ratio;
				if( s1.Height > frpContenitoreMaxH )
					s1 = Size.Empty;

				s2.Height = frpContenitoreMaxH - MARG;
				s2.Width = s2.Height * ratio;
				if( s2.Width > frpContenitoreMaxW )
					s2 = Size.Empty;

				// Ora scelgo il risultato migliore.
				if( Size.Empty.Equals( s1 ) )
					ris = s2;
				else if( Size.Empty.Equals( s2 ) )
					ris = s1;
				else {
					// Scelgo l'area più grande
					ris = ProiettoreArea.max( s1, s2 );
				}
				
				frpContenitoreW = ris.Width;
				frpContenitoreH = ris.Height;
			} else {
				frpContenitoreH = 0;
				frpContenitoreW = 0;
			}
		}

		/// <summary>
		/// Sposto la paginazione della lista delle foto in attesa di modifica
		/// </summary>
		/// <param name="direzione">
		/// F = First
		/// P = Previous
		/// N = Next
		/// L = Last
		/// </param>
		void fdmPaginare( string direzione ) {

			int iniz = (fdmPaginaCorrente-1) * fdmFotoPerPagina;
			int fine = fdmPaginaCorrente * fdmFotoPerPagina;
			if( fine > fotografieDaModificare.Count )
				fine = fotografieDaModificare.Count;

			for( int pos=iniz; pos<fine; pos++ ) {
				AiutanteFoto.disposeImmagini( fotografieDaModificare[pos], IdrataTarget.Originale );
				AiutanteFoto.disposeImmagini( fotografieDaModificare[pos], IdrataTarget.Risultante );
			}

			if( direzione == "F" ) { // First
				fdmPaginaCorrente = 1;
			} else if( direzione == "P" ) { // Previous
				if( fdmPaginaCorrente > 1 )
					--fdmPaginaCorrente;
			} else if( direzione == "N" ) { // Next
				if( fdmPaginaCorrente < fdmTotPagine )
					++fdmPaginaCorrente;
			} else if( direzione == "L" ) { // Last
				fdmPaginaCorrente = fdmTotPagine;
			}
			fotografieDaModificareCW.Refresh();
			OnPropertyChanged( "fdmPaginaCorrente" );
		}

		public bool fdmPossoPaginare( string direzione ) {

			// Se non ci sono (abbastanza) foto, non si pagina.
			if( fotografieDaModificare == null || fotografieDaModificare.Count < fdmFotoPerPagina )
				return false;

			if( direzione == "F" ) { // First
				return (fdmPaginaCorrente > 1);
			} else if( direzione == "P" ) { // Previous
				return (fdmPaginaCorrente > 1);
			} else if( direzione == "N" ) { // Next
				return (fdmPaginaCorrente < fdmTotPagine);
			} else if( direzione == "L" ) { // Last
				return( fdmPaginaCorrente < fdmTotPagine);
			}

			return true;
		}

		bool fdmViewFilter( object foto ) {

			int index = fotografieDaModificare.IndexOf( (Fotografia)foto );
			int currentPageIndex = fdmPaginaCorrente - 1;
			if( index >= fdmFotoPerPagina * currentPageIndex && index < fdmFotoPerPagina * (currentPageIndex + 1) ) {
				return true;
			} else {
				return false;
			}
		}


		/// <summary>
		/// Quando viene selezionata una nuova fotografia,
		/// se la fotografia corrente ha subito modifiche
		/// la salvo automaticamente
		/// </summary>
		void eventualeSalvataggioAutomatico() {

			if( !salvataggioAutomatico )
				return;

			// Non usare il getter della property, ma direttamnte il field.
			if( _fotografiaInModifica == null )
				return;

			if( modificheInCorso == false )
				return;

			if( modalitaEdit == ModalitaEdit.FotoRitocco && applicareCorrezioniCommand.CanExecute( null ) ) {
				applicareCorrezioniCommand.Execute( null );
			}
		}

		/// <summary>
		/// Quando salvo una foto con il tasto rapido SPAZIO,
		/// voglio anche spostarmi sulla foto successiva.
		/// </summary>
		public void selezionaProssimaFoto() {

			bool esito = fotografieDaModificareCW.MoveCurrentToNext();
			if( esito == false ) {
				// Quando arrivo sull'ultima foto a video, se posso vado alla pagina seguente

				if( fdmPaginareCommand.CanExecute("N") ) {
									
					int pagPrec = fdmPaginaCorrente;

					fdmPaginareCommand.Execute( "N" );

					if( pagPrec != fdmPaginaCorrente ) {
						fotografieDaModificareCW.MoveCurrentToFirst();
					}
				}
				// Prima invece mi spostavo sulla prima foto della pagina attuale
				// fotografieDaModificareCW.MoveCurrentToFirst();
			}
		}

		/// <summary>
		/// Questo metodo esegue un ciclo sui loghi : passa da niente e poi cicla tutta la enumeration Logo.PosizLogo
		/// </summary>
		void aggiungereLogo() {

			if( logo == null ) {
				logo = LogoCorrettore.creaLogoDefault();
				logo.posiz = Logo.PosizLogo.SudEst;
			} else {

				// Devo provocare il property change perché la UI si aggiorni. Clono quindi il logo per riassegnarlo.
				Logo clone = (Logo)this.logo.Clone();
				 
				if( logo.posiz == Logo.PosizLogo.SudEst ) {
					clone.posiz = Logo.PosizLogo.SudOvest;
				} else if( logo.posiz == Logo.PosizLogo.SudOvest ) {
					clone.posiz = Logo.PosizLogo.NordOvest;
				} else if( logo.posiz == Logo.PosizLogo.NordOvest ) {
					clone.posiz = Logo.PosizLogo.NordEst;
				} else if( logo.posiz == Logo.PosizLogo.NordEst )
					clone = null;

				logo = clone; // Provoca il propertychanged
			}

			forseInizioModifiche();
		}


		void ruotareQuadro() {

			float ratio = fotografiaInModifica.imgOrig.rapporto;

			if( quadroRuotato ) {
				// torno regolare
				frpCalcolaDimensioniContenitore( ratio );
			} else {
				// rovescio
				frpCalcolaDimensioniContenitore( 1f/ratio );
			}

			quadroRuotato = (!quadroRuotato);
		}

		void gotFocus() {
			// Eventualmente idrato nuovamente i provini che potrebbero essere stati disidratati
			if( fotografieDaModificare != null )
				foreach( Fotografia foto in fotografieDaModificare )
					AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Provino, false );
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

		public void OnNext( Messaggio msg ) {

			if( msg is FotoDaModificareMsg )
				gestisciFotoDaModificareMsg( msg as FotoDaModificareMsg );

			if( msg is NuovaFotoMsg )
				gestisciNuovaFotoMsg( msg as NuovaFotoMsg );

			if( msg is EliminateFotoMsg )
				gestisciFotoEliminate( msg as EliminateFotoMsg );

		}

		// Sono state eliminate delle foto. Se per caso le avevo in modifica, le devo togliere
		private void gestisciFotoEliminate( EliminateFotoMsg eliminateFotoMsg ) {
			foreach( Fotografia ff in eliminateFotoMsg.listaFotoEliminate ) {
				rifiutareCorrezioni( ff, true );
				fotografieDaModificare.Remove( ff );
			}
		}

		private void gestisciNuovaFotoMsg( NuovaFotoMsg nuovaFotoMsg) {

//			if( AiutanteFoto.isMaschera(nuovaFotoMsg.foto) )
			if( 1==1 )
			{
				// E' stata memorizzata una nuova fotografia che in realtà è una cornice
				addFotoDaModificare( nuovaFotoMsg.foto );

				// Visto che l'immagine del provino viene caricata in un altro thread, qui non sono in grado di visualizzarla. La devo rileggere per forza.
				// Questo mi consente di visualizzare il provino come primo elemento 
				AiutanteFoto.idrataImmaginiFoto( nuovaFotoMsg.foto, IdrataTarget.Provino, true );
			}
		}

		private void gestisciFotoDaModificareMsg( FotoDaModificareMsg fotoDaModificareMsg ) {

			bool refresh = false;

			// Ecco che sono arrivate delle nuove foto da modificare
			// Devo aggiungerle alla lista delle foto in attesa di modifica.
			foreach( Fotografia f in fotoDaModificareMsg.fotosDaModificare )
				addFotoDaModificare( f );
			
			// Se richiesta la modifica immediata...
			if( fotoDaModificareMsg.immediata ) {
				// ... e sono in modalità di fotoritocco
				if( this.modalitaEdit == ModalitaEdit.FotoRitocco ) {
					// ... e non ho nessuna altra modifica in corso ...
					if( modificheInCorso == false ) {
						//fotografieDaModificareCW.SelectedItems.Clear();
						if( fotoDaModificareMsg.fotosDaModificare.Count == 1 ) {
							fotografiaInModifica = fotoDaModificareMsg.fotosDaModificare[0];
							refresh = true;
						}
					}
				}

				// Pubblico un messaggio di richiesta cambio pagina. Voglio andare sulla pagina del fotoritocco
				CambioPaginaMsg cambioPaginaMsg = new CambioPaginaMsg( this );
				cambioPaginaMsg.nuovaPag = "FotoRitoccoPag";
				LumenApplication.Instance.bus.Publish( cambioPaginaMsg );

				if( refresh ) {
					// TODO verificare se necessario rinfrescare il Filter
				}
			}

			forzaRefreshStato();
		}

		/// <summary>
		/// Quando cambia la fotografia in modifica.
		/// <param name="primaDopo">
		///   false = prima di cambiare 
		///   true  = dopo aver cambiato
		/// </param>
		/// </summary>
		void onFotografiaInModificaChanged( bool primaDiCambiare ) {

			if( primaDiCambiare ) {
				// Operazioni prima di spostare la foto corrente
				// Se la foto precedente aveva delle modifiche in sospeso, allora le salvo
				eventualeSalvataggioAutomatico();

			} else {

				// Operazioni dopo di spostare la foto corrente

				bool saveFaseRipristinoFoto = _faseRipristinoFoto; // salvo
				try {
					// Evito di far scattare il trigger di modifiche in corso
					_faseRipristinoFoto = true;

					// Riposiziono i controlli in modo da velocizzare il tutto.
					riposizionaControlliFotoritocco();  // ** 1 **

					// Se ho spento la fotografia, allora spengo anche una eventuale maschera
					if( fotografiaInModifica == null && modalitaEdit == ModalitaEdit.FotoRitocco ) {
						modificheInCorso = false;
						mascheraAttiva = null;
					}

					// Provoco la rilettura delle property che determinano lo stato dei pulsanti.
					forzaRefreshStato();  //  ** 2 **

					pubblicaMessaggioEffettiCambiati();  //  **  3 **

				} catch( Exception ) {
					fotografiaInModifica = null;
				} finally {
					_faseRipristinoFoto = saveFaseRipristinoFoto;
				}

			}
		}

#endregion Gestori Eventi

	}
}
