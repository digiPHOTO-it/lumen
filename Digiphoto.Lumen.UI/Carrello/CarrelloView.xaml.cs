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
using System.Collections;

namespace Digiphoto.Lumen.UI
{
    /// <summary>
    /// Interaction logic for Carrello.xaml
    /// </summary>
    public partial class CarrelloView : UserControlBase
    {
        public CarrelloView() 
        {
            InitializeComponent();
        }

		private CarrelloViewModel carrelloViewModel
		{
			get
			{
				return (CarrelloViewModel)base.viewModelBase;
			}
		}

		private void oggiButton_Click(object sender, RoutedEventArgs e)
		{
			calendario.SelectedDates.Clear();
			calendario.SelectedDates.AddRange(carrelloViewModel.oggi, carrelloViewModel.oggi);
		}

		private void ieriButton_Click(object sender, RoutedEventArgs e)
		{
			calendario.SelectedDates.Clear();
			TimeSpan unGiorno = new TimeSpan(1, 0, 0, 0);
			DateTime ieri = carrelloViewModel.oggi.Subtract(unGiorno);
			calendario.SelectedDates.AddRange(ieri, ieri);
		}

		private void ieriOggiButton_Click(object sender, RoutedEventArgs e)
		{
			calendario.SelectedDates.Clear();
			TimeSpan unGiorno = new TimeSpan(1, 0, 0, 0);
			DateTime ieri = carrelloViewModel.oggi.Subtract(unGiorno);
			calendario.SelectedDates.AddRange(ieri, carrelloViewModel.oggi);
		}

		private void calendario_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
		{
			IList giorni = e.AddedItems;

			if (giorni.Count > 0)
			{

				// A seconda di come si esegue la selezione, il range può essere ascendente o discendente.
				// A me serve sempre prima la più piccola poi la più grande
				DateTime aa = (DateTime)giorni[0];
				DateTime bb = (DateTime)giorni[giorni.Count - 1];

				// Metto sempre per prima la data più piccola
				carrelloViewModel.paramCercaCarrello.giornataIniz = minDate(aa, bb);
				carrelloViewModel.paramCercaCarrello.giornataFine = maxDate(aa, bb);
			}
		}

		public static DateTime minDate(DateTime aa, DateTime bb)
		{
			return aa > bb ? bb : aa;
		}
		public static DateTime maxDate(DateTime aa, DateTime bb)
		{
			return aa > bb ? aa : bb;
		}
    }
}
