using System;
using System.ComponentModel;

namespace Digiphoto.Lumen.SelfService.SlideShow.Config {

	public class UserConfig : INotifyPropertyChanged, ICloneable {

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

		private int _intervallo;
		public int intervallo {
			get {
				return _intervallo;
			}
			set {
				if( _intervallo != value ) {
					_intervallo = value;
					OnPropertyChanged( "intervallo" );
				}
			}
		}

		private short _numRighe;
		public short numRighe {
			get {
				return _numRighe;
			}
			set {
				if( _numRighe != value ) {
					_numRighe = value;
					OnPropertyChanged( "numRighe" );
				}
			}
		}

		private short _numColonne;
		public short numColonne {
			get {
				return _numColonne;
			}
			set {
				if( _numColonne != value ) {
					_numColonne = value;
					OnPropertyChanged( "numColonne" );
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

		public object Clone() {
			UserConfig newConfig = (UserConfig)this.MemberwiseClone();
			return newConfig;
		}
	}
}
