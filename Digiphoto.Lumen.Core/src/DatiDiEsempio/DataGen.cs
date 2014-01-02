using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using  System.Data.Entity.Core.Objects.DataClasses;
using Digiphoto.Lumen.Servizi.Stampare;


namespace Digiphoto.Lumen.Core.DatiDiEsempio {
	/// <summary>
	/// Serve per generare dei dati di esempio da utilizzare nel designer
	/// di visual studio (o Blend) per decorare le schermate
	/// </summary>
	public class DataGen<TEntity> where TEntity : class {

		private ParoleCasuali pc;

		public DataGen() {
			pc = new ParoleCasuali();
		}

		public IEnumerable<TEntity> generaMolti( int quanti ) {

			List<TEntity> lista = new List<TEntity>();

			for( int ii = 0; ii < quanti; ii++ )
				lista.Add( generaUno( typeof( TEntity ) ) );
	
			return lista;
		}


		private TEntity generaUno( Type tipo ) {

			if( tipo.Equals( typeof(Digiphoto.Lumen.Model.Fotografo) ) )
				return generaUnoFotografo() as TEntity;
			if( tipo.Equals( typeof( Digiphoto.Lumen.Model.Evento ) ) )
				return generaUnoEvento() as TEntity;
            if (tipo.Equals(typeof(Digiphoto.Lumen.Model.FormatoCarta)))
                return generaUnoFormatoCarta() as TEntity;
            if (tipo.Equals(typeof(StampanteInstallata)))
                return generaUnoStampantiInstallate() as TEntity;
			return null;
		}

		private Fotografo generaUnoFotografo() {
			return new Fotografo { id=pc.genera( 16 ),  cognomeNome=pc.genera( 30 ), iniziali=pc.genera( 2 ) };
		}

		private Evento generaUnoEvento() {
			return new Evento { id=Guid.Empty,  descrizione=pc.genera(15) };
		}

        private FormatoCarta generaUnoFormatoCarta() {
            return new FormatoCarta { id=Guid.Empty, descrizione=pc.genera(15), prezzo=5 };
        }

        private StampanteInstallata generaUnoStampantiInstallate() {
            return StampanteInstallata.CreateStampanteInstallata(pc.genera(15),"LTP1");
        }

	}
}
