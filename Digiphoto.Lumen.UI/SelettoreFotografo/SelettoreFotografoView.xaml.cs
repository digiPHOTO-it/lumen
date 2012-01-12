using System.Windows;
using System.Windows.Controls;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.UI {

	public partial class SelettoreFotografo : UserControl {

		private SelettoreFotografoViewModel _sceltafotografoViewModel;

		public SelettoreFotografo() {

			InitializeComponent();

			_sceltafotografoViewModel = new SelettoreFotografoViewModel();
			DataContext = _sceltafotografoViewModel;

			// Mi aggancio all'evento di elemento selezionato della listbox.
			this.fotografiListBox.SelectionChanged += new SelectionChangedEventHandler( onSelectionChanged );
		}

		private void onSelectionChanged( object sender, SelectionChangedEventArgs e ) {
			e.Handled = true;  // evita il bubble
			RoutedEventArgs args = new RoutedEventArgs( fotografoChangedEvent );
			RaiseEvent( args );
		}

		#region fotografoSelezionato Dependency Property
		public static readonly DependencyProperty fotografoSelezionatoProperty = DependencyProperty.Register("fotografoSelezionato", typeof(Fotografo), typeof(SelettoreFotografo));

		// wrapper .net opzionale
		public Fotografo fotografoSelezionato {
			get { 
				return (Fotografo)GetValue(fotografoSelezionatoProperty); 
			}
			set { 
				SetValue(fotografoSelezionatoProperty, value); 
			}
		}
		#endregion


		#region evento fotografoSelezionato cambiato

		public static readonly RoutedEvent fotografoChangedEvent = EventManager.RegisterRoutedEvent("fotografoChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SelettoreFotografo));

		public event RoutedEventHandler fotografoChanged {
			add { AddHandler(fotografoChangedEvent, value); }
			remove { RemoveHandler(fotografoChangedEvent, value); }
		}

		#endregion


		#region possoCreare Dependency Property
		public static readonly DependencyProperty possoCreareProperty = DependencyProperty.Register( "possoCreare", typeof( bool ),	typeof( SelettoreFotografo ), new FrameworkPropertyMetadata( true,	FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

		public bool possoCreare {
			get {
				return (bool)GetValue( possoCreareProperty );
			}
			set {
				SetValue( possoCreareProperty, value );
			}
		}
		#endregion

	}
}
