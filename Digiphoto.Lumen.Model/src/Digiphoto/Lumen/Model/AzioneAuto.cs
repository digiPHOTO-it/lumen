using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[Table( "AzioniAutomatiche" )]
	public partial class AzioneAuto : INotifyPropertyChanged {

		public AzioneAuto() {
			this.attivo = true;
		}

		#region Attributi

		[Key]
		public System.Guid id { get; set; }

		[Required]
		public string nome { get; set; }

		[Required]
		public string correzioniXml { get; set; }

		public bool attivo { get; set; }

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
