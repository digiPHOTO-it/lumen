using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using log4net;
using Digiphoto.Lumen.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Digiphoto.Lumen.Imaging.Nativa {


	public class ProvinatoreNet : Provinatore, IProvinatore {

		enum AnchorPosition {
			Top,
			Left,
			Bottom,
			Right,
			Center
		}



		private static readonly ILog _giornale = LogManager.GetLogger( typeof( ProvinatoreNet ) );

		public ProvinatoreNet() {
		}


		//public override IImmagine creaImmagine( string nomeFile ) {
		//    return new ImmagineNet( Image.FromFile( nomeFile ) );
		//}

		public override IImmagine creaProvino( IImmagine immagineGrande ) {

			this.immagine = immagineGrande;

			// Valorizza le due proprietà che mi servono per lanciare la Resize
			calcolaEsattaWeH();

			Image imageGrande = ((ImmagineNet)immagineGrande).image;
			
			Image imagePiccola = ScaleSimple( imageGrande, calcW, calcH );

			return new ImmagineNet( imagePiccola );
		}

		/**
		 * Questo è il modo più semplice ed efficace che ho trovato
		 */
		static Image ScaleSimple( Image imgPhoto, int Width, int Height ) {
			return new Bitmap( imgPhoto, Width, Height );
		}


		/**
		 * Scala l'immagine alla dimensione richiesta.
		 * Per creare una thumbnail esiste il metodo apposito della classe Image.GetThumbnailImage
		 * peccato che i risultati siano schifosi.
		 * Utilizzo questo sistema che garantisce un buon algoritmo bucubico di interpolazione.
		 * 
		 * 
		 * ATTENZIONE: 
		 * nonostante sembra scritto tutto bene, mi rimangono due righe rosse larghe 1 pixel,
		 * nella prima riga orizzontale e verticale.
		 * Se vuoi usare questo metodo, correggere prima questo problema.
		 */
		static Image Scale( Image imgPhoto, int Width, int Height ) {

			Bitmap bmPhoto = new Bitmap( Width, Height, PixelFormat.Format24bppRgb );

			bmPhoto.SetResolution( imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution );

			Graphics grPhoto = Graphics.FromImage( bmPhoto );
			grPhoto.Clear( Color.Red );    // TODO non serve. Eliminare o sostituire con il bianco. Per me non serve
			grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;  // Ottimo per ridurre la foto

			grPhoto.DrawImage( imgPhoto, new Rectangle( 0, 0, Width, Height ) );

			grPhoto.Dispose();
			return bmPhoto;
		}
		
		/**
		 * Questo metodo, si può usare per fare delle resize con approssimazione.
		 * I valori float usati internamente, provocano delle piccole differenze
		 * nel centramento della foto all'interno della sua area di bitmap.
		 */
		static Image ScaleToFixedSize( Image imgPhoto, int Width, int Height ) {

			int sourceWidth = imgPhoto.Width;
			int sourceHeight = imgPhoto.Height;
			int sourceX = 0;
			int sourceY = 0;
			int destX = 0;
			int destY = 0;

			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;

			nPercentW = ((float)Width / (float)sourceWidth);
			nPercentH = ((float)Height / (float)sourceHeight);
			if( nPercentH < nPercentW ) {
				nPercent = nPercentH;
				// Invece del cast a intero prima c'era   System.Convert.ToInt16
				destX = (int) ( (Width -  (sourceWidth * nPercent)) / 2 );
			} else {
				nPercent = nPercentW;
				// Invece del cast a intero prima c'era   System.Convert.ToInt16
				destY = (int)((Height - (sourceHeight * nPercent)) / 2);
			}

			int destWidth = (int)(sourceWidth * nPercent);
			int destHeight = (int)(sourceHeight * nPercent);

			Bitmap bmPhoto = new Bitmap( Width, Height,
									PixelFormat.Format24bppRgb );
			bmPhoto.SetResolution( imgPhoto.HorizontalResolution,
								  imgPhoto.VerticalResolution );

			Graphics grPhoto = Graphics.FromImage( bmPhoto );
			grPhoto.Clear( Color.Red );
			grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

			grPhoto.DrawImage( imgPhoto,
				 new Rectangle( destX, destY, destWidth, destHeight ),
				 new Rectangle( sourceX, sourceY, sourceWidth, sourceHeight ),
				 GraphicsUnit.Pixel );

			grPhoto.Dispose();
			return bmPhoto;
		}


		static Image ScaleByPercent( Image imgPhoto, int Percent ) {
			float nPercent = ((float)Percent / 100);

			int sourceWidth = imgPhoto.Width;
			int sourceHeight = imgPhoto.Height;
			int sourceX = 0;
			int sourceY = 0;

			int destX = 0;
			int destY = 0;
			int destWidth = (int)(sourceWidth * nPercent);
			int destHeight = (int)(sourceHeight * nPercent);

			Bitmap bmPhoto = new Bitmap( destWidth, destHeight,
											 PixelFormat.Format24bppRgb );
			bmPhoto.SetResolution( imgPhoto.HorizontalResolution,
											imgPhoto.VerticalResolution );

			Graphics grPhoto = Graphics.FromImage( bmPhoto );
			grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

			grPhoto.DrawImage( imgPhoto,
				 new Rectangle( destX, destY, destWidth, destHeight ),
				 new Rectangle( sourceX, sourceY, sourceWidth, sourceHeight ),
				 GraphicsUnit.Pixel );

			grPhoto.Dispose();
			return bmPhoto;
		}

		static Image Crop( Image imgPhoto, int Width,
						  int Height, AnchorPosition Anchor ) {
			int sourceWidth = imgPhoto.Width;
			int sourceHeight = imgPhoto.Height;
			int sourceX = 0;
			int sourceY = 0;
			int destX = 0;
			int destY = 0;

			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;

			nPercentW = ((float)Width / (float)sourceWidth);
			nPercentH = ((float)Height / (float)sourceHeight);

			if( nPercentH < nPercentW ) {
				nPercent = nPercentW;
				switch( Anchor ) {
					case AnchorPosition.Top:
						destY = 0;
						break;
					case AnchorPosition.Bottom:
						destY = (int)
							 (Height - (sourceHeight * nPercent));
						break;
					default:
						destY = (int)
							 ((Height - (sourceHeight * nPercent)) / 2);
						break;
				}
			} else {
				nPercent = nPercentH;
				switch( Anchor ) {
					case AnchorPosition.Left:
						destX = 0;
						break;
					case AnchorPosition.Right:
						destX = (int)
						  (Width - (sourceWidth * nPercent));
						break;
					default:
						destX = (int)
						  ((Width - (sourceWidth * nPercent)) / 2);
						break;
				}
			}

			int destWidth = (int)(sourceWidth * nPercent);
			int destHeight = (int)(sourceHeight * nPercent);

			Bitmap bmPhoto = new Bitmap( Width,
					  Height, PixelFormat.Format24bppRgb );
			bmPhoto.SetResolution( imgPhoto.HorizontalResolution,
					  imgPhoto.VerticalResolution );

			Graphics grPhoto = Graphics.FromImage( bmPhoto );
			grPhoto.InterpolationMode =
					  InterpolationMode.HighQualityBicubic;

			grPhoto.DrawImage( imgPhoto,
				 new Rectangle( destX, destY, destWidth, destHeight ),
				 new Rectangle( sourceX, sourceY, sourceWidth, sourceHeight ),
				 GraphicsUnit.Pixel );

			grPhoto.Dispose();
			return bmPhoto;
		}
	}
}
