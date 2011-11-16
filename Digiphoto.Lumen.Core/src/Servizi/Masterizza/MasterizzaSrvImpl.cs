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
                    break;
            }
        }

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
                    BurnerSrvImpl _impl = new BurnerSrvImpl();
                    foreach (Fotografia fot in _listFotografie)
                    {
                        _impl.addFileToBurner(configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar +fot.nomeFile);
                    }
                    _impl.setDiscRecorder(@_driverLetter);
                     _impl.testMedia();
                    _impl.etichetta = "Test";
                    _impl.burning();
                    _impl.Dispose();
                    masterizzaMsg.fase = Fase.MasterizzazioneCompletata;
                    pubblicaMessaggio(masterizzaMsg);
                    break;
            }
        }

        private void copiaCartellaDestinazioneAsincrono()
        {         
            // Pattern Unit-of-work
            using (LumenEntities objContext = new LumenEntities())
            {
                System.Diagnostics.Trace.WriteLine("COPIO.....");
                masterizzaMsg = new MasterizzaMsg();
                copiaFile(_destinazione, false);
                masterizzaMsg.fase = Fase.MasterizzazioneCompletata;
                pubblicaMessaggio(masterizzaMsg);
            }
        }

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
                   // _giornale.Error("Il file " + @configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile + " non è stato copiato nella stagin Area " + nomeFileDest, ee);
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

        public bool Remove(Model.Fotografia item)
        {
            return _listFotografie.Remove(item);
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
            }
        }
    }
}
