using System;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Digiphoto.Lumen.Model.Util;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	// Questi attributi sono transienti e non li gestisco sul database.
	// Ci penserò io a riempirli a mano

	[MetadataType( typeof( Fotografia ) )]
	[Table("Fotografie")]
	public partial class Fotografia : IValidatableObject, INotifyPropertyChanged {


		#region Attributi

		[Key]
		public System.Guid id { get; set; }

		[Required]
		public string nomeFile { get; set; }

		public Nullable<System.DateTime> dataOraScatto { get; set; }
		
		private string _didascalia;
		public string didascalia {
			get {
				return _didascalia;
			}
			set {
				if( value != _didascalia ) {
					_didascalia = value;
					OnPropertyChanged( "didascalia" );
				}
			}
		}

		[Required]
		public System.DateTime dataOraAcquisizione { get; set; }

		public int numero { get; set; }

		public Nullable<short> faseDelGiorno { get; set; }
		
		[Required]
		public System.DateTime giornata { get; set; }
		
		public string correzioniXml { get; set; }
		
		private short _contaStampata;
		public short contaStampata {
			get {
				return _contaStampata;
			}
			set {
				if( value != _contaStampata ) {
					_contaStampata = value;
					OnPropertyChanged( "contaStampata" );
				}
			}
		}

		public short contaMasterizzata { get; set; }

		[ForeignKey( "evento" )]
		public Nullable<System.Guid> evento_id { get; set; }

		public virtual Evento evento { get; set; }

		[ForeignKey( "fotografo" )]
		public string fotografo_id { get; set; }

		[Required]
		public virtual Fotografo fotografo { get; set; }
		
		public Nullable<bool> miPiace { get; set; }


		#endregion Attributi

		// ??
		public static Boolean compNumFoto {
			get;
			set;
		}
		
		/// <summary>
		/// Questa è la scritta che faccio vedere al cliente per ordinare la foto
		/// </summary>
		public string etichetta {
			get {
				if( compNumFoto ) {
					return CompNumFoto.getStringValue( numero );
				}
				return numero.ToString();
			}
		}

		#region Immagini 

		public IImmagine imgOrig { get; set; }

		private IImmagine _imgProvino;
		public IImmagine imgProvino {
			get {
				return _imgProvino;
			}
			set {
				if( value != _imgProvino ) {
					_imgProvino = value;
					OnPropertyChanged( "imgProvino" );
				}
			}
		}

		private IImmagine _imgRisultante;
		public IImmagine imgRisultante {
			get {
				return _imgRisultante;
			}
			set {
				if( value != _imgRisultante ) {
					_imgRisultante = value;
					OnPropertyChanged( "imgRisultante" );
				}
			}
		}

		#endregion Immagini

		public override string ToString() {
			return String.Format( "Num.{0} del={1}", etichetta, dataOraAcquisizione.ToShortDateString() );
		}

		#region IValidatableObject

		public IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {

			List<ValidationResult> errors = new List<ValidationResult>();
			// TODO
			return errors;
		}

		#endregion IValidatableObject

		#region Uguaglianza

		public override bool Equals( object altro ) {
			bool uguali = false;
			if( altro != null && altro is Fotografia ) {
				uguali = this.id.Equals( ((Fotografia)altro).id );
			}

			return uguali;
		}

		public override int GetHashCode() {
			int hash = 7;
			hash = 31 * hash + (null == this.id ? 0 : this.id.GetHashCode());
			return hash;
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
	}
}
