using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using System.Collections.ObjectModel;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.DatiDiEsempio;
using System.Windows.Input;
using System.Windows;

namespace Digiphoto.Lumen.UI
{
    public class SelettoreFormatoCartaViewModel : ViewModelBase
    {

		public SelettoreFormatoCartaViewModel()
        {
			this.DisplayName = "Selettore Formato Carta";

			// istanzio la lista vuota
            formatiCarta = new ObservableCollection<FormatoCarta>();

			rileggereFormatiCarta();
			istanziaNuovoFormatoCarta();
		}

		#region Proprietà

		public FormatoCarta nuovoFormatoCarta {
			get;
			set;
		}

        //public string descrizioneFormatoNew
        //{
        //    get {
        //        return nuovoFormatoCarta.descrizione;
        //    }
        //    set {
        //        nuovoFormatoCarta.descrizione = value;
        //    }
        //}

		/// <summary>
		/// Tutti i formatiCarta da visualizzare
		/// </summary>
		public ObservableCollection<FormatoCarta> formatiCarta {
			get;
			set;
		}

		/// <summary>
		/// Il FormatoCarta attualmente selezionato
		/// </summary>
		FormatoCarta _formatoCartaSelezionato;
		public FormatoCarta formatoCartaSelezionato {
			get {
                return _formatoCartaSelezionato;
			}
			set {
                if (value != _formatoCartaSelezionato)
                {
                    _formatoCartaSelezionato = value;
                    OnPropertyChanged("_formatoCartaSelezionato");
				}
			}
		}


		public IEntityRepositorySrv<FormatoCarta> formatoCartaReporitorySrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<FormatoCarta>>();
			}
		}
		#endregion

		#region Metodi
		private void rileggereFormatiCarta() {

			IEnumerable<FormatoCarta> listaF = null;
			if( IsInDesignMode ) {
				// genero dei dati casuali
				DataGen<FormatoCarta> dg = new DataGen<FormatoCarta>();
				listaF = dg.generaMolti( 5 );

			} else {
                listaF = formatoCartaReporitorySrv.getAll();
			}

			// purtoppo pare che rimpiazzare il reference con uno nuovo, causa dei problemi.
			// Non posso istanziare nuovamente la lista, ma la devo svuotare e ripopolare.
			formatiCarta.Clear();
            foreach (FormatoCarta f in listaF)
            {
                formatiCarta.Add(f);
            }
		}

		private void creareNuovoFormatoCarta() {

			// Salvo nel database
			formatoCartaReporitorySrv.addNew( nuovoFormatoCarta );

			// Aggiungo alla collezione visuale (per non dover rifare la query)
			formatiCarta.Add( nuovoFormatoCarta );

			// Svuoto per nuova creazione
			istanziaNuovoFormatoCarta();

		}

		/// <summary>
		///  istanzia un oggetto nomeCartellaRecente tipo FormatoCarta, pronto per essere utilizzato nella creazione
		///  nomeCartellaRecente un nuovo fotografoSelezionato, in caso nell'elenco mancasse.
		/// </summary>
		private void istanziaNuovoFormatoCarta() {

			// Questo è d'appoggio per la creazione nomeCartellaRecente un nuovo formatoCartaSelezionato al volo
			nuovoFormatoCarta = new FormatoCarta();
			nuovoFormatoCarta.id = Guid.NewGuid();

            //OnPropertyChanged("descrizioneFormatoNew");
            OnPropertyChanged("nuovoFormatoCarta");
		}

		#endregion

		#region Comandi

		private RelayCommand _creareNuovoCommand;
		public ICommand creareNuovoCommand {
			get {
				if( _creareNuovoCommand == null ) {
					_creareNuovoCommand = new RelayCommand( param => this.creareNuovoFormatoCarta(),
															param => this.possoCreareNuovoFormatoCarta,
															true );
				}
				return _creareNuovoCommand;
			}
		}


		private RelayCommand _rileggereFormatiCartaCommand;
		public ICommand rileggereFormatiCartaCommand {
			get {
				if( _rileggereFormatiCartaCommand == null ) {
					_rileggereFormatiCartaCommand = new RelayCommand( param => this.rileggereFormatiCarta(), null, false );
				}
				return _rileggereFormatiCartaCommand;
			}
		}

		private bool possoCreareNuovoFormatoCarta {
			
			get {

				List<string> avvisi;
				List<string> errori;
				bool esito = nuovoFormatoCarta != null && nuovoFormatoCarta.Validate( out avvisi, out errori );
				return esito;
			}
		}

		#endregion
    }
}
