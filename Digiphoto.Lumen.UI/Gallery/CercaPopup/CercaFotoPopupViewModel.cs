﻿using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Digiphoto.Lumen.UI.Identifica;

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

	public enum FiltroDidascalia {

			SoloPiene,
			SoloVuote,
			Impronta
	}

	public class CercaFotoPopupViewModel : ClosableWiewModel {

		#region Costruttori

		public CercaFotoPopupViewModel() : this( ModoRicercaPop.PosizionaPaginaDaNumero ) {
		}

		public CercaFotoPopupViewModel( ModoRicercaPop modoDefault ) {
			// Imposto la modalità di ricerca di default, oppure quella indicata dall'utente.
			this.modoRicercaPop = modoDefault;
			this.confermata = false;


			identificatoreImprontaViewModel = new IdentificatoreImprontaViewModel();
			identificatoreImprontaViewModel.PropertyChanged += IdentificatoreImprontaViewModel_PropertyChanged;
		}

		private void IdentificatoreImprontaViewModel_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {
			// sento quando cambia il nome corrispondente alla impronta.
			// Quando assume un valore valido, chiudo la ricerca.
			if( e.PropertyName == "nomeIdentificato" ) {
				if( identificatoreImprontaViewModel.nomeIdentificato != null )
					this.filtroDidascalia = FiltroDidascalia.Impronta;
			}
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

		public IdentificatoreImprontaViewModel identificatoreImprontaViewModel {
			get;
			private set;
		}

		public Nullable<FiltroDidascalia> filtroDidascalia {
			set;
			get;
		}

		public UserConfigLumen userConfig {
			get {
				return Configurazione.UserConfigLumen;
			}
		}

		#endregion Proprietà

		#region Metodi

		public bool possoConfermare { 
			get {
				return numeroFotogramma > 0 && numeroFotogramma < Int32.MaxValue;
			}
		}

		void confermare() {
			confermata = true;
			CloseCommand.Execute( null );
		}

		void setFiltroDidascalia( string quale ) {
			this.filtroDidascalia = (FiltroDidascalia) Enum.Parse( typeof( FiltroDidascalia ), quale );
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

		private RelayCommand _setFiltroDidascaliaCommand;
		public ICommand setFiltroDidascaliaCommand {
			get {
				if( _setFiltroDidascaliaCommand == null ) {
					_setFiltroDidascaliaCommand = new RelayCommand( quale => setFiltroDidascalia( (string)quale ),
					                                                quale => true );
				}
				return _setFiltroDidascaliaCommand;
			}
		}

		protected override void OnRequestClose() {
			base.OnRequestClose();
		}

		protected override void OnDispose() {

			if( this.identificatoreImprontaViewModel != null ) {
				identificatoreImprontaViewModel.PropertyChanged -= IdentificatoreImprontaViewModel_PropertyChanged;
				this.identificatoreImprontaViewModel.Dispose();
			}
				
		}

		#endregion Comandi

	}
}
