using System;
using System.Collections.Generic;
using System.Collections;
using System.ServiceModel;
using System.Windows;
using Digiphoto.Lumen.OnRide.UI.FingerServiceReference;

namespace Digiphoto.Lumen.OnRide.UI.Servizi {

	public class FPSClientSingleton
    {
       
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
			Open();
		}

		public string GetNome( string base64Template ) {
			return fpClient.IdentificaOrAggiungi( base64Template );
		}
		
		

	
	}

}
