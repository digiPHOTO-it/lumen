using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using log4net;
using System.Threading;
using System.IO;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Model;
using System.Collections;
using Digiphoto.Lumen.Util;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{

    public class EliminaFotoVecchieSrvImpl : ServizioImpl, IEliminaFotoVecchieSrv 
    {
        private static readonly ILog _giornale = LogManager.GetLogger(typeof(EliminaFotoVecchieSrvImpl));      

        public EliminaFotoVecchieSrvImpl()
        {
        }

        ~EliminaFotoVecchieSrvImpl()
        {

        }

        /// <summary>
        /// Restituisce la lista dei path che soddisfano il criterio di eliminazione in base alla data
        /// </summary>
        public IList<String> getListaCartelleDaEliminare()
        {
			IList<String> listaCartelleDaEliminare = new List<String>();
            String pathCartellaRepositoryFoto = Configurazione.cartellaRepositoryFoto;
			DateTime dataIntervallo = DateTime.Now.AddDays(-UserConfigLumen.GiorniDeleteFoto);

            if (System.IO.Directory.Exists(pathCartellaRepositoryFoto))
            {
                String[] listPathsDate = Directory.GetDirectories(pathCartellaRepositoryFoto);
                foreach (String filePathEtichettaData in listPathsDate)
                {
                    //System.Diagnostics.Trace.WriteLine("[EliminaFotoVecchieSvrImpl]: "+filePathEtichettaData);
                    //Calcolo il percoso fino alle date e ne recupero le etichette per avere una data da confrotare
                    String etichettaData = Path.GetFileName(filePathEtichettaData);
                    DateTime dateDaEtichetta = Convert.ToDateTime(PathUtil.giornoFromPath(etichettaData));
					String strDateDaEtichetta = dateDaEtichetta.ToString("yyyy/MM/dd");

                    String dataInt = dataIntervallo.ToString("yyyy/MM/dd");

					if (Convert.ToDateTime(strDateDaEtichetta).Date <= Convert.ToDateTime(dataInt).Date)
					{
						String[] filePath = Directory.GetDirectories(filePathEtichettaData);
						foreach(String path in filePath){
							listaCartelleDaEliminare.Add(path);
						}
						
					}
                }
            }
            else
            {
                _giornale.Info("Non esiste il percorso");
            }
            return listaCartelleDaEliminare;
        }

        /// <summary>
        /// Consente di eliminare tutte le foto contenute nel path
        /// </summary>
        /// <param name="pathCartella"></param>
        public void elimina(String pathCartella)
        {
            String fotografoID = PathUtil.fotografoIDFromPath(pathCartella);
            DateTime dataRiferimento = Convert.ToDateTime(PathUtil.giornoFromPath(pathCartella)).Date;

            EliminaFotoVecchieMsg eliminaFotoVecchieMsg = new EliminaFotoVecchieMsg( this );

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
            using (new UnitOfWorkScope())
			{
                LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
                objContext.ObjectContext.ExecuteStoreCommand("DELETE FROM Fotografie WHERE fotografo_id = {0} AND DATEPART(yyyy, dataOraAcquisizione) = {1} AND DATEPART(mm, dataOraAcquisizione) = {2} AND DATEPART(dd, dataOraAcquisizione)= {3}", fotografoID, dataRiferimento.Year, dataRiferimento.Month, dataRiferimento.Day);
                // Elimino tutti gli album rimasti senza foto
                objContext.ObjectContext.ExecuteStoreCommand("DELETE FROM Albums WHERE (id NOT IN(SELECT DISTINCT AlbumRigaAlbum_RigaAlbum_id FROM RigheAlbum))");
                objContext.SaveChanges();
			}
            eliminaFotoVecchieMsg.fase = Fase.FineEliminazione;
            pubblicaMessaggio(eliminaFotoVecchieMsg);
        }

        /// <summary>
        /// Consente di eliminare tutti gli album rimasti senza foto
        /// </summary>
        public void eliminaAlbumNonReferenziati()
        {
            EliminaFotoVecchieMsg eliminaFotoVecchieMsg = new EliminaFotoVecchieMsg( this );
            using (new UnitOfWorkScope())
            {
                LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
                objContext.ObjectContext.ExecuteStoreCommand("DELETE FROM Albums WHERE (id NOT IN(SELECT DISTINCT AlbumRigaAlbum_RigaAlbum_id FROM RigheAlbum))");
                objContext.SaveChanges();
                eliminaFotoVecchieMsg.fase = Fase.FineEliminazioneAlbumNonReferenziati;
                pubblicaMessaggio(eliminaFotoVecchieMsg);
            }
        }

        /// <summary>
        /// Restituisce il fotografo a cui appartengono le foto memorizzate nel path
        /// </summary>
        /// <param name="path"></param>
        public Fotografo diChiSonoQuesteFoto(String path)
		{
            //EliminaFotoVecchieMsg eliminaFotoVecchieMsg = new EliminaFotoVecchieMsg();
            String fotografoID= PathUtil.fotografoIDFromPath(path);
            LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
            return objContext.Fotografi.SingleOrDefault(f => f.id == fotografoID);
		}
    }
}
