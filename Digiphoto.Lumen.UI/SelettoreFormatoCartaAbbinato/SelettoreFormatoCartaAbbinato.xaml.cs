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

namespace Digiphoto.Lumen.UI
{
    /// <summary>
    /// Interaction logic for SelettoreFormatoCartaAbbinatoView.xaml
    /// </summary>
    public partial class SelettoreFormatoCartaAbbinatoView : UserControl
    {
        static DependencyProperty selectedIndexProperty = null;

        public SelettoreFormatoCartaAbbinatoView()
        {
            InitializeComponent();
            selectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SelettoreFormatoCartaAbbinatoView), new PropertyMetadata(new PropertyChangedCallback(OnIsFocusedPropertyChanged)));
        }

        public int SelectedIndex
        {
            get
            {
                return (int)GetValue(selectedIndexProperty);
            }
            set
            {
                SetValue(selectedIndexProperty, value);
            }
        }

        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBox.Show("CIAO");
        }
    }
}
