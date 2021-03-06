﻿using System;
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

namespace Digiphoto.Lumen.GestoreConfigurazione.UI.Licenze {
	/// <summary>
	/// Interaction logic for LicenseEditor.xaml
	/// </summary>
	public partial class LicenseEditor : UserControlBase {
		public LicenseEditor() {
			InitializeComponent();
						
			DataContextChanged += new DependencyPropertyChangedEventHandler(licenseEditor_DataContextChanged);
        }

		void licenseEditor_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			associaDialogProvider();
		}

	}
}
