using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Model;
using System.ComponentModel;
using Digiphoto.Lumen.Servizi.Ricerca;

namespace Digiphoto.Lumen.Servizi.BarCode
{
	public interface IBarCodeSrv : IServizio
	{

		/// <summary>
		/// Cerca solamente nella foto se è presente un barcode.
		/// </summary>
		/// <param name="foto">La fotografia da scansionare</param>
		/// <returns>Una stringa contenente il codice a barre. Se non trovo niente, ritorno null</returns>
		string searchBarCode( Fotografia foto );

		/// <summary>
		/// Ricerca i codici a barre e se trovati vengono applicati alla didascalia della Fotografia relativa.
		/// Siccome lavora in background, occorre poi invocare il metodo start() del servizio per farlo partire
		/// </summary>
		/// <param name="fotos">La lista delle fotografie da scansionare</param>
		void scan( IEnumerable<Fotografia> fotos );

		/// <summary>
		/// Ricerca i codici a barre e se trovati vengono applicati alla didascalia della Fotografia relativa.
		/// Siccome lavora in background, occorre poi invocare il metodo start() del servizio per farlo partire.
		/// </summary>
		/// <param name="fotos">La lista delle fotografie da scansionare</param>
		/// <returns>Il risultato si recupera nella callback di completamento: StatoScansione</returns>
		void prepareToScan( IEnumerable<Fotografia> fotos, ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler runWorkerCompleted );

		/// <summary>
		/// Ricerca i codici a barre e se trovati vengono applicati alla didascalia della Fotografia relativa.
		/// Siccome lavora in background, occorre poi invocare il metodo start() del servizio per farlo partire.
		/// Tramite i parametri cerca le foto con il servizio IRicercatoreSrv
		/// </summary>
		/// <param name="fotos">La lista delle fotografie da scansionare</param>
		/// <returns>Il risultato si recupera nella callback di completamento: StatoScansione</returns>
		void prepareToScan( ParamCercaFoto param, ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler runWorkerCompleted );
	}
}
