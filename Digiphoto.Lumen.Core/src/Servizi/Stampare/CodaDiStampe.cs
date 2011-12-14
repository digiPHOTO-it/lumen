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
	public class CodaDiStampe : ThreadedQueueBase<LavoroDiStampa> {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(CodaDiStampe) );

		public delegate void StampaCompletataEventHandler( object sender, StampatoMsg eventArgs );

		private StampaCompletataEventHandler stampaCompletataEventHandler;

		public CodaDiStampe( string nomeStampante, StampaCompletataEventHandler callback ) : base( nomeStampante ) {
			
			if( String.IsNullOrEmpty( nomeStampante ) )
				throw new ArgumentException( "Nome stampante vuota" );

			this.stampaCompletataEventHandler = callback;
		}

		public override int GetHashCode() {
			int hash = 7;
			hash = 31 * hash + (null == this.Name ? 0 : this.Name.GetHashCode());
			return hash;			
		}

		public override bool Equals( object obj ) {
			bool isEqual = false;
			if (obj is CodaDiStampe) {
				CodaDiStampe thatCar = (CodaDiStampe) obj;
				isEqual = this.Name == thatCar.Name || (this.Name != null && this.Name.Equals(thatCar.Name));
			}
			return isEqual;
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
			lavoroDiStampa.stato = LavoroDiStampa.Stato.InEsecuzione;

			IEsecutoreStampa stampatore = ImagingFactory.Instance.creaStampatore( lavoroDiStampa.param.nomeStampante );
			EsitoStampa esito = stampatore.esegui( lavoroDiStampa );

			lavoroDiStampa.esitostampa = esito;
			lavoroDiStampa.stato = LavoroDiStampa.Stato.Completato;

			StampatoMsg eventArgs = new StampatoMsg( lavoroDiStampa );
			eventArgs.descrizione = "+StampaCompletata";

			stampaCompletataEventHandler.Invoke( this, eventArgs ); 
		}



	}
}
