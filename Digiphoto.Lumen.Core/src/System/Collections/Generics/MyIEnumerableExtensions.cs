using System.Linq;

namespace System.Collections.Generic {


	/// <summary>
	/// Questa classe l'ho presa da qui:
	/// http://www.singingeels.com/Articles/Extending_LINQ__Specifying_a_Property_in_the_Distinct_Function.aspx
	/// 
	/// Serve per far funzionare il distinct nelle espressioni Linq 
	/// (che come tutte le cose della Microsoft sono studiate per non funzionare o per funzionare a cacchio).
	/// </summary>

	public static class MyIEnumerableExtensions {

		public static IEnumerable<T> Distinct<T>( this IEnumerable<T> source, Func<T, object> uniqueCheckerMethod ) {
			return source.Distinct( new GenericComparer<T>( uniqueCheckerMethod ) );
		}

		class GenericComparer<T> : IEqualityComparer<T> {
			public GenericComparer( Func<T, object> uniqueCheckerMethod ) {
				this._uniqueCheckerMethod = uniqueCheckerMethod;
			}

			private Func<T, object> _uniqueCheckerMethod;

			bool IEqualityComparer<T>.Equals( T x, T y ) {
				return this._uniqueCheckerMethod( x ).Equals( this._uniqueCheckerMethod( y ) );
			}

			int IEqualityComparer<T>.GetHashCode( T obj ) {
				return this._uniqueCheckerMethod( obj ).GetHashCode();
			}
		}
	}
}
