using System;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;


namespace Digiphoto.Lumen.Model {

	// Questi attributi sono transienti e non li gestisco sul database.
	// Ci penserò io a riempirli a mano

	public partial class Fotografia {

		public IImmagine imgOrig { get; set; }

		private IImmagine _imgProvino;
		public IImmagine imgProvino {
			get {
				return _imgProvino;
			}
			set {
				if( value != _imgProvino ) {
					_imgProvino = value;
					OnPropertyChanged( "imgProvino" );
				}
			}
		}

		public IImmagine imgRisultante { get; set; }

		private bool _isSelezionata;
		public bool isSelezionata {
			get {
				return _isSelezionata;
			}
			set {
				if( value != _isSelezionata ) {
					_isSelezionata = value;
					OnPropertyChanged( "isSelezionata" );
				}
			}
		}

		public override string ToString() {
			return String.Format( "Num.{0} del={1}", numero, dataOraAcquisizione.ToShortDateString() );
		}

	}

}
