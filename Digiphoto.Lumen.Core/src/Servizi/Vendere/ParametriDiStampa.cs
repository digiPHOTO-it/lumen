﻿using Digiphoto.Lumen.Model;

namespace Digiphoto.Lumen.Servizi.Vendere {

	public class ParametriDiStampa {

		private FormatoCarta _formatoCarta;
		public FormatoCarta FormatoCarta {
			get {
				return _formatoCarta;
			}
			set {
				if( _formatoCarta != value ) {
					_formatoCarta = value;
				}
			}
		}

		private string _nomeStampante;
		public string NomeStampante {
			get {
				return _nomeStampante;
			}
			set {
				if( _nomeStampante != value ) {
					_nomeStampante = value;
				}
			}
		}
		
		private short _quantita;
		public short Quantita {
			get {
				return _quantita;
			}
			set {
				if( _quantita != value ) {
					_quantita = value;
				}
			}
		}

		private decimal _prezzoLordoUnitario;
		public decimal PrezzoLordoUnitario {
			get {
				return _prezzoLordoUnitario;
			}
			set {
				if( _prezzoLordoUnitario != value ) {
					_prezzoLordoUnitario = value;
				}
			}
		}

		private decimal _prezzoNettoTotale;
		public decimal PrezzoNettoTotale {
			get {
				return _prezzoNettoTotale;
			}
			set {
				if( _prezzoNettoTotale != value ) {
					_prezzoNettoTotale = value;
				}
			}
		}

		private bool _bordiBianchi;
		public bool BordiBianchi {
			get {
				return _bordiBianchi;
			}
			set {
				if( _bordiBianchi != value ) {
					_bordiBianchi = value;
				}
			}
		}
	}

}
