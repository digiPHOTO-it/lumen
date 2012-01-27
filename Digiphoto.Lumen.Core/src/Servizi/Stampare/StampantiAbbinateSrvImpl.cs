using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Data.Objects;

namespace Digiphoto.Lumen.Servizi.Stampare
{
    public class StampantiAbbinateSrvImpl : ServizioImpl, IStampantiAbbinateSrv
    {
        private IList<StampanteAbbinata> listStampantiAbbinate = null;

        public StampantiAbbinateSrvImpl()
        {
            this.listStampantiAbbinate = new List<StampanteAbbinata>();
        }

        public void addAbbinamento(StampanteAbbinata stampanteAbbinata)
        {
            this.listStampantiAbbinate.Add(stampanteAbbinata);
        }

        public void removeAbbinamento(StampanteAbbinata stampanteAbbinata)
        {
            this.listStampantiAbbinate.Remove(stampanteAbbinata);
        }

        public IList<StampanteAbbinata> listaStampantiAbbinate(String stampantiAbbinate)
        {
            IList<StampanteAbbinata> list = new List<StampanteAbbinata>();
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
                using (LumenEntities dbContext = new LumenEntities())
                {
                    //formatoCarta = dbContext.ExecuteStoreCommand("SELECT * FROM FormatiCarta WHERE descrizione = {0} AND prezzo = {1}", formato, prezzo);

                    //ObjectQuery<FormatoCarta> contactQuery = dbContext.FormatiCarta.Where("it.descrizione = @ln AND it.prezzo = @fn",
                    //new ObjectParameter("ln", formato),
                    //new ObjectParameter("fn", prezzo));
                    //formatoCarta = contactQuery.First<FormatoCarta>();
                    formatoCarta = dbContext.FormatiCarta.FirstOrDefault(f => f.descrizione.Equals(formato) && f.prezzo == prezzo);
                }
                StampanteInstallata stampanteInstallata = stampantiInstallateSrvImpl.stampanteInstallataByString(stampante);
                list.Add(new StampanteAbbinata(stampanteInstallata, formatoCarta));
            }
            return list;
        }

        public void sostituisciAbbinamento(IList<StampanteAbbinata> listaStampantiAbbinate)
        {
            this.listStampantiAbbinate = listaStampantiAbbinate;
        }


        public IList<StampanteAbbinata> listaStampantiAbbinate()
        {
            return this.listStampantiAbbinate;
        }

        public String listaStampantiAbbinateToString()
        {
            // A4;4;PDFCreator;PS1
            String stampantiAbbinateString = "";
            foreach(StampanteAbbinata stampanteAbbinata in this.listStampantiAbbinate){
                stampantiAbbinateString += stampanteAbbinata.FormatoCarta.descrizione + ";";
                stampantiAbbinateString += stampanteAbbinata.FormatoCarta.prezzo + ";";
                stampantiAbbinateString += stampanteAbbinata.StampanteInstallata.NomeStampante + ";";
                stampantiAbbinateString += stampanteAbbinata.StampanteInstallata.PortaStampante + "#";
            }
            return stampantiAbbinateString;
        }
    }
}
