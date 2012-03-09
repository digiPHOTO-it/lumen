using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System;

namespace Digiphoto.Lumen.UI.Mvvm.MultiSelect {

	public class MultiSelectCollectionView<T> : ListCollectionView, IMultiSelectCollectionView {

		public MultiSelectCollectionView( IList list )
			: base( list ) {
			SelectedItems = new ObservableCollection<T>();
		}

		void IMultiSelectCollectionView.AddControl( Selector selector ) {
			this.controls.Add( selector );
			SetSelection( selector );
			selector.SelectionChanged += control_SelectionChanged;
		}

		void IMultiSelectCollectionView.RemoveControl( Selector selector ) {
			if( this.controls.Remove( selector ) ) {
				selector.SelectionChanged -= control_SelectionChanged;
			}
		}

		public ObservableCollection<T> SelectedItems {
			get;
			private set;
		}

		void SetSelection( Selector selector ) {
			MultiSelector multiSelector = selector as MultiSelector;
			ListBox listBox = selector as ListBox;

			if( multiSelector != null ) {
				multiSelector.SelectedItems.Clear();

				foreach( T item in SelectedItems ) {
					multiSelector.SelectedItems.Add( item );
				}
			} else if( listBox != null ) {
				listBox.SelectedItems.Clear();

				foreach( T item in SelectedItems ) {
					listBox.SelectedItems.Add( item );
				}
			}
		}

		void control_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
			if( !this.ignoreSelectionChanged ) {
				bool changed = false;

				this.ignoreSelectionChanged = true;

				try {
					foreach( T item in e.AddedItems ) {
						if( !SelectedItems.Contains( item ) ) {
							SelectedItems.Add( item );
							changed = true;
						}
					}

					foreach( T item in e.RemovedItems ) {
						if( SelectedItems.Remove( item ) ) {
							changed = true;
						}
					}

					if( changed ) {
						foreach( Selector control in this.controls ) {
							if( control != sender ) {
								SetSelection( control );
							}
						}
					}
				} finally {
					this.ignoreSelectionChanged = false;
				}
			}
		}

		bool ignoreSelectionChanged;
		List<Selector> controls = new List<Selector>();

		public void DeselectAll() {

			// Remove all the selected items
			SelectedItems.Clear();

			// Update the UI control.
			foreach( Control control in controls )
				SetSelection( (Selector)control );
		}

		public void SelectAll() {

			// Reload all the elements in the selected collection
			SelectedItems.Clear();
			foreach( T item in SourceCollection )
				SelectedItems.Add( item );

			// Update the UI control.
			foreach( Control control in controls )
				SetSelection( control as Selector );
		}

		protected override void RefreshOverride() {
			base.RefreshOverride();

			// Update the UI control.
			foreach( Control control in controls )
				SetSelection( control as Selector );
		}

		/// <summary>
		///  Per un motivo che non mi è chiaro,
		///  quando attivo il filtro, il componente UI per esempio la ListBox, mi fa scatenare
		///  un cambio di selezione, dove vengono rimossi alcuni elementi.
		///  Per evitare ciò devo alzare il flag che ignora questo evento.
		///  Inoltre al termine devo riallineare gli elementi selezionati con un refresh.
		/// </summary>
		public override Predicate<object> Filter {
			get { 
				return base.Filter;
			}
			set { 

				this.ignoreSelectionChanged = true;

				// Qui mi provocherebbe un selection changed non voluto.
				base.Filter = value;

				this.ignoreSelectionChanged = false;

				// non fare Refresh(); perchè non funziona
			}
		}

	}
}