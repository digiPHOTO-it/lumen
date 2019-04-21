using System.ComponentModel;

namespace Digiphoto.Lumen.OnRide.UI.Config {

	/// <summary>
	/// Il programma può 
	/// </summary>
	public enum RunningMode {

		/// <summary>
		/// Esiste un operatore che decide cosa acquisire
		/// e associa i barcode alle foto.
		/// </summary>
		Presidiato,

		/// <summary>
		/// Il programma è abbandonato a se stesso.
		/// Non c'è associazione con i talloncini barcode
		/// quindi vengono solo acquisite le foto.
		/// </summary>
		Automatico
	} 

	public class UserConfigOnRide : INotifyPropertyChanged {

		private string _idFotografo;
		public string idFotografo {
			get {
				return _idFotografo;
			}
			set {
				if( _idFotografo != value ) {
					_idFotografo = value;
					OnPropertyChanged( "idFotografo" );
				}
			}
		}

		private string _nomeMaschera;
		public string nomeMaschera {
			get {
				return _nomeMaschera;
			}
			set {
				if( _nomeMaschera != value ) {
					_nomeMaschera = value;
					OnPropertyChanged( "nomeMaschera" );
				}
			}
		}
		
		private bool _isMascheraAttiva;
		public bool isMascheraAttiva {
			get {
				return _isMascheraAttiva;
			}
			set {
				if( _isMascheraAttiva ) {
					_isMascheraAttiva = value;
					OnPropertyChanged( "isMascheraAttiva" );
				}
			}
		}

		private RunningMode _runningMode;
		public RunningMode runningMode {
			get {
				return _runningMode;
			}
			set {
				if( _runningMode != value ) {
					_runningMode = value;
					OnPropertyChanged( "runningMode" );
				}
			}
		}

		/// <summary>
		/// Orario che separa la mattina dal pomeriggio
		/// nel formato hh:mm (5 caratteri)
		/// </summary>
		private string _orarioSeparaMatPom;
		public string orarioSeparaMatPom {
			get {
				return _orarioSeparaMatPom;
			}
			set {
				if( _orarioSeparaMatPom != value ) {
					_orarioSeparaMatPom = value;
					OnPropertyChanged( "orarioSeparaMatPom" );
				}
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		public void OnPropertyChanged( string name ) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if( handler != null ) {
				handler( this, new PropertyChangedEventArgs( name ) );
			}
		}
	}
}
