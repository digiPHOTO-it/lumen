using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Model {

	/// <summary>
	/// esempio: Per una foto stampata e pagata a prezzo pieno, 
	/// puoi avere anche il file (della stessa foto) pagandola soltanto 1 euro.
	/// </summary>

	public class PromoStessaFotoSuFile : Promozione {

		private short _prezzoFile;
		public short prezzoFile {
			get {
				return _prezzoFile;
			}
			set {
				if( _prezzoFile != value ) {
					_prezzoFile = value;
					OnPropertyChanged( "prezzoFile" );
				}
			}
		}

		public override IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> res = new List<ValidationResult>( base.Validate( validationContext ) );

			if( this.attiva && prezzoFile <= 0 ) {
				ValidationResult vr = new ValidationResult( "Prezzo promozione non valido", new string[] { this.GetType().Name } );
				res.Add( vr );
			}
			
			return res;
		}

	}
}
