using Digiphoto.Lumen.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Servizi.Vendere {

	interface ICalcolatorePromozione {

		Carrello Applica( Carrello cin, Promozione promo, ContestoDiVendita contestoDiVendita );

	}
}