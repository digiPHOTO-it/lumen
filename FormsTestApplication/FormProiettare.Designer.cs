namespace FormsTestApplication {
	partial class FormProiettare {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if( disposing && (components != null) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.buttonOpenFile = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.buttonProietta = new System.Windows.Forms.Button();
			this.checkBoxAutoRotate = new System.Windows.Forms.CheckBox();
			this.checkBoxZoomToFit = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.pictureBoxDest = new System.Windows.Forms.PictureBox();
			this.pictureBoxSrc = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxDest)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxSrc)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 46);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Immagine";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(15, 62);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(71, 20);
			this.textBox1.TabIndex = 1;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// buttonOpenFile
			// 
			this.buttonOpenFile.Location = new System.Drawing.Point(85, 62);
			this.buttonOpenFile.Name = "buttonOpenFile";
			this.buttonOpenFile.Size = new System.Drawing.Size(30, 23);
			this.buttonOpenFile.TabIndex = 2;
			this.buttonOpenFile.Text = "...";
			this.buttonOpenFile.UseVisualStyleBackColor = true;
			this.buttonOpenFile.Click += new System.EventHandler(this.buttonOpenFile_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// buttonProietta
			// 
			this.buttonProietta.Location = new System.Drawing.Point(12, 2);
			this.buttonProietta.Name = "buttonProietta";
			this.buttonProietta.Size = new System.Drawing.Size(100, 33);
			this.buttonProietta.TabIndex = 7;
			this.buttonProietta.Text = "Proiettare";
			this.buttonProietta.UseVisualStyleBackColor = true;
			this.buttonProietta.Click += new System.EventHandler(this.buttonProietta_Click);
			// 
			// checkBoxAutoRotate
			// 
			this.checkBoxAutoRotate.AutoSize = true;
			this.checkBoxAutoRotate.Checked = true;
			this.checkBoxAutoRotate.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAutoRotate.Location = new System.Drawing.Point(15, 194);
			this.checkBoxAutoRotate.Name = "checkBoxAutoRotate";
			this.checkBoxAutoRotate.Size = new System.Drawing.Size(99, 17);
			this.checkBoxAutoRotate.TabIndex = 8;
			this.checkBoxAutoRotate.Text = "Auto Rotazione";
			this.checkBoxAutoRotate.UseVisualStyleBackColor = true;
			// 
			// checkBoxZoomToFit
			// 
			this.checkBoxZoomToFit.AutoSize = true;
			this.checkBoxZoomToFit.Checked = true;
			this.checkBoxZoomToFit.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxZoomToFit.Location = new System.Drawing.Point(15, 217);
			this.checkBoxZoomToFit.Name = "checkBoxZoomToFit";
			this.checkBoxZoomToFit.Size = new System.Drawing.Size(79, 17);
			this.checkBoxZoomToFit.TabIndex = 9;
			this.checkBoxZoomToFit.Text = "Zoom to Fit";
			this.checkBoxZoomToFit.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(9, 250);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 61);
			this.label2.TabIndex = 10;
			this.label2.Text = "Fare il resize della finestra per  modificare l\'area di destinazione";
			// 
			// pictureBoxDest
			// 
			this.pictureBoxDest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBoxDest.Location = new System.Drawing.Point(121, 2);
			this.pictureBoxDest.Name = "pictureBoxDest";
			this.pictureBoxDest.Size = new System.Drawing.Size(318, 347);
			this.pictureBoxDest.TabIndex = 4;
			this.pictureBoxDest.TabStop = false;
			// 
			// pictureBoxSrc
			// 
			this.pictureBoxSrc.Location = new System.Drawing.Point(15, 88);
			this.pictureBoxSrc.Name = "pictureBoxSrc";
			this.pictureBoxSrc.Size = new System.Drawing.Size(100, 100);
			this.pictureBoxSrc.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBoxSrc.TabIndex = 3;
			this.pictureBoxSrc.TabStop = false;
			// 
			// FormProiettare
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(719, 362);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.checkBoxZoomToFit);
			this.Controls.Add(this.checkBoxAutoRotate);
			this.Controls.Add(this.buttonProietta);
			this.Controls.Add(this.pictureBoxDest);
			this.Controls.Add(this.pictureBoxSrc);
			this.Controls.Add(this.buttonOpenFile);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Name = "FormProiettare";
			this.Text = "FormProiettare";
			this.Load += new System.EventHandler(this.FormProiettare_Load);
			this.ResizeEnd += new System.EventHandler(this.FormProiettare_ResizeEnd);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxDest)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxSrc)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button buttonOpenFile;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.PictureBox pictureBoxSrc;
		private System.Windows.Forms.PictureBox pictureBoxDest;
		private System.Windows.Forms.Button buttonProietta;
		private System.Windows.Forms.CheckBox checkBoxAutoRotate;
		private System.Windows.Forms.CheckBox checkBoxZoomToFit;
		private System.Windows.Forms.Label label2;
	}
}