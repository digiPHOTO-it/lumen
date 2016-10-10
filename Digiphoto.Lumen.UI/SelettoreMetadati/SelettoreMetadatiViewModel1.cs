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
	public class SelettoreMetadatiViewModel1 : SelettoreMetadatiViewModel {

		public SelettoreMetadatiViewModel1( ISelettore<Fotografia> fotografieSelector ) : base()
		{
			this.fotografieSelector = fotografieSelector;
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



		#region Proprieta


		private ISelettore<Fotografia> fotografieSelector {
			set;
			get;
		}


		public override int countFotografieSelezionate {
			get {
				return fotografieSelector.countElementiSelezionati;
            }
		}

		public override bool isAlmenoUnaFotoSelezionata {
			get {
				return fotografieSelector.isAlmenoUnElementoSelezionato;
            }
		}

		#endregion Proprieta


		#region Metodi

		protected override void applicareMetadati() {
			base.applicareMetadati();
			deselezionareTutto();
		}

		protected override void eliminareMetadati() {
			base.eliminareMetadati();
			deselezionareTutto();
		}
		

		private void deselezionareTutto()
		{
			fotografieSelector.deselezionareTutto();
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

		protected override IEnumerable<Fotografia> getElementiSelezionati() {
			return fotografieSelector.getElementiSelezionati();
        }

		protected override IEnumerator<Fotografia> getEnumeratorElementiSelezionati() {
			return fotografieSelector.getEnumeratorElementiSelezionati();
		}

		#endregion Metodi

		#region Comandi


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
