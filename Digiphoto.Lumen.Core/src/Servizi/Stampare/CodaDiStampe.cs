using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Threading;
using log4net;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Imaging;

namespace Digiphoto.Lumen.Servizi.Stampare {
	
	/**
	 * Implementa una coda di stampa gesita in multithread.
	 * Concettualmente rappresenta una stampante.
	 */
	internal class CodaDiStampe : ThreadedQueueBase<LavoroDiStampa> {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(CodaDiStampe) );

		public CodaDiStampe( string nomeStampante ) : base( nomeStampante )  {
		}

		/**
		 *  Pronto per stampare
		 */
		protected override void ProcessItem( LavoroDiStampa lavoroDiStampa ) {

			//check if the user called Stop
            if (StopRequested) {
                _giornale.Info( "Stampa stoppata dall'utente");
                _giornale.InfoFormat( "Thread di stampa terminato durante il lavoro '{0}'.", lavoroDiStampa.ToString() );
                return;
            }

			// Inizio a stampare
			IEsecutoreStampa stampatore = ImagingFactory.Instance.creaStampatore( lavoroDiStampa.param.nomeStampante );
			EsitoStampa esito = stampatore.esegui( lavoroDiStampa );
			
		}
	}
}
