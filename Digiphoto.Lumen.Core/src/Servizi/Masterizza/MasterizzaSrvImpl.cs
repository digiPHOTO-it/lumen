using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Servizi.Masterizzare;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Database;
using log4net;
using System.Threading;
using System.Collections;
using System.IO;
using IMAPI2.Interop;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using IMAPI2.MediaItem;
using Digiphoto.Lumen.Servizi.Masterizzare.MyBurner;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Servizi.Masterizzare
{
    public class MasterizzaSrvImpl : ServizioImpl, IMasterizzaSrv 
    {
        private static readonly ILog _giornale = LogManager.GetLogger(typeof(MasterizzaSrvImpl));

        private TipoDestinazione _tipoDestinazione;

        private IList<Fotografia> _listFotografie;

        private String _destinazione;

        private bool _morto = false;

        private String _driverLetter;

        private BurnerSrvImpl _burner;

        private Thread _threadCopiaSuChiavetta;

        private BackgroundWorker backgroundWorkerCopia;

        private Thread _threadMasterizza;

        private int countFotoAggiunte;

        private int countFotoNonAggiunte;

        private Fotografia fotCopia;

        private bool erroriCopia;

        public MasterizzaSrvImpl()
        {
            this._listFotografie = new List<Fotografia>();
        }

        ~MasterizzaSrvImpl()
        {
			// avviso se il thread di copia è ancora attivo
            if (backgroundWorkerCopia != null && backgroundWorkerCopia.WorkerSupportsCancellation == true)
            {
                backgroundWorkerCopia.CancelAsync();
            }
		}

        private void seNonPossoCopiareSpaccati()
        {
            // Voglio evitare doppie esecuzioni. Si scarica e poi si distrugge
            if (_morto)
                throw new InvalidOperationException("Il metodo Copia si può chiamare solo una volta");

            if (isRunning == false)
                throw new InvalidOperationException("Il servizio è fermo. Impossibile Copiare le foto adesso");   
        }

        /// <summary>
        /// Metodo che consente di copiare le foto su di una chiavetta o di masterizzarle. 
        /// Imposto il tipo di copia su "CARTELLA" o su "MASTERIZZATORE" nel caso che utilizzi come destinazione la cartella
        /// il parametro destinazione rappresenta il percorso su cui copiare i file; nel caso utilizzi il Masterizzatore
        /// rappresenta il Driver da utilizzare
        /// </summary>
        /// <param name="tipoDestinazione"></param>
        /// <param name="destinazione"></param>
        public void impostaDestinazione(TipoDestinazione tipoDestinazione, String destinazione)
        {
            this._tipoDestinazione = tipoDestinazione;
            switch (_tipoDestinazione)
            {
                case TipoDestinazione.CARTELLA:
                    this._destinazione = destinazione;
                    if (!Directory.Exists(_destinazione))
                    {
                        Directory.CreateDirectory(_destinazione);
                    }
                    break;
                case TipoDestinazione.MASTERIZZATORE:
                    this._driverLetter = destinazione;
                    break;
            }
        }

        /// <summary>
        /// Metodo che consenta la "MASTERIZZAZIONE" o la copia su "CHIAVETTA" delle foto
        /// </summary>
        public void masterizza()
        {
            switch(_tipoDestinazione){
                case TipoDestinazione.CARTELLA :
                    // Scarico in un thread separato per non bloccare l'applicazione
                    //seNonPossoCopiareSpaccati();
                    //backgroundWorkerCopia = new BackgroundWorker();
                    //backgroundWorkerCopia.WorkerReportsProgress = true;
                    //backgroundWorkerCopia.WorkerSupportsCancellation = true;
                    //backgroundWorkerCopia.DoWork += backgroundWorkerCopia_DoWork;
                    //backgroundWorkerCopia.ProgressChanged += backgroundWorkerCopia_ProgressChanged;
                    //backgroundWorkerCopia.RunWorkerCompleted += backgroundWorkerCopia_RunWorkerCompleted;
                    //backgroundWorkerCopia.RunWorkerAsync();
                    _threadMasterizza = new Thread(copiaCartellaDestinazioneAsincrono);
                    _threadMasterizza.Start();
                    break;
                case TipoDestinazione.MASTERIZZATORE :
                    _burner = new BurnerSrvImpl();
                    _burner.InviaStatoMasterizzazione += new BurnerSrvImpl.StatoMasterizzazioneEventHandler(inviaMessaggioStatoMasterizzazione);
                    _burner.start();
                    foreach (Fotografia fot in _listFotografie)
                    {
                        _burner.addFileToBurner(configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile);
                    }
                    _burner.setDiscRecorder(_driverLetter);
                    _burner.testMedia();
                    //Imposto l'etichetta del CD
                    _burner.etichetta = DateTime.Now.ToString("yyyy/MM/dd");;
                    _burner.burning();
                    break;
            }
        }

        /// <summary>
        /// The thread that does the burning of the media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copiaCartellaDestinazioneAsincrono()
        {
            // Pattern Unit-of-work
            using (LumenEntities objContext = new LumenEntities())
            {
                System.Diagnostics.Trace.WriteLine("INIZIO COPIA SU CARTELLA");
                MasterizzaMsg inizioCopiaMsg = new MasterizzaMsg();
                inizioCopiaMsg.fase = Fase.InizioCopiaChiavetta;
                inizioCopiaMsg.progress = 0;
                inizioCopiaMsg.result = "Inizio Copia Su Chiavetta";
                pubblicaMessaggio(inizioCopiaMsg);

                int countfotoAggiunte = 0;
                int countFotoNonAggiunte = 0;
                bool errori = false;
                foreach (Fotografia fot in _listFotografie)
                {
                    string nomeFileDest = "";
                    try
                    {  
                        nomeFileDest = Path.Combine(_destinazione, Path.GetFileName(fot.nomeFile));
                        String nomeFileSorgente = configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile;
                        File.Copy(@nomeFileSorgente, nomeFileDest, false);
                        //Elimino gli attributi solo lettura                                
                        File.SetAttributes(nomeFileDest, File.GetAttributes(nomeFileDest) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));

                        MasterizzaMsg statoCopiaMsg = new MasterizzaMsg();
                        statoCopiaMsg.riscontratiErrori = false;
                        statoCopiaMsg.fotoAggiunta = 1;
                        statoCopiaMsg.totFotoAggiunte = ++countfotoAggiunte;
                        statoCopiaMsg.totFotoNonAggiunte = countFotoNonAggiunte;
                        statoCopiaMsg.result = "Il File " + fot.nomeFile + " è stato copiato sulla chiavetta con successo";
                        //pubblicaMessaggio(statoCopiaMsg);
                        System.Diagnostics.Trace.WriteLine("Il File " + fot.nomeFile + " è stato copiato sulla chiavetta con successo");
                    }
                    catch (IOException ee)
                    {
                        MasterizzaMsg statoCopiaErroreMsg = new MasterizzaMsg();
                        statoCopiaErroreMsg.riscontratiErrori = true;
                        errori = true;
                        statoCopiaErroreMsg.fotoAggiunta = 0;
                        statoCopiaErroreMsg.totFotoAggiunte = countfotoAggiunte;
                        statoCopiaErroreMsg.totFotoNonAggiunte = ++countFotoNonAggiunte;
                        statoCopiaErroreMsg.result = "Il file " + fot.nomeFile + " non è stato copiato sulla chiavetta: " + nomeFileDest;
                        _giornale.Error("Il file " + @configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile + " non è stato copiato il file sulla chiavetta " + nomeFileDest, ee);
                        //pubblicaMessaggio(statoCopiaErroreMsg);
                        System.Diagnostics.Trace.WriteLine("Il file " + fot.nomeFile + " non è stato copiato sulla chiavetta: " + nomeFileDest);
                    }
                }
                MasterizzaMsg copiaCompletataMsg = new MasterizzaMsg();
                if (errori)
                {
                    copiaCompletataMsg.fase = Fase.CopiaChiavettaFallita;
                    copiaCompletataMsg.result = "Copia non Completata";
                }
                else
                {
                    copiaCompletataMsg.fase = Fase.CopiaChiavettaCompletata;
                    copiaCompletataMsg.result = "Copia Completata con Successo";
                }
                copiaCompletataMsg.riscontratiErrori = errori;
                copiaCompletataMsg.progress = 100;
                copiaCompletataMsg.totFotoNonAggiunte = countFotoNonAggiunte;
                copiaCompletataMsg.totFotoAggiunte = countfotoAggiunte;
                
                pubblicaMessaggio(copiaCompletataMsg);
                System.Diagnostics.Trace.WriteLine("FINE");
            } 
        }

        #region CopiaSuChiavetta;

        /// <summary>
        /// The thread that does the burning of the media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerCopia_DoWork(object sender, DoWorkEventArgs e)
        {
            // Pattern Unit-of-work
            using (LumenEntities objContext = new LumenEntities())
            {
                System.Diagnostics.Trace.WriteLine("INIZIO COPIA SU CARTELLA");
                MasterizzaMsg inizioCopiaMsg = new MasterizzaMsg();
                inizioCopiaMsg.fase = Fase.InizioCopiaChiavetta;
                inizioCopiaMsg.progress = 0;
                inizioCopiaMsg.result = "Inizio Copia Su Chiavetta";
                pubblicaMessaggio(inizioCopiaMsg);

                countFotoAggiunte = 0;
                countFotoNonAggiunte = 0;
                erroriCopia = false;
                foreach (Fotografia fot in _listFotografie)
                {
                    fotCopia = fot;
                    string nomeFileDest = "";
                    try
                    {
                        nomeFileDest = Path.Combine(_destinazione, Path.GetFileName(fot.nomeFile));
                        String nomeFileSorgente = configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile;
                        File.Copy(@nomeFileSorgente, nomeFileDest, false);
                        //Elimino gli attributi solo lettura                                
                        File.SetAttributes(nomeFileDest, File.GetAttributes(nomeFileDest) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));

                        //MasterizzaMsg statoCopiaMsg = new MasterizzaMsg();
                        //statoCopiaMsg.riscontratiErrori = false;
                        //statoCopiaMsg.fotoAggiunta = 1;
                        //statoCopiaMsg.totFotoAggiunte = ++countFotoAggiunte;
                        //statoCopiaMsg.result = "Copia File " + fot.nomeFile;
                        
                        backgroundWorkerCopia.ReportProgress(++countFotoAggiunte / _listFotografie.Count * 100);
                    }
                    catch (IOException ee)
                    {
                        //MasterizzaMsg statoCopiaErroreMsg = new MasterizzaMsg();
                        //statoCopiaErroreMsg.riscontratiErrori = true;
                        erroriCopia = true;
                        //statoCopiaErroreMsg.totFotoNonAggiunte = ++countFotoNonAggiunte;
                        //statoCopiaErroreMsg.result = "Il file " + @configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile + " non è stato copiato il file sulla chiavetta " + nomeFileDest;
                        //_giornale.Error("Il file " + @configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile + " non è stato copiato il file sulla chiavetta " + nomeFileDest, ee);
                        backgroundWorkerCopia.ReportProgress(++countFotoNonAggiunte / _listFotografie.Count * 100);
                    }
                }
               
            }
        }

        /// <summary>
        /// Event receives notification from the Burn thread of an event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerCopia_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MasterizzaMsg statoCopiaMsg = new MasterizzaMsg();
            if (erroriCopia)
            {
                statoCopiaMsg.riscontratiErrori = true;           
                statoCopiaMsg.fotoAggiunta = 0;
                statoCopiaMsg.result = "Il File " + fotCopia.nomeFile + " non è stato copiato sulla chiavetta";
            }
            else
            {
                statoCopiaMsg.riscontratiErrori = false;
                statoCopiaMsg.fotoAggiunta = 1;
                statoCopiaMsg.result = "Il File " + fotCopia.nomeFile + " è stato copiato sulla chiavetta con successo";
            }
            statoCopiaMsg.totFotoAggiunte = countFotoNonAggiunte;
            statoCopiaMsg.totFotoNonAggiunte = countFotoAggiunte;
            pubblicaMessaggio(statoCopiaMsg);
            erroriCopia = false;
        }

        /// <summary>
        /// Copia Completata
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerCopia_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MasterizzaMsg statoCopiaCompletatoMsg = new MasterizzaMsg();
            statoCopiaCompletatoMsg.riscontratiErrori = erroriCopia;
            if(erroriCopia){
                statoCopiaCompletatoMsg.fase = Fase.CopiaChiavettaFallita;
            }
            statoCopiaCompletatoMsg.fase = Fase.CopiaChiavettaCompletata;
            statoCopiaCompletatoMsg.totFotoNonAggiunte = countFotoNonAggiunte;
            statoCopiaCompletatoMsg.totFotoAggiunte = countFotoAggiunte;
            pubblicaMessaggio(statoCopiaCompletatoMsg);
        }

        #endregion CopiaSuChiavetta;

        /// <summary>
        /// Metodo che consente di cancellare le foto da una Directory
        /// </summary>
        /// <param name="currDir"></param>
        private void DeleteFolder(DirectoryInfo currDir)
        {
          if (currDir.GetFiles().Length > 0)
          {
            foreach (FileInfo file in currDir.GetFiles())
            {
                File.Delete(file.FullName);
            }

            foreach (DirectoryInfo folder in currDir.GetDirectories())
            {
                DeleteFolder(folder);
            }
          }
        }

        #region AggiuntaFile

        /// <summary>
        /// Consente di aggiungere un intero album alla lista delle foto da copiare
        /// </summary>
        ///  <param name="album"></param>
        public void addAlbum(Model.Album album)
        {
            using (LumenEntities objContext = new LumenEntities())
            {
                foreach (RigaAlbum alb in objContext.RigheAlbum.Where(r => r.id == album.id))
                {
                    _listFotografie.Add(alb.fotografia);
                }
            }
        }

        /// <summary>
        /// Consente di aggiungere una foto alla lista delle foto da copiare
        /// </summary>
        ///  <param name="item"></param>
        public void Add(Model.Fotografia item)
        {
            _listFotografie.Add(item);
        }

        public int IndexOf(Model.Fotografia item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Model.Fotografia item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public Model.Fotografia this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Clear()
        {
            _listFotografie.Clear();
        }

        public bool Contains(Model.Fotografia item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Model.Fotografia[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _listFotografie.Count(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Consente di rimuovere una foto dalla lista delle foto da copiare
        /// </summary>
        ///  <param name="item"></param>
        public bool Remove(Model.Fotografia item)
        {
            return _listFotografie.Remove(item);
        }

        /// <summary>
        /// Consente di rimuovere un intero album dalla lista delle foto da copiare
        /// </summary>
        ///  <param name="album"></param>
        public void Remove(Model.Album album)
        {
            using (LumenEntities objContext = new LumenEntities())
            {
                foreach (RigaAlbum alb in objContext.RigheAlbum.Where(r => r.id == album.id))
                {
                    _listFotografie.Remove(alb.fotografia);
                }
            }
        }

        public IEnumerator<Model.Fotografia> GetEnumerator()
        {
            return _listFotografie.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _listFotografie.GetEnumerator();
        }

        public override void Dispose()
        {
            try
            {


            }
            finally
            {
                base.Dispose();
                if (_burner != null)
                {
                    _burner.Dispose();
                }
            }
        }

        #endregion

        #region NotificaMessaggiBurner
        private void inviaMessaggioStatoMasterizzazione(object sender, BurnerMsg burnerMsg)
        {
            MasterizzaMsg masterizzaMsg = new MasterizzaMsg();
            masterizzaMsg.totFotoAggiunte = burnerMsg.totaleFileAggiunti;
            masterizzaMsg.totFotoNonAggiunte = 0;
            masterizzaMsg.result = burnerMsg.statusMessage;
            masterizzaMsg.progress = burnerMsg.progress;
            pubblicaMessaggio(masterizzaMsg);
        }
        #endregion
    }
}
