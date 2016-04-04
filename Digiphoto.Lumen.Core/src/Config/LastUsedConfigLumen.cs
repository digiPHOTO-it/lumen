using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Servizi.Vendere;
using System.IO;
using System.ComponentModel;
using Digiphoto.Lumen.Core.Servizi.Stampare;

namespace Digiphoto.Lumen.Config
{
    public sealed class LastUsedConfigLumen : INotifyPropertyChanged
    {
		public LastUsedConfigLumen() {
		}

		public short slideShowNumRighe
		{
			get;
			set;
		}

		public short slideShowNumColonne
		{
			get;
			set;
		}

		public bool collassaFiltriInRicercaGallery {
			get;
			set;
		}

		public Margini marginiStampaProvini {
			get;
			set;
		}
		
		public event PropertyChangedEventHandler PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		public void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

	}
}
