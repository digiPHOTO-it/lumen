using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Eventi;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Scaricatore {
		
	public enum FaseScaricoFoto {
			InizioLavora, //Identifica la fase precisa di inizio
			InizioScarico, //Identifica la fase in cui inizio a scaricare le foto
			InizioProvinatura, //Identitica la fase in cui inizio la provinatura
			FineScarico, //Identifica la fase di fine scaricoc delle foto
			FineProvinatura, //Identifica la fase di fine provinatura
			FineLavora, //Identifica la fase di fine dell'intera operazione
			Idle, //Indica la fase di attesa
			Scaricamento, //Indica la fase generica di scarimento...
			Provinatura //Indica la fase generica di provinatura...
	};

	public class ScaricoFotoMsg : Messaggio {

		public ScaricoFotoMsg( object sender, string descrizione ) : base( sender, descrizione ) {
		}

		/// <summary>
		/// Occhio può contenere il nome della cartella, oppure il nome del file singolo scaricato.
		/// </summary>
		public string sorgente {
			get;
			set;
		}

		public EsitoScarico esitoScarico {
			get;
			set;
		}
		
		public FaseScaricoFoto fase {get;	set;}


		public bool _puoiTogliereLaFlashCard {
			get;
			set;
		}
	}
}
