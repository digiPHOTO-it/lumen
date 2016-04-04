using log4net;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Servizi.Stampare;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Printing;
using System.Windows.Markup;
using System.Text;
using Digiphoto.Lumen.Model;
using System.IO;
using Digiphoto.Lumen.Config;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections;
using Digiphoto.Lumen.Util;
using System.Globalization;
using Digiphoto.Lumen.Imaging.Wic.Documents;

namespace Digiphoto.Lumen.Imaging.Wic.Stampe {

	public class EsecutoreStampaProvini : IEsecutoreStampa {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EsecutoreStampaWic ) );

		private EsitoStampa _esito;

		private long _conta = 0;

		public EsecutoreStampaProvini() {
		}

		/**
		 * Attenzione:
		 * questo metodo deve ritornare l'esito della stampa, quindi non deve essere asincrono.
		 * Deve essere sicronizzato
		 */
		public EsitoStampa esegui( LavoroDiStampa lavoroDiStampa ) {

			LavoroDiStampaProvini _lavoroDiStampa = lavoroDiStampa as LavoroDiStampaProvini;

			_giornale.Debug( "Sto per avviare il lavoro di stampa: " + lavoroDiStampa.ToString() );

			_conta++;

			try {
				// Come print-server uso me stesso
				using( PrintServer ps1 = new PrintServer() ) {

					// Ricavo la coda di stampa (cioè la stampante) e le sue capacità.
					using( PrintQueue coda = ps1.GetPrintQueue( lavoroDiStampa.param.nomeStampante ) ) {

						PrintCapabilities capabilities = coda.GetPrintCapabilities();

						// Imposto la stampante (così che mi carica le impostazioni)
						PrintDialog dialog = new PrintDialog();
						dialog.PrintQueue = coda;

						Size areaStampabile = new Size( dialog.PrintableAreaWidth, dialog.PrintableAreaHeight );

						// Imposto qualche attributo della stampa
						bool piuRealistaDelRe = false;
						if( piuRealistaDelRe ) { // Meglio non essere più realisti del re.
							dialog.PrintTicket.OutputQuality = OutputQuality.Photographic;
							dialog.PrintTicket.PhotoPrintingIntent = PhotoPrintingIntent.PhotoBest;
						}

						// Compongo il titolo della stampa che comparirà nella descrizione della riga nello spooler di windows
						StringBuilder titolo = new StringBuilder();
						titolo.AppendFormat( "Intestazione={0} Righe={1} Colonne={2}",
							_lavoroDiStampa.param.intestazione+" "+Configurazione.infoFissa.idPuntoVendita,
							_lavoroDiStampa.param.numeroRighe,
							_lavoroDiStampa.param.numeroColonne
							);

						if( _giornale.IsDebugEnabled ) {
							titolo.Append( " #" );
							titolo.Append( _conta );
						}

						// Eventuale rotazione dell'orientamento dell'area di stampa
						// Devo decidere in anticipo se la stampante va girata. Dopo che ho chiamato Print non si può più fare !!!
						bool _ruotareStampante = false;

						if( _ruotareStampante ) {

							if( capabilities.PageOrientationCapability.Contains( PageOrientation.Landscape ) && capabilities.PageOrientationCapability.Contains( PageOrientation.Portrait ) ) {
								// tutto ok
								dialog.PrintTicket.PageOrientation = (dialog.PrintTicket.PageOrientation == PageOrientation.Landscape ? PageOrientation.Portrait : PageOrientation.Landscape);
							} else
								_giornale.Warn( "La stampante " + lavoroDiStampa.param.nomeStampante + " non accetta cambio orientamento landscape/portrait" );

							// Quando giro la stampante, non mi si girano anche le dimensioni. Ci penso da solo.
							areaStampabile = ProiettoreArea.ruota( areaStampabile );
						}

						//
						// ----- gestisco il numero di copie
						//
						int cicliStampa = 1;
						if( lavoroDiStampa.param.numCopie > 1 ) {
							// Se la stampante gestisce le copie multiple, faccio un invio solo.
							if( capabilities.MaxCopyCount >= lavoroDiStampa.param.numCopie )
								dialog.PrintTicket.CopyCount = lavoroDiStampa.param.numCopie;
							else
								cicliStampa = lavoroDiStampa.param.numCopie;
						}


						// Ora creo il documento che andrò a stampare.
						using( ProviniDocPaginator documentPaginator = new ProviniDocPaginator( _lavoroDiStampa, areaStampabile ) ) {

							//
							// ----- STAMPA per davvero
							//
							for( int ciclo = 0; ciclo < cicliStampa; ciclo++ ) {
								dialog.PrintDocument( documentPaginator, titolo.ToString() );
								_esito = EsitoStampa.Ok;
							}

							_giornale.Debug( "Stampa completata" );
						}
					}
				}
			} catch( Exception ee ) {
				_esito = EsitoStampa.Errore;
				_giornale.Error( "Stampa fallita", ee );
			}
		
			_giornale.Info( "Completato lavoro di stampa. Esito = " + _esito + " lavoro = " + lavoroDiStampa.ToString() );
			return _esito;
		}



		/**
		 * Se la foto e la stampante non sono orientate nello stesso verso,
		 * allora devo uniformarle.
		 */
		private static bool determinaRotazione( Size areaStampabile, IImmagine immagineDaStampare ) {

			bool _ruotareStampante = false;

			// Entrambe orizzontali.
			// Entrambe verticali
			if( !ProiettoreArea.isStessoOrientamento( areaStampabile, immagineDaStampare ) ) {

				// Ok sono dissimili.
				_ruotareStampante = true;

			}
			return _ruotareStampante;
		}



		public bool asincrono {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}


		public Type tipoParamGestito {
			get {
				return typeof(ParamStampaProvini);
			}
		}
	}
}
