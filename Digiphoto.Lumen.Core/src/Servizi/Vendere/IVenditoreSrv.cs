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

		/** Foto da vendere come stampe */
		void aggiungereStampe( IEnumerable<Fotografia> fotografie, ParamStampa param );

		void removeDatiDischetto();
		void setDatiDischetto( TipoDestinazione tipoDest, String nomeCartella );
		void setDatiDischetto( TipoDestinazione tipoDest, String nomeCartella, decimal? prezzoDischetto );
		void aggiungereMasterizzate( IEnumerable<Fotografia> fotografie );

		/** 
		 * Prepara un nuovo carrello vuoto (da riempire).
		 * Il carrello corrente viene "abbandonato" senza nessun salvataggio.
		 */
		void creaNuovoCarrello();

		/**
		 * Il carrello corrente viene venduto. Diventa definitivo
		 */
		bool vendereCarrello();

		/**
		 * Consente il Salvattaggio del Carrello senza effettuare ne la stampa ne la Masterizzazione
         */
		bool salvaCarrello();

		void abbandonaCarrello();

		void rimasterizza();

		void removeRigaCarrello(RigaCarrello rigaCarrello);

		void spostaRigaCarrello(RigaCarrello rigaCarrello);

		/// <summary>
		/// Elimina dal carrello tutte le righe di un certo tipo
		/// </summary>
		/// <param name="discriminator">S=Stampe ; M=Masterizzate</param>
		void removeRigheCarrello( string discriminator );

		void removeCarrello(Carrello carrello);

		/** Lavoro con un carrello alla volta. Esiste un solo carrello "corrente". */
		Carrello carrello { get; }

		void caricaCarrello( Carrello c );

		List<RigaReportVendite> creaReportVendite( ParamRangeGiorni param );

		bool isStatoModifica {
			get;
		}

		short? scontoApplicato {
			get;
		}

		bool isPossibileSalvareCarrello {
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
	}
}
