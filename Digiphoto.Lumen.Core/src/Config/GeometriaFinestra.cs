using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Digiphoto.Lumen.Config
{

	/// <summary>
	/// Questa classe contiene le informazioni della posizione di una finestra
	/// Viene serializzata nella configurazione utente, per memorizzare la posizione 
	/// delle finestre pubbliche (es. Finestra Slide Show)
	/// </summary>
	public class GeometriaFinestra : INotifyPropertyChanged, ICloneable
	{

		private short _deviceEnum = -1;
		public short deviceEnum
		{
			get
			{
				return _deviceEnum;
			}
			set
			{
				if (_deviceEnum != value)
				{
					_deviceEnum = value;
					OnPropertyChanged("deviceEnum");
				}
			}
		}


#if false
		// TODO non capisco perché sia necessario memorizzare la dimensione dello schermo !!!
		//      la posso sempre ricavare a runtime !!!!
		// BOH chiedere a Edi
		private int _screenHeight;
		public int screenHeight
		{
			get
			{
				return _screenHeight;
			}
			set
			{
				if (_screenHeight != value)
				{
					_screenHeight = value;
					OnPropertyChanged("screenHeight");
				}
			}
		}


		// TODO non capisco perché sia necessario memorizzare la dimensione dello schermo !!!
		//      la posso sempre ricavare a runtime !!!!
		// BOH chiedere a Edi
		private int _screenWidth;
		public int screenWidth
		{
			get
			{
				return _screenWidth;
			}
			set
			{
				if (_screenWidth != value)
				{
					_screenWidth = value;
					OnPropertyChanged("screenWidth");
				}
			}
		}
#endif

		private bool _fullScreen;
		public bool fullScreen
		{
			get
			{
				return _fullScreen;
			}
			set
			{
				if (_fullScreen != value)
				{
					_fullScreen = value;
					OnPropertyChanged("fullScreen");
				}
			}
		}

		private int _Height;
		public int Height
		{
			get
			{
				return _Height;
			}
			set
			{
				if (_Height != value)
				{
					_Height = value;
					OnPropertyChanged("Height");
				}
			}
		}

		private int _Width;
		public int Width
		{
			get
			{
				return _Width;
			}
			set
			{
				if (_Width != value)
				{
					_Width = value;
					OnPropertyChanged("Width");
				}
			}
		}

		private int _Top;
		public int Top
		{
			get
			{
				return _Top;
			}
			set
			{
				if (_Top != value)
				{
					_Top = value;
					OnPropertyChanged("Top");
				}
			}
		}

		private int _Left;
		public int Left
		{
			get
			{
				return _Left;
			}
			set
			{
				if (_Left != value)
				{
					_Left = value;
					OnPropertyChanged("Left");
				}
			}
		}


#if false
		private int _BoundsX;
		public int BoundsX {
			get {
				return _BoundsX;
			}
			set {
				if( _BoundsX != value ) {
					_BoundsX = value;
					OnPropertyChanged( "BoundsX" );
				}
			}
		}

		private int _BoundsY;
		public int BoundsY {
			get {
				return _BoundsY;
			}
			set {
				if( _BoundsY != value ) {
					_BoundsY = value;
					OnPropertyChanged( "BoundsY" );
				}
			}
		}
#endif
		

		public event PropertyChangedEventHandler PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		public string ToDebugString() {

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat( "Dev={0} FullScr={1}\n", deviceEnum, fullScreen );
			sb.AppendFormat( "L={0} T={1} W={2} H={3}\n", Left, Top, Width, Height );

			return sb.ToString();
		}

		public object Clone() {
			GeometriaFinestra copia = (GeometriaFinestra)MemberwiseClone();
			return copia;
		}

		/// <summary>
		/// Indica che questo oggetto è empty cioè non inizializzato con valori reali
		/// </summary>
		/// <returns></returns>
		public bool isEmpty() {
			return ( _deviceEnum < 0 || _Width == 0 || _Height == 0 );
		}

	}
}
