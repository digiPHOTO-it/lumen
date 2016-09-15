using System.Collections.Generic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Ricerca;
using System;

namespace Digiphoto.Lumen.Servizi.Explorer {


	public interface IFotoExplorerSrv : IServizio {

		/** Queste sono tutte le foto estratte dall'archivio */
		List<Fotografia> fotografie { get; }

		/** Questa è la fotografia corrente */
		Fotografia fotoCorrente { get; set; }

		// Cerca le foto nell'archivio e le carica in memoria nella proprietà "fotografie"
		void cercaFoto( ParamCercaFoto param );

		/// <summary>
		/// Cerca le foto nell'archivio e le ritorna, senza tenerle in memoria e senza idratarle
		/// </summary>
		IList<Fotografia> cercaFotoTutte( ParamCercaFoto param );

		// Questa funzionalità sarebbe utile anche in altri servizi.
		// Forse andrebbe messo nel servizio FotografiaEntityRepository, che però attualmente non esiste. 
		// Quindi lo appoggio qui perché ora mi serve qui.
		// Eventualmente servire anche nel fotoritocco, lo sposterò più avanti.
		bool modificaMetadatiFotografie( IEnumerable<Fotografia> fotografie, MetadatiFoto metadati );

		IEnumerable<ScaricoCard> loadUltimiScarichiCards();

		int contaFoto( ParamCercaFoto paramCercaFoto );

		/// <summary>
		/// Carico la gallery recuperando tutte le righe presenti nel carrello (sia stampe che masterizzate)
		/// </summary>
		IEnumerable<Guid> caricaFotoDalCarrello();

		/// <summary>
		/// Ricavo l'entità dato il suo identificativo
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Fotografia get( Guid id );
	}

	public class MetadatiFoto {

		public string didascalia { set; get; }
		public Evento evento { set; get; }
		public FaseDelGiorno? faseDelGiorno { set; get; }

        public bool isDidascaliaEnabled { set; get; }
        public bool isEventoEnabled { set; get; }
        public bool isFaseDelGiornoEnabled { set; get; }

		public bool isEmpty() {
			return didascalia == null && evento == null && faseDelGiorno == null && isDidascaliaEnabled && isEventoEnabled && isFaseDelGiornoEnabled;
		}

        public string didascaliaString
        {
            get
            {
                return didascalia != null ? didascalia : "empty";
            }
        }

        public string eventoString
        {
            get{
                return evento != null ? evento.ToString() : "empty";
            }
        }

        public string faseDelGiornoString
        {
            get
            {
                return faseDelGiorno != null ? faseDelGiorno.ToString() : "empty";
            }
        }
    }
}
