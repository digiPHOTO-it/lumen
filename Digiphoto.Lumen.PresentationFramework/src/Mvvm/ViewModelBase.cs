﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Digiphoto.Lumen.Eventi;
using log4net;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.UI.Mvvm.Event;

namespace Digiphoto.Lumen.UI.Mvvm
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable {

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( ViewModelBase ) );

		public IDialogProvider dialogProvider {
			get;
			set;
		}

		public ITrayIconProvider trayIconProvider
		{
			get;
			set;
		}

        #region Constructor

        protected ViewModelBase()
        {
			_giornale.Debug( "Costruzione ViewModel : " + this.GetType() );

		}

        #endregion // Constructor

        #region DisplayName

        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        public virtual string DisplayName { get; protected set; }

        #endregion // DisplayName

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            this.OnDispose();
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
        }


#if DEBUG
		/// <summary>
		/// Useful for ensuring that ViewModel objects are properly garbage collected.
		/// </summary>
		~ViewModelBase()
        {
            string msg = string.Format("{0} ({1}) ({2}) Finalized", this.GetType().Name, this.DisplayName, this.GetHashCode());
            System.Diagnostics.Debug.WriteLine(msg);
        }
#endif

        #endregion // IDisposable Members

		#region Design Mode
		private static bool? _isInDesignMode;

		/// <summary>
		/// Gets a value indicating whether the control is in design mode
		/// (running under Blend or Visual Studio).
		/// </summary>
		[SuppressMessage(
			"Microsoft.Performance",
			"CA1822:MarkMembersAsStatic",
			Justification = "Non static member needed for data binding" )]
		public bool IsInDesignMode {
			get {
				return IsInDesignModeStatic;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the control is in design mode
		/// (running in Blend or Visual Studio).
		/// </summary>
		[SuppressMessage(
			"Microsoft.Security",
			"CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
			Justification = "The security risk here is neglectible." )]
		public static bool IsInDesignModeStatic {
			get {
				if( !_isInDesignMode.HasValue ) {
#if SILVERLIGHT
                    _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
#if WIN8
                    _isInDesignMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
					var prop = DesignerProperties.IsInDesignModeProperty;
					_isInDesignMode
						= (bool)DependencyPropertyDescriptor
									 .FromProperty( prop, typeof( FrameworkElement ) )
									 .Metadata.DefaultValue;

					// Just to be sure
					if( !_isInDesignMode.Value
						&& Process.GetCurrentProcess().ProcessName.StartsWith( "devenv", StringComparison.Ordinal ) ) {
						_isInDesignMode = true;
					}
#endif
#endif
				}

				return _isInDesignMode.Value;
			}
		}
		#endregion

		#region Popup

		private EventHandler _openPopupDialogRequest;
		public event EventHandler openPopupDialogRequest
		{
			add
			{
				_openPopupDialogRequest -= value;
				_openPopupDialogRequest += value;
			}

			remove
			{
				_openPopupDialogRequest -= value;
			}
		}

		protected virtual void RaisePopupDialogRequest( OpenPopupRequestEventArgs eventArgs ) {
			_openPopupDialogRequest?.Invoke( this, eventArgs );
		}

		#endregion Popup

	}
}