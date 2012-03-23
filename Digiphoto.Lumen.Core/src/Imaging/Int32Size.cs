using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Imaging {

	public struct Int32Size {

		private static Int32Size vuoto = new Int32Size {
			Width = 0,
			Height = 0
		};

/*
		public Int32Size( int width, int height ) : this() {
			this.Width = width;
			this.Height = height;
		}
*/
		// Summary:
		//     Compares two rectangles for inequality.
		//
		// Parameters:
		//   int32Rect1:
		//     The first rectangle to compare.
		//
		//   int32Rect2:
		//     The second rectangle to compare.
		//
		// Returns:
		//     false if int32Rect1 and int32Rect2 have the same 
		//     System.Windows.Int32Rect.Width, and System.Windows.Int32Rect.Height;
		//     otherwise, if all of these values are the same, then true.
		public static bool operator !=( Int32Size s1, Int32Size s2 ) {
			return !Int32Size.Equals( s1, s2 );
		}

		//
		// Summary:
		//     Compares two rectangles for exact equality.
		//
		// Parameters:
		//   int32Rect1:
		//     The first rectangle to compare.
		//
		//   int32Rect2:
		//     The second rectangle to compare.
		//
		// Returns:
		//     true if int32Rect1 and int32Rect2 have the same 
		//     , System.Windows.Int32Rect.Width, and System.Windows.Int32Rect.Height;
		//     otherwise, false.
		public static bool operator ==( Int32Size s1, Int32Size s2 ) {
			return Int32Size.Equals( s1, s2 );
		}

		// Summary:
		//     Gets the empty rectangle, a special value that represents a rectangle with
		//     no position or area.
		//
		// Returns:
		//     An empty rectangle with no position or area.
		public static Int32Size Empty {
			get {
				return vuoto;
			}
		}

		//
		// Summary:
		//     Gets or sets the height of the rectangle.
		//
		// Returns:
		//     The height of the rectangle. The default value is 0.
		public int Height {
			get;
			set;
		}
		//
		// Summary:
		//     Gets a value indicating whether the rectangle is empty.
		//
		// Returns:
		//     true if the rectangle is empty; otherwise, false. The default value is true.
		public bool IsEmpty {
			get {
				return (Height == 0 && Width == 0);
			}
		}
		//
		// Summary:
		//     Gets or sets the width of the rectangle.
		//
		// Returns:
		//     The width of the rectangle. The default value is 0.
		public int Width {
			get;
			set;
		}

		// Summary:
		//     Determines whether the specified area is equal to this area.
		//
		// Parameters:
		//   value:
		//     The area to compare to the current area.
		//
		// Returns:
		//     true if both rectangles have the same System.Windows.Int32Rect.X, System.Windows.Int32Rect.Y,
		//     System.Windows.Int32Rect.Width, and System.Windows.Int32Rect.Height as this
		//     rectangle; otherwise, false.
		public bool Equals( Int32Size value ) {
			return Int32Size.Equals( this, value );
		}

		//
		// Summary:
		//     Determines whether the specified rectangle is equal to this rectangle.
		//
		// Parameters:
		//   o:
		//     The object to compare to the current rectangle.
		//
		// Returns:
		//     true if o is an System.Windows.Int32Rect and the same System.Windows.Int32Rect.X,
		//     System.Windows.Int32Rect.Y, System.Windows.Int32Rect.Width, and System.Windows.Int32Rect.Height
		//     as this rectangle; otherwise, false.
		public override bool Equals( object o ) {
			if( o != null && o is Int32Size )
				return Int32Size.Equals( this, (Int32Size)o );
			else
				return false;
		}
		//
		// Summary:
		//     Determines whether the specified rectangles are equal.
		//
		// Parameters:
		//   int32Rect1:
		//     The first rectangle to compare.
		//
		//   int32Rect2:
		//     The second rectangle to compare.
		//
		// Returns:
		//     true if int32Rect1 and int32Rect2 have the same System.Windows.Int32Rect.X,
		//     System.Windows.Int32Rect.Y, System.Windows.Int32Rect.Width, and System.Windows.Int32Rect.Height;
		//     otherwise, false.
		public static bool Equals( Int32Size s1, Int32Size s2 ) {
			if( s1.Width != s2.Width )
				return false;
			if( s1.Height != s2.Height )
				return false;
			return true;
		}


		public override int GetHashCode() {
			// TODO calcolare l'hash in base ai due attributi w e h
			return base.GetHashCode();
		}
	}
}
