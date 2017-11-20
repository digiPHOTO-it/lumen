using Digiphoto.Lumen.UI.Mvvm;
using System.Windows;
using System;

namespace Digiphoto.Lumen.UI.Main {

	/// <summary>
	/// Interaction logic for DbRebuilderWindow.xaml
	/// </summary>
	public partial class DbRebuilderWiew : WindowBase {

		public DbRebuilderWiew() {

			InitializeComponent();

			this.DataContextChanged += DbRebuilderWiew_DataContextChanged;
		}

		private void DbRebuilderWiew_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();
		}

		DbRebuilderViewModel viewModel {
			get {
				return (DbRebuilderViewModel) this.DataContext;
			}
		}

	}
}
