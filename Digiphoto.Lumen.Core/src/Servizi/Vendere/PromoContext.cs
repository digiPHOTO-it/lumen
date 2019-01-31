using Digiphoto.Lumen.Model;
using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	public class PromoContext {

		public PromoContext() {
			promoApplicate = new List<Promozione>();
			applicarePromoADiscrezione = false;
		}

		#region Proprietà

		public bool applicarePromoADiscrezione {
			get; set;
		}

		/// <summary>
		/// Elenco delle promo applicate durante la elaborazione
		/// </summary>
		public List<Promozione> promoApplicate {
			get; set;
		}

		#endregion Proprietà

	}
}
