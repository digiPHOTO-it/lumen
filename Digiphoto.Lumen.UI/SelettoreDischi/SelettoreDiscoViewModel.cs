using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.UI.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.UI.SelettoreDischi {

	public class SelettoreDiscoViewModel : ViewModelBase, ISelettore<DriveInfo>, IObserver<VolumeCambiatoMsg> {

		public SelettoreDiscoViewModel() {
			IObservable<VolumeCambiatoMsg> observable = LumenApplication.Instance.bus.Observe<VolumeCambiatoMsg>();
			observable.Subscribe( this );

			caricareDischiRimovibili();
		}

		public SelettoreDiscoViewModel( string defaultMasterizzatore ) : this() {

			if( defaultMasterizzatore != null )  // potrebbe non essere stato configurato.x
				for( int ii = 0; ii < dischi.Count; ii++ ) {
					if( dischi[ii].Name.StartsWith( defaultMasterizzatore ) ) {
						discoSelezionato = dischi [ii];
					}
				}

		}

		#region Metodi

		// private static object _lock = new object();

		/// <summary>
		// Popolo la collezione con i dischi che trovo in questo momento sul PC
		/// </summary>
		/// <param name="verso"></param>
		public void caricareDischiRimovibili() {

			this.dischi = load();
			// BindingOperations.EnableCollectionSynchronization( this.dischi, _lock );
		
			dischiCW = new ListCollectionView( dischi );
			OnPropertyChanged( "dischiCW" );
		}

		private ObservableCollectionEx<DriveInfo> load() {

			var lista = System.IO.DriveInfo.GetDrives().Where( d => d.DriveType == DriveType.CDRom || d.DriveType == DriveType.Removable );

			return new ObservableCollectionEx<DriveInfo>( lista.ToList() );
		}

		public void deselezionareTutto() {
			discoSelezionato = null;
		}

		public void deselezionareSingola( DriveInfo elem ) {
			if( elem.Equals( discoSelezionato ) )
				discoSelezionato = null;
		}

		public IEnumerator<DriveInfo> getEnumeratorElementiTutti() {
			return dischi.GetEnumerator();
		}

		public IEnumerable<DriveInfo> getElementiTutti() {
			return dischi;
		}

		public IEnumerator<DriveInfo> getEnumeratorElementiSelezionati() {
			throw new NotImplementedException();
		}

		public IEnumerable<DriveInfo> getElementiSelezionati() {
			throw new NotImplementedException();
		}

		#endregion Metodi


		#region Proprietà

		public ListCollectionView dischiCW {
			private set;
			get;
		}

		private IVolumeCambiatoSrv volumeCambiatoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IVolumeCambiatoSrv>();
			}
		}

		private IGestoreImmagineSrv gestoreImmagineSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			}
		}

		private ObservableCollectionEx<DriveInfo> dischi {
			get;
			set;
		}

		private DriveInfo _discoSelezionato;
		public DriveInfo discoSelezionato {
			get {
				return _discoSelezionato;
			}
			set {
				if( _discoSelezionato != value ) {
					_discoSelezionato = value;
					OnPropertyChanged( "discoSelezionato" );

					raiseSelezioneCambiataEvent();
				}
			}
		}

		public int countElementiTotali {
			get {
				return dischiCW == null ? 0 : dischiCW.Count;
			}
		}

		public int countElementiSelezionati {
			get {
				return discoSelezionato == null ? 0 : 1;
			}
		}

		public bool isAlmenoUnElementoSelezionato {
			get {
				return countElementiSelezionati > 0;
			}
		}

		#endregion Proprietà


		#region Comandi

		private RelayCommand _caricareDischiRimovibiliCommand;
		

		public ICommand caricareDischiRimovibiliCommand
		{
			get
			{
				if( _caricareDischiRimovibiliCommand == null ) {
					_caricareDischiRimovibiliCommand = new RelayCommand( param => caricareDischiRimovibili(),
																			param => true );
				}
				return _caricareDischiRimovibiliCommand;
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

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( VolumeCambiatoMsg value ) {
			aggiungiTogliNomeVolume( value.montato, value.nomeVolume );
		}

		private void aggiungiTogliNomeVolume( bool aggiungi, string driveName ) {
		
			if( aggiungi ) {
				
				// E' stato aggiunto. lo cerco nella lista di tutti i drive
				DriveInfo [] drives = DriveInfo.GetDrives();
				for( int i = 0; i < drives.Count(); i++ ) {
					if( drives[i].Name.StartsWith( driveName ) ) {

						// Controllo che non ci sia già. E' capitato!
						bool esisteGia = false;
						for( int bb = 0; bb < dischi.Count; bb++ )
							if( dischi[bb].Name.StartsWith( driveName ) )  // uno è con la barra, l'altro senza barra
								esisteGia = true;

						if( ! esisteGia )
							dischi.Add( drives [i] );
						
						break;
					}
				}

			} else {

				// E' stato smontato. lo rimuovo
				for( int i = 0; i < dischi.Count; i++ ) {
					if( dischi[i].Name.StartsWith( driveName ) ) {  // uno è con la barra, l'altro senza barra
						dischi.RemoveAt( i );
						break;
					}
				}
			}

			// OnPropertyChanged( "dischiCW" );
			
		}

		#endregion Eventi

	}
}
