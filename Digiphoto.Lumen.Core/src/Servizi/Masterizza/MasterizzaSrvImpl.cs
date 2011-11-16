using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Servizi.Masterizza;
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
using Digiphoto.Lumen.Servizi.Masterizza.MyBurner;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Servizi.Masterizzare
{
    class MasterizzaSrvImpl : ServizioImpl, IMasterizzaSrv 
    {
        private static readonly ILog _giornale = LogManager.GetLogger(typeof(MasterizzaSrvImpl));

        private TipoDestinazione _tipoDestinazione;

        private IList<Fotografia> _listFotografie;

        private String _destinazione;

        private bool _morto = false;

        private String _driverLetter;

        private BurnerSrvImpl _burner;

        private Thread _threadMasterizza;

        /** Questo rappresenta l'esito dello scaricamento delle foto */
        public MasterizzaMsg masterizzaMsg
        {
            get;
            private set;
        }

        public MasterizzaSrvImpl()
        {
            this._listFotografie = new List<Fotografia>();
        }

        ~MasterizzaSrvImpl()
        {
			// avviso se il thread di copia è ancora attivo
			if( _threadMasterizza != null && _threadMasterizza.IsAlive ) {
				_giornale.Warn( "Il thread di masterizzazione file è ancora attivo. Non è stata fatta la Join o la Abort.\nProababilmente il programma si inchioderà" );
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
        /// Metodo che conferma la vendita di un album fotografico su "CHIAVETTA" o su "MASTERIZZATORE" e possibile impostare un prezzo
        /// forfettario
        /// </summary>
        /// <param name="prezzoForfettario"></param>
        public Model.Carrello confermaVendita(decimal prezzoForfettario)
        {
            Carrello carrello;
            using (LumenEntities objContext = new LumenEntities())
            {
                carrello = new Carrello();
                carrello.id = Guid.NewGuid();
                carrello.giornata = DateTime.Today;
                carrello.tempo = DateTime.Now;
                carrello.totaleAPagare = prezzoForfettario;
                objContext.Carrelli.AddObject(carrello);
                objContext.SaveChanges();
            }
            return carrello;
        }

        /// <summary>
        /// Metodo che consenta la "MASTERIZZAZIONE" o la copia su "CHIAVETTA" delle foto
        /// </summary>
        public void masterizza()
        {
            masterizzaMsg = new MasterizzaMsg();
            switch(_tipoDestinazione){
                case TipoDestinazione.CARTELLA :
                    System.Diagnostics.Trace.WriteLine("INIZIO COPIA SU CARTELLA");
                    // Scarico in un thread separato per non bloccare l'applicazione
                    seNonPossoCopiareSpaccati();
                    _threadMasterizza = new Thread(copiaCartellaDestinazioneAsincrono);
                    _threadMasterizza.Start();
                    break;
                case TipoDestinazione.MASTERIZZATORE :
                    _burner = new BurnerSrvImpl();
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
        /// Metodo privato che consente di copiare i file sulla chiavetta in modo asincrono
        /// </summary>
        private void copiaCartellaDestinazioneAsincrono()
        {         
            // Pattern Unit-of-work
            using (LumenEntities objContext = new LumenEntities())
            {
                System.Diagnostics.Trace.WriteLine("COPIO SU CHIAVETTA!!!.....");
                masterizzaMsg = new MasterizzaMsg();
                copiaFile(_destinazione, false);
                masterizzaMsg.fase = Fase.MasterizzazioneCompletata;
                pubblicaMessaggio(masterizzaMsg);
            }
        }

        /// <summary>
        /// Metodo privato che consente di copiare i file all'interno di un path specificato
        /// </summary>
        ///  <param name="path"></param>
        ///  <param name="sovrascrivi"></param>
        private void copiaFile(String path, bool sovrascrivi){
            foreach (Fotografia fot in _listFotografie)
            {
                System.Diagnostics.Trace.WriteLine("Copia File " + fot.nomeFile);
                string nomeFileDest = "";
                try
                {
                    nomeFileDest = Path.Combine(path, Path.GetFileName(fot.nomeFile));
                    String nomeFileSorgente = configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile;
                    File.Copy(@nomeFileSorgente, nomeFileDest, sovrascrivi);
                    //Elimino gli attributi solo lettura                                
                    File.SetAttributes(nomeFileDest, File.GetAttributes(nomeFileDest) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));
                }
                catch (Exception ee)
                {
                    masterizzaMsg.riscontratiErrori = true;
                    _giornale.Error("Il file " + @configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile + " non è stato copiato il file sulla chiavetta " + nomeFileDest, ee);
                }
            }
        }

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
                // Se il tread di copia è ancora vivo, lo uccido
                if (_threadMasterizza != null)
                {
                    if (_threadMasterizza.IsAlive)
                        _threadMasterizza.Abort();
                    else
                        _threadMasterizza.Join();
                }

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
    }
}
