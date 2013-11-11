using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Config;
using System.IO;
using System.Windows.Media.Imaging;
using log4net;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Windows.Media.Effects;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using System.Data;

namespace Digiphoto.Lumen.Imaging.Wic {

	public class GestoreImmagineSrvImpl : ServizioImpl, IGestoreImmagineSrv  {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( GestoreImmagineSrvImpl ) );

		private Correttore _provinatore;
		private ICorrettoreFactory _correttoreFactory;
		private Resize _correzioneProvino;

		public GestoreImmagineSrvImpl() {

			// Questa è la definizione per provinare.
			_correzioneProvino = new Resize {
				latoMax = Configurazione.infoFissa.pixelProvino
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

		}

		public IImmagine creaProvino( IImmagine immagineGrande, long sizeLatoMax ) {
			Resize resize = new Resize {
				latoMax = sizeLatoMax
			};
			return _provinatore.applica( immagineGrande, resize );
		}

		public IImmagine creaProvino( IImmagine immagineGrande ) {
			return _provinatore.applica( immagineGrande, _correzioneProvino );
		}

		public void save( IImmagine immagine, string fileName ) {

			_giornale.Debug( "Richiesto di salvare l'immagine su disco: " + fileName );

			using( FileStream fileStream = new FileStream( fileName, FileMode.Create ) ) {

				BitmapSource bmSource = ((ImmagineWic)immagine).bitmapSource;

				// TODO : gestire encoder giusto in base alla estensione del file.
				JpegBitmapEncoder encoder = new JpegBitmapEncoder();

				if( immagine.ww == Configurazione.infoFissa.pixelProvino || immagine.hh == Configurazione.infoFissa.pixelProvino )
					encoder.QualityLevel = 80;
				else
					encoder.QualityLevel = 99;

				_giornale.Debug( "Uso quality Level = " + encoder.QualityLevel );

				encoder.Frames.Add( BitmapFrame.Create( bmSource ) );
				encoder.Save( fileStream );

				fileStream.Close();
				_giornale.Debug( "Ok salvataggio file immagine completato" );
			}
		}

		public Correttore getCorrettore( Correzione correzione ) {
			// Se non ce l'ho in cache, allora lo creo.
			return _correttoreFactory.creaCorrettore( correzione.GetType() );
		}

		public Correttore getCorrettore( TipoCorrezione tipoCorrezione ) {
			return _correttoreFactory.creaCorrettore( tipoCorrezione );
		}

		public Correttore getCorrettore( object oo ) {
			if( oo is ShaderEffect )
				return getCorrettore( EffectsUtil.tipoCorrezioneCorrispondente( oo as ShaderEffect ) );
			else
				return null;
		}

		public void salvaCorrezioniTransienti( Fotografia fotografia ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;

			objContext.Fotografie.Attach( fotografia );
			objContext.ObjectContext.ObjectStateManager.ChangeObjectState( fotografia, EntityState.Modified );
			objContext.SaveChanges();
		}
	}
}
