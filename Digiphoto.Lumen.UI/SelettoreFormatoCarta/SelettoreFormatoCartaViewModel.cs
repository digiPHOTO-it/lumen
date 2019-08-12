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
using Digiphoto.Lumen.Core.Database;

using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Eventi;

namespace Digiphoto.Lumen.UI
{
    public class SelettoreFormatoCartaViewModel : ViewModelBase, IObserver<EntityCambiataMsg>
    {

		public SelettoreFormatoCartaViewModel()
        {
			this.DisplayName = "Selettore Formato Carta";

			// istanzio la lista vuota
            formatiCarta = new ObservableCollection<FormatoCarta>();

			IObservable<EntityCambiataMsg> observable = LumenApplication.Instance.bus.Observe<EntityCambiataMsg>();
			observable.Subscribe( this );

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
                    OnPropertyChanged("formatoCartaSelezionato");
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
			rileggereFormatiCarta( false );
		}

		private void eliminareFormatoCarta() {

			FormatoCarta dacanc = formatoCartaSelezionato;

			try {

				try {
					OrmUtil.forseAttacca( ref dacanc );
				} catch( Exception ) {
				}

				var test = UnitOfWorkScope.currentDbContext.FormatiCarta.Remove( dacanc );

				var test2 = UnitOfWorkScope.currentDbContext.SaveChanges();

				// Se tutto è andato bene, allora rimuovo l'elemento dalla collezione visuale.
				formatoCartaSelezionato = null;
				formatiCarta.Remove( dacanc );

			} catch( Exception ee ) {
				UnitOfWorkScope.currentObjectContext.ObjectStateManager.ChangeObjectState( dacanc, System.Data.Entity.EntityState.Unchanged );
				throw ee;
			}

		}

		private void rileggereFormatiCarta( object param ) {

			// Decido se devo dare un avviso all'utente
			Boolean avvisami = false;

			if( param != null ) {
				if( param is Boolean )
					avvisami = (Boolean)param;
				if( param is string )
					Boolean.TryParse( param.ToString(), out avvisami );
			}
			// ---


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

			if( avvisami && dialogProvider != null )
				dialogProvider.ShowMessage( "Riletti " + formatiCarta.Count() + " formati carta", "Successo" );
		}

		private void creareNuovoFormatoCarta() {

			// Salvo nel database
			formatoCartaReporitorySrv.addNew( nuovoFormatoCarta );
			formatoCartaReporitorySrv.saveChanges();

			// Aggiungo alla collezione visuale (per non dover rifare la query)
			//I formati carta vengono sempre riletti tutti!!!!
			//formatiCarta.Add( nuovoFormatoCarta );

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
			nuovoFormatoCarta.attivo = true;

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

		private RelayCommand _eliminareCommand;
		public ICommand eliminareCommand {
			get {
				if( _eliminareCommand == null ) {
					_eliminareCommand = new RelayCommand( param => this.eliminareFormatoCarta(),
					                                      param => this.possoEliminareFormatoCarta,
														  false );
				}
				return _eliminareCommand;
			}
		}

		private RelayCommand _rileggereFormatiCartaCommand;
		public ICommand rileggereFormatiCartaCommand {
			get {
				if( _rileggereFormatiCartaCommand == null ) {
					_rileggereFormatiCartaCommand = new RelayCommand( param => this.rileggereFormatiCarta( param ), null, false );
				}
				return _rileggereFormatiCartaCommand;
			}
		}

		private bool possoCreareNuovoFormatoCarta {
			get {
				return nuovoFormatoCarta != null && OrmUtil.isValido( nuovoFormatoCarta );
			}
		}

		private bool possoEliminareFormatoCarta {
			get {
				return formatoCartaSelezionato != null;
			}
		}

		#endregion

		#region Interfaccia IObserver

		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( EntityCambiataMsg value ) {
			
			// Qualcuno ha spataccato nella tabella dei formati carta. Rileggo tutto
			if( value.type == typeof( FormatoCarta ) )
				rileggereFormatiCartaCommand.Execute( false );
		}

		#endregion Interfaccia IObserver
	}
}
