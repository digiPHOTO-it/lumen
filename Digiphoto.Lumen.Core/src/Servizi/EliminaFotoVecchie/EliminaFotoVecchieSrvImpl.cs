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

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{

    public class EliminaFotoVecchieSrvImpl : ServizioImpl, IEliminaFotoVecchie 
    {

        private static readonly ILog _giornale = LogManager.GetLogger(typeof(EliminaFotoVecchieSrvImpl));

        private IList<String> _listaCartelleDaEliminare = null;

        private EliminaFotoVecchieMsg _eliminaFotoVecchieMsg;

        private DateTime _dataIntervallo;

        private Fotografo _fotografo;

        public EliminaFotoVecchieSrvImpl()
        {
            _listaCartelleDaEliminare = new List<String>();
        }

        ~EliminaFotoVecchieSrvImpl()
        {

        }

        public void init(ParamEliminaFotoVecchieSrv param)
        {
            this._dataIntervallo = param.dataIntervallo;
            this._fotografo = param.fotografo;
        }

        public IList<String> getListaCartelleDaEliminare()
        {
            string nomeDirDest = @configurazione.getCartellaRepositoryFoto();
            string idFotografo = this._fotografo.id;

            _eliminaFotoVecchieMsg = new EliminaFotoVecchieMsg();

            if (System.IO.Directory.Exists(nomeDirDest))
            {
                string[] filePathsDate = Directory.GetDirectories(nomeDirDest);
                foreach (String filePathEtichettaData in filePathsDate)
                {
                    Console.WriteLine(filePathEtichettaData);
                    //Calcolo il percoso fino alle date e ne recupero le etichette per avere una data da confrotare
                    String etichettaData = Path.GetFileName(filePathEtichettaData);
                    DateTime newDate = Convert.ToDateTime(etichettaData);

                    string strNewDate = newDate.ToString("yyyy/MM/dd");

                    string dataIntervallo = _dataIntervallo.ToString("yyyy/MM/dd");

                    if (Convert.ToDateTime(strNewDate).Date <= Convert.ToDateTime(dataIntervallo).Date)
                    {
                        String filePath = @filePathEtichettaData + "\\" + idFotografo;
                        _listaCartelleDaEliminare.Add(filePath);
                    }
                }
            }
            else
            {
                _giornale.Info("Non esiste il percorso");
            }
            return _listaCartelleDaEliminare;
        }


        public void elimina(String pathCartella)
        {
            foreach (string directoryPath in Directory.GetFiles(pathCartella))
            {
                //Elimino gli attributi solo lettura file nascosti
                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) & ~FileAttributes.Hidden);
                //Elimino gli attributi solo lettura                                
                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));
                using (LumenEntities dbContext = new LumenEntities())
                {

                    /**
                     * Ah ricordati che per quel lavoro che stai facendo, non basta buttare via le cartelle dal filesystem, ma occorre buttare via anche 
                     * le Fotografia dal database.
                     * Esiste anche l'associazione Fotografia -> RigaAlbum da eliminare (quindi se quella foto era presente in un album, occorre toglierla)
                     * Altrimenti l'integrità referenziale si incazza.
                    */
                    Fotografia fotografia = (Fotografia)dbContext.Fotografie.SingleOrDefault<Fotografia>(ff => ff.nomeFile == _idFotografo);
                    //RigaAlbum rigaAlbum = (RigaAlbum)dbContext.RigheAlbum.SingleOrDefault<RigaAlbum>(ff => ff.id == fotografia.id);

                    //dbContext.DeleteObject(rigaAlbum);
                    dbContext.DeleteObject(fotografia);

                    dbContext.SaveChanges();
                }
            }
            Directory.Delete(pathCartella, true);
            // Controlle se la cartella e rimasta vuota e nel caso la elimino
            //DirectoryInfo dir = new DirectoryInfo(filePathEtichettaData);
            //if (dir.GetDirectories().Length == 0 && dir.GetFiles().Length == 0)
            //{
            //    Directory.Delete(filePathEtichettaData);
            //}
        }

        public Fotografo diChiSonoQuesteFoto()
        {
            return this._fotografo;
        }

    }


}
