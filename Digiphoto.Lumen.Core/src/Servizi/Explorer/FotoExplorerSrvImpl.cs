using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.ComponentModel;
using Digiphoto.Lumen.Comandi;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Eventi;
using System.Threading;
using Digiphoto.Lumen.Util;
using log4net;
using Digiphoto.Lumen.Database;

namespace Digiphoto.Lumen.Servizi.Explorer {

	public class FotoExplorerSrvImpl : ServizioImpl, IFotoExplorerSrv {

		#region Proprietà

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( FotoExplorerSrvImpl ) );


		/** Uso una lista bindabile, in questo modo la UI dovrebbe essere notificata delle modifiche che avvengono */
		public BindingList<Fotografia> fotografie {
			get;
			private set;
		}

		public Fotografia fotoCorrente { get; set; }

		/** Alcuni attributi della foto, sono transienti e devo gestirli io a mano */
		private Thread _threadIdrata;

		#endregion

		public FotoExplorerSrvImpl() : base() {
			fotografie = new BindingList<Fotografia>();
		}

		~FotoExplorerSrvImpl() {

			// avviso se il thread di copia è ancora attivo
			if( _threadIdrata != null && _threadIdrata.IsAlive ) {
				_giornale.Warn( "Il thread di caricamento foto è ancora attivo. Non è stata fatta la Dispose del servizio.\nProababilmente il programma si inchioderà" );
			}
		}

		public void invoca( Comando comando, Target target ) {

			using( new UnitOfWorkScope( true ) ) {

				switch( target ) {

					// --
					case Target.Corrente:
						if( fotoCorrente == null )
							throw new ArgumentException( "Nessuna foto corrente selezionata" );

						comando.esegui( fotoCorrente );
						break;

					// --
					case Target.Selezionate:
						var querySelezionate = from ff in fotografie
											   where ff.selezionata == true
											   select ff;
						foreach( Fotografia foto in querySelezionate )
							comando.esegui( foto );
						break;

					// --
					case Target.Tutte:
						foreach( Fotografia foto in fotografie )
							comando.esegui( foto );
						break;
				}
			}
		}


		/** Eseguo il caricamento delle foto richieste */
		public void cercaFoto( ParamRicercaFoto param ) {

			// Per prima cosa azzero la gallery corrente
			fotografie = null;

			using( IRicercatoreSrv ricercaSrv = LumenApplication.Instance.creaServizio<IRicercatoreSrv>() ) {
				fotografie = new BindingList<Fotografia>( ricercaSrv.cerca( param ) );
			}


			if( fotografie != null ) {
				// idrato le immagini in un thread separato
				_threadIdrata = new Thread( idrataImmaginiFoto );
				_threadIdrata.Start();
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
			LumenApplication.Instance.bus.Publish( new RicercaModificataMessaggio() );
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
