using System;
using Digiphoto.Lumen.SelfService.MobileUI.AutoCloseWindow;
using Digiphoto.Lumen.SelfService.MobileUI.SelfServiceReference;
using System.Collections.Generic;
using System.Collections;
using System.ServiceModel;
using System.Windows;

namespace Digiphoto.Lumen.SelfService.MobileUI.Servizi
{
    public class SSClientSingleton
    {
       
        private static SSClientSingleton instance;

		private SelfServiceClient ssClient;

		public static SSClientSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SSClientSingleton();
                }
                return instance;
            }
        }

        private SSClientSingleton()
        {
			ssClient = new SelfServiceClient();
		}

		internal void Open()
		{
			bool autoCloseNotification = false;
			bool shutdownApp = false;

			while (!shutdownApp && isConnectionOK)
			{
				if (!ssClient.State.Equals(CommunicationState.Opening))
				{
					try
					{
						autoCloseNotification = false;
						ssClient.Open();
					}
					catch (Exception)
					{
						autoCloseNotification = true;
						ssClient.Abort();

						ssClient = new SelfServiceClient();
                    }
				}

				if (autoCloseNotification)
				{					
					var dialogResult = AutoClosingMessageBox.Show(System.Windows.Application.Current, "Il server sembra non rispondere voi rimanere in attesa?", "Avviso", 10000, MessageBoxButton.YesNoCancel);
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
					ssClient.Abort();
                    //System.Windows.Application.Current.Shutdown();
					Environment.Exit(0);
                }
				
			}
		}

		internal void Close()
		{
			if (ssClient != null)
			{
				if (ssClient.InnerChannel.State != System.ServiceModel.CommunicationState.Faulted)
				{
					ssClient.Close();
					ssClient = null;
				}
			}
		}

		private bool isConnectionOK
		{
			get{
				return ssClient != null && !ssClient.State.Equals(CommunicationState.Opened);
			}
			
        }

		private void connectionRestart()
		{
			ssClient.Abort();
			Open();
		}

		internal Dictionary<String, String> getSettings()
		{		
			Open();
			try
			{
				return ssClient.getSettings();
			}
			catch (Exception)
			{
				connectionRestart();
                return ssClient.getSettings();
			}
		}

		internal byte[] getImageLogo()
		{
			byte[] result = new byte[0];
			Open();
			try
			{
				result = ssClient.getImageLogo();
			}
			catch (Exception)
			{
				connectionRestart();
				if (isConnectionOK)
				{
					return ssClient.getImageLogo();
				}             
			}
			return result;
		}

		internal byte[] getImage(Guid fotografiaId)
		{
			byte[] result = new byte[0];
			Open();
			try
			{
				result = ssClient.getImage(fotografiaId);
			}
			catch (Exception)
			{
				connectionRestart();
				if (isConnectionOK)
				{
					return ssClient.getImage(fotografiaId);
				}
			}
			return result;
		}

		internal FotografoDto[] getListaFotografi()
		{
			FotografoDto[] result = new FotografoDto[0];
            Open();
			try
			{
				result = ssClient.getListaFotografi();
			}
			catch (Exception)
			{
				connectionRestart();
                if (isConnectionOK)
				{
					return ssClient.getListaFotografi();
				}
			}
			return result;
		}

		internal CarrelloDto[] getListaCarrelli() {
			CarrelloDto[] result = new CarrelloDto[0];
			Open();
			try {
				result = ssClient.getListaCarrelli();
			} catch( Exception ee ) {
				connectionRestart();
				if( isConnectionOK ) {
					return ssClient.getListaCarrelli();
				}
			}
			return result;
		}


		internal byte[] getImageProvino(Guid fotografiaId)
		{
			byte[] result = new byte[0];
			Open();
			try
			{
				result = ssClient.getImageProvino(fotografiaId);
			}
			catch (Exception)
			{
				connectionRestart();
				if (isConnectionOK)
				{
					return ssClient.getImageProvino(fotografiaId);
				}
			}
			return result;
		}

		internal FotografiaDto[] getListaFotografie(Guid id)
		{
			FotografiaDto[] result = new FotografiaDto[0];
			Open();
			try
			{
				result = ssClient.getListaFotografie(id);
			}
			catch (Exception)
			{
				connectionRestart();
                if (isConnectionOK){
					result = ssClient.getListaFotografie(id);
				}
			}
			return result;
		}

		internal void setMiPiace(Guid id, bool v)
		{
			Open();
			try
			{
				ssClient.setMiPiace(id, v);
			}
			catch (Exception)
			{
				connectionRestart();
            }
		}

		internal IList getListaFotografieDelFotografo(string id, int v, int _PAGE_SIZE)
		{
			IList result = new ArrayList();
			Open();
			try
			{
				result = ssClient.getListaFotografieDelFotografo(id, v, _PAGE_SIZE);
			}
			catch (Exception)
			{
				connectionRestart();
				if (isConnectionOK)
				{
					return ssClient.getListaFotografieDelFotografo(id, v, _PAGE_SIZE);
				}  
			}
			return result;
		}
	}

}
