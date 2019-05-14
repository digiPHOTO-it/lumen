using System;
using System.Runtime.InteropServices;

namespace Digiphoto.Lumen.Core.Util {

	public struct SystemTime {
		public ushort Year;
		public ushort Month;
		public ushort DayOfWeek;
		public ushort Day;
		public ushort Hour;
		public ushort Minute;
		public ushort Second;
		public ushort Millisecond;
	};

	public class Orologio {

		[DllImport( "kernel32.dll", EntryPoint = "GetSystemTime", SetLastError = true )]
		public extern static void Win32GetSystemTime( ref SystemTime sysTime );

		[DllImport( "kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true )]
		public extern static bool Win32SetSystemTime( ref SystemTime sysTime );

		public static void Set( DateTime newTempo ) {

			DateTime dateTime = newTempo.ToUniversalTime();

			SystemTime st = new SystemTime();
			// All of these must be short
			st.Year = (ushort)dateTime.Year;
			st.Month = (ushort)dateTime.Month;
			st.Day = (ushort)dateTime.Day;
			st.Hour = (ushort)dateTime.Hour;
			st.Minute = (ushort)dateTime.Minute;
			st.Second = (ushort)dateTime.Second;

			// invoke the SetSystemTime method now
			Win32SetSystemTime( ref st );
		}
	}

}
