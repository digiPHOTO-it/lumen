using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using log4net;
using System.Threading;
using System.IO;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Model;
using System.Collections;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{

    public class EliminaFotoVecchieSrvImpl : ServizioImpl, IEliminaFotoVecchie 
    {

        private static readonly ILog _giornale = LogManager.GetLogger(typeof(EliminaFotoVecchieSrvImpl));      

        private EliminaFotoVecchieMsg _eliminaFotoVecchieMsg;

        private DateTime _dataIntervallo;

        private Fotografo _fotografo;

        public EliminaFotoVecchieSrvImpl()
        {
           
        }

        ~EliminaFotoVecchieSrvImpl()
        {

        }

        public void init(ParamEliminaFotoVecchieSrv param)
        {
            this._fotografo = param.fotografo;
        }

        public IList<String> getListaCartelleDaEliminare()
        {
			IList<String> listaCartelleDaEliminare = new List<String>();
            string nomeDirDest = @configurazione.getCartellaRepositoryFoto();
			if (_dataIntervallo == DateTime.MinValue)
			{
				_dataIntervallo = DateTime.Now.AddDays(-configurazione.getGiorniDeleteFoto());
			}
			
            string idFotografo = this._fotografo.id;

            _eliminaFotoVecchieMsg = new EliminaFotoVecchieMsg();

            if (System.IO.Directory.Exists(nomeDirDest))
            {
                string[] filePathsDate = Directory.GetDirectories(nomeDirDest);
                foreach (String filePathEtichettaData in filePathsDate)
                {
                    Console.WriteLine("[EliminaFotoVecchieSvrImpl]: "+filePathEtichettaData);
                    //Calcolo il percoso fino alle date e ne recupero le etichette per avere una data da confrotare
                    String etichettaData = Path.GetFileName(filePathEtichettaData);
                    DateTime newDate = Convert.ToDateTime(etichettaData);

                    string strNewDate = newDate.ToString("yyyy/MM/dd");

                    string dataIntervallo = _dataIntervallo.ToString("yyyy/MM/dd");

					if (Convert.ToDateTime(strNewDate).Date <= Convert.ToDateTime(dataIntervallo).Date)
					{
						String filePath = @filePathEtichettaData + "\\" + idFotografo;
						if(System.IO.Directory.Exists(filePath)){
							listaCartelleDaEliminare.Add(filePath);
						}
					}
					else
					{
					}
                }
            }
            else
            {
                _giornale.Info("Non esiste il percorso");
            }
            return listaCartelleDaEliminare;
        }

        public void elimina(String pathCartella)
        {
			String[] dirNameArray = pathCartella.Split(Path.DirectorySeparatorChar);
            foreach (string directoryPath in Directory.GetFiles(pathCartella))
            {			
				String nomeFile = Path.GetFileName(directoryPath);
                //Elimino gli attributi solo lettura file nascosti
                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) & ~FileAttributes.Hidden);
                //Elimino gli attributi solo lettura                                
                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));
            }
			String dirDataPath = Path.GetDirectoryName(pathCartella);
            Directory.Delete(pathCartella, true);
            // Controlle se la cartella e rimasta vuota e nel caso la elimino
			DirectoryInfo dir = new DirectoryInfo(dirDataPath);
			if (dir.GetDirectories().Length == 0 && dir.GetFiles().Length == 0)
			{
				Directory.Delete(dirDataPath);
			}

			using (LumenEntities dbContext = new LumenEntities())
			{
				dbContext.ExecuteStoreCommand("DELETE FROM Fotografie WHERE fotografo_id = {0} AND dataoraAcquisizione ={1}", _fotografo.id, _dataIntervallo);
				dbContext.SaveChanges();
			}

        }

        public Fotografo diChiSonoQuesteFoto()
        {
            return this._fotografo;
        }

		public void eliminaRecord()
		{
			DateTime dataRiferimento =  Convert.ToDateTime("2011/10/26").Date;
			using (LumenEntities dbContext = new LumenEntities())
			{
				/**
				 * Ah ricordati che per quel lavoro che stai facendo, non basta buttare via le cartelle dal filesystem, ma occorre buttare via anche 
				 * le Fotografia dal database.
				 * Esiste anche l'associazione Fotografia -> RigaAlbum da eliminare (quindi se quella foto era presente in un album, occorre toglierla)
				 * Altrimenti l'integrità referenziale si incazza.
				*/
				//IList<Fotografia> result = dbContext.Fotografie.Where(ff => ff.fotografo.id == _fotografo.id).ToList();

				dbContext.ExecuteStoreCommand("DELETE FROM Fotografie WHERE fotografo_id = {0} AND dataoraAcquisizione ={1}", _fotografo.id, dataRiferimento);

				//foreach(Fotografia r in result){
				//	Console.WriteLine(r.nomeFile);
				//}

				//RigaAlbum rigaAlbum = (RigaAlbum)dbContext.RigheAlbum.SingleOrDefault<RigaAlbum>(ff => ff.id == fotografia.id);

				//dbContext.DeleteObject(rigaAlbum);
				//dbContext.DeleteObject(result);
				dbContext.SaveChanges();
			}
		}

    }


}
