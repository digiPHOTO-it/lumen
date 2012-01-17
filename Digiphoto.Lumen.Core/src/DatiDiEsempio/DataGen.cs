using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects.DataClasses;


namespace Digiphoto.Lumen.Core.DatiDiEsempio {
	/// <summary>
	/// Serve per generare dei dati di esempio da utilizzare nel designer
	/// di visual studio (o Blend) per decorare le schermate
	/// </summary>
	public class DataGen<TEntity> where TEntity : class {

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

			return null;
		}

		private Fotografo generaUnoFotografo() {

			ParoleCasuali pc = new ParoleCasuali();

			Fotografo f = Fotografo.CreateFotografo( pc.genera( 16 ), pc.genera( 30 ), pc.genera( 2 ) );
			return f;
		}

		private Evento generaUnoEvento() {

			ParoleCasuali pc = new ParoleCasuali();

			Evento e = Evento.CreateEvento( Guid.Empty, pc.genera(15) );
			return e;
		}


	}
}
