using Digiphoto.Lumen.Servizi.Stampare;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.UI {

	public partial class SelettoreAzioniRapideResources {

		/// <summary>
		/// Quando viene disegnato il menu contestuale per la prima volta,
		/// devo fare una operazione che da XAML non si riesce.
		/// Ovvero devo creare le voci di menu nel menu "padre" rispetto al sottomenu che si riesce a creare
		/// nello xaml.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void onMenuItemStampePieneUpdated( object sender, DataTransferEventArgs args ) {

			MenuItem menuItemStampePiene = (MenuItem) sender;
			MenuItem menuItemSingolaFoto = (MenuItem) menuItemStampePiene.Parent;
			
			if( menuItemStampePiene.HasItems ) {
				int conta = 0;
				foreach( var item in menuItemStampePiene.Items ) {

					MenuItem newItem = new MenuItem();
					SelettoreAzioniRapideViewModel vm = (SelettoreAzioniRapideViewModel) menuItemStampePiene.DataContext;
					newItem.DataContext = vm;

					StampanteAbbinata cartaStampabile = (StampanteAbbinata)item;

					// bindo il comando di stampa
					newItem.Command = vm.stampaRapidaCommand;
					newItem.CommandParameter = cartaStampabile;

//					newItem.ItemTemplate = menuItemStampePiene.ItemTemplate;

					newItem.Header = cartaStampabile.ToString();

					Uri uri = new Uri( "/Resources/Printer-16x16.ico", UriKind.Relative );
					newItem.Icon = new System.Windows.Controls.Image {
						Source = new BitmapImage( uri ),
						ToolTip = "Stampa immediata a formato pieno"
					};
					

					( (MenuItem)menuItemStampePiene.Parent).Items.Insert( conta++, newItem );
				}

				menuItemSingolaFoto.Items.Remove( menuItemStampePiene );
			}

		}

#if DACANCELLARENONSERVE
		private MenuItem findMenuFiglio( ItemsControl menu, string nome ) {

			if( menu.HasItems ) {
				foreach( var item in menu.Items ) {
					if( item is MenuItem ) {  // ci sono anche i separator
						var menuItem = (MenuItem)item;
						if( menuItem.Name == nome )
							return menuItem;
						else {
							var test = findMenuFiglio( menuItem, nome );
							if( test != null )
								return test;
						}
					}
				}
			} 
			
			return null;
		}
#endif
	}
}
