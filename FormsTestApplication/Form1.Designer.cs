namespace FormsTestApplication {
	partial class Form1 {
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
			this.buttonTestProiettare = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonTestProiettare
			// 
			this.buttonTestProiettare.Location = new System.Drawing.Point(41, 54);
			this.buttonTestProiettare.Name = "buttonTestProiettare";
			this.buttonTestProiettare.Size = new System.Drawing.Size(63, 21);
			this.buttonTestProiettare.TabIndex = 0;
			this.buttonTestProiettare.Text = "Proiettare Aree";
			this.buttonTestProiettare.UseVisualStyleBackColor = true;
			this.buttonTestProiettare.Click += new System.EventHandler(this.buttonTestProiettare_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.buttonTestProiettare);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonTestProiettare;
	}
}

