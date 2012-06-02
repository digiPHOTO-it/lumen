using System;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Config;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using log4net;

namespace Digiphoto.Lumen.Imaging.Wic {

	public class GestoreImmagineSrvImpl : ServizioImpl, IGestoreImmagineSrv  {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( GestoreImmagineSrvImpl ) );

		private Correttore _provinatore;
		private ICorrettoreFactory _correttoreFactory;
		private Resize _correzioneProvino;

		public GestoreImmagineSrvImpl() {


			// Questa è la definizione per provinare.
			_correzioneProvino = new Resize() { 
				latoMax = Configurazione.UserConfigLumen.PixelLatoProvino
			};

			_correttoreFactory = ImagingFactory.Instance.creaCorrettoreFactory();

			// Questo è il provinatore.
			// TODO vedere se si può fare con un generics
			_provinatore = _correttoreFactory.creaCorrettore( typeof(Resize) );

		}

		public IImmagine load( string nomeFile ) {

			if( !File.Exists( nomeFile ) )
				return null;

			return new ImmagineWic( nomeFile );


/*
			// Per essere più veloce, uso un BitmapFrame (almeno ci provo)
			byte [] bytes = File.ReadAllBytes( nomeFile );
			Stream stream = new MemoryStream( bytes );
			BitmapFrame bitmapFrame = WicUtil.ReadBitmapFrame( stream );

			return new ImmagineWic( bitmapFrame );
 */
		}

		public IImmagine creaProvino( IImmagine immagineGrande ) {
			return _provinatore.applica( immagineGrande, _correzioneProvino );
		}

		public void save( IImmagine immagine, string fileName ) {

			FileStream fileStream = new FileStream( fileName, FileMode.Create );

			BitmapSource bmSource = ((ImmagineWic)immagine).bitmapSource;

			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			
			encoder.QualityLevel = 80;

			encoder.Frames.Add( BitmapFrame.Create( bmSource ) );
			encoder.Save( fileStream );
			fileStream.Close();
		}

		public IImmagine applicaCorrezioni( IImmagine immaginePartenza, IEnumerable<Correzione> correzioni ) {
			
			IImmagine modificata = immaginePartenza;

			foreach( Correzione correzione in correzioni )
				modificata = applicaCorrezione( modificata, correzione );

			return modificata;
		}


		public IImmagine applicaCorrezione( IImmagine immaginePartenza, Correzione correzione ) {
			Correttore correttore = _correttoreFactory.creaCorrettore( correzione.GetType() );
			return correttore.applica( immaginePartenza, correzione );
		}

	}
}
