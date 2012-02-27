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

		#endregion

		#region Comandi

		private RelayCommand _grayScaleCommand;
		public ICommand grayScaleCommand {
			get {
				if( _grayScaleCommand == null ) {
					_grayScaleCommand = new RelayCommand( param => this.grayScale(),
														param => possoApplicareComando,
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
														sGradi => this.possoApplicareComando,
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
														gradi => this.possoApplicareComando,
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
													  param => this.possoApplicareComando,
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
													  param => this.possoApplicareComando,
													  true );
				}
				return _flipCommand;
			}
		}

		public bool possoApplicareComando {
			get {
				return isAlmenoUnaFotoSelezionata;
			}
		}
		#endregion

		#region Metodi

		private void ruotare( int pGradi ) {

			RuotaCorrezione correzione = new RuotaCorrezione() { gradi = pGradi };			
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, correzione );
		}


		private void grayScale() {
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, new BiancoNeroCorrezione() );
		}

		private void tornareOriginale() {
			fotoRitoccoSrv.tornaOriginale( Target.Selezionate );			
		}

		private void sepia() {
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, new SepiaCorrezione() );
		}

		private void flip() {
			fotoRitoccoSrv.addCorrezione( Target.Selezionate, new SpecchioCorrezione() );
		}

		#endregion

	}
}
