﻿using Digiphoto.Lumen.SelfService.Carrelli;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Digiphoto.Lumen.SelfService {

	/// <summary>
	/// Interfaccia di contratto tra client e server
	/// per implementare il servizio di SelfService in modo che il cliente possa selezionare le proprie foto
	/// e dire quali foto gli piacciono.
	/// </summary>
	[ServiceContract]
	public interface ISelfService {

		/// <summary>
		/// Ricava la lista dei carrelli che vanno mostrati sul client del self-service
		/// </summary>
		/// <returns>Una lista di oggetti di trasporto leggeri, con solo i dati necessari</returns>
		[OperationContract]
		List<CarrelloDto> getListaCarrelli();

		[OperationContract]
		CarrelloDto getCarrello( Guid carrelloId );

		/// <summary>
		/// Questo metodo serve per reperire un carrello tramite un id corto, in 
		/// modo che un essere umano possa digitarlo
		/// </summary>
		/// <param name="carrelloIdCorto"></param>
		/// <returns></returns>
		[OperationContract]
		CarrelloDto getCarrello2( String carrelloIdCorto );

		/// <summary>
		/// Ricava la lista delle foto contenute nel carrello indicato nel parametro
		/// </summary>
		/// <param name="carrelloId"></param>
		/// <returns>Una lista di oggetti di trasporto leggeri, con solo i dati necessari</returns>
		[OperationContract]
		List<FotografiaDto> getListaFotografie( Guid carrelloId );

		/// <summary>
		/// Ottiene la foto ad alta qualità.
		/// </summary>
		/// <param name="fotografiaId"></param>
		/// <returns>stream di byte che compongono il jpeg</returns>
		[OperationContract]
		byte[] getImage( Guid fotografiaId );

		/// <summary>
		/// Ottiene la foto a bassa qualità per fare una eventuale anteprima
		/// </summary>
		/// <param name="fotografiaId">identificativo della foto.</param>
		/// <returns>stream di byte che compongono il jpeg</returns>
		[OperationContract]
		byte[] getImageProvino( Guid fotografiaId );

		/// <summary>
		/// Ottine l'immagine del logo da visualizzare nella home page
		/// </summary>
		/// <returns>stream di byte che compongono il jpeg</returns>
		[OperationContract]
		byte[] getImageLogo();

		/// <summary>
		/// Permette al cliente finale di taggare le foto con la preferenza dell'utente (sia positiva che negativa)
		/// </summary>
		/// <param name="fotografiaId">identificativo della foto.</param>
		/// <param name="miPiace">flag che indica preferenza positiva/negativa.</param>
		[OperationContract]
		void setMiPiace( Guid fotografiaId, bool miPiace );

		/// <summary>
		/// Ottiene la lista dei fotografi attivi.
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		List<FotografoDto> getListaFotografi();

		/// <summary>
		/// Ricava la lista delle foto di un determinato fotografo nella giornata odierna
		/// </summary>
		/// <param name="carrelloId"></param>
		/// <returns>Una lista di oggetti di trasporto leggeri, con solo i dati necessari</returns>
		[OperationContract]
		List<FotografiaDto> getListaFotografieDelFotografo( RicercaFotoParam ricercaFotoParam );

		[OperationContract]
		Dictionary<string, string> getSettings();
	}


}
