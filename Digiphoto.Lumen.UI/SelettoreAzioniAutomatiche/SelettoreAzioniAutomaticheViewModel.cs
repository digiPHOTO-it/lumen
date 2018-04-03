using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.DatiDiEsempio;
using System.Windows.Input;
using System.Windows;
using Digiphoto.Lumen.Core.Database;

using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.UI.Dialogs;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.UI
{
    public class SelettoreAzioniAutomaticheViewModel : ViewModelBase, ISelettore<AzioneAuto>, IObserver<EntityCambiataMsg>
    {

		public SelettoreAzioniAutomaticheViewModel( ISelettore<Fotografia> fotografieSelector )
        {
			this.DisplayName = "Selettore Azioni Automatiche";

			this.fotografieSelector = fotografieSelector;

			// istanzio la lista vuota
			azioniAutomatiche = new ObservableCollection<AzioneAuto>();

			IObservable<EntityCambiataMsg> observable = LumenApplication.Instance.bus.Observe<EntityCambiataMsg>();
			observable.Subscribe(this);

			rileggereAzioniAutomatiche();
		}

		#region Proprietà

		public ISelettore<Fotografia> fotografieSelector;


		/// <summary>
		/// Tutti i formatiCarta da visualizzare
		/// </summary>
		public ObservableCollection<AzioneAuto> azioniAutomatiche {
			get;
			set;
		}

		/// <summary>
		/// I'Azione Automatica attualmente selezionata
		/// </summary>
		AzioneAuto _azioneAutomaticaSelezionata;
		public AzioneAuto azioneAutomaticaSelezionata
		{
			get {
				return _azioneAutomaticaSelezionata;
			}
			set {
				if (value != _azioneAutomaticaSelezionata)
                {
					_azioneAutomaticaSelezionata = value;
					OnPropertyChanged("azioneAutomaticaSelezionata");

					// Quando cambia la selezione, potrei eseguire l'associazione (se ci sono i presupposti)
					if( value != null )
						forseAssociareMaschere(); 
				}
			}
		}

		private IEntityRepositorySrv<AzioneAuto> azioniAutomaticheRepositorySrv
		{
			get
			{
				return (IEntityRepositorySrv<AzioneAuto>)LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<AzioneAuto>>();
			}
		}

		private IFotoRitoccoSrv fotoRitoccoSrv
		{
			get
			{
				return LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
			}
		}

		/// <summary>
		/// Questa è la prima azione automatica che l'utente ha selezionato per l'associazione
		/// </summary>
		private AzioneAuto azioneAutoAssociare1 {
			get;
			set;
		}

		private bool _modalitaAssociazione;
		public bool modalitaAssociazione {
			get {
				return _modalitaAssociazione;
			}
			set {
				if( _modalitaAssociazione != value ) {
					_modalitaAssociazione = value;
					OnPropertyChanged( "modalitaAssociazione" );
				}
			}
		}

		public bool isAlmenoUnaFotoSelezionata {
			get {
				return fotografieSelector.isAlmenoUnElementoSelezionato;
			}
		}

		public bool possoEseguireAzioneAutomatica {
			get {
				return isAlmenoUnaFotoSelezionata && isAlmenoUnElementoSelezionato;
			}
		}

		private bool _isComposizione; 
		public bool isComposizione {
			get {
				return _isComposizione;
			}
			set {
				if( _isComposizione != value ) {
					_isComposizione = value;
					OnPropertyChanged( "isComposizione" );
				}
			}
		}


		#endregion Proprietà

		#region Metodi
		private void rileggereAzioniAutomatiche()
		{
			rileggereAzioniAutomatiche(false);
		}

		private void rileggereAzioniAutomatiche(object param)
		{

			// Decido se devo dare un avviso all'utente
			Boolean avvisami = false;

			if( param != null ) {
				if( param is Boolean )
					avvisami = (Boolean)param;
				if( param is string )
					Boolean.TryParse( param.ToString(), out avvisami );
			}
			// ---


			IEnumerable<AzioneAuto> listaAzioniAuto = null;
			if( IsInDesignMode ) {
				// genero dei dati casuali
				DataGen<AzioneAuto> dg = new DataGen<AzioneAuto>();
				listaAzioniAuto = dg.generaMolti(5);

			} else {
				listaAzioniAuto = azioniAutomaticheRepositorySrv.getAll().OrderBy(a => a.nome);
			}

			// purtoppo pare che rimpiazzare il reference con uno nuovo, causa dei problemi.
			// Non posso istanziare nuovamente la lista, ma la devo svuotare e ripopolare.
			azioniAutomatiche.Clear();
            foreach (AzioneAuto a in listaAzioniAuto)
            {
				azioniAutomatiche.Add(a);
            }

			if( avvisami && dialogProvider != null )
				dialogProvider.ShowMessage("Rilette " + azioniAutomatiche.Count() + " azioni automatiche ", "Successo");
		}

		private void eseguireAzioneAutomatica()
		{
			if (azioneAutomaticaSelezionata != null)
			{
				const int troppe = 150;
				const int max = 3;
				bool conferma = false;


				// Verifica se troppe. Oltre questo tot, si pianta il pc
				if( fotografieSelector.countElementiSelezionati > troppe ) {
					dialogProvider.ShowError( "Le foto selezionate sono troppe\nRidurre al massimo a " + troppe + " foto", "Impossibile proseguire", null );
					return;
				}

				if( fotografieSelector.countElementiSelezionati <= max )
					conferma = true;
				else {
					dialogProvider.ShowConfirmation( "Sei sicuro di voler applicare questa Azione Automatica? \n\n(" +
						azioneAutomaticaSelezionata.nome +
						")\n\nn° " + fotografieSelector.countElementiSelezionati +
						" foto", "Elimina",
						( confermato ) => {
							conferma = confermato;
						} );
				}

				if (!conferma)
				{
					return;
				}

				fotoRitoccoSrv.applicareAzioneAutomatica( fotografieSelector.getElementiSelezionati(), azioneAutomaticaSelezionata );
			}
		}


		private void rinomina()
		{
			InputBoxDialog d = new InputBoxDialog();
			d.Title = "Inserire il nome dell'azione";
			bool? esito = d.ShowDialog();

			if (esito != true)
				return;

			AzioneAuto azione = azioneAutomaticaSelezionata;
			OrmUtil.forseAttacca<AzioneAuto>(ref azione);
			azione.nome = d.inputValue.Text;
			OrmUtil.cambiaStatoModificato(azione);

			azioneAutomaticaSelezionata = azione;

			rileggereAzioniAutomatiche();

			dialogProvider.ShowMessage("Modifica Effettuata con successo","Avviso");
		}

		private void elimina()
		{
			bool conferma = false;

			dialogProvider.ShowConfirmation("Sei sicuro di voler eliminare questa Azione Automatica? \n\n(" + azioneAutomaticaSelezionata.nome + ")", "Elimina",
				(confermato) =>
				{
					conferma = confermato;
				});

			if (!conferma)
			{
				return;
			}

			AzioneAuto azione = azioneAutomaticaSelezionata;
			OrmUtil.forseAttacca<AzioneAuto>(ref azione);
			LumenEntities dbContext = UnitOfWorkScope.currentDbContext;
			dbContext.AzioniAutomatiche.Remove(azione);
			dbContext.SaveChanges();

			rileggereAzioniAutomatiche();

		}


		void iniziareAssociazione() {

			// Per poter iniziare l'associazione, ocorre che la riga selezionata, contenga una mascheratura
			Mascheratura m1 = estraiMascheratura( azioneAutomaticaSelezionata );

			if( m1 == null ) {
				dialogProvider.ShowError( "Questa azione non contiene una Mascheratura.\nSi possono associare solo azioni contenenti Mascherature", "Associazione Maschere", null );
			} else {
				// Mi salvo la prima azione che è stata selezionata
				azioneAutoAssociare1 = azioneAutomaticaSelezionata;

				// Cambio modalità
				modalitaAssociazione = true;
			}
		}

		private Mascheratura estraiMascheratura( AzioneAuto azioneAuto ) {

			if( azioneAuto.correzioniXml == null )
				return null;

			CorrezioniList correzioniList = SerializzaUtil.stringToObject<CorrezioniList>( azioneAuto.correzioniXml );
			if( correzioniList == null || correzioniList.Count < 1 )
				return null;

			return (Mascheratura)correzioniList.FirstOrDefault( c => c is Mascheratura );
		}

		void forseAssociareMaschere() {

			if( !modalitaAssociazione )
				return;

			// Per poter iniziare l'associazione, ocorre che la riga selezionata, contenga una mascheratura
			Mascheratura mascheratura2 = estraiMascheratura( azioneAutomaticaSelezionata );
			if( mascheratura2 == null ) {
				dialogProvider.ShowError( "L'azione selezionata non contiene Mascheratura", "Azioni non associate", null );
				return;
			}

			// Dalla prima azione, estraggo la mascheratura, perché devo controllare che sia di Orientamento opposto alla prima che ho fissato.
			CorrezioniList correzioni1 = SerializzaUtil.stringToObject<CorrezioniList>( azioneAutoAssociare1.correzioniXml );
			Mascheratura mascheratura1 = (Mascheratura)correzioni1.ToList<Correzione>().FirstOrDefault( c => c is Mascheratura );

			var ratio1 = mascheratura1.width / mascheratura1.height;
			var ratio2 = mascheratura2.width / mascheratura2.height;

			if( (ratio1 < 1 && ratio2 < 1) || (ratio1 > 1 && ratio2 > 1) ) {
				dialogProvider.ShowError( "Le maschere devono ossere di diverso orientamento.\nUna orizzontale ed una verticale!", "Azioni non associate", null );
				return;
			}

			// Ok : adesso posso procedere alla associazione

			MascheraturaOrientabile mo = new MascheraturaOrientabile();
			mo.mascheraturaH = (ratio1 < 1 ? mascheratura2 : mascheratura1);
			mo.mascheraturaV = (ratio1 < 1 ? mascheratura1 : mascheratura2);

			using( new UnitOfWorkScope() ) {

				// Sostituisco la correzione nella lista, cercando di mettere quella nuova nella stessa posizione
				int pos = correzioni1.IndexOf( mascheratura1 );
				correzioni1.RemoveAt( pos );
				correzioni1.Insert( pos, mo );

				// Rimuovo l'azione2 dalla collezione a video
				AzioneAuto azioneDacanc = azioneAutomaticaSelezionata;
				azioniAutomatiche.Remove( azioneDacanc );

				// Ora vado ad aggiornare l'azione1 con le correzioni nuove
				AzioneAuto azione = azioneAutoAssociare1;
				OrmUtil.forseAttacca<AzioneAuto>( ref azione );
				azioneAutoAssociare1.correzioniXml = SerializzaUtil.objectToString( correzioni1 );
				
				// Elimino dal db la azione2
				OrmUtil.forseAttacca<AzioneAuto>( ref azioneDacanc );

				// Rimuovo l'azione dal database
				UnitOfWorkScope.currentDbContext.AzioniAutomatiche.Remove( azioneDacanc );

				UnitOfWorkScope.currentDbContext.SaveChanges();

				// Torno in modalità normale
				modalitaAssociazione = false;
			}

			// Purtroppo non si aggiornano le icone di overlay. devo ricaricare.
			App.Current.Dispatcher.BeginInvoke( new Action( () => {
				rileggereAzioniAutomaticheCommand.Execute( null );
			}
			) );

		}
		
		/// <summary>
		/// Se una azione automatica contiene una mascheratura orientabile (quindi doppia)
		/// posso disassociarla e creare una nuova azione
		/// </summary>
		void disassociareMascheratura() {

			CorrezioniList correzioniList = null;
			AzioneAuto azioneAuto = azioneAutomaticaSelezionata;

			MascheraturaOrientabile mascheraturaOrientabile = null;

			// Controllo che l'azione corrente contenga una mascheratura orientabile
			if( azioneAuto.correzioniXml != null ) {
				correzioniList = SerializzaUtil.stringToObject<CorrezioniList>( azioneAuto.correzioniXml );
				if( correzioniList != null && correzioniList.Count > 0 ) {
					mascheraturaOrientabile = (MascheraturaOrientabile)correzioniList.SingleOrDefault( mo => mo is MascheraturaOrientabile );
				}
			}

			if( mascheraturaOrientabile == null ) {
				dialogProvider.ShowError( "L'azione selezionata non contiene una <MascheraturaOrientabile>.\nSolo queste si possono separare!", "Azione non separabile", null );
				return;
			}

			// ok procedo a separare le cornici
			// Sostituisco la correzione nella lista, cercando di mettere quella nuova nella stessa posizione
			Mascheratura masH = mascheraturaOrientabile.mascheraturaH;
			Mascheratura masV = mascheraturaOrientabile.mascheraturaV;

			// Elimino la mascheratura doppia ...
			correzioniList.Remove( mascheraturaOrientabile );
			// aggiungo solo la mascheratura Orizzontale
			correzioniList.Insert( 0, masV );

			// Aggiorno l'entità sul db
			OrmUtil.forseAttacca<AzioneAuto>( ref azioneAuto );
			azioneAuto.correzioniXml = SerializzaUtil.objectToString( correzioniList );

			// Ora creo l'altra azione
			CorrezioniList correzioniList2 = new CorrezioniList();
			correzioniList2.Add( masH );
			AzioneAuto azioneV = new AzioneAuto {
				id = Guid.NewGuid(),
				attivo = true,
				nome = "Separata",
				correzioniXml = SerializzaUtil.objectToString( correzioniList2 )
			};
			UnitOfWorkScope.currentDbContext.AzioniAutomatiche.Add( azioneV );

			// Ora aggiungo anche alla collezione visiva
			azioniAutomatiche.Add( azioneV );

			deselezionareTutto();

			// Purtroppo non si aggiornano le icone di overlay. devo ricaricare.
			App.Current.Dispatcher.BeginInvoke( new Action( () => {
				rileggereAzioniAutomaticheCommand.Execute( null );
				}
			) );
		}
	

		void rinunciareAssociazione() {
			azioneAutoAssociare1 = null;
			modalitaAssociazione = false;
		}

		

		#endregion

		#region Comandi

		private RelayCommand _rileggereAzioniAutomaticheCommand;
		public ICommand rileggereAzioniAutomaticheCommand {
			get {
				if (_rileggereAzioniAutomaticheCommand == null)
				{
					_rileggereAzioniAutomaticheCommand = new RelayCommand(param => this.rileggereAzioniAutomatiche(param), 
																			null, 
																			false);
				}
				return _rileggereAzioniAutomaticheCommand;
			}
		}

		private RelayCommand _eseguireAzioneAutomaticaCommand;
		public ICommand eseguireAzioneAutomaticaCommand
		{
			get {
				if (_eseguireAzioneAutomaticaCommand == null)
				{
					_eseguireAzioneAutomaticaCommand = new RelayCommand(param => eseguireAzioneAutomatica(),
																		paramp => possoEseguireAzioneAutomatica,  
																		false);
				}
				return _eseguireAzioneAutomaticaCommand;
			}
		}

		private RelayCommand _rinominaCommand;
		public ICommand rinominaCommand
		{
			get
			{
				if (_rinominaCommand == null)
				{
					_rinominaCommand = new RelayCommand(param => this.rinomina(), null, true);
				}
				return _rinominaCommand;
			}
		}

		private RelayCommand _eliminaCommand;
		public ICommand eliminaCommand
		{
			get
			{
				if (_eliminaCommand == null)
				{
					_eliminaCommand = new RelayCommand(param => this.elimina(), null, false);
				}
				return _eliminaCommand;
			}
		}
		
		public event SelezioneCambiataEventHandler selezioneCambiata;

		private RelayCommand _abilitareModoAssociaCommand;
		public ICommand abilitareModoAssociaCommand {
			get {
				if( _abilitareModoAssociaCommand == null ) {
					_abilitareModoAssociaCommand = new RelayCommand( p => this.iniziareAssociazione(), p => true );
				}
				return _abilitareModoAssociaCommand;
			}
		}

		private RelayCommand _rinunciareAssociazioneCommand;
		public ICommand rinunciareAssociazioneCommand {
			get {
				if( _rinunciareAssociazioneCommand == null ) {
					_rinunciareAssociazioneCommand = new RelayCommand( p => this.rinunciareAssociazione(), p => (modalitaAssociazione == true) );
				}
				return _rinunciareAssociazioneCommand;
			}
		}

		private RelayCommand _disassociareMascheraturaCommand;
		public ICommand disassociareMascheraturaCommand {
			get {
				if( _disassociareMascheraturaCommand == null ) {
					_disassociareMascheraturaCommand = new RelayCommand( p => this.disassociareMascheratura(), p => (modalitaAssociazione == false), true );
				}
				return _disassociareMascheraturaCommand;
			}
		}
		
		#endregion

		#region Interfaccia IObserver

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( EntityCambiataMsg value ) {
			
			// Qualcuno ha spataccato nella tabella dei formati carta. Rileggo tutto
			if( value.type == typeof( AzioneAuto ) )
				rileggereAzioniAutomaticheCommand.Execute( false );
		}

		#endregion Interfaccia IObserver

		#region Interfaccia ISelettore


		public void deselezionareTutto() {
			this.azioneAutomaticaSelezionata = null;
		}

		public void deselezionareSingola( AzioneAuto elem ) {
			if( elem.Equals( azioneAutomaticaSelezionata ) )
				azioneAutomaticaSelezionata = null;
		}

		public IEnumerator<AzioneAuto> getEnumeratorElementiTutti() {
			return azioniAutomatiche.GetEnumerator();
		}

		public IEnumerable<AzioneAuto> getElementiTutti() {
			return azioniAutomatiche.AsEnumerable();
		}

		public IEnumerator<AzioneAuto> getEnumeratorElementiSelezionati() {
			throw new NotImplementedException();
		}

		public IEnumerable<AzioneAuto> getElementiSelezionati() {
			throw new NotImplementedException();
		}
		
		public int countElementiSelezionati {
			get {
				return azioneAutomaticaSelezionata != null ? 1 : 0;
			}
		}
		
		public int countElementiTotali {
			get {
				return azioniAutomatiche == null ? 0 : azioniAutomatiche.Count;
			}
		}

		public bool isAlmenoUnElementoSelezionato {
			get {
				return countElementiSelezionati > 0 ? true : false;
			}
		}

		#endregion
	}
}
