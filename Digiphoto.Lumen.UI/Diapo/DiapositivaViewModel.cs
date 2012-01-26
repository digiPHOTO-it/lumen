using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Imaging;
using System.Windows.Media;
using Digiphoto.Lumen.Imaging.Wic;
using System.Windows.Media.Imaging;

namespace Digiphoto.Lumen.UI.Diapo {

	public class DiapositivaViewModel : ViewModelBase {

		public DiapositivaViewModel() {
		}

		public DiapositivaViewModel( Fotografia foto ) : base() {
			this.fotografia = foto;
		}

		#region Proprietà

		private Fotografia _fotografia;
		public Fotografia fotografia {
			get {
				return _fotografia;
			}
			set {

				if( value != _fotografia ) {
					_fotografia = value;
					OnPropertyChanged( "fotografia" );
				}
			}
		}

		public string idFoto {
			get {
				if( IsInDesignMode )
					return "AB-123";
				else {
					if( fotografia == null )
						return null;
					else
						return fotografia.fotografo.iniziali + "-" + fotografia.numero.ToString();
				}
			}
		}

		public ImageSource imageSourceProvino {

			get {
				ImageSource ret = null;

				if( IsInDesignMode ) {

					ret = new BitmapImage( new Uri( "/Resources/cani_e_bimbi.jpg", UriKind.Absolute ) );

					// TODO si potrebbe caricare una immagine fissa dalle risorse del progetto
				} else {

					if( this.fotografia != null ) {
						IImmagine immagineProvino = fotografia.imgProvino;
						if( immagineProvino != null ) {
							ret = ((ImmagineWic)immagineProvino).bitmapSource;
						}
					}
				}
				return ret;
			}
		}

		#endregion


		#region Metodi
		#endregion

		#region Comandi
		#endregion
	}
}
