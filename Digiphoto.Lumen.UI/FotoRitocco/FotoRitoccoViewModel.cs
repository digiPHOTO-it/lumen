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

namespace Digiphoto.Lumen.UI {


	public class FotoRitoccoViewModel : ViewModelBase {

		public FotoRitoccoViewModel() {

			if( IsInDesignMode ) {
				// caricare qualche foto a casaccio
			} else {
				
				fotografieDaModificareCW = CollectionViewSource.GetDefaultView( fotoRitoccoSrv.fotografieDaModificare );
//				fotografieDaModificareCW = new CollectionView( fotoRitoccoSrv.fotografieDaModificare );

			}
		}

		#region Proprietà

		public ICollectionView fotografieDaModificareCW {
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
				int quanti = 0;
				if( fotografieDaModificareCW != null )
					quanti = fotografieDaModificareCW.Cast<Fotografia>().Where( f => f.isSelezionata == true ).Count();
				return quanti;
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

		#endregion   // Proprietà

		#region Comandi

		private RelayCommand _grayScaleCommand;
		public ICommand grayScaleCommand {
			get {
				if( _grayScaleCommand == null ) {
					_grayScaleCommand = new RelayCommand( param => this.grayScale(),
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
					_sepiaCommand = new RelayCommand( param => this.sepia(),
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


		private void ruotare( int pGradi ) {
			forseInizioModifiche();
			RuotaCorrezione correzione = new RuotaCorrezione() { gradi = pGradi };			
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, correzione );
		}

		private void grayScale() {
			forseInizioModifiche();
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, new BiancoNeroCorrezione() );
		}

		private void tornareOriginale() {
			forseInizioModifiche();
			fotoRitoccoSrv.tornaOriginale( Target.Selezionate );			
		}

		private void sepia() {
			forseInizioModifiche();
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, new SepiaCorrezione() );
		}

		private void flip() {
			forseInizioModifiche();
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, new SpecchioCorrezione() );
		}

		private void salvareCorrezioni() {
			fotoRitoccoSrv.salvaCorrezioniTransienti( Target.Selezionate );
			modificheInCorso = false;
		}

		private void rifiutareCorrezioni() {
			fotoRitoccoSrv.undoCorrezioniTransienti( Target.Selezionate );
			modificheInCorso = false;
		}

		private void salvareMetadati() {
			// TODO
		}

		#endregion Metodi



	}
}
