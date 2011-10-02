using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using log4net;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Servizi.VolumeCambiato {

	/**
	 * Notifico un evento quando viene inserito (o rimosso) un disco
	 */
	class QQQVolumeCambiatoNotifier : IDisposable {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( LumenApplication ) );

		private ManagementEventWatcher _watcher;


		public QQQVolumeCambiatoNotifier() {

			_watcher = new ManagementEventWatcher();

			// WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
			WqlEventQuery query = new WqlEventQuery( "SELECT * FROM Win32_VolumeChangeEvent" );
			_watcher.Query = query;
		}

		public void inizioAscolto() {
			_watcher.Start();
		}

		public void fermaAscolto() {
			_watcher.Stop();
		}

		private void onVolumeCambiatoEvent( object sender, EventArrivedEventArgs e ) {

			ManagementBaseObject mo = e.NewEvent;

			String driveName = (String)mo.Properties ["DriveName"].Value;
			UInt16 eventType = (UInt16)mo.Properties ["EventType"].Value;

			_giornale.Info( "Volume cambiato! driveName = (" + driveName + ")  tipo = " + eventType );
			_giornale.Debug( e.NewEvent.GetText( TextFormat.Mof ) );

			// Creo un messaggio da mettere sul bus.
			VolumeCambiatoMessaggio cambioMsg = new VolumeCambiatoMessaggio();
			cambioMsg.sender = sender;
			cambioMsg.eventArgs = e;
			UInt64 timeCreated = (UInt64)mo.Properties ["TIME_CREATED"].Value;
			cambioMsg.timeStamp = new DateTime( (long)timeCreated );

			cambioMsg.nomeVolume = driveName;
			cambioMsg.montato = (eventType == 2);

			LumenApplication._instance.bus.Publish( cambioMsg );
		}

		public void Dispose() {
			fermaAscolto();
		}
	}
}
