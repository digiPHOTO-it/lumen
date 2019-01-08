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
using System.Collections.Specialized;
using Digiphoto.Lumen.Servizi.Io;
using log4net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Digiphoto.Lumen.SelfService {

	public class SelfService : ISelfService {


		private static readonly ILog _giornale = LogManager.GetLogger( typeof( SelfService ) );

		public SelfService() {


#if DEBUG
			// Siccome in debug mi avvalgo del truschino di Visual Studio per avviare il servizio, faccio questo trucco solo per il debug.
			// Normalmente deve essere l'applicazione Host che avvia e termina l'infrastruttura di Lumen
            if( LumenApplication.Instance.avviata == false ) {
				LumenApplication.Instance.avvia();
            }
#endif

		}

		public CarrelloDto getCarrello2( String idCorto ) {

			CarrelloDto dto = null;

			using( new UnitOfWorkScope() ) {

				// Ricavo il servizio per estrarre i carrelli
				ICarrelloExplorerSrv srv = LumenApplication.Instance.getServizioAvviato<ICarrelloExplorerSrv>();

				ParamCercaCarrello param = new ParamCercaCarrello {
					soloSelfService = true,
					carrelloIdCorto = idCorto
				};

				srv.cercaCarrelli( param );

				// Creo la lista contenente gli oggetti di trasporto leggeri che ho ricavato dal servizio core.
				if( srv.carrelli != null && srv.carrelli.Count == 1 ) {
					var carrello = srv.carrelli.ElementAt( 0 );
					dto = SelfService.idrataDaCarrello( carrello );
				}
			}

			return dto;
		}


		public CarrelloDto getCarrello( Guid carrelloId ) {

			CarrelloDto dto = null;

			using( new UnitOfWorkScope() ) {

				// Ricavo il servizio per estrarre i carrelli
				ICarrelloExplorerSrv srv = LumenApplication.Instance.getServizioAvviato<ICarrelloExplorerSrv>();

				ParamCercaCarrello param = new ParamCercaCarrello {
					soloSelfService = true,
					carrelloId = carrelloId
				};

				srv.cercaCarrelli( param );

				// Creo la lista contenente gli oggetti di trasporto leggeri che ho ricavato dal servizio core.
				if( srv.carrelli != null && srv.carrelli.Count == 1 ) {
					var carrello = srv.carrelli.ElementAt( 0 );
					dto = SelfService.idrataDaCarrello( carrello );
				}
			}

			return dto;
		}

		private static CarrelloDto idrataDaCarrello( Carrello carrello ) {
			var dto = new CarrelloDto();
			dto.id = carrello.id;
			dto.titolo = carrello.intestazione;
			dto.isVenduto = carrello.venduto;
			return dto;
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
					dto.isVenduto = carrello.venduto;
					listaCarrelli.Add( dto );
				}
			}

			// ritorno gli oggetti di trasporto al client
			return listaCarrelli;
		}


		public List<FotografiaDto> getListaFotografie( Guid carrelloId ) {

			_giornale.Debug( "inizio metodo getListaFotografie( " + carrelloId  + " )" );

			List <FotografiaDto> listaFotografie = new List<FotografiaDto>();

			using( new UnitOfWorkScope() ) {

				_giornale.Debug( "apertura unit-ok-work ok" );

				// ricavo il servizio dei carrelli e imposto quello corrente
				ICarrelloExplorerSrv srv = LumenApplication.Instance.getServizioAvviato<ICarrelloExplorerSrv>();
				_giornale.Debug( "ottenuto servizio ICarrelloExplorerSrv" );

				try {

					// Se non ho il carrello nella cache, provo a caricarlo per ID (soltanto lui)
					if( srv.carrelli == null || srv.carrelli.Any( c => c.id == carrelloId ) == false )
						srv.cercaCarrelli( new ParamCercaCarrello { carrelloId = carrelloId, soloSelfService = true } );

					// Se ancora non l'ho trovato significa che non c'è oppure non è destinato al self service.
					if( srv.carrelli == null || srv.carrelli.Any( c => c.id == carrelloId ) == false )
						return listaFotografie;

					srv.setCarrelloCorrente( carrelloId );

					_giornale.Debug( "settatto carrello corrente: " + carrelloId + " tot. righe = " +  srv.carrelloCorrente.righeCarrello.Count );
					if( srv.carrelloCorrente != null ) {
					
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
				} catch( Exception ee ) {
					_giornale.Error( ee );
					throw ee;
				}
			}

			_giornale.Debug( "ritorno lista di " + listaFotografie .Count + " + righe" );

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

			try {
				return getImage( fotografiaId, IdrataTarget.Provino );
			} catch( Exception ) {
				return null;
			}

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
				if( File.Exists( nomeLogo ) )
					return File.ReadAllBytes( nomeLogo );
				else
					return null;
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
					dto.immagine = getImmagineFotografo( fotografo );
					listaDto.Add( dto );
				}
			}

			// ritorno gli oggetti di trasporto al client
			return listaDto;
		}

		private byte [] getImmagineFotografo( Fotografo f ) {

			string nomeFile = AiutanteFoto.nomeFileImgFotografo( f );
			if( nomeFile != null && File.Exists( nomeFile ) ) {
				IGestoreImmagineSrv g = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
				return g.load( nomeFile ).getBytes();
			} else
				return  null;
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

		/**
		 * Preparo i settaggi per il self-service.
		 * Il client cerchiamo di farlo girare senza configurazione. Plug-and-Play
		 * I settaggi li passiamo dal server
		 */
		public Dictionary<string, string> getSettings() {

			Dictionary<string, string> settings = new Dictionary<string, string>();
			
			var tipric = Configurazione.UserConfigLumen.modoRicercaSS ?? "fotografi";
			settings.Add( "tipo-ricerca", tipric );

			return settings;
		}
			

	}
}
