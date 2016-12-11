using Digiphoto.Lumen.SelfService.Carrelli;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Core.Database;

namespace Digiphoto.Lumen.SelfService {

	public class SelfService : ISelfService {

		public SelfService() {


#if DEBUG
				// Siccome in debug mi avvalgo del truschino di Visual Studio per avviare il servizio, faccio questo trucco solo per il debug.
				// Normalmente deve essere l'applicazione Host che avvia e termina l'infrastruttura di Lumen

				LumenApplication.Instance.avvia();
#endif

		}


		public List<CarrelloDto> getListaCarrelli() {

			List<CarrelloDto> listaCarrelli = new List<CarrelloDto>();

			// Ricavo il servizio per estrarre i carrelli
			ICarrelloExplorerSrv srv = LumenApplication.Instance.getServizioAvviato<ICarrelloExplorerSrv>();

			// TODO decidere quanti carrelli al massimo. Per ora ne fisso 10.
			ParamCercaCarrello param = new ParamCercaCarrello {
			    soloSelfService = true,
				isVenduto = false,
				paginazione = new Util.Paginazione {
					skip = 0,
					take = 10
				}
			};

			srv.cercaCarrelli( param );

			// Creo la lista contenente gli oggetti di trasporto leggeri che ho ricavato dal servizio core.
			foreach( var carrello in srv.carrelli ) {
				CarrelloDto dto = new CarrelloDto();
				dto.id = carrello.id;
				dto.titolo = carrello.intestazione;
				listaCarrelli.Add( dto );
			}

			// ritorno gli oggetti di trasporto al client
			return listaCarrelli;
		}


		public List<FotografiaDto> getListaFotografie( Guid carrelloId ) {

			// ricavo il servizio dei carrelli e imposto quello corrente
			ICarrelloExplorerSrv srv = LumenApplication.Instance.getServizioAvviato<ICarrelloExplorerSrv>();
			srv.setCarrelloCorrente( carrelloId );

			List<FotografiaDto> listaFotografie = new List<FotografiaDto>();
			
			foreach( RigaCarrello riga in srv.carrelloCorrente.righeCarrello ) {


				if( riga.fotografia_id != null ) {

					FotografiaDto dto = new FotografiaDto();
					dto.id = riga.fotografia.id;
					dto.etichetta = riga.fotografia.etichetta;
					dto.miPiace = false; // TODO aggiungere nuovo flag su Fotografia

					listaFotografie.Add( dto );
				}


			}


			return listaFotografie;
		}


		public void setMiPiace( Guid fotografiaId, bool miPiace ) {

			using( new UnitOfWorkScope( true ) ) {

				var srv = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografia>>();
				Fotografia foto = srv.getById( fotografiaId );

				// TODO per il momento non ho a disposizione il nuovo flag. Lo scrivo nella didascalia, giusto per fare una prova.
				foto.miPiace = miPiace;

				srv.update( ref foto );
			}

		}

		/// <summary>
		/// Ritorna lo stream di byte di cui è composto il jpeg della foto
		/// </summary>
		/// <param name="fotografiaId"></param>
		/// <returns></returns>
		public byte[] getImage( Guid fotografiaId ) {

			byte[] bytes = null;

            using( new UnitOfWorkScope() ) { 

				var srv = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografia>>();

				Fotografia fotografia = srv.getById( fotografiaId );

				string nomeFileImg = AiutanteFoto.idrataImmagineDaStampare( fotografia );

				bytes = File.ReadAllBytes( nomeFileImg );

				AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Tutte );
			}

			return bytes;
		}
	}
}
