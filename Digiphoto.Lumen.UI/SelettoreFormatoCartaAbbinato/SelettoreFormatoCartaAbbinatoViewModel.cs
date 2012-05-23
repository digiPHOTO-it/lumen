using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Servizi.Stampare;
using System.Windows.Input;
using Digiphoto.Lumen.Core.DatiDiEsempio;
using Digiphoto.Lumen.Config;
using System.Windows.Forms;
using System.Windows;

namespace Digiphoto.Lumen.UI
{
    public class SelettoreFormatoCartaAbbinatoViewModel : ViewModelBase 
    {

        private StampantiAbbinateSrvImpl stampantiAbbinateSrvImpl = null;

        public SelettoreFormatoCartaAbbinatoViewModel()
        {
            // istanzio la lista vuota
            formatoCartaAbbinato = new ObservableCollection<StampanteAbbinata>();
            rileggereFormatiCartaAbbinati();
        }

        #region Proprietà

        /// <summary>
        /// Tutti i fotografi da visualizzare
        /// </summary>
        public ObservableCollection<StampanteAbbinata> formatoCartaAbbinato
        {
            get;
            set;
        }

        /// <summary>
        /// La Stampante attualmente selezionata
        /// </summary>
        StampanteAbbinata _formatoCartaAbbinatoSelezionato;
        public StampanteAbbinata formatoCartaAbbinatoSelezionato
        {
            get
            {
                return _formatoCartaAbbinatoSelezionato;
            }
            set
            {
                if (value != _formatoCartaAbbinatoSelezionato)
                {
                    _formatoCartaAbbinatoSelezionato = value;
                    OnPropertyChanged("formatoCartaAbbinatoSelezionato");
                }
            }
        }

        int _SelectedAbbinamentoIndex;
        public int SelectedAbbinamentoIndex
        {
            get
            {
                return _SelectedAbbinamentoIndex;
            }
            set
            {
                if (value != _SelectedAbbinamentoIndex)
                {
                    _SelectedAbbinamentoIndex = value;
                    OnPropertyChanged("SelectedAbbinamentoIndex");
                }
            }
        }

        #endregion

        #region Metodi
        private void rileggereFormatiCartaAbbinati()
        {

            IEnumerable<StampanteAbbinata> listaS = null;
            if (IsInDesignMode)
            {
                // genero dei dati casuali
                DataGen<StampanteAbbinata> dg = new DataGen<StampanteAbbinata>();
                listaS = dg.generaMolti(5);
            }
            else
            {
                stampantiAbbinateSrvImpl = new StampantiAbbinateSrvImpl();
				listaS = stampantiAbbinateSrvImpl.listaStampantiAbbinate(UserConfigLumen.StampantiAbbinate);
            }

            // purtoppo pare che rimpiazzare il reference con uno nuovo, causa dei problemi.
            // Non posso istanziare nuovamente la lista, ma la devo svuotare e ripopolare.
            formatoCartaAbbinato.Clear();
            foreach (StampanteAbbinata s in listaS)
                formatoCartaAbbinato.Add(s);
        }

        #endregion

        #region Comandi

        private RelayCommand _rileggereFormatiCartaAbbinatiCommand;
        public ICommand rileggereFormatiCartaAbbinatiCommand
        {
            get
            {
                if (_rileggereFormatiCartaAbbinatiCommand == null)
                {
                    _rileggereFormatiCartaAbbinatiCommand = new RelayCommand(param => this.rileggereFormatiCartaAbbinati(), null, false);
                }
                return _rileggereFormatiCartaAbbinatiCommand;
            }
        }

        private RelayCommand _suCommand;
        public ICommand suCommand
        {
            get
            {
                if (_suCommand == null)
                {
                    _suCommand = new RelayCommand(param => this.suAbbina(), null);
                }
                return _suCommand;
            }
        }

        private RelayCommand _giuCommand;
        public ICommand giuCommand
        {
            get
            {
                if (_giuCommand == null)
                {
                    _giuCommand = new RelayCommand(param => this.giuAbbina(), null);
                }
                return _giuCommand;
            }
        }

        #endregion
        #region esecuzioneComandi

        private void suAbbina()
        {
            int index = 0;
            foreach (StampanteAbbinata fC in formatoCartaAbbinato)
            {
                if(fC.FormatoCarta == formatoCartaAbbinatoSelezionato.FormatoCarta && 
                    fC.StampanteInstallata == formatoCartaAbbinatoSelezionato.StampanteInstallata)
                {
                    int oldIndex = index;
                    int newIndex = --index;
                    newIndex = newIndex < 0 ? formatoCartaAbbinato.Count - 1 : newIndex;
                    formatoCartaAbbinato.Move(oldIndex, newIndex);
                    break;
                }
                index++;
            }
            stampantiAbbinateSrvImpl.sostituisciAbbinamento(formatoCartaAbbinato);
			stampantiAbbinateSrvImpl.updateAbbinamento();
            OnPropertyChanged("formatoCartaAbbinato");
        }

        private void giuAbbina()
        {
            int index = 0;
            foreach (StampanteAbbinata fC in formatoCartaAbbinato)
            {
                if (fC.FormatoCarta == formatoCartaAbbinatoSelezionato.FormatoCarta &&
                    fC.StampanteInstallata == formatoCartaAbbinatoSelezionato.StampanteInstallata)
                {
                    int oldIndex = index;
                    int newIndex = ++index;
                    newIndex = newIndex > formatoCartaAbbinato.Count - 1 ? 0 : newIndex;
                    formatoCartaAbbinato.Move(oldIndex, newIndex);
                    break;
                }
                index++;
            }
            stampantiAbbinateSrvImpl.sostituisciAbbinamento(formatoCartaAbbinato);
            stampantiAbbinateSrvImpl.updateAbbinamento();
            OnPropertyChanged("formatoCartaAbbinato");
        }

        #endregion
    }

}
