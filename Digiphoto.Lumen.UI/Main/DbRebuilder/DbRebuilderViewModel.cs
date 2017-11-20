using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricostruzione;
using Digiphoto.Lumen.UI.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Digiphoto.Lumen.UI.Main {
	
	public class DbRebuilderViewModel : ClosableWiewModel {


		public DbRebuilderViewModel() {

			paramRebuild = new ParamRebuild();
			paramRebuild.giorno = DateTime.Today;

			this.selettoreFotografoViewModel = new SelettoreFotografoViewModel();

			analisiCompletata = false;
		}

		#region Proprietà

		public SelettoreFotografoViewModel selettoreFotografoViewModel {
			get;
			private set;
		}

		public ParamRebuild paramRebuild {
			get;
			set;
		}

		public bool possoIniziareAnalisi { 
			get {
				return this.paramRebuild.giorno != null;
			}
		}

		private bool _analisiCompletata;
		public bool analisiCompletata { 
			get {
				return _analisiCompletata;
			}
			set {
				if( _analisiCompletata != value ) {
					_analisiCompletata = value;
					OnPropertyChanged( "analisiCompletata" );
					OnPropertyChanged( "isNecessarioRicostruire" );
				}
			}
		}

		IDbRebuilderSrv dbRebuilderSrv = null;

		public bool isNecessarioRicostruire { 
			get {
				return analisiCompletata && dbRebuilderSrv.necessarioRicostruire;
			}
		}

		public int totFotografiMancanti {
			get {
				return analisiCompletata ? dbRebuilderSrv.contaFotografiMancanti : 0;
			}
		}

		public int totFotoMancanti { 
			get {
				return analisiCompletata ? dbRebuilderSrv.contaFotoMancanti : 0;
			}
		}

		public int totFotoElaborate { 
			get {
				return analisiCompletata? dbRebuilderSrv.contaFotoElaborate : 0;
			}
		}

		public int totJpegMancanti { 
			get {
				return analisiCompletata ? dbRebuilderSrv.contaJpegMancanti : 0;
			}
		}
		public int totJpegElaborati {
			get {
				return analisiCompletata ? dbRebuilderSrv.contaJpegElaborati : 0;
			}
		}

		public bool possoRicostruireDatabase { 
			get {
				return analisiCompletata && dbRebuilderSrv.necessarioRicostruire;
			}
		}

		#endregion Proprietà

		#region Metodi

		private void iniziareAnalisi() {

			bool procediPure = false;

			analisiCompletata = false;

			dialogProvider.ShowConfirmation( "Questa operazione potrebbe durare parecchi minuti.\nSei sicuro di voler cominciare l'analisi?", "Fase 1 : analisi foto",
				( sino ) => {
					procediPure = sino;
				} );
			if( !procediPure )
				return;

			this.dbRebuilderSrv = LumenApplication.Instance.creaServizio<IDbRebuilderSrv>();

			// Questo attributo non riesco a bindarlo nel form. Lo assegno qui.
			paramRebuild.fotografo = selettoreFotografoViewModel.fotografoSelezionato;
			dbRebuilderSrv.analizzare( this.paramRebuild );

			analisiCompletata = true;

			OnPropertyChanged( "totFotografiMancanti" );
			OnPropertyChanged( "totFotoMancanti" );
			OnPropertyChanged( "totFotoElaborate" );
			OnPropertyChanged( "totJpegMancanti" );
			OnPropertyChanged( "totJpegElaborati" );
			OnPropertyChanged( "isNecessarioRicostruire" );
		}

		void ricostruireDatabase() {
			
			dbRebuilderSrv.ricostruire();

			StringBuilder msg = new StringBuilder();
			msg.Append( "Sono state apportate le seguenti modifiche:" );
			msg.Append( "\nAggiunti  " + dbRebuilderSrv.contaFotografiAggiunti + " fotografi." );
			msg.Append( "\nAggiunte  " + dbRebuilderSrv.contaFotoAggiunte + " fotografie." );
			msg.Append( "\nEliminate " + dbRebuilderSrv.contaFotoEliminate + " fotografie." );

			dialogProvider.ShowMessage( msg.ToString(), "Ricostruzione terminata" );
		}

		#endregion Metodi

		#region Comandi
		

		private RelayCommand _iniziareAnalisiCommand;
		public ICommand iniziareAnalisiCommand {
			get {
				if( _iniziareAnalisiCommand == null ) {
					_iniziareAnalisiCommand = new RelayCommand( param => iniziareAnalisi(), param => possoIniziareAnalisi, false );
				}
				return _iniziareAnalisiCommand;
			}
		}

		private RelayCommand _ricostruireDatabaseCommand;
		public ICommand ricostruireDatabaseCommand {
			get {
				if( _ricostruireDatabaseCommand == null ) {
					_ricostruireDatabaseCommand = new RelayCommand( param => ricostruireDatabase(), param => possoRicostruireDatabase, false );
				}
				return _ricostruireDatabaseCommand;
			}
		}

		
		#endregion Comandi
	}
}
