using System;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Applicazione;
using log4net;


/**
 * Questa classe si preoccupa di gestire i numeratori (le sequenze)
 * dei fotogrammi.
 * Gestisce anche l'azzeramento
 */
namespace Digiphoto.Lumen.Servizi.Scaricatore {

	 internal class NumeratoreFotogrammi {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( NumeratoreFotogrammi ) );


		internal static int incrementaNumeratoreFoto( int quante ) {
	
			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			InfoFissa infoFissa = objContext.InfosFisse.Single<InfoFissa>( f => f.id == "K" );
			
			int ultimoNum = infoFissa.ultimoNumFotogramma;

			if( ultimoNum > 0 )
				ultimoNum = eventualeAzzeramento( infoFissa.modoNumerazione, infoFissa.ultimoNumFotogramma, infoFissa.dataUltimoScarico );

			// Aggiorno sempre e comunque
			infoFissa.ultimoNumFotogramma = ultimoNum + quante;
			infoFissa.dataUltimoScarico = DateTime.Today;

			objContext.SaveChanges();

			return ultimoNum;
		}



		/**
		 * Se è passato il tempo stabilito,
		 * azzero il contatore dei fotogrammi. Altrimenti lo incremento
		 * del valore richiesto.
		 * Il risultato devo scriverlo nel db perché deve essere visibile da tutti
		 * gli utenti della rete.
		 */
		private static int eventualeAzzeramento( string modoAzzeramento, int ultimoFotogramma, DateTime? dataUltimoScarico ) {

			int numero = ultimoFotogramma;

			if( dataUltimoScarico != null ) {

				TimeSpan diff = LumenApplication.Instance.stato.giornataLavorativa - dataUltimoScarico.Value;

				switch( modoAzzeramento [0] ) {

					case 'G':    // giornaliento
						if( diff.Days > 0 )
							numero = 0;
						break;

					case 'S':   // settimanale
						if( diff.Days > 7 )
							numero = 0;
						break;

					case 'M':   // mai (non faccio nulla)
						break;

					default:
						_giornale.Error( "Modo azzerameto numeratore fotogrammi non gestito: " + modoAzzeramento );
						throw new NotSupportedException( "modo azzeramento non gestito" );
				}
			}
			return numero;
		}
	}
}
