using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace FormsTestApplication
{
    public partial class FormModificaConnectionString : Form
    {
        public FormModificaConnectionString()
        {
            InitializeComponent();
            entityNameField.Text = "LumenEntities";
        }

        private void sfoglia_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                percorsoFileConfigField.Text = openFileDialog.FileName;
            }

            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = percorsoFileConfigField.Text;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            ConnectionStringsSection connSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            configTextArea.AppendText(connSection.ConnectionStrings[entityNameField.Text].ConnectionString);
        }

        private void modifica_Click(object sender, EventArgs e)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = percorsoFileConfigField.Text;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            ConnectionStringsSection connSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            if (connSection != null)
            {
                connSection.ConnectionStrings[entityNameField.Text].ConnectionString = configTextArea.Text;
                config.Save();
                ConfigurationManager.RefreshSection("connectionStrings");
            }
            MessageBox.Show("Modifica Avvenuta","Modifica Avvenuta");
        }
    }
}
