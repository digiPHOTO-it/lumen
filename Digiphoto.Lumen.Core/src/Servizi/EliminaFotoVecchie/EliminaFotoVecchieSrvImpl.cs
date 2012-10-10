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
using Digiphoto.Lumen.Database;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie
{

    public class EliminaFotoVecchieSrvImpl : ServizioImpl, IEliminaFotoVecchieSrv 
    {
        private static readonly ILog _giornale = LogManager.GetLogger(typeof(EliminaFotoVecchieSrvImpl));      

        public EliminaFotoVecchieSrvImpl()
        {
        }


        /// <summary>
        /// Restituisce la lista dei path che soddisfano il criterio di eliminazione in base alla data
        /// </summary>
        public IList<String> getListaCartelleDaEliminare()
        {
			IList<String> listaCartelleDaEliminare = new List<String>();
            String pathCartellaRepositoryFoto = Configurazione.cartellaRepositoryFoto;
			int ggDel = Configurazione.infoFissa.numGiorniEliminaFoto;
			DateTime dataIntervallo = DateTime.Now.AddDays( -ggDel );

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
                _giornale.Info("Non usabile il percorso");
            }
            return listaCartelleDaEliminare;
        }

        /// <summary>
        /// Consente di eliminare tutte le foto contenute nel path
        /// </summary>
        /// <param name="pathCartella"></param>
        public int elimina(String pathCartella)
        {
			int quante = 0;
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
                quante = objContext.ObjectContext.ExecuteStoreCommand("DELETE FROM Fotografie WHERE fotografo_id = {0} AND DATEPART(yyyy, dataOraAcquisizione) = {1} AND DATEPART(mm, dataOraAcquisizione) = {2} AND DATEPART(dd, dataOraAcquisizione)= {3}", fotografoID, dataRiferimento.Year, dataRiferimento.Month, dataRiferimento.Day);
                // Elimino tutti gli album rimasti senza foto
                objContext.ObjectContext.ExecuteStoreCommand("DELETE FROM Albums WHERE (id NOT IN(SELECT DISTINCT AlbumRigaAlbum_RigaAlbum_id FROM RigheAlbum))");
                objContext.SaveChanges();
			}
            eliminaFotoVecchieMsg.fase = Fase.FineEliminazione;
			eliminaFotoVecchieMsg.descrizione = "Eliminate " + quante + " foto dalla cartella " + pathCartella;
			eliminaFotoVecchieMsg.showInStatusBar = true;
            pubblicaMessaggio(eliminaFotoVecchieMsg);
			return quante;
        }

		/// <summary>
		/// Elimino e distruggo le foto sparse indicate
		/// </summary>
		/// <param name="fotosDaCanc"></param>
		public int elimina( IEnumerable<Fotografia> fotosDaCanc ) {

			int conta = 0;
			_giornale.Info( "E' stata richiesta la distruzione di " + fotosDaCanc.Count() + " fotografie. Iniizo eliminazione" );

			LumenEntities lumenEntities = UnitOfWorkScope.CurrentObjectContext;

			foreach( Fotografia ff in fotosDaCanc ) {

				Fotografia ff2 = ff;
				OrmUtil.forseAttacca<Fotografia>( ref ff2 );

				AiutanteFoto.disposeImmagini( ff2 );

				// Elimino la foto da disco
				seEsisteCancella( PathUtil.nomeCompletoRisultante( ff2 ) );
				seEsisteCancella( PathUtil.nomeCompletoProvino( ff2 ) );
				seEsisteCancella( PathUtil.nomeCompletoOrig( ff2 ) );

				// Poi dal database

				var appo = lumenEntities.Fotografie.Remove( ff2 );
				++conta;
			}

	
			int test3 = lumenEntities.ObjectContext.SaveChanges();


			_giornale.Info( "Eliminazione foto completata. Tot = " + conta );
			_giornale.Debug( "la SaveChanges ha ritornato: " + test3 );

			if( test3 > 0 ) {
				// Rilancio un messaggio in modo che tutta l'applicazione (e tutti i componenti) vengano notificati 
				EliminateFotoMsg msg = new EliminateFotoMsg( this as IEliminaFotoVecchieSrv );
				msg.listaFotoEliminate = fotosDaCanc;
				pubblicaMessaggio( msg );
			}

			return conta;
		}

		/// <summary>
		/// Se esiste il file indicato lo cancello, altrimenti pazienza
		/// </summary>
		/// <param name="nomeFile"></param>
		void seEsisteCancella( string nomeFile ) {
			
			if( File.Exists(nomeFile) ) {
				try {

					//Elimino gli attributi solo lettura file nascosti
					File.SetAttributes( nomeFile, File.GetAttributes( nomeFile ) & ~FileAttributes.Hidden );
					//Elimino gli attributi solo lettura                                
					File.SetAttributes( nomeFile, File.GetAttributes( nomeFile ) & ~(FileAttributes.Archive | FileAttributes.ReadOnly) );

					File.Delete( nomeFile );

					_giornale.Debug( "Eliminata immagine " + nomeFile );
				} catch( Exception ee ) {
					_giornale.Warn( "Impossibile eliminare immagine " + nomeFile, ee );
				}
			}
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
