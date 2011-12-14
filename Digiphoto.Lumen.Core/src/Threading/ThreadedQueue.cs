using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Digiphoto.Lumen.Threading {

	public class ThreadedQueue<T> : ThreadedQueueBase<T> {

		Action<T> _processItemAction = null;

		public ThreadedQueue( Action<T> processItemAction ) : this( null, ThreadPriority.Normal, processItemAction ) {
		}

		public ThreadedQueue( string name, Action<T> processItemAction ) : this( name, ThreadPriority.Normal, processItemAction ) {
		}

		public ThreadedQueue( string name, ThreadPriority priority,	Action<T> processItemAction ) : this( name, priority, false, processItemAction ) {
		}

		public ThreadedQueue( string name, ThreadPriority priority,	bool isBackground, Action<T> processItemAction ) : base( name, priority, isBackground ) {
			_processItemAction = processItemAction;
		}

		protected override void ProcessItem( T item ) {
			_processItemAction( item );
		}
	}
}

