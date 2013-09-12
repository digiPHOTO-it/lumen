using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging.Correzioni {

	public interface ICorrettore {

		IImmagine applica( IImmagine immagineSorgente, Correzione correzione );


/*
		/// <summary>
		/// Il correttore mi dice su che tipo di correzione è in grado di lavorare.
		/// </summary>
		/// <returns></returns>
		Type getTypeOfCorrezione();


		
		public bool canConvertFromCorrezione();
		public Correzione convertFrom( object sourceObject );

		public bool canConvertTo( Type destinationType );
		public Object convertTo( object value, Type destinationType );
 */ 
	}

}
