using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Digiphoto.Lumen.SelfService.MobileUI.AutoCloseWindow
{
	public class AutoClosingMessageBox
	{

		const int WM_CLOSE = 0x0010;
		const int WM_DESTROY = 0x0002;

		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
		[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

		System.Threading.Timer _timeoutTimer;
		string _caption;

		MessageBoxResult _result;
		MessageBoxResult _timerResult;

		private AutoClosingMessageBox(Application app, string text, string caption, int timeout, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult timerResult = MessageBoxResult.None)
		{
			_caption = caption;
			_timerResult = timerResult;

			_timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, System.Threading.Timeout.Infinite);

			using (_timeoutTimer)
			{
				_result = MessageBox.Show(text, caption, MessageBoxButton.YesNoCancel);
			}
		}

		public static MessageBoxResult Show(Application app, string text, string caption, int timeout, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult timerResult = MessageBoxResult.None)
		{
			return new AutoClosingMessageBox(app, text, caption, timeout, buttons, timerResult)._result;
        }

		private void OnTimerElapsed(object state)
		{
			IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
			if (mbWnd != IntPtr.Zero)
				SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			_timeoutTimer.Dispose();
			_result = _timerResult;
		}
			
	}
}
