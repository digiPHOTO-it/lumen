using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Config {

	[Serializable]
	public class FlashCardConfig {	

		private int version { get; set; }
		public string idFotografo { get; set; }
		public Guid idEvento { get; set; }

		public string didascalia { get; set; }

		public static readonly string NOMEFILECONFIG = typeof( FlashCardConfig ).FullName + ".xml";

		public FlashCardConfig() {
		}

		public FlashCardConfig( Fotografo fotografo ) : this ( fotografo, null ) {
		}

		public FlashCardConfig( Fotografo fotografo, Evento evento ) {
			version = 1;
			this.idFotografo = fotografo.id;
		}

		public static void serialize( string file, FlashCardConfig c ) {
			System.Xml.Serialization.XmlSerializer xs
				= new System.Xml.Serialization.XmlSerializer( c.GetType() );
			StreamWriter writer = File.CreateText( file );
			xs.Serialize( writer, c );
			writer.Flush();
			writer.Close();
		}

		public static FlashCardConfig Deserialize( string file ) {
			System.Xml.Serialization.XmlSerializer xs
				= new System.Xml.Serialization.XmlSerializer(
					typeof( FlashCardConfig ) );
			StreamReader reader = File.OpenText( file );
			FlashCardConfig c = (FlashCardConfig)xs.Deserialize( reader );
			reader.Close();
			return c;
		}

	}


}
