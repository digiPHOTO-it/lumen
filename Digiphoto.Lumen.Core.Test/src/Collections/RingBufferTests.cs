using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Core.Test.Collections {

	[TestClass()]
	public class RingBufferTest {

		[TestMethod]
		public void RingBufferClearTest() {

			int capacity = 5;
			RingBuffer<string> target = new RingBuffer<string>( capacity );
			target.Write( "100" );
			Assert.IsTrue( target.HeadElement.Equals( "100" ) );
			target.Write( "200" );
			Assert.IsTrue( target.HeadElement.Equals( "200" ) );
			target.Clear();
			Assert.IsTrue( target.Count == 0 );
			Assert.IsTrue( target.HeadElement == null );
		}

		[TestMethod()]
		[DeploymentItem( "Digiphoto.Lumen.Core.dll" )]
		public void RingBufferWrite() {

			RingBuffer<int> ring = new RingBuffer<int>( 3 );
			Assert.IsTrue( ring.Count == 0 );
			Assert.IsFalse( ring.IsFull );
			Assert.IsTrue( ring.IsEmpty );
			ring.Write( 1 );
			Assert.IsTrue( ring.Count == 1 );
			Assert.IsFalse( ring.IsFull );
			Assert.IsFalse( ring.IsEmpty );
			ring.Write( 2 );
			Assert.IsTrue( ring.Count == 2 );
			Assert.IsFalse( ring.IsFull );
			Assert.IsFalse( ring.IsEmpty );
			ring.Write( 3 );
			Assert.IsTrue( ring.IsFull );
			Assert.IsFalse( ring.IsEmpty );
			ring.Write( 4 );
			Assert.IsTrue( ring.Count == 3 );
			Assert.IsTrue( ring.IsFull );
			Assert.IsFalse( ring.IsEmpty );

		}

		[TestMethod()]
		public void RingBufferReadTest() {

			RingBuffer<string> ring = new RingBuffer<string>( 3 );
			ring.Write( "q" );
			Assert.IsTrue( ring.Count == 1 );
			string s = ring.Read();
			Assert.AreEqual( s, "q" );
			Assert.IsTrue( ring.IsEmpty );

			ring.Write( "aaa" );
			ring.Write( "bbb" );
			ring.Write( "ccc" );
			ring.Write( "ddd" );
			string s2 = ring.Read();
			Assert.AreEqual( s2, "bbb" );
			string s3 = ring.Read();
			Assert.AreEqual( s3, "ccc" );
			Assert.IsTrue( ring.Count == 1 );
		}

		[TestMethod()]
		public void GetEnumeratorTest() {
			RingBuffer<string> ring = new RingBuffer<string>( 3 );
			ring.Write( "aaa" );
			ring.Write( "bbb" );
			ring.Write( "ccc" );
			ring.Write( "ddd" );

			IEnumerator<string> en = ring.GetEnumerator();
			int conta = 0;
			while( en.MoveNext() ) {
				if( conta == 0 )
					Assert.AreEqual( en.Current, "bbb" );
				if( conta == 1 )
					Assert.AreEqual( en.Current, "ccc" );
				if( conta == 2 )
					Assert.AreEqual( en.Current, "ddd" );
				++conta;
			}

			ring.Clear();

			Assert.IsTrue( ring.Count == 0 );
			IEnumerator<string> en2 = ring.GetEnumerator();
			Assert.IsFalse( en.MoveNext() );
		}
	}

}