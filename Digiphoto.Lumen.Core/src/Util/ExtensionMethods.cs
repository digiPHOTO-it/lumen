using Digiphoto.Lumen.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Util {

	public static class ExtensionMethods {

		public static T[] ConvertToArray<T>( this IEnumerable<T> enumerable ) {
			if( enumerable == null )
				throw new ArgumentNullException( "enumerable" );

			return enumerable as T[] ?? enumerable.ToArray();
		}

		/// <summary>
		/// Metodo di estensione statica.
		/// Si può usare su di un qualunque oggetto per farne una copia.
		/// In pratica si serializza in memorie e si ri-istanzia un altro oggetto 
		/// che diventa un clone del primo
		/// 
		/// In pratica realizza una Deep Copy che è il contrario della ShallowCopy
		/// </summary>
		/// <typeparam name="T">La classe dell'oggtto da copiare</typeparam>
		/// <param name="source">L'oggetto da copiare</param>
		/// <returns></returns>
		public static T deepCopy<T>( this T source ) {
			var isNotSerializable = !typeof( T ).IsSerializable;
			if( isNotSerializable )
				throw new ArgumentException( "The type must be serializable.", "source" );

			var sourceIsNull = ReferenceEquals( source, null );
			if( sourceIsNull )
				return default( T );

			var formatter = new BinaryFormatter();
			using( var stream = new MemoryStream() ) {
				formatter.Serialize( stream, source );
				stream.Seek( 0, SeekOrigin.Begin );
				return (T)formatter.Deserialize( stream );
			}
		}

		public static IEnumerable<T> ToIEnumerable<T>( this IEnumerator<T> enumerator ) {
			while( enumerator.MoveNext() ) {
				yield return enumerator.Current;
			}
		}



		public static void SaveMailMessage( this MailMessage msg, string filePath ) {
			using( var fs = new FileStream( filePath, FileMode.Create ) ) {
				msg.ToEMLStream( fs );
			}
		}

		/// <summary>
		/// Converts a MailMessage to an EML file stream.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public static void ToEMLStream( this MailMessage msg, Stream str ) {
			using( var client = new SmtpClient() ) {
				var id = Guid.NewGuid();

				var tempFolder = Path.Combine( Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name );

				tempFolder = Path.Combine( tempFolder, "MailMessageToEMLTemp" );

				// create a temp folder to hold just this .eml file so that we can find it easily.
				tempFolder = Path.Combine( tempFolder, id.ToString() );

				if( !Directory.Exists( tempFolder ) ) {
					Directory.CreateDirectory( tempFolder );
				}

				client.UseDefaultCredentials = true;
				client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
				client.PickupDirectoryLocation = tempFolder;
				client.Send( msg );

				// tempFolder should contain 1 eml file

				var filePath = Directory.GetFiles( tempFolder ).Single();

				// stream out the contents
				using( var fs = new FileStream( filePath, FileMode.Open ) ) {
					fs.CopyTo( str );
				}
			}
		}

		/// <summary>
		/// Data una riga di un carrelllo, ritorno il discriminatore invertito
		/// es.
		/// se Masterizzata -> torno Stampata
		/// se Stampata     -> torno Masterizzata
		/// </summary>
		/// <param name="riga"></param>
		/// <returns></returns>
		public static string InverteDiscriminator( this Model.RigaCarrello riga ) {

			if( riga.discriminator == RigaCarrello.TIPORIGA_MASTERIZZATA )
				return RigaCarrello.TIPORIGA_STAMPA;
			if( riga.discriminator == RigaCarrello.TIPORIGA_STAMPA )
				return RigaCarrello.TIPORIGA_MASTERIZZATA;

			throw new Exception( "tipo riga non ricosciuta: " + riga.discriminator );
		}

	}
}
