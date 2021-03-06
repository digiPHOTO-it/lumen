﻿using System;
using System.Collections.Generic;
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

		ModoVendita modoVendita {
			get; set;
		}


		/** In base alla configurazione e ad altre variabili di lavoro, creo i parametri di stampa di default */
		ParamStampaFoto creaParamStampaFoto();

		ParamStampaProvini creaParamStampaProvini();

		/// <summary>
		///  Aggiungere foto da vendere come stampe
		/// </summary>
		void aggiungereStampe( IEnumerable<Fotografia> fotografie, ParamStampa param );
		void aggiungereStampe( Fotografia fotografia, ParamStampa param );

		void removeDatiDischetto();
		void setDatiDischetto( MasterizzaTarget masterizzaTarget, String nomeCartella );
		void setDatiDischetto( MasterizzaTarget masterizzaTarget, String nomeCartella, decimal? prezzoDischetto );

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
		void eliminareRigaCarrello( RigaCarrello rigaCarrello );

		/// <summary>
		/// Elimina dal carrello tutte le righe di un certo tipo
		/// </summary>
		/// <param name="discriminator">S=Stampe ; M=Masterizzate</param>
		void eliminareRigheCarrello( string discriminator );

		void eliminareCarrello( Carrello carrello );

		void spostareRigaCarrello( RigaCarrello rigaCarrello );

		void spostareTutteRigheCarrello( String discriminator, ParametriDiStampa parametriDiStampa );

		void copiaSpostaRigaCarrello( RigaCarrello rigaCarrello, ParametriDiStampa parametriDiStampa );

		void copiaSpostaTutteRigheCarrello( String discriminator, ParametriDiStampa parametriDiStampa );

		/** Lavoro con un carrello alla volta. Esiste un solo carrello "corrente". */
		Carrello carrello {
			get;
		}


		/// <summary>
		/// Riesegue di nuovo le stampe del carrello (che magari si erano inceppate)
		/// </summary>
		/// <param name="carrelloId"></param>
		void RistampareCarrello( Guid carrelloId );
		void RistampareRigaCarrello( Guid rigaCarrelloId );

		void caricareCarrello( Carrello c );

		ReportVendite creaReportVendite( ParamRangeGiorni param );
		List<RigaReportProvvigioni> creaReportProvvigioni( ParamRangeGiorni paramRangeGiorni );

		/// <summary>
		/// ottendo la lista degli ID delle foto che sono nel carrello
		/// </summary>
		/// <returns></returns>
		IEnumerable<Guid> enumeraIdsFoto();


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

		Decimal sommatoriaPrezziFotoDaMasterizzare {
			get;
		}

		Decimal prezzoNettoTotale {
			get;
		}

		Nullable<decimal> prezzoPromozione {
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

		string spazioFotoDaMasterizzate {
			get;
		}

		bool applicarePromoDiscrez {
			get;
			set;
		}

		bool esistonoPromoADiscrezione {
			get;
		}

		bool esistonoPromoAttive {
			get;
		}
	}
}
