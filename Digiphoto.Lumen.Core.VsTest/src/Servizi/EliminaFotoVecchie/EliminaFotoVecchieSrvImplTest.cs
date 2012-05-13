using Digiphoto.Lumen.Servizi.EliminaFotoVecchie;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Core.Database;
using System.Linq;
using System.Text;
using System.IO;
using System.Transactions;
using System.Diagnostics;
using Digiphoto.Lumen.Properties;
using Digiphoto.Lumen.Util;
using System.Data.Objects;
using System.Data.Common;
using Digiphoto.Lumen.Core.VsTest.Servizi.Scaricatore;

namespace Digiphoto.Lumen.Core.VsTest
{
    /// <summary>
    ///This is a test class for EliminaFotoVecchieSrvImpTest and is intended
    ///to contain all EliminaFotoVecchieSrvImpTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EliminaFotoVecchieSrvImplTest
    {

        private EliminaFotoVecchieSrvImpl _impl = new EliminaFotoVecchieSrvImpl();

		Fotografo _mario = null;

		private int giorni = 0;

        [TestInitialize()]
        public void initTest()
        {
            System.Diagnostics.Trace.WriteLine("INIZIO");
			LumenApplication app = LumenApplication.Instance;
			app.avvia();

            ScaricatoreFotoImplTest scaricatore = new ScaricatoreFotoImplTest();
            scaricatore.Init();
            scaricatore.scaricaFileTest();

			giorni = Settings.Default.giorniDeleteFoto;
            System.Diagnostics.Trace.WriteLine("GIORNI " + giorni);

			using (LumenEntities dbContext = new LumenEntities()) {
				_mario = Utilita.ottieniFotografoMario(dbContext);
			}
			_impl = new EliminaFotoVecchieSrvImpl();
			_impl.start();
        }

        [TestMethod()]
        public void TestDiChiSonoQuesteFoto()
        {
			using( new UnitOfWorkScope() ) {

				IList<String> listCartelleDaEliminare = _impl.getListaCartelleDaEliminare();
				if( listCartelleDaEliminare.Count() == 0 ) {
					System.Diagnostics.Trace.WriteLine( "Non vi sono Cartelle da Eliminare" );
					Assert.IsTrue( true );
				} else {
					foreach( String path in listCartelleDaEliminare ) {
						System.Diagnostics.Trace.WriteLine( path );
						Assert.IsNotNull( _impl.diChiSonoQuesteFoto( path ) );
					}
				}
			}
		}
        public void TestGetListaCartelleDaEliminare()
        {
             IList<String> listCartelleDaEliminare = _impl.getListaCartelleDaEliminare();
             if (listCartelleDaEliminare.Count()==0)
             {
                 System.Diagnostics.Trace.WriteLine("Non vi sono Cartelle da Eliminare");
                 Assert.IsTrue(cartelleDaEliminare() == 0);
			 }
			 else
			 {
                 foreach(String path in listCartelleDaEliminare){
                     System.Diagnostics.Trace.WriteLine(path);
                 }
                 System.Diagnostics.Trace.WriteLine("[Numero Cartelle da Eliminare]: " + listCartelleDaEliminare.Count());
                 Assert.IsTrue(cartelleDaEliminare() == listCartelleDaEliminare.Count());
			}
		}
		 public void TestElimina()
		 {
			 if (_impl.getListaCartelleDaEliminare().Count() == 0)
			 {
                 System.Diagnostics.Trace.WriteLine("Non vi sono Cartelle da Eliminare");
                 Assert.IsTrue(cartelleDaEliminare() == 0);
			 }
			 else
			 {
                 System.Diagnostics.Trace.WriteLine("I Seguenti Path saranno Eliminati");
				 IList<String> list = _impl.getListaCartelleDaEliminare();
				 foreach(String path in list){
					 Trace.WriteLine(path);
					_impl.elimina(path);
				 }
                 Assert.IsTrue(cartelleDaEliminare() == 0);
			 }
		 }

		 [TestMethod()]
         public void TestPathUtil()
		 {
             String fotografoID = PathUtil.fotografoIDFromPath(@"C:\Documents and Settings\All Users\Dati applicazioni\digiPHOTO\Lumen\Foto\2011-10-31.Gio\ROSSIMARIO.Fot");
             String giorno = PathUtil.giornoFromPath(@"C:\Documents and Settings\All Users\Dati applicazioni\digiPHOTO\Lumen\Foto\2011-10-31.Gio\ROSSIMARIO.Fot");
             System.Diagnostics.Trace.WriteLine("Fotografo: " + fotografoID);
             System.Diagnostics.Trace.WriteLine("Giorno: " + giorno);
             Assert.IsTrue(fotografoID.Equals("ROSSIMARIO"));
             Assert.IsTrue(giorno.Equals("2011-10-31"));
		 }

         [TestMethod()]
         public void TestEliminaAlbumNonReferenziati()
		 {
             _impl.eliminaAlbumNonReferenziati();
             Assert.IsTrue(true);
		 }

         [TestCleanup]
         public void Cleanup()
         {
             _impl.Dispose();
         }

         /**
          * Recupera le cartelle da eliminare lo utilizzo per confrontare il risultato con quello del metodo getListaCartelleDaEliminare
         */
         private int cartelleDaEliminare()
         {
             int count = 0;
             using (LumenEntities dbContext = new LumenEntities())
             {
                 DateTime dataIntervallo = DateTime.Now.AddDays(-giorni);

                 string queryString = @"SELECT COUNT(Fot.giornata) AS NumeroFoto, Fot.giornata FROM LumenEntities.Fotografie AS Fot GROUP BY Fot.giornata HAVING (Fot.giornata <= @data)";
                 ObjectQuery<DbDataRecord> contactQuery = new ObjectQuery<DbDataRecord>(queryString, dbContext.ObjectContext);

                 contactQuery.Parameters.Add(new ObjectParameter("data", DateTime.Now.AddDays(-giorni)));

                 foreach (DbDataRecord rec in contactQuery)
                 {
                     Console.WriteLine("NumeroFoto {0}; Giornata {1}", rec[0], rec[1]);
                     if (Convert.ToDateTime(rec[1]).Date <= Convert.ToDateTime(dataIntervallo).Date)
                     {
                         count++;
                     }
                 }
             }
             System.Diagnostics.Trace.WriteLine("[Numero Cartelle da Eliminare]: " + count);
             return count;
         }
    }
}
