using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.Imaging.Wic {
	
	internal static class WicUtil {

		public static byte [] ToByteArray( BitmapFrame bfResize ) {

			MemoryStream msStream = new MemoryStream();
			PngBitmapEncoder pbdDecoder = new PngBitmapEncoder();
			pbdDecoder.Frames.Add( bfResize );
			pbdDecoder.Save( msStream );
			return msStream.ToArray();
		}

		public static BitmapFrame ReadBitmapFrame( Stream streamPhoto ) {
			BitmapDecoder bdDecoder = BitmapDecoder.Create( streamPhoto, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None );
			return bdDecoder.Frames [0];
		}
	}
}
