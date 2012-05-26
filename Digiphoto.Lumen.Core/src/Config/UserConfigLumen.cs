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

		public static String CartellaFoto
		{
			get
			{
				string _cartellaFoto = UserConfigXML.Instance.getPropertiesValue("cartellaFoto");
				if (_cartellaFoto == null)
				{

					UserConfigXML.Instance.setPropertiesValue("cartellaFoto", ConfigDefaultValue.CARTELLA_FOTO);
					return ConfigDefaultValue.CARTELLA_FOTO;
				}
				return _cartellaFoto;
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("cartellaFoto", value);
			}
		}

		/// <summary>
		/// L'auto zoom, significa che la foto viene automaticamente ingrandita per
		/// essere stampata senza bordi bianchi, e riempire quindi interamente l'area stampabile.
		/// </summary>
		public static bool AutoZoomNoBordiBianchi
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("autoZoomNoBordiBianchi") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("autoZoomNoBordiBianchi", ConfigDefaultValue.AUTO_ZOOM_NO_BORDI_BIANCHI);
					return Boolean.Parse(ConfigDefaultValue.AUTO_ZOOM_NO_BORDI_BIANCHI);
				}
				return Boolean.Parse(UserConfigXML.Instance.getPropertiesValue("autoZoomNoBordiBianchi"));
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("autoZoomNoBordiBianchi", ""+value);
			}
		}

		public static ModoVendita ModoVendita
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("modoVendita") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("modoVendita", ConfigDefaultValue.MODO_VENDITA);
					return (ModoVendita)(short.Parse(ConfigDefaultValue.MODO_VENDITA));
				}
				return (ModoVendita)short.Parse(UserConfigXML.Instance.getPropertiesValue("modoVendita"));
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("modoVendita", "" + value);
			}
		}

		public static bool EraseFotoMemoryCard
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("eraseFotoMemoryCard") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("eraseFotoMemoryCard", ConfigDefaultValue.ERASE_FOTO_MEMORY_CARD);
					return ConfigDefaultValue.MODO_VENDITA.Equals(0) ? true : false;
				}
				return UserConfigXML.Instance.getPropertiesValue("eraseFotoMemoryCard").Equals(0) ? true : false;;
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("eraseFotoMemoryCard", "" + value);
			}
		}

		public static int PixelLatoProvino
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("pixelLatoProvino") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("pixelLatoProvino", ConfigDefaultValue.PIXEL_LATO_PROVINO);
					return Int32.Parse(ConfigDefaultValue.PIXEL_LATO_PROVINO);
				}
				return Int32.Parse(UserConfigXML.Instance.getPropertiesValue("pixelLatoProvino"));
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("pixelLatoProvino", "" + value);
			}
		}

        public static String UserConfigConnectionString
        {
            get
            {
				if (UserConfigXML.Instance.getPropertiesValue("connectionString")==null)
				{
					//UserConfigXML.Instance.setPropertiesValue("connectionString", ConfigDefaultValue.);
					return UserConfigXML.Instance.getPropertiesValue("connectionString");
				}
				return UserConfigXML.Instance.getPropertiesValue("connectionString");
            }

            set
            {
                UserConfigXML.Instance.setPropertiesValue("connectionString", value);
            }
        }

		public static int GiorniDeleteFoto
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("giorniDeleteFoto") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("giorniDeleteFoto", ConfigDefaultValue.GIORNI_DELETE_FOTO);
					return Int32.Parse(ConfigDefaultValue.GIORNI_DELETE_FOTO);
				}
				return Int32.Parse(UserConfigXML.Instance.getPropertiesValue("giorniDeleteFoto"));
			}
			set
			{
				UserConfigXML.Instance.setPropertiesValue("giorniDeleteFoto", ""+value);
			}
		}


		public static string CartellaMaschere
		{
			get
			{
				if (String.IsNullOrEmpty(UserConfigXML.Instance.getPropertiesValue("cartellaMaschere")))
				{
					UserConfigXML.Instance.setPropertiesValue("cartellaMaschere", ConfigDefaultValue.CARTELLA_MASCHERE);
					return ConfigDefaultValue.CARTELLA_MASCHERE;
				}
				return UserConfigXML.Instance.getPropertiesValue("cartellaMaschere");
			}

			set 
			{
				UserConfigXML.Instance.setPropertiesValue("cartellaMaschere", "" + value);
			}
		}

        public static String StampantiAbbinate
        {
            get
            {
				string _stampantiAbbinate = UserConfigXML.Instance.getPropertiesValue("stampantiAbbinate");
				if (_stampantiAbbinate==null)
				{
					UserConfigXML.Instance.setPropertiesValue("stampantiAbbinate", ConfigDefaultValue.STAMPANTI_ABBINATE);
					return ConfigDefaultValue.STAMPANTI_ABBINATE;
				}
				return _stampantiAbbinate;
            }
            set
            {
                UserConfigXML.Instance.setPropertiesValue("stampantiAbbinate", value);
            }
        }

		public static Boolean ProiettaDiapo
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("proiettaDiapo") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("proiettaDiapo", ConfigDefaultValue.PROIETTA_DIAPO);
					return Boolean.Parse(ConfigDefaultValue.PROIETTA_DIAPO);
				}
				return Boolean.Parse(UserConfigXML.Instance.getPropertiesValue("proiettaDiapo"));
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("proiettaDiapo", ""+value);
			}
		}

        public static String DbNomeDbVuoto
        {
            get
            {
				string _dbNomeDbVuoto = UserConfigXML.Instance.getPropertiesValue("dbNomeDbVuoto");
				if (_dbNomeDbVuoto==null)
				{
					UserConfigXML.Instance.setPropertiesValue("dbNomeDbVuoto", ConfigDefaultValue.DB_NOME_DB_VUOTO);
					return ConfigDefaultValue.DB_NOME_DB_VUOTO;
				}
				return _dbNomeDbVuoto;
            }
            set
            {
                UserConfigXML.Instance.setPropertiesValue("dbNomeDbVuoto", value);
            }
        }

        public static String DbNomeDbPieno
        {
            get
            {
				string _dbNomeDbPieno = UserConfigXML.Instance.getPropertiesValue("dbNomeDbPieno");
				if (_dbNomeDbPieno == null)
				{
					UserConfigXML.Instance.setPropertiesValue("dbNomeDbPieno", ConfigDefaultValue.DB_NOME_DB_PIENO);
					return ConfigDefaultValue.DB_NOME_DB_PIENO;
				}
				return _dbNomeDbPieno;
            }
            set
            {
                UserConfigXML.Instance.setPropertiesValue("dbNomeDbPieno", value);
            }
        }

        public static String DbCartella
        {
            get
            {
				string _dbCartella = UserConfigXML.Instance.getPropertiesValue("dbCartella");
				if (_dbCartella == null)
				{
					UserConfigXML.Instance.setPropertiesValue("dbCartella", ConfigDefaultValue.DB_CARTELLA);
					return ConfigDefaultValue.DB_CARTELLA;
				}
				return _dbCartella;
            }
            set
            {
                UserConfigXML.Instance.setPropertiesValue("dbCartella", value);
            }
        }

		public static string EstensioniGrafiche
		{
			get
			{
				string _estensioniGrafiche = UserConfigXML.Instance.getPropertiesValue("estensioniGrafiche");
				if (_estensioniGrafiche == null)
				{
					UserConfigXML.Instance.setPropertiesValue("estensioniGrafiche", ConfigDefaultValue.ESTENSIONI_GRAFICHE);
					return ConfigDefaultValue.ESTENSIONI_GRAFICHE;
				}
				return _estensioniGrafiche;
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("estensioniGrafiche", value);
			}
		}

		public static string EditorImmagini
		{
			get
			{
				string _editorImmagini = UserConfigXML.Instance.getPropertiesValue("editorImmagini");
				if (_editorImmagini == null)
				{
					UserConfigXML.Instance.setPropertiesValue("editorImmagini", ConfigDefaultValue.EDITOR_IMMAGINI);
					return ConfigDefaultValue.EDITOR_IMMAGINI;
				}
				return _editorImmagini;
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("editorImmagini", value);
			}
		}

		public static string EditorImmaginiMultiArgs
		{
			get
			{
				string _editorImmaginiMultiArgs = UserConfigXML.Instance.getPropertiesValue("editorImmaginiMultiArgs");
				if (_editorImmaginiMultiArgs == null)
				{
					UserConfigXML.Instance.setPropertiesValue("editorImmaginiMultiArgs", ConfigDefaultValue.EDITOR_IMMAGINI_MULTI_ARGS);
					return ConfigDefaultValue.EDITOR_IMMAGINI_MULTI_ARGS;
				}
				return _editorImmaginiMultiArgs;
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("editorImmaginiMultiArgs", value);
			}
		}

		public static string CodicePuntoVendita
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("codicePuntoVendita") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("codicePuntoVendita", ConfigDefaultValue.CODICE_PUNTO_VENDITA);
					return ConfigDefaultValue.CODICE_PUNTO_VENDITA;
				}
				return UserConfigXML.Instance.getPropertiesValue("codicePuntoVendita");
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("codicePuntoVendita", "" + value);
			}
		}

		public static string DescrizionePuntoVendita
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("descrizionePuntoVendita") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("descrizionePuntoVendita", ConfigDefaultValue.DESCRIZIONE_PUNTO_VENDITA);
					return ConfigDefaultValue.DESCRIZIONE_PUNTO_VENDITA;
				}
				return UserConfigXML.Instance.getPropertiesValue("descrizionePuntoVendita");
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("descrizionePuntoVendita", "" + value);
			}
		}

		public static string DestMasterizza
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("destMasterizza") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("destMasterizza", ConfigDefaultValue.DEST_MASTERIZZA);
					return ConfigDefaultValue.DEST_MASTERIZZA;
				}
				return UserConfigXML.Instance.getPropertiesValue("destMasterizza");
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("destMasterizza", value);
			}
		}

		public static string DefaultMasterizzatore
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("defaultMasterizzatore") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("defaultMasterizzatore", ConfigDefaultValue.DEFAULT_MASTERIZZATORE);
					return ConfigDefaultValue.DEFAULT_MASTERIZZATORE;
				}
				return UserConfigXML.Instance.getPropertiesValue("defaultMasterizzatore");
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("defaultMasterizzatore", value);
			}
		}

		public static string DefaultChiavetta
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("defaultChiavetta") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("defaultChiavetta", ConfigDefaultValue.DEFAULT_CHIAVETTA);
					return ConfigDefaultValue.DEFAULT_CHIAVETTA;
				}
				return UserConfigXML.Instance.getPropertiesValue("defaultChiavetta");
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("defaultChiavetta", value);
			}
		}

		public static Boolean IsWindowPubblicaVisibile
		{
			get
			{
				if (UserConfigXML.Instance.getPropertiesValue("isWindowPubblicaVisibile") == null)
				{
					UserConfigXML.Instance.setPropertiesValue("isWindowPubblicaVisibile", ConfigDefaultValue.IS_WINDOW_PUBBLICA_VISIBILE);
					return Boolean.Parse(ConfigDefaultValue.IS_WINDOW_PUBBLICA_VISIBILE);
				}
				return Boolean.Parse(UserConfigXML.Instance.getPropertiesValue("isWindowPubblicaVisibile"));
			}

			set
			{
				UserConfigXML.Instance.setPropertiesValue("isWindowPubblicaVisibile", ""+value);
			}
		}

		public static Boolean stampiglioGiornata {
			get {
				if( UserConfigXML.Instance.getPropertiesValue( "stampiglioGiornata" ) == null ) {
					UserConfigXML.Instance.setPropertiesValue( "stampiglioGiornata", ConfigDefaultValue.STAMPIGLIO_GIORNATA );
					return Boolean.Parse( ConfigDefaultValue.STAMPIGLIO_GIORNATA );
				}
				return Boolean.Parse( UserConfigXML.Instance.getPropertiesValue( "stampiglioGiornata" ) );
			}

			set {
				UserConfigXML.Instance.setPropertiesValue( "stampiglioGiornata", "" + value );
			}
		}

		public static Boolean stampiglioOperatore {
			get {
				if( UserConfigXML.Instance.getPropertiesValue( "stampiglioOperatore" ) == null ) {
					UserConfigXML.Instance.setPropertiesValue( "stampiglioOperatore", ConfigDefaultValue.STAMPIGLIO_OPERATORE );
					return Boolean.Parse( ConfigDefaultValue.STAMPIGLIO_OPERATORE );
				}
				return Boolean.Parse( UserConfigXML.Instance.getPropertiesValue( "stampiglioOperatore" ) );
			}

			set {
				UserConfigXML.Instance.setPropertiesValue( "stampiglioOperatore", "" + value );
			}
		}

		public static Boolean stampiglioNumFoto {
			get {
				if( UserConfigXML.Instance.getPropertiesValue( "stampiglioNumFoto" ) == null ) {
					UserConfigXML.Instance.setPropertiesValue( "stampiglioNumFoto", ConfigDefaultValue.STAMPIGLIO_NUMFOTO );
					return Boolean.Parse( ConfigDefaultValue.STAMPIGLIO_NUMFOTO );
				}
				return Boolean.Parse( UserConfigXML.Instance.getPropertiesValue( "stampiglioNumFoto" ) );
			}

			set {
				UserConfigXML.Instance.setPropertiesValue( "stampiglioNumFoto", "" + value );
			}
		}

    }
}
