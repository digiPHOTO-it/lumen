using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Servizi.Ricerca {

	/// <summary>
	/// Con questo messaggio, la gallery svuota tutti i filtri tranne quelli di chi ha lanciato il messaggio.
	/// </summary>
	public class SvuotaFiltriMsg : Messaggio {

		public SvuotaFiltriMsg( object sender )	: base( sender ) {
		}

	}
}
