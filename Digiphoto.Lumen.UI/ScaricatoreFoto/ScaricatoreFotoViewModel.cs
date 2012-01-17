using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Applicazione;
using System.Windows.Input;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core;

namespace Digiphoto.Lumen.UI {


	public class ScaricatoreFotoViewModel : ViewModelBase {


		public ScaricatoreFotoViewModel() {

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoViewModel = new SelettoreEventoViewModel();

			selettoreFotografoViewModel = new SelettoreFotografoViewModel();

			selettoreCartellaViewModel = new SelettoreCartellaViewModel();	
		}

		#region Proprietà

		public SelettoreEventoViewModel selettoreEventoViewModel {
			get;
			private set;
		}

		public SelettoreFotografoViewModel selettoreFotografoViewModel {
			get;
			private set;
		}

		public SelettoreCartellaViewModel selettoreCartellaViewModel {
			get;
			private set;
		}

		public FaseDelGiorno [] fasiDelGiorno {
			get {
				return FaseDelGiornoUtil.fasiDelGiorno;
			}
		}

		public FaseDelGiorno faseDelGiorno {
			get;
			set;
		}

		public Fotografo fotografo {
			get {
				return selettoreFotografoViewModel.fotografoSelezionato;
			}
		}

		public string cartellaSorgente {
			get {
				return selettoreCartellaViewModel.cartellaSelezionata;
			}
		}

		public IScaricatoreFotoSrv scaricatoreFotoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IScaricatoreFotoSrv>();
			}
		}

		/// <summary>
		///  Se vero, quando scarico lascio le foto, le elimino dalla cartella (in pratica le sposto)
		/// </summary>
		public bool eliminaFileSorgenti {
			get;
			set;
		}

		#endregion

		#region Comandi

		private RelayCommand _scaricareCommand;
		public ICommand scaricareCommand {
			get {
				if( _scaricareCommand == null ) {
					_scaricareCommand = new RelayCommand( param => this.scaricare(),
														param => this.possoScaricare,
														true );
				}
				return _scaricareCommand;
			}
		}

		private RelayCommand _setSpostaCopiaCommand;
		public ICommand setSpostaCopiaCommand {
			get {
				if( _setSpostaCopiaCommand == null ) {
					_setSpostaCopiaCommand = new RelayCommand( param => eliminaFileSorgenti = Convert.ToBoolean(param) );
				}
				return _scaricareCommand;
			}
		}

		#endregion

		#region Metodi
		private void scaricare() {
			
			Console.WriteLine( "STOP" );

			ParamScarica paramScarica = new ParamScarica();

			// Cartella sorgente da cui scaricare
			paramScarica.cartellaSorgente = cartellaSorgente;

			// Fotografo a cui attribuire le foto
			if( fotografo != null )
				paramScarica.flashCardConfig.idFotografo = fotografo.id;

			// Evento
			if( selettoreEventoViewModel.eventoSelezionato != null )
				paramScarica.flashCardConfig.idEvento = selettoreEventoViewModel.eventoSelezionato.id;

			paramScarica.eliminaFilesSorgenti = eliminaFileSorgenti;

			paramScarica.faseDelGiorno = faseDelGiorno;

			System.Windows.Forms.MessageBox.Show( paramScarica.ToString(), "DEVO SCARICARE" );

			scaricatoreFotoSrv.scarica( paramScarica );
		}

		public bool possoScaricare {
			get {
				return true;
			}
		}
		#endregion
	}




}
