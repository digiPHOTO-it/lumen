namespace FormsTestApplication
{
    partial class FormModificaConnectionString
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
            this.entityNameField = new System.Windows.Forms.MaskedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.modifica = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.configTextArea = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.sfoglia = new System.Windows.Forms.Button();
            this.percorsoFileConfigField = new System.Windows.Forms.MaskedTextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // entityNameField
            // 
            this.entityNameField.ForeColor = System.Drawing.SystemColors.WindowText;
            this.entityNameField.HidePromptOnLeave = true;
            this.entityNameField.Location = new System.Drawing.Point(21, 30);
            this.entityNameField.Name = "entityNameField";
            this.entityNameField.Size = new System.Drawing.Size(531, 20);
            this.entityNameField.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Entity Name";
            // 
            // modifica
            // 
            this.modifica.Location = new System.Drawing.Point(571, 139);
            this.modifica.Name = "modifica";
            this.modifica.Size = new System.Drawing.Size(75, 23);
            this.modifica.TabIndex = 13;
            this.modifica.Text = "Modifica";
            this.modifica.UseVisualStyleBackColor = true;
            this.modifica.Click += new System.EventHandler(this.modifica_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 125);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Config Text";
            // 
            // configTextArea
            // 
            this.configTextArea.Location = new System.Drawing.Point(21, 141);
            this.configTextArea.Name = "configTextArea";
            this.configTextArea.Size = new System.Drawing.Size(531, 159);
            this.configTextArea.TabIndex = 11;
            this.configTextArea.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Percorso File Config";
            // 
            // sfoglia
            // 
            this.sfoglia.Location = new System.Drawing.Point(568, 84);
            this.sfoglia.Name = "sfoglia";
            this.sfoglia.Size = new System.Drawing.Size(75, 23);
            this.sfoglia.TabIndex = 9;
            this.sfoglia.Text = "Sfoglia";
            this.sfoglia.UseVisualStyleBackColor = true;
            this.sfoglia.Click += new System.EventHandler(this.sfoglia_Click);
            // 
            // percorsoFileConfigField
            // 
            this.percorsoFileConfigField.Location = new System.Drawing.Point(18, 88);
            this.percorsoFileConfigField.Name = "percorsoFileConfigField";
            this.percorsoFileConfigField.Size = new System.Drawing.Size(534, 20);
            this.percorsoFileConfigField.TabIndex = 8;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "(*.config)|*.config|All Files (*.*)|*.*";
            // 
            // FormModificaConnectionString
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 314);
            this.Controls.Add(this.entityNameField);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.modifica);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.configTextArea);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sfoglia);
            this.Controls.Add(this.percorsoFileConfigField);
            this.Name = "FormModificaConnectionString";
            this.Text = "ModificaConnectionString";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MaskedTextBox entityNameField;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button modifica;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox configTextArea;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button sfoglia;
        private System.Windows.Forms.MaskedTextBox percorsoFileConfigField;
        private System.Windows.Forms.OpenFileDialog openFileDialog;

    }
}