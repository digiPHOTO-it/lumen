using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using System.IO;
using System.Windows.Media.Imaging;
using Digiphoto.Lumen.Imaging.Wic.Correzioni;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging.Correzioni;

namespace Digiphoto.Lumen.Imaging.Wic {

	public class GestoreImmagineSrvImpl : ServizioImpl, IGestoreImmagineSrv  {

		ProvinatoreWic _provinatoreWic;

		public GestoreImmagineSrvImpl() {
			_provinatoreWic = new ProvinatoreWic();
		}

		public IImmagine load( string nomeFile ) {

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
			return _provinatoreWic.creaProvino( immagineGrande );
		}

		public void save( IImmagine immagine, string fileName ) {

			FileStream fileStream = new FileStream( fileName, FileMode.Create );

			BitmapSource bmSource = ((ImmagineWic)immagine).bitmapSource;

			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			
			// encoder.QualityLevel = 100;

			encoder.Frames.Add( BitmapFrame.Create( bmSource ) );
			encoder.Save( fileStream );
		}

		public IImmagine applicaCorrezioni( IImmagine immaginePartenza, ICollection<Model.Correzione> correzioni ) {
			
			CorrettoreFactory factory = new CorrettoreFactory();

			IImmagine modificata = immaginePartenza;

			foreach( Correzione correzione in correzioni ) {
				Correttore correttore = factory.creaCorrettore( correzione.GetType() );
				modificata = correttore.applica( modificata, correzione );
			}

			return modificata;
		}
	}
}
