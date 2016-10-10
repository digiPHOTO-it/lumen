using System;
using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Mvvm.MultiSelect;
using System.Windows.Input;
using System.Windows;
using Digiphoto.Lumen.Core.Collections;
using Digiphoto.Lumen.Servizi.EntityRepository;

namespace Digiphoto.Lumen.UI
{

	/// <summary>
	/// Questo VM lavora con una lista fissa di foto. Viene usato puntualmente nei dialoghi
	/// </summary>
	public class SelettoreMetadatiViewModel2 : SelettoreMetadatiViewModel {

		public SelettoreMetadatiViewModel2( IEnumerable<Fotografia> listaFoto ) : base() {
			
			this.listaFoto = listaFoto;

			caricareStatoMetadati();
		}

		#region Astratti

		public override int countFotografieSelezionate {
			get {
				return listaFoto.Count();
			}
		}

		public override bool isAlmenoUnaFotoSelezionata {
			get {
				return listaFoto.Count() > 0;
			}
		}

		protected override IEnumerable<Fotografia> getElementiSelezionati() {
			return listaFoto;
		}

		protected override IEnumerator<Fotografia> getEnumeratorElementiSelezionati() {
			return listaFoto.GetEnumerator();
		}

		#endregion Astratti

		#region Proprieta

		public IEnumerable<Fotografia> listaFoto { get; private set; }

		#endregion Proprieta


	}
}
