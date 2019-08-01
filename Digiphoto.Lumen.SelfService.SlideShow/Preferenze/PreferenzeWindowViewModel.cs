


using Digiphoto.Lumen.SelfService.SlideShow.Config;
using Digiphoto.Lumen.SelfService.SlideShow.SelfServiceReference;
using Digiphoto.Lumen.SelfService.SlideShow.Servizi;

namespace Digiphoto.Lumen.SelfService.SlideShow.Preferenze {

	public class PreferenzeWindowViewModel {

		public UserConfig userConfig {
			private set;
			get;
		}
		public FotografoDto[] fotografiDto { get; }

		public PreferenzeWindowViewModel( UserConfig userConfig ) {
			this.userConfig = userConfig;

			fotografiDto = SSClientSingleton.Instance.getListaFotografi();
		}


	}
}
