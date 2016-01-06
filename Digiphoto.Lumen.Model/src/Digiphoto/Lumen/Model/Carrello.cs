using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Model {

	public partial class Carrello {

		public const string TIPORIGA_STAMPA = "S";
		public const string TIPORIGA_MASTERIZZATA = "M";

		public override bool Equals( object altro ) {
			bool uguali = false;
			if( altro != null && altro is Carrello ) {
				uguali = this.id.Equals( ((Carrello)altro).id );
			}

			return uguali;
		}

		public override int GetHashCode() {
			int hash = 7;
			hash = 31 * hash + (null == this.id ? 0 : this.id.GetHashCode());
			return hash;
		}

        public static string Not(string d)
        {
            return d.Equals(Carrello.TIPORIGA_MASTERIZZATA) ? Carrello.TIPORIGA_STAMPA : Carrello.TIPORIGA_MASTERIZZATA;
        }

        public class ParametriDiStampa
        {
            private FormatoCarta _formatoCarta;
            public FormatoCarta FormatoCarta
            {
                get
                {
                    return _formatoCarta;
                }
                set
                {
                    if(_formatoCarta != value)
                    {
                        _formatoCarta = value;
                    }
                }
            }

            private string _nomeStampante;
            public string NomeStampante
            {
                get
                {
                    return _nomeStampante;
                }
                set
                {
                    if(_nomeStampante != null)
                    {
                        _nomeStampante = value;
                    }
                }
            }


            private short _quantita;
            public short Quantita
            {
                get
                {
                    return _quantita;
                }
                set
                {
                    if (_quantita != value)
                    {
                        _quantita = value;
                    }
                }
            }

            private decimal _prezzoLordoUnitario;
            public decimal PrezzoLordoUnitario
            {
                get
                {
                    return _prezzoLordoUnitario;
                }
                set
                {
                    if (_prezzoLordoUnitario != value)
                    {
                        _prezzoLordoUnitario = value;
                    }
                }
            }

            private decimal _prezzoNettoTotale;
            public decimal PrezzoNettoTotale
            {
                get
                {
                    return _prezzoNettoTotale;
                }
                set
                {
                    if (_prezzoNettoTotale != value)
                    {
                        _prezzoNettoTotale = value;
                    }
                }
            }

        }
    }
}
