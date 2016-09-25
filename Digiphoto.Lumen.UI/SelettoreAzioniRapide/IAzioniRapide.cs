using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using Digiphoto.Lumen.Core.Collections;

namespace Digiphoto.Lumen.UI.SelettoreAzioniRapide
{
	public interface IAzioniRapide : ISelettore<Fotografia>
    {
		SelettoreAzioniRapideViewModel selettoreAzioniRapideViewModel
		{
			get;
			set;
		}


		/// <summary>
		/// Mi faccio ritornare un iteratore per poter applicare l'azione su tutte le foto.
		/// </summary>		
		// System.Collections.Generic.IEnumerator<Fotografia> geEnumeratorFotoTutte();


    }


}
