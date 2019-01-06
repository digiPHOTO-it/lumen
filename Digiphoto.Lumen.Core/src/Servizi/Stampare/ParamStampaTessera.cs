using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Digiphoto.Lumen.Core.Servizi.Stampare;

namespace Digiphoto.Lumen.Servizi.Stampare {

	public class ParamStampaTessera : ParamStampaFoto {

		public ParamStampaTessera() {
			numRighe = 3;
			numColonne = 3;
			mmHFoto = 45;
			mmWFoto = 35;
		}

		private int _numRighe;
		public int numRighe {
			get {
				return _numRighe;
			}
			set {
				if( _numRighe != value ) {
					_numRighe = value;
					OnPropertyChanged( "numRighe" );
				}
			}
		}

		private int _numColonne;
		public int numColonne {
			get {
				return _numColonne;
			}
			set {
				if( _numColonne != value ) {
					_numColonne = value;
					OnPropertyChanged( "numColonne" );
				}
			}
		}

		public int mmHFoto {
			get; set;
		}

		public int mmWFoto {
			get; set;
		}

		public override string ToString() {

			StringBuilder s = new StringBuilder();
			s.Append( " Righe=" + numRighe );
			s.Append( " Righe=" + numColonne );
			s.Append( " Copie=" + numCopie );

			return s.ToString();
		}
	}
}
