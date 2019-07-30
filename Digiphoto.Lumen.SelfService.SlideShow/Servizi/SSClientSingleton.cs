using System;
using System.Collections.Generic;
using System.Collections;
using System.ServiceModel;
using System.Windows;
using Digiphoto.Lumen.SelfService.SlideShow.SelfServiceReference;

namespace Digiphoto.Lumen.SelfService.SlideShow.Servizi {
	public class SSClientSingleton {

		private static SSClientSingleton instance;

		private SelfServiceClient ssClient;

		public static SSClientSingleton Instance {
			get {
				if( instance == null ) {
					instance = new SSClientSingleton();
				}
				return instance;
			}
		}

		private SSClientSingleton() {
			ssClient = new SelfServiceClient();
		}

		internal void Open() {
			bool autoCloseNotification = false;
			bool shutdownApp = false;

			while( !shutdownApp && !isConnectionOK ) {
				if( !ssClient.State.Equals( CommunicationState.Opening ) ) {
					try {
						autoCloseNotification = false;
						ssClient.Open();
					} catch( Exception ) {
						autoCloseNotification = true;
						ssClient.Abort();

						ssClient = new SelfServiceClient();
					}
				}

				if( autoCloseNotification ) {
					var dialogResult = AutoClosingMessageBox.Show( System.Windows.Application.Current, "Il server sembra non rispondere voi rimanere in attesa?", "Avviso", 10000, MessageBoxButton.YesNoCancel );
					if( dialogResult == MessageBoxResult.No ) {
						MessageBoxResult chiudiResult = MessageBox.Show( "Vuoi veramente uscire dall'applicazione?", "Avviso", MessageBoxButton.YesNo );
						if( chiudiResult == MessageBoxResult.Yes ) {
							shutdownApp = true;
						}
					}
				}

				if( shutdownApp ) {
					ssClient.Abort();
					//System.Windows.Application.Current.Shutdown();
					Environment.Exit( 0 );
				}

			}
		}

		internal async System.Threading.Tasks.Task<object> getImageAsync( Guid fotografiaId ) {
			byte[] result = new byte[0];
			Open();
			try {
				result = await ssClient.getImageAsync( fotografiaId );
			} catch( Exception ) {
				connectionRestart();
				if( isConnectionOK ) {
					return ssClient.getImageAsync( fotografiaId );
				}
			}
			return result;
		}

		internal void Close() {
			if( ssClient != null ) {
				if( ssClient.InnerChannel.State != System.ServiceModel.CommunicationState.Faulted ) {
					ssClient.Close();
					ssClient = null;
				}
			}
		}

		private bool isConnectionOK {
			get {
				return ssClient != null && ssClient.State == CommunicationState.Opened;
			}
		}

		private void connectionRestart() {
			ssClient.Abort();
			Open();
		}

		internal Dictionary<String, String> getSettings() {
			Open();
			try {
				return ssClient.getSettings();
			} catch( Exception ) {
				connectionRestart();
				return ssClient.getSettings();
			}
		}

		internal byte[] getImage( Guid fotografiaId ) {
			byte[] result = new byte[0];
			Open();
			try {
				result = ssClient.getImage( fotografiaId );
			} catch( Exception ) {
				connectionRestart();
				if( isConnectionOK ) {
					return ssClient.getImage( fotografiaId );
				}
			}
			return result;
		}

		internal FotografoDto[] getListaFotografi() {
			FotografoDto[] result = new FotografoDto[0];
			Open();
			try {
				result = ssClient.getListaFotografi();
			} catch( Exception ) {
				connectionRestart();
				if( isConnectionOK ) {
					return ssClient.getListaFotografi();
				}
			}
			return result;
		}

		internal byte[] getImageProvino( Guid fotografiaId ) {
			byte[] result = new byte[0];
			Open();
			try {
				result = ssClient.getImageProvino( fotografiaId );
			} catch( Exception ) {
				connectionRestart();
				if( isConnectionOK ) {
					return ssClient.getImageProvino( fotografiaId );
				}
			}
			return result;
		}

		internal FotografiaDto[] getListaFotografieDelFotografo( RicercaFotoParam param ) {
			{
				FotografiaDto[] result = new FotografiaDto[0];
				Open();

				try {
					result = ssClient.getListaFotografieDelFotografo( param );
				} catch( Exception ) {
					connectionRestart();
					if( isConnectionOK ) {
						return ssClient.getListaFotografieDelFotografo( param );
					}
				}
				return result;
			}
		}
	}
}
