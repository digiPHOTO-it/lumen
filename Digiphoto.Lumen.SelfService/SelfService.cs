using Digiphoto.Lumen.SelfService.Carrelli;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Digiphoto.Lumen.SelfService {

	public class SelfService : ISelfService {

		public SelfService() {

			creaListaCarrelli();
			creaListaFotografie();
		}

		private List<CarrelloDto> listaCarrelli;

		public List<CarrelloDto> getListaCarrelli() {
			return listaCarrelli;
		}

		public void creaListaCarrelli() {

			listaCarrelli = new List<CarrelloDto>();

			listaCarrelli.Add( new CarrelloDto {
				id = Guid.NewGuid(),
				titolo = "carrello1"
			} );

			listaCarrelli.Add( new CarrelloDto {
				id = Guid.NewGuid(),
				titolo = "carrello2"
			} );

			listaCarrelli.Add( new CarrelloDto {
				id = Guid.NewGuid(),
				titolo = "carrello3"
			} );

		}

		
		private List<FotografiaDto> listaFotografie;

		public List<FotografiaDto> getListaFotografie( Guid carrelloId ) {
			return listaFotografie;
		}

		void creaListaFotografie() {

			listaFotografie = new List<FotografiaDto>();

			listaFotografie.Add( new FotografiaDto {
				id = Guid.NewGuid(),
				numero = "A1",
				miPiace = true
			} );

			listaFotografie.Add( new FotografiaDto {
				id = Guid.NewGuid(),
				numero = "B2",
				miPiace = true
			} );

			listaFotografie.Add( new FotografiaDto {
				id = Guid.NewGuid(),
				numero = "C3",
				miPiace = true
			} );

		}

		public void setMiPiace( Guid fotografiaId, bool miPiace ) {
			listaFotografie.Single( f => f.id == fotografiaId ).miPiace = miPiace;
		}

		public byte[] getImage( Guid fotografiaId ) {

			// prendo un file a caso
			var files = Directory.GetFiles( @"C:\Users\bluca\Pictures\Battesimo Emanuele", "*.jpg" );
			int quanti = files.Count();

			Random rnd = new Random();
			int pos = rnd.Next( 0, quanti - 1 );

			return System.IO.File.ReadAllBytes( files[pos] );
		}
	}
}
