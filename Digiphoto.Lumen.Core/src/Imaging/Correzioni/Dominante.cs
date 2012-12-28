
namespace Digiphoto.Lumen.Imaging.Correzioni {

	public class Dominante : Correzione {

		public double rosso;
		public double verde;
		public double blu;

		public override bool isSommabile( Correzione altra ) {
			return (altra is Dominante);
		}

		public override Correzione somma( Correzione altra ) {

			Dominante dAltra = altra as Dominante;

			Dominante ris = new Dominante {
				rosso = this.rosso + dAltra.rosso,
				verde = this.verde + dAltra.verde,
				blu = this.blu + dAltra.blu
			};

			return ris;
		}
	}
}
