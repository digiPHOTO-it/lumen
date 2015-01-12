using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;

namespace Digiphoto.Lumen.UI.SelettoreAzioniRapide
{
	public interface IAzzioniRapide
    {
		SelettoreAzioniRapideViewModel selettoreAzioniRapideViewModel
		{
			get;
			set;
		}
		
		MultiSelectCollectionView<Fotografia> fotografieCW	{
			get;
		}

    }


}
