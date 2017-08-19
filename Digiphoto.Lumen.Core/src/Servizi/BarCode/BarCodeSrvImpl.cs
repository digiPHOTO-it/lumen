using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Util;
using log4net;
using System.ComponentModel;
using Digiphoto.Lumen.Servizi.Ricerca;

namespace Digiphoto.Lumen.Servizi.BarCode
{
	public class BarCodeSrvImpl : ServizioImpl, IBarCodeSrv
	{
		private static readonly ILog _giornale = LogManager.GetLogger(typeof(BarCodeSrvImpl));

		private BackgroundWorker scansionatore {
			get; 
			set;
		}
		public IEnumerable<Fotografia> fotografie { get; private set; }

		public override bool possoChiudere()
		{
			return scansionatore == null || scansionatore.IsBusy == false;
		}

		public BarCodeSrvImpl()
        {
		}

		/// <summary>
		/// Questo provoca l'inizio della scansione tramite background worker
		/// </summary>
		public override void start()
		{
			base.start();

			if( scansionatore != null ) {
				scansionatore.RunWorkerAsync( fotografie );
			}
		}

		public override void stop()
		{
			if (isRunning)
			{
				try
				{
					scansionatore.CancelAsync();
				}
				catch (Exception ee)
				{
					_giornale.Warn("Non riesco a stoppare lo scansionatore. Porca paletta", ee);
				} finally {
					base.stop();
				}

			}
		}

		/// <summary>
		/// Questo metodo si può usare spot anche se il servizio non è avviato
		/// </summary>
		/// <param name="foto"></param>
		/// <returns></returns>
		public String searchBarCode(Fotografia foto)
		{
			FileInfo fotoInfo = PathUtil.fileInfoFoto(foto);

			_giornale.Debug("E' stata richiesta la ricerca del codice a carre sulla foto " + fotoInfo.Name + " Inizio ricerca codici a barre");

			String provinoFileName = Path.Combine(Configurazione.cartellaRepositoryFoto, PathUtil.decidiCartellaProvini(foto), fotoInfo.Name);
			
			// Prima provo sul provino (che è più veloce)
			String findBarCode = searchBarCodeExecutable( provinoFileName );

			// Poi provo anche sulla foto grande originale
			if( findBarCode == null )
				findBarCode = searchBarCodeExecutable( fotoInfo.FullName );

			if (findBarCode != null)
			{
				_giornale.Info("E' stato trovato il codice a barre sulla foto " + fotoInfo.Name + " BAR_CODE: " + findBarCode);
			}
			else
			{
				_giornale.Debug("Non è stato trovato alcun codice a basse sulla foto " + fotoInfo.Name);
			}

			return findBarCode;
		}

		public void prepareToScan( ParamCercaFoto param, ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler runWorkerCompleted ) {

			var ricercatoreSrv = LumenApplication.Instance.getServizioAvviato<IRicercatoreSrv>();
			fotografie = ricercatoreSrv.cerca( param );

			prepareToScan( fotografie, progressChanged, runWorkerCompleted );
		}

		/// <summary>
		/// Metodo che ricerca e applica il codice a barre trovato alla didascalie delle foto
		/// </summary>
		/// <param name="fotosDaEsaminare"></param>
		/// <returns></returns>
		public void scan( IEnumerable<Fotografia> fotos ) {
			prepareToScan( fotos, null, null );
			start();
		}

		public void prepareToScan( IEnumerable<Fotografia> fotos, ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler runWorkerCompleted ) {

			if( scansionatore != null && scansionatore.IsBusy )
				throw new InvalidOperationException( "scansionatore già in esecuzione" );

			// Rilascio eventuale scansionatore precedente
			if( scansionatore != null )
				scansionatore.Dispose();

			scansionatore = new BackgroundWorker();
			scansionatore.WorkerReportsProgress = true;
			scansionatore.WorkerSupportsCancellation = true;
			scansionatore.DoWork += scansionatore_DoWork;

			// Progresso
			if( progressChanged != null )
				scansionatore.ProgressChanged += progressChanged;

			// Finale
			if( runWorkerCompleted != null )
				scansionatore.RunWorkerCompleted += runWorkerCompleted;
			scansionatore.RunWorkerCompleted += scansionatore_RunWorkerCompleted;

			this.fotografie = fotos;

		}

		private void scansionatore_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e ) {

			scansionatore.Dispose();
			scansionatore = null;
			base.stop();			// fermo il servizio
		}

		/// <summary>
		/// In questo metodo eseguo effettivamente il lavoro di lanciare l'eseguibile ZSCAN che si occupa 
		/// di cercare il codice a barre e applicarlo eventualmente alla Fotografia.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void scansionatore_DoWork( object sender, DoWorkEventArgs e ) {

			BackgroundWorker worker = sender as BackgroundWorker;
			IEnumerable<Fotografia> fotografie = (IEnumerable<Fotografia>)e.Argument;

			StatoScansione statoScansione = new StatoScansione();
			statoScansione.totale = fotografie.Count();

			_giornale.Info( "E' stata richiesta la scansione di " + statoScansione.totale + " fotografie. Inizio ricerca codici a barre" );

			using( new UnitOfWorkScope() ) {

				foreach( Fotografia fotografia in fotografie ) {

					// Se mi è stato chiesto di uscire, allora mi interrompo
					if( worker.CancellationPending )
						break;

					// Calcolo percentuale progressione
					statoScansione.percentuale = (100 * (++statoScansione.attuale)) / statoScansione.totale;

					try {

						String findBarCode = searchBarCode( fotografia );

						if( findBarCode != null ) {
							modificaDidascaliaFotografie( fotografia, findBarCode );
							++statoScansione.barcodeTrovati;

							UnitOfWorkScope.currentDbContext.SaveChanges();
						}
						
					} catch( Exception eee ) {
						_giornale.Error( "Impossibile salvare la didascalia", eee );
					}

					worker.ReportProgress( statoScansione.percentuale, statoScansione );
				}
			}

			e.Result = statoScansione;

			_giornale.Info( "Scansionamento per ricerca codici a barre completato. Trovati " + statoScansione.barcodeTrovati + " barcode su " + statoScansione.totale + " foto" );
		}

		private void modificaDidascaliaFotografie(Fotografia ff, String findBarCode)
		{
			OrmUtil.forseAttacca<Fotografia>(ref ff);
			ff.didascalia = findBarCode;
			OrmUtil.cambiaStatoModificato(ff);
		}

		private String searchBarCodeExecutable( String path )
		{
			String outputCodiceBarre = UsbEjectWithExe.RunExecutable( @"Resources\ZBar\zbarimg.exe", " --xml " + path, null ).Output.ToString();
			return findBarCodeString(outputCodiceBarre);
		}

		/**
		 * Metodo che ricerca i codi a barre in questa struttura:
		 * "<barcodes xmlns='http://zbar.sourceforge.net/2008/barcode'>\r\n<source href='C:\\Users\\Edward_Acer\\Desktop\\RULLINI\\2015-03-28.Gio\\0001.Fot\\.Thumb\\code4.jpg'>\r\n<index num='0'>\r\n<symbol type='EAN-13' quality='315'><data><![CDATA[0123456789012]]></data></symbol>\r\n</index>\r\n</source>\r\n</barcodes>\r\n\r\n"
		 * "<barcodes xmlns='http://zbar.sourceforge.net/2008/barcode'>\r\n<source href='C:\\Users\\Edward_Acer\\Desktop\\RULLINI\\2015-03-28.Gio\\0001.Fot\\.Thumb\\DSC06508.JPG'>\r\n</source>\r\n</barcodes>\r\n\r\n"
		 * **/
		private String findBarCodeString(string inputString)
		{
			String findBarCode = null;

			int init = inputString.LastIndexOf("[CDATA[");
			//Testo se la stringa contiene il codice a barre
			if( init > 0 ) {

				init += "[CDATA[".Length;

				String subString = inputString.Substring( init );
				int fine = subString.IndexOf( "]" );

				findBarCode = subString.Substring( 0, fine );
			}

			return findBarCode;
		}


	}
}
