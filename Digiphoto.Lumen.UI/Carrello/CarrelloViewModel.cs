using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using System.ComponentModel;
using System.Windows.Data;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Servizi.Explorer;
using Digiphoto.Lumen.Applicazione;
using System.Windows.Forms;
using System.Drawing;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Vendere;
using System.Windows.Input;
using Digiphoto.Lumen.Servizi.Stampare;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.UI
{
    public class CarrelloViewModel : ViewModelBase 
    {
        public CarrelloViewModel()
        {
            paramCercaFoto = new ParamCercaFoto();
            if (IsInDesignMode)
            {
            }
            else
            {

                paramCercaFoto.giornataIniz = new DateTime(2012, 1, 20);

                // Faccio una ricerca a vuoto
                fotoExplorerSrv.cercaFoto(paramCercaFoto);

                this.FotoCarrello = CollectionViewSource.GetDefaultView(fotoExplorerSrv.fotografie);
            }
		}

        #region Proprietà

        public ICollectionView FotoCarrello
        {
            get;
            set;
        }

        public ParamCercaFoto paramCercaFoto
        {
            get;
            set;
        }

        IFotoExplorerSrv fotoExplorerSrv
        {
            get
            {
                return LumenApplication.Instance.getServizioAvviato<IFotoExplorerSrv>();
            }
        }

        public int TotaleFotoSelezionate
        {
            get
            {
                int quanti = 0;
                if (FotoCarrello != null)
                    quanti = FotoCarrello.Cast<Fotografia>().Where(f => f.isSelezionata == true).Count();
                return quanti;
            }
        }

        public decimal PrezzoTotale
        {
            get
            {
                decimal totale = 0;
                decimal prezzo = 5;
                int quanti = 1;
                if (FotoCarrello != null)
                    quanti = FotoCarrello.Cast<Fotografia>().Where(f => f.isSelezionata == true).Count();
                totale = quanti * prezzo;
                return totale;
            }
        }

        public bool possoAggiungereAlMasterizzatore
        {
            get
            {
                return TotaleFotoSelezionate > 0;
            }
        }

        private IVenditoreSrv venditoreSrv
        {
            get
            {
                return (IVenditoreSrv)LumenApplication.Instance.getServizioAvviato<IVenditoreSrv>();
            }
        }

        #endregion


        #region Metodi

        /// <summary>
        /// Aggiungo le immagini selezionate al masterizzatore
        /// </summary>
        /// <returns></returns>
        public void aggiungereAlMasterizzatore()
        {

            IEnumerable<Fotografia> listaSelez = creaListaFotoSelezionate();
            venditoreSrv.aggiungiMasterizzate(listaSelez);
        }

        private IList<Fotografia> creaListaFotoSelezionate()
        {
            var fotos = FotoCarrello.OfType<Fotografia>().Where(f => f.isSelezionata == true);

            return new List<Fotografia>(fotos);
        }

        /// <summary>
        /// Spengo tutte le selezioni
        /// </summary>
        private void deselezionareTutto()
        {
            int quanti = TotaleFotoSelezionate;

            foreach (Fotografia f in FotoCarrello)
                f.isSelezionata = false;
        }

        /// <summary>
        /// Devo mandare in stampa le foto selezionate
        /// Nel parametro mi arriva l'oggetto StampanteAbbinata che mi da tutte le indicazioni
        /// per la stampa: il formato carta e la stampante
        /// </summary>
        private void stampare(object objStampanteAbbinata)
        {

            StampanteAbbinata stampanteAbbinata = (StampanteAbbinata)objStampanteAbbinata;

            IList<Fotografia> listaSelez = creaListaFotoSelezionate();

            // Se ho selezionato più di una foto, e lavoro in stampa diretta, allora chiedo conferma
            bool procediPure = true;
            int quante = listaSelez.Count;
            if (quante > 1 && Configurazione.modoVendita == ModoVendita.StampaDiretta)
            {
                dialogProvider.ShowConfirmation("Confermi la stampa di " + quante + " foto ?", "Richiesta conferma",
                  (confermato) =>
                  {
                      procediPure = confermato;
                  });
            }

            if (procediPure)
            {
                // Aggiungo al carrello oppure stampo direttamente
                venditoreSrv.aggiungiStampe(listaSelez, creaParamStampaFoto(stampanteAbbinata));

                // Spengo tutto
                deselezionareTutto();
            }

        }

        /// <summary>
        /// Creo i parametri di stampa, mixando un pò di informazioni prese
        /// dalla configurazione, dallo stato dell'applicazione, e dalla scelta dell'utente.
        /// </summary>
        /// <param name="stampanteAbbinata"></param>
        /// <returns></returns>
        private ParamStampaFoto creaParamStampaFoto(StampanteAbbinata stampanteAbbinata)
        {

            ParamStampaFoto p = venditoreSrv.creaParamStampaFoto();

            p.nomeStampante = stampanteAbbinata.StampanteInstallata.NomeStampante;
            p.formatoCarta = stampanteAbbinata.FormatoCarta;
            // TODO per ora il nome della Porta a cui è collegata la stampante non lo uso. Non so cosa farci.

            return p;
        }

        private void calcolaTotali()
        {
            OnPropertyChanged("TotaleFotoSelezionate");
            OnPropertyChanged("PrezzoTotale"); 
        }

        #endregion

        #region Comandi

        private RelayCommand _aggiungereAlMasterizzatoreCommand;
        public ICommand aggiungereAlMasterizzatoreCommand
        {
            get
            {
                if (_aggiungereAlMasterizzatoreCommand == null)
                {
                    _aggiungereAlMasterizzatoreCommand = new RelayCommand(param => aggiungereAlMasterizzatore()
                        //  ,param => possoAggiungereAlMasterizzatore 
                                                                           );
                }
                return _aggiungereAlMasterizzatoreCommand;
            }
        }

        private RelayCommand _stampareCommand;
        public ICommand stampareCommand
        {
            get
            {
                if (_stampareCommand == null)
                {
                    _stampareCommand = new RelayCommand(param => stampare(param)
                        //  ,param => possoAggiungereAlMasterizzatore 
                                                                           );
                }
                return _stampareCommand;
            }
        }

        private RelayCommand _calcolaTotali;
        public ICommand CalcolaTotali
        {
            get
            {
                if (_calcolaTotali == null)
                {
                    _calcolaTotali = new RelayCommand(param => calcolaTotali());
                }
                return _calcolaTotali;
            }
        }

        #endregion

    }
}
