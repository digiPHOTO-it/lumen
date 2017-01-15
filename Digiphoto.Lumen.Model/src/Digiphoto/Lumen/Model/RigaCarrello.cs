using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Model {
	
	public partial class RigaCarrello {

		public const string TIPORIGA_STAMPA       = "S";
		public const string TIPORIGA_MASTERIZZATA = "M";

		public bool isTipoStampa {
			get {
				return TIPORIGA_STAMPA == this.discriminator;
			}
		}

		public bool isTipoMasterizzata {
			get {
				return TIPORIGA_MASTERIZZATA == this.discriminator;
			}
		}

		/// <summary>
		/// Data una riga di un carrelllo, ritorno il discriminatore invertito
		/// es.
		/// se Masterizzata -> torno Stampata
		/// se Stampata     -> torno Masterizzata
		/// </summary>
		/// <param name="riga"></param>
		/// <returns></returns>
		public string getDiscriminatorOpposto() {
			return RigaCarrello.getDiscriminatorOpposto( this.discriminator );
		}

		/// <summary>
		/// Data una riga di un carrelllo, ritorno il discriminatore invertito
		/// es.
		/// se Masterizzata -> torno Stampata
		/// se Stampata     -> torno Masterizzata
		/// </summary>
		/// <param name="riga"></param>
		/// <returns></returns>
		public static string getDiscriminatorOpposto( string disc ) {
			if( disc == RigaCarrello.TIPORIGA_MASTERIZZATA )
				return RigaCarrello.TIPORIGA_STAMPA;
			if( disc == RigaCarrello.TIPORIGA_STAMPA )
				return RigaCarrello.TIPORIGA_MASTERIZZATA;

			return null;
		}

	}
}
