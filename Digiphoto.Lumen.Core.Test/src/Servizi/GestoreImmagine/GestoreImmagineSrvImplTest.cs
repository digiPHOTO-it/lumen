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
using Digiphoto.Lumen.Core.Test.Util;

namespace Digiphoto.Lumen.Core.Test.Servizi.GestoreImmagine {

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

			string nomeImg = Costanti.getNomeImmagineRandom();
			BitmapSource bmp = new BitmapImage( new Uri( nomeImg ) );

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
			string nomeImg = Costanti.getNomeImmagineRandom();

			GestoreImmagineSrvImpl impl = (GestoreImmagineSrvImpl) (LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>() );
			BitmapSource bmp2 = new BitmapImage( new Uri( nomeImg ) );
			IImmagine immagine = new ImmagineWic( bmp2 );

			// Ricavo la memoria libera prima del test
			GC.Collect();
			long memoryPrima = Process.GetCurrentProcess().WorkingSet64;

			for( int ii = 0; ii < quante; ii++ ) {				

				string tmpFile = System.IO.Path.GetTempFileName();

				if( 1 == 1 ) {
					impl.save( immagine, tmpFile );
				}
#if FALSO
				if( 1 == 0 ) {
					// cosi funziona
					impl.save2( bmp2, tmpFile );
				} else {
#endif
					using( FileStream fileStream = new FileStream( tmpFile, FileMode.Create ) ) {
						JpegBitmapEncoder encoder = new JpegBitmapEncoder();
						encoder.Frames.Add( BitmapFrame.Create( ((ImmagineWic)immagine).bitmapSource ) );
						encoder.Save( fileStream );
						fileStream.Close();
					}
#if FALSO
				}
#endif
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

			immagine.Dispose();

		}

		private List<Fotografia> cercaFotoQuasiasi( int quante ) {

			IFotoExplorerSrv fotoExplorer = LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
			ParamCercaFoto p = new ParamCercaFoto();
			p.paginazione = new Lumen.Util.Paginazione {
				take = quante
			};

			fotoExplorer.cercaFoto( p );
			if( fotoExplorer.fotografie.Capacity == 0 )
				return null;

			return fotoExplorer.fotografie;
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

			const int quante = 1000;
			const int ogniTotPurga = 100;

			List<Fotografia> ff = cercaFotoQuasiasi( 5 );
			
			// Ricavo la memoria libera prima del test
			long memoryPrima = Process.GetCurrentProcess().WorkingSet64;

			for( int ii = 0; ii < quante; ii++ ) {
				foreach( Fotografia f in ff ) {

					AiutanteFoto.idrataImmaginiFoto( f, IdrataTarget.Tutte );

					AiutanteFoto.disposeImmagini( f, IdrataTarget.Tutte );

					// ogni tot iterazioni, vado a liberare la memoria che inspiegabilmente non viene pulita.
					if( (ii % ogniTotPurga) == 0 ) {
						// ATTENZIONE: IMPORTANTE.
						// Se non metto questa formula magica,
						// il GC non pulisce la memoria occupata dalla bitmap (inspiegabilmente)
						FormuleMagiche.rilasciaMemoria();
					}

					long memoryDurante = Process.GetCurrentProcess().WorkingSet64;
					long consumata = (memoryDurante - memoryPrima);

					// Se supero il massimo impostato, probabilmente il gc non sta pulendo.
					if( consumata > maxMem )
						Assert.Fail( "Probabilmente si sta consumando troppa memoria: diff(MB)=" + consumata / 1024 );
				}
			}
		}


		[TestMethod]
		public void outOfMemoryStampa() {

			long memoryPrima = Process.GetCurrentProcess().WorkingSet64;

			string[] nomiFilesImmagine = Costanti.NomiFileImmagini;

			// 10 foto x 50 giri = 500 foto
			foreach( string nomeFileImmagine in nomiFilesImmagine ) {
				for( int giri = 1; giri <= 50; giri++ ) {
					using( PrintServer ps1 = new PrintServer() ) {
						using( PrintQueue coda = ps1.GetPrintQueue( Costanti.NomeStampante ) ) {

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
							image.VerticalAlignment = VerticalAlignment.Center;
							image.HorizontalAlignment = HorizontalAlignment.Center;

							// E' importante fare la dispose dell'oggetto Immagine
							using( ImmagineWic qq = new ImmagineWic( nomeFileImmagine ) ) {

								image.Source = qq.bitmapSource;

								image.Stretch = Stretch.UniformToFill;
								image.StretchDirection = StretchDirection.Both;
								grid.Children.Add( image );
								page1.Children.Add( grid );


								// add the page to the document
								PageContent page1Content = new PageContent();
								page1Content.Child = page1;
								document.Pages.Add( page1Content );

								//
								// ----- STAMPA per davvero
								//
								dialog.PrintDocument( document.DocumentPaginator, "test" + giri.ToString() );

								foreach( var fixedPage in document.Pages.Select( pageContent => pageContent.Child ) ) {
									fixedPage.Children.Clear();
								}
							}

							// ATTENZIONE: IMPORTANTE.
							// Se non metto questa formula magica,
							// il GC non pulisce la memoria occupata dalla bitmap (inspiegabilmente)
							FormuleMagiche.rilasciaMemoria();
						}
					}

					long memoryDopo = Process.GetCurrentProcess().WorkingSet64;
					long consumata = (memoryDopo - memoryPrima);

					// Se supero il massimo impostato, probabilmente il gc non sta pulendo.
					if( consumata > maxMem )
						Assert.Fail( "Probabilmente si sta consumando troppa memoria: diff(MB)=" + consumata / 1024 );
				}
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
