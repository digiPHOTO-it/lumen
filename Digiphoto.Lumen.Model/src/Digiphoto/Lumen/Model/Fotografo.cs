using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( Fotografo ) )]
	public partial class Fotografo : IValidatableObject, IDataErrorInfo

	{

		public IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> errors = new List<ValidationResult>();

			if( String.IsNullOrEmpty( this.cognomeNome ) || this.cognomeNome.Length > 100 ) {
				ValidationResult vr = new ValidationResult( "Cognome e nome fotografo non valido", new string [] { "cognomeNome" } );
				errors.Add( vr );
			}

			if( this.id == null || this.id.Length > 16 ) {
				ValidationResult vr = new ValidationResult( "Id fotografo non valido", new string [] { "id" } );
				errors.Add( vr );
			}

			return errors;
		}

		//IDataErrorInfo - you can copy this code to each class

       public string Error

       {
           get { return this[""]; }
       }
 
       public string this[string columnName]
       {
           get
           {
               string ret = null;
               var data = this.Validate(new ValidationContext(this, null, null));
               switch (columnName)
               {
                   case "":
 
                       foreach (var item in data)
                       {
                           ret += item.ErrorMessage + Environment.NewLine;
                       }
                       break;
                   default:
                       foreach (var item in data)
                       {
                           if (item.MemberNames.Contains(columnName))
                               ret += item.ErrorMessage + Environment.NewLine;
                       }
                       break;
               }
               return ret;
           }

       }


	}
}
