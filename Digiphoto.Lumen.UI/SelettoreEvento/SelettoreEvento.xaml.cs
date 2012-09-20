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
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm;
using System.ComponentModel;

namespace Digiphoto.Lumen.UI {
	/// <summary>
	/// Interaction logic for SelettoreEvento.xaml
	/// </summary>
	public partial class SelettoreEvento : UserControlBase {

		public SelettoreEvento() {
			InitializeComponent();
		}

		private SelettoreEventoViewModel selettoreEventoViewModel {
			get {
				return (SelettoreEventoViewModel)base.viewModelBase;
			}
		}
	
		public Evento eventoSelezionato {
			get {
				return selettoreEventoViewModel.eventoSelezionato;
			}
		}

		#region possoCreare Dependency Property
		public static readonly DependencyProperty possoCreareProperty = DependencyProperty.Register( "possoCreare", typeof( bool ), typeof( SelettoreEvento ), new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

		public bool possoCreare {
			get {
				return (bool)GetValue( possoCreareProperty );
			}
			set {
				SetValue( possoCreareProperty, value );
			}
		}

		public static readonly DependencyProperty selectedIndexProperty = DependencyProperty.Register("MySelectedItem",
																			typeof(Evento),
																			typeof(SelettoreEvento),
																			 new FrameworkPropertyMetadata(
		 new PropertyChangedCallback(ChangeSelectedItem)));


		private static void ChangeSelectedItem(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			(source as SelettoreEvento).UpdateSelectedItem((Evento)e.NewValue);
		}

		private void UpdateSelectedItem(Evento NewText)
		{
			eventiListBox.SelectedItem = NewText;
		}

		[Description("MySelectedItem")]
		public Evento MySelectedItem
		{
			get
			{
				return (Evento)GetValue(selectedIndexProperty);
			}
			set
			{
				SetValue(selectedIndexProperty, value);
			}
		}

		#endregion
	}
}
