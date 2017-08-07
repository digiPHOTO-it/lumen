using System;
using System.Runtime.Serialization;

namespace Digiphoto.Lumen.SelfService.Carrelli {

	[DataContract]
	public class FotografoDto {

		private string _id;
		private string _nome;
		private byte [] _immagine;
		 
		[DataMember]
		public string id {
			get { return _id; }
			set { _id = value; }
		}

		[DataMember]
		public string nome {
			get { return _nome; }
			set { _nome = value; }
		}

		[DataMember]
		public byte [] immagine
		{
			get { return _immagine; }
			set { _immagine = value; }
		}



	}
}
