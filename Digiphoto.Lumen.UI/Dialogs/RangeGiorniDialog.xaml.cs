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
using System.Collections;

namespace Digiphoto.Lumen.UI.Dialogs {
	/// <summary>
	/// Interaction logic for RangeGiorniDialog.xaml
	/// </summary>
	public partial class RangeGiorniDialog : Window {


		public DateTime giornoIniz { get; set; }
		public DateTime giornoFine { get; set; }

		public RangeGiorniDialog() {
			
			InitializeComponent();

			giornoIniz = DateTime.Today;
			giornoFine = DateTime.Today;

			calendario.SelectedDates.AddRange( giornoIniz, giornoFine );
		}

		private void calendario_SelectedDatesChanged( object sender, SelectionChangedEventArgs e ) {
			IList giorni = e.AddedItems;
			
			if( giorni.Count > 0 ) {

				// A seconda di come si esegue la selezione, il range può essere ascendente o discendente.
				// A me serve sempre prima la più piccola poi la più grande
				DateTime aa = (DateTime) giorni[0];
				DateTime bb = (DateTime) giorni[giorni.Count-1];

				// Metto sempre per prima la data più piccola
				giornoIniz = minDate( aa, bb );
				giornoFine = maxDate( aa, bb );
			}
		}

		public static DateTime minDate( DateTime aa, DateTime bb ) {
			return aa > bb ? bb : aa;
		}
		public static DateTime maxDate( DateTime aa, DateTime bb ) {
			return aa > bb ? aa : bb;
		}

		private void buttonOk_Click( object sender, RoutedEventArgs e ) {
			this.DialogResult = true;
			this.Hide();
		}

		private void buttonAnnulla_Click( object sender, RoutedEventArgs e ) {
			this.DialogResult = false;
			this.Hide();
		}


	}
}
