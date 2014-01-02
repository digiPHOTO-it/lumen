using System;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Digiphoto.Lumen.Model.Util;


namespace Digiphoto.Lumen.Model {

	// Questi attributi sono transienti e non li gestisco sul database.
	// Ci penserò io a riempirli a mano

	[MetadataType(typeof(Fotografia))]
	public partial class Fotografia : IValidatableObject, INotifyPropertyChanged {

		public static Boolean compNumFoto
		{
			get;
			set;
		}

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


		public override string ToString() {
			return String.Format( "Num.{0} del={1}", etichetta, dataOraAcquisizione.ToShortDateString() );
		}

		/// <summary>
		/// Questa è la scritta che faccio vedere al cliente per ordinare la foto
		/// </summary>
		public string etichetta {
			get {
				if(compNumFoto){
					return CompNumFoto.getStringValue(numero);
				}
				return numero.ToString();
			}
		}

		public IEnumerable<ValidationResult> Validate( ValidationContext validationContext ) {
			
			List<ValidationResult> errors = new List<ValidationResult>();

			// TODO 
			
			return errors;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged( string propertyName ) {

			PropertyChangedEventHandler handler = this.PropertyChanged;
			if( handler != null ) {
				var e = new PropertyChangedEventArgs( propertyName );
				handler( this, e );
			}
		}

		public string faseDelGiornoString {
			get {
				return FaseDelGiornoUtil.valoreToString( faseDelGiorno );
			}
		}

	}

}
