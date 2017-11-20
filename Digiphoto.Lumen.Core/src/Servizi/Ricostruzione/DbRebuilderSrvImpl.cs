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
		public void analizzare()
		{
			this.analizzare( null );
		}

		public void analizzare( ParamRebuild paramRebuild ) { 

			_giornale.Info( "Inizio analisi per rebuild del database" );

			this.paramRebuild = paramRebuild;

			inizializzazioni();

			analizzare1();
			analizzare2();

			analisiEffettuata = true;
		}

		/// <summary>
		/// Parto dal filesystem e confronto con il db
		/// </summary>
		public void analizzare1() {

			_giornale.Debug( "Inizio analisi1 parto dal FS cerco nel DB. Param = " + paramRebuild );
			contaFotoElaborate = 0;

			DirectoryInfo dirInfoRoot = new DirectoryInfo( Configurazione.UserConfigLumen.cartellaFoto );
			string cosa = "*" + Configurazione.suffissoCartellaGiorni;
			foreach( DirectoryInfo dig in dirInfoRoot.EnumerateDirectories( cosa ) ) {
				
				diGiornataCorr = dig;

				// Controllo il parametro del giorno
				if( paramRebuild != null ) {
					DateTime giornoInEsame = (DateTime)PathUtil.giornoFromPath2( dig.Name );
					if( giornoInEsame != paramRebuild.giorno ) {
						_giornale.Debug( "Salto controllo cartella " + dig + " perché fuori filtro" );
						continue;
					}
				}

				_giornale.Debug( "Considero cartella per giornata " + diGiornataCorr );

				cosa = "*" + Configurazione.suffissoCartellaFoto;
				foreach( DirectoryInfo dif in dig.EnumerateDirectories( cosa ) ) {
				
					diFotografoCorr = dif;

					// Controllo il parametro del fotografo che è facoltativo
					if( paramRebuild != null && paramRebuild.fotografo != null ) {
						string fotografoIdInEsame = PathUtil.fotografoIDFromPath( dif.Name );
						if( fotografoIdInEsame != paramRebuild.fotografo.id ) {
							_giornale.Debug( "Salto controllo cartella " + dif + " perché fuori filtro" );
							continue;
						}
					}

					_giornale.Debug( "Esamino cartella: " + dig + " " + dif );

					controllaFotografoMancante();

					
					foreach( FileInfo fileInfo in dif.EnumerateFiles() ) {

						if( estensioniAmmesse.Contains( fileInfo.Extension.ToLower() ) ) {

							// Ok trovata una foto. La tratto.
							++contaFotoElaborate;
							controllaFotoMancante( fileInfo );
						}
					}
				}
			}

			if( fiFotosMancanti.Count == 0 )
				_giornale.Info( "Analisi1 completata. Tutto ok" );
			else
				_giornale.Warn( "Analisi1 completata ci sono " + fiFotosMancanti.Count + " JPG senza record" );
		}

		private void controllaFotoMancante( FileInfo fInfoFoto ) {

			// Se non c'è già nel database, allora la aggiungo all'elenco
			string nomeRel = PathUtil.nomeRelativoFoto( fInfoFoto );

			if( ! UnitOfWorkScope.currentDbContext.Fotografie.Any( f => f.nomeFile == nomeRel ) ) {
				fiFotosMancanti.Add( fInfoFoto );
				_giornale.Debug( "Attenzione! Foto mancante nel database : " + nomeRel );
			}
		}

		private void controllaFotografoMancante() {

			string idFotografo = Path.GetFileNameWithoutExtension( diFotografoCorr.Name );

			// Ok è già nella lista dei mancanti
			if( idsFotografiMancanti.Contains( idFotografo ) )
				return;

			// Ok è già nel db
			if( UnitOfWorkScope.currentDbContext.Fotografi.SingleOrDefault( f => f.id == idFotografo ) != null )
				return;

			if( !idsFotografiMancanti.Contains( idFotografo ) ) {
				idsFotografiMancanti.Add( idFotografo );
				_giornale.Warn( "Fotografo mancante nel db: " + idFotografo );
			}
		}

		private void creaFotografoMancante( string id ) {
			Fotografo mancante = new Fotografo();
			mancante.id = id;
			mancante.cognomeNome = id;
			mancante.iniziali = Convert.ToString( ++contaFotografiAggiunti );
			mancante.umano = true;
			mancante.attivo = true;
			mancante.note = "Generato automaticamente da DbRebuilder";
			UnitOfWorkScope.currentDbContext.Fotografi.Add( mancante );
		}

		/// <summary>
		/// Parto dal db e confronto con il filesystem
		/// </summary>
		void analizzare2() {

			_giornale.Debug( "Inizio analisi2 parto dal DB e cerco nel FS. Param = " + paramRebuild );

			contaJpegElaborati = 0;

			var q = UnitOfWorkScope.currentDbContext.Fotografie.AsQueryable();
			if( paramRebuild != null )
				q = q.Where( f => f.giornata == paramRebuild.giorno );


			foreach( Fotografia foto in q ) {
				++contaJpegElaborati;
				if( ! File.Exists( PathUtil.nomeCompletoOrig( foto ) ) )
					fotografieSenzaImmagini.Add( foto );
			}

			if( fotografieSenzaImmagini.Count == 0 )
				_giornale.Info( "Analisi2 completata. Tutto ok" );
			else
				_giornale.Warn( "Analisi2 completata. Ci sono " + fotografieSenzaImmagini.Count + " record senza JPG" );
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

		public int contaJpegMancanti {
			get {
				return fotografieSenzaImmagini.Count;
			}
		}
		public int contaJpegElaborati {
			get;
			private set;
		}

		public int contaFotoMancanti {
			get {
				return fiFotosMancanti.Count;
			}
		}

		public int contaFotoElaborate {
			get;
			private set;
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
				return contaFotoMancanti > 0 || contaFotografiMancanti > 0 || contaJpegMancanti > 0;
			}
		}

		public int contaFotografiMancanti {
			get {
				return idsFotografiMancanti.Count;
			}
		}

		public ParamRebuild paramRebuild { get; private set; }

		public void ricostruire() {

			if( !analisiEffettuata )
				throw new InvalidOperationException( "Prima di ricostruire, occorre lanciare l'analisi" );

			foreach( string idFotografoMancante in idsFotografiMancanti ) {
				creaFotografoMancante( idFotografoMancante );
			}
			int test = UnitOfWorkScope.currentDbContext.SaveChanges();

			int ultimoNumFoto = NumeratoreFotogrammi.incrementaNumeratoreFoto( fiFotosMancanti.Count );
			int conta = 0;

			// Creo eventuali fotografie mancanti
			contaFotoAggiunte = 0;
			foreach( FileInfo fiFotoMancante in fiFotosMancanti )
				creaFotografiaMancante( fiFotoMancante, ++conta + ultimoNumFoto );

			// Elimino eventuali fotografie che non hanno più l'immagine jpg su disco
			contaFotoEliminate = 0;
			foreach( Fotografia f in fotografieSenzaImmagini ) {
				UnitOfWorkScope.currentDbContext.Fotografie.Remove( f );
				++contaFotoEliminate;
			}

			// Salvo il contesto
			int quanti = UnitOfWorkScope.currentDbContext.SaveChanges();

			// Piccolo controllo di paranoia
			if( quanti != fotografieSenzaImmagini.Count ) {
				_giornale.Warn( "Dovevo cancellare " + contaJpegMancanti + " fotografie ma in test mi dice: " + quanti );
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
					foto.fotografo = UnitOfWorkScope.currentDbContext.Fotografi.SingleOrDefault( f => f.id == idFotografo );
					foto.numero = numFotogramma;

					// il nome del file, lo memorizzo solamente relativo
					// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
					// Questo perché le stesse foto le devono vedere altri computer della rete che
					// vedono il percorso condiviso in maniera differente.
					foto.nomeFile = PathUtil.nomeRelativoFoto( fiFotoMancante );

					// caricaMetadatiImmagine( foto );

					UnitOfWorkScope.currentDbContext.Fotografie.Add( foto );

					int test = UnitOfWorkScope.currentDbContext.SaveChanges();

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
