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

	public enum MotoreDatabase : short {
		SqlServerCE = 0,
		SqLite = 1,
		SqlServer = 2
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

		private string _cartellaMaschere;
		public string cartellaMaschere {
			get {
				return _cartellaMaschere;
			}
			set {
				if( _cartellaMaschere != value ) {
					_cartellaMaschere = value;
					OnPropertyChanged( "cartellaMaschere" );
				}
			}
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

		private string _dbNomeDbPieno;
		public string dbNomeDbPieno {
			get {
				return _dbNomeDbPieno;
			}
			set {
				if( _dbNomeDbPieno != value ) {
					_dbNomeDbPieno = value;
					OnPropertyChanged( "dbNomeDbPieno" );
				}
			}
		}

		string _cartellaDatabase;
		public string cartellaDatabase {
			get {
				return _cartellaDatabase;
			}
			set {
				if( _cartellaDatabase != value ) {
					_cartellaDatabase = value;
					OnPropertyChanged( "cartellaDatabase" );
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

		/// <summary>
		/// Nome per gestire i fuori standard
		/// </summary>
		public string fuoriStandard {
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

		private MotoreDatabase _motoreDatabase;
		public MotoreDatabase motoreDatabase {
			get {
				return _motoreDatabase;
			}
			set {
				if( _motoreDatabase != value ) {
					_motoreDatabase = value;
					OnPropertyChanged( "motoreDatabase" );
				}
			}
		}

		public int millisIntervalloSlideShow {
			get;
			set;
		}

		public string oraCambioGiornata {
			get;
			set;
		}
		public int numRigheProvini 
		{ 
			get;
			set; 
		}

		public int numColoneProvini
		{ 
			get; 
			set; 
		}

		public bool macchiaProvini
		{
			get;
			set;
		}

		public bool compNumFoto
		{
			get;
			set;
		}

		# region SlideShowParam

		public short deviceEnum
		{
			get;
			set;
		}

		public int screenHeight
		{
			get;
			set;
		}

		public int screenWidth
		{
			get;
			set;
		}

		public int slideHeight
		{
			get;
			set;
		}

		public int slideWidth
		{
			get;
			set;
		}

		public int slideTop
		{
			get;
			set;
		}

		public int slideLeft
		{
			get;
			set;
		}

		public int slideBoundsX
		{
			get;
			set;
		}

		public int slideBoundsY
		{
			get;
			set;
		}

		# endregion

	}
}
