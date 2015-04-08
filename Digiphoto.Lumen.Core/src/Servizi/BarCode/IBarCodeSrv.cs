using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.BarCode
{
	public interface IBarCodeSrv : IServizio
	{
		string searchBarCode(Fotografia foto);

		int applicaBarCodeDidascalia(IEnumerable<Fotografia> fotos);

	}
}
