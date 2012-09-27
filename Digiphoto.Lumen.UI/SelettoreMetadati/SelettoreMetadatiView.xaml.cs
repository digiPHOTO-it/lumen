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
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;

namespace Digiphoto.Lumen.UI
{
	/// <summary>
	/// Interaction logic for SelettoreMetadatiView.xaml
	/// </summary>
	public partial class SelettoreMetadatiView : UserControlBase
	{

		public static readonly DependencyProperty MyItemsSourceProperty;


		static SelettoreMetadatiView()
        {
			SelettoreMetadatiView.MyItemsSourceProperty = DependencyProperty.Register("MyItemsSource",
																			typeof(IEnumerable), 
																			typeof(SelettoreMetadatiView)
																			);
        }

        public IEnumerable MyItemsSource
        {
            get
            {
				return (IEnumerable)GetValue(SelettoreMetadatiView.MyItemsSourceProperty);
            }

            set
            {
				SetValue(SelettoreMetadatiView.MyItemsSourceProperty, value);
            }
        }

		SelettoreMetadatiViewModel _model = null;

		public SelettoreMetadatiView()
		{
			InitializeComponent();

			_model = new SelettoreMetadatiViewModel(this);

			this.DataContext = _model;

		}

	}
}