﻿using System.Windows.Controls.Primitives;

namespace Digiphoto.Lumen.UI.Mvvm.MultiSelect {

	interface IMultiSelectCollectionView {

		void AddControl( Selector selector );
		void RemoveControl( Selector selector );

	}
}
