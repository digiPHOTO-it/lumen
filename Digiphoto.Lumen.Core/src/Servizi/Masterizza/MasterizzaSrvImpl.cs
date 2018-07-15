using System;
using System.Collections.Generic;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using log4net;
using System.Threading;
using System.IO;
using System.ComponentModel;
using Digiphoto.Lumen.Servizi.Masterizzare.MyBurner;
using Digiphoto.Lumen.Eventi;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Config;


namespace Digiphoto.Lumen.Servizi.Masterizzare
{
    public class MasterizzaSrvImpl : ServizioImpl, IMasterizzaSrv 
    {
        private static readonly ILog _giornale = LogManager.GetLogger(typeof(MasterizzaSrvImpl));

        private MasterizzaTarget _tipoDestinazione;

        private IList<Fotografia> _fotografie;

        private String _destinazione;

        
        private String _driverLetter;

        private BurnerSrvImpl _burner;

        private Thread _threadCopiaSuChiavetta;

        private BackgroundWorker backgroundWorkerCopia = null;

        private Fotografia fotCopia;

        private bool erroriCopia;

		private object senderTag;

		public override bool possoChiudere()
		{
			return !(backgroundWorkerCopia != null && backgroundWorkerCopia.WorkerSupportsCancellation == true) &&
				!(_threadCopiaSuChiavetta != null && _threadCopiaSuChiavetta.IsAlive);
		}

		public bool notificareProgressione {
			get;
			set;
		}

        public MasterizzaSrvImpl()
        {
            this._fotografie = new List<Fotografia>();
			
			notificareProgressione = false;			// per default risparmio tempo ed energia
		}

        ~MasterizzaSrvImpl()
        {
			// avviso se il thread di copia è ancora attivo
            if (backgroundWorkerCopia != null && backgroundWorkerCopia.WorkerSupportsCancellation == true)
            {
                backgroundWorkerCopia.CancelAsync();
            }
			if (_threadCopiaSuChiavetta != null && _threadCopiaSuChiavetta.IsAlive)
            {
                _giornale.Warn("Il thread di copia file è ancora attivo. Non è stata fatta la Join o la Abort.\nProababilmente il programma si inchioderà");
            }
		}

        private void seNonPossoCopiareSpaccati()
        {
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
        public void impostaDestinazione( MasterizzaTarget tipoDestinazione, String destinazione)
        {
            this._tipoDestinazione = tipoDestinazione;
            switch (_tipoDestinazione)
            {
                case MasterizzaTarget.Cartella:
                    this._destinazione = destinazione;
                    if (!Directory.Exists(_destinazione))
                    {
                        Directory.CreateDirectory(_destinazione);
                    }
                    break;
                case MasterizzaTarget.Masterizzatore:
                    this._driverLetter = destinazione;
                    break;
            }
        }


        /// <summary>
        /// Metodo che consenta la "MASTERIZZAZIONE" o la copia su "CHIAVETTA" delle foto
        /// </summary>
        public void masterizza() {
		}

		public void masterizza( Guid idCarrello )  {

			senderTag = idCarrello;

            switch(_tipoDestinazione){
                case MasterizzaTarget.Cartella :
                    // Scarico in un thread separato per non bloccare l'applicazione
                    seNonPossoCopiareSpaccati();
                    _threadCopiaSuChiavetta = new Thread(copiaCartellaDestinazioneAsincrono);
                    _threadCopiaSuChiavetta.Start();
                    break;
                case MasterizzaTarget.Masterizzatore :
                    _burner = new BurnerSrvImpl();
                    _burner.InviaStatoMasterizzazione += new BurnerSrvImpl.StatoMasterizzazioneEventHandler(inviaMessaggioStatoMasterizzazione);
                    _burner.start();
                    foreach (Fotografia fot in _fotografie)
                    {
                        _burner.addFileToBurner(PathUtil.nomeCompletoVendita(fot));
                    }
                    _burner.setDiscRecorder(_driverLetter);

                    if(_burner.testMedia()){
                        //Imposto l'etichetta del CD
                        _burner.etichetta = DateTime.Now.ToString("dd MMM yyyy");
						if (_burner.CapacitaResidua() < 0)
						{
							MasterizzaMsg errorTestMediaMsg = new MasterizzaMsg(this);
							errorTestMediaMsg.senderTag = senderTag;
							errorTestMediaMsg.fase = Fase.ErroreSpazioDisco;
							errorTestMediaMsg.esito = Esito.Errore;
							errorTestMediaMsg.progress = 0;
							errorTestMediaMsg.result = "Capacita del disco superata!!!";
							pubblicaMessaggio(errorTestMediaMsg);
						}
						else
						{
							
							_burner.burning();
						}
                    }else{
						MasterizzaMsg errorTestMediaMsg = new MasterizzaMsg( this );
						errorTestMediaMsg.senderTag = senderTag;
						errorTestMediaMsg.fase = Fase.ErroreMedia;
                        errorTestMediaMsg.esito = Esito.Errore;
                        errorTestMediaMsg.progress = 0;
                        errorTestMediaMsg.result = "Error Media";
                        pubblicaMessaggio(errorTestMediaMsg);
						throw new Exception("Errore supporto\n"+
											"Molto probabilmente non è stato inserito il CD nell'unita o la capacita del supporto non è sufficiente!!"+ 
											"\n\nVerifica il supporto e prova a rimasterizzare usando il pulsante"+
											"\n\nRimasterizza nuovamente causa eventuali errori"+
											"\n\nChe si trova alla tua Sinistra!!!!");
                    }
                    break;
				default:
					throw new ArgumentException( "TipoDestinazione non indicato o non supportato: " + _tipoDestinazione );
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
            using (LumenEntities objContext = UnitOfWorkScope.currentDbContext)
            {
                _giornale.Info("Inizio copia " + _fotografie.Count + " foto su cartella");
                MasterizzaMsg inizioCopiaMsg = new MasterizzaMsg( this );
				inizioCopiaMsg.senderTag = senderTag;
				inizioCopiaMsg.fase = Fase.InizioCopia;
                inizioCopiaMsg.progress = 0;
                inizioCopiaMsg.result = "Inizio Copia Su Chiavetta";
				inizioCopiaMsg.cartella = _destinazione;

				pubblicaMessaggio(inizioCopiaMsg);

                totFotoCopiate = 0;
                totFotoNonCopiate = 0;
                bool errori = false;
                foreach (Fotografia fot in _fotografie)
                {
                    string nomeFileDest = "";
                    try
                    {  
                        nomeFileDest = Path.Combine(_destinazione, Path.GetFileName(fot.nomeFile));
                        String nomeFileSorgente = PathUtil.nomeCompletoVendita( fot );
                        File.Copy(@nomeFileSorgente, nomeFileDest, true);
                        //Elimino gli attributi solo lettura                                
                        File.SetAttributes(nomeFileDest, File.GetAttributes(nomeFileDest) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));

						++totFotoCopiate;

						if( notificareProgressione ) {
							MasterizzaMsg statoCopiaMsg = new MasterizzaMsg( this );
							statoCopiaMsg.senderTag = senderTag;
							statoCopiaMsg.esito = Esito.Ok;
							statoCopiaMsg.fotoAggiunta = 1;
							statoCopiaMsg.totFotoAggiunte = totFotoCopiate;
							statoCopiaMsg.totFotoNonAggiunte = totFotoNonCopiate;
							statoCopiaMsg.progress = ((totFotoCopiate + totFotoNonCopiate) * 100) / _fotografie.Count;
							statoCopiaMsg.result = "Il File " + fot.nomeFile + " è statoScarica copiato sulla chiavetta con successo";
							statoCopiaMsg.cartella = _destinazione;
							pubblicaMessaggio( statoCopiaMsg );
						}
                    }
                    catch (IOException ee)
                    {
						++totFotoNonCopiate;

						MasterizzaMsg statoCopiaErroreMsg = new MasterizzaMsg( this );
						statoCopiaErroreMsg.senderTag = senderTag;
                        statoCopiaErroreMsg.esito = Esito.Errore;
                        errori = true;
                        statoCopiaErroreMsg.fotoAggiunta = 0;
                        statoCopiaErroreMsg.totFotoAggiunte = totFotoCopiate;
                        statoCopiaErroreMsg.totFotoNonAggiunte = totFotoNonCopiate;
                        statoCopiaErroreMsg.progress = ((totFotoCopiate + totFotoNonCopiate) * 100) / _fotografie.Count;
                        statoCopiaErroreMsg.result = "Il file " + fot.nomeFile + " non è statoScarica copiato sulla chiavetta: " + nomeFileDest;
                        _giornale.Error("Il file " + Configurazione.cartellaRepositoryFoto + Path.DirectorySeparatorChar + fot.nomeFile + " non è statoScarica copiato il file sulla chiavetta " + nomeFileDest, ee);
                        pubblicaMessaggio(statoCopiaErroreMsg);
                    }
                }

				_giornale.Info( "Fine copia " + totFotoCopiate + "/" + _fotografie.Count + " foto su cartella" );

				MasterizzaMsg copiaCompletataMsg = new MasterizzaMsg( this );
				copiaCompletataMsg.senderTag = senderTag;
				copiaCompletataMsg.fase = errori ? Fase.ErroreMedia : Fase.CopiaCompletata;
				copiaCompletataMsg.esito = errori ? Esito.Errore : Esito.Ok;
				copiaCompletataMsg.result = errori ? "Riscontrati errori" : "Copia Completata con Successo";
				copiaCompletataMsg.progress = 100;
                copiaCompletataMsg.totFotoNonAggiunte = totFotoNonCopiate;
                copiaCompletataMsg.totFotoAggiunte = totFotoCopiate;
				copiaCompletataMsg.cartella = _destinazione;
				this.isCompletato = true;
                pubblicaMessaggio(copiaCompletataMsg);
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
            using (LumenEntities objContext = UnitOfWorkScope.currentDbContext)
            {
                System.Diagnostics.Trace.WriteLine("INIZIO COPIA SU CARTELLA");
                MasterizzaMsg inizioCopiaMsg = new MasterizzaMsg( this );
                inizioCopiaMsg.fase = Fase.InizioCopia;
                inizioCopiaMsg.progress = 0;
                inizioCopiaMsg.result = "Inizio Copia Su Chiavetta";
                pubblicaMessaggio(inizioCopiaMsg);

                this.totFotoCopiate = 0;
                this.totFotoNonCopiate = 0;
                erroriCopia = false;
                foreach (Fotografia fot in _fotografie)
                {
                    fotCopia = fot;
                    string nomeFileDest = "";
                    try
                    {
                        nomeFileDest = Path.Combine(_destinazione, Path.GetFileName(fot.nomeFile));
                        String nomeFileSorgente = PathUtil.nomeCompletoVendita(fot);
                        File.Copy(@nomeFileSorgente, nomeFileDest, false);
                        //Elimino gli attributi solo lettura                                
                        File.SetAttributes(nomeFileDest, File.GetAttributes(nomeFileDest) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));

						if( notificareProgressione ) {
							//MasterizzaMsg statoCopiaMsg = new MasterizzaMsg();
							//statoCopiaMsg.riscontratiErrori = false;
							//statoCopiaMsg.fotoAggiunta = 1;
							//statoCopiaMsg.totFotoAggiunte = ++countFotoAggiunte;
							//statoCopiaMsg.result = "Copia File " + fot.nomeFile;
						
							backgroundWorkerCopia.ReportProgress(++this.totFotoCopiate / _fotografie.Count * 100);
						}
					}
                    catch (IOException ee)
                    {
						_giornale.Debug( "errore copia foto: " + fot, ee );
                        //MasterizzaMsg statoCopiaErroreMsg = new MasterizzaMsg();
                        //statoCopiaErroreMsg.riscontratiErrori = true;
                        erroriCopia = true;
                        //statoCopiaErroreMsg.totFotoNonAggiunte = ++countFotoNonAggiunte;
                        //statoCopiaErroreMsg.result = "Il file " + @configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile + " non è statoScarica copiato il file sulla chiavetta " + nomeFileDest;
                        //_giornale.Error("Il file " + @configurazione.getCartellaRepositoryFoto() + Path.DirectorySeparatorChar + fot.nomeFile + " non è statoScarica copiato il file sulla chiavetta " + nomeFileDest, ee);
                        backgroundWorkerCopia.ReportProgress(++totFotoNonCopiate / _fotografie.Count * 100);
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
            MasterizzaMsg statoCopiaMsg = new MasterizzaMsg( this );
            if (erroriCopia)
            {
                statoCopiaMsg.esito = Esito.Errore;           
                statoCopiaMsg.fotoAggiunta = 0;
                statoCopiaMsg.result = "Il File " + fotCopia.nomeFile + " non è statoScarica copiato sulla chiavetta";
            }
            else
            {
                statoCopiaMsg.esito = Esito.Ok;
                statoCopiaMsg.fotoAggiunta = 1;
                statoCopiaMsg.result = "Il File " + fotCopia.nomeFile + " è statoScarica copiato sulla chiavetta con successo";
            }
            statoCopiaMsg.totFotoAggiunte = this.totFotoNonCopiate;
            statoCopiaMsg.totFotoNonAggiunte = this.totFotoCopiate;
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
            MasterizzaMsg statoCopiaCompletatoMsg = new MasterizzaMsg( this );
            statoCopiaCompletatoMsg.esito = erroriCopia ? Esito.Errore : Esito.Ok;
			statoCopiaCompletatoMsg.fase = erroriCopia ? Fase.ErroreMedia : Fase.CopiaCompletata;
            statoCopiaCompletatoMsg.totFotoNonAggiunte = totFotoNonCopiate;
            statoCopiaCompletatoMsg.totFotoAggiunte = totFotoCopiate;
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

  

		public void addFotografia( Fotografia foto ) {
			if( !fotografie.Contains( foto ) )
				fotografie.Add( foto );
		}

		public IList<Fotografia> fotografie {
			get {
				return _fotografie;
			}
		}

		#endregion

		#region NotificaMessaggiBurner
		private void inviaMessaggioStatoMasterizzazione(object sender, BurnerMsg burnerMsg)
        {
			MasterizzaMsg masterizzaMsg = new MasterizzaMsg(this);
			masterizzaMsg.totFotoAggiunte = burnerMsg.totaleFileAggiunti;
			masterizzaMsg.totFotoNonAggiunte = 0;
			masterizzaMsg.esito = Esito.Ok;
			switch (burnerMsg.fase)
			{
				case Digiphoto.Lumen.Servizi.Masterizzare.MyBurner.Fase.ErrorMedia:
					masterizzaMsg.esito = Esito.Errore;
					masterizzaMsg.fase = Fase.ErroreMedia;
					break;
				case Digiphoto.Lumen.Servizi.Masterizzare.MyBurner.Fase.MasterizzazioneIniziata:
					masterizzaMsg.fase = Fase.InizioCopia;
					break;
				case Digiphoto.Lumen.Servizi.Masterizzare.MyBurner.Fase.FormattazioneIniziata:
					masterizzaMsg.fase = Fase.InizioCopia;
					break;
				case Digiphoto.Lumen.Servizi.Masterizzare.MyBurner.Fase.Completed:
					masterizzaMsg.fase = Fase.CopiaCompletata;
					break;
				case Digiphoto.Lumen.Servizi.Masterizzare.MyBurner.Fase.MasterizzazioneCompletata:
					
					masterizzaMsg.fase = Fase.CopiaCompletata;
					break;
				case Digiphoto.Lumen.Servizi.Masterizzare.MyBurner.Fase.FormattazioneCompletata:
					masterizzaMsg.fase = Fase.CopiaCompletata;
					break;
				case Digiphoto.Lumen.Servizi.Masterizzare.MyBurner.Fase.MasterizzazioneFallita:
					masterizzaMsg.esito = Esito.Errore;
					masterizzaMsg.fase = Fase.ErroreMedia;
					break;
				case Digiphoto.Lumen.Servizi.Masterizzare.MyBurner.Fase.FormattazioneFallita:
					masterizzaMsg.esito = Esito.Errore;
					masterizzaMsg.fase = Fase.ErroreMedia;
					break;
			}

			masterizzaMsg.result = burnerMsg.statusMessage;
			masterizzaMsg.progress = burnerMsg.progress;
			pubblicaMessaggio(masterizzaMsg);
        }
        #endregion
		
		protected override void Dispose( bool disposing )
        {

            System.Diagnostics.Trace.WriteLine("DISPOSE");
            try
            {
                // Se il tread di copia è ancora vivo, lo uccido
                if (_threadCopiaSuChiavetta != null)
                {
                    System.Diagnostics.Trace.WriteLine("VIVO");
                    if (_threadCopiaSuChiavetta.IsAlive)
                        _threadCopiaSuChiavetta.Abort();
                    else
                        _threadCopiaSuChiavetta.Join();
                }
            }
            finally
            {
				isCompletato = true;

                if (_burner != null)
                {
                    _burner.Dispose();
                }
                base.Dispose( disposing );
            }
        }


		public void addFotografie( IEnumerable<Fotografia> fotos ) {
			foreach( Fotografia foto in fotos )
				this.fotografie.Add( foto );
		}
		

		public bool isCompletato {
			get;
			private set;
		}

		public int totFotoCopiate {
			get;
			private set;
		}

		public int totFotoNonCopiate {
			get;
			private set;
		}


		public decimal prezzoForfaittario {
			get;
			set;
		}
	}
}
