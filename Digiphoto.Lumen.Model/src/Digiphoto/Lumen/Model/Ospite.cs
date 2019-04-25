using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {


	[MetadataType( typeof( Ospite ) )]
	[Serializable]
	[Table( "Ospiti" )]
	public partial class Ospite {

		public Ospite() {
		}

		#region Attributi

		[Key]
		[DatabaseGenerated( DatabaseGeneratedOption.None )]
		public int id { get; set; }

		[Required]
		public byte [] impronta { get; set; }

		public string nome { get; set; }

		#endregion Attributi

		#region Uguaglianza

		public override int GetHashCode() {
			return id.GetHashCode();
		}

		public override bool Equals( object obj ) {

			bool sonoUguali = false;

			if( obj is Ospite ) {

				Ospite altro = (Ospite)obj;
				sonoUguali = this.id == altro.id;
			}

			return sonoUguali;
		}

		#endregion Uguaglianza


	}
}