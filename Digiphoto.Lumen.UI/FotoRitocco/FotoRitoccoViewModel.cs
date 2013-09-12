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

namespace Digiphoto.Lumen.UI.FotoRitocco {


	public class FotoRitoccoViewModel : ViewModelBase, IObserver<Messaggio> {

		public delegate void EditorModeChangedEventHandler( object sender, EditorModeEventArgs args );
		public event EditorModeChangedEventHandler editorModeChangedEvent;

		
		public FotoRitoccoViewModel() {

			if( IsInDesignMode ) {
				// caricare qualche foto a casaccio
			} else {

				// Mi sottoscrivo per ascoltare i messaggi di richiesta di modifica delle foto.
				IObservable<Messaggio> observable = LumenApplication.Instance.bus.Observe<Messaggio>();
				observable.Subscribe( this );

				selettoreAzioniRapideViewModel = new SelettoreAzioniRapideViewModel();

				fotografieDaModificare = new ObservableCollectionEx<Fotografia>();
				fotografieDaModificareCW = new MultiSelectCollectionView<Fotografia>( fotografieDaModificare );

				fotografieDaModificareCW.SelectionChanged += onFotografieDaModificareSelectionChanged;

				modalitaEdit = ModalitaEdit.FotoRitoccoPuntuale;
			}

			cfg = Configurazione.UserConfigLumen;

			resetEffettiAndTrasformazioni();
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

					OnPropertyChanged( "trasformazione1" );
					OnPropertyChanged( "trasformazione2" );

					forseInizioModifiche();
				}	
			}
		}

		private MultiSelectCollectionView<Fotografia> _fotografieDaModificareCW;
		public MultiSelectCollectionView<Fotografia> fotografieDaModificareCW
		{
			get
			{
				return _fotografieDaModificareCW;
			}
			set
			{
				if (_fotografieDaModificareCW != value)
				{
					_fotografieDaModificareCW = value;
					selettoreAzioniRapideViewModel.fotografieCW = value;
					OnPropertyChanged("fotografieDaModificareCW");
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
				return isGestioneMaschereDisattiva && modificheInCorso == true && isAlmenoUnaFotoSelezionata;
			}
		}

		public bool possoRifiutareCorrezioni {
			get {

				return modalitaEdit == ModalitaEdit.GestioneMaschere || possoSalvareCorrezioni;
			}
		}

		public bool possoApplicareCorrezione {
			get {
				return isGestioneMaschereDisattiva && isAlmenoUnaFotoSelezionata;
			}
		}

		public bool possoTornareOriginale {
			get {
				return isAlmenoUnaFotoSelezionata;
			}
		}

		public bool possoRiempireElencoInModifica {
			get {
				return esistonoFotoInAttesaDiModifica /* && modificheInCorso == false */;
			}
		}

		public bool possoSvuotareElencoInModifica {
			get {
				return isAlmenoUnaFotoSelezionata /* && modificheInCorso == false */;
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

		public List<Transform> trasformazioni {
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

		public Transform trasformazione1 {
			get {
				return trasformazioni != null && trasformazioni.Count > 0 ? trasformazioni[0] : null;
			}
		}
		public Transform trasformazione2 {
			get {
				return trasformazioni != null && trasformazioni.Count > 1 ? trasformazioni[1] : null;
			}
		}


		/// <summary>
		///  Cerco se esiste lo specifico effetto nella lista di tutti gli effetti
		/// </summary>
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
				return (trasformazioni != null && trasformazioni.Exists( e => e is ScaleTransform ));
			}
		}

		public bool isRotatePiu90Checked {
			get {
				return (trasformazioni != null && trasformazioni.Exists( t => t is RotateTransform && ((RotateTransform)t).Angle == 90.0d ));
			}
		}

		public bool isRotateMeno90Checked {
			get {
				return (trasformazioni != null && trasformazioni.Exists( t => t is RotateTransform && ((RotateTransform)t).Angle == -90.0d ));
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

		public bool selectorChecked {
			get {
				return _croppingAdorner != null && esistonoFotoInAttesaDiModifica;
			}
		}

		public bool selectorEnabled {
			get {
				return (isGestioneMaschereDisattiva && contaSelez == 1);
			}
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

		private ObservableCollection<BitmapImage> maschere {
			get;
			set;
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
					OnPropertyChanged( "isGestioneMaschereAttiva" );
					OnPropertyChanged( "isGestioneMaschereDisattiva" );
					OnPropertyChanged( "possoSalvareMaschera" );
					forzaRefreshStato();
					
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
				return String.IsNullOrEmpty( Configurazione.UserConfigLumen.editorImmagini ) == false &&
					   modalitaEdit == ModalitaEdit.FotoRitoccoPuntuale &&
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
				return isGestioneMaschereAttiva && mascheraAttiva != null;
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

/*
		/// <summary>
		/// Questa è la collezione puntuale delle correzioni della singola foto selezionata per il fotoritocco puntuale.
		/// </summary>
		private CorrezioniList correzioniPuntuali {
			get;
			set;
		}
*/

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

		private RelayCommand _attivareSelectorCommand;
		public ICommand attivareSelectorCommand {
			get {
				if( _attivareSelectorCommand == null ) {
					_attivareSelectorCommand = new RelayCommand( param => this.attivareSelector( (FrameworkElement)param ),
																 param => selectorEnabled );
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

		private RelayCommand _commandRiempireElencoInModifica;
		public ICommand commandRiempireElencoInModifica {
			get {
				if( _commandRiempireElencoInModifica == null ) {
					_commandRiempireElencoInModifica = new RelayCommand( p => riempireElencoInModifica(),
																		 p => possoRiempireElencoInModifica );
				}
				return _commandRiempireElencoInModifica;
			}
		}

		private RelayCommand _commandSvuotareElencoInModifica;
		public ICommand commandSvuotareElencoInModifica {
			get {
				if( _commandSvuotareElencoInModifica == null ) {
					_commandSvuotareElencoInModifica = new RelayCommand( p => svuotareElencoInModifica(),
																		 p => possoSvuotareElencoInModifica );
				}
				return _commandSvuotareElencoInModifica;
			}
		}
		
		private RelayCommand _commandBrowseForFileCornice;
		public ICommand commandBrowseForFileCornice {
			get {
				if( _commandBrowseForFileCornice == null ) {
					_commandBrowseForFileCornice = new RelayCommand( p => browseForFileCornice() );
				}
				return _commandBrowseForFileCornice;
			}
		}

		private RelayCommand _maschereCropCommand;
		public ICommand maschereCropCommand
		{
			get
			{
				if (_maschereCropCommand == null)
				{
					_maschereCropCommand = new RelayCommand(p => maschereCrop((string)p));
				}
				return _maschereCropCommand;
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
			
			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.addCorrezione( f, correzione );
		}

		private void ruotare( int pGradi ) {

			if( pGradi != 0 ) {
				forseCambioTrasformazioneCorrente( typeof(RotateTransform) );

				// Gestisco lo spegnimento
				if( ((RotateTransform)trasformazioneCorrente).Angle == pGradi )
					removeTrasformazione( typeof( RotateTransform ) );
				else
					((RotateTransform)trasformazioneCorrente).Angle = pGradi;
			} else
				removeTrasformazione( typeof(RotateTransform) );

			forseInizioModifiche();

			OnPropertyChanged( "isRotatePiu90Checked" );
			OnPropertyChanged( "isRotateMeno90Checked" );
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

			_giornale.Debug( "Richiesto tornaoriginale" );
			// elimino tutti gli effetti creati
			resetEffettiAndTrasformazioni();
			
			// per ogni foto elimino le correzioni e ricreo il provino partendo dall'originale.
			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.tornaOriginale( f );

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

			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.removeCorrezione( f, type );
		}

		private void flip( bool crea ) {

			if( crea ) {
				forseCambioTrasformazioneCorrente( typeof( ScaleTransform ) );
				((ScaleTransform)trasformazioneCorrente).ScaleX = -1;
			} else {
				removeTrasformazione( typeof( ScaleTransform ) );	
			}

			forseInizioModifiche();

			OnPropertyChanged( "isFlipChecked" );
		}

		void removeTrasformazione( Type typeTrasformazione ) {

			if( trasformazioneCorrente != null && trasformazioneCorrente.GetType() == typeTrasformazione )
				trasformazioneCorrente = null;

			trasformazioni.RemoveAll( e => e.GetType() == typeTrasformazione );
			forzaRefreshStato();
		}



		private void salvareCorrezioni() {


			bool puntuale =  (modalitaEdit == ModalitaEdit.FotoRitoccoPuntuale && fotografieDaModificareCW.SelectedItems.Count == 1); 
			
			if( puntuale ) {

				// SSS Sto lavorando con il fotoritocco puntuale. Devo "perdere" tutte le correzioni in modo che non si sommino.
				fotografieDaModificareCW.SelectedItems [0].correzioniXml = null;

				fotografieDaModificareCW.SelectedItems [0].imgProvino.Dispose();
				fotografieDaModificareCW.SelectedItems [0].imgProvino = null;
			}



			// Vado ad aggiungerli solo al momento di applicare per davvero
			// Prima tratto gli effetti
			CorrezioniList lista1 = fotoRitoccoSrv.converteInCorrezioni( effetti.AsEnumerable<Object>() );
			foreach( Correzione correz in lista1 )
				addCorrezione( correz );

			// Poi tratto le trasformazioni
			CorrezioniList lista2 = fotoRitoccoSrv.converteInCorrezioni( trasformazioni );
			foreach( Correzione correz in lista2 )
				addCorrezione( correz );


			// Ormai che li ho acquisiti, li svuoto
			resetEffettiAndTrasformazioni();

			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems ) {
				fotoRitoccoSrv.salvaCorrezioniTransienti( f );
			}


			// Ora che ho persistito, concludo "dicamo cosi" la transazione, faccio una specie di commit.
			modificheInCorso = false;
		}

		private void rifiutareCorrezioni() {

			// resetEffetti();

			// Se ho una sola foto, allora i controlli devono riposizionarsi al giusto valore. 
			// Questo succede solo quando riseleziono la singola foto per la prima volta.
			// Per far si che questo accada, spengo l'unica foto, così l'utente è obbligato a riselezionarla.
			// TODO per ora cosi. Poi sarebbe bello che funzionasse in modo naturale, senza questa forzatura.
//			bool toglila = (fotografieDaModificareCW.SelectedItems.Count == 1);

			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				rifiutareCorrezioni( f, false );

			riposizionaControlliFotoritoccoPuntuale();

			modificheInCorso = false;
		}

		/// <summary>
		/// Voglio rinunciare a modificare una foto e la tolgo anche dall'elenco di quelle in modifica.
		/// </summary>
		/// <param name="daTogliere"></param>
		internal void rifiutareCorrezioni( Fotografia daTogliere, bool toglila ) {

			resetEffettiAndTrasformazioni();


//			fotoRitoccoSrv.undoCorrezioniTransienti( daTogliere );
			if( toglila ) {
				// La spengo
				fotografieDaModificareCW.Deselect( daTogliere );

				// Se non mi rimane piu nulla, azzero tutti gli effetti.
				if( fotografieDaModificareCW.SelectedItems.Count <= 0 )
					resetEffettiAndTrasformazioni();
				else {
					forzaRefreshStato();  // In teoria dovrebbe farlo già il Deselect. ma non funziona. da controllare.
				}
			}

		}

		/// <summary>
		/// Siccome alcune azioni avvengono solo nella UI, devo forzare in qualche 
		/// modo l'aggiornamento dello stato dei pulsanti.
		/// </summary>
		public void forzaRefreshStato() {
			
			// TODO queste due proprietà non sarebbero da gestire qui nel viewmodel, ma nella ui.
			OnPropertyChanged( "selectorChecked" );
			OnPropertyChanged( "selectorEnabled" );
			
			OnPropertyChanged( "possoTornareOriginale" );
			OnPropertyChanged( "possoApplicareCorrezione" );
			OnPropertyChanged( "possoSalvareCorrezioni" );
			OnPropertyChanged( "possoRifiutareCorrezioni" );
			OnPropertyChanged( "possoModificareConEditorEsterno" );

			OnPropertyChanged( "possoRiempireElencoInModifica" );
			OnPropertyChanged( "possoSvuotareElencoInModifica" );
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

			OnPropertyChanged( "trasformazione1" );
			OnPropertyChanged( "trasformazione2" );
			OnPropertyChanged( "trasformazioneCorrente" );
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
				trasformazioni = new List<Transform>();
			} else {
				trasformazioni.Clear();					
			}

			// Spengo le proprietà che indicano elementi correnti.
			effettoCorrente = null;
			trasformazioneCorrente = null;
			attivareSelector( null );  // Spegno eventuale selettore
			mascheraAttiva = null;


			modalitaEdit = ModalitaEdit.FotoRitoccoPuntuale;
		}


		public bool forseCambioTrasformazioneCorrente( Type type ) {

			bool creatoNuovo = false;

			// Controllo se la trasformazione corrente è già quello attuale non faccio niente.
			if( trasformazioneCorrente != null && trasformazioneCorrente.GetType() == type )
				return false;

			trasformazioneCorrente = trasformazioni.FirstOrDefault( t => t.GetType() == type );

			// Se non l'ho trovata, allora la creo
			if( trasformazioneCorrente == null ) {
				Transform nuovo = (Transform)Activator.CreateInstance( type );
				trasformazioni.Add( nuovo );
				trasformazioneCorrente = nuovo;
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


		/// <summary>
		/// Attivo il selettore sulla immagine corrente
		/// </summary>
		/// TODO questo metodo non dovrebbe stare qui ma nel form. Però forse mi serviva per
		private void attivareSelector( FrameworkElement imageToCrop ) {

			if( imageToCrop != null ) {
				AddCropToElement( imageToCrop );
				_brOriginal = _croppingAdorner.Fill;
				RefreshCropImage();
			} else {
				RemoveCropFromCur();
			}

			OnPropertyChanged( "selectorChecked" );
			OnPropertyChanged( "selectorEnabled" );
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


			_croppingAdorner.AspectRatio = determinaRatioAreaStampa();
			_croppingAdorner.MantainAspectRatio = (_croppingAdorner.AspectRatio >= 0);

			aly.Add( _croppingAdorner );
			// Questa è una anteprima che non mi serve
			// imgCrop.Source = _croppingAdorner.BpsCrop();
			_croppingAdorner.CropChanged += CropChanged;
			_felCur = fel;

			SetClipColorGrey();
		}

		/// <summary>
		/// Prendo la prima stampante dalla lista di quelle abbinate.
		/// </summary>
		/// <returns></returns>
		private float determinaRatioAreaStampa() {

			ISpoolStampeSrv srv = LumenApplication.Instance.getServizioAvviato<ISpoolStampeSrv>();
			return srv.ratioAreaStampabile;
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
				return selectorChecked;
			}
		}

		void addFotoDaModificare( Fotografia f ) {

			if( this.fotografieDaModificare.Contains( f ) == false ) {
				//fotografieDaModificareCW.AddNewItem( f );
				if (fotografieDaModificare.Count >= Configurazione.UserConfigLumen.lungFIFOFotoMod)
				{
					_giornale.Debug("Ho raggiunto il massimo numero di foto da modificare di " + Configurazione.UserConfigLumen.lungFIFOFotoMod + " fotografie");

					//Recupero l'ultima foto che non è tra quelle selezionate

					var listaIds = from le in fotografieDaModificareCW.SelectedItems
								   select le.id;

					Fotografia isDelFoto = null;
//					if( listaIds.Count() > 0 ) {
						var qq = fotografieDaModificare.Where( ff => !listaIds.Contains( ff.id ) );
						if( qq != null && qq.Count() > 0 )
							isDelFoto = qq.Last();
//					}

					if (isDelFoto == null)
					{
						_giornale.Debug("Non ho trovato nessuna foto da eliminare dalla coda");

					} else
						this.fotografieDaModificare.Remove(isDelFoto);
				}

				this.fotografieDaModificare.Insert( 0, f );

				AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Provino );
			}
		}


		/// <summary>
		/// Carico la collezione con le 
		/// </summary>
		void loadMaschereDaDisco() {

			if( maschere == null ) {

				maschere = new ObservableCollection<BitmapImage>();

				string [] nomiMiniature = fotoRitoccoSrv.caricaMiniatureMaschere();
				foreach( string nomeMiniatura in nomiMiniature ) {

					try {
						maschere.Add( loadMascheraDaDisco( nomeMiniatura ) );

					} catch( Exception ee ) {
						_giornale.Error( "Maschera non caricata", ee );						
					}
				}
			}
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

		/// <summary>
		///  verso:   H = solo orizzontali
		///           V = solo verticali
		///           T = tutte
		///           N = nessuna (svuota)
		/// </summary>
		/// <param name="verso"></param>
		private void caricareMaschere( string verso ) {

			if( verso == "T" ) {
				loadMaschereDaDisco();
				maschereCW = new ListCollectionView( maschere );
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
				
			// Siccome la bitmap selezionata è solo una thumnail di 80 pixel, rileggo il file vero effettivo.
			BitmapImage bi = (BitmapImage)p;
			string nomeFile = Path.GetFileName( bi.UriSource.LocalPath );
			Uri uriMaschera = null;

			string nomeMaschera = Path.Combine( Configurazione.UserConfigLumen.cartellaMaschere, nomeFile );
			// Le maschere quelle aggiunte al volo, non sono trattate come le altre che hanno una miniatura.
			if( File.Exists( nomeMaschera ) )
				uriMaschera = new Uri( nomeMaschera );
			else
				uriMaschera = bi.UriSource;

			BitmapImage msk = new BitmapImage( uriMaschera );
			mascheraAttiva = msk;

			svuotareElencoInModifica( false );

			// cambio stato. Vado in modalità di editing maschere
			modalitaEdit = ModalitaEdit.GestioneMaschere;


			// Mi serve per accendere i pulsanti di rifiuta e salva
			forseInizioModifiche();
		}


		// Devo creare una immagine modificata in base
		internal void salvareImmagineIncorniciataWithArtista( RenderTargetBitmap bitmapIncorniciata ) {

			BitmapFrame frame = BitmapFrame.Create( bitmapIncorniciata );

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add( frame );

			string tempFile = PathUtil.dammiTempFileConEstesione( "png" );

			// ----- scrivo su disco
			using( FileStream fs = new FileStream( tempFile, FileMode.Create ) ) {
				encoder.Save( fs );
				fs.Flush();
			}

			// Ora che il file su disco, devo portarlo dentro il database ed acquisirlo come una normale fotografia.
			fotoRitoccoSrv.acquisisciImmagineIncorniciataWithArtista( tempFile );

			// spengo tutto
			resetEffettiAndTrasformazioni();
		}

		// Devo creare una immagine modificata in base
		internal void salvareImmagineIncorniciata(Fotografia fotoOrig, RenderTargetBitmap bitmapIncorniciata)
		{

			BitmapFrame frame = BitmapFrame.Create(bitmapIncorniciata);

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(frame);

			string tempFile = PathUtil.dammiTempFileConEstesione("png");

			// ----- scrivo su disco
			using (FileStream fs = new FileStream(tempFile, FileMode.Create))
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

				// Accodo le stampe da modificare
				fotoRitoccoSrv.modificaConProgrammaEsterno( fotografieDaModificareCW.SelectedItems.ToArray() );

			} finally {
				isTuttoBloccato = false;
			}
		}

		void svuotareListaDaModificare() {

			// Prima di svuotare la lista, voglio provare a liberare un pò di memoria che forse è inutile.
			foreach( Fotografia foto in fotografieDaModificare )
				AiutanteFoto.disposeImmagini( foto, IdrataTarget.Originale );

			fotografieDaModificare.Clear();
			resetEffettiAndTrasformazioni();

			// Pubblico un messaggio di richiesta cambio pagina. Voglio tornare sulla gallery
			CambioPaginaMsg cambioPaginaMsg = new CambioPaginaMsg( this );
			cambioPaginaMsg.nuovaPag = "GalleryPag";
			LumenApplication.Instance.bus.Publish( cambioPaginaMsg );

		}

		void riempireElencoInModifica() {
			fotografieDaModificareCW.SelectAllMax();
			resetEffettiAndTrasformazioni();
		}

		void svuotareElencoInModifica() {
			svuotareElencoInModifica( true );
		}

		void svuotareElencoInModifica( bool ancheResetEffetti ) {
			fotografieDaModificareCW.DeselectAll();
			if( ancheResetEffetti )
				resetEffettiAndTrasformazioni();
		}

		private void browseForFileCornice() {

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
					maschere.Add( bmp );
				} catch( Exception ee ) {
					_giornale.Warn( "Errore in caricamento maschera : " + dlg.FileName, ee );
					dialogProvider.ShowError( ee.Message, "Imposssibile caricare cornice", null );
				}
			}
		}

		private void maschereCrop(String nomeMskCrop)
		{
			string nomeMaschera = Path.Combine(Configurazione.UserConfigLumen.cartellaMaschereCrop, nomeMskCrop);

			//			Uri uriMask = ((BitmapImage)p).UriSource;
			BitmapImage msk = new BitmapImage(new Uri(nomeMaschera));
			mascheraAttiva = msk;

			svuotareElencoInModifica(false);

			// cambio stato. Vado in modalità di editing maschere
			modalitaEdit = ModalitaEdit.GestioneMaschere;


			// Mi serve per accendere i pulsanti di rifiuta e salva
			forseInizioModifiche();
		}

		private void riposizionaControlliFotoritoccoPuntuale() {

			bool puntuale = true;

			// Lavoro solo se ho una sola singola foto selezionata
			if( puntuale && fotografieDaModificareCW.SelectedItems.Count != 1 ) {
				puntuale = false;
			}

/*
			// Lavoro solo se ho qualche correzione. Se la foto non è stata ritoccata, allora non devo fare nulla.
			if( puntuale && fotografieDaModificareCW.SelectedItems [0].correzioniXml == null) {
				puntuale = false;
			}
*/

			// resetto tutti gli effetti e trasformazioni precedenti per resettare i controlli ui.
			resetEffettiAndTrasformazioni();

			if( puntuale ) {

				// Quando lavoro puntualmente su di una foto, ho bisogno dell'immagine originale (TODO sarebbe più veloce avere un provino originale)
				AiutanteFoto.idrataImmaginiFoto( fotografieDaModificareCW.SelectedItems[0], IdrataTarget.Originale );

				if( fotografieDaModificareCW.SelectedItems[0].correzioniXml != null ) {

					// carico Effetti precedenti
					IList<ShaderEffectBase> carEffetti = fotoRitoccoSrv.converteCorrezioni<ShaderEffectBase>( fotografieDaModificareCW.SelectedItems[0] );
					effetti.AddRange( carEffetti );

					// carico Trasformazioni precedenti
					IList<Transform> carTrasformazioni = fotoRitoccoSrv.converteCorrezioni<Transform>( fotografieDaModificareCW.SelectedItems[0] );
					trasformazioni.AddRange( carTrasformazioni );
				}

			} 

			// Pubblico un messaggio per indicare che ci sono degli effetti cambiati.
			// Tramite questo messaggio, la UI può re-bindare i controlli interessati
			RitoccoPuntualeMsg ritoccoPuntualeMsg = new RitoccoPuntualeMsg( this );
			ritoccoPuntualeMsg.senderTag = puntuale;
			LumenApplication.Instance.bus.Publish( ritoccoPuntualeMsg );
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

			if (nuovaFotoMsg.descrizione.Contains(Configurazione.ID_FOTOGRAFO_ARTISTA) || AiutanteFoto.isMaschera(nuovaFotoMsg.foto))
			{
				// E' stata memorizzata una nuova fotografia che in realtà è una cornice
				addFotoDaModificare( nuovaFotoMsg.foto );

				// Visto che l'immagine del provino viene caricata in un altro thread, qui non sono in grado di visualizzarla. La devo rileggere per forza.
				// Questo mi consente di visualizzare il provino come primo elemento 
				AiutanteFoto.idrataImmaginiFoto( nuovaFotoMsg.foto, IdrataTarget.Provino, true );
			}
		}

		private void gestisciFotoDaModificareMsg( FotoDaModificareMsg fotoDaModificareMsg ) {
			
			// Ecco che sono arrivate delle nuove foto da modificare
			// Devo aggiungerle alla lista delle foto in attesa di modifica.
			foreach( Fotografia f in fotoDaModificareMsg.fotosDaModificare )
				addFotoDaModificare( f );
			
			// Se richiesta la modifica immediata...
			if( fotoDaModificareMsg.immediata ) {
				// ... e sono in modalità di fotoritocco
				if( this.modalitaEdit == ModalitaEdit.FotoRitoccoPuntuale ) {
					// ... e non ho nessuna altra modifica in corso ...
					if( modificheInCorso == false ) {
						//fotografieDaModificareCW.SelectedItems.Clear();
						foreach( Fotografia f in fotoDaModificareMsg.fotosDaModificare )
						{
							// Verifico se ho raggiunto il numero massimo di foto da modificare
							if( Configurazione.UserConfigLumen.maxNumFotoMod > 0 ) {
								if (fotografieDaModificareCW.SelectedItems.Count >= Configurazione.UserConfigLumen.maxNumFotoMod)
								{
									_giornale.Debug( "Raggiunto il limite massimo di foto " + Configurazione.UserConfigLumen.maxNumFotoMod + " foto modificabili Contemporaneamente" );
									dialogProvider.ShowMessage( "Hai raggiunto il numero massimo di foto modificabili di " + Configurazione.UserConfigLumen.maxNumFotoMod + " foto\nLe foto in eccesso verranno aggiunte ma no selezionate", "AVVISO" );
									break;
								}
							 }
							fotografieDaModificareCW.SelectedItems.Add( f );
						}

						// ERROR32
						fotografieDaModificareCW.RefreshSelectedItemWithMemory();
					}
				}

				// Pubblico un messaggio di richiesta cambio pagina. Voglio andare sulla pagina del fotoritocco
				CambioPaginaMsg cambioPaginaMsg = new CambioPaginaMsg( this );
				cambioPaginaMsg.nuovaPag = "FotoRitoccoPag";
				LumenApplication.Instance.bus.Publish( cambioPaginaMsg );
			}

			forzaRefreshStato();
		}

		void onFotografieDaModificareSelectionChanged( object sender, SelectionChangedEventArgs e ) {

			// Riposiziono i controlli in modo da velocizzare il tutto.
			riposizionaControlliFotoritoccoPuntuale();
			

			// Provoco la rilettura delle property che determinano lo stato dei pulsanti.
			forzaRefreshStato();
		}

		#endregion Gestori Eventi



	}
}
