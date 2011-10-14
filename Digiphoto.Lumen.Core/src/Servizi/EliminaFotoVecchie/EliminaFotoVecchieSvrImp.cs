using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.IO;
using log4net;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using System.Threading;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Model;
using System.Data.Objects;
using System.Linq;

namespace Digiphoto.Lumen.Servizi.EliminaFotoVecchie{

    public class EliminaFotoVecchieSvrImp : ServizioImpl, IEliminaFotoVecchie{

        private static readonly ILog _giornale = LogManager.GetLogger(typeof(EliminaFotoVecchieSvrImp));

        #region Proprietà     

        private bool _morto = false;
        private ParamEliminaFotoVecchie _paramEliminaFotoVecchie;
        private Thread _threadCopia;

        public EliminaFotoVecchieMsg eliminaFotoVecchieMsg{
            get;
            private set;
        }

        #endregion

        public EliminaFotoVecchieSvrImp(){
    
        }

        ~EliminaFotoVecchieSvrImp()
        {
			// avviso se il thread di copia è ancora attivo
			if( _threadCopia != null && _threadCopia.IsAlive ) {
				_giornale.Warn( "Il thread che consente di eliminare i file è ancora attivo. Non è stata fatta la Join o la Abort.\nProababilmente il programma si inchioderà" );
			}
		}

        /**
		 * Lo scopo di questa operazione è quella di scaricare le foto
		 * dalla flash card all'hard disk locale il più velocemente
		 * possibile. In questo modo posso congedare il fotografo che
		 * sta aspettando. Durante questa fase, non ho feedback con l'utente.
		 * Terminato lo scarico, allora posso muovere il file nella giusta 
		 * cartella di destinazione, scrivendo nel database e sollevando gli
		 * eventi per poter visualizzare le foto.
		 */
        public void elimina(ParamEliminaFotoVecchie paramEliminaFotoVecchie)
        {

            if (!isRunning) {
                _giornale.Warn("Il servizio è stoppato. Non scarico le foto");
                return;
            }

            _paramEliminaFotoVecchie = paramEliminaFotoVecchie;

            seNonPossoEliminareSpaccati();

            // Scarico in un thread separato per non bloccare l'applicazione
            _threadCopia = new Thread(eliminaAsincrono);
            _threadCopia.Start();

        }

        private void seNonPossoEliminareSpaccati()
        {

            // Voglio evitare doppie esecuzioni.
            if (_morto)
                throw new InvalidOperationException("Il metodo scarica si può chiamare solo una volta");

            if (isRunning == false)
                throw new InvalidOperationException("Il servizio è fermo. Impossibile eliminare le foto adesso");

            if (!Directory.Exists(_paramEliminaFotoVecchie.cartellaSorgente))
                throw new FileNotFoundException("cartella da scaricare inesistente:\n" + _paramEliminaFotoVecchie.cartellaSorgente);

            if (_paramEliminaFotoVecchie.fotografo.id == null)
                throw new ArgumentException("fotografo non indicato");

        }

        private void eliminaAsincrono() {

            int conta = 0;
            DateTime oraInizio = DateTime.Now;


            _giornale.Debug("Inizio l'eliminazione delle foto da " + _paramEliminaFotoVecchie.cartellaSorgente);


            // Pattern Unit-of-work
            using (new UnitOfWorkScope(true))
            {

                string nomeDirDest = configurazione.getCartellaRepositoryFoto();
                string idFotografo = _paramEliminaFotoVecchie.fotografo.id;
                DateTime data = _paramEliminaFotoVecchie.intervalloEliminazione;

                eliminaFotoVecchieMsg = new EliminaFotoVecchieMsg();
               
                if (!System.IO.Directory.Exists(nomeDirDest)){
                  
                    string[] filePathsDate = Directory.GetDirectories(nomeDirDest);
                    foreach (String filePathEtichettaData in filePathsDate)
                    {
                        //Calcolo il percoso fino alle date e ne recupero le etichette per avere una data da confrotare
                        String etichettaData = Path.GetFileName(filePathEtichettaData);
                        DateTime newDate = Convert.ToDateTime(etichettaData);

                        string strNewDate = newDate.ToString("yyyy/MM/dd");

                        //string confronto = new DateTime(2011/10/14).ToString("yyyy/MM/dd");

                        if (Convert.ToDateTime(strNewDate).Date <= DateTime.Now) {
                            String filePath = filePathEtichettaData+"\\"+idFotografo;
                            foreach (string directoryPath in Directory.GetFiles(filePath))
                            {
                                //Elimino gli attributi solo lettura fiel nascosti
                                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) & ~FileAttributes.Hidden);
                                //Elimino gli attributi solo lettura                                
                                File.SetAttributes(directoryPath, File.GetAttributes(directoryPath) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));                  
                            }
                            Directory.Delete(filePath,true);
                            ++conta;
                            // Controlle se la cartella e rimasta vuota e nel caso la elimino
                            DirectoryInfo dir = new DirectoryInfo(filePathEtichettaData);
                            if (dir.GetDirectories().Length == 0 && dir.GetFiles().Length == 0){
                                Directory.Delete(filePathEtichettaData);
                            }
                        }
                    }
                    Console.WriteLine("FINE");
                }else{
                    _giornale.Info("Eliminazione annullata non esiste il percorso");
                }
                
                // Nel log scrivo anche il tempo che ci ho messo a scaricare le foto. Mi servirà per profilare
                TimeSpan tempoImpiegato = DateTime.Now.Subtract(oraInizio);
                _giornale.Info("Terminato l'eliminazione di " + conta + " foto. Tempo impiegato = " + tempoImpiegato);

                // Finito: genero un evento per notificare che l'utente può togliere la flash card.
                eliminaFotoVecchieMsg.fase = Digiphoto.Lumen.Servizi.EliminaFotoVecchie.EliminaFotoVecchieMsg.Fase.FineEliminazione;
                eliminaFotoVecchieMsg.descrizione = "Eliminazione foto Terminata";
                eliminaFotoVecchieMsg.cartellaSorgente= _paramEliminaFotoVecchie.cartellaSorgente;
            }
        }

        public override void Dispose(){
            try {

                // Se il tread di copia è ancora vivo, lo uccido
                if (_threadCopia != null){
                    if (_threadCopia.IsAlive)
                        _threadCopia.Abort();
                    else
                        _threadCopia.Join();
                }

            }
            finally{
                base.Dispose();
            }
        }

    }


}
