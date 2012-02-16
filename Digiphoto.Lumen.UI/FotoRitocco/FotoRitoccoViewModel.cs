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

namespace Digiphoto.Lumen.UI {

	public class FotoRitoccoViewModel : ViewModelBase {


		public FotoRitoccoViewModel() {

			if( IsInDesignMode ) {
				// caricare qualche foto a casaccio
			} else {
				
				fotografieDaModificareCW = (ListCollectionView)CollectionViewSource.GetDefaultView( fotoRitoccoSrv.fotografieDaModificare );

				fotografieDaModificareCW.CurrentChanged += delegate {
					fotoSelezionata = (Fotografia)fotografieDaModificareCW.CurrentItem;
				};

			}

		}

		private Fotografia _fotoSelezionata;
		public Fotografia fotoSelezionata {
			get {
				return _fotoSelezionata;
			}
			set {
				if( _fotoSelezionata != value ) {
					_fotoSelezionata = value;
					OnPropertyChanged( "fotoSelezionata" );
				}
			}
		}

		public ListCollectionView fotografieDaModificareCW {
			get;
			set;
		}

		public IFotoRitoccoSrv fotoRitoccoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}


		public bool isFotoSelezionata {
			get {
				return fotoSelezionata != null;
			}
		}

		#region Comandi

		private RelayCommand _grayScaleCommand;
		public ICommand grayScaleCommand {
			get {
				if( _grayScaleCommand == null ) {
					_grayScaleCommand = new RelayCommand( param => this.grayScale(),
														param => this.isFotoSelezionata,
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
														sGradi => this.isFotoSelezionata,
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
														gradi => this.isFotoSelezionata,
														true );
				}
				return _tornareOriginaleCommand;
			}
		}

		#endregion


		#region Metodi

		private void ruotare( int pGradi ) {

			RuotaCorrezione correzione = new RuotaCorrezione() { gradi = pGradi };
			
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, correzione );

			OnPropertyChanged( "fotoSelezionata" );
		}


		private void grayScale() {

			fotoRitoccoSrv.addCorrezione( Target.Selezionate, new BiancoNeroCorrezione() );

			OnPropertyChanged( "fotoSelezionata" ); // TODO questo evento dovrebbe sollevarlo il servizio per informare tutta l'applicazione
		}

		private void tornareOriginale() {

			fotoRitoccoSrv.tornaOriginale( Target.Selezionate ); 
			
			OnPropertyChanged( "fotoSelezionata" );   // TODO questo evento dovrebbe sollevarlo il servizio per informare tutta l'applicazione
		}

		#endregion

	}
}
