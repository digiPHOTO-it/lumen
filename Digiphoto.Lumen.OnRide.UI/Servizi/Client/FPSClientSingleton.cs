using System;
using System.Collections.Generic;
using System.Collections;
using System.ServiceModel;
using System.Windows;
using Digiphoto.Lumen.OnRide.UI.FingerServiceReference;
using Digiphoto.Lumen.Core.Util;
using log4net;

namespace Digiphoto.Lumen.OnRide.UI.Servizi {

	public class FPSClientSingleton
    {
		protected new static readonly ILog _giornale = LogManager.GetLogger( typeof( FPSClientSingleton ) );


		private static FPSClientSingleton instance;

		private FingerprintServiceClient fpClient;

		public static FPSClientSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FPSClientSingleton();
                }
                return instance;
            }
        }

        private FPSClientSingleton()
        {
			fpClient = new FingerprintServiceClient();
		}

		internal void Open()
		{
			bool autoCloseNotification = false;
			bool shutdownApp = false;

			while (!shutdownApp && isConnectionOK)
			{
				if (!fpClient.State.Equals(CommunicationState.Opening))
				{
					try
					{
						autoCloseNotification = false;
						fpClient.Open();
					}
					catch (Exception)
					{
						autoCloseNotification = true;
						fpClient.Abort();
						System.Threading.Thread.Sleep( 2000 );
						fpClient = new FingerprintServiceClient();
                    }
				}

				if (autoCloseNotification)
				{
					MessageBoxResult dialogResult;
					// dialogResult = AutoClosingMessageBox.Show(System.Windows.Application.Current, "Il server sembra non rispondere voi rimanere in attesa?", "Avviso", 10000, MessageBoxButton.YesNoCancel);
					dialogResult = MessageBoxResult.Yes;
					if (dialogResult == MessageBoxResult.No)
					{
						MessageBoxResult chiudiResult = MessageBox.Show("Vuoi veramente uscire dall'applicazione?", "Avviso", MessageBoxButton.YesNo);
                        if (chiudiResult == MessageBoxResult.Yes)
						{
							shutdownApp = true;
						}    
					}
				}

				if (shutdownApp)
				{
					fpClient.Abort();
                    //System.Windows.Application.Current.Shutdown();
					Environment.Exit(0);
                }
				
			}
		}

		internal void Close()
		{
			if (fpClient != null)
			{
				if (fpClient.InnerChannel.State != System.ServiceModel.CommunicationState.Faulted)
				{
					fpClient.Close();
					fpClient = null;
				}
			}
		}

		private bool isConnectionOK
		{
			get{
				return fpClient != null && !fpClient.State.Equals(CommunicationState.Opened);
			}
			
        }

		private void connectionRestart()
		{
			fpClient.Abort();
			System.Threading.Thread.Sleep( 2000 );
			Open();
		}

		public string GetNome( string base64Template ) {
			return fpClient.IdentificaOrAggiungi( base64Template );
		}

		internal void SyncroOrarioColServer() {

			DateTime orarioMio = DateTime.Now;
			DateTime orarioDelServer = fpClient.GetOrario();

			if( Math.Abs( (orarioDelServer - orarioMio).TotalSeconds ) > 5 ) {
				try {
					Orologio.Set( orarioDelServer );
					_giornale.Info( String.Format( "Orologio del mio sistema = {0} ; Orologio del server {1}. Effettuata sincronizzazione", orarioMio, orarioDelServer ) );
				} catch( Exception ee ) {
					_giornale.Error( "Syncro orologio server", ee );
				}
			} else {
				_giornale.Info( "Orologio di sistema già sincronizzato con quello del sever. Non faccio nulla" );
			}
		}
	}

}
