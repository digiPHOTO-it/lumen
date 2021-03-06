﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[MetadataType( typeof( ScaricoCard ) )]
	[Serializable]
	[Table( "ScarichiCards" )]
	public partial class ScaricoCard : INotifyPropertyChanged {

		#region Attributi

		[Key]
		public System.Guid id { get; set; }

		public System.DateTime tempo { get; set; }

		public short totFoto { get; set; }

		public System.DateTime giornata { get; set; }

		[ForeignKey("fotografo")]
		public string fotografo_id { get; set; }

		public virtual Fotografo fotografo { get; set; }

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
