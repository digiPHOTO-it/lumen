using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Config;
using System.ComponentModel;

namespace Digiphoto.Lumen.Servizi.Stampare
{
	public class ParamStampa : ICloneable, INotifyPropertyChanged
	{
		/**
		 * La stampante su cui vado a stampare, deve essere già opportunamente configurata
		 * per accettare il formato carta che sto per indicare.
		 * Il programma non interviene in nessun modo sulle impostazioni del formato o degli
		 * attributi della stampante.
		 * Basta solo il nome per sapere dove andare.
		 * 
		 * FACOLTATIVO
		 * Lasciare vuota property in modo che il servizio lo valorizzi in automatico.
		 */
		public string nomeStampante;

		/**
		 *  Decide automaticamente l'orientamento giusto per stampare
		 *  la foto in modo che riempia più possibile la carta.
		 *  Per esempio se la foto è verticale e l'area di stampa è orizzontale, viene concettualmente girata.
		 */
		public bool autoRuota;

		public FormatoCarta formatoCarta
		{
			get;
			set;
		}

		public short numCopie;

		public Stampigli stampigli
		{
			get;
			set;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
