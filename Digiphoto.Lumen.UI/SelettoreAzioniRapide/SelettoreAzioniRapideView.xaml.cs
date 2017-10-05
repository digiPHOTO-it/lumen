using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Digiphoto.Lumen.UI.Mvvm;
using System.Collections;

namespace Digiphoto.Lumen.UI.SelettoreAzioniRapide {

	/// <summary>
	/// Interaction logic for AzioniRapideView.xaml
	/// </summary>
	public partial class SelettoreAzioniRapide : UserControlBase
	{
		public SelettoreAzioniRapide()
		{
			InitializeComponent();

			DataContextChanged += selettoreAzioniRapideView_DataContextChanged;
		}

		void selettoreAzioniRapideView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			associaDialogProvider();

			// Devo anche gestire la popup per associare la faccia del fotografo
			if( this.DataContext is SelettoreAzioniRapideViewModel )
				viewModel.openPopupDialogRequest += viewModel_openPopupDialogRequest;
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

		SelettoreAzioniRapideViewModel viewModel {
			get {
				return (SelettoreAzioniRapideViewModel) this.DataContext;
			}
		}

	}
}
