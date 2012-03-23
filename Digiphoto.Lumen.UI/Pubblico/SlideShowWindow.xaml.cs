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
using System.Windows.Shapes;

namespace Digiphoto.Lumen.UI.Pubblico {
	/// <summary>
	/// Interaction logic for SlideShowWindow.xaml
	/// </summary>
	public partial class SlideShowWindow : Window {

		SlideShowViewModel _slideShowViewModel;

		public SlideShowWindow() {

			InitializeComponent();

if( 1 == 0 ) {  // TODO
			// creo ed associo il datacontext
			_slideShowViewModel = new SlideShowViewModel();
			this.DataContext = _slideShowViewModel;
}
		}

		protected override void OnClosed( EventArgs e ) {

			_slideShowViewModel.Dispose();
			base.OnClosed( e );
		}
	}
}
