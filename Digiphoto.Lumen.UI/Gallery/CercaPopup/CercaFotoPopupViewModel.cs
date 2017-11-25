using Digiphoto.Lumen.UI.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI.Gallery {
	
	public enum ModoRicercaPop {
		/// <summary>
		/// I filtri di ricerca rimangono inalterati.
		/// Mi sposto solamente sulla pagina in cui trovo il fotogramma indicato
		/// </summary>
		PosizionaPaginaDaNumero,

		/// <summary>
		/// Vengono svuotati i filtri di ricerca, e viene effettuata una ricerca con il numero del 
		/// fotogramma indicato con intorno altre foto prima e dopo
		/// </summary>
		RicercaNumeroConIntorno,

		/// <summary>
		/// Esattamente come con il numero, ma prima si cerca la prima didascalia
		/// </summary>
		RicercaDidascaliaConIntorno
	}

	public class CercaFotoPopupViewModel : ViewModelBase {

		#region Costruttori

		public CercaFotoPopupViewModel() : this( ModoRicercaPop.PosizionaPaginaDaNumero ) {
		}

		public CercaFotoPopupViewModel( ModoRicercaPop modoDefault ) {
			// Imposto la modalità di ricerca di default, oppure quella indicata dall'utente.
			this.modoRicercaPop = modoDefault;
			this.confermata = false;
		}
		#endregion Costruttori

		#region Proprietà
		
		private int _numeroFotogramma;
		public int numeroFotogramma {
			get {
				return _numeroFotogramma;
			}

			set {
				if( _numeroFotogramma != value ) {
					_numeroFotogramma = value;
					OnPropertyChanged( "numeroFotogramma" );
				}
			}
		}

		private bool _possoRicercareLaPagina;
		public bool possoRicercareLaPagina {
			get {
				return _possoRicercareLaPagina;
			}

			set {
				if( _possoRicercareLaPagina != value ) {
					_possoRicercareLaPagina = value;
					OnPropertyChanged( "possoRicercareLaPagina" );
				}
			}
		}

		

		public ModoRicercaPop _modoRicercaPop;
		public ModoRicercaPop modoRicercaPop {

			get {
				return _modoRicercaPop;
			}

			set {
				if( _modoRicercaPop != value ) {
					_modoRicercaPop = value;
					OnPropertyChanged( "modoRicercaPop" );
				}
			}
		}

		public bool confermata { get; set; }

		#endregion Proprietà

		#region Metodi

		public bool possoConfermare { 
			get {
				return numeroFotogramma > 0 && numeroFotogramma < Int32.MaxValue;
			}
		}

		void confermare() {
			confermata = true;
		}
		
		#endregion Metodi

		#region Comandi

		private RelayCommand _confermareCommand;
		public ICommand confermareCommand
		{
			get
			{
				if( _confermareCommand == null ) {
					_confermareCommand = new RelayCommand( p => confermare(),
					                                       p => possoConfermare );
				}
				return _confermareCommand;
			}
		}

		#endregion Comandi

	}
}
