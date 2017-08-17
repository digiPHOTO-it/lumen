using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Digiphoto.Lumen.UI.Pubblico {
	/// <summary>
	/// Interaction logic for PubblicoWindow.xaml
	/// </summary>
	public partial class PubblicoWindow : Window {

		GalleryUIRispetto galleryUICommon;

		public PubblicoWindow() {

			InitializeComponent();

			if( String.IsNullOrEmpty( Configurazione.infoFissa.descrizPuntoVendita ) )
				this.Title = "Photo Gallery - digiPHOTO Lumen";
			else
				this.Title = "Photo Gallery - " + Configurazione.infoFissa.descrizPuntoVendita;

			this.galleryVuotaText.Text = "digiPHOTO.it\r\nLumen";

			this.DataContextChanged += PubblicoWindow_DataContextChanged;

			this.galleryItemsControl.SizeChanged += galleryItemsControl_SizeChanged;
		}

		private void galleryItemsControl_SizeChanged( object sender, SizeChangedEventArgs e ) {
			// Se la dimensione della finestra è cambiata devo riposizionare i controlli
			if( fotoGalleryViewModel != null && fotoGalleryViewModel.isAltaQualita )
				galleryUICommon.gestioneAreaStampabileHQ();
		}

		private void PubblicoWindow_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {

			// Creo la classe che gestisce le aree di rispetto per ritaglio
			galleryUICommon = new GalleryUIRispetto( galleryItemsControl, this );

			// Purtroppo qui devo sempre ascoltare perché non ho indicazione sulla checkbox indicata dall'utente nella gallery
			galleryUICommon.ascolta( true );
		}

		FotoGalleryViewModel fotoGalleryViewModel {
			get {
				return (FotoGalleryViewModel) this.DataContext;
			}
		}

		protected override void OnClosing( CancelEventArgs e ) {

			if( fotoGalleryViewModel != null ) {
				// rilascio eventuali componenti aggiunti durante l'ascolto
				galleryUICommon.gestioneAreaStampabileHQ( true );
				galleryUICommon.ascolta( false );
			}

			base.OnClosing( e );
		}

	}
}
