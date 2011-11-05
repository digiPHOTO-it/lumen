using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Comandi {

	public class CorrezioneComando : Comando {

		IList<Correzione> _correzioni;

		public CorrezioneComando() : this( Target.Nessuna, null ) {
		}

		public CorrezioneComando( Target target, Correzione correzione ) : base( target ) {
			
			_correzioni = new List<Correzione>();

			if( correzione != null )
				_correzioni.Add( correzione );
		}


	
		internal override Esito esegui( Fotografia foto) {

			IGestoreImmagineSrv gis = LumenApplication.Instance.getGestoreImmaginiSrv();

			Immagine modificata = gis.applicaCorrezioni( foto.imgProvino, _correzioni );

			//
 			// ATTENZIONE :   questo passaggio è importante.
			// In questa fase, sto per perdere il reference all'Image precedente.
			// Se non faccio esplicitamente la dispose, quel file sul filesystem rimane lockato.
			// Infatti il metodo Image.FromFile() quando apre l'immagine, impone un lock sul file.
			// Questo lock non verrebbe più rilasciato
			//
			
			foto.imgProvino.Dispose();
			foto.imgProvino = modificata;


			// Aggiungo la correzione all'elenco
			foreach( Correzione c in _correzioni )
				foto.correzioni.Add( c );

			// TODO attenzione 
			//      ci vuole un refactoring 
			//      se ho 10 correzioni, non posso salvare il file 10 volte,
			//      bisogna che carico la bitmap,
			//      poi la modifico in tutti i modi
			//      poi la salvo alla  fine.
			//      Per il momento la tengo cosi che almeno funziona per fare qualche test.

			// Salvo l'immagine cosi modificata
			gis.save( foto.imgProvino, PathUtil.nomeCompletoProvino( foto ) );

			return Esito.Ok;
		}
	}
}
