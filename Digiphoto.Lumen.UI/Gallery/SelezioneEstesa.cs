using Digiphoto.Lumen.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.UI.Gallery {
	
	public class SelezioneEstesa {

	
		public Fotografia limiteA { get; set; }
		public Fotografia limiteB { get; set; }

		public void azzera() {
			limiteA = limiteB = null;
		}

		internal void remove( Fotografia fr ) {
			if( fr.Equals( limiteA ) )
				limiteA = null;
			if( fr.Equals( limiteB ) )
				limiteB = null;
		}

		public bool contains( Fotografia foto ) {
			return foto.Equals( limiteA ) || foto.Equals( limiteB );
		}

		public bool isCompleta {
			get {
				return limiteA != null && limiteB != null;
			}
		}

		/// <summary>
		/// La selezione è su di una singola foto,
		/// ovvero il limite A è uguale al limite B
		/// </summary>
		public bool isSingola {
			get {
				return isCompleta && limiteA.Equals( limiteB );
			}
		}

		public int numeroMinore {
			get {
				return isCompleta ? Math.Min( limiteA.numero, limiteB.numero ) : -1;
			}
		}

		public int numeroMaggiore {
			get {
				return isCompleta ? Math.Max( limiteA.numero, limiteB.numero ) : -1;
			}
		}

	}
}
