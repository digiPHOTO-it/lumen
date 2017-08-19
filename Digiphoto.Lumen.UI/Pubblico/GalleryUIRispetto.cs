using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.UI.Util;
using Digiphoto.Lumen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Digiphoto.Lumen.UI.Pubblico {
	

	/// <summary>
	/// Questa classe implementa la funzione di ricoprire l'immagine con due fasce opache,
	/// che rimarca l'area di stampa.
	/// In pratica evidenzia i due ritagli che verranno effettuati nel momento della stampa.
	/// Il ratio di default della immagine viene preso dalla configurazione
	/// 
	/// Viene usata sia nella gallery (FotoGallery.xaml.cs) che nella finestra del pubblico (Pubblico.xaml.cs)
	/// 
	///	</summary>
	/// 
	public class GalleryUIRispetto : IDisposable {

		/// <summary>
		/// Questo è il viemodel che pilota entrambe le finestre (gallery e pubblico)
		/// </summary>
		FotoGalleryViewModel fotoGalleryViewModel { 
			get {
				return this.contentControl == null ? null : (FotoGalleryViewModel) contentControl.DataContext;
			}
		}

		/// <summary>
		/// Questo l'elemento grafico che contiene ed itera la collezione delle foto.
		/// (Nella Gallery è un ListBox, mentre nel Pubblico è un ItemsControl)
		/// </summary>
		ItemsControl itemsControl;

		/// <summary>
		/// Questo è l'elemento grafico che contiene tutto.
		/// (nella Gallery è uno UserControl mentre nel pubblico è una Window completa)
		/// </summary>
		ContentControl contentControl;

		String nomeComponenteGrid { get; set; }
		String nomeComponenteImmagine { get; set; }

		/// <summary>
		/// Il rapporto dell'area di rispetto è data da una frazione indicata in configurazione (es. 4/3 o 3/2)
		/// E' la stessa che viene usata per imprimere l'area stampabile sui provini (riga rossa tratteggiata)
		/// </summary>
		static float ratioRispetto = (float)CoreUtil.evaluateExpression( Configurazione.UserConfigLumen.expRatioAreaDiRispetto );

		public GalleryUIRispetto( ItemsControl itemsControl, ContentControl contentControl ) {

			this.itemsControl = itemsControl;
			this.contentControl = contentControl;
			this.nomeComponenteGrid = "fotoGrid";
			this.nomeComponenteImmagine = "fotoImage";
		}


		void dimensionaRettangoloPerAreaDiRispetto( Rectangle[] rettangoli, Grid fotoGrid ) {

			try {
				Rectangle rectA = rettangoli[0];
				Rectangle rectB = rettangoli[1];

				// Ora ricalcolo la dimensione dell'area di rispetto
				// float ratio = fotoGalleryViewModel.ratioAreaStampabile;
				if( fotoGalleryViewModel.ratioAreaStampabile == 0f )
					return;

				CalcolatoreAreeRispetto.Geo imageGeo = new CalcolatoreAreeRispetto.Geo();

				Image fotoImage = (Image) fotoGrid.FindName( nomeComponenteImmagine );
				imageGeo.w = fotoImage.ActualWidth;
				imageGeo.h = fotoImage.ActualHeight;


				// Calcolo la fascia A
				Rect rettangoloA = CalcolatoreAreeRispetto.calcolaDimensioni( CalcolatoreAreeRispetto.Fascia.FasciaA, ratioRispetto, imageGeo );
				CalcolatoreAreeRispetto.Bordi bordiA = CalcolatoreAreeRispetto.calcolcaLatiBordo( CalcolatoreAreeRispetto.Fascia.FasciaA, ratioRispetto, imageGeo, imageGeo );

				// Calcolo la fascia B
				Rect rettangoloB = CalcolatoreAreeRispetto.calcolaDimensioni( CalcolatoreAreeRispetto.Fascia.FasciaB, ratioRispetto, imageGeo );
				CalcolatoreAreeRispetto.Bordi bordiB = CalcolatoreAreeRispetto.calcolcaLatiBordo( CalcolatoreAreeRispetto.Fascia.FasciaB, ratioRispetto, imageGeo, imageGeo );

				// Calcolo left e top in base alla posizione della immagine rispetto alla grid che la contiene
				var qq = fotoImage.TransformToAncestor( fotoGrid );
				Point relativePoint = qq.Transform( new Point( 0, 0 ) );
				double currentLeft = relativePoint.X;
				double currentTop = relativePoint.Y;

				// Setto fascia A
				rectA.Width = rettangoloA.Width;
				rectA.Height = rettangoloA.Height;
				var left = currentLeft + rettangoloA.Left;
				var top = currentTop + rettangoloA.Top;
				var right = 0;
				var bottom = 0;

				Thickness ticA = new Thickness( left, top, right, bottom );
				rectA.Margin = ticA;

				// ---

				// Setto fascia B
				rectB.Width = rettangoloB.Width;
				rectB.Height = rettangoloB.Height;
				left = currentLeft + rettangoloB.Left;
				top = currentTop + rettangoloB.Top;
				right = 0;
				bottom = 0;

				Thickness ticB = new Thickness( left, top, right, bottom );
				rectB.Margin = ticB;

			} catch( Exception ee ) {
				// pazienza : dovrei loggare l'errore
				int a = 3;
			}
		}


		private void fotoGalleryViewModel_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {

			bool cambiaAreaStampabile = false;

			// Significa che ho cambiato le impostazioni di visualizzazione
			if( e.PropertyName == "devoVisualizzareAreaDiRispettoHQ" )
				cambiaAreaStampabile = true;

			if( e.PropertyName == "vorreiVisualizzareAreaDiRispettoHQ" )
				cambiaAreaStampabile = true;

			if( fotoGalleryViewModel.isAltaQualita ) {
				// Significa che mi sono spostato alla foto successiva in HQ
				if( e.PropertyName == "fotografieCW" ||
					e.PropertyName == "numRighe" || e.PropertyName == "numColonne" ||
					e.PropertyName == "numRighePag" || e.PropertyName == "numColonnePag" )
					cambiaAreaStampabile = true;
			}

			if( cambiaAreaStampabile ) {
				// Devo farlo nel thread della UI altrimenti non si sono ancora disposti i componenti grafici correttamente
				contentControl.Dispatcher.BeginInvoke( gestioneAreaStampabileHQAction );
			}

		}

		Action _gestioneAreaStampabileHQAction;
		Action gestioneAreaStampabileHQAction
		{
			get
			{
				if( _gestioneAreaStampabileHQAction == null )
					_gestioneAreaStampabileHQAction = new Action( gestioneAreaStampabileHQ );
				return _gestioneAreaStampabileHQAction;
			}
		}


		public void gestioneAreaStampabileHQ() {
			gestioneAreaStampabileHQ( false );
		}

		// Questo è lo style per decorare i rettangoli
		Style coperturaRispettoStyle;

		public void gestioneAreaStampabileHQ( bool spegniForzatamente ) {

			// Se non so il ratio dell'area stampabile, esco subito
			if( fotoGalleryViewModel.ratioAreaStampabile == 0f )
				return;

			if( fotoGalleryViewModel.fotografieCW == null )
				return;

			if( spegniForzatamente == false && fotoGalleryViewModel.vorreiVisualizzareAreaDiRispettoHQ == false )
				return;

			if( spegniForzatamente == false && fotoGalleryViewModel.isAltaQualita ) {

				if( fotoGalleryViewModel.fotografieCW.Count > 2 )
					return;

				itemsControl.UpdateLayout();


				if( coperturaRispettoStyle == null )
					coperturaRispettoStyle = contentControl.FindResource( "coperturaRispettoStyle" ) as Style;

				// Le foto in alta qualità possono essere 1 oppure 2 affiancate
				foreach( Fotografia foto in fotoGalleryViewModel.fotografieCW ) {

					Rectangle[] rettangoli = new Rectangle[2];
					bool esiste = false;

					Grid fotoGrid = findFotoGrid( foto );

					for( char ab = 'A'; ab <= 'B'; ab++ ) {

						// -- areaStampabileA - areaStampabileB
						int pos = ab - 'A';
						string nome = string.Format( "areaStampabile{0}", ab );

						rettangoli[pos] = (Rectangle)AiutanteUI.FindChild( fotoGrid, nome, typeof( Rectangle ) );

						esiste = rettangoli[pos] != null;
						if( !esiste ) {
							rettangoli[pos] = new Rectangle();
							rettangoli[pos].Name = nome;
							rettangoli[pos].Style = coperturaRispettoStyle;
						}
					}

					dimensionaRettangoloPerAreaDiRispetto( rettangoli, fotoGrid );


					if( !esiste ) {
						fotoGrid.Children.Add( rettangoli[0] );
						fotoGrid.Children.Add( rettangoli[1] );
					}
				}

			} else {

				// Elimino tutti i rettangoli. Possono essere 2 o 4
				foreach( Fotografia foto in fotoGalleryViewModel.fotografieCW ) {
					Grid fotoGrid = findFotoGrid( foto );
					for( char ab = 'A'; ab <= 'B'; ab++ ) {

						// -- areaStampabileA - areaStampabileB
						int pos = ab - 'A';
						string nome = string.Format( "areaStampabile{0}", ab );
						Rectangle rettangolo = (Rectangle)AiutanteUI.FindChild( fotoGrid, nome, typeof( Rectangle ) );
						if( rettangolo != null )
							fotoGrid.Children.Remove( rettangolo );
					}
				}
			}
		}


		/// <summary>
		/// Questa grid viene creata a runtime per ogni foto che viene iterata dalla listbox.
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		Grid findFotoGrid( Fotografia f ) {
			return findComponentFromTemplate<Grid>( f, nomeComponenteGrid );
		}

		Image findFotoImage( Fotografia f ) {
			return findComponentFromTemplate<Image>( f, nomeComponenteImmagine );
		}

		T findComponentFromTemplate<T>( Fotografia f, string nomeComponente ) {

			// Per ricavare il componente desiderato, devo fare diversi passaggi

			// 2. dalla foto ricavo il ListBoxItem che la contiene
			ContentPresenter contentPresenter;
			var test = itemsControl.ItemContainerGenerator.ContainerFromItem( f );
			if( test is ContentControl ) {
				// 3. dal ListBoxItem ricavo il suo ContentPresenter
				ContentControl listBoxItem = (ContentControl)test;
				contentPresenter = AiutanteUI.FindVisualChild<ContentPresenter>( listBoxItem );
			} else {
				// IL componente è già avvolto direttamente nel presenter
				contentPresenter = (ContentPresenter)test;
			}

			// 4. con il ContentPresenter ricavo il DataTemplate (del singolo elemento)
			DataTemplate dataTemplate = contentPresenter.ContentTemplate;
			// 5. con il DataTemplate ricavo l'Image contenuta

			return (T)dataTemplate.FindName( nomeComponente, contentPresenter );
		}

		public void ascolta( bool sino ) {

			// Abilito / Disabilito l'ascolto dei cambi di property
			if( sino == true ) {
				// Metto un ascoltatore su tutte le property. perché devo sentire i cambi della AltaQualità
				fotoGalleryViewModel.PropertyChanged += fotoGalleryViewModel_PropertyChanged;
			} else {
				fotoGalleryViewModel.PropertyChanged -= fotoGalleryViewModel_PropertyChanged;
			}
		}

		public void Dispose() {
			
			// Forzo eliminazione fasce
			gestioneAreaStampabileHQ( true );
			
			// Smetto di ascoltare i cambiamenti delle propery della gallery
			this.ascolta( false );

			// riascio i reference agli oggetti di interesse altrimenti rischio di creare una dipendenza circolare e il GC non libera più memoria
			contentControl = null;
			itemsControl = null;

		}
	}
}
