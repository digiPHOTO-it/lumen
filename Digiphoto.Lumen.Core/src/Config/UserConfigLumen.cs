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

	public class Griglia {
		public short numRighe { get; set; }
		public short numColonne { get; set; }
	}

	public enum ModoVendita : short {
		[Description( "Con Carrello" )]
		Carrello = 0,
		[Description( "Stampa Diretta" )]
		StampaDiretta = 1
	}

	public enum MasterizzaTarget : short {
		[Description( "Non gestita masterizzazione" )]
		Nulla = 0,
		[Description( "Solo drive rimovibili" )]
		DriveRimovibili = 1,
		[Description( "Solo sul masterizzatore" )]
		Masterizzatore = 2,
		[Description( "Solo su specifica cartella" )]
		Cartella = 3,
		[Description( "Ovunque" )]
		Ovunque = 4
	}


	public enum MotoreDatabase : short {
		SqLite = 1,
		MySQL = 2
	}



    public sealed class UserConfigLumen : INotifyPropertyChanged
    {

		public UserConfigLumen() {

			// Questo oggetto deve sempre essere istanziato
			this.geometriaFinestraSlideShow = new GeometriaFinestra();
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

		private string _cartellaLoghi;
		public string cartellaLoghi {
			get {
				return _cartellaLoghi;
			}
			set {
				if( _cartellaLoghi != value ) {
					_cartellaLoghi = value;
					OnPropertyChanged( "cartellaLoghi" );
				}
			}
		}

		private string _cartellaOnRide;
		public string cartellaOnRide {
			get {
				return _cartellaOnRide;
			}
			set {
				if( _cartellaOnRide != value ) {
					_cartellaOnRide = value;
					OnPropertyChanged( "cartellaOnRide" );
				}
			}
		}

		public string stampantiAbbinate {
			get;
			set;
		}

		public bool imprimereAreaDiRispetto {
			get;
			set;
		}

		public string expRatioAreaDiRispetto {
			get;
			set;
		}

		public bool invertiRicerca
		{
			get;
			set;
		}

		public string dbNomeDbVuoto {
			get;
			set;
		}

		private string _dbNomeServer;
		public string dbNomeServer {
			get {
				return _dbNomeServer;
			}
			set {
				if( _dbNomeServer != value ) {
					_dbNomeServer = value;
					OnPropertyChanged( "dbNomeServer" );
				}
			}
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

		public MasterizzaTarget masterizzaTarget {
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

		/// <summary>
		/// Il nome del file di default per i loghi.
		/// </summary>
		string _logoNomeFile;
		public string logoNomeFile {
			get {
				return _logoNomeFile;
			}
			set {
				if( _logoNomeFile != value ) {
					_logoNomeFile = value;
					OnPropertyChanged( "logoNomeFile" );
				}
			}
		}

		/// <summary>
		/// Il nome del file di default per i loghi.
		/// </summary>
		string _logoNomeFileSelfService;
		public string logoNomeFileSelfService {
			get {
				return _logoNomeFileSelfService;
			}
			set {
				if( _logoNomeFileSelfService != value ) {
					_logoNomeFileSelfService = value;
					OnPropertyChanged( "logoNomeFileSelfService" );
				}
			}
		}

		/// <summary>
		/// Il nome del file di default per i loghi.
		/// </summary>
		string _modoRicercaSS;
		public string modoRicercaSS
		{
			get
			{
				return _modoRicercaSS;
			}
			set
			{
				if( _modoRicercaSS != value ) {
					_modoRicercaSS = value;
					OnPropertyChanged( "modoRicercaSS" );
				}
			}
		}


		/// <summary>
		/// La percentuale di copertura della foto originale da parte del logo.
		/// </summary>
		public short logoPercentualeCopertura {
			get;
			set;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		public void OnPropertyChanged( string name ) {
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

		/// <summary>
		/// Applico un watermark sulla stampa dei provini si/no
		/// </summary>
		public bool macchiaProvini
		{
			get;
			set;
		}

		/// <summary>
		/// Applica un watermark sullo slide show si/no
		/// </summary>
		public bool macchiaSlideShow {
			get;
			set;
		}


		public bool compNumFoto
		{
			get;
			set;
		}

		
		string _cartellaPubblicita;
		public string cartellaPubblicita {
			get {
				return _cartellaPubblicita;
			}
			set {
				if( _cartellaPubblicita != value ) {
					_cartellaPubblicita = value;
					OnPropertyChanged( "cartellaPubblicita" );
				}
			}
		}

		/// <summary>
		/// Questo numero indica ogni quante videate dello slide, occorre visualizzare 
		/// uno spot pubblicitario.
		/// Esempio: se imposto = 5 significa che ogni 5 schermate di s.s. ce ne sarà una 
		/// intera di pubblicità.
		/// </summary>
		public int intervalliPubblicita {
			get;
			set;
		}

		/// <summary>
		/// Questo valore indica il numero massimo di foto in modifica
		/// </summary>
		public int maxNumFotoMod
		{
			get;
			set;
		}

		/// <summary>
		/// Questo valore indica la lunghezza della lista FIFO
		/// </summary>
		public int lungFIFOFotoMod
		{
			get;
			set;
		}

		/// <summary>
		/// Quando si lancia una stampa rapida di foto, senza passare dal carrello,
		/// se il totale delle foto, raggiunge questa soglia, allora chiedo conferma all'utente che
		/// davvero è intenzionato a stamparne così tante in modo immediato. Probabilmente si è sbagliato.
		/// Se impostato = 0 non viene utilizzato (non ha effetto).
		/// </summary>
		public int sogliaNumFotoConfermaInStampaRapida {
			get;
			set;
		}

		/// <summary>
		/// Questo parametro serve a creare un margine alla lista delle foto nella gallery,
		/// così che quando si visualizza una sola riga di foto, si attiva questa correzione
		/// in modo da farci stare due foto sulla stessa riga (altrimenti ce ne sarebbe una sola).
		/// </summary>
		public int correzioneAltezzaGalleryDueFoto {
			get;
			set;
		}


		/// <summary>
		/// Durante la stampa dei provini abbiamo un problema di sovraccarico di memoria sulla singola
		/// pagina stampata. Di conseguenza, non posso caricare 50 foto grandi in una unica pagina altrimenti
		/// si rischia di esaurire la memoria.
		/// Questo parametro, attualmente non viene visualizzato nella UI di configurazione, perché
		/// è solo un parametro tecnico, che ho deciso di tenere esterno al programma per non doverlo
		/// ricompilare.
		/// 
		/// I valori sono:  -1 = uso sempre la foto grande<br/>
		///                 -2 = uso sempre la foto piccola<br/>
		///                 -3 = ridurre la foto grande di un valore
		///                      opportuno in base alla grandezza di ciascun provino da stampare.
		///                      Potrebbe però portare via molto tempo di elaborazione.<br/>
		///                > 0 = indica una soglia. Esempio se impostato a 20 significa che se nella
		///                      singola pagina ci sono più di 20 provini, uso la foto piccola (else grande).
		///                      <br/>
		/// </summary>
		public int tecSogliaStampaProvini {
			get;
			set;
		}

		/// <summary>
		/// Questo valore indica l'Offset di rientro dello stampiglio dai lati orizzontali della foto
		/// </summary
		public int stampigliMarginBottom
		{
			get;
			set;
		}

		/// <summary>
		/// Questo valore indica l'Offset di rientro dello stampiglio dai lati verticali della foto
		/// </summary
		public int stampigliMarginRight
		{
			get;
			set;
		}

		public Griglia [] prefGalleryViste {
			get;
			set;
		}


		/// <summary>
		/// Tutti i dati di posizionamento della finestra dello slide show
		/// </summary>
		public GeometriaFinestra geometriaFinestraSlideShow {
			set;
			get;
		}


		/// <summary>
		/// Se impostato a true, quando si scaricano le foto, se queste sono verticali,
		/// viene introdotta una rotazione di +90° o -90°
		/// </summary>
		public bool autoRotazione {
			get;
			set;
		}
		
		public int fontSizeStampaFoto {
			get;
			set;
		}

	}
}
