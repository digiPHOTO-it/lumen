using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Diapo;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Servizi.Vendere;

namespace Digiphoto.Lumen.UI {

	public class FotoGalleryViewModel : ViewModelBase {


		public FotoGalleryViewModel() {

			paramCercaFoto = new ParamCercaFoto();

			if( IsInDesignMode ) {

			} else {

				paramCercaFoto.giornataIniz = new DateTime( 2012, 1, 20 );

				// Faccio una ricerca a vuoto
				fotoExplorerSrv.cercaFoto( paramCercaFoto );
				/*
							var query = from f in fotoExplorerSrv.fotografie
										select new DiapositivaViewModel( f );
							diapositiveViewModel = query.ToList<DiapositivaViewModel>();
				 */

	//			ObservableCollection<Fotografia> appo = new ObservableCollection<Fotografia>( fotoExplorerSrv.fotografie );

				this.fotografieCW = CollectionViewSource.GetDefaultView( fotoExplorerSrv.fotografie );
				deselezionareTutto();
			} 
		}


		#region Proprietà

		public ICollectionView fotografieCW {
			get;
			set;
		}

		public ParamCercaFoto paramCercaFoto {
			get;
			set;
		}

		IFotoExplorerSrv fotoExplorerSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}
		
		int contaSelez {
			get {
				int quanti = 0;
				if( fotografieCW != null )
					quanti = fotografieCW.Cast<Fotografia>().Where( f => f.isSelezionata == true ).Count();
				return quanti;
			}
		}

		public bool possoAggiungereAlMasterizzatore {
			get {
				return contaSelez > 0;
			}
		}
		#endregion

		#region Comandi

		private RelayCommand _deselezionareTuttoCommand;
		public ICommand deselezionareTuttoCommand {
			get {
				if( _deselezionareTuttoCommand == null ) {
					_deselezionareTuttoCommand = new RelayCommand( param => deselezionareTutto() );
				}
				return _deselezionareTuttoCommand;
			}
		}

		private RelayCommand _aggiungereAlMasterizzatoreCommand;
		public ICommand aggiungereAlMasterizzatoreCommand {
			get {
				if( _aggiungereAlMasterizzatoreCommand == null ) {
					_aggiungereAlMasterizzatoreCommand = new RelayCommand( param => aggiungereAlMasterizzatore()
				                                                         //  ,param => possoAggiungereAlMasterizzatore 
																		   );
				}
				return _aggiungereAlMasterizzatoreCommand;
			}
		}

		private RelayCommand _filtrareSelezionateCommand;
		public ICommand filtrareSelezionateCommand {
			get {
				if( _filtrareSelezionateCommand == null ) {
					_filtrareSelezionateCommand = new RelayCommand( param => filtrareSelezionate( Convert.ToBoolean(param) ) );
				}
				return _filtrareSelezionateCommand;
			}
		}

		private void filtrareSelezionate( bool attivareFiltro ) {

			// Alcune collezioni non sono filtrabili, per esempio la IEnumerable
			if( fotografieCW.CanFilter == false )
				return;

			if( attivareFiltro ) {

				// Creo un oggetto Predicate al volo.
				fotografieCW.Filter = obj => {
					Fotografia ff = (Fotografia)obj;
					return ff.isSelezionata;
				};

			} else {
				fotografieCW.Filter = null;
			}
		}

		/// <summary>
		/// Aggiungo le immagini selezionate al masterizzatore
		/// </summary>
		/// <returns></returns>
		public void aggiungereAlMasterizzatore() {

			IEnumerable<Fotografia> listaSelez = creaListaFotoSelezionate();
			IVenditoreSrv srv = LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
			srv.aggiungiMasterizzate( listaSelez );
		}

		private IEnumerable<Fotografia> creaListaFotoSelezionate() {

			var fotos = fotografieCW.OfType<Fotografia>().Where( f => f.isSelezionata == true );

/*
			IList<Fotografia> fotosSelez = new List<Fotografia>();
			foreach( Fotografia f in fotografieCW )
				if( f.isSelezionata )
					fotosSelez.Add( f );
 */
			return fotos;
		}
			

		#endregion


		#region Metodi

		/// <summary>
		/// Spengo tutte le selezioni
		/// </summary>
		private void deselezionareTutto() {

			int quanti = contaSelez;

			foreach( Fotografia f in fotografieCW )
				f.isSelezionata = false;
		}


		#endregion



	}
}
