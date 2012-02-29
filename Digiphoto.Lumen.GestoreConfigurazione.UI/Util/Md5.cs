using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Digiphoto.Lumen.GestoreConfigurazione.UI.Util
{
    public class Md5
    {
        /// <summary>
        /// Compute MD5 hash of string 
        /// and return a string encoded base64
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string MD5GenerateHash(string inputString)
        {
          System.Security.Cryptography.MD5 hash = System.Security.Cryptography.MD5.Create();
          Byte[] inputBytes = ASCIIEncoding.Default.GetBytes(inputString);
          Byte[] outputBytes = hash.ComputeHash(inputBytes);
          return Convert.ToBase64String(outputBytes);
        }
    }
}
