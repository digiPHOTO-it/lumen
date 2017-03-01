using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[Table("Giornate")]
	public partial class Giornata : INotifyPropertyChanged {

		#region Attributi

		private System.DateTime _id;
		[Key]
		public System.DateTime id {
			get {
				return _id;
			}
			set {
				if( _id != value ) {
					_id = value;
					OnPropertyChanged( "id" );
				}
			}
		}

		[Required]
		public System.DateTime orologio { get; set; }

		[Required]
		public decimal incassoDichiarato { get; set; }
		
		public string note { get; set; }

		[Required]
		private decimal _incassoPrevisto;
		public decimal incassoPrevisto {
			get {
				return _incassoPrevisto;
			}
			set {
				if( _incassoPrevisto != value ) {
					_incassoPrevisto = value;
					OnPropertyChanged( "incassoPrevisto" );
				}
			}
		}

		public string prgTaglierina1 { get; set; }
		public string prgTaglierina2 { get; set; }
		public string prgTaglierina3 { get; set; }

		public Nullable<short> totScarti { get; set; }

		[Required]
		public string firma { get; set; }

		#endregion Attributi

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
