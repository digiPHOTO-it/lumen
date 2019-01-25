using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( Promozione ) )]
	[Serializable]
	[Table( "Promozioni" )]
	public abstract class Promozione : INotifyPropertyChanged, IValidatableObject {

		#region Attributi

		[Key]
		[DatabaseGenerated( DatabaseGeneratedOption.None )]
		public short id {
			get; set;
		}

		[Required]
		public string descrizione {
			get; set;
		}

		[Required]
		private bool _attiva;
		public bool attiva {
			get {
				return _attiva;
			}
			set {
				if( _attiva != value ) {
					_attiva = value;
					OnPropertyChanged( "attiva" );
				}
			}
		}

		[Required]
		public bool esclusiva {
			get; set;
		}

		[Required]
		public bool discrezionale {
			get; set;
		}

		[Required]
		public bool attivaSuStampe {
			get; set;
		}

		[Required]
		public bool attivaSuFile {
			get; set;
		}

		[Required]
		public short priorita {
			get; set;
		}

		#endregion Attributi

		#region Uguaglianza

		public override int GetHashCode() {
			return 17 + 31 * id.GetHashCode();
		}

		public override bool Equals( object obj ) {

			bool sonoUguali = false;

			if( obj is Promozione ) {

				Promozione altra = (Promozione)obj;

				if( this.id != altra.id ) {
					sonoUguali = (this.id == altra.id);
				}
			}

			return sonoUguali;
		}

		#endregion Uguaglianza

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

		#region Interfaccia IValidatableObject

		public virtual IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> errors = new List<ValidationResult>();
			return errors;
		}

		#endregion Interfaccia IValidatableObject
	}
}
