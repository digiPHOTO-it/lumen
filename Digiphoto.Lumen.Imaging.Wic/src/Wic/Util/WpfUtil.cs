using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup.Primitives;

namespace Digiphoto.Lumen.Imaging.Wic.Util {

	public static class WpfUtil {

		public static IList<DependencyProperty> GetDependencyProperties( Object element ) {

			IList<DependencyProperty> properties = new List<DependencyProperty>();
			MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor( element );
			if( markupObject != null ) {
				foreach( MarkupProperty mp in markupObject.Properties ) {
					if( mp.DependencyProperty != null ) {
						properties.Add( mp.DependencyProperty );
					}
				}
			}

			return properties;
		}
	}
}
