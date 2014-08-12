using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Digiphoto.Lumen.UI.Pubblico {

	public class SnapshotPubblicoViewModel : ClosableWiewModel {

		private ImageSource _snapshotImageSource;

		public ImageSource snapshotImageSource {
			get {
				return _snapshotImageSource;
			}

			set {
				if( _snapshotImageSource != value ) {
					_snapshotImageSource = value;
					OnPropertyChanged( "snapshotImageSource" );
				}
			}
		}

	}
}
