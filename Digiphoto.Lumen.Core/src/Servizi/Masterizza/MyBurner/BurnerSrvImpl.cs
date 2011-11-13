using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using IMAPI2.Interop;
using Digiphoto.Lumen.Servizi;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using IMAPI2.MediaItem;
using System.Collections;
using Digiphoto.Lumen.Model;
using System.IO;
using Digiphoto.Lumen.Servizi.Masterizza.MyBurner;

namespace Digiphoto.Lumen.Servizi.Masterizza.MyBurner
{
    public class BurnerSrvImpl : ServizioImpl, IBurnerSrv
    {
        private const string ClientName = "BurnMedia";

        public string etichetta {get;set;}

        private bool quickFormat{get;set;}

        Int64 _totalDiscSize = 0;

        private bool _isBurning = false;
        private bool _isFormatting = false;
        private IMAPI_BURN_VERIFICATION_LEVEL _verificationLevel = IMAPI_BURN_VERIFICATION_LEVEL.IMAPI_BURN_VERIFICATION_NONE;
        private bool closeMedia { get; set; }
        //Espello il disco
        public bool ejectMedia {get; set;}

        private BurnData _burner = new BurnData();

        private BackgroundWorker backgroundBurnWorker;

        private BackgroundWorker backgroundFormatWorker;

        MsftDiscMaster2 discMaster = null;

        private IList<MsftDiscRecorder2> _listRecorders = new List<MsftDiscRecorder2>();

        private IList listaFileDaMasterizzare = new List<FileItem>();

        private IDiscRecorder2 discRecorder = null;

        public BurnerSrvImpl(){
            try
            {
                discMaster = new MsftDiscMaster2();

                if (!discMaster.IsSupportedEnvironment)
                    return;

                //Creo la lista con i masterizzatori
                foreach (string uniqueRecorderId in discMaster)
                {
                    var discRecorder2 = new MsftDiscRecorder2();
                    discRecorder2.InitializeDiscRecorder(uniqueRecorderId);
                    System.Diagnostics.Trace.WriteLine("[Volume]: "+discRecorder2.VolumePathNames.GetValue(0));
                    _listRecorders.Add(discRecorder2);
                }

                // Setto alcuni valori di Default
                ejectMedia = true;
                closeMedia = false;
                quickFormat = true;
                discRecorder = _listRecorders.First();
            }
            catch (COMException ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, string.Format("Error:{0} - Please install IMAPI2", ex.ErrorCode));
                return;
            }
            finally
            {
                if (discMaster != null)
                {
                    Marshal.ReleaseComObject(discMaster);
                }
            }
        }

        ~BurnerSrvImpl()
        {
            //
            // Release the disc recorder items
            //
            foreach (MsftDiscRecorder2 discRecorder2 in _listRecorders)
            {
                if (discRecorder2 != null)
                {
                    Marshal.ReleaseComObject(discRecorder2);
                }
            }
		}

        /// <summary>
        /// Lista degli elementi da masterizzare
        /// </summary>
        /// <param name="pathFile"></param>
        public void addFileToBurner(String pathFile)
        {
            // Carico i file da masterizzare
            var fileItem = new FileItem(pathFile);
            listaFileDaMasterizzare.Add(fileItem);
        }

        /// <summary>
        /// Recupero la Lista dei possibili masterizzatori
        /// </summary>
        public IList<MsftDiscRecorder2> listaMasterizzatori()
        {
            return _listRecorders;
        }

        /// <summary>
        /// Setto il masterizzatore sul quale scrivere
        /// </summary>
        /// <param name="discRecorder"></param>
        public void setDiscRecorder(IDiscRecorder2 discRecorder){
            this.discRecorder = discRecorder;
        }

        /// <summary>
        /// Setto il masterizzatore sul quale scrivere
        /// </summary>
        /// <param name="volume"></param>
        public void setDiscRecorder(String volume)
        {
            foreach(IDiscRecorder2 discRecorder in _listRecorders){
                if (discRecorder.VolumePathNames.GetValue(0).Equals(volume))
                {
                    this.discRecorder = discRecorder;
                    break;
                }
            }
        }

        /// <summary>
        /// Verifico se il supporto può essere utilizzato per masterizzare
        /// </summary>
        public void testMedia()
        {
            MsftFileSystemImage fileSystemImage = null;
            MsftDiscFormat2Data discFormatData = null;
            try
            {
                //
                // Create and initialize the IDiscFormat2Data
                //
                discFormatData = new MsftDiscFormat2Data();
                if (!discFormatData.IsCurrentMediaSupported(discRecorder))
                {
                    System.Diagnostics.Trace.WriteLine("Media not supported!");
                    _totalDiscSize = 0;
                    return;
                }
                else
                {
                    //
                    // Get the media type in the recorder
                    //
                    discFormatData.Recorder = this.discRecorder;
                    IMAPI_MEDIA_PHYSICAL_TYPE mediaType = discFormatData.CurrentPhysicalMediaType;
                    System.Diagnostics.Trace.WriteLine("Etichetta Media: "+GetMediaTypeString(mediaType));

                    //
                    // Create a file system and select the media type
                    //
                    fileSystemImage = new MsftFileSystemImage();
                    fileSystemImage.ChooseImageDefaultsForMediaType(mediaType);

                    //
                    // See if there are other recorded sessions on the disc
                    //
                    if (!discFormatData.MediaHeuristicallyBlank)
                    {
                        fileSystemImage.MultisessionInterfaces = discFormatData.MultisessionInterfaces;
                        fileSystemImage.ImportFileSystem();
                    }

                    Int64 freeMediaBlocks = fileSystemImage.FreeMediaBlocks;
                    _totalDiscSize = 2048 * freeMediaBlocks;
                }
            }
            catch (COMException exception)
            {
                System.Diagnostics.Trace.WriteLine("Detect Media Error "+exception.Message);
            }
            finally
            {
                if (discFormatData != null)
                {
                    Marshal.ReleaseComObject(discFormatData);
                }

                if (fileSystemImage != null)
                {
                    Marshal.ReleaseComObject(fileSystemImage);
                }
            }
            UpdateCapacity();
        }

        /// <summary>
        /// Aggiorna la capacità rimanente nel disco
        /// </summary>
        private String UpdateCapacity()
        {
            string capacity = "OMB";
            //
            // Get the text for the Max Size
            //
            if (_totalDiscSize == 0)
            {
                return capacity = "0MB";
            }

            capacity = _totalDiscSize < 1000000000 ?
                string.Format("{0}MB", _totalDiscSize / 1000000) :
                string.Format("{0:F2}GB", (float)_totalDiscSize / 1000000000.0);

            //
            // Calculate the size of the files
            //
            Int64 totalMediaSize = 0;
            foreach (IMediaItem mediaItem in listaFileDaMasterizzare)
            {
                totalMediaSize += mediaItem.SizeOnDisc;
            }
            BurnerMsg burnerMsg= new BurnerMsg();
            burnerMsg.capacity = capacity;
            pubblicaMessaggio(burnerMsg);
            return capacity;
            // Aggiornamento grafica

            //if (totalMediaSize == 0)
            //{
            //    progressBarCapacity.Value = 0;
            //    progressBarCapacity.ForeColor = SystemColors.Highlight;
            //}
            //else
            //{
            //    var percent = (int)((totalMediaSize * 100) / _totalDiscSize);
            //    if (percent > 100)
            //    {
            //        progressBarCapacity.Value = 100;
            //        progressBarCapacity.ForeColor = Color.Red;
            //    }
            //    else
            //    {
            //        progressBarCapacity.Value = percent;
            //        progressBarCapacity.ForeColor = SystemColors.Highlight;
            //    }
            //}
        }

        #region Burning

        public void burning()
        {
            BurnerMsg burnerMsg = new BurnerMsg();
            burnerMsg.fase = Fase.MasterizzazioneIniziata;
            pubblicaMessaggio(burnerMsg);

            _burner.uniqueRecorderId = this.discRecorder.ActiveDiscRecorder;

            System.Diagnostics.Trace.WriteLine("Inizio Masterizzazione");

            backgroundBurnWorker = new BackgroundWorker();
            backgroundBurnWorker.DoWork += backgroundBurnWorker_DoWork;
            backgroundBurnWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundBurnWorker_ProgressChanged);
            backgroundBurnWorker.RunWorkerCompleted += backgroundBurnWorker_RunWorkerCompleted;
            backgroundBurnWorker.RunWorkerAsync(_burner);
        }

        /// <summary>
        /// The thread that does the burning of the media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundBurnWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("WORKER");
            MsftDiscRecorder2 discRecorder = null;
            MsftDiscFormat2Data discFormatData = null;
            BurnerMsg burnerMsg = new BurnerMsg();
            try
            {
                //
                // Create and initialize the IDiscRecorder2 object
                //
                discRecorder = new MsftDiscRecorder2();
                var burnData = (BurnData)e.Argument;
                discRecorder.InitializeDiscRecorder(burnData.uniqueRecorderId);

                //
                // Create and initialize the IDiscFormat2Data
                //
                discFormatData = new MsftDiscFormat2Data
                {
                    Recorder = discRecorder,
                    ClientName = ClientName,
                    ForceMediaToBeClosed = closeMedia
                };

                //
                // Set the verification level
                //
                var burnVerification = (IBurnVerification)discFormatData;
                burnVerification.BurnVerificationLevel = _verificationLevel;

                //
                // Check if media is blank, (for RW media)
                //
                object[] multisessionInterfaces = null;
                if (!discFormatData.MediaHeuristicallyBlank)
                {
                    multisessionInterfaces = discFormatData.MultisessionInterfaces;
                }

                //
                // Create the file system
                //
                IStream fileSystem;
                if (!CreateMediaFileSystem(discRecorder, multisessionInterfaces, out fileSystem))
                {
                    e.Result = -1;
                    return;
                }

                //
                // add the Update event handler
                //
                discFormatData.Update += discFormatData_Update;

                //
                // Write the data here
                //
                try
                {
                    discFormatData.Write(fileSystem);
                    e.Result = 0;
                }
                catch (COMException ex)
                {
                    e.Result = ex.ErrorCode;
                    burnerMsg.fase = Fase.MasterizzazioneFallita;
                    pubblicaMessaggio(burnerMsg);
                    System.Diagnostics.Trace.WriteLine(ex.Message, "IDiscFormat2Data.Write failed");
                }
                finally
                {
                    if (fileSystem != null)
                    {
                        Marshal.FinalReleaseComObject(fileSystem);
                    }
                }

                //
                // remove the Update event handler
                //
                discFormatData.Update -= discFormatData_Update;

                if (ejectMedia)
                {
                    discRecorder.EjectMedia();
                }
            }
            catch (COMException exception)
            {
                //
                // If anything happens during the format, show the message
                //
                System.Diagnostics.Trace.WriteLine(exception.Message);
                e.Result = exception.ErrorCode;
            }
            finally
            {
                if (discRecorder != null)
                {
                    Marshal.ReleaseComObject(discRecorder);
                }

                if (discFormatData != null)
                {
                    Marshal.ReleaseComObject(discFormatData);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progress"></param>
        void discFormatData_Update([In, MarshalAs(UnmanagedType.IDispatch)] object sender, [In, MarshalAs(UnmanagedType.IDispatch)] object progress)
        {
            //
            // Check if we've cancelled
            //
            if (backgroundBurnWorker.CancellationPending)
            {
                var format2Data = (IDiscFormat2Data)sender;
                format2Data.CancelWrite();
                return;
            }

            var eventArgs = (IDiscFormat2DataEventArgs)progress;

            _burner.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING;

            // IDiscFormat2DataEventArgs Interface
            _burner.elapsedTime = eventArgs.ElapsedTime;
            _burner.remainingTime = eventArgs.RemainingTime;
            _burner.totalTime = eventArgs.TotalTime;

            // IWriteEngine2EventArgs Interface
            _burner.currentAction = eventArgs.CurrentAction;
            _burner.startLba = eventArgs.StartLba;
            _burner.sectorCount = eventArgs.SectorCount;
            _burner.lastReadLba = eventArgs.LastReadLba;
            _burner.lastWrittenLba = eventArgs.LastWrittenLba;
            _burner.totalSystemBuffer = eventArgs.TotalSystemBuffer;
            _burner.usedSystemBuffer = eventArgs.UsedSystemBuffer;
            _burner.freeSystemBuffer = eventArgs.FreeSystemBuffer;

            //
            // Report back to the UI
            //
            backgroundBurnWorker.ReportProgress(0, _burner);
        }

        private bool CreateMediaFileSystem(IDiscRecorder2 discRecorder, object[] multisessionInterfaces, out IStream dataStream)
        {
            MsftFileSystemImage fileSystemImage = null;
            try
            {
                fileSystemImage = new MsftFileSystemImage();
                fileSystemImage.ChooseImageDefaults(discRecorder);
                fileSystemImage.FileSystemsToCreate = FsiFileSystems.FsiFileSystemJoliet | FsiFileSystems.FsiFileSystemISO9660;
                System.Diagnostics.Trace.WriteLine("Etichetta: "+this.etichetta);
                fileSystemImage.VolumeName = this.etichetta;

                fileSystemImage.Update += fileSystemImage_Update;

                //
                // If multisessions, then import previous sessions
                //
                if (multisessionInterfaces != null)
                {
                    fileSystemImage.MultisessionInterfaces = multisessionInterfaces;
                    fileSystemImage.ImportFileSystem();
                }

                //
                // Get the image root
                //
                IFsiDirectoryItem rootItem = fileSystemImage.Root;

                //
                // Add Files and Directories to File System Image
                //
                foreach (IMediaItem mediaItem in listaFileDaMasterizzare)
                {
                    //
                    // Check if we've cancelled
                    //
                    if (backgroundBurnWorker.CancellationPending)
                    {
                        break;
                    }

                    //
                    // Add to File System
                    //
                    mediaItem.AddToFileSystem(rootItem);
                }

                fileSystemImage.Update -= fileSystemImage_Update;

                //
                // did we cancel?
                //
                if (backgroundBurnWorker.CancellationPending)
                {
                    dataStream = null;
                    return false;
                }

                dataStream = fileSystemImage.CreateResultImage().ImageStream;
            }
            catch (COMException exception)
            {
                System.Diagnostics.Trace.WriteLine("FileSystem" + exception.Message);
                //MessageBox.Show(this, exception.Message, "Create File System Error",
                //    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataStream = null;
                return false;
            }
            finally
            {
                if (fileSystemImage != null)
                {
                    Marshal.ReleaseComObject(fileSystemImage);
                }
            }

            return true;
        }

        void fileSystemImage_Update([In, MarshalAs(UnmanagedType.IDispatch)] object sender, [In, MarshalAs(UnmanagedType.BStr)]string currentFile, [In] int copiedSectors, [In] int totalSectors)
        {
            var percentProgress = 0;
            if (copiedSectors > 0 && totalSectors > 0)
            {
                percentProgress = (copiedSectors * 100) / totalSectors;
            }

            if (!string.IsNullOrEmpty(currentFile))
            {
                var fileInfo = new FileInfo(currentFile);
                _burner.statusMessage = "Adding \"" + fileInfo.Name + "\" to image...";

                //
                // report back to the ui
                //
                _burner.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM;
                backgroundBurnWorker.ReportProgress(percentProgress, _burner);
            }

        }

        /// <summary>
        /// Event receives notification from the Burn thread of an event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundBurnWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BurnerMsg burnerMsg = new BurnerMsg();
            //int percent = e.ProgressPercentage;
            var burnData = (BurnData)e.UserState;

            if (burnData.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM)
            {
                System.Diagnostics.Trace.WriteLine("[0]:" + burnData.statusMessage);
                burnerMsg.statusMessage = burnData.statusMessage;
                pubblicaMessaggio(burnerMsg);
            }
            else if (burnData.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING)
            {
                switch (burnData.currentAction)
                {
                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VALIDATING_MEDIA:
                        System.Diagnostics.Trace.WriteLine("[0]:" + "Validating current media...");
                        burnerMsg.fase = Fase.ValidatingCurrentMedia;
                        pubblicaMessaggio(burnerMsg);
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FORMATTING_MEDIA:
                        System.Diagnostics.Trace.WriteLine("[0]:" + "Formatting media...");
                        burnerMsg.fase = Fase.FormattingMedia;
                        pubblicaMessaggio(burnerMsg);
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_INITIALIZING_HARDWARE:
                        System.Diagnostics.Trace.WriteLine("[0]:" + "Initializing hardware...");
                        burnerMsg.fase = Fase.InitializingHardware;
                        pubblicaMessaggio(burnerMsg);
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_CALIBRATING_POWER:
                        System.Diagnostics.Trace.WriteLine("[0]:" + "Optimizing laser intensity...");
                        burnerMsg.fase = Fase.OptimizingLaserIntensity;
                        pubblicaMessaggio(burnerMsg);
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_WRITING_DATA:
                        long writtenSectors = burnData.lastWrittenLba - burnData.startLba;

                        if (writtenSectors > 0 && burnData.sectorCount > 0)
                        {
                            var percent = (int)((100 * writtenSectors) / burnData.sectorCount);
                            System.Diagnostics.Trace.WriteLine("[0]:" + string.Format("Progress: {0}%", percent));
                            burnerMsg.progress = percent;
                            pubblicaMessaggio(burnerMsg);
                        }
                        else
                        {
                            System.Diagnostics.Trace.WriteLine("[0]:" + "Progress 0%");
                            burnerMsg.progress = 0;
                            pubblicaMessaggio(burnerMsg);
                        }
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FINALIZATION:
                        System.Diagnostics.Trace.WriteLine("[0]:" + "Finalizing writing...");
                        burnerMsg.fase = Fase.FinalizingWriting;
                        pubblicaMessaggio(burnerMsg);
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_COMPLETED:
                        System.Diagnostics.Trace.WriteLine("[0]:" + "Completed!");
                        burnerMsg.fase = Fase.Completed;
                        pubblicaMessaggio(burnerMsg);
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VERIFYING:
                        System.Diagnostics.Trace.WriteLine("[0]:" + "Verifying");
                        burnerMsg.fase = Fase.Verifying;
                        pubblicaMessaggio(burnerMsg);
                        break;
                }
            }
        }

        /// <summary>
        /// Completed the "Burn" thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundBurnWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BurnerMsg burnerMsg = new BurnerMsg();
            System.Diagnostics.Trace.WriteLine((int)e.Result == 0 ? "Finished Burning Disc!" : "Error Burning Disc!");
            burnerMsg.fase = (int)e.Result == 0 ? Fase.MasterizzazioneCompletata : Fase.MasterizzazioneFallita;
            pubblicaMessaggio(burnerMsg);
            _isBurning = false;
        }
        #endregion

        #region Formatting

        public void formatting()
        {
            BurnerMsg burnerMsg = new BurnerMsg();
            burnerMsg.fase = Fase.FormattazioneIniziata;
            pubblicaMessaggio(burnerMsg);

            System.Diagnostics.Trace.WriteLine("Inizio Formattazione");

            backgroundFormatWorker = new BackgroundWorker();
            backgroundFormatWorker.DoWork += backgroundFormatWorker_DoWork;
            backgroundFormatWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundFormatWorker_ProgressChanged);
            backgroundFormatWorker.RunWorkerCompleted += backgroundFormatWorker_RunWorkerCompleted;
            backgroundFormatWorker.RunWorkerAsync(discRecorder.ActiveDiscRecorder);
        }

        /// <summary>
        /// Worker thread that Formats the Disc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundFormatWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MsftDiscRecorder2 discRecorder = null;
            MsftDiscFormat2Erase discFormatErase = null;
            BurnerMsg burnerMsg = new BurnerMsg();
            try
            {
                //
                // Create and initialize the IDiscRecorder2
                //
                discRecorder = new MsftDiscRecorder2();
                var activeDiscRecorder = (string)e.Argument;
                discRecorder.InitializeDiscRecorder(activeDiscRecorder);

                //
                // Create the IDiscFormat2Erase and set properties
                //
                discFormatErase = new MsftDiscFormat2Erase
                    {
                        Recorder = discRecorder,
                        ClientName = ClientName,
                        FullErase = !quickFormat
                    };

                //
                // Setup the Update progress event handler
                //
                discFormatErase.Update += discFormatErase_Update;

                //
                // Erase the media here
                //
                try
                {
                    discFormatErase.EraseMedia();
                    e.Result = 0;
                }
                catch (COMException ex)
                {
                    e.Result = ex.ErrorCode;
                    System.Diagnostics.Trace.WriteLine("IDiscFormat2.EraseMedia failed "+ex.Message);
                    burnerMsg.fase = Fase.FormattazioneFallita;
                    pubblicaMessaggio(burnerMsg);
                }

                //
                // Remove the Update progress event handler
                //
                discFormatErase.Update -= discFormatErase_Update;

                //
                // Eject the media 
                //
                if (ejectMedia)
                {
                    discRecorder.EjectMedia();
                }
                burnerMsg.fase = Fase.FormattazioneCompletata;
                pubblicaMessaggio(burnerMsg);
            }
            catch (COMException exception)
            {
                //
                // If anything happens during the format, show the message
                //
                System.Diagnostics.Trace.WriteLine(exception.Message);
                burnerMsg.fase = Fase.FormattazioneFallita;
                pubblicaMessaggio(burnerMsg);
            }
            finally
            {
                if (discRecorder != null)
                {
                    Marshal.ReleaseComObject(discRecorder);
                }

                if (discFormatErase != null)
                {
                    Marshal.ReleaseComObject(discFormatErase);
                }
            }
        }

        /// <summary>
        /// Event Handler for the Erase Progress Updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedSeconds"></param>
        /// <param name="estimatedTotalSeconds"></param>
        void discFormatErase_Update([In, MarshalAs(UnmanagedType.IDispatch)] object sender, int elapsedSeconds, int estimatedTotalSeconds)
        {
            var percent = elapsedSeconds * 100 / estimatedTotalSeconds;
            //
            // Report back to the UI
            //
            backgroundFormatWorker.ReportProgress(percent);
        }

        private void backgroundFormatWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Formatting {0}%... "+ e.ProgressPercentage);
            //formatProgressBar.Value = e.ProgressPercentage;
        }

        private void backgroundFormatWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine((int)e.Result == 0 ?"Finished Formatting Disc!" : "Error Formatting Disc!");

            //formatProgressBar.Value = 0;
            _isFormatting = false;
        }

        #endregion

        #region Converts

        /// <summary>
        /// Converts an IMAPI_MEDIA_PHYSICAL_TYPE to it's string
        /// </summary>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        private static string GetMediaTypeString(IMAPI_MEDIA_PHYSICAL_TYPE mediaType)
        {
            switch (mediaType)
            {
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_UNKNOWN:
                default:
                    return "Unknown Media Type";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM:
                    return "CD-ROM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR:
                    return "CD-R";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW:
                    return "CD-RW";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDROM:
                    return "DVD ROM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDRAM:
                    return "DVD-RAM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR:
                    return "DVD+R";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW:
                    return "DVD+RW";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR_DUALLAYER:
                    return "DVD+R Dual Layer";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR:
                    return "DVD-R";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHRW:
                    return "DVD-RW";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER:
                    return "DVD-R Dual Layer";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK:
                    return "random-access writes";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW_DUALLAYER:
                    return "DVD+RW DL";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDROM:
                    return "HD DVD-ROM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDR:
                    return "HD DVD-R";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDRAM:
                    return "HD DVD-RAM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDROM:
                    return "Blu-ray DVD (BD-ROM)";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDR:
                    return "Blu-ray media";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDRE:
                    return "Blu-ray Rewritable media";
            }
        }

        /// <summary>
        /// Converts an IMAPI_PROFILE_TYPE to it's string
        /// </summary>
        /// <param name="profileType"></param>
        /// <returns></returns>
        static string GetProfileTypeString(IMAPI_PROFILE_TYPE profileType)
        {
            switch (profileType)
            {
                default:
                    return string.Empty;

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_CD_RECORDABLE:
                    return "CD-R";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_CD_REWRITABLE:
                    return "CD-RW";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVDROM:
                    return "DVD ROM";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_RECORDABLE:
                    return "DVD-R";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_RAM:
                    return "DVD-RAM";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_PLUS_R:
                    return "DVD+R";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_PLUS_RW:
                    return "DVD+RW";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_PLUS_R_DUAL:
                    return "DVD+R Dual Layer";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_REWRITABLE:
                    return "DVD-RW";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_RW_SEQUENTIAL:
                    return "DVD-RW Sequential";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_R_DUAL_SEQUENTIAL:
                    return "DVD-R DL Sequential";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_R_DUAL_LAYER_JUMP:
                    return "DVD-R Dual Layer";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_PLUS_RW_DUAL:
                    return "DVD+RW DL";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_HD_DVD_ROM:
                    return "HD DVD-ROM";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_HD_DVD_RECORDABLE:
                    return "HD DVD-R";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_HD_DVD_RAM:
                    return "HD DVD-RAM";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_ROM:
                    return "Blu-ray DVD (BD-ROM)";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_R_SEQUENTIAL:
                    return "Blu-ray media Sequential";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_R_RANDOM_RECORDING:
                    return "Blu-ray media";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_REWRITABLE:
                    return "Blu-ray Rewritable media";
            }
            #endregion
        }
    }
}
