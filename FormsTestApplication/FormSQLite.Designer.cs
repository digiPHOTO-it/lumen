namespace FormsTestApplication
{
    partial class FormSQLite
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.descrizioneField = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.providerConnectionStringLabel = new System.Windows.Forms.MaskedTextBox();
            this.connect = new System.Windows.Forms.Button();
            this.close = new System.Windows.Forms.Button();
            this.timeWriteText = new System.Windows.Forms.TextBox();
            this.timeLoadText = new System.Windows.Forms.TextBox();
            this.write = new System.Windows.Forms.Button();
            this.load = new System.Windows.Forms.Button();
            this.sfoglia = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.percorsoDataBaseField = new System.Windows.Forms.TextBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(233, 211);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Descrizione";
            // 
            // descrizioneField
            // 
            this.descrizioneField.Location = new System.Drawing.Point(317, 204);
            this.descrizioneField.Name = "descrizioneField";
            this.descrizioneField.Size = new System.Drawing.Size(203, 20);
            this.descrizioneField.TabIndex = 28;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 13);
            this.label2.TabIndex = 27;
            this.label2.Text = "Provider Connection String ";
            // 
            // providerConnectionStringLabel
            // 
            this.providerConnectionStringLabel.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.providerConnectionStringLabel.Location = new System.Drawing.Point(26, 80);
            this.providerConnectionStringLabel.Name = "providerConnectionStringLabel";
            this.providerConnectionStringLabel.Size = new System.Drawing.Size(494, 20);
            this.providerConnectionStringLabel.TabIndex = 26;
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(26, 125);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(75, 23);
            this.connect.TabIndex = 25;
            this.connect.Text = "Connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(107, 125);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(80, 23);
            this.close.TabIndex = 24;
            this.close.Text = "Close";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // timeWriteText
            // 
            this.timeWriteText.Location = new System.Drawing.Point(125, 205);
            this.timeWriteText.Name = "timeWriteText";
            this.timeWriteText.Size = new System.Drawing.Size(100, 20);
            this.timeWriteText.TabIndex = 23;
            // 
            // timeLoadText
            // 
            this.timeLoadText.Location = new System.Drawing.Point(125, 176);
            this.timeLoadText.Name = "timeLoadText";
            this.timeLoadText.Size = new System.Drawing.Size(100, 20);
            this.timeLoadText.TabIndex = 22;
            // 
            // write
            // 
            this.write.Location = new System.Drawing.Point(26, 202);
            this.write.Name = "write";
            this.write.Size = new System.Drawing.Size(75, 23);
            this.write.TabIndex = 21;
            this.write.Text = "Write";
            this.write.UseVisualStyleBackColor = true;
            this.write.Click += new System.EventHandler(this.write_Click);
            // 
            // load
            // 
            this.load.Location = new System.Drawing.Point(26, 173);
            this.load.Name = "load";
            this.load.Size = new System.Drawing.Size(75, 23);
            this.load.TabIndex = 20;
            this.load.Text = "Load";
            this.load.UseVisualStyleBackColor = true;
            this.load.Click += new System.EventHandler(this.load_Click);
            // 
            // sfoglia
            // 
            this.sfoglia.Location = new System.Drawing.Point(450, 29);
            this.sfoglia.Name = "sfoglia";
            this.sfoglia.Size = new System.Drawing.Size(75, 23);
            this.sfoglia.TabIndex = 19;
            this.sfoglia.Text = "Sfoglia";
            this.sfoglia.UseVisualStyleBackColor = true;
            this.sfoglia.Click += new System.EventHandler(this.sfoglia_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Percorso DataBase";
            // 
            // percorsoDataBaseField
            // 
            this.percorsoDataBaseField.Location = new System.Drawing.Point(26, 29);
            this.percorsoDataBaseField.Name = "percorsoDataBaseField";
            this.percorsoDataBaseField.Size = new System.Drawing.Size(418, 20);
            this.percorsoDataBaseField.TabIndex = 17;
            // 
            // dataGridView
            // 
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new System.Drawing.Point(26, 243);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(494, 150);
            this.dataGridView.TabIndex = 16;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "(*.sqlite)|*.sqlite|All Files (*.*)|*.*";
            // 
            // FormSQLite
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 409);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.descrizioneField);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.providerConnectionStringLabel);
            this.Controls.Add(this.connect);
            this.Controls.Add(this.close);
            this.Controls.Add(this.timeWriteText);
            this.Controls.Add(this.timeLoadText);
            this.Controls.Add(this.write);
            this.Controls.Add(this.load);
            this.Controls.Add(this.sfoglia);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.percorsoDataBaseField);
            this.Controls.Add(this.dataGridView);
            this.Name = "FormSQLite";
            this.Text = "FormSQLite";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox descrizioneField;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox providerConnectionStringLabel;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.Button close;
        private System.Windows.Forms.TextBox timeWriteText;
        private System.Windows.Forms.TextBox timeLoadText;
        private System.Windows.Forms.Button write;
        private System.Windows.Forms.Button load;
        private System.Windows.Forms.Button sfoglia;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox percorsoDataBaseField;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}