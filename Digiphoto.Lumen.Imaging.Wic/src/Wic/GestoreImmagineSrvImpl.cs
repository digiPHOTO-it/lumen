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
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Applicazione;
using System;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Eventi;
using System.Collections.Generic;

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

		/// <summary>
		/// Carico la IImmagine indicata nel nome del file.
		/// </summary>
		/// <param name="nomeFile"></param>
		/// <returns>se non ci sono riuscito sollevo una eccezione</returns>
		public IImmagine load( string nomeFile ) {

			if( File.Exists( nomeFile ) )
				return new ImmagineWic( nomeFile );
			else
				throw new LumenException( "Nome file insesistente: " + nomeFile + ". Impossibile caricare immagine" );
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
			BitmapSource bmpSrc = ((ImmagineWic)immagine).bitmapSource;
			this.save2( bmpSrc, fileName );
		}


		public void save2( BitmapSource bmpSrc, string fileName ) {

//			_giornale.Debug( "Richiesto di salvare l'immagine su disco: " + fileName );

        
			// A rivaffanculo ancora alla microsoft. Questo pezzo di codice, si lascia il file loccato e non lo chiude.
			// Nello stesso processo !!! E' da non credere!!!
			// Se non ci credi googola questo: "filestream IOException file used by another process"
			// il test-case di nome "creaProvinoANastro" fallisce.
			// Ho dovuto creare un workaround con queso waitForFile.
			using( FileStream fileStream = FileUtil.waitForFile( fileName ) ) {

				// TODO : gestire encoder giusto in base alla estensione del file.
				BitmapEncoder encoder;
				if( fileName.ToLower().EndsWith( ".png" ) )
					encoder = new PngBitmapEncoder();
				else {
					JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();

					if( bmpSrc.PixelWidth == Configurazione.infoFissa.pixelProvino || bmpSrc.PixelHeight == Configurazione.infoFissa.pixelProvino )
						jpgEncoder.QualityLevel = 80;
					else
						jpgEncoder.QualityLevel = 99;
					encoder = jpgEncoder;
				}

//				_giornale.Debug( "Uso quality Level = " + encoder.QualityLevel );

				encoder.Frames.Add( BitmapFrame.Create( bmpSrc ) );
				encoder.Save( fileStream );
				fileStream.Flush();				
				fileStream.Close();
				fileStream.Dispose();
//				_giornale.Debug( "Ok salvataggio file immagine completato" );
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

		private IEntityRepositorySrv<Fotografia> fotografieRepository {
			get {
				return (IEntityRepositorySrv<Fotografia>)LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografia>>();
			}
		}

		public void salvaCorrezioniTransienti( Fotografia fotografia ) {

			fotografieRepository.update( ref fotografia, true );
			fotografieRepository.saveChanges();
		}

		public void salvaCorrezioniAutomatiche( IEnumerable<Fotografia> fotografie, AzioneAuto azioneAuto ) {

			foreach( Fotografia f in fotografie ) {
				Fotografia fotografia = f;
				fotografia.correzioniXml = azioneAuto.correzioniXml;
				fotografieRepository.update( ref fotografia, true );
			}
			fotografieRepository.saveChanges();
		}

		public void idrataImmaginiFoto( Fotografia foto, IdrataTarget target, bool forzatamente ) {

			String nomeCompleto = null;
			try {

				//
				if( forzatamente || foto.imgProvino == null )
					if( (target & IdrataTarget.Provino) != 0 ) {
						nomeCompleto = PathUtil.nomeCompletoProvino( foto );
						foto.imgProvino = this.load( nomeCompleto );
					}

				//
				if( forzatamente || foto.imgOrig == null )
					if( (target & IdrataTarget.Originale) != 0 ) {
						nomeCompleto = PathUtil.nomeCompletoFoto( foto );
						foto.imgOrig = this.load( nomeCompleto );
					}

				//
				if( forzatamente || foto.imgRisultante == null )
					if( (target & IdrataTarget.Risultante) != 0 ) {
						nomeCompleto = PathUtil.nomeCompletoRisultante( foto );
						foto.imgRisultante = this.load( nomeCompleto );
					}

			} catch( Exception ee ) {

				// Se non riesco a caricare una immagine, non posso farci niente qui. Devo tirare dritto.
				_giornale.Warn( ee.Message + " nomeCompleto=" + nomeCompleto, ee );

				Messaggio msg = new Messaggio( ee.Source );
				msg.esito = Esito.Errore;
				msg.descrizione = ee.Message;
				msg.showInStatusBar = true;
				pubblicaMessaggio( msg );

				throw ee;
			}
		}

	}
}
