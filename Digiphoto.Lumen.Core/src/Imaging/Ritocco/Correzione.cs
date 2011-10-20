using System;
using System.Xml.Serialization;

namespace Digiphoto.Lumen.Imaging.Ritocco {

	public enum Cardinalita {
		UNOSOLO,    // Nella lista ci può essere solo una istanza di questa correzione (doppioni non ammessi)
		SOMMABILE,  // Nella lista ci può essere solo una istanza di questa correzione e si possono sommare
		DISTINTI    // Nella lista le istanze di queste Correzione devono essere separate.
	}

	[Serializable]
	[XmlInclude( typeof( BiancoNero ) )]
	[XmlInclude( typeof( Contrasto ) )]
	[XmlInclude( typeof( Cornice ) )]
	[XmlInclude( typeof( Crop ) )]
	[XmlInclude( typeof( Luminosita ) )]
	[XmlInclude( typeof( Ruota ) )]
	[XmlInclude( typeof( Seppia ) )]
	[XmlInclude( typeof( Specchio ) )]
	public abstract class Correzione {

		/** ciao, questo metodo mi dice se la stessa correzione può comparire più volte
		 * nella lista delle correzioni oppure va sommata.
		 * Per esempio se ruoto prima di 20 e poi di 10, posso sommare e far ruota=30.
		 * In teoria questa informazione è statica, ma non si riesce a derivare un metodo statico.
		 */
		public abstract Cardinalita getCardinalita();

	}

}
