using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using log4net;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Applicazione;
using System.Threading;
using System.IO;

namespace Digiphoto.Lumen.Servizi.VolumeCambiato {

	public class VolumeCambiatoSrvImpl : ServizioImpl, IVolumeCambiatoSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( VolumeCambiatoSrvImpl ) );

		private ManagementEventWatcher _watcher;
		private bool? _attivazioneAttesa;

		/** 
		 * Questo oggetto serve per evitare il problema descritto qui:
		 * http://stackoverflow.com/questions/1567017/com-object-that-has-been-separated-from-its-underlying-rcw-cannot-be-used
		 */
		private EventArrivedEventHandler eventArrivedEventHandler;

		/** Mi tengo a mente l'ultimo drive che ho montato */
		private string _ultimoDriveMontato;

		public string ultimoDriveMontato {
			get { return _ultimoDriveMontato; }
			set {	_ultimoDriveMontato = value; }
		}
			
		public bool attesaBloccante {
			get;
			set;
		}

		public VolumeCambiatoSrvImpl() {
			_watcher = new ManagementEventWatcher();
			
			// WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
			WqlEventQuery query = new WqlEventQuery( "SELECT * FROM Win32_VolumeChangeEvent" );

			_watcher.Query = query;

			_attivazioneAttesa = null;
			attesaBloccante = true;  // decido io
		}

		public override void start() {
			base.start();
			_watcher.Start();
		}

		public override void stop() {
			if( isRunning ) {
				base.stop();
				try {
					// TODO non so perché ma mi da questo errore:
					// Impossibile utilizzare oggetti COM separati dai relativi RCW sottostanti.
					_watcher.Stop();
				} catch( Exception ee ) {
					_giornale.Warn( "Non riesco a stoppare questo servizio. Porca paletta", ee );
				}
			}
		}

		/** Occhio questo loop va chiamato probabilmente in un thread separato */
		public void attesaEventi() {

			// Solo la prima volta
			if( _attivazioneAttesa == null ) {
				// Attivo ascoltatore
				// Devo tenermi un riferimento di questa callback altrimenti durante interop si spacca
				eventArrivedEventHandler = new EventArrivedEventHandler( onVolumeCambiatoEvent );
				_watcher.EventArrived += eventArrivedEventHandler;
				// poi non lo ribadisco più
				_attivazioneAttesa = attesaBloccante;
			}

			if( attesaBloccante ) {
				_giornale.Debug( "Sto per mettermi in attesa del prossimo evento" );
				ManagementBaseObject oo = _watcher.WaitForNextEvent();
				_giornale.Debug( "Esco dalla attesa di eventi" );
			}

		}



		private void onVolumeCambiatoEvent( object sender, EventArrivedEventArgs e ) {

			// Faccio un controllo di sicurezza
			if( !isRunning ) {
				_giornale.Warn( "Come mai ho sentito un evento, ma sono in statoScarica di fermo?\nIn ogni caso vado avanti lo stesso" );
			}

			ManagementBaseObject mo = e.NewEvent;

			String driveName = (String)mo.Properties ["DriveName"].Value;
			UInt16 eventType = (UInt16)mo.Properties ["EventType"].Value;

			_giornale.Info( "Volume cambiato! driveName = (" + driveName + ")  tipo = " + eventType );

			// Creo un messaggio da mettere sul bus.
			VolumeCambiatoMsg volumeCambiatoMsg = new VolumeCambiatoMsg( this );

			UInt64 timeCreated = (UInt64)mo.Properties ["TIME_CREATED"].Value;
			volumeCambiatoMsg.timeStamp = new DateTime( (long)timeCreated );

			volumeCambiatoMsg.nomeVolume = driveName;

			// 2=montato ; 3=smontato
			volumeCambiatoMsg.montato = (eventType == 2);

			volumeCambiatoMsg.descrizione = (volumeCambiatoMsg.montato) ? "Attivato" : "Smontato";
			volumeCambiatoMsg.descrizione += " il volume " + driveName;


			// Memorizzo l'ultimo drive
			ultimoDriveMontato = volumeCambiatoMsg.montato ? driveName : null;

			// Metto il messaggio sul BUS degli eventi
			LumenApplication._instance.bus.Publish( volumeCambiatoMsg );

		}  

		protected override void Dispose( bool disposing ) {
			base.Dispose( disposing );  // se mi chiama lo stop mi da dei problemi. Evito e faccio solo la dispose
			_watcher.Dispose();
		}



		public System.IO.DriveInfo [] GetDrivesUsbAttivi() {

			var removableDrives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable);

			return removableDrives.ToArray<DriveInfo>();
		}
	}
}
