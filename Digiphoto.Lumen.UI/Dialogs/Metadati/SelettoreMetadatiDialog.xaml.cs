using System;
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
using System.Windows.Shapes;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for SelettoreMetadatiDialog.xaml
	/// </summary>
	public partial class SelettoreMetadatiDialog : Window, IObserver<MetadatiMsg>
	{
		public SelettoreMetadatiDialog(MultiSelectCollectionView<Fotografia> fotografieCW)
		{
			InitializeComponent();

			this.fotografieCW = fotografieCW;

			this.DataContext = this;

			IObservable<MetadatiMsg> observableMetadati = LumenApplication.Instance.bus.Observe<MetadatiMsg>();
			observableMetadati.Subscribe(this);
		}

		#region Proprieta

		public MultiSelectCollectionView<Fotografia> fotografieCW
		{
			get;
			set;
		}

		#endregion
		
		#region MemBus

		public void OnCompleted()
		{
			throw new NotImplementedException();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}

		public void OnNext(MetadatiMsg msg)
		{
			if (msg.sender is SelettoreMetadatiViewModel)
			{
				msg.fase = Digiphoto.Lumen.Servizi.Explorer.Fase.Completata;
				this.Hide();
			}

		}

		#endregion
	}
}
