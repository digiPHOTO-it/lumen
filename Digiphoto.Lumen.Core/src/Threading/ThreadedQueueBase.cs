using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Digiphoto.Lumen.Threading {

	public enum PendingItemAction {
		ProcessPendingItems,
		AbortPendingItems
	}

	public abstract class ThreadedQueueBase<T> : WorkerThreadBase {

		private AutoResetEvent _itemArrived;
		private Queue<T> _itemQueue;
		private bool _abortPendingItems;

		protected ThreadedQueueBase()
			: this( null, ThreadPriority.Normal ) {
		}

		protected ThreadedQueueBase( string name )
			: this( name, ThreadPriority.Normal ) {
		}

		protected ThreadedQueueBase( string name, ThreadPriority priority )
			: this( name, priority, false ) {
		}

		protected ThreadedQueueBase( string name, ThreadPriority priority, bool isBackground )
			: base( name, priority, isBackground ) {
			_itemArrived = new AutoResetEvent( false );
			_itemQueue = new Queue<T>();
		}

		protected override void Work() {
			do {
				_itemArrived.WaitOne();
				do {
					if( _itemQueue.Count > 0 ) {
						System.Diagnostics.Trace.WriteLine(
							string.Format( "'{0}'::Work Abort='{1}'", Name, _abortPendingItems ) );
						if( _abortPendingItems ) {
							break;
						}

						lock( ((System.Collections.ICollection)_itemQueue).SyncRoot ) {
							T item = _itemQueue.Dequeue();
							if( item != null ) {
								ProcessItem( item );
							}
						}
					}
				} while( _itemQueue.Count > 0 );
				Thread.Sleep( 0 );
			} while( !Disposing && !StopRequested );
		}

		public void EnqueueItem( T item ) {
			ThrowIfDisposedOrDisposing();
			_itemQueue.Enqueue( item );
			_itemArrived.Set();
		}

		public new void Stop() {
			Stop( PendingItemAction.ProcessPendingItems );
		}

		public void Stop( PendingItemAction pendingItemAction ) {
			ThrowIfDisposedOrDisposing();
			//set pendingItemAction
			_abortPendingItems = pendingItemAction == PendingItemAction.AbortPendingItems;

			System.Diagnostics.Trace.WriteLine(
				string.Format( "'{0}'::Work Stop requested Abort='{1}'", Name, _abortPendingItems ) );

			//signal item arrived
			_itemArrived.Set();

			_stopping.Set();
			
			// Se devo processare i pendenti, ed ho la coda ancora piena, aspetto all'infinito
			if( pendingItemAction == PendingItemAction.ProcessPendingItems && _itemQueue.Count > 0 )
				_stopped.WaitOne();
		}

		protected override void Dispose( bool disposing ) {
			base.Dispose( disposing );
			base.DisposeWaitHandle( _itemArrived );
		}

		public void Clear() {
			_itemQueue.Clear();
		}
		protected abstract void ProcessItem( T item );
	}
}
