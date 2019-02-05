using Digiphoto.Lumen.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	/// <summary>
	/// La Promo ha un limite di innesco con la quantità di un prodotto.
	/// Il regalo è un prodotto con una quantità
	/// </summary>

	public class PromoProdXProd : Promozione {

		public const String DESCRIZ = "Prendi 'N' prodotti e ne ricevi un secondo  in omaggio'";

		#region Attributi da deserializzare

		private short _qtaDaInnesco;
		public short qtaInnesco {
			get {
				return _qtaDaInnesco;
			}
			set {
				if( _qtaDaInnesco != value ) {
					_qtaDaInnesco = value;
					OnPropertyChanged( "qtaInnesco" );
				}
			}
		}

		private Prodotto _prodottoInnesco;
		public virtual Prodotto prodottoInnesco {
			get {
				return _prodottoInnesco;
			}
			set {
				if( _prodottoInnesco != value ) {
					_prodottoInnesco = value;
					OnPropertyChanged( "prodottoInnesco" );
				}
			}
		}

		[ForeignKey( "prodottoInnesco" )]
		public Guid prodottoInnesco_id;

		private short _qtaElargito;
		public short qtaElargito {
			get {
				return _qtaElargito;
			}
			set {
				if( _qtaElargito != value ) {
					_qtaElargito = value;
					OnPropertyChanged( "qtaOmaggio" );
				}
			}
		}

		private Prodotto _prodottoElargito;
		public virtual Prodotto prodottoElargito {
			get {
				return _prodottoElargito;
			}
			set {
				if( _prodottoElargito != value ) {
					_prodottoElargito = value;
					OnPropertyChanged( "prodottoElargito" );
				}
			}
		}

		[ForeignKey( "prodottoElargito" )]
		public Guid prodottoElargito_id;

		#endregion Attributi da deserializzare

		public override IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> res = new List<ValidationResult>( base.Validate( validationContext ) );

			if( this.attiva ) {
				
				if( qtaInnesco < 1 || qtaElargito < 1 ) {
					ValidationResult vr = new ValidationResult( "Quantità promozione n.3 non valide", new string[] { this.GetType().Name } );
					res.Add( vr );
				}

				if( prodottoInnesco == null ) {
					ValidationResult vr = new ValidationResult( "Prodotto elargito non valido (promo n.3)", new string[] { this.GetType().Name } );
					res.Add( vr );
				}

				if( prodottoElargito == null ) {
					ValidationResult vr = new ValidationResult( "Prodotto elargito non valido (promo n.3)", new string[] { this.GetType().Name } );
					res.Add( vr );
				}

			}

			return res;
		}

	}
}
