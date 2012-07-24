using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.Stampare;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Core.DatiDiEsempio;
using System.Windows.Input;
using System.Windows.Forms;

namespace Digiphoto.Lumen.UI
{
    public class SelettoreStampantiInstallateViewModel : ViewModelBase 
    {

        public SelettoreStampantiInstallateViewModel()
        {
            // istanzio la lista vuota
            stampantiInstallate = new ObservableCollection<StampanteInstallata>();
            rileggereStampantiInstallate();
        }

        #region Proprietà

        /// <summary>
        /// Tutti i fotografi da visualizzare
        /// </summary>
        public ObservableCollection<StampanteInstallata> stampantiInstallate
        {
            get;
            set;
        }

        /// <summary>
        /// La Stampante attualmente selezionata
        /// </summary>
        StampanteInstallata _stampanteSelezionata;
        public StampanteInstallata stampanteSelezionata
        {
            get
            {
                return _stampanteSelezionata;
            }
            set
            {
                if (value != _stampanteSelezionata)
                {
                    _stampanteSelezionata = value;
                    OnPropertyChanged("stampanteSelezionata");
                }
            }
        }

        public IEntityRepositorySrv<StampanteInstallata> stampnatiInstallateReporitorySrv
        {
            get
            {
                return LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<StampanteInstallata>>();
            }
        }

        #endregion

        #region Metodi
        private void rileggereStampantiInstallate()
        {

            IEnumerable<StampanteInstallata> listaS = null;
            if (IsInDesignMode)
            {
                // genero dei dati casuali
                DataGen<StampanteInstallata> dg = new DataGen<StampanteInstallata>();
                listaS = dg.generaMolti(5);
            }
            else
            {
				listaS = LumenApplication.Instance.stampantiInstallate;
            }

            // purtoppo pare che rimpiazzare il reference con uno nuovo, causa dei problemi.
            // Non posso istanziare nuovamente la lista, ma la devo svuotare e ripopolare.
            stampantiInstallate.Clear();
            foreach (StampanteInstallata s in listaS)
                stampantiInstallate.Add(s);
        }

        #endregion

        #region Comandi

        private RelayCommand _rileggereStampantiInstallateCommand;
        public ICommand rileggereStampantiInstallateCommand
        {
            get
            {
                if (_rileggereStampantiInstallateCommand == null)
                {
                    _rileggereStampantiInstallateCommand = new RelayCommand(param => this.rileggereStampantiInstallate() );
                }
                return _rileggereStampantiInstallateCommand;
            }
        }

        #endregion
    }
}
