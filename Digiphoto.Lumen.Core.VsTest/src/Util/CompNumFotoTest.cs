using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.src.Util;

namespace Digiphoto.Lumen.Core.VsTest.src.Util
{
	[TestClass]
	public class CompNumFotoTest
	{
		[TestMethod]
		public void CompNumFotoTest1()
		{
			string enc = CompNumFoto.getStringValue(36);
			long dec = CompNumFoto.getLongValue(enc);
			Assert.IsTrue(36 == dec);
			Console.WriteLine("From {0}, to {1}, to {2}", 36, enc, dec);
		}
		
		[TestMethod]
		public void CompNumFotoTest2()
		{

			for (long i = 0; i < 400000; ++i)
			{
				string enc = CompNumFoto.getStringValue(i);
				long dec = CompNumFoto.getLongValue(enc);
				Assert.IsTrue(i == dec);
				Console.WriteLine("From {0}, to {1}, to {2}", i, enc,dec);
			}

		}
	}
}
