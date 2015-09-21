using System;
using System.Collections;
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
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;

namespace Digiphoto.Lumen.UI
{
    /// <summary>
	/// Interaction logic for SelettoreAzioniAutomatiche.xaml
    /// </summary>
    public partial class SelettoreAzioniAutomatiche : UserControlBase
    {
		public static readonly DependencyProperty MyItemsSourceProperty;

		public SelettoreAzioniAutomatiche()
		{
			InitializeComponent();
			this.DataContext = new SelettoreAzioniAutomaticheViewModel();
		}

		static SelettoreAzioniAutomatiche()
        {
			SelettoreAzioniAutomatiche.MyItemsSourceProperty = DependencyProperty.Register("MyItemsSource",
																			typeof(IEnumerable),
																			typeof(SelettoreAzioniAutomatiche),
																			new PropertyMetadata(new PropertyChangedCallback(MyItemSourcePropertyCallback)));
        }

        public IEnumerable MyItemsSource
        {
            get
            {
				return (IEnumerable)GetValue(SelettoreAzioniAutomatiche.MyItemsSourceProperty);
            }

            set
            {
				SetValue(SelettoreAzioniAutomatiche.MyItemsSourceProperty, value);
            }
        }

		private static void MyItemSourcePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var uc = d as SelettoreAzioniAutomatiche;
			((SelettoreAzioniAutomaticheViewModel)uc.viewModelBase).fotografieMCW = e.NewValue as MultiSelectCollectionView<Fotografia>;
		}
    }
}
