using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Services {

	[ServiceContract]
	public interface ISpoolerService {

		[OperationContract]
		String About();

		/// <summary>
		/// Eseguo le stampe sul server, per disimpegnare i vari client
		/// </summary>
		/// <param name="td">T = testata : D = dettaglio</param>
		/// <param name="guid">Può essere la chiave delle testate carrello oppure dei dettagli</param>
		[OperationContract]
		void EseguireStampe( char td, Guid guid );

	}
}
