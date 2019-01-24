using Digiphoto.Lumen.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Digiphoto.Lumen.Model {

	/// <summary>
	/// Simile alla promozione 3x2 del supermercato.
	/// Nel nostro caso Prendi 6 Paghi 5 di prodotti omogenei (stampe o file)
	/// </summary>

	public class PromoPrendiNPaghiM : Promozione {

		public const String DESCRIZ = "Prendi 'N' prodotti e ne paghi 'M'";

		#region Attributi da deserializzare

		private short _qtaDaPrendere;
		public short qtaDaPrendere {
			get {
				return _qtaDaPrendere;
			}
			set {
				if( _qtaDaPrendere != value ) {
					_qtaDaPrendere = value;
					OnPropertyChanged( "qtaDaPrendere" );
				}
			}
		}

		private short _qtaDaPagare;
		public short qtaDaPagare {
			get {
				return _qtaDaPagare;
			}
			set {
				if( _qtaDaPagare != value ) {
					_qtaDaPagare = value;
					OnPropertyChanged( "qtaDaPagare" );
				}
			}
		}

		#endregion Attributi da deserializzare

		public override IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> res = new List<ValidationResult>( base.Validate( validationContext ) );

			if( this.attiva ) {
				if( qtaDaPrendere <= 1 || qtaDaPagare >= qtaDaPrendere ) {
					ValidationResult vr = new ValidationResult( "Quantità promozione NxM non valide", new string[] { this.GetType().Name } );
					res.Add( vr );
				}
			}

			return res;
		}

	}
}
