using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using System.IO;
using Digiphoto.Lumen.Applicazione;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Imaging.Wic;

namespace Digiphoto.Lumen.UI.Pubblico {

	public class SlideShowViewModel : ClosableWiewModel {

		private DispatcherTimer _orologio;

		public SlideShowViewModel() {
		}

		#region Proprietà

		private int numSlideCorrente {
			get;
			set;
		}

		public ObservableCollection<Slide> slidesVisibili {
			get;
			set;
		}

		private int totSlidesPerPagina {
			get {
				return slideShow.colonne * slideShow.righe;
			}
		}

		/// <summary>
		/// Slide Show corrente
		/// </summary>
		public SlideShow slideShow {
			get;
			private set;
		}

		public bool isRunning {
			get {
				return _orologio != null && _orologio.IsEnabled;
			}
		}

		#endregion

		#region Metodi

		protected override void OnDispose() {

			try {
				if( _orologio != null ) {
					_orologio.Stop();
					_orologio.Tick -= orologio_Tick;
				}				
			} finally {
				base.OnDispose();
			}
		}

		protected override void OnRequestClose() {
			stop();  // Fermo lo slide show
			base.OnRequestClose();
		} 

		
		public void start() {
			_orologio.Start();
		}

		public void stop() {
			_orologio.Stop();
		}

		
		/// <summary>
		/// Creo un nuovo slide show.
		/// </summary>
		/// <param name="?"></param>
		public void create( IEnumerable<Fotografia> fotografie ) {

			List<Slide> slides = new List<Slide>();
			foreach( Fotografia ff in fotografie ) {

				if( ff.imgProvino == null )
					AiutanteFoto.idrataImmaginiFoto( IdrataTarget.Provino, ff);

				Slide slide = new Slide();
				slide.imgProvino = ((ImmagineWic)ff.imgProvino).bitmapSource;
				slide.etichetta = (string)ff.etichetta;
				slides.Add( slide );
			}
			create( slides );
		}

		/// <summary>
		/// creo uno slide show con un elenco di immagini qualsiasi
		/// </summary>
		public void create( List<Slide> slides )  {

			SlideShow slideShow = new SlideShow();
			slideShow.colonne = 3;
			slideShow.righe = 2;
			slideShow.millisecondiIntervallo = 3800;
			slideShow.itemsShow = slides;
			this.slideShow = slideShow;

			creaNuovoTimer();
		}

		private void creaNuovoTimer() {

			// Se avevo un timer precedete, lo distruggo. Poi ne creo un altro.
			if( _orologio != null )
				_orologio.Stop();

			_orologio = new DispatcherTimer();
			_orologio.Interval = new TimeSpan( 0,0,0,0, slideShow.millisecondiIntervallo );
			_orologio.Tick += new EventHandler( orologio_Tick );
		}

		#endregion

			
		private void orologio_Tick (object sender, EventArgs e) {

			// carico la collezione delle slide visibili andando avanti di una pagina

			if( slidesVisibili == null )
				slidesVisibili = new ObservableCollection<Slide>();
			else
				slidesVisibili.Clear();

			int conta = 0;

			do {

				// Se arrivo al massimo, torno all'inizio
				if( numSlideCorrente >= slideShow.itemsShow.Count )
					numSlideCorrente = 0;

				if( numSlideCorrente < slideShow.itemsShow.Count )
					this.slidesVisibili.Add( slideShow.itemsShow [numSlideCorrente++] );
				else
					break;   // si vede che la lista è vuota lunga zero.

				++conta;

				// esco se ho finito la pagina, oppure se ho finito le foto
			} while( conta < totSlidesPerPagina && conta < slideShow.itemsShow.Count );

			OnPropertyChanged( "slidesVisibili" );


			// Dopo che ho visualizzato le foto, se mi accorgo che il numero totale di foto da visualizzare 
			// è inferiore al numero massimo di foto che stanno nello show,
			// allora è inutile che lascio il timer acceso, tanto non ho altro da mostrare.
			if( slideShow.itemsShow.Count <= totSlidesPerPagina )
				stop();
		}
	}
}
