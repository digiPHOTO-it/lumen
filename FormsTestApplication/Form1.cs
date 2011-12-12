using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FormsTestApplication {
	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
		}

		private void buttonTestProiettare_Click( object sender, EventArgs e ) {

			using( FormProiettare f = new FormProiettare() ) {
				f.ShowDialog();
			}
			
		}

        private void button1_Click(object sender, EventArgs e)
        {
            using (FormModificaConnectionString f = new FormModificaConnectionString())
            {
                f.ShowDialog();
            }
        }

        private void sfoglia_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FormSQLite f = new FormSQLite())
            {
                f.ShowDialog();
            }
        }
	}
}
