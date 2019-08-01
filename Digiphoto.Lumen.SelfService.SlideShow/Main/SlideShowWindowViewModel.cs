using Digiphoto.Lumen.SelfService.SlideShow.Config;
using Digiphoto.Lumen.SelfService.SlideShow.SelfServiceReference;
using Digiphoto.Lumen.SelfService.SlideShow.Servizi;
using Digiphoto.Lumen.UI;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.UI.Pubblico;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Digiphoto.Lumen.SelfService.SlideShow.Main {



	public class SlideShowWindowViewModel : ClosableWiewModel, IContenitoreGriglia {

		public SlideShowWindowViewModel() {

			_giornale.Debug( "Avvio" );

			puntatore = 999;
			numColonne = 3;
			numRighe = 2;

			// Istanzio collezione vuota degli oggetti da visualizzare
			this.fotografieDto = new ObservableCollectionEx<FotografiaDto>();

			// carico configurazione
			caricaConfigurazione();


			timer = new DispatcherTimer();
			timer.Interval = new TimeSpan( 0, 0, 2 );
			timer.Tick += new EventHandler( this.ProssimoTick );
		}

		#region Proprieta

		private DispatcherTimer timer;

		public UserConfig userConfig {
			get;
			private set;
		}

		public ObservableCollectionEx<FotografiaDto> fotografieDto {
			get;
			set;
		}

		public short numRighe { get; set; }

		public short numColonne { get; set; }

		private int puntatore { get; set; }

		public int totFrame {
			get {
				return numRighe * numColonne;
			}
		}

		#endregion Proprieta

		#region Metodi

		public void Start() {
			timer.Start();
		}

		public void Stop() {
			timer.Stop();
		}

		async void caricaFotografieAsync() {

			RicercaFotoParam parametri = new RicercaFotoParam();
			parametri.fotografoId = "ONRIDE1";

			var fotos = SSClientSingleton.Instance.getListaFotografieDelFotografo( parametri );
			fotografieDto = new ObservableCollectionEx<FotografiaDto>( fotos );

			foreach( FotografiaDto foto in fotos ) {

				var bitmapImage = new BitmapImage();
				using( var memoryStream = new MemoryStream() ) {
					var buffer = await SSClientSingleton.Instance.getImageAsync( foto.id );


				}
			}

		}

		internal BitmapSource GetBitmap( FotografiaDto fotografiaDto ) {

			byte [] bytes = SSClientSingleton.Instance.getImage( fotografiaDto.id );

			MemoryStream memoryStream = new MemoryStream( bytes );
			BitmapImage bi = new BitmapImage();
			memoryStream.Position = 0;
			bi.BeginInit();
			bi.StreamSource = memoryStream;
			bi.EndInit();
			BitmapSource bitmapSource = bi;

			// Se non frizzo, non riesco a passare queste bitmap da un thread all'altro.
			bitmapSource.Freeze();

			return bitmapSource;
		}

		int pagina = 0;
		bool arrivatoInFondo;
		void caricaProssimaPagina() {

			++pagina;

			// Prendo un elemento in piu per capire se sono alla fine della lista.
			RicercaFotoParam param = new RicercaFotoParam {
				fotografoId = userConfig.idFotografo,
				giorno = DateTime.Today,
				skip = ((pagina - 1) * totFrame),
				take = totFrame+1
			};

			
			var listaAttesaTmp = SSClientSingleton.Instance.getListaFotografieDelFotografo( param );

			int quanti = Math.Min( totFrame, listaAttesaTmp.Length );
			listaAttesa = new FotografiaDto[quanti];

			Array.Copy( listaAttesaTmp, listaAttesa, quanti );

			arrivatoInFondo = (listaAttesaTmp.Length < totFrame);

			puntatore = 0;
		}

		

		FotografiaDto[] listaAttesa = null;

		private void ProssimoTick( object state, EventArgs e ) {

			if( listaAttesa != null && puntatore >= listaAttesa.Length && arrivatoInFondo ) {
				// Non ci sono abbastanza foto. Ricomincio da capo
				pagina = 0;
				puntatore = 999;
			}

 			if( puntatore >= totFrame )
				caricaProssimaPagina();

			// Vediamo se ho delle foto da mostrare
			if( listaAttesa.Length > 0 ) {

				if( fotografieDto.Count > puntatore )
					fotografieDto[puntatore] = listaAttesa[puntatore];
				else
					fotografieDto.Add( listaAttesa[puntatore] );
			
				++puntatore;
			}

		}

		void caricaConfigurazione() {
			userConfig = UserConfigSerializer.deserialize();
			if( userConfig == null )
				userConfig = new UserConfig {
					intervallo = 2000,
					numRighe = 2,
					numColonne = 3
				};
		}

		/// <summary>
		/// Salvo la nuova configurazione e poi la ricarico
		/// </summary>
		/// <param name="newUserConfig"></param>
		public void SalvaNuovaConfigurazione( UserConfig newUserConfig ) {

			UserConfigSerializer.serializeToFile( newUserConfig );

			caricaConfigurazione();

			// Per sicurezza riparto da capo
			pagina = 0;
			puntatore = 999;
		}

		#endregion Metodi

	}
}
