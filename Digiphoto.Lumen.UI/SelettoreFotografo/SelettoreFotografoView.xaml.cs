using System.Windows;
using System.Windows.Controls;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;

namespace Digiphoto.Lumen.UI {

	public partial class SelettoreFotografo : UserControlBase {

		public SelettoreFotografo() {

			InitializeComponent();
		}

		#region Proprietà

#if CREDOCHENONSERVA
		public Fotografo fotografoSelezionato {
			get {
				return sceltafotografoViewModel.fotografoSelezionato;
			}
		}
#endif

		private SelettoreFotografoViewModel sceltafotografoViewModel {
			get {
				return (SelettoreFotografoViewModel)base.viewModelBase;
			}
		}

		#endregion

		#region Dependency Property

		public static readonly DependencyProperty possoCreareProperty = DependencyProperty.Register( "possoCreare", typeof( bool ),	typeof( SelettoreFotografo ), new FrameworkPropertyMetadata( true,	FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

		public bool possoCreare {
			get {
				return (bool)GetValue( possoCreareProperty );
			}
			set {
				SetValue( possoCreareProperty, value );
			}
		}


		public static readonly DependencyProperty selezioneMultiplaProperty = DependencyProperty.Register( "selezioneMultipla", typeof( bool ), typeof( SelettoreFotografo ), new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.None ) );

		public bool selezioneMultipla
		{
			get
			{
				return (bool)GetValue( selezioneMultiplaProperty );
			}
			set
			{
				SetValue( selezioneMultiplaProperty, value );
			}
		}
		#endregion Dependency Property


		/**
		 * La selezione singola non funziona:
		 * esempio:
		 * 
		 * https://stackoverflow.com/questions/4184243/wpf-listbox-selection-does-not-work 
		 * https://stackoverflow.com/questions/21748150/listbox-selectionchanged-not-getting-called
		 * 
		 * Allora sono costretto a gestire "manualmente" questa situazione.
		 * Invece la selezione multipla senmbra funzionare
		 */
		private void fotografiListBox_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
		
			ListBox listBox = (ListBox)sender;
			if( listBox.SelectionMode == SelectionMode.Single ) {

				MultiSelectCollectionView<Fotografo> cw = (MultiSelectCollectionView<Fotografo>)listBox.ItemsSource;

				foreach( var obj in e.RemovedItems )
					cw.deseleziona( (Fotografo)obj );

				foreach( var obj in e.AddedItems )
					cw.seleziona( (Fotografo)obj );

				if( cw.SelectedItems.Count > 1 )
					if( System.Diagnostics.Debugger.IsAttached )
						System.Diagnostics.Debugger.Break();
			}
		}

	}
}
