using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.UI.SelettoreMaschera {

	public class SelettoreMascheraViewModel : ViewModelBase, ISelettore<Digiphoto.Lumen.Model.Maschera> {

		public SelettoreMascheraViewModel() {
		}

		#region Metodi

		/// <summary>
		///  verso:   S = Maschere singole
		///           M = Maschere multiple (Composizione)
		///           N = nessuna (svuota)
		/// </summary>
		/// <param name="verso"></param>
		public void caricareMaschere( string verso ) {

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



		/// <summary>
		/// Carico la collezione con le maschere
		/// </summary>
		void loadMaschereDaDisco( FiltroMask tipoMask ) {

			if( tipoMask == FiltroMask.MskSingole ) {
				if( maschereSingole == null )
					maschereSingole = caricaMaschere( tipoMask );
			} else {
				if( maschereMultiple == null )
					maschereMultiple = caricaMaschere( tipoMask );
			}
		}

		private ObservableCollection<Maschera> caricaMaschere( FiltroMask filtro ) {

			List<Maschera> maschere = fotoRitoccoSrv.caricaListaMaschere( filtro );
			if( maschere != null ) {
				foreach( var msk in maschere ) {
					try {
						// idrato solo l'immaginetta del provino
						gestoreImmagineSrv.idrataMaschera( msk, false );
					} catch( Exception ee ) {
						_giornale.Error( "Maschera non caricata", ee );
					}
				}
			}

			return new ObservableCollection<Maschera>( maschere );
		}



		public bool possoCaricareMaschere( string param ) {

			if( param == "V" || param == "H" )
				return maschereCW != null && maschereCW.IsEmpty == false;
			else
				return true;  // T=tutto ; N=Niente
		}

		private void sfogliarePerFile() {

			throw new NotImplementedException( "funzionalità da sistemare" );
#if false
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Devo aggiungere gli asterischi
			string testo = Configurazione.UserConfigLumen.estensioniGrafiche;
			StringBuilder extWithStar = new StringBuilder();
			foreach( string ext in Configurazione.estensioniGraficheAmmesse ) {
				extWithStar.Append( '*' );
				extWithStar.Append( ext );
				extWithStar.Append( ';' );
			}

			extWithStar.Remove( extWithStar.Length - 1, 1 );  // tolgo l'ultimo punto e virgola

			dlg.DefaultExt = ".png";
			dlg.Filter = "Images |" + extWithStar;

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if( result == true ) {

				try {
					// Carico la immagine e la aggiungo alla lista che sta a video
					var m = gestoreImmagineSrv.caricaMaschera( dlg.FileName, FiltroMask.MskMultiple, true );
					maschereMultiple.Add( m );
				} catch( Exception ee ) {
					_giornale.Warn( "Errore in caricamento maschera : " + dlg.FileName, ee );
					dialogProvider.ShowError( ee.Message, "Imposssibile caricare cornice", null );
				}
			}
#endif
		}

		public void deselezionareTutto() {
			mascheraSelezionata = null;
		}

		public void deselezionareSingola( Maschera elem ) {
			if( elem.Equals( mascheraSelezionata ) )
				mascheraSelezionata = null;
		}

		public IEnumerator<Maschera> getEnumeratorElementiTutti() {
			return filtro == FiltroMask.MskSingole ? maschereSingole.GetEnumerator() : maschereMultiple.GetEnumerator();
		}

		public IEnumerable<Maschera> getElementiTutti() {
			return filtro == FiltroMask.MskSingole ? maschereSingole : maschereMultiple;
		}

		public IEnumerator<Maschera> getEnumeratorElementiSelezionati() {
			throw new NotImplementedException();
		}

		public IEnumerable<Maschera> getElementiSelezionati() {
			throw new NotImplementedException();
		}

		#endregion Metodi


		#region Proprietà

		public ListCollectionView maschereCW {
			private set;
			get;
		}

		private IFotoRitoccoSrv fotoRitoccoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}

		private IGestoreImmagineSrv gestoreImmagineSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			}
		}

		private ObservableCollection<Maschera> maschereSingole {
			get;
			set;
		}

		private ObservableCollection<Maschera> maschereMultiple {
			get;
			set;
		}

		private FiltroMask _filtro;
		public FiltroMask filtro {
			get {
				return _filtro;
			}
			set {
				if( _filtro != value ) {
					_filtro = value;
					OnPropertyChanged( "filtro" );
				}
			}
		}

		/// <summary>
		/// Se sono in gestione maschere, allora permetto di leggere da disco
		/// Se sono in gestione cornici , allora devo scegliere tra quelle presenti
		/// </summary>
		public bool possoSfogliarePerFile {
			get {
				return filtro == FiltroMask.MskMultiple;
			}
		}

		private Maschera _mascheraSelezionata;
		public Maschera mascheraSelezionata {
			get {
				return _mascheraSelezionata;
			}
			set {
				if( _mascheraSelezionata != value ) {
					_mascheraSelezionata = value;
					OnPropertyChanged( "mascheraSelezionata" );

					raiseSelezioneCambiataEvent();
				}
			}
		}

		public int countElementiTotali {
			get {
				return maschereCW == null ? 0 : maschereCW.Count;
			}
		}

		public int countElementiSelezionati {
			get {
				return mascheraSelezionata == null ? 0 : 1;
			}
		}

		public bool isAlmenoUnElementoSelezionato {
			get {
				return countElementiSelezionati > 0;
			}
		}

		#endregion Proprietà


		#region Comandi

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

		private RelayCommand _sfogliarePerFileCommand;
		public ICommand sfogliarePerFileCommand
		{
			get
			{
				if( _sfogliarePerFileCommand == null ) {
					_sfogliarePerFileCommand = new RelayCommand( p => sfogliarePerFile(),
																		p => possoSfogliarePerFile );
				}
				return _sfogliarePerFileCommand;
			}
		}

		#endregion Comandi


		#region Eventi
		
		public event SelezioneCambiataEventHandler selezioneCambiata;


		/// <summary>
		///   Avviso eventuali ascoltatori esterni
		/// </summary>
		private void raiseSelezioneCambiataEvent() {

			if( selezioneCambiata != null )
				selezioneCambiata( this, EventArgs.Empty );
		}

		#endregion Eventi

	}
}
