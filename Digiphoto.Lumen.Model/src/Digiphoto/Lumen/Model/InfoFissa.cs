using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[Table( "InfosFisse" )]
	public partial class InfoFissa : INotifyPropertyChanged {

		public InfoFissa() {
			this.id = "K";
			this.versioneDbCompatibile = "1.3";
			this.modoNumerazFoto = "X";
		}

		[Key]
		public string id { get; set; }
		
		public int ultimoNumFotogramma { get; set; }
		public Nullable<System.DateTime> dataUltimoScarico { get; set; }

		[Required]
		public string versioneDbCompatibile { get; set; }

		[Required]
		public string modoNumerazFoto { get; set; }

		public short pixelProvino { get; set; }
		public string idPuntoVendita { get; set; }
		public string descrizPuntoVendita { get; set; }
		public short numGiorniEliminaFoto { get; set; }

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
