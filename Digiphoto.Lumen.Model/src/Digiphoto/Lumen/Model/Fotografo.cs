using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( Fotografo ) )]
	[Serializable]
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


	   public override bool Equals( object altro ) {
		   bool uguali = false;
		   if( altro != null && altro is Fotografo ) {

				if( this.id != null )
					uguali = this.id.Equals(((Fotografo)altro).id);
				else if( (((Fotografo)altro).id) != null )
					uguali = ((Fotografo)altro).id.Equals( this.id );
				// Se sono entrambi null ?? per me non devono essere uguali !!
		   }

		   return uguali;
	   }

	   public override int GetHashCode() {
		   int hash = 7;
		   hash = 31 * hash + (null == this.id ? 0 : this.id.GetHashCode());
		   return hash;
	   }
	}
}
