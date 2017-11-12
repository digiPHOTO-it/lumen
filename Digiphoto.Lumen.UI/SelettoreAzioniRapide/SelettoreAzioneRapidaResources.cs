using Digiphoto.Lumen.Servizi.Stampare;
using log4net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.UI.SelettoreAzioniRapide {

	public partial class SelettoreAzioneRapidaResources {

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( SelettoreAzioneRapidaResources ) );
		
		private SelettoreAzioneRapidaViewModel azioniRapideViewModel {
			get;
			set;
		}


		public void azioniRapideMenu_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {

			// Questa operazione non dovrebbe servire
			ContextMenu menu = (ContextMenu)sender;
			azioniRapideViewModel = (SelettoreAzioneRapidaViewModel)menu.DataContext;
			if( azioniRapideViewModel == null ) {

				_giornale.Warn( "DataContext nullo" );

#if DEBUG
				if( System.Diagnostics.Debugger.IsAttached )
					System.Diagnostics.Debugger.Break();
#endif
			}
		}

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
			SelettoreAzioneRapidaViewModel vm = (SelettoreAzioneRapidaViewModel)menuItemStampePiene.DataContext;
			
			if( vm == null ) {
				// Questa cosa non ha senso, perché il VM dovrebbe essere sempre valorizzato

#if DEBUG
				if( System.Diagnostics.Debugger.IsAttached )
					System.Diagnostics.Debugger.Break();
#endif
				vm = azioniRapideViewModel;

			}

			if( vm == null )
				_giornale.Warn( "data context vuoto per menu contestuale. Come mai ?" );
			else {
				if( menuItemStampePiene.HasItems ) {
					int conta = 0;
					foreach( var item in menuItemStampePiene.Items ) {

						// Creo una nuova voce di menu
						MenuItem newItem = new MenuItem();

						newItem.DataContext = vm;

						StampanteAbbinata cartaStampabile = (StampanteAbbinata)item;

						// bindo il comando di stampa
						newItem.Command = vm.stampaRapidaCommand;
						newItem.CommandParameter = cartaStampabile;

						newItem.Header = cartaStampabile.ToString();

						Uri uri = new Uri( "/Resources/Printer-16x16.ico", UriKind.Relative );
						newItem.Icon = new System.Windows.Controls.Image {
							Source = new BitmapImage( uri ),
							ToolTip = "Stampa immediata a formato pieno"
						};

						// Inserisco l'elemento appena creato, nel menu superiore.
						((MenuItem)menuItemStampePiene.Parent).Items.Insert( conta++, newItem );
					}

					// Ora che ho aggiunto tutte le voci al menu superiore, rimuovo il sottomenu che risulterebbe un doppione.	
					menuItemSingolaFoto.Items.Remove( menuItemStampePiene );
				}
			}
		}

		private void onSubmenuOpened( object sender, RoutedEventArgs e ) {

			MenuItem curr = (MenuItem)sender;
			SelettoreAzioneRapidaViewModel vm = (SelettoreAzioneRapidaViewModel)curr.DataContext;
			if( vm != null )
				vm.setTarget( (string)curr.Tag );
			else {
				// Impossibile !!!     QUI NON DOVREBBE MAI CADERE (invece succede)
				// Non so perché ma il menu contestuale che appare con il tasto destro, a volte
				// perde il datacontext e quindi non sono più in grado di eseguire l'azione corrispondente.
				// In questo caso, occorre uscire dal programma
				_giornale.Error( "ContextMenu ha perso il DataContext. Non riesco bindare il datacontext" );
			}		
		}

	}
}
