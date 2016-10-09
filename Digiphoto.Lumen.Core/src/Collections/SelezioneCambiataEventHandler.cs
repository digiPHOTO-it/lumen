using System;
using System.Runtime.InteropServices;

namespace Digiphoto.Lumen.Core.Collections {

	[SerializableAttribute]
	[ComVisibleAttribute( true )]
	public delegate void SelezioneCambiataEventHandler( object sender, EventArgs e );
}