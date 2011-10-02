using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Core.Applicazione {

	[TestFixture]
	public class LumenApplicationTest {

		[Test]
		public void istanzaSingolaTest() {
			LumenApplication actual1 = LumenApplication.Instance;
			LumenApplication actual2 = LumenApplication.Instance;
			Assert.AreSame( actual1, actual2 );
		}
	}
}
