using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Servizi.Vendere;
using System.IO;
using System.ComponentModel;

namespace Digiphoto.Lumen.Config
{
	public struct Stampigli {
		public bool numFoto;
		public bool operatore;
		public bool giornata;
	}

	public enum ModoVendita : short {
		[Description( "Con Carrello" )]
		Carrello = 0,
		[Description( "Stampa Diretta" )]
		StampaDiretta = 1
	}


    public sealed class UserConfigLumen : INotifyPropertyChanged
    {

		public UserConfigLumen() {
		}


		string _cartellaFoto;
		public string cartellaFoto
		{
			get {
				return _cartellaFoto;
			}
			set {
				if( _cartellaFoto != value ) {
					_cartellaFoto = value;
					OnPropertyChanged( "cartellaFoto" );
				}
			}
		}

		/// <summary>
		/// L'auto zoom, significa che la foto viene automaticamente ingrandita per
		/// essere stampata senza bordi bianchi, e riempire quindi interamente l'area stampabile.
		/// </summary>
		public bool autoZoomNoBordiBianchi {
			get;
			set;
		}

		public ModoVendita modoVendita
		{
			get;
			set;
		}

		public bool eraseFotoMemoryCard {
			get;
			set;
		}

		public int pixelLatoProvino {
			get;
			set;
		}

		public int giorniDeleteFoto {
			set;
			get;
		}

		public string cartellaMaschere {
			get;
			set;
		}

		public string stampantiAbbinate {
			get;
			set;
		}

		public bool proiettaDiapo {
			get;
			set;
		}

		public string dbNomeDbVuoto {
			get;
			set;
		}

		public string dbNomeDbPieno {
			get;
			set;
		}

		string _dbCartella;
		public string dbCartella {
			get {
				return _dbCartella;
			}
			set {
				if( _dbCartella != value ) {
					_dbCartella = value;
					OnPropertyChanged( "dbCartella" );
				}
			}
        }

		public string estensioniGrafiche {
			get;
			set;
		}

		public string editorImmagini {
			get;
			set;
		}

		public bool editorImmaginiMultiArgs {
			get;
			set;
		}

		public string codicePuntoVendita {
			get;
			set;
		}

		public string descrizionePuntoVendita {
			get;
			set;
		}

		public bool masterizzaDirettamente {
			get;
			set;
		}

		public string defaultMasterizzatore {
			get;
			set;
		}

		private string _defaultChiavetta;
		public string defaultChiavetta {
			get {
				return _defaultChiavetta;
			}
			set {
				if( _defaultChiavetta != value ) {
					_defaultChiavetta = value;
					OnPropertyChanged( "defaultChiavetta" );
				}
			}
		}

		public bool isWindowPubblicaVisibile {
			get;
			set;
		}

		public bool stampiglioGiornata {
			get;
			set;
		}

		public bool stampiglioOperatore {
			get;
			set;
		}

		public bool stampiglioNumFoto {
			get;
			set;
		}


		public event PropertyChangedEventHandler PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged( string name ) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if( handler != null ) {
				handler( this, new PropertyChangedEventArgs( name ) );
			}
		}
	}
}
