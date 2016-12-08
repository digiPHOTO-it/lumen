using Digiphoto.Lumen.SelfService.Carrelli;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Digiphoto.Lumen.SelfService {

	/// <summary>
	/// Interfaccia di contratto tra client e server
	/// per implementare il servizio di SelfService in modo che i clienti possano selezionare le proprie foto
	/// e dire quali foto gli piacciono
	/// </summary>
	[ServiceContract]
	public interface ISelfService {

		[OperationContract]
		List<CarrelloDto> getListaCarrelli();

		[OperationContract]
		List<FotografiaDto> getListaFotografie( Guid carrelloId );

		/// <summary>
		/// Ritorno lo stream binario del jpeg
		/// </summary>
		/// <param name="fotografiaId"></param>
		/// <returns></returns>
		[OperationContract]
		byte[] getImage( Guid fotografiaId );

		[OperationContract]
		void setMiPiace( Guid fotografiaId, bool miPiace );

	}


}
