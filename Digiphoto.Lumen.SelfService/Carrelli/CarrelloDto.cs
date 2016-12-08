using System;
using System.Runtime.Serialization;

namespace Digiphoto.Lumen.SelfService.Carrelli {

	[DataContract]
	public class CarrelloDto {

		private Guid _id;
		private string _titolo;

		[DataMember]
		public Guid id {
			get { return _id; }
			set { _id = value; }
		}

		[DataMember]
		public string titolo {
			get { return _titolo; }
			set { _titolo = value; }
		}
	}
}
