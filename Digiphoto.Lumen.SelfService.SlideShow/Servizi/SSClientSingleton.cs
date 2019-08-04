using System;
using System.Collections.Generic;
using System.Collections;
using System.ServiceModel;
using System.Windows;
using Digiphoto.Lumen.SelfService.SlideShow.SelfServiceReference;
using log4net;
using System.Threading;

namespace Digiphoto.Lumen.SelfService.SlideShow.Servizi {

	public class SSClientSingleton {

		protected static readonly ILog _giornale = LogManager.GetLogger( typeof( SSClientSingleton ) );


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

			// Se sono già connesso, non faccio nulla.
			if( isConnectionOK )
				return;

			_giornale.Debug( "Devo aprire connessione con il servizio wcf" );

			bool shutdownApp = false;

			while( !shutdownApp && !isConnectionOK ) {

				if( !ssClient.State.Equals( CommunicationState.Opening ) ) {

					try {

						ssClient.Open();
						_giornale.Info( "Aperta connessione con il servizio wcf" );

					} catch( Exception ee ) {

						_giornale.Error( "connessione fallita resto in attesa", ee );
						Thread.Sleep( 5000 );
						ssClient.Abort();

						// istanzio un altro client
						ssClient = new SelfServiceClient();
					}
				} else
					Thread.Sleep( 100 );
				
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
			} catch( Exception ee ) {
				_giornale.Warn( ee );
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
