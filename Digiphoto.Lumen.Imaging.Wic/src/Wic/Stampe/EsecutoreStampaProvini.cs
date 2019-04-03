using log4net;
using Digiphoto.Lumen.Servizi.Stampare;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Printing;
using System.Text;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Imaging.Wic.Documents;
using System.Text.RegularExpressions;

namespace Digiphoto.Lumen.Imaging.Wic.Stampe {

	public class EsecutoreStampaProvini : IEsecutoreStampa {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( EsecutoreStampaProvini ) );

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
                var match = Regex.Match(lavoroDiStampa.param.nomeStampante, @"(?<machine>\\\\.*?)\\(?<queue>.*)");
                PrintServer ps1 = null;
                if (match.Success)
                {
                    // Come print-server uso il server di rete
                    ps1 = new PrintServer(match.Groups["machine"].Value);
                }
                else
                {
                    // Come print-server uso me stesso
                    ps1 = new PrintServer();
                }
                using ( ps1) {
                    PrintQueue coda = null;
                    if (match.Success)
                    {
                        coda = ps1.GetPrintQueue(match.Groups["queue"].Value);
                    }
                    else
                    {
                        coda = ps1.GetPrintQueue(lavoroDiStampa.param.nomeStampante);
                    }

                    // Ricavo la coda di stampa (cioè la stampante) e le sue capacità.
                    using (coda ) {

						PrintCapabilities capabilities = null;
						try {
							capabilities = coda.GetPrintCapabilities();
						} catch( Exception ) {
							// Le stampanti shinko non supportano queste capabilities
						}

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

							if( capabilities != null && capabilities.PageOrientationCapability.Contains( PageOrientation.Landscape ) && capabilities.PageOrientationCapability.Contains( PageOrientation.Portrait ) ) {
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
							if( capabilities != null && capabilities.MaxCopyCount >= lavoroDiStampa.param.numCopie )
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
