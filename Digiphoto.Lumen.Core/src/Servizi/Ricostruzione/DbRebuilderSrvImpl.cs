using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Config;
using System.IO;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Model;
using log4net;
using Digiphoto.Lumen.Util;
using System.Transactions;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Applicazione;

namespace Digiphoto.Lumen.Servizi.Ricostruzione {

	public class DbRebuilderSrvImpl : ServizioImpl, IDbRebuilderSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(DbRebuilderSrvImpl) );

		public DbRebuilderSrvImpl() {
		}

		private bool analisiEffettuata = false;


		public void analizzare() {

			_giornale.Info( "Inizio analisi per rebuild del database" );

			inizializzazioni();

			analizzare1();
			analizzare2();

			analisiEffettuata = true;
		}

		/// <summary>
		/// Parto dal filesystem e confronto con il db
		/// </summary>
		public void analizzare1() {

			DirectoryInfo dirInfoRoot = new DirectoryInfo( Configurazione.UserConfigLumen.cartellaFoto );
			string cosa = "*" + Configurazione.suffissoCartellaGiorni;
			foreach( DirectoryInfo dig in dirInfoRoot.EnumerateDirectories( cosa ) ) {
				
				diGiornataCorr = dig;
				_giornale.Debug( "Considero cartella per giornata " + diGiornataCorr );

				cosa = "*" + Configurazione.suffissoCartellaFoto;
				foreach( DirectoryInfo dif in dig.EnumerateDirectories( cosa ) ) {
				
					diFotografoCorr = dif;
					_giornale.Debug( "Considero cartella per fotografo " + diFotografoCorr );

					controllaFotografoMancante();

					foreach( FileInfo fileInfo in dif.EnumerateFiles() ) {

						if( estensioniAmmesse.Contains( fileInfo.Extension.ToLower() ) ) {

							// Ok trovata una foto. La tratto.
							controllaFotoMancante( fileInfo );
						}
					}
				}
			}
		}

		private void controllaFotoMancante( FileInfo fInfoFoto ) {

			// Se non c'è già nel database, allora la aggiungo all'elenco
			string nomeRel = PathUtil.nomeRelativoFoto( fInfoFoto );

			if( ! UnitOfWorkScope.CurrentObjectContext.Fotografie.Any( f => f.nomeFile == nomeRel ) ) {
				fiFotosMancanti.Add( fInfoFoto );
			}
		}

		private void controllaFotografoMancante() {

			string idFotografo = Path.GetFileNameWithoutExtension( diFotografoCorr.Name );

			// Ok è già nella lista dei mancanti
			if( idsFotografiMancanti.Contains( idFotografo ) )
				return;

			// Ok è già nel db
			if( UnitOfWorkScope.CurrentObjectContext.Fotografi.SingleOrDefault( f => f.id == idFotografo ) != null )
				return;

			idsFotografiMancanti.Add( idFotografo );
		}

		private void creaFotografoMancante( string id ) {
			Fotografo mancante = new Fotografo();
			mancante.id = id;
			mancante.cognomeNome = id;
			mancante.iniziali = Convert.ToString( ++contaFotografiAggiunti );
			mancante.umano = true;
			mancante.attivo = true;
			mancante.note = "Generato automaticamente da DbRebuilder";
			UnitOfWorkScope.CurrentObjectContext.Fotografi.Add( mancante );
		}

		/// <summary>
		/// Parto dal db e confronto con il filesystem
		/// </summary>
		void analizzare2() {
					
			foreach( Fotografia foto in UnitOfWorkScope.CurrentObjectContext.Fotografie ) {
				if( ! File.Exists( PathUtil.nomeCompletoOrig( foto ) ) )
					fotografieSenzaImmagini.Add( foto );
			}

		}

		private void inizializzazioni() {

			estensioniAmmesse = Configurazione.UserConfigLumen.estensioniGrafiche.Split( ';' );
			
			idsFotografiMancanti = new List<string>();

			fiFotosMancanti = new List<FileInfo>();

			fotografieSenzaImmagini = new List<Fotografia>();
		}


		/// <summary>
		/// Giorno corrente in esame
		/// </summary>
		private DirectoryInfo diGiornataCorr {
			get;
			set;
		}

		/// <summary>
		/// Fotografo corrente in esame
		/// </summary>
		private DirectoryInfo diFotografoCorr {
			get;
			set;
		}

		string [] estensioniAmmesse {
			get;
			set;
		}

		List<String> idsFotografiMancanti {
			get;
			set;
		}

		List<FileInfo> fiFotosMancanti {
			get;
			set;
		}

		/// <summary>
		/// Queste fotografie non hanno neanche una immagine su disco
		/// </summary>
		List<Fotografia> fotografieSenzaImmagini {
			get;
			set;
		}

		public int contaFotografieSenzaImmagini {
			get {
				return fotografieSenzaImmagini.Count;
			}
		}

		public int contaFotoMancanti {
			get {
				return fiFotosMancanti.Count;
			}
		}

		public int contaFotoAggiunte {
			get;
			private set;
		}

		public int contaFotoEliminate {
			get;
			private set;
		}

		public int contaFotografiAggiunti {
			get;
			private set;
		}

		public bool necessarioRicostruire {
			get {
				return contaFotoMancanti > 0 || contaFotografiMancanti > 0 || contaFotografieSenzaImmagini > 0;
			}
		}

		public int contaFotografiMancanti {
			get {
				return idsFotografiMancanti.Count;
			}
		}

		public void ricostruire() {

			if( !analisiEffettuata )
				throw new InvalidOperationException( "Prima di ricostruire, occorre lanciare l'analisi" );

			foreach( string idFotografoMancante in idsFotografiMancanti ) {
				creaFotografoMancante( idFotografoMancante );
			}
			int test = UnitOfWorkScope.CurrentObjectContext.SaveChanges();

			int ultimoNumFoto = NumeratoreFotogrammi.incrementaNumeratoreFoto( fiFotosMancanti.Count );
			int conta = 0;

			// Creo eventuali fotografie mancanti
			contaFotoAggiunte = 0;
			foreach( FileInfo fiFotoMancante in fiFotosMancanti )
				creaFotografiaMancante( fiFotoMancante, ++conta + ultimoNumFoto );

			// Elimino eventuali fotografie che non hanno più l'immagine jpg su disco
			contaFotoEliminate = 0;
			foreach( Fotografia f in fotografieSenzaImmagini ) {
				UnitOfWorkScope.CurrentObjectContext.Fotografie.Remove( f );
				++contaFotoEliminate;
			}

			// Salvo il contesto
			int quanti = UnitOfWorkScope.CurrentObjectContext.SaveChanges();

			// Piccolo controllo di paranoia
			if( quanti != fotografieSenzaImmagini.Count ) {
				_giornale.Warn( "Dovevo cancellare " + contaFotografieSenzaImmagini + " fotografie ma in test mi dice: " + quanti );
			}

		}

		private void creaFotografiaMancante( FileInfo fiFotoMancante, int numFotogramma ) {

			bool success = false;


//			using( TransactionScope transaction = new TransactionScope() ) {

				try {

					Fotografia foto = new Fotografia();
					foto.id = Guid.NewGuid();
					foto.dataOraAcquisizione = fiFotoMancante.CreationTime;
					foto.giornata = Convert.ToDateTime( PathUtil.giornoFromPath( fiFotoMancante.FullName ) );
					string idFotografo = PathUtil.fotografoIDFromPath( fiFotoMancante.FullName );
					foto.fotografo = UnitOfWorkScope.CurrentObjectContext.Fotografi.SingleOrDefault( f => f.id == idFotografo );
					foto.numero = numFotogramma;

					// il nome del file, lo memorizzo solamente relativo
					// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
					// Questo perché le stesse foto le devono vedere altri computer della rete che
					// vedono il percorso condiviso in maniera differente.
					foto.nomeFile = PathUtil.nomeRelativoFoto( fiFotoMancante );

					// caricaMetadatiImmagine( foto );

					UnitOfWorkScope.CurrentObjectContext.Fotografie.Add( foto );

					int test = UnitOfWorkScope.CurrentObjectContext.SaveChanges();

					// Mark the transaction as complete.
					success = true;
//					transaction.Complete();
					contaFotoAggiunte++;

					_giornale.Debug( "Inserita nuova foto: " + foto.ToString() + "test=" + test );

				} catch( Exception ee ) {
					_giornale.Error( "Non riesco ad inserire una foto. Nel db non c'è ma nel filesystem si: " + fiFotoMancante, ee );
				}

				
//			}

				_giornale.Debug( "success = " + success );
		}




	}
}
