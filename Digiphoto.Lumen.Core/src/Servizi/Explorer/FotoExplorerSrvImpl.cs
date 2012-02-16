using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.ComponentModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Eventi;
using System.Threading;
using Digiphoto.Lumen.Util;
using log4net;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Servizi.Selezionare;

namespace Digiphoto.Lumen.Servizi.Explorer {

	public class FotoExplorerSrvImpl : SelettoreMultiFotoImpl, IFotoExplorerSrv {

		#region Proprietà

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( FotoExplorerSrvImpl ) );


		public List<Fotografia> fotografie {
			get;
			private set;
		}

		public override IEnumerable<Fotografia> tutteLeFoto {
			get {
				return fotografie;
			}
		}

		public Fotografia fotoCorrente { get; set; }

		/** Alcuni attributi della foto, sono transienti e devo gestirli io a mano */
		private Thread _threadIdrata;

		#endregion

		public FotoExplorerSrvImpl() : base() {
			fotografie = new List<Fotografia>();
		}

		~FotoExplorerSrvImpl() {

			// avviso se il thread di copia è ancora attivo
			if( _threadIdrata != null && _threadIdrata.IsAlive ) {
				_giornale.Warn( "Il thread di caricamento foto è ancora attivo. Non è stata fatta la Dispose del servizio.\nProababilmente il programma si inchioderà" );
			}
		}

		


		/** Eseguo il caricamento delle foto richieste */
		public void cercaFoto( ParamCercaFoto param ) {

			// Per prima cosa azzero la gallery corrente
			fotografie = null;

			using( IRicercatoreSrv ricercaSrv = LumenApplication.Instance.creaServizio<IRicercatoreSrv>() ) {
				fotografie = ricercaSrv.cerca( param );
			}


			if( fotografie != null ) {

// TODO capire come mai
// se idrato le immagini in un thread separato, la UI mi da problemi.
// mi dice che i dati sono stati caricati in un thread diverso da quello corrente
// non ho però capito come risolvere.
				idrataImmaginiFoto();
/*
				// idrato le immagini in un thread separato
				_threadIdrata = new Thread( idrataImmaginiFoto );
				_threadIdrata.Start();
 */

			}


		}

		/** Idrato in modo asincrono gli attributi delle immagini che ho caricato */
		private void idrataImmaginiFoto() {

			foreach( Fotografia fotografia in fotografie ) {

				AiutanteFoto.idrataImmaginiFoto( fotografia );
				// TODO forse occorre lanciare un evento di foto caricata ??? 
				//      essendo la collezione bindabile, forse non ce ne sarà bisogno..... 
				//      vedremo. Per ora risparmio fatica.
			}

			// Lancio un messaggio che dice che è stata portata a termine una nuova ricerca
			LumenApplication.Instance.bus.Publish( new RicercaModificataMessaggio( this ) );
		}

		public override void Dispose() {

			try {

				// Se il tread di copia è ancora vivo, lo uccido
				if( _threadIdrata != null ) {
					if( _threadIdrata.IsAlive )
						_threadIdrata.Abort();
					else
						_threadIdrata.Join();
				}
			} finally {
			}

			try {
				foreach( Fotografia foto in this.fotografie ) {
					AiutanteFoto.disposeImmagini( foto );
				}
			} finally {
			}

			base.Dispose();
		}

	}
}
