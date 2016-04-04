using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Digiphoto.Lumen.Core.Servizi.Stampare;

namespace Digiphoto.Lumen.Servizi.Stampare
{
	public class ParamStampaProvini : ParamStampa
	{
		private bool _macchiaProvini;
		public bool macchiaProvini
		{
			get
			{
				return _macchiaProvini;
			}
			set
			{
				if (_macchiaProvini != value)
				{
					_macchiaProvini = value;
					OnPropertyChanged("macchiaProvini");
				}
			}
		}

		private string _intestazione;
		public string intestazione
		{
			get
			{
				return _intestazione;
			}
			set
			{
				if (_intestazione != value)
				{
					_intestazione = value;
					OnPropertyChanged("intestazione");
				}
			}
		}

		public Margini margini { get; set; }

		private int _numeroRighe;
		public int numeroRighe
		{
			get
			{
				return _numeroRighe;
			}
			set
			{
				if (_numeroRighe != value)
				{
					_numeroRighe = value;
					OnPropertyChanged("numeroRighe");
				}
			}
		}

		private int _numeroColonne;
		public int numeroColonne
		{
			get
			{
				return _numeroColonne;
			}
			set
			{
				if( _numeroColonne != value ) {
					_numeroColonne = value;
					OnPropertyChanged( "numeroColonne" );
				}
			}
		}

		private short _numPag;
		public short numPag
		{
			get
			{
				return _numPag;
			}
			set
			{
				if (_numPag != value)
				{
					_numPag = value;
					OnPropertyChanged("numPag");
				}
			}
		}

		private bool _rompePerGiorno;
		/// <summary>
		/// Saltare pagina al cambio di giorno
		/// </summary>
		public bool rompePerGiorno {
			get {
				return _rompePerGiorno;
			}
			set {
				if( _rompePerGiorno != value ) {
					_rompePerGiorno = value;
					OnPropertyChanged( "rompePerGiorno" );
				}
			}
		}

		public ParamStampaProvini()
		{
			this.autoRuota = true;
			this.rompePerGiorno = true;
			this.numCopie = 1;
			this.margini = new Margini();
		}

		public override string ToString()
		{

			StringBuilder s = new StringBuilder();
			if (intestazione != null)
				s.Append(" Intestazione=" + intestazione);
			if (nomeStampante != null)
				s.Append(" Stampante=" + nomeStampante);
			if (formatoCarta != null)
				s.Append(" Carta=" + formatoCarta.descrizione);
			s.Append(" Righe=" + numeroRighe);
			s.Append(" Righe=" + numeroColonne);
			s.Append(" Copie=" + numCopie);

			return s.ToString();
		}
	}
}
