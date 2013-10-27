using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Collections;

namespace Digiphoto.Lumen.UI.Util {


	/// <summary>
	/// Qui ho trovato tanto materiale:
	/// http://stackoverflow.com/questions/636383/wpf-ways-to-find-controls
	/// se dovesse servire si può integrare qui dentro.
	/// </summary>
	public static class AiutanteUI {

		/// <summary>
		/// Questo è della microsoft:
		/// http://msdn.microsoft.com/en-us/library/bb613579.aspx
		/// </summary>
		public static CHILDTYPE FindVisualChild<CHILDTYPE>( DependencyObject obj ) where CHILDTYPE : DependencyObject {
			return FindVisualChild<CHILDTYPE>( obj, null );
		}

		public static CHILDTYPE FindVisualChild<CHILDTYPE>( DependencyObject depObj, string childName ) where CHILDTYPE : DependencyObject {

			// Confirm obj is valid. 
			if( depObj == null )
				return null;

			// success case
			if( depObj is CHILDTYPE ) {
				// Se il nome richiesto è indicato, allora lo testo. Altrimenti, siccome il tipo coincide, l'ho già trovato.
				if( childName == null || ((FrameworkElement)depObj).Name == childName )
					return depObj as CHILDTYPE;
			}

			for( int i = 0; i < VisualTreeHelper.GetChildrenCount( depObj ); i++ ) {
				DependencyObject child = VisualTreeHelper.GetChild( depObj, i );

				//DFS Depth-First Search (Ricorsione)
				CHILDTYPE obj = FindVisualChild<CHILDTYPE>( child, childName );

				if( obj != null )
					return obj;
			}

			return null;
		}

		public static CHILDTYPE FindFirstChild<CHILDTYPE>( FrameworkElement element ) where CHILDTYPE : FrameworkElement {
			int childrenCount = VisualTreeHelper.GetChildrenCount( element );
			var children = new FrameworkElement[childrenCount];

			for( int i = 0; i < childrenCount; i++ ) {
				var child = VisualTreeHelper.GetChild( element, i ) as FrameworkElement;
				children[i] = child;
				if( child is CHILDTYPE )
					return (CHILDTYPE)child;
			}

			for( int i = 0; i < childrenCount; i++ )
				if( children[i] != null ) {
					var subChild = FindFirstChild<CHILDTYPE>( children[i] );
					if( subChild != null )
						return subChild;
				}

			return null;
		}

	}
}
