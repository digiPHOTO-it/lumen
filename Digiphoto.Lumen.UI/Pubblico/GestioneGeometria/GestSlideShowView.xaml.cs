using System;
using System.Windows;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.UI.Mvvm;
using System.Diagnostics;

namespace Digiphoto.Lumen.UI.Pubblico {
	/// <summary>
	/// Interaction logic for GestSlideShowView.xaml
	/// </summary>
	public partial class GestSlideShowView : UserControlBase
	{

		private UserConfigLumen cfg = Configurazione.UserConfigLumen;

		public GestSlideShowView()
		{
			InitializeComponent();
		}

		#region Proprieta

		private GestSlideShowViewModel gestSlideShowViewModel
		{
			get
			{
				return (GestSlideShowViewModel)base.viewModelBase;
			}
		}

		#endregion

		

	}
}
