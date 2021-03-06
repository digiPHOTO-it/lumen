﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( Prodotto ) )]
	[Serializable]
	[Table( "Prodotti" )]
	public abstract class Prodotto : IValidatableObject, INotifyPropertyChanged {

		#region Attributi

		public System.Guid id {
			get; set;
		}

		[Required]
		public string descrizione {
			get; set;
		}

		public decimal prezzo {
			get; set;
		}

		public bool attivo {
			get; set;
		}

		public Nullable<short> ordinamento {
			get; set;
		}

		public string tipologia {
			get; set;
		}

		#endregion Attributi


		#region Interfaccia IValidatableObject

		public IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> errors = new List<ValidationResult>();

			if( this.descrizione == null || this.descrizione.Length < 1 || this.descrizione.Length > 100 ) {
				ValidationResult vr = new ValidationResult( "descrizione formato carta non valido", new string[] { "descrizione" } );
				errors.Add( vr );
			}

			return errors;
		}

		#endregion Interfaccia IValidatableObject


		#region Interfaccia INotifyPropertyChanged

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( string propertyName ) {
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		protected virtual void OnPropertyChanged( PropertyChangedEventArgs e ) {
			if( PropertyChanged != null )
				PropertyChanged( this, e );
		}
		
		#endregion Interfaccia INotifyPropertyChanged	


		#region Uguaglianza

		public override bool Equals( object obj ) {

			bool isEqual = false;
			if( obj is Prodotto ) {
				Prodotto altro = (Prodotto)obj;
				isEqual = this.id.Equals( altro.id );
			}
			return isEqual;
		}

		public override int GetHashCode() {
			return this.id != null ? this.id.GetHashCode() : 0;
		}

		#endregion Uguaglianza

	}
}
