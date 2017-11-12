using System;
using System.Windows;
using Digiphoto.Lumen.UI.Mvvm;

namespace Digiphoto.Lumen.UI.SelettoreAzioniRapide {

	/// <summary>
	/// Interaction logic for AzioniRapideView.xaml
	/// </summary>
	public partial class SelettoreAzioneRapida : UserControlBase
	{
		public SelettoreAzioneRapida()
		{
			InitializeComponent();

			DataContextChanged += selettoreAzioniRapideView_DataContextChanged;
		}

		void selettoreAzioniRapideView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			associaDialogProvider();

			// Devo anche gestire la popup per associare la faccia del fotografo
			if( this.DataContext is SelettoreAzioneRapidaViewModel )
				viewModel.openPopupDialogRequest += this.viewModel_openPopupDialogRequest;
		}

		private void viewModel_openPopupDialogRequest( object sender, EventArgs e ) {

			if( e is SelezioneFotografoPopupRequestEventArgs ) {

				SelezioneFotografoPopupRequestEventArgs popEventArgs = (SelezioneFotografoPopupRequestEventArgs)e;
				
				SelettoreFotografoPopup win = new SelettoreFotografoPopup();
				viewModel.selettoreFotografoViewModelFaccia.deselezionareTutto();

				// Questo è il viewmodel della finestra di popup				
				SelettoreFotografoPopupViewModel sfpViewModel = new SelettoreFotografoPopupViewModel();
				sfpViewModel.immagine = popEventArgs.fotoFaccia.imgProvino;
				win.DataContext = sfpViewModel;

				// Questo è il viewmodel del componente che deve selezionare un fotografo
				win.selettoreFotografoFaccia.DataContext = viewModel.selettoreFotografoViewModelFaccia;
				var esito = win.ShowDialog();

				if( esito == true ) {
					viewModel.associareFacciaFotografoCommand.Execute( null );
				}

				Console.WriteLine( esito );

				win.Close();
			}

		}

		SelettoreAzioneRapidaViewModel viewModel {
			get {
				return (SelettoreAzioneRapidaViewModel) this.DataContext;
			}
		}

	}
}
