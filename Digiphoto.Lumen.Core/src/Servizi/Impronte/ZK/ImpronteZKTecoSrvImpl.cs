using Digiphoto.Lumen.Core.Servizi.Impronte.ZK;
using Digiphoto.Lumen.Servizi;
using libzkfpcsharp;
using log4net;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace Digiphoto.Lumen.Core.Servizi.Impronte {

	public class ImpronteZKTecoSrvImpl : ServizioImpl, IImpronteSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ImpronteZKTecoSrvImpl ) );

		#region Campi

		private IntPtr deviceHandle;
		private IntPtr dbHandle;
		private bool isTimeToDie;

		private byte[] captureTemplate = new byte[2048];
		private int templateSize = 2048;

		private int imgWidth = 0;
		private int imgHeight = 0;
		private int imgDpi = 0;
		private byte[] imgBuffer;

		#endregion


		#region Proprieta

		public InfoScanner infoScanner {
			get;
			private set;
		}

		#endregion


		public ImpronteZKTecoSrvImpl() {

			deviceHandle = IntPtr.Zero;
			dbHandle = IntPtr.Zero;
		}

		public override void start() {

			bool tuttoBene = true;
			statoRun = StatoRun.Stopped;
			infoScanner = null;

			int ret = zkfp2.Init();

			if( ret == zkfperrdef.ZKFP_ERR_OK )
				_giornale.Info( "Inizializzato scanner ok" );
			else {
				tuttoBene = false;
				_giornale.Error( "err=" + ret + " : " + ZKErrors.getDescrizione( ret ) );
			}

			// Apro il device
			int idx = 0;
			if( tuttoBene ) {
				if( IntPtr.Zero == (deviceHandle = zkfp2.OpenDevice( idx )) ) {
					tuttoBene = false;
					_giornale.Error( "Open device " + idx + " fallita" );
				} else
					_giornale.Info( "Open device connessione ok" );
			}

			// Inizializzo la cache
			if( tuttoBene ) {
				if( IntPtr.Zero == (dbHandle = zkfp2.DBInit()) ) {
					_giornale.Error( "Inizializzazione cache fallita" );
					tuttoBene = false;
				} else
					_giornale.Info( "Inizializzazione cache ok" );
			}

			if( tuttoBene ) {
				try {
					LeggiParametriScanner();
				} catch( Exception ee ) {
					tuttoBene = false;
					_giornale.Fatal( "Lettura parametri scanner", ee );
				}

			}


			if( tuttoBene )
				base.start();
			else
				zkfp2.Terminate();
		}

		/**
		 * Attenzione ::
		 * Questo metodo funziona anche se lo scanner è stato scollegato fisicamente dal PC.
		 * Probabilmente è il driver che risponde e non interroga veramente il dispositivo attraverso la porta usb
		 */
		private void LeggiParametriScanner() {

			if( deviceHandle == IntPtr.Zero )
				throw new InvalidOperationException( "Il device handle dello scanner non è inizializzato" );

			byte[] paramValue = new byte[4];

			int size = 4;
			zkfp2.GetParameters( deviceHandle, 1, paramValue, ref size );
			zkfp2.ByteArray2Int( paramValue, ref imgWidth );

			size = 4;
			zkfp2.GetParameters( deviceHandle, 2, paramValue, ref size );
			zkfp2.ByteArray2Int( paramValue, ref imgHeight );
			
			// Ora che so le dimensioni dell'area di scanning, alloco il buffer per contenere i byte
			imgBuffer = new byte[imgWidth * imgHeight];

			// ricavo qualche info aggiuntiva che può essere utile da stampare
			byte[] buffer = new byte [1024];
			infoScanner = new InfoScanner();

			size = 1024;
			zkfp2.GetParameters( deviceHandle, 1101, buffer, ref size );
			infoScanner.vendor = Encoding.UTF8.GetString( buffer, 0, size );

			size = 1024;
			zkfp2.GetParameters( deviceHandle, 1102, buffer, ref size );
			infoScanner.productName = Encoding.UTF8.GetString( buffer, 0, size );

			size = 1024;
			zkfp2.GetParameters( deviceHandle, 1103, buffer, ref size );
			infoScanner.serialNumber = Encoding.UTF8.GetString( buffer, 0, size );
		}

		Thread captureThread = null;
		OnImmagineAcquisita callback;
		public void Listen( OnImmagineAcquisita callback ) {

			this.callback = callback;
			captureThread = new Thread( new ThreadStart( DoCapture ) );
			captureThread.IsBackground = true;
			captureThread.Start();
			isTimeToDie = false;

			_giornale.Debug( "Avviato ascoltatore eventi di cattura immagine da scanner" );
		}

		private void DoCapture() {

			while( ! isTimeToDie ) {

				int ret = zkfp2.AcquireFingerprint( deviceHandle, imgBuffer, captureTemplate, ref templateSize );
				if( ret == zkfp.ZKFP_ERR_OK ) {

					// Ricreo l'immagine
					MemoryStream stream = new MemoryStream();
					BitmapFormat.GetBitmap( imgBuffer, imgWidth, imgHeight, ref stream );
					Bitmap bmp = new Bitmap( stream );

					
					// Creo il template
					String strTemplate = zkfp2.BlobToBase64( captureTemplate, templateSize );

					// Preparo la callback ed eseguo
					ScansioneEvent sevent = new ScansioneEvent {
						tempo = DateTime.Now,
						isValid = true,
						bmpFileName = Path.ChangeExtension( Path.GetTempFileName(), ".bmp" ),
						strBase64Template = strTemplate
					};

					// TODO questo salvataggio potrebbe anche non servire (per risparmiare tempo)
					// bmp.Save( sevent.bmpFileName );

					EmissioneFeedback( true );

					//
					callback( this, sevent );


					// SendMessage( FormHandle, MESSAGE_CAPTURED_OK, IntPtr.Zero, IntPtr.Zero );
				}
				Thread.Sleep( 300 );
			}
		}

		private void EmissioneFeedback( bool suonoPositivo ) {

			int giri = suonoPositivo ? 1 : 3;
			int durataMs = suonoPositivo ? 400 : 200;

			// Accendo la luce verde
			byte[] paramValue2 = new byte[4];
			zkfp.Int2ByteArray( 1, paramValue2 );
			zkfp2.SetParameters( deviceHandle, 102, paramValue2, 4 );
			//

			for( int giro = 1; giro <= giri; giro++ ) {
				byte[] paramValue1 = new byte[4];
				zkfp.Int2ByteArray( 1, paramValue1 );
				zkfp2.SetParameters( deviceHandle, 104, paramValue1, 4 );
				Thread.Sleep( durataMs );
				zkfp.Int2ByteArray( 0, paramValue1 );
				zkfp2.SetParameters( deviceHandle, 104, paramValue1, 4 );

				if( suonoPositivo == false )
					Thread.Sleep( (int)(durataMs * 0.8) );
			}

			// Spengo la luce verde
			zkfp.Int2ByteArray( 0, paramValue2 );
			zkfp2.SetParameters( deviceHandle, 102, paramValue2, 4 );

		}

		public override void stop() {

			int ret;

			// Causo l'uscita dal thread ed aspetto
			isTimeToDie = true;
			if( captureThread != null ) {
				captureThread.Join();
				captureThread = null;
			}

			if( dbHandle != IntPtr.Zero ) {
				ret = zkfp2.DBFree( dbHandle );
				dbHandle = IntPtr.Zero;
				if( ret == zkfp.ZKFP_ERR_OK )
					_giornale.Info( "Release della cache ok" );
				else
					Console.Error.WriteLine( "Release cache : err=" + ret + " : " + ZKErrors.getDescrizione( ret ) );
			}

			// Inizializza il device che deve essere connesso, altrimenti da errore
			if( deviceHandle != IntPtr.Zero ) {
				ret = zkfp2.CloseDevice( deviceHandle );
				deviceHandle = IntPtr.Zero;
				if( ret == zkfp.ZKFP_ERR_OK )
					_giornale.Info( "Chiusura device handle ok" );
				else
					_giornale.Error( "Chiusura device handle : err=" + ret + " : " + ZKErrors.getDescrizione( ret ) );
			}

			// Rilascia le risorse utilizzate dalla libreria
			ret = zkfp2.Terminate();
			if( ret == zkfp.ZKFP_ERR_OK )
				Console.Out.WriteLine( "Terminate ok" );
			else
				_giornale.Error( "Terminate : err=" + ret + " : " + ZKErrors.getDescrizione( ret ) );


			base.stop();
		}

		public bool testScannerConnected() {

			bool connesso = false;
			if( this.statoRun == StatoRun.Running ) {

				if( deviceHandle != IntPtr.Zero ) {

					try {

						LeggiParametriScanner();

						if( infoScanner != null )
							connesso = true;

					} catch( Exception ee ) {
						_giornale.Warn( "errore lettura scanner", ee );
					}
				}
			}

			return connesso;
		}

	}
}
