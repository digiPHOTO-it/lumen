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

namespace Digiphoto.Lumen.UI
{
    public class SelettoreAzioniAutomaticheViewModel : ViewModelBase, IObserver<EntityCambiataMsg>
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

		#endregion

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

		private void eseguiAzioniAutomatiche()
		{
			if (azioneAutomaticaSelezionata != null)
			{
				const int max = 3;
				bool conferma = false;

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

		#endregion

		#region Controlli
		public bool isAlmenoUnaSelezionata
		{
			get
			{
				return fotografieSelector.isAlmenoUnElementoSelezionato;
			}
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

		private RelayCommand _eseguiAzioniAutomaticheCommand;
		public ICommand eseguiAzioniAutomaticheCommand
		{
			get {
				if (_eseguiAzioniAutomaticheCommand == null)
				{
					_eseguiAzioniAutomaticheCommand = new RelayCommand(param => this.eseguiAzioniAutomatiche(),
																		paramp => isAlmenoUnaSelezionata,  
																		false);
				}
				return _eseguiAzioniAutomaticheCommand;
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
	}
}
