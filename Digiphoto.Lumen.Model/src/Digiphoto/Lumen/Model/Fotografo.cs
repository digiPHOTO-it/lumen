using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( Fotografo ) )]
	public partial class Fotografo : IValidatableObject {

		public IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> errors = new List<ValidationResult>();

			if( this.cognomeNome == null || this.cognomeNome.Length < 1 || this.cognomeNome.Length > 100 ) {
				ValidationResult vr = new ValidationResult( "Cognome e nome fotografo non valido", new string [] { "cognomeNome" } );
				errors.Add( vr );
			}

			if( this.id == null || (this.id.Length < 4 || this.id.Length > 100) ) {
				ValidationResult vr = new ValidationResult( "Id fotografo non valido", new string [] { "id" } );
				errors.Add( vr );
			}

			return errors;
		}

	}
}
