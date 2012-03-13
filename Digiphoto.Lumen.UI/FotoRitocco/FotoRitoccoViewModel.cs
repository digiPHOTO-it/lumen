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
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Windows.Media.Effects.LuminositaContrasto;
using Digiphoto.Lumen.Windows.Media.Effects.Sepia;
using Digiphoto.Lumen.Windows.Media.Effects;

namespace Digiphoto.Lumen.UI {


	public class FotoRitoccoViewModel : ViewModelBase {

		public FotoRitoccoViewModel() {

			if( IsInDesignMode ) {
				// caricare qualche foto a casaccio
			} else {
				fotografieDaModificareCW = new MultiSelectCollectionView<Fotografia>( fotoRitoccoSrv.fotografieDaModificare );
			}

			resetEffetti();
		}

		#region Proprietà

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

		#endregion   // Proprietà

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
													  param => this.possoApplicareCorrezione,
													  true );
				}
				return _sepiaCommand;
			}
		}

		private RelayCommand _flipCommand;
		public ICommand flipCommand {
			get {
				if( _flipCommand == null ) {
					_flipCommand = new RelayCommand( param => this.flip(),
													  param => this.possoApplicareCorrezione,
													  true );
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



		#endregion Comandi

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
			addCorrezione( new RuotaCorrezione() { gradi = pGradi } );
		}

		private void grayScale( bool addRemove ) {
			if( addRemove )
				addCorrezione( new BiancoNeroCorrezione() );
			else
				removeCorrezione( typeof( BiancoNeroCorrezione ) );
		}

		private void tornareOriginale() {
			resetEffetti();
			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.tornaOriginale( f );			
		}

		private void sepia( bool addRemove ) {
			if( addRemove )
				addCorrezione( new SepiaCorrezione() );
			else
				removeCorrezione( typeof( SepiaCorrezione ) );
		}

		private void removeCorrezione( Type type ) {
						
			forseInizioModifiche();

			foreach( Fotografia f in fotografieDaModificareCW.SelectedItems )
				fotoRitoccoSrv.removeCorrezione( f, type );
		}

		private void flip() {
			addCorrezione( new SpecchioCorrezione() );
		}

		private void salvareCorrezioni() {

			
			// Purtoppo gli shader effects sono gestiti a parte
			// Vado ad aggiungerli solo al momento di applicare per davvero
			foreach( ShaderEffectBase effetto in effetti )
				addCorrezione( convertiInCorrezione( effetto ) );

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
			}
		}

		private Correzione convertiInCorrezione( ShaderEffectBase effetto ) {

			Correzione ret = null;

			if( effetto is LuminositaContrastoEffect ) {
				ret = new LuminositaContrastoCorrezione {
					luminosita = ((LuminositaContrastoEffect)effetto).Brightness,
					contrasto = ((LuminositaContrastoEffect)effetto).Contrast
				};

			}

			return ret;
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



		#endregion Metodi



	}
}
