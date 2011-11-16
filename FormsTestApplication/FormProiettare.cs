using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Digiphoto.Lumen.Imaging;
using System.Drawing.Drawing2D;

namespace FormsTestApplication {
	public partial class FormProiettare : Form {

	
		public FormProiettare() {
			InitializeComponent();
		}

		private void buttonOpenFile_Click( object sender, EventArgs e ) {
			openFileDialog1.FileName = textBox1.Text;
			if( openFileDialog1.ShowDialog() == DialogResult.OK ) {
				textBox1.Text = openFileDialog1.FileName;
			}
		}

		private void textBox1_TextChanged( object sender, EventArgs e ) {
			
			// Carico l'immagine sorgente
			pictureBoxSrc.Image = new Bitmap( textBox1.Text );


		}

		private void buttonProietta_Click( object sender, EventArgs e ) {

			Rectangle rDest = new Rectangle( 0, 0, pictureBoxDest.Width, pictureBoxDest.Height );
			Rectangle rSorg = new Rectangle( 0, 0, pictureBoxSrc.Image.Width, pictureBoxSrc.Image.Height );

			ProiettoreArea p = new ProiettoreArea( rDest );
			p.autoRotate = checkBoxAutoRotate.Checked;
			p.autoZoomToFit = checkBoxZoomToFit.Checked;
			Proiezione esito = p.calcola( rSorg );

			// Adesso disegno il risultato: Mi tengo due pixel per il bordo
			Bitmap nuova = new Bitmap( pictureBoxDest.Width-2 , pictureBoxDest.Height-2 );

			using( Graphics g = pictureBoxDest.CreateGraphics() ) {
				g.Clear( Color.Cyan );

				Image src = (Image) pictureBoxSrc.Image.Clone();
				Rectangle rectSrc = esito.sorg;
				if( esito.effettuataRotazione ) {
					src.RotateFlip( RotateFlipType.Rotate90FlipXY );
					rectSrc = ProiettoreArea.ruota( rectSrc );
				}
				g.DrawImage( src, esito.dest, rectSrc, GraphicsUnit.Pixel );



				Pen pennaRossa = new Pen( Color.Red );
				g.DrawRectangle( pennaRossa, esito.dest );
			}


		}

		private void FormProiettare_Load( object sender, EventArgs e ) {
			FormProiettare_ResizeEnd( sender, e );
		}

		private void FormProiettare_ResizeEnd( object sender, EventArgs e ) {
			pictureBoxDest.Width = this.Width - pictureBoxDest.Left - 20;
			pictureBoxDest.Height = this.Height - pictureBoxDest.Top - 50;
		}
	}
}
