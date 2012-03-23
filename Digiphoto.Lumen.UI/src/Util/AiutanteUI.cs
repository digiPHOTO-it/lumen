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
		public static childItem FindVisualChild<childItem>( DependencyObject obj ) where childItem : DependencyObject {

			for( int i = 0; i < VisualTreeHelper.GetChildrenCount( obj ); i++ ) {
				DependencyObject child = VisualTreeHelper.GetChild( obj, i );
				if( child != null && child is childItem )
					return (childItem)child;
				else {
					childItem childOfChild = FindVisualChild<childItem>( child );
					if( childOfChild != null )
						return childOfChild;
				}
			}
			return null;
		}

	}
}
