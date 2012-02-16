using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Servizi.Stampare
{
    public class StampantiAbbinateSrvImpl : ServizioImpl, IStampantiAbbinateSrv
    {


        public StampantiAbbinateSrvImpl()
        {
			// Carico la lista degli abbinamenti correnti
			this.stampantiAbbinate = listaStampantiAbbinate( Configurazione.stampantiAbbinate );
        }

        public void addAbbinamento(StampanteAbbinata stampanteAbbinata)
        {
			this.stampantiAbbinate.Add( stampanteAbbinata );
        }

        public void removeAbbinamento(StampanteAbbinata stampanteAbbinata)
        {
			this.stampantiAbbinate.Remove( stampanteAbbinata );
        }

        public IList<StampanteAbbinata> listaStampantiAbbinate(String stampantiAbbinate)
        {
			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
			
			List<StampanteAbbinata> list = new List<StampanteAbbinata>();
            StampantiInstallateSrvImpl stampantiInstallateSrvImpl = new StampantiInstallateSrvImpl();
            String[] st = stampantiAbbinate.Split('#');
            for (int i = 0; i < st.Length; i++)
            {
                //formato A4 (12 euro) su Shinko S2115 [PS1]
                String formato = st[i].Split(';')[0];
                Decimal prezzo = Decimal.Parse(st[i].Split(';')[1]);
                String stampante = st[i].Split(';')[2];
                String porta = st[i].Split(';')[3];
                //String abbinamento = formato + euro + stampante + porta;

                FormatoCarta formatoCarta = null;
				

                //formatoCarta = dbContext.ExecuteStoreCommand("SELECT * FROM FormatiCarta WHERE descrizione = {0} AND prezzo = {1}", formato, prezzo);

                //ObjectQuery<FormatoCarta> contactQuery = dbContext.FormatiCarta.Where("it.descrizione = @ln AND it.prezzo = @fn",
                //new ObjectParameter("ln", formato),
                //new ObjectParameter("fn", prezzo));
                //formatoCarta = contactQuery.First<FormatoCarta>();
				formatoCarta = dbContext.FormatiCarta.FirstOrDefault() ; // f => f.descrizione.Equals( formato ) && f.prezzo == prezzo );
				if( formatoCarta != null ) {
					StampanteInstallata stampanteInstallata = stampantiInstallateSrvImpl.stampanteInstallataByString( stampante );
					list.Add( new StampanteAbbinata( stampanteInstallata, formatoCarta ) );
				}
            }

			// Ordino la lista per il valore di ordinamento impostato nel database nel formato carta.

			list.Sort( StampanteAbbinata.CompareByImportanza );
			return list;
        }

        public void sostituisciAbbinamento(IList<StampanteAbbinata> listaStampantiAbbinate)
        {
			this.stampantiAbbinate = listaStampantiAbbinate;
        }


        public IList<StampanteAbbinata> stampantiAbbinate {
			get;
			private set;
        }

        public String listaStampantiAbbinateToString()
        {
            // A4;4;PDFCreator;PS1
            String stampantiAbbinateString = "";
			foreach( StampanteAbbinata stampanteAbbinata in this.stampantiAbbinate ) {
                stampantiAbbinateString += stampanteAbbinata.FormatoCarta.descrizione + ";";
                stampantiAbbinateString += stampanteAbbinata.FormatoCarta.prezzo + ";";
                stampantiAbbinateString += stampanteAbbinata.StampanteInstallata.NomeStampante + ";";
                stampantiAbbinateString += stampanteAbbinata.StampanteInstallata.PortaStampante + "#";
            }
            return stampantiAbbinateString;
        }
    }
}
