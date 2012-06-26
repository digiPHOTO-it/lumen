using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Digiphoto.Lumen.Servizi.Stampare
{
	public class ParamStampaProvini : ParamStampa
	{
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

		public ParamStampaProvini()
		{
			this.autoRuota = true;
			this.numCopie = 1;
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
			if (numeroRighe != null)
				s.Append(" Righe=" + numeroRighe);
			if (numeroColonne != null)
				s.Append(" Righe=" + numeroColonne);
			s.Append(" Copie=" + numCopie);

			return s.ToString();
		}
	}
}
