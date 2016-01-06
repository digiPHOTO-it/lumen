using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Servizi.Reports;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Servizi.Vendere {

	/**
	 * Questo servizio funge da Gestore di Carrello ed anche come facade per il servizio IMasterizzaSrv
	 */
	public interface IVenditoreSrv : IServizio {

		ModoVendita modoVendita { get; set; }


		/** In base alla configurazione e ad altre variabili di lavoro, creo i parametri di stampa di default */
		ParamStampaFoto creaParamStampaFoto();

		ParamStampaProvini creaParamStampaProvini();

		/// <summary>
		///  Aggiungere foto da vendere come stampe
		/// </summary>
		void aggiungereStampe( IEnumerable<Fotografia> fotografie, ParamStampa param );

		void removeDatiDischetto();
		void setDatiDischetto( TipoDestinazione tipoDest, String nomeCartella );
		void setDatiDischetto( TipoDestinazione tipoDest, String nomeCartella, decimal? prezzoDischetto );

		/// <summary>
		///  Aggiungere foto da vendere come cd da masterizzare
		/// </summary>
		void aggiungereMasterizzate( IEnumerable<Fotografia> fotografie );

		/// <summary>
		/// Prepara un nuovo carrello vuoto (da riempire).
		/// Il carrello corrente viene "abbandonato" senza nessun salvataggio.
		/// </summary>
		void creareNuovoCarrello();
		

		/// <summary>
		/// Il carrello corrente viene venduto. Diventa definitivo
		/// </summary>
		/// <returns>
		/// null se tutto ok. Altrimenti ritorna il messaggio di errore
		/// che descrive il problema che è capitato.
		/// </returns>
		string vendereCarrello();

		/// <summary>
		/// Consente il Salvattaggio del Carrello senza effettuare ne la stampa ne la Masterizzazione
        /// </summary>
		string salvareCarrello();

		void abbandonareCarrello();

		void clonareCarrello();

		/// <summary>
		/// Esegue nuovamente la masterizzazione che potrebbe, in prima istanza,
		/// essere fallita (per es. se il CD non è vuoto)
		/// </summary>
		void rimasterizza();

		/// <summary>
		/// Elimina la riga dal carrello. Se la riga è persistente, allora
		/// rimuove anche dal db (senza commit).
		/// </summary>
		/// <param name="rigaCarrello"></param>
		void eliminareRigaCarrello(RigaCarrello rigaCarrello);

		/// <summary>
		/// Elimina dal carrello tutte le righe di un certo tipo
		/// </summary>
		/// <param name="discriminator">S=Stampe ; M=Masterizzate</param>
		void eliminareRigheCarrello( string discriminator );

		void eliminareCarrello(Carrello carrello);

		void spostareRigaCarrello(RigaCarrello rigaCarrello);

        void spostareTutteRigheCarrello(String discriminator, Carrello.ParametriDiStampa parametriDiStampa);

        void copiaSpostaRigaCarrello(RigaCarrello rigaCarrello);

        void copiaSpostaTutteRigheCarrello(String discriminator, Carrello.ParametriDiStampa parametriDiStampa);

		/** Lavoro con un carrello alla volta. Esiste un solo carrello "corrente". */
		Carrello carrello { get; }

		void caricareCarrello( Carrello c );

		List<RigaReportVendite> creaReportVendite( ParamRangeGiorni param );
		List<RigaReportProvvigioni> creaReportProvvigioni( ParamRangeGiorni paramRangeGiorni );



		bool isStatoModifica {
			get;
		}

		short? scontoApplicato {
			get;
		}

		bool isPossibileSalvareCarrello {
			get;
		}

		bool isPossibileModificareCarrello {
			get;
		}

		bool isPossibileClonareCarrello {
			get;
		}

		int sommatoriaFotoDaMasterizzare {
			get;
		}

		int sommatoriaQtaFotoDaStampare {
			get;
		}

		// Ritorna la sommatoria dei prezzi delle foto da stampare (in pratica è il totale carrello - il prezzo del dischetto)
		Decimal sommatoriaPrezziFotoDaStampare {
			get;
		}

		Decimal prezzoNettoTotale {
			get;
		}

		string msgValidaCarrello {
			get;
		}

		bool possoAggiungereStampe {
			get;
		}

		bool possoAggiungereMasterizzate {
			get;
		}

		Decimal calcolaIncassoPrevisto( DateTime giornata );

		IList<IncassoFotografo> calcolaIncassiFotografiPrevisti( DateTime giornata );

		/// <summary>
		/// Aggiorno i totali (senza le provvigioni)
		/// </summary>
		void ricalcolaTotaleCarrello();

		void ricalcolaProvvigioni();

		void rimpiazzaFotoInRiga( RigaCarrello riga, Fotografia fMod );

		string spazioFotoDaMasterizzate{
			get;
		}
	}
}
