using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[Table( "ConsumiCartaGiornalieri" )]	
	public partial class ConsumoCartaGiornaliero : INotifyPropertyChanged {

		#region Attribute

		[Key]
		public System.Guid id { get; set; }

		[Required]
		public System.DateTime giornata { get; set; }

		[Required]
		public System.DateTime tempo { get; set; }

		public short totFogli { get; set; }

		public short diCuiProvini { get; set; }

		public short diCuiFoto { get; set; }

		[ForeignKey( "formatoCarta" )]
		public System.Guid formatoCarta_id { get; set; }

		public virtual FormatoCarta formatoCarta { get; set; }

		#endregion Attribute

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
