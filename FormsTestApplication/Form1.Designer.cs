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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonTestProiettare
            // 
            this.buttonTestProiettare.Location = new System.Drawing.Point(21, 24);
            this.buttonTestProiettare.Name = "buttonTestProiettare";
            this.buttonTestProiettare.Size = new System.Drawing.Size(63, 21);
            this.buttonTestProiettare.TabIndex = 0;
            this.buttonTestProiettare.Text = "Proiettare Aree";
            this.buttonTestProiettare.UseVisualStyleBackColor = true;
            this.buttonTestProiettare.Click += new System.EventHandler(this.buttonTestProiettare_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(21, 107);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(190, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Modifica Connection String";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(21, 64);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Test SQLite";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 224);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonTestProiettare);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Button buttonTestProiettare;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
	}
}

