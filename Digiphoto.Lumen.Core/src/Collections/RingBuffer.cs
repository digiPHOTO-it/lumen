using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using log4net;

namespace Digiphoto.Lumen.Core.Collections {

	public class RingBuffer<T> : IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged {

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( RingBuffer<T> ) );

		private int _start;
		private T [] _items;

		public RingBuffer( int size ) {
			this.Size = size;
			_items = new T [size];
			Clear();
		}

		public int Size {
			get;
			private set;
		}

		public int Count {
			get;
			private set;
		}

		public bool IsFull {
			get {
				return Count == Size;
			}
		}

		public bool IsEmpty {
			get {
				return Count == 0;
			}
		}

		/// <summary>
		/// Ritorno l'elemento di testa, cioè l'ultimo inserito (il più fresco, il più nuovo)
		/// </summary>
		public T HeadElement {
			get {
				if( IsEmpty )
					return default( T );

				int end = ((_start + Count) % Size) - 1;
				if( end < 0 )
					end = Size - 1;

				return _items [end];
			}
		}

		public void Write( T item ) {
			
			int end = (_start + Count) % Size;

			T oldItem = _items [end];
			_items [end] = item;

			try {
				if( IsFull ) {
					// Devo eliminare l'elemento che vado a sovrascrivere. Nel mio caso è sempre in posizione 0 perché è la coda.
					this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, oldItem, 0 ) );
				}
				// Aggiungo elemento alla collection
				this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, item ) );
			} catch( Exception ee ) {
				_giornale.Warn( "notifica collection changed fallita", ee );
			}

			if( Count == Size )
				_start = (_start + 1) % Size; /* full, overwrite */
			else
				++Count;
		}

		public T Read() {
			T item = _items [_start];
			_start = (_start + 1) % Size;
			--Count;
			this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item, 0 ) );
			return item;
		}

		public void Clear() {
			_start = 0;
			Count = 0;
			this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
		}

		public IEnumerator<T> GetEnumerator() {

			int resto = (_start + Count) % Size;
			int end = _start + Count;

			int bufferIndex = _start;
			for( int i = _start; i < end; i++, bufferIndex++ ) {
				if( bufferIndex == Size )
					bufferIndex = 0;

				yield return _items [bufferIndex];
			}
		}


		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator)GetEnumerator();
		}

		#region INotifyCollectionChanged

		public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

		private void OnCollectionChanged( NotifyCollectionChangedEventArgs e ) {
			if( this.CollectionChanged != null )
				this.CollectionChanged( this, e );
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged( PropertyChangedEventArgs e ) {
			if( this.PropertyChanged != null )
				this.PropertyChanged( this, e );
		}

		#endregion

	}

}
