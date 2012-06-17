﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( FormatoCarta ) )]
	public partial class FormatoCarta : IValidatableObject {

		public IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> errors = new List<ValidationResult>();

			if( this.descrizione == null || this.descrizione.Length < 1 || this.descrizione.Length > 100 ) {
				ValidationResult vr = new ValidationResult( "descrizione formato carta non valido", new string [] { "descrizione" } );
				errors.Add( vr );
			}

			return errors;
		}
	}

}