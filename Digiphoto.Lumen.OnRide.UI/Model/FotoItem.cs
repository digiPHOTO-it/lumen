using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.OnRide.UI.Model {

	public class FotoItem : INotifyPropertyChanged {

	public FotoItem( FileInfo fileInfo ) {
		this.fileInfo = fileInfo;
		this.daTaggare = true;
	}

	public FileInfo fileInfo {
		get; set;
	}

	public string tag {
		get; set;
	}

	private bool _daTaggare;
	public bool daTaggare {
		get {
			return _daTaggare;
		}
		set {
			if( _daTaggare != value ) {
				_daTaggare = value;
				OnPropertyChanged( "daTaggare" );
			}
		}
	}

	private bool _daEliminare;
	public bool daEliminare {
		get {
			return _daEliminare;
		}
		set {
			if( _daEliminare != value ) {
				_daEliminare = value;
				OnPropertyChanged( "daEliminare" );
			}
		}
	}


	public string nomeFileTag {
		get {
			return fileInfo == null ? null : fileInfo.FullName + ".tag.txt";
		}
	}

	#region INotifyPropertyChanged
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
