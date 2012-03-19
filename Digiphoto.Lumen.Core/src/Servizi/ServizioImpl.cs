using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi {

	public abstract class ServizioImpl : IServizio {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(ServizioImpl) );

		public ServizioImpl() {
			Console.Write( "STOP" );
		}

		public bool isRunning {
			get {
				return (this.statoRun == Servizi.StatoRun.Running);
			}
		}

		public virtual void start() {

			statoRun = Servizi.StatoRun.Running;

			_giornale.Info( "E' stato avviato il servizio: " + this.GetType().Name );

		}

		public virtual void stop() {

			statoRun = Servizi.StatoRun.Stopped;

			_giornale.Info( "E' stato fermato il servizio: " + this.GetType().Name );
		}

		public virtual void Dispose() {

			// Qui non devo fare lo stop,  altrimenti mi si accavallano i casini
			if( isRunning )
				stop();  // però ci vuole... ora provo
			
			// Se per caso avevo aperto un object context localmente, allora lo rilascio
			if( _objectContext != null ) {
				_objectContext.Dispose();
				_objectContext = null;
			}

		}

		#region Messaggi
		
		public virtual void OnCompleted() {
			_giornale.Debug( "observer.OnCompleted" );
		}

		public virtual void OnError( Exception error ) {
			_giornale.Debug( "observer.OnError" );
		}

		public virtual void OnNext( Messaggio messaggio ) {
		}

		#endregion

		protected Configurazione configurazione {
			get {
				return LumenApplication.Instance.configurazione;
			}
		}


		private StatoRun _statoRun = StatoRun.Stopped;
		public StatoRun statoRun {
			get {
				return _statoRun;
			}
			set {
				if( value != _statoRun ) {

					_statoRun = value;

					// Notifico tutti che questo servizio ha cambiato statoScarica
					CambioStatoMsg msg = new CambioStatoMsg( this );
					msg.nuovoStato = (int) _statoRun;
					msg.descrizione = this.GetType().Name + " partito";
					LumenApplication.Instance.bus.Publish( msg );
				}
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
