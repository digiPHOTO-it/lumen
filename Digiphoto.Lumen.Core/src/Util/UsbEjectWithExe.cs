using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Digiphoto.Lumen.Util
{
	public class UsbEjectWithExe
	{

		public int ExitCode;

		public Exception RunException;

		public StringBuilder Output;

		public StringBuilder Error;

		public static bool usbEject(char driveLetter)
		{
			String output = UsbEjectWithExe.RunExecutable(@"Resources\RemoveDrive.exe", driveLetter + ":", null).Output.ToString();
			if (output.Contains("success"))
				return true;
			else
				return false;
		}

		public static UsbEjectWithExe RunExecutable(string executablePath, string arguments, string workingDirectory)
		{
			UsbEjectWithExe runResults = new UsbEjectWithExe
			{
				Output = new StringBuilder(),
				Error = new StringBuilder(),
				RunException = null
			};

			try
			{
				if (File.Exists(executablePath))
				{
					using (Process proc = new Process())
					{
						proc.StartInfo.FileName = executablePath;
						proc.StartInfo.Arguments = arguments;
						proc.StartInfo.WorkingDirectory = workingDirectory;
						proc.StartInfo.UseShellExecute = false;
						proc.StartInfo.CreateNoWindow = true;
						proc.StartInfo.RedirectStandardOutput = true;
						proc.StartInfo.RedirectStandardError = true;
						proc.OutputDataReceived +=
							(o, e) => runResults.Output.Append(e.Data).Append(Environment.NewLine);
						proc.ErrorDataReceived +=
							(o, e) => runResults.Error.Append(e.Data).Append(Environment.NewLine);
						proc.Start();
						proc.BeginOutputReadLine();
						proc.BeginErrorReadLine();
						proc.WaitForExit();
						runResults.ExitCode = proc.ExitCode;
					}
				}
				else
				{
					throw new ArgumentException("Invalid executable path.", "executablePath");
				}
			}
			catch (Exception e)
			{
				runResults.RunException = e;
			}
			return runResults;
		}
	}
}
