using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( Evento ) )]
	public partial class Evento : IValidatableObject  {

		
		public IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> errors = new List<ValidationResult>();

			if( this.descrizione == null || this.descrizione.Length < 1 || this.descrizione.Length > 100 ) {
				ValidationResult vr = new ValidationResult( "descrizione evento non valido", new string [] { "descrizione" } );
				errors.Add( vr );
			}

			return errors;
		}

	}
}
