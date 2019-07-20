using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Digiphoto.Lumen.SelfService.MobileUI.Util {
	public class Clessidra : IDisposable {

		private Cursor _previousCursor;

		public Clessidra() {
			_previousCursor = Mouse.OverrideCursor;

			Mouse.OverrideCursor = Cursors.Wait;
		}

		#region IDisposable Members

		public void Dispose() {
			Mouse.OverrideCursor = _previousCursor;
		}

		#endregion
	}
}
