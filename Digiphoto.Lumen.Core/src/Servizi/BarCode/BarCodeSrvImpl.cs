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

namespace Digiphoto.Lumen.Servizi.BarCode
{
	public class BarCodeSrvImpl : ServizioImpl, IBarCodeSrv
	{
		private static readonly ILog _giornale = LogManager.GetLogger(typeof(BarCodeSrvImpl));

		private bool _possoChiudere;
		public override bool possoChiudere()
		{
			return _possoChiudere;
		}

		public BarCodeSrvImpl()
        {
			_possoChiudere = true;
        }

		public override void start()
		{
			base.start();
		}

		public override void stop()
		{
			if (isRunning)
			{
				base.stop();
				try
				{
				}
				catch (Exception ee)
				{
					_giornale.Warn("Non riesco a stoppare questo servizio. Porca paletta", ee);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);  // se mi chiama lo stop mi da dei problemi. Evito e faccio solo la dispose
		}

		public String searchBarCode(Fotografia foto)
		{
			_possoChiudere = false;

			FileInfo fotoInfo = PathUtil.fileInfoFoto(foto);

			_giornale.Info("E' stata richiesta la ricerca del codice a carre sulla foto " + fotoInfo.Name + " Inizio ricerca codici a barre");

			String path = Path.Combine(Configurazione.cartellaRepositoryFoto, PathUtil.decidiCartellaProvini(foto), fotoInfo.Name);
			
			String findBarCode = searchBarCodeExecutable(path, fotoInfo.Name);
			if (findBarCode != null)
			{
				_giornale.Info("E' stato trovato il codice a barre sulla foto " + fotoInfo.Name + " BAR_CODE: " + findBarCode);
			}
			else
			{
				_giornale.Info("Non è stato trovato alcun codice a basse sulla foto " + fotoInfo.Name);
			}

			_possoChiudere = true;

			return findBarCode;
		}

		/// <summary>
		/// Metodo che ricerca e applica il codice a barre trovato alla didascalie delle foto
		/// </summary>
		/// <param name="fotosDaEsaminare"></param>
		/// <returns></returns>
		public int applicaBarCodeDidascalia(IEnumerable<Fotografia> fotos)
		{
			_possoChiudere = false;
			int conta = 0;
			_giornale.Info("E' stata richiesta la ricerca di " + fotos.Count() + " fotografie. Inizio ricerca codici a barre");

			using (TransactionScope transaction = new TransactionScope()) {
				try
				{
					foreach (Fotografia ff in fotos)
					{
						FileInfo fotoInfo = PathUtil.fileInfoFoto(ff);
						String path = Path.Combine(Configurazione.cartellaRepositoryFoto, PathUtil.decidiCartellaProvini(ff), fotoInfo.Name);

						String findBarCode = searchBarCodeExecutable(path, fotoInfo.Name);
						if (findBarCode != null)
						{
							modificaDidascaliaFotografie(ff, findBarCode);
							_giornale.Info("E' stato trovato il codice a barre sulla foto " + fotoInfo.Name + " BAR_CODE: " + findBarCode);
							++conta;
						}
					}

					UnitOfWorkScope.currentDbContext.SaveChanges();
					_giornale.Debug("Modifica metadati salvataggio eseguito. Ora committo la transazione");

					transaction.Complete();
					_giornale.Info("Commit metadati andato a buon fine");

				} catch( Exception eee ) {
					_giornale.Error("Impossibile salvare la didascalia", eee);
				}
			}

			_possoChiudere = true;
			return conta;
		}

		private void modificaDidascaliaFotografie(Fotografia ff, String findBarCode)
		{
			OrmUtil.forseAttacca<Fotografia>(ref ff);
			ff.didascalia = findBarCode;
			OrmUtil.cambiaStatoModificato(ff);
		}

		private String searchBarCodeExecutable(String path, string pathProvino)
		{
			String outputCodiceBarre = UsbEjectWithExe.RunExecutable(@"Resources\ZBar\zbarimg.exe",
																	" --xml " +
																	path,
																	null).Output.ToString();
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
			if (init == -1)
				return findBarCode;

			init+="[CDATA[".Length;

			String subString = inputString.Substring(init);
			int fine = subString.IndexOf("]");
			
			findBarCode = subString.Substring(0, fine);

			return findBarCode;
		}
	
	}
}
