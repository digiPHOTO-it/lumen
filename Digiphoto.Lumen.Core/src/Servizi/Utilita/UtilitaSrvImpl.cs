using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Servizi;
using log4net;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;

namespace Digiphoto.Lumen.Core.Servizi.Utilita {

	public class UtilitaSrvImpl : ServizioImpl, IUtilitaSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( UtilitaSrvImpl ) );

		public bool inviaLog() {

			bool esito = false;

			string nomeFileZip = zippaLog();

			// Mando tutti gli zip che eventualmente non ho ancora mandato
			string folder = Path.GetDirectoryName( nomeFileZip );
			string[] listaZip = Directory.GetFiles( folder, "Log_Pdv_*.zip" );

			foreach( var unoZip in listaZip ) {

				var esito1 = inviaFileHttp( nomeFileZip );

				var esito2 = inviaFileSmtp( nomeFileZip );

				if( esito1 || esito2 ) {
					esito = true;

					// Rinomino il file in modo che al prossimo giro non parta più
					string dest = Path.Combine( folder, "SENT_" + Path.GetFileName( unoZip ) );
					File.Move( unoZip, dest );
				}
			}

			return esito;
		}

		private bool inviaFileSmtp( string nomeFileZip ) {

			bool esito = false;

			try {

				using( MailMessage mail = new MailMessage() ) {

					SmtpClient SmtpServer = new SmtpClient( "smtp.digiphoto.it" );

					String from = String.Format( "Pdv_{0}@digiphoto.it", Configurazione.infoFissa.idPuntoVendita );

					mail.From = new MailAddress( from );
					mail.To.Add( "assistenzalumen@digiphoto.it" );
					mail.Subject = "Invio Log " + Configurazione.infoFissa.descrizPuntoVendita + ". Host=" + System.Environment.MachineName;
					mail.Body = "In allegato";
					System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment( nomeFileZip );
					mail.Attachments.Add( attachment );

					SmtpServer.Port = 587;
					SmtpServer.Credentials = new System.Net.NetworkCredential( "assistenzalumen@digiphoto.it", "P0rcaPal3tta" );

					SmtpServer.EnableSsl = false;

					SmtpServer.Send( mail );
				}

				esito = true;

				_giornale.Info( "Spedizione log via smtp: " + esito );

			} catch( Exception ee ) {
				esito = false;
				_giornale.Error( "Spedizione log via smtp", ee );
			}

			return esito;
		}


		/// <summary>
		/// Se ci sono problemi, questo metodo deve saltare con una eccezione
		/// </summary>
		/// <returns></returns>
		private string zippaLog() {

			String nomeFileZip = String.Format( "Log_Pdv_{0}_{1:yyyyMMdd_HHmmss}.zip", Configurazione.infoFissa.idPuntoVendita, DateTime.Now );

			string[] configFiles = Directory.GetFiles( Configurazione.configPath, "*.config" );

			string logDir = Path.Combine( Configurazione.configPath, "Log" );
			String pathFileZip = Path.Combine( logDir, nomeFileZip );

			// I vecchi file di log ruotati si chiamavano per esempio: lumenUI-log.txt.2016-12-10
			string[] oldLogFiles1 = Directory.GetFiles( logDir, "*-log.txt.????-??-??" );
			// I nuovi file di log ruotati si chiamano per esempio: lumenUI-log.2017-06-13.txt
			string[] oldLogFiles2 = Directory.GetFiles( logDir, "*-log.????-??-??.txt" );

			string[] currentLogFiles = Directory.GetFiles( logDir, "*-log.txt" );

			using( var zip = ZipFile.Open( pathFileZip, ZipArchiveMode.Create ) ) {
				addToZip( zip, configFiles );
				addToZip( zip, oldLogFiles1 );
				addToZip( zip, oldLogFiles2 );
				addToZip( zip, currentLogFiles );
			}

			try {

				// Se tutto è andato bene, cancello i file di log ruotati (tanto sono dentro lo zip)
				foreach( var oldLog in oldLogFiles1 )
					File.Delete( oldLog );
				foreach( var oldLog in oldLogFiles2 )
					File.Delete( oldLog );

			} catch( Exception ee ) {
				_giornale.Error( "Impossibile eliminare file di log vecchi", ee );
			}

			return pathFileZip;
		}

		/// <summary>
		/// Aggiungo tutti i file in lista nello zip passato
		/// </summary>
		/// <param name="zip"></param>
		/// <param name="listaFiles"></param>
		private void addToZip( ZipArchive zip, string[] listaFiles ) {

			foreach( var file in listaFiles ) {
				using( var stream = new FileStream( file, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite ) ) {
					var zipArchiveEntry = zip.CreateEntry( Path.GetFileName( file ), CompressionLevel.Optimal );
					using( var destination1 = zipArchiveEntry.Open() )
						stream.CopyTo( destination1 );
				}
			}

		}

		public bool inviaFileHttp( String nomeFile ) {

			bool esito = false;

			try {

				FileInfo f = new FileInfo( nomeFile );

				using( WebClient client = new WebClient() ) {
					client.UseDefaultCredentials = false;
					client.Credentials = new NetworkCredential( "lumen-user", "P0rcaPal3tta44" );
					if( nomeFile.EndsWith( ".zip" ) )
						client.Headers.Add( HttpRequestHeader.ContentType, "application/zip" );

					var qq = client.UploadFile( "https://www.digiphoto.it/ricez/ricezione-file.php", nomeFile );

					if( System.Text.Encoding.UTF8.GetString( qq ) == "OK" )
						esito = true;
				}

				_giornale.Info( "Spedizione log via http: " + esito );

			} catch( Exception ee ) {
				esito = false;
				_giornale.Error( "Spedizione log via http", ee );
			}

			return esito;
		}

	}
}
