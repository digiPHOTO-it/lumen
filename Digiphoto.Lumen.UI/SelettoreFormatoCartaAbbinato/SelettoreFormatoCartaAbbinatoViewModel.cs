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

		/// <summary>
		/// Se nessuno mi dice niente, allora deserializzo la configurazione statica
		/// </summary>
        public SelettoreFormatoCartaAbbinatoViewModel() : this( Configurazione.UserConfigLumen.stampantiAbbinate )
        {
        }

		/// <summary>
		/// Deserializzo la stringa indicata. Il formato è:
		///   FC;ST#FC;ST#
		/// dove FC = Guid del formato carta ; ST=Nome della stampante installata
		/// </summary>
		public SelettoreFormatoCartaAbbinatoViewModel( string strAbbinamenti ) : base() {

			_strAbbinamenti = strAbbinamenti;

			refresh();  // carico la lista
		}

		private string _strAbbinamenti;


        #region Proprietà

		public StampantiAbbinateCollection formatiCartaAbbinati {
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

		/// <summary>
		/// Partendo dalla stringa con gli abbinamenti, ricreo la collezione di oggetti.
		/// </summary>
        private void refresh()
        {

            if (IsInDesignMode)
            {
                // genero dei dati casuali
                DataGen<StampanteAbbinata> dg = new DataGen<StampanteAbbinata>();
				formatiCartaAbbinati = new StampantiAbbinateCollection( dg.generaMolti(5) );
            }
            else
            {
				// Popolo la collezione partendo dalla stringa serializzata aumma aumma
				formatiCartaAbbinati = StampantiAbbinateUtil.deserializza(_strAbbinamenti );
            }
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
                    _rileggereFormatiCartaAbbinatiCommand = new RelayCommand(param => this.refresh(), null, false);
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

		/// <summary>
		/// Sposto in SU
		/// </summary>
        private void suAbbina()
        {
			if( SelectedAbbinamentoIndex > 0 )
				formatiCartaAbbinati.Move( SelectedAbbinamentoIndex, SelectedAbbinamentoIndex - 1 );
        }

        private void giuAbbina()
        {
			if( SelectedAbbinamentoIndex < formatiCartaAbbinati.Count - 1 )
				formatiCartaAbbinati.Move( SelectedAbbinamentoIndex, SelectedAbbinamentoIndex + 1 );
        }

		public void removeSelected() {
			formatiCartaAbbinati.RemoveAt( SelectedAbbinamentoIndex );
		}

        #endregion
    }

}
