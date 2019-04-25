using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	
	[MetadataType( typeof( Evento ) )]
	[Serializable]
	[Table( "Eventi" )]
	public partial class Evento : IValidatableObject, IDataErrorInfo, INotifyPropertyChanged {

		public Evento() {
			this.attivo = true;
		}

		#region Attributi

		[Key]
		public System.Guid id { get; set; }

		[Required]
		public string descrizione { get; set; }

		public bool attivo { get; set; }

		public Nullable<short> ordinamento { get; set; }

		#endregion Attributi

		#region IValidatableObject
		public IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> errors = new List<ValidationResult>();

			if( this.descrizione == null || this.descrizione.Length < 1 || this.descrizione.Length > 100 ) {
				ValidationResult vr = new ValidationResult( "descrizione evento non valido", new string [] { "descrizione" } );
				errors.Add( vr );
			}

			return errors;
		}
		#endregion IValidatableObject

		#region Uguaglianza

		public override int GetHashCode() {
			return 17 + 31 * id.GetHashCode();
		}

		public override bool Equals( object obj ) {

			bool sonoUguali = false;

			if( obj is Evento ) {

				Evento altra = (Evento)obj;

				if( this.id != Guid.Empty && altra.id != Guid.Empty ) {
					sonoUguali = this.id.Equals( altra.id );
				}
			}

			return sonoUguali;
		}

		#endregion Uguaglianza

		#region IDataErrorInfo

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

		#endregion IDataErrorInfo

		#region INotifyPropertyChanged

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( string propertyName ) {
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		protected virtual void OnPropertyChanged( PropertyChangedEventArgs e ) {
			if( PropertyChanged != null )
				PropertyChanged( this, e );
		}

		#endregion INotifyPropertyChanged	

	}
}