﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Threading;
using log4net;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Stampare {
	
	/**
	 * Implementa una coda di stampa gesita in multithread.
	 * Concettualmente rappresenta una stampante.
	 */
	public class CodaDiStampe : ThreadedQueueBase<LavoroDiStampa> {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(CodaDiStampe) );

		public delegate void StampaCompletataCallback( object sender, StampatoMsg eventArgs );

		private StampaCompletataCallback stampaCompletataCallback;
		IEsecutoreStampa _stampatore;


		public CodaDiStampe( ParamStampa param, string nomeStampante ) : this( param, nomeStampante, null ) {
		}

		public CodaDiStampe( ParamStampa param, string nomeStampante, StampaCompletataCallback callback ) : base( nomeStampante ) {

			this.workerThread.SetApartmentState( System.Threading.ApartmentState.STA );

			if( String.IsNullOrEmpty( nomeStampante ) )
				throw new ArgumentException( "Nome stampante vuota" );

			_stampatore = ImagingFactory.Instance.creaStampatore(param, nomeStampante );
			this.stampaCompletataCallback = callback;
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


			// Per evitare problemi di multi-thread, le immagini le idrato nello stesso thread con cui le manderò in stampa.
			// Non anticipare questo metodo altrimenti poi non va.
			// Se le immagini non sono idratate, le carico!
			ParamStampa param = null;
			if(lavoroDiStampa is LavoroDiStampaFoto){

				LavoroDiStampaFoto lavoroDiStampaFoto = lavoroDiStampa as LavoroDiStampaFoto;

				//Rinstanzio il stampatore che potrebbe essere di un tipo differente dal mio
				LavoroDiStampaFoto lsp = (LavoroDiStampaFoto)lavoroDiStampa;
				param = lsp.param;

			}else if(lavoroDiStampa is LavoroDiStampaProvini){
				LavoroDiStampaProvini lavoroDiStampaProvini = lavoroDiStampa as LavoroDiStampaProvini;

/* li carico dopo in modo diverso
				foreach (Fotografia fot in lavoroDiStampaProvini.fotografie)
				{
					AiutanteFoto.idrataImmagineDaStampare( fot );
				}
*/

				//Rinstanzio il stampatore che potrebbe essere di un tipo differente dal mio
				LavoroDiStampaProvini lsp = (LavoroDiStampaProvini)lavoroDiStampa;
				param = lsp.param;
			} else if( lavoroDiStampa is LavoroDiStampaTessera ) {
				LavoroDiStampaTessera lavoroDiStampaTessera = lavoroDiStampa as LavoroDiStampaTessera;
				param = lavoroDiStampaTessera.param;
			}

			// Se sono cambiati i parametri istanzio nuovamente lo stampatore.

			if( _stampatore == null ||  !param.GetType().IsAssignableFrom( _stampatore.tipoParamGestito ) )
				_stampatore = ImagingFactory.Instance.creaStampatore( param, param.nomeStampante );

			
			EsitoStampa esito = _stampatore.esegui( lavoroDiStampa );

			lavoroDiStampa.esitostampa = esito;
			lavoroDiStampa.stato = LavoroDiStampa.Stato.Completato;

			StampatoMsg eventArgs = new StampatoMsg( lavoroDiStampa );
			eventArgs.descrizione = lavoroDiStampa.ToString();
			eventArgs.esito = lavoroDiStampa.esitostampa == EsitoStampa.Ok ? Esito.Ok : Esito.Errore;

			if( stampaCompletataCallback != null )
				stampaCompletataCallback.Invoke( this, eventArgs );
		}

		

	}
}
