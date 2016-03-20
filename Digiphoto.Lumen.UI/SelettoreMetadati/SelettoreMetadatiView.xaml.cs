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
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;

namespace Digiphoto.Lumen.UI
{
	/// <summary>
	/// Interaction logic for SelettoreMetadatiView.xaml
	/// </summary>
	public partial class SelettoreMetadati : UserControlBase
	{

		public static readonly DependencyProperty MyItemsSourceProperty;


        public SelettoreMetadati()
        {
            InitializeComponent();

            this.DataContext = new SelettoreMetadatiViewModel(this); ;
        }

        static SelettoreMetadati()
        {
            SelettoreMetadati.MyItemsSourceProperty = DependencyProperty.Register("MyItemsSource",
                                                                            typeof(IEnumerable),
                                                                            typeof(SelettoreMetadati),
                                                                            new PropertyMetadata(new PropertyChangedCallback(MyItemSourcePropertyCallback)));
        }

        public IEnumerable MyItemsSource
        {
            get
            {
				return (IEnumerable)GetValue(SelettoreMetadati.MyItemsSourceProperty);
            }

            set
            {
				SetValue(SelettoreMetadati.MyItemsSourceProperty, value);
            }
        }

        private static void MyItemSourcePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uc = d as SelettoreMetadati;
            ((SelettoreMetadatiViewModel)uc.viewModelBase).fotografieMCW = e.NewValue as MultiSelectCollectionView<Fotografia>;
        }

		/// <summary>
		/// Quando spengo la checkbox, spengo la voce selezionata
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void checkBoxEventi_Unchecked( object sender, RoutedEventArgs e ) {
			
			((SelettoreMetadatiViewModel)this.DataContext).selettoreEventoMetadato.eventoSelezionato = null;
			
		}

		private void checkBoxFasidelGiorno_Unchecked( object sender, RoutedEventArgs e ) {
			fasiDelGiorno.SelectedItem = null;
		}

		private void checkDidascalia_Unchecked( object sender, RoutedEventArgs e ) {
			didascalia.Text = null;
		}
	}
}
