using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Servizi.Ritoccare;
using Digiphoto.Lumen.Windows.Media.Effects;

namespace Digiphoto.Lumen.Imaging.Wic.Correzioni {

	internal class RigaCache {
		public Correttore correttore { set; get; }
		public Type implementazione { set; get; }
	}

	internal class CorrettoreFactory : ICorrettoreFactory {

		Dictionary<Type, Correttore> _cache;

		public CorrettoreFactory() {
			_cache = new Dictionary<Type, Correttore>();
		}


		public Correttore creaCorrettore<T>() where T : Correzione {
			return creaCorrettore( typeof( T ) );
		}

		public Correttore creaCorrettore( Type tipo ) {

			Correttore correttore = null;

			// Prima controllo in cache
			if( _cache.ContainsKey( tipo ) ) {
				correttore = _cache[tipo];
			} else {

				// In base al tipo di correzione, istanzio il giusto correttore
				// In base al tipo di correzione, istanzio il giusto correttore
				if( tipo == typeof( BiancoNero ) )
					correttore = new BiancoNeroCorrettore();
				else if( tipo == typeof( Resize ) )
					correttore = new ResizeCorrettore();
				else if( tipo == typeof( Sepia ) )
					correttore = new SepiaCorrettore();
				else if( tipo == typeof( Ruota ) )
					correttore = new RuotaCorrettore();
				else if( tipo == typeof( Specchio ) )
					correttore = new SpecchioCorrettore();
				else if( tipo == typeof( Luce ) )
					correttore = new LuminositaContrastoCorrettore();
				else if( tipo == typeof( Crop ) )
					correttore = new CropCorrettore();
				else if( tipo == typeof( Gimp ) )
					correttore = new GimpCorrettore();
				else if( tipo == typeof( Dominante ) )
					correttore = new DominantiCorrettore();
				else if( tipo == typeof( Zoom ) )
					correttore = new ZoomCorrettore();
				else if( tipo == typeof( Trasla ) )
					correttore = new TraslaCorrettore();
				else if( tipo == typeof( Maschera ) )
					correttore = new MascheraCorrettore();
				else if( tipo == typeof( Logo ) )
					correttore = new LogoCorrettore();
				else if( tipo == typeof( AreaRispetto ) )
					correttore = new AreaRispettoCorrettore();

				if( correttore != null ) {
					_cache.Add( tipo, correttore );  // Metto in cache
				} else {
					throw new NotSupportedException( "tipo correzione = " + tipo );
				}
			}

			return correttore;
		}


		private Type calcolaTipoCorrispondente( TipoCorrezione tipoCorrezione ) {

			Type tipo = null;

			switch( tipoCorrezione ) {

				case TipoCorrezione.BiancoNero:
					tipo = typeof(BiancoNero);
					break;
				case TipoCorrezione.Ridimensiona:
					tipo = typeof(Resize);
					break;
				case TipoCorrezione.Sepia:
					tipo = typeof(Sepia);
					break;
				case TipoCorrezione.Ruota:
					tipo = typeof(Ruota);
					break;
				case TipoCorrezione.Specchio:
					tipo = typeof(Specchio);
					break;
				case TipoCorrezione.Luce:
					tipo = typeof(Luce);
					break;
//					case TipoCorrezione.Crop:
//						tipo = typeof(CropCorrettore);
//						break;
 				case TipoCorrezione.Gimp:
					tipo = typeof(Gimp);
					break;
				case TipoCorrezione.Dominante:
					tipo = typeof(Dominante);
					break;
				case TipoCorrezione.Zoom:
					tipo = typeof( Zoom );
					break;
				case TipoCorrezione.Trasla:
					tipo = typeof( Trasla );
					break;
				default:
					throw new NotSupportedException( "tipo correzione = " + tipoCorrezione );
			}

			return tipo;
		}

		public Correttore creaCorrettore( TipoCorrezione tipoCorrezione ) {
			Type typeCorrezione = calcolaTipoCorrispondente( tipoCorrezione );
			return creaCorrettore( typeCorrezione );
		}
	}
}
