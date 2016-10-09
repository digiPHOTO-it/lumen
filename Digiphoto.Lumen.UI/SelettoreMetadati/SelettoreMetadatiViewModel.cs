using System;
using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using System.Windows.Input;
using System.Windows;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.Servizi.EntityRepository;

namespace Digiphoto.Lumen.UI
{
	public class SelettoreMetadatiViewModel : ViewModelBase
	{
		public SelettoreMetadatiViewModel( ISelettore<Fotografia> fotografieSelector )
		{
			this.fotografieSelector = fotografieSelector;
			
			// Questo è l'oggetto che contiene le proprietà che vado ad editare
			metadati = new MetadatiFoto();

			// Istanzio i ViewModel dei componenti di cui io sono composto
			selettoreEventoViewModel = new SelettoreEventoViewModel();
		}


		/// <summary>
		/// Io avrei voluto bindare nella view il componente che visualizza il numero delle foto selezionate, direttamente a "fotografieSelector.countElementiSelezionati".
		/// Ma questo non è possibile, perché fotografieSelector è un interfaccia.
		/// Pare che non si possono bindare le interfacce (se non con strani artifici incomprensibili).
		/// Allora ho optato per gestire un evento nella interfaccia di selezione cambiata. Quando la gallery mi solleva l'evento, allora
		/// rilancio a mia volta il change di una property locale.
		/// E' un pò esagerata come cosa, ma non ho trovato altro modo.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FotografieSelector_selezioneCambiata( object sender, EventArgs e ) {

			// Sistemo le spille
			caricareStatoMetadatiCommand.Execute( null );

			OnPropertyChanged( "countFotografieSelezionate" );
		}

		#region Servizi

		IFotoExplorerSrv fotoExplorerSrv
		{
			get
			{
				return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			}
		}

		#endregion
		
		#region Proprieta


		private ISelettore<Fotografia> fotografieSelector {
			set;
			get;
		}

		/// <summary>
		/// In questa proprietà ci sono i 3 valori che l'utente può modificare con anche i flag per dire queli sono interessati
		/// </summary>
		private MetadatiFoto _metadati;
		public MetadatiFoto metadati
		{
			get {
				return _metadati;
			}
			private set {
				if( _metadati != value ) {
					_metadati = value;
					OnPropertyChanged( "metadati" );
				}
			}
		}

		public FaseDelGiorno[] fasiDelGiorno
		{
			get
			{
				return FaseDelGiornoUtil.fasiDelGiorno;
			}
		}

		public SelettoreEventoViewModel selettoreEventoViewModel
		{
			get;
			private set;
		}


		private bool _faseDelgiornoEnabled;
		public bool FaseDelGiornoEnabled
		{
			get
			{
				return _faseDelgiornoEnabled;
			}

			set
			{
				if (_faseDelgiornoEnabled != value)
				{
					_faseDelgiornoEnabled = value;
					OnPropertyChanged( "FaseDelGiornoEnabled" );
				}
			}
		}

		private bool _eventoEnabled;
		public bool EventoEnabled
		{
			get
			{
				return _eventoEnabled;
			}

			set
			{
				if (_eventoEnabled != value)
				{
					_eventoEnabled = value;
					OnPropertyChanged("EventoEnabled");

#if NONCAPISCO
					foreach (Fotografia fot in fotografieCW)
					{
						if (fot.evento != null)
						{
							//Serve a selezzionare l'evento dal menu rapido
							selettoreEventoViewModel.eventoSelezionato = fot.evento;
						}
					}
#endif
				}
			}
		}

		public bool possoCaricareStatoMetadati { 
			get {
				return true;
			}
		}

		public int countFotografieSelezionate {
			get {
				return fotografieSelector.countElementiSelezionati;
            }
		}

		#endregion

		#region Controlli

		public bool isAlmenoUnaFotoSelezionata {
			get {
				return fotografieSelector.isAlmenoUnElementoSelezionato;
            }
		}

		/// <summary>
		/// Se ho almeno una foto selezionata, ed ho scelto almeno una spunta per imporre il dato ...
		/// allora posso applicare.
		/// Amche se il dato da imporre è vuoto, va bene lo stesso, perché signigica che voglio eliminare la didascalia.
		/// </summary>
		public bool possoApplicareMetadati
		{
			get
			{
				return     isAlmenoUnaFotoSelezionata 
				        && metadati != null 
						&& metadati.isAlmenoUnoUsato;
			}
		}

		public bool possoEliminareMetadati
		{
			get
			{
				return possoApplicareMetadati;
			}
		}

        #endregion Controlli

        #region Metodi

        void applicareMetadati()
		{
			// Questo attributo, non riesco a puntarlo direttamente nei metadati, ma risiede nel vm del suo componente.
			// Lo copio qui adesso.
			metadati.evento = selettoreEventoViewModel.eventoSelezionato;


			// Ricavo l'Evento dall'apposito componente di selezione.
			// Tutti gli altri attributi sono bindati direttamente sulla struttura MetadatiFoto.
			if (fotoExplorerSrv.modificaMetadatiFotografie( fotografieSelector.getElementiSelezionati(), metadati))
			{
				dialogProvider.ShowMessage("Metadati Modificati correttamente","AVVISO");
			}
			else
			{
				dialogProvider.ShowError("Errore modifica metadati", "ERRORE",null);
			}

			MetadatiMsg msg = new MetadatiMsg(this);
			msg.fase = Fase.Completata;
			LumenApplication.Instance.bus.Publish(msg);

			// Svuoto ora i metadati per prossime elaborazioni
			metadati = new MetadatiFoto();

            deselezionareTutto();
		}

		void eliminareMetadati()
		{
			bool procediPure = false;

            String metadatiToDelete = "";
            //Verifico quali metadati devono essere eliminati
            if (metadati.usoDidascalia)
            {
                metadati.didascalia = null;
                metadatiToDelete += "\nDidascalia";
            }

            if (metadati.usoEvento)
            {
                metadati.evento = null;
                metadatiToDelete += "\nEvento";
            }

            if (metadati.usoFaseDelGiorno)
            {
                metadati.faseDelGiorno = null;
                metadatiToDelete += "\nFase del Giorno";
            }

            dialogProvider.ShowConfirmation("Sei sicuro di voler eliminare i seguenti metadati"+ metadatiToDelete+"\ndelle " + fotografieSelector.countElementiSelezionati + " fotografie selezionate?", "Eliminazione metadati",
								  (confermato) =>
								  {
									  procediPure = confermato;
								  });

			if (!procediPure)
				return;

			if(fotoExplorerSrv.modificaMetadatiFotografie(fotografieSelector.getElementiSelezionati(), metadati))
			{
				dialogProvider.ShowMessage("Metadati Modificati correttamente", "AVVISO");
			}
            else
            {
                dialogProvider.ShowError("Errore modifica metadati", "ERRORE", null);
            }

            // Svuoto ora i metadati
            metadati = new MetadatiFoto();

			//dialogProvider.ShowMessage("Eliminati i metadati delle " + selettoreMetadatiView.FotografiaCWP.SelectedItems.Count + " fotografie selezionate!", "Operazione eseguita");
			MetadatiMsg msg = new MetadatiMsg(this);
			msg.fase = Fase.Completata;
			LumenApplication.Instance.bus.Publish(msg);

            deselezionareTutto();
		}


		private void deselezionareTutto()
		{
			fotografieSelector.deselezionareTutto();
		}

		/// <summary>
		/// Carico lo stato dei metadati prendendolo dalle foto selezionate
		/// </summary>
		private void caricareStatoMetadati() {

			bool didascalieDiscordanti = false;
			string didascaliaNew = null;

			bool eventiDiscordanti = false;
			Guid? eventoIdNew = null;  // Non posso usare l'oggetto "Evento" perché entity framework sulle entità non idratate mi torna null anche se non è vero :-(
			Evento eventoNew = null;

			bool fasiDelGiornoDiscordanti = false;
            short? faseDelGiornoNew = null;

			IEnumerator<Fotografia> itera = fotografieSelector.getEnumeratorElementiSelezionati();
			int conta = 0;
			while( itera.MoveNext() ) {

				Fotografia f = itera.Current;
				++conta;

				// -- didascalia

				if( String.IsNullOrWhiteSpace( f.didascalia ) ) {
					if( didascaliaNew != null )
						didascalieDiscordanti = true;
				} else {
					if( f.didascalia != didascaliaNew && conta > 1 )
						didascalieDiscordanti = true;
				}
				didascaliaNew = String.IsNullOrWhiteSpace( f.didascalia ) ? null : f.didascalia;

				// -- evento

				if( f.evento_id == null ) {
					if( eventoIdNew != null )
						eventiDiscordanti = true;
				} else {
					if( f.evento_id != eventoIdNew && conta > 1 )
						eventiDiscordanti = true;
				}
				eventoIdNew = f.evento_id;
				if( f.evento != null )
					eventoNew = f.evento;  // Se la foto è staccata, questo è null (mentre il suo ID è valorizzato)

				// -- fase del giorno

				if( f.faseDelGiorno == null ) {
					if( faseDelGiornoNew != null )
						fasiDelGiornoDiscordanti = true;
				} else {
					if( f.faseDelGiorno != faseDelGiornoNew && conta > 1 )
						fasiDelGiornoDiscordanti = true;
				}
				faseDelGiornoNew = f.faseDelGiorno;

			}


			// -- ora travaso i dati che sono concordanti
			MetadatiFoto metadatiNew = new MetadatiFoto();


			if( didascalieDiscordanti ) {
				metadatiNew.didascalia = null;
				metadatiNew.usoDidascalia = false;
			} else {
				metadatiNew.didascalia = String.IsNullOrWhiteSpace( didascaliaNew ) ? null : didascaliaNew;
				metadatiNew.usoDidascalia = (metadatiNew.didascalia != null);
			}
			
			if( eventiDiscordanti ) {
				metadatiNew.evento = null;
				metadatiNew.usoEvento = false;
			} else {
				if( eventoIdNew != null ) {
					if( eventoNew == null ) {
						// L'oggetto era staccato. Lo rileggo
						// Devo ricaricare l'oggetto evento dall'ID	
						IEntityRepositorySrv<Evento> repo = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Evento>>();
						metadatiNew.evento = repo.getById( eventoIdNew );
					} else
						metadatiNew.evento = eventoNew;
				}
				metadatiNew.usoEvento = (metadatiNew.evento != null);
			}
			selettoreEventoViewModel.eventoSelezionato = metadatiNew.evento;  // ribalto questo valore nel vm del componente perché è li che sta il valore buono

			if( fasiDelGiornoDiscordanti ) {
				metadatiNew.faseDelGiorno = null;
				metadatiNew.usoFaseDelGiorno = false;
			} else {
				if( faseDelGiornoNew != null )
					metadatiNew.faseDelGiorno = FaseDelGiornoUtil.getFaseDelGiorno( (short)faseDelGiornoNew );
				metadatiNew.usoFaseDelGiorno = (metadatiNew.faseDelGiorno != null);
			}

			this.metadati = metadatiNew;
		}

		void cambiareModalitaOperativa( string modo ) {
			if( modo == "A" )
				attiavazione();
			if( modo == "P" )
				passivazione();
		}

		/// <summary>
		/// Inizio ad ascoltare gli eventi di selezione cambiata
		/// </summary>
		public void attiavazione() {

			caricareStatoMetadati();
			OnPropertyChanged( "countFotografieSelezionate" );

			fotografieSelector.selezioneCambiata += FotografieSelector_selezioneCambiata;
		}

		/// <summary>
		/// Smetto di ascoltare gli eventi di selezione cambiata
		/// </summary>
		public void passivazione() {
			fotografieSelector.selezioneCambiata -= FotografieSelector_selezioneCambiata;
		}



		#endregion

		#region Comandi

		private RelayCommand _applicareMetadatiCommand;
		public ICommand applicareMetadatiCommand
		{
			get
			{
				if (_applicareMetadatiCommand == null)
				{
					_applicareMetadatiCommand = new RelayCommand(p => applicareMetadati(),
																  p => possoApplicareMetadati, false);
				}
				return _applicareMetadatiCommand;
			}
		}

		private RelayCommand _eliminareMetadatiCommand;
		public ICommand eliminareMetadatiCommand
		{
			get
			{
				if (_eliminareMetadatiCommand == null)
				{
					_eliminareMetadatiCommand = new RelayCommand(p => eliminareMetadati(),
																  p => possoEliminareMetadati, false);
				}
				return _eliminareMetadatiCommand;
			}
		}

		private RelayCommand _caricareStatoMetadatiCommand;
		public ICommand caricareStatoMetadatiCommand {
			get {
				if( _caricareStatoMetadatiCommand == null ) {
					_caricareStatoMetadatiCommand = new RelayCommand( p => caricareStatoMetadati(),
					                                                 p => possoCaricareStatoMetadati, false );
				}
				return _caricareStatoMetadatiCommand;
			}
		}

		private RelayCommand _cambiareModalitaOperativaCommand;
		public ICommand cambiareModalitaOperativaCommand {
			get {
				if( _cambiareModalitaOperativaCommand == null ) {
					_cambiareModalitaOperativaCommand = new RelayCommand( p => cambiareModalitaOperativa( p as string ),
					                                                      p => true, false );
				}
				return _cambiareModalitaOperativaCommand;
			}
		}
		#endregion Comandi
	}
}
