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
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.EntityRepository;
using System.IO;
using log4net;
using System.Windows;

namespace Digiphoto.Lumen.UI {


	public class ScaricatoreFotoViewModel : ViewModelBase, IObserver<Messaggio> {

		public ScaricatoreFotoViewModel() {

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoViewModel = new SelettoreEventoViewModel();

			selettoreFotografoViewModel = new SelettoreFotografoViewModel();

			selettoreCartellaViewModel = new SelettoreCartellaViewModel();

			applicaConfigurazione();

			faseDelGiorno = null;

			// Ascolto messaggio
			IObservable<Messaggio> observable = LumenApplication.Instance.bus.Observe<Messaggio>();
			observable.Subscribe( this );
		}

		private void applicaConfigurazione() {

			this.eraseFotoMemoryCard = Configurazione.UserConfigLumen.eraseFotoMemoryCard;

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

		private FaseDelGiorno? _faseDelGiorno;
		public FaseDelGiorno? faseDelGiorno {
			get
			{
				return _faseDelGiorno;
			}
			set
			{
				if (value != _faseDelGiorno)
				{
					_faseDelGiorno = value;
					OnPropertyChanged("faseDelGiorno");
				}
			}
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

		
		private bool _eraseFotoMemoryCard;
		public bool eraseFotoMemoryCard {
			get {
				return _eraseFotoMemoryCard;
			}
			set {
				if( value != _eraseFotoMemoryCard ) {
					_eraseFotoMemoryCard = value;
					OnPropertyChanged( "eraseFotoMemoryCard" );
				}
			}

		}

		private bool _ricercaBarCode;
		public bool ricercaBarCode
		{
			get
			{
				return _ricercaBarCode;
			}
			set
			{
				if (value != _ricercaBarCode)
				{
					_ricercaBarCode = value;
					OnPropertyChanged("ricercaBarCode");
				}
			}

		}


		/// <summary>
		///  Mi dice se il servizio scaricatore foto è impegnato oppure no.
		///  Se è impegnato, significa che sta scaricando o provinando.
		/// </summary>
		public bool isScaricatoreBusy {
			get {
				return !isScaricatoreIdle;
			}
		}

		public bool isScaricatoreIdle {
			get {
				return (scaricatoreFotoSrv != null && scaricatoreFotoSrv.statoScarica == StatoScarica.Idle);
			}
		}
		public bool isScaricamentoInCorso {
			get {
				return (scaricatoreFotoSrv != null && scaricatoreFotoSrv.statoScarica == StatoScarica.Scaricamento);
			}
		}				

		public bool possoScaricare {
			get {
				if( IsInDesignMode )
					return true;

				bool posso = true;
				
				if( posso && String.IsNullOrEmpty( cartellaSorgente ) )
					posso = false;

				if( posso && Directory.Exists( cartellaSorgente ) == false )
					posso = false;

				// Verifico che i dati minimi siano stati indicati
				if( posso && selettoreFotografoViewModel.countElementiSelezionati < 1 )
					posso = false;
					
				if( posso && isScaricatoreBusy )
					posso = false;
	
				return posso;
			}
		}

		/// <summary>
		/// Ciccio non vuole modificare i radio button
		/// </summary>
		public bool possoModificareSpostaCopia {
			get {
				return ! Configurazione.isFuoriStandardCiccio;
			}
		}

		#endregion

		#region Comandi

		private RelayCommand _scaricareCommand;
		public ICommand scaricareCommand {
			get {
				if( _scaricareCommand == null ) {
					_scaricareCommand = new RelayCommand( param => this.scaricare(),
														param => this.possoScaricare,
														null );
				}
				return _scaricareCommand;
			}
		}

		private RelayCommand _setSpostaCopiaCommand;
		public ICommand setSpostaCopiaCommand {
			get {
				if( _setSpostaCopiaCommand == null ) {
					_setSpostaCopiaCommand = new RelayCommand( param => eraseFotoMemoryCard = Convert.ToBoolean(param) );
				}
				return _scaricareCommand;
			}
		}

		#endregion

		#region Metodi
		private void scaricare() {
			// Verifico fase del giorno
			if (controllaFaseDelGiorno() == false)
				return;

			// Per sicurezza domando se va tutto bene.
			if( chiediConfermaScarico() == false )
				return;

			

			ParamScarica paramScarica = new ParamScarica();

			// Cartella sorgente da cui scaricare
			paramScarica.cartellaSorgente = cartellaSorgente;

			// Fotografo a cui attribuire le foto
			if( fotografo != null )
				paramScarica.flashCardConfig.idFotografo = fotografo.id;

			// Evento
			if( selettoreEventoViewModel.eventoSelezionato != null ) 
				paramScarica.flashCardConfig.idEvento = selettoreEventoViewModel.eventoSelezionato.id;

			paramScarica.eliminaFilesSorgenti = eraseFotoMemoryCard;

			paramScarica.ricercaBarCode = ricercaBarCode;

			paramScarica.faseDelGiorno = faseDelGiorno;

			scaricatoreFotoSrv.scarica( paramScarica );
		}

		private bool controllaFaseDelGiorno()
		{
			if (faseDelGiorno != null)
				return true;

			FaseDelGiorno faseDelGiornoCorrente = FaseDelGiornoUtil.getFaseDelGiorno(DateTime.Now);

			StringBuilder msg = new StringBuilder("Attenzione : non hai assegnato nessuna fase del giorno");
			msg.AppendFormat(".\nVuoi assegnare automaticamente la fase {0} a tutte le foto che si stanno per scaricare?", faseDelGiornoCorrente.ToString().ToUpper());

			MessageBoxResult procediPure = MessageBoxResult.Cancel;
			dialogProvider.ShowConfirmationAnnulla( msg.ToString(), "Richiesta conferma",
				( confermato ) => {
					procediPure = confermato;

				} );

			if (procediPure == MessageBoxResult.Yes)
				faseDelGiorno = faseDelGiornoCorrente;

			return (procediPure == MessageBoxResult.Yes) || 
				(procediPure == MessageBoxResult.No);
		}


		private bool chiediConfermaScarico()
		{

			StringBuilder msg = new StringBuilder( "Confermare scarico foto:\n" );
			msg.Append( "\nCartella  = " ).Append( selettoreCartellaViewModel.cartellaSelezionata );
			msg.Append( "\nFotografo = " ).Append( selettoreFotografoViewModel.fotografoSelezionato.cognomeNome );
			if( selettoreEventoViewModel.eventoSelezionato != null )
				msg.Append( "\nEvento = " ).Append( selettoreEventoViewModel.eventoSelezionato.descrizione );
			if( faseDelGiorno != null )
				msg.Append( "\nFase Giorno = " ).Append( faseDelGiorno.ToString() );

			bool procediPure = false;
			dialogProvider.ShowConfirmation( msg.ToString(), "Richiesta conferma",
				(confermato) =>
				{
					procediPure = confermato;
				} );

			return procediPure;
		}

		/// <summary>
		///  Se nella chiavetta ci sono i
		/// 
		/// </summary>
		void caricaDatiDaChiavetta() {

			ParamScarica param = scaricatoreFotoSrv.ultimaChiavettaInserita;
			if( param == null || param.flashCardConfig == null )
				return;

			// Cartella
			if( param.cartellaSorgente != null )
				selettoreCartellaViewModel.cartellaSelezionata = param.cartellaSorgente;

			try {
				// Attenzione:
				// Per fare in modo che i componenti grafici si aggiornino sull'elemento selezionato,
				// devo trovare il giusto elemento nella collezione di valori istanziati.
				// Se invece rileggo dal db, avrò l'effetto che il SelectedItem della ListBox non si aggiorna.

				// Fotografo
				if( param.flashCardConfig.idFotografo != null )
					selettoreFotografoViewModel .fotografoSelezionato = selettoreFotografoViewModel.fotografi.Where( fo => fo.id == param.flashCardConfig.idFotografo ).SingleOrDefault();

				// Evento
				if( param.flashCardConfig.idEvento != Guid.Empty )
					selettoreEventoViewModel.eventoSelezionato = selettoreEventoViewModel.eventi.Where( ev => ev.id == param.flashCardConfig.idEvento ).SingleOrDefault();

			} catch( Exception ee ) {
				_giornale.Error( "Non sono riuscito ad impostare i valori della FlashCardConfig", ee );
			}
		}

		#endregion

		#region Messaggi
		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( Messaggio msg ) {

			if( msg is CambioStatoMsg ) {

				if( msg.sender is IScaricatoreFotoSrv ) {
					// Per qualsiasi cambio di stato del servizio, devo rivalutare la possibilità di scaricare le foto
					OnPropertyChanged( "isScaricatoreBusy" );
					OnPropertyChanged( "isScaricatoreIdle" );
					OnPropertyChanged( "isScaricamentoInCorso" );
				}
			}

			if (msg is ScaricoFotoMsg)
			{
				ScaricoFotoMsg msgScaricoFotoMsg = msg as ScaricoFotoMsg;
				
				if(msgScaricoFotoMsg.fase == FaseScaricoFoto.FineScarico){

					bool discoRemovibile = false;

					DriveInfo driveInfo = new DriveInfo(msgScaricoFotoMsg.sorgente);
					if (driveInfo.DriveType == DriveType.Removable)
					{
						discoRemovibile = true;
					}

					if (!msgScaricoFotoMsg.esitoScarico.riscontratiErrori)
					{
						StringBuilder msgScarico = new StringBuilder();
						msgScarico.AppendFormat("Scaricate {0} foto.{1}", 
							msgScaricoFotoMsg.esitoScarico.totFotoCopiateOk,
							discoRemovibile ? " Si può estrarre la card." : null);
					
						trayIconProvider.showInfo( "AVVISO",msgScarico.ToString(), 5000);
					}
					else
					{
						StringBuilder msgError = new StringBuilder("Confermare scarico foto:\n").Append(msgScaricoFotoMsg.esitoScarico.totFotoCopiateOk);
						msgError.Append("\nErrori  = ").Append(msgScaricoFotoMsg.esitoScarico.totFotoNonCopiate);
						if (discoRemovibile)
						{
							msgError.Append("\nSi può estrarre la card.");
						}

						trayIconProvider.showError("ERRORE",msgError.ToString(), 5000);
					}
				}
				else if (msgScaricoFotoMsg.fase == FaseScaricoFoto.FineLavora)
				{
					StringBuilder msgProvinatura = new StringBuilder();
					msgProvinatura.AppendFormat("Provinate {0} foto.", msgScaricoFotoMsg.esitoScarico.totFotoCopiateOk);
					
					trayIconProvider.showInfo("AVVISO",msgProvinatura.ToString(),5000);
				}
			}

			// Questo messaggio me lo lancia il mio servizio quando ha acquisito i dati di una nuova chiavetta
			if( msg.senderTag != null && msg.senderTag.Equals( "::OnLetturaFlashCardConfig" ) ) {

				// carico i dati dell'ultima chiavetta inserita
				if( isScaricatoreIdle ) {
					using( new UnitOfWorkScope() ) {
						caricaDatiDaChiavetta();
					}
				}
			}
		}
		#endregion

	}

}
