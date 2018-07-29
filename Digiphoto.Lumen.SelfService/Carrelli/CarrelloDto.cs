using System;
using System.Runtime.Serialization;

namespace Digiphoto.Lumen.SelfService.Carrelli {

	[DataContract]
	public class CarrelloDto {

		private Guid _id;
		private string _titolo;
		private bool _isVenduto;

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

		[DataMember]
		public bool isVenduto {
			get {
				return _isVenduto;
			}
			set {
				_isVenduto = value;
			}
		}

	}
}
