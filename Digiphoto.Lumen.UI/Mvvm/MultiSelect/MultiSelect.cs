using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Digiphoto.Lumen.UI.Mvvm.Event;
using System.Windows.Controls;

namespace Digiphoto.Lumen.UI.Mvvm.MultiSelect {

	public partial class MultiSelect : UIElement
	{

		private static Dictionary<Selector, IMultiSelectCollectionView> collectionViews;

		public static readonly DependencyProperty MaxNumSelectedItemProperty;

		public static readonly DependencyProperty IsEnabledProperty;

		static MultiSelect()
		{
			collectionViews = new Dictionary<Selector, IMultiSelectCollectionView>();
			
			MaxNumSelectedItemProperty = DependencyProperty.RegisterAttached("MaxNumSelectedItem", typeof(int), typeof(MultiSelect));

			IsEnabledProperty = DependencyProperty.RegisterAttached( "IsEnabled", typeof( bool ), typeof( MultiSelect ),new UIPropertyMetadata( IsEnabledChanged ) );
		}

		public static bool GetIsEnabled( Selector target ) {
			return (bool)target.GetValue(MultiSelect.IsEnabledProperty);
		}

		public static void SetIsEnabled( Selector target, bool value ) {
			target.SetValue(MultiSelect.IsEnabledProperty, value);
		}

		static void IsEnabledChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			Selector selector = sender as Selector;
			bool enabled = (bool)e.NewValue;

			if( selector != null ) {
				DependencyPropertyDescriptor itemsSourceProperty =
					DependencyPropertyDescriptor.FromProperty( Selector.ItemsSourceProperty, typeof( Selector ) );
				IMultiSelectCollectionView collectionView = selector.ItemsSource as IMultiSelectCollectionView;

				if( enabled ) {
					if( collectionView != null )
						collectionView.AddControl(selector, GetMaxNumSelectedItem(selector));
					itemsSourceProperty.AddValueChanged( selector, ItemsSourceChanged );
				} else {
					if( collectionView != null )
						collectionView.RemoveControl( selector );
					itemsSourceProperty.RemoveValueChanged( selector, ItemsSourceChanged );
				}
			}
		}

		static void ItemsSourceChanged( object sender, EventArgs e ) {
			Selector selector = sender as Selector;

			if( GetIsEnabled( selector ) ) {
				IMultiSelectCollectionView oldCollectionView;
				IMultiSelectCollectionView newCollectionView = selector.ItemsSource as IMultiSelectCollectionView;
				MultiSelect.collectionViews.TryGetValue(selector, out oldCollectionView);

				if( oldCollectionView != null ) {
					oldCollectionView.RemoveControl( selector );
					collectionViews.Remove( selector );
				}

				if( newCollectionView != null ) {
					newCollectionView.AddControl(selector, GetMaxNumSelectedItem(selector));
					MultiSelect.collectionViews.Add(selector, newCollectionView);
				}
			}
		}

		public static int GetMaxNumSelectedItem(Selector target)
		{
			return (int)target.GetValue(MultiSelect.MaxNumSelectedItemProperty);
		}

		public static void SetMaxNumSelectedItem(Selector target, int value)
		{
			target.SetValue(MultiSelect.MaxNumSelectedItemProperty, value);
		}

		public static readonly RoutedEvent SelectionFailedEvent = EventManager.RegisterRoutedEvent("SelectionFailed", RoutingStrategy.Bubble, typeof(SelectionFailedEventHandler), typeof(MultiSelect));

		public event SelectionFailedEventHandler SelectionFailed
		{
			add { AddHandler(SelectionFailedEvent, value); }
			remove { RemoveHandler(SelectionFailedEvent, value); }
		}

	}
}
