using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Digiphoto.Lumen.Util;
using static System.Environment;

namespace Digiphoto.Lumen.Core.Util {

	public static class MailUtil {


		public static String getNomeFileLog() {
			// TODO soluzione tampone. Occorre un ragionamento piu sofisticato perché i log sono configurabili
			return Path.Combine( Environment.GetFolderPath( SpecialFolder.LocalApplicationData ), "digiPHOTO.it", "Lumen", "Log", "lumenUI-log.txt" );
		}

		/// <summary>
		/// Questo url riesce a far aprire il programma di mail di default, ma non va l'allegato.
		/// E' una questione di sicurezza.
		/// TODO: trovare un altro sistema per allegare il file.
		/// </summary>
		public static void spedireLog() {

			String nomeFileLog = getNomeFileLog();
			String url = "mailto:webmaster@digiphoto.it?subject=Invio Log Lumen&body=Devi allegare il file:%0D%0A " + nomeFileLog + "&attachment=" + nomeFileLog;
			System.Diagnostics.Process.Start( url );
		}

		/// <summary>
		/// Questo sistema crea un messaggio email, poi lo salva su disco come estensione .EML
		/// Poi chiama l'apertura del file .EML in modo che il sistema operativo apre il file con il programma di posta predefinito.
		/// In questo caso l'allegato funziona e viene caricato, ma il programma non si mette in modalità di invio mail, ma di sola visualizzazione del messsagio.
		/// </summary>
		private static void spedireLogNonVa() {

			String nomeFileLog = getNomeFileLog();

			MailMessage mailMessage = new MailMessage( "webmaster@digiphoto.it", "webmaster@digiphoto.it" );
			mailMessage.Subject = "Log Lumen";
			mailMessage.IsBodyHtml = true;
			mailMessage.Body = "<span style='font-size: 12pt; color: red;'>Allegare il fil di log: " + nomeFileLog + "</span>";

			String temp1 = System.IO.Path.GetTempFileName() + Guid.NewGuid().ToString() + ".log.txt";
			File.Copy( nomeFileLog, temp1 );

			mailMessage.Attachments.Add( new Attachment( temp1 ) );

			string emailFileName = Path.ChangeExtension( Path.GetTempFileName(), ".eml" );

			mailMessage.SaveMailMessage( emailFileName );

			//Open the file with the default associated application registered on the local machine
			System.Diagnostics.Process.Start( emailFileName );
		}


	}
}
