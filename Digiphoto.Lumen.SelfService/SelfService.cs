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
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.SelfService {

	public class SelfService : ISelfService {

		public SelfService() {


#if DEBUG
			// Siccome in debug mi avvalgo del truschino di Visual Studio per avviare il servizio, faccio questo trucco solo per il debug.
			// Normalmente deve essere l'applicazione Host che avvia e termina l'infrastruttura di Lumen
            if( LumenApplication.Instance.avviata == false ) {
				LumenApplication.Instance.avvia();
            }
#endif

		}


		public List<CarrelloDto> getListaCarrelli() {

			List<CarrelloDto> listaCarrelli = new List<CarrelloDto>();

			using( new UnitOfWorkScope() ) {

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
			}

			// ritorno gli oggetti di trasporto al client
			return listaCarrelli;
		}


		public List<FotografiaDto> getListaFotografie( Guid carrelloId ) {

			List<FotografiaDto> listaFotografie = new List<FotografiaDto>();

			using( new UnitOfWorkScope() ) {

				// ricavo il servizio dei carrelli e imposto quello corrente
				ICarrelloExplorerSrv srv = LumenApplication.Instance.getServizioAvviato<ICarrelloExplorerSrv>();
				srv.setCarrelloCorrente( carrelloId );

				foreach( RigaCarrello riga in srv.carrelloCorrente.righeCarrello ) {

					if( riga.fotografia_id != null ) {

						FotografiaDto dto = new FotografiaDto();
						dto.id = riga.fotografia.id;
						dto.etichetta = riga.fotografia.etichetta;
						dto.miPiace = riga.fotografia.miPiace; // TODO aggiungere nuovo flag su Fotografia

						listaFotografie.Add( dto );
					}
				}
			}

			return listaFotografie;
		}


		public void setMiPiace( Guid fotografiaId, bool miPiace ) {

			using( new UnitOfWorkScope( true ) ) {

				var srv = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografia>>();
				Fotografia foto = srv.getById( fotografiaId );

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
			return getImage( fotografiaId, IdrataTarget.Risultante );
		}

		public byte[] getImageProvino( Guid fotografiaId ) {
			return getImage( fotografiaId, IdrataTarget.Provino );
		}

		private byte[] getImage( Guid fotografiaId, IdrataTarget quale ) {

			byte[] bytes = null;

			using( new UnitOfWorkScope() ) {

				var srv = LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografia>>();

				Fotografia fotografia = srv.getById( fotografiaId );

				string nomeFileImg;

				// Qui faccio una piccola miglioria: Se l'immagine risultante ha delle correzioni non ancora applicate, le applico adesso.
                if( quale == IdrataTarget.Risultante )
					nomeFileImg = AiutanteFoto.idrataImmagineDaStampare( fotografia );
				else
					nomeFileImg = AiutanteFoto.idrataImmaginiFoto( fotografia, quale );

				bytes = File.ReadAllBytes( nomeFileImg );

				AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Tutte );
			}

			return bytes;
		}

		public byte[] getImageLogo() {
		
			if( !String.IsNullOrWhiteSpace( Configurazione.UserConfigLumen.logoNomeFileSelfService ) ) {
				string nomeLogo = Path.Combine( Configurazione.UserConfigLumen.cartellaLoghi, Configurazione.UserConfigLumen.logoNomeFileSelfService );
				return File.ReadAllBytes( nomeLogo );
			} else
				return null;
		}

		/// <summary>
		/// Ritorno la lista dei fotografi attivi
		/// </summary>
		/// <returns></returns>
		public List<FotografoDto> getListaFotografi() {

			List<FotografoDto> listaDto = new List<FotografoDto>();

			using( new UnitOfWorkScope() ) {

				var fotografi = UnitOfWorkScope.currentDbContext.Fotografi.Where( f => f.attivo == true ).OrderBy( f => f.cognomeNome );

		
				// Creo la lista contenente gli oggetti di trasporto leggeri che ho ricavato dal servizio core.
				foreach( var fotografo in fotografi ) {
					FotografoDto dto = new FotografoDto();
					dto.id = fotografo.id;
					dto.nome = fotografo.cognomeNome;
					dto.immagine = fotografo.immagine;
					listaDto.Add( dto );
				}
			}

			// ritorno gli oggetti di trasporto al client
			return listaDto;
		}

		public List<FotografiaDto> getListaFotografieDelFotografo( string fotografoId, int skip, int take ) {

			List<FotografiaDto> listaDto = new List<FotografiaDto>();

			using( new UnitOfWorkScope() ) {
				
				// uso apposito servizio di ricerca foto
				IRicercatoreSrv ricercaSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
				
				// preparo parametri
				ParamCercaFoto param = new ParamCercaFoto();
				
				var fotografo = UnitOfWorkScope.currentDbContext.Fotografi.Single( f => f.id == fotografoId );
				param.fotografi = new Fotografo [] { fotografo };
				param.evitareJoinEvento = true;
				param.paginazione = new Paginazione { skip = skip, take = take };
				param.idratareImmagini = false;
				DateTime giornata = StartupUtil.calcolaGiornataLavorativa();
				param.ordinamento = Ordinamento.Asc;
				param.giornataIniz = giornata;
				param.giornataFine = giornata;

				var fotografie = ricercaSrv.cerca( param );
				foreach( var foto in fotografie ) {
					FotografiaDto dto = new FotografiaDto();
					dto.etichetta = foto.etichetta;
					dto.id = foto.id;

					// da vedere se conviene 
					// dto.imgProvino = .... 

					listaDto.Add( dto );
				}
			}

			return listaDto;
		}
	}
}
