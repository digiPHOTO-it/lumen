using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.Cli {

	class Program : IObserver<Messaggio> {

		Program() {
		}

		private void avvia() {

			LumenApplication app = LumenApplication.Instance;

			app.avvia();

			
			IObservable<Messaggio> observable = app.bus.Observe<Messaggio>();
			observable.Subscribe( this );

			attendiComandi();

			app.ferma();
			
		}

		private void attendiComandi() {

			IDictionary<char,string> voci = new Dictionary<char,string>();
			voci.Add( 'B', "Battezza Flash Card" );
			voci.Add( 'S', "Scarica Foto" );
			
			char scelta = eseguiMenu( voci );

			switch( scelta ) {
				case 'Q':
					return;
				case 'B':
					battezzaFlashCard();
					break;

				case 'S':
					scaricaFoto();
					break;
			}
		}

		private void battezzaFlashCard() {
			
			IScaricatoreFotoSrv s = LumenApplication.Instance.creaScaricatoreFotoSrv();
			
			ParamScarica param = s.ultimaChiavettaInserita;
			// TODO da fare
			s.battezzaFlashCard( param );

		}

		private void scaricaFoto() {
			IScaricatoreFotoSrv srv;
			// TODO
		}
		
		char eseguiMenu( IDictionary<char, string> voci ) {

			foreach( char key in voci.Keys ) {
				Console.WriteLine( key + ")  =  " + voci[key] );
			}
			Console.Write( "\n\n Q)  =  Quit" );
			Console.Write( "\t--> Scelta ..." );
			
			return Console.ReadLine().ToUpper().ElementAt( 0 );
		}



		public static void Main( string [] args ) {

			Program p = new Program();
			p.avvia();
		}


		public void OnCompleted() {
			throw new NotImplementedException();
		}

		public void OnError( Exception error ) {
			throw new NotImplementedException();
		}

		public void OnNext( Messaggio value ) {

			if( value is VolumeCambiatoMessaggio ) {
				VolumeCambiatoMessaggio vcm = (VolumeCambiatoMessaggio)value;
				if( vcm.montato ) {
					Console.WriteLine( "\n++ montato il volume : " + vcm.nomeVolume );
				} else {
					Console.WriteLine( "\n-- Disabilitato il volume : " + vcm.nomeVolume );
				}
			}
		}

	}
}
