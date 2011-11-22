using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Collections.Specialized;
using System.Drawing.Printing;
using Digiphoto.Lumen.Imaging;

namespace Digiphoto.Lumen.Servizi.Stampare {

	internal class SpoolStampeSrvImpl : ServizioImpl, ISpoolStampeSrv {

		private	Dictionary<string, CodaDiStampe> _code;

		public SpoolStampeSrvImpl() {
			// Istanzio la mappa con tutte le code.
			_code = new Dictionary<string,CodaDiStampe>();
		}

		/** Avvio tutte le stampe */
		public override void start() {
			foreach( string key in _code.Keys )
				_code[key].Start();
			base.start();
		}

		/** Fermo tutte le stampe */
		public override void stop() {
			foreach( string key in _code.Keys )
				_code[key].Stop();
			base.stop();
		}

		public void accodaStampa( Fotografia foto, ParamStampaFoto param ) {

			CodaDiStampe codaDiStampe = ricavaCodaDiStampa( param.nomeStampante );

			// Creo un nuovo lavoro di stampa e lo aggiungo alla coda.
			LavoroDiStampa lavoro = new LavoroDiStampa( foto, param );
			codaDiStampe.EnqueueItem( lavoro );
		}

		private CodaDiStampe ricavaCodaDiStampa( string nomeStampante ) {

			// Se non esiste già la stampante nella collezione, allora la istanzio
			CodaDiStampe coda;
			if( _code.ContainsKey(nomeStampante) )
				coda = _code[nomeStampante];
			else {
				coda = new CodaDiStampe( nomeStampante );
				coda.Start();
				_code.Add( nomeStampante, coda );
			}

			return coda;
		}

	}
}
