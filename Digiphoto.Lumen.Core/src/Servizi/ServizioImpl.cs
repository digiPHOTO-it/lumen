using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi {

	public abstract class ServizioImpl : IServizio {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(ServizioImpl) );
		private volatile bool _running = false;

		public ServizioImpl() {
		}

		public bool isRunning {
			get {
				return _running;
			}
		}

		public virtual void start() {

			bool notificaCambio = (_running == false);

			_giornale.Debug( "Sta per partire il servizio: " + this.GetType().Name );
			_running = true;

			_giornale.Info( "E' stato avviato il servizio: " + this.GetType().Name );

			if( notificaCambio ) {
				CambioStatoMessaggio msg = new CambioStatoMessaggio();
				msg.sender = this;
				msg.descrizione = this.GetType().Name + " partito";
				msg.nuovoStato = '1'; // Acceso 
				LumenApplication.Instance.bus.Publish( msg );
			}
		}

		public virtual void stop() {

			bool notificaCambio = (_running == true);

			_giornale.Debug( "Sto per fermare il servizio: " + this.GetType().Name );
			_running = false;
			_giornale.Info( "E' stato fermato il servizio: " + this.GetType().Name );

			if( notificaCambio ) {
				CambioStatoMessaggio msg = new CambioStatoMessaggio();
				msg.sender = this;
				msg.descrizione = this.GetType().Name + " fermato";
				msg.nuovoStato = '0'; // Acceso 
				LumenApplication.Instance.bus.Publish( msg );
			}
		}

		public virtual void Dispose() {
			if( isRunning )
				stop();
		}

#region Metodi-IObserver
		
		public virtual void OnCompleted() {
			// throw new NotImplementedException();
			_giornale.Debug( "observer.OnCompleted" );
		}

		public virtual void OnError( Exception error ) {
			// throw new NotImplementedException();
			_giornale.Debug( "observer.OnError" );
		}

		public virtual void OnNext( Messaggio messaggio ) {
			_giornale.Debug( this.ToString() +  " observer.OnNext = " + messaggio.descrizione );
		}

#endregion

		public Configurazione configurazione {
			get {
				return LumenApplication.Instance.configurazione;
			}
		}

		public Stato stato {
			get {
				return LumenApplication.Instance.stato;
			}
		}

		protected void pubblicaMessaggio( Messaggio messaggio ) {
			LumenApplication.Instance.bus.Publish( messaggio );
		}


		private LumenEntities _objectContext;
		/// <summary>
		/// Returns the ObjectContext instance that belongs to the 
		/// current UnitOfWorkScope. If currently no UnitOfWorkScope
		/// exists, a local instance of the Lumen-Object-Context class
		/// is returned.
		/// </summary>
		protected LumenEntities objectContext {
			get {
				if( UnitOfWorkScope.CurrentObjectContext != null )
					return UnitOfWorkScope.CurrentObjectContext;
				else {
					if( _objectContext == null )
						_objectContext = new LumenEntities();
					return _objectContext;
				}
			}
		}

	}

}
