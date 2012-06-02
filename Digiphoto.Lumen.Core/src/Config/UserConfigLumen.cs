using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.src.Config;
using Digiphoto.Lumen.Servizi.Vendere;
using System.IO;

namespace Digiphoto.Lumen.Config
{
	public struct Stampigli {
		public bool numFoto;
		public bool operatore;
		public bool giornata;
	}

    public class UserConfigLumen
    {
		private string _cartellaFoto;
		public string CartellaFoto
		{
			get
			{
				if (_cartellaFoto == null)
				{
					return ConfigDefaultValue.CARTELLA_FOTO;
				}
				return _cartellaFoto;
			}

			set
			{
				_cartellaFoto = value;
			}
		}

		/// <summary>
		/// L'auto zoom, significa che la foto viene automaticamente ingrandita per
		/// essere stampata senza bordi bianchi, e riempire quindi interamente l'area stampabile.
		/// </summary>
		private bool? _autoZoomNoBordiBianchi;
		public bool AutoZoomNoBordiBianchi
		{
			get
			{
				if (_autoZoomNoBordiBianchi == null)
				{
					return Boolean.Parse(ConfigDefaultValue.AUTO_ZOOM_NO_BORDI_BIANCHI);
				}
				return (bool)_autoZoomNoBordiBianchi;
			}

			set
			{
				_autoZoomNoBordiBianchi = value;
			}
		}

		private ModoVendita _modoVendita = (ModoVendita)(short.Parse(ConfigDefaultValue.MODO_VENDITA));
		public ModoVendita ModoVendita
		{
			get
			{
				return _modoVendita;
			}

			set
			{
				_modoVendita = value;
			}
		}

		private bool? _eraseFotoMemoryCard;
		public bool EraseFotoMemoryCard
		{
			get
			{
				if (_eraseFotoMemoryCard == null)
				{
					return ConfigDefaultValue.MODO_VENDITA.Equals(0) ? true : false;
				}
				return (bool)_eraseFotoMemoryCard;
			}

			set
			{
				_eraseFotoMemoryCard = value;
			}
		}

		private int? _pixelLatoProvino;
		public int PixelLatoProvino
		{
			get
			{
				if (_pixelLatoProvino == null)
				{
					return Int32.Parse(ConfigDefaultValue.PIXEL_LATO_PROVINO);
				}
				return (int)_pixelLatoProvino;
            }

            set
            {
				_pixelLatoProvino = value;
            }
        }

		private int? _giorniDeleteFoto;
		public int GiorniDeleteFoto
		{
			get
			{
				if (_giorniDeleteFoto == null)
				{
					return Int32.Parse(ConfigDefaultValue.GIORNI_DELETE_FOTO);
				}
				return (int)_giorniDeleteFoto;
			}
			set
			{
				_giorniDeleteFoto = value;
			}
		}

		private string _cartellaMaschere;
		public string CartellaMaschere
		{
			get
			{
				if (_cartellaMaschere == null)
				{
					return ConfigDefaultValue.CARTELLA_MASCHERE;
				}
				return _cartellaMaschere;
			}

			set 
			{
				_cartellaMaschere = value;
			}
		}

		private string _stampantiAbbinate;
		public string StampantiAbbinate
        {
            get
            {
				if (_stampantiAbbinate==null)
				{
					return ConfigDefaultValue.STAMPANTI_ABBINATE;
				}
				return _stampantiAbbinate;
            }
            set
            {
                _stampantiAbbinate = value;
            }
        }

		private bool? _proiettaDiapo;
		public bool ProiettaDiapo
		{
			get
			{
				if (_proiettaDiapo == null)
				{
					return Boolean.Parse(ConfigDefaultValue.PROIETTA_DIAPO);
				}
				return (bool)_proiettaDiapo;
			}

			set
			{
				_proiettaDiapo = value;
			}
		}

		private string _dbNomeDbVuoto;
		public string DbNomeDbVuoto
        {
            get
            {
				if (_dbNomeDbVuoto==null)
				{
					return ConfigDefaultValue.DB_NOME_DB_VUOTO;
				}
				return _dbNomeDbVuoto;
            }
            set
            {
                _dbNomeDbVuoto = value;
            }
        }

		private string _dbNomeDbPieno;
		public string DbNomeDbPieno
        {
            get
            {
				if (_dbNomeDbPieno == null)
				{
					return ConfigDefaultValue.DB_NOME_DB_PIENO;
				}
				return _dbNomeDbPieno;
            }
            set
            {
                _dbNomeDbPieno = value;
            }
        }

		private string _dbCartella;
		public string DbCartella
        {
            get
            {
				if (_dbCartella == null)
				{
					return ConfigDefaultValue.DB_CARTELLA;
				}
				return _dbCartella;
            }
            set
            {
                _dbCartella = value;
            }
        }

		private string _estensioniGrafiche;
		public string EstensioniGrafiche
		{
			get
			{
				if (_estensioniGrafiche == null)
				{
					return ConfigDefaultValue.ESTENSIONI_GRAFICHE;
				}
				return _estensioniGrafiche;
			}

			set
			{
				_estensioniGrafiche = value;
			}
		}

		private string _editorImmagini;
		public string EditorImmagini
		{
			get
			{
				if (_editorImmagini == null)
				{
					return ConfigDefaultValue.EDITOR_IMMAGINI;
				}
				return _editorImmagini;
			}

			set
			{
				_editorImmagini = value;
			}
		}

		private string _editorImmaginiMultiArgs;
		public string EditorImmaginiMultiArgs
		{
			get
			{
				if (_editorImmaginiMultiArgs == null)
				{
					return ConfigDefaultValue.EDITOR_IMMAGINI_MULTI_ARGS;
				}
				return _editorImmaginiMultiArgs;
			}

			set
			{
				_editorImmaginiMultiArgs = value;
			}
		}

		private string _codicePuntoVendita;
		public string CodicePuntoVendita
		{
			get
			{
				if (_codicePuntoVendita == null)
				{
					return ConfigDefaultValue.CODICE_PUNTO_VENDITA;
				}
				return _codicePuntoVendita;
			}

			set
			{
				_codicePuntoVendita = value;
			}
		}

		private string _descrizionePuntoVendita;
		public string DescrizionePuntoVendita
		{
			get
			{
				if (_descrizionePuntoVendita == null)
				{
					return ConfigDefaultValue.DESCRIZIONE_PUNTO_VENDITA;
				}
				return _descrizionePuntoVendita;
			}

			set
			{
				_descrizionePuntoVendita = value;
			}
		}

		private string _destMasterizza;
		public string DestMasterizza
		{
			get
			{
				if (_destMasterizza == null)
				{
					return ConfigDefaultValue.DEST_MASTERIZZA;
				}
				return _destMasterizza;
			}

			set
			{
				_destMasterizza = value;
			}
		}

		private string _defaultMasterizzatore;
		public string DefaultMasterizzatore
		{
			get
			{
				if (_defaultMasterizzatore == null)
				{
					return ConfigDefaultValue.DEFAULT_MASTERIZZATORE;
				}
				return _defaultMasterizzatore;
			}

			set
			{
				_defaultMasterizzatore = value;
			}
		}

		private string _defaultChiavetta;
		public string DefaultChiavetta
		{
			get
			{
				if (_defaultChiavetta == null)
				{
					return ConfigDefaultValue.DEFAULT_CHIAVETTA;
				}
				return _defaultChiavetta;
			}

			set
			{
				_defaultChiavetta = value;
			}
		}

		private bool? _isWindowPubblicaVisibile;
		public bool IsWindowPubblicaVisibile
		{
			get
			{
				if (_isWindowPubblicaVisibile == null)
				{
					return Boolean.Parse(ConfigDefaultValue.IS_WINDOW_PUBBLICA_VISIBILE);
				}
				return (bool)_isWindowPubblicaVisibile;
			}

			set
			{
				_isWindowPubblicaVisibile = value;
			}
		}

		private bool? _stampiglioGiornata;
		public bool stampiglioGiornata 
		{
			get {
				if (_stampiglioGiornata == null)
				{
					return Boolean.Parse( ConfigDefaultValue.STAMPIGLIO_GIORNATA );
				}
				return (bool)_stampiglioGiornata;
			}

			set {
				_stampiglioGiornata = value;
			}
		}

		private bool? _stampiglioOperatore;
		public bool stampiglioOperatore 
		{
			get {
				if (_stampiglioOperatore == null)
				{
					return Boolean.Parse( ConfigDefaultValue.STAMPIGLIO_OPERATORE );
				}
				return (bool)_stampiglioOperatore;
			}

			set {
				_stampiglioOperatore = value;
			}
		}

		private bool? _stampiglioNumFoto;
		public bool stampiglioNumFoto
		{
			get {
				if (_stampiglioNumFoto == null)
				{
					return Boolean.Parse( ConfigDefaultValue.STAMPIGLIO_NUMFOTO );
				}
				return (bool)_stampiglioNumFoto;
			}

			set {
				_stampiglioNumFoto = value;
			}
		}

		public void SalvaUserConfig()
		{
			UserConfigXML.Instance.SaveUserConfig();
		}

    }
}
