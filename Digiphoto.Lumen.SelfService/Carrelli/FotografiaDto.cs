using System;
using System.Runtime.Serialization;

namespace Digiphoto.Lumen.SelfService.Carrelli {

	[DataContract]
	public class FotografiaDto {

		private Guid _id;
		private bool? _miPiace;
		private string _etichetta;

		[DataMember]
		public Guid id {
			get { return _id; }
			set { _id = value; }
		}

		[DataMember]
		public bool? miPiace {
			get { return _miPiace; }
			set { _miPiace = value; }
		}

		[DataMember]
		public string etichetta {
			get { return _etichetta; }
			set { _etichetta = value; }
		}

	}
}
