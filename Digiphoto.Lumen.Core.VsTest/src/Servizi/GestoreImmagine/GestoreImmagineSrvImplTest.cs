using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Imaging;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Core.Database;
using System.Printing;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Digiphoto.Lumen.Core.VsTest {

	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class GestoreImmagineSrvImplTest {

		#region Preparazione

		const long maxMem = 800*1000*1000; // 800 MB

		[ClassInitialize]
		public static void initialize( TestContext tc ) {
			LumenApplication.Instance.avvia();
		}

		[ClassCleanup]
		public static void cleanup() {
			LumenApplication.Instance.ferma();
		}

		#endregion Preparazione

		public GestoreImmagineSrvImplTest() {
		}


		[TestMethod]
		public void outOfMemorySimpleTest() {

			BitmapSource bmp = new BitmapImage( new Uri(@"C:\Users\Public\Pictures\Sample Pictures\koala.jpg") );

			long memoryBefore = Process.GetCurrentProcess().WorkingSet64;

			for( int ii = 0; ii < 500; ii++ ) {

				saveBitmapToFile( bmp );

				long memoryAfter = Process.GetCurrentProcess().WorkingSet64;
				long consumption = (memoryAfter - memoryBefore);

				// Check memory low
				if( consumption > (800*1000*1000) )
					Assert.Fail( "Memory low. Consuption=" + (consumption/1024) +  "MB" );
			}
		}

		private void saveBitmapToFile( BitmapSource bmp ) {

			string tmpFile = Path.GetTempFileName();

			using( FileStream fileStream = new FileStream( tmpFile, FileMode.Create ) ) {
				JpegBitmapEncoder encoder = new JpegBitmapEncoder();
				encoder.Frames.Add( BitmapFrame.Create( bmp ) );
				encoder.Save( fileStream );
				fileStream.Close();
			}

			File.Delete( tmpFile );
		}


		[TestMethod]
		public void outOfMemoryTest() {

			const int quante = 1000;

			GestoreImmagineSrvImpl impl = (GestoreImmagineSrvImpl) (LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>() );

/*
			IFotoExplorerSrv fotoExplorer = LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			ParamCercaFoto p = new ParamCercaFoto();
			p.paginazione = new Lumen.Util.Paginazione {
				take = 1
			};
			fotoExplorer.cercaFoto( p );
			if( fotoExplorer.fotografie.Capacity == 0 )
				return;
			
			Fotografia f = fotoExplorer.fotografie[0];

			IImmagine immagine = AiutanteFoto.idrataImmagineGrande( f );
*/
			
//			IImmagine immagine = new ImmagineWic(@"C:\Users\Public\Pictures\Sample Pictures\koala.jpg");
			BitmapSource bmp2 = new BitmapImage( new Uri( @"C:\Users\Public\Pictures\Sample Pictures\koala.jpg" ) );
			IImmagine immagine = new ImmagineWic( bmp2 );





			// Ricavo la memoria libera prima del test
			GC.Collect();
			long memoryPrima = Process.GetCurrentProcess().WorkingSet64;

			for( int ii = 0; ii < quante; ii++ ) {				

				string tmpFile = System.IO.Path.GetTempFileName();

				if( 1 == 1 ) {
					impl.save( immagine, tmpFile );
				}
				if( 1 == 0 ) {
					// cosi funziona
					impl.save2( bmp2, tmpFile );
				} else {
					using( FileStream fileStream = new FileStream( tmpFile, FileMode.Create ) ) {
						JpegBitmapEncoder encoder = new JpegBitmapEncoder();
						encoder.Frames.Add( BitmapFrame.Create( ((ImmagineWic)immagine).bitmapSource ) );
						encoder.Save( fileStream );
						fileStream.Close();
					}
				}




				System.IO.File.Delete( tmpFile );

				long memoryDurante = Process.GetCurrentProcess().WorkingSet64;

				long diffDurante = (memoryDurante - memoryPrima);

				if( (ii % 10) == 0 )
					Console.WriteLine( "consumo = " + (diffDurante / 1024) );

				// Se supero il massimo impostato, probabilmente il gc non sta pulendo.
				if( diffDurante > maxMem ) {
					Assert.Fail( "Probabilmente si sta consumando troppa memoria: diff(MB)=" + diffDurante / 1024 );
				}
			}
		}


		private Fotografia cercaUnaFotoQuasiasi() {

			IFotoExplorerSrv fotoExplorer = LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			ParamCercaFoto p = new ParamCercaFoto();
			p.paginazione = new Lumen.Util.Paginazione {
				take = 1
			};

			fotoExplorer.cercaFoto( p );
			if( fotoExplorer.fotografie.Capacity == 0 )
				return null;

			return fotoExplorer.fotografie[0];
		}

		[TestMethod]
		public void creaProvinoANastro() {

			Fotografia f = cercaUnaFotoQuasiasi();
			for( int ii = 0; ii < 100; ii++ ) {
				AiutanteFoto.creaProvinoFoto( f );
			}
		}

		[TestMethod]
		public void tornaOriginaleANastro() {

			using( new UnitOfWorkScope() ) {
				Fotografia f = cercaUnaFotoQuasiasi();
				IFotoRitoccoSrv srv = LumenApplication.Instance.getServizioAvviato<IFotoRitoccoSrv>();
				for( int ii = 0; ii < 100; ii++ ) {
					srv.tornaOriginale( f );
				}
			}
		}


		[TestMethod]
		public void outOfMemoryImmagini() {

			const int quante = 500;

			Fotografia f = cercaUnaFotoQuasiasi();

			// Ricavo la memoria libera prima del test
			long memoryPrima = Process.GetCurrentProcess().WorkingSet64;

			for( int ii = 0; ii < quante; ii++ ) {

				AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Tutte );

//				AiutanteFoto.creaProvinoFoto( f );

				AiutanteFoto.disposeImmagini( f, IdrataTarget.Tutte );

				long memoryDurante = Process.GetCurrentProcess().WorkingSet64;
				long consumata = (memoryDurante - memoryPrima);

				// Se supero il massimo impostato, probabilmente il gc non sta pulendo.
				if( consumata > maxMem )
					Assert.Fail( "Probabilmente si sta consumando troppa memoria: diff(MB)=" + consumata / 1024 );
			}
		}

		[TestMethod]
		public void outOfMemoryStampa() {

			long memoryPrima = Process.GetCurrentProcess().WorkingSet64;
			string nomeFileImmagine = @"C:\Users\bluca\Pictures\tante\2011-05-23 Mirabilandia\Mirabilandia 023.JPG";

			for( int giri = 1; giri <= 50; giri++ ) {
				using( PrintServer ps1 = new PrintServer() ) {
					using( PrintQueue coda = ps1.GetPrintQueue( "Shinko CHC-S2145" ) ) {

						PrintDialog dialog = new PrintDialog();
						dialog.PrintQueue = coda;

						Size areaStampabile = new Size( dialog.PrintableAreaWidth, dialog.PrintableAreaHeight );
						// Ora creo il documento che andrò a stampare.
						// L'uso di un FixedDocument, mi permetterà di interagire con misure, dimensioni e margini
						FixedDocument document = new FixedDocument();
						document.DocumentPaginator.PageSize = new Size( dialog.PrintableAreaWidth, dialog.PrintableAreaHeight );

						// Creo una pagina della grandezza massima
						FixedPage page1 = new FixedPage();
						page1.Width = document.DocumentPaginator.PageSize.Width;
						page1.Height = document.DocumentPaginator.PageSize.Height;
						page1.VerticalAlignment = VerticalAlignment.Center;
						page1.HorizontalAlignment = HorizontalAlignment.Center;


						// Per fare in modo che l'immagine venga centrata bene automaticamente, e non venga tagliata solo da una parte ma nel centro,
						// non devo mettere le dimensioni al componente Image, ma devo creare 
						// una Grid più esterna con le dimensioni precise.
						Grid grid = new Grid();
						grid.Height = page1.Height;
						grid.Width = page1.Width;


						// Creo una immagine che contiene la bitmap da stampare
						Image image = new Image();
						//						image.BeginInit();
						image.VerticalAlignment = VerticalAlignment.Center;
						image.HorizontalAlignment = HorizontalAlignment.Center;


						ImmagineWic qq = new ImmagineWic( nomeFileImmagine );

#if NONONO
//						BitmapImage bi = new BitmapImage( new Uri(nomeFileImmagine) );
//						bi.CacheOption = BitmapCacheOption.OnLoad;


						//						BitmapSource clone = bmp.Clone();
						//						clone.Freeze();
						using( FileStream fs = new FileStream( nomeFileImmagine, FileMode.Open ) ) {
							image.Source = CreateImageSource( fs );
						}
#endif
						image.Source = qq.bitmapSource;

						image.Stretch = Stretch.UniformToFill;
						image.StretchDirection = StretchDirection.Both;

						//						image.EndInit();

						grid.Children.Add( image );
						page1.Children.Add( grid );


						// add the page to the document
						PageContent page1Content = new PageContent();
						page1Content.Child = page1;
						document.Pages.Add( page1Content );

						//
						// ----- STAMPA per davvero
						//
						dialog.PrintDocument( document.DocumentPaginator, "test"+giri.ToString() );

						foreach( var fixedPage in document.Pages.Select( pageContent => pageContent.Child ) ) {
							fixedPage.Children.Clear();
						}


						// ATTENZIONE: IMPORTANTE.
						// Se non metto questa formula magica,
						// il GC non pulisce la memoria occupata dalla bitmap (inspiegabilmente)
						Dispatcher.CurrentDispatcher.Invoke( DispatcherPriority.SystemIdle, new DispatcherOperationCallback( delegate {
							return null;
						} ), null );

					}
				}

//				GC.Collect();
//				GC.WaitForPendingFinalizers();
//				GC.Collect();

				long memoryDopo = Process.GetCurrentProcess().WorkingSet64;
				long consumata = (memoryDopo - memoryPrima);

				// Se supero il massimo impostato, probabilmente il gc non sta pulendo.
				if( consumata > maxMem )
					Assert.Fail( "Probabilmente si sta consumando troppa memoria: diff(MB)=" + consumata / 1024 );
			}
		}

		private BitmapSource CreateImageSource( Stream stream ) {
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.CacheOption = BitmapCacheOption.OnLoad;
			bi.StreamSource = stream;
			bi.EndInit();
			bi.Freeze();
			BitmapSource prgbaSource = new FormatConvertedBitmap( bi, PixelFormats.Pbgra32, null, 0 );
			WriteableBitmap bmp = new WriteableBitmap( prgbaSource );
			int w = bmp.PixelWidth;
			int h = bmp.PixelHeight;
			int[] pixelData = new int[w * h];
			//int widthInBytes = 4 * w;
			int widthInBytes = bmp.PixelWidth * (bmp.Format.BitsPerPixel / 8); //equals 4*w
			bmp.CopyPixels( pixelData, widthInBytes, 0 );
			bmp.WritePixels( new Int32Rect( 0, 0, w, h ), pixelData, widthInBytes, 0 );
			bi = null;
			return bmp;
		}

	}
}
