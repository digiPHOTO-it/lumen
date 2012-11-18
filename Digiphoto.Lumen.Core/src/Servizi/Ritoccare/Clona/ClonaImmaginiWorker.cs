using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Threading;
using log4net;
using Digiphoto.Lumen.Core.Database;
using System.IO;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Config;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Model;
using System.Transactions;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Imaging;

namespace Digiphoto.Lumen.Servizi.Ritoccare.Clona {

	internal class ClonaImmaginiWorker : WorkerThreadBase
	{
		private static readonly ILog _giornale = LogManager.GetLogger(typeof(ClonaImmaginiWorker));

		internal delegate void ElaboraImmaginiAcquisiteCallback( EsitoClone esitoClone );

		Fotografia[] fotografie;

		ElaboraImmaginiAcquisiteCallback _elaboraImmaginiAcquisiteCallback;

		EsitoClone _esitoClone;

		public ClonaImmaginiWorker( Fotografia[] fotografie, ElaboraImmaginiAcquisiteCallback elaboraCallback ) {
			this.fotografie = fotografie;
			_elaboraImmaginiAcquisiteCallback = elaboraCallback;
		}

		public int conta
		{
			get;
			private set;
		}
		/**
		 * Mi dice se ho completato la copia
		 * torna TRUE anche se la copia è stata stoppata
		 */
		internal bool disposed {
			get {
				return base.Disposed;
			}
		}

		/// <summary>
		/// In alcune situazioni, devo evitare di far partire la copia e la elaborazione in thread
		/// separati.
		/// Per esempio quando lavoro una sola foto per la cornice.
		/// In questo caso, non avvio i worker, ma eseguo il metodo work in modo assestante.
		/// </summary>
		public void StartSingleThread() {
			ThrowIfDisposedOrDisposing();
			Work();
		}

		/**
		 * Processo reale di trasferimento immagini
		 */
		protected override void Work() {

			int conta = 0;
			DateTime oraInizio = DateTime.Now;

			_giornale.Debug( "Inizio a clonare le foto");

			_esitoClone = new EsitoClone();

			ClonaFotoMsg clonaFotoMsg = new ClonaFotoMsg(this, "Inizia Clona Foto");
			clonaFotoMsg.fase = FaseClone.InizioClone;

			clonaFotoMsg.showInStatusBar = true;
			LumenApplication.Instance.bus.Publish(clonaFotoMsg);

			using(new UnitOfWorkScope(true))
			{
				// trasferisco tutti i files elencati
				foreach (Fotografia foto in fotografie)
				{
					if (clonaAsincronoUnFile(foto))
						++conta;
				}
			}
			// Nel log scrivo anche il tempo che ci ho messo a scaricare le foto. Mi servirà per profilare
			TimeSpan tempoImpiegato = DateTime.Now.Subtract( oraInizio );
			_giornale.Info( "Terminato trasferimento di " + conta + " foto. Tempo impiegato = " + tempoImpiegato );
			
			// Deve essere già aperto
			using( new UnitOfWorkScope( true ) ) {
					// ::: Ultima fase eleboro le foto memorizzando nel db e creando le dovute cache
				_elaboraImmaginiAcquisiteCallback.Invoke( _esitoClone );
			}
			
		}

		/**
		 * Se  va tutto bene ritorna true
		 */
		private bool clonaAsincronoUnFile( Fotografia foto) {

			FileInfo fileInfoSrc = new FileInfo( foto.nomeFile );

			//La foto è un clone, recupero il suo originale
			if (isClone(foto))
				fileInfoSrc = getOriginalFileNameFromClone(foto);

			//Directory File

			// "2012-10-29.Gio\\EDOARDO.Fot"
			string subDirFileOrig = Path.GetDirectoryName(foto.nomeFile);
			// "2012-10-29.Gio\\EDOARDO.Fot\\.Thumb"
			string subDirFileProvino = Path.Combine(subDirFileOrig, PathUtil.THUMB);
			// "2012-10-29.Gio\EDOARDO.Fot\.Modif"
			string subDirFileRisult = Path.Combine(subDirFileOrig, PathUtil.MODIF);

			// "e (10).jpg"
			string nomeFileOrig = fileInfoSrc.Name;
			// "e (10)"
			string nomeFileOrigWithoutExtension = Path.GetFileNameWithoutExtension(nomeFileOrig);

			int count = getMaxCount(Path.Combine(Configurazione.cartellaRepositoryFoto, subDirFileOrig), nomeFileOrigWithoutExtension);

			string nomeFileClone = getNomeFileClone(nomeFileOrigWithoutExtension, count, fileInfoSrc);

			// "C:\\Users\\edoardo.colantonio\\Desktop\\RULLINI\\2012-10-29.Gio\\EDOARDO.Fot"
			string cartellaFoto = PathUtil.decidiCartellaFoto(foto);

			// "C:\\Users\\edoardo.colantonio\\Desktop\\RULLINI\\2012-10-29.Gio\\EDOARDO.Fot\\.Thumb"
			string cartellaProvini = PathUtil.decidiCartellaProvini(foto);
			
			// "C:\\Users\\edoardo.colantonio\\Desktop\\RULLINI\\2012-10-29.Gio\\EDOARDO.Fot\\.Modif"
			string cartellaRisultanti =  PathUtil.decidiCartellaRisultanti(foto );

			/*
			// "C:\\Users\\edoardo.colantonio\\Desktop\\RULLINI\\2012-10-29.Gio\\EDOARDO.Fot\\e (10).jpg"
			string srcOrig = PathUtil.nomeCompletoOrig(foto);

			// "C:\\Users\\edoardo.colantonio\\Desktop\\RULLINI\\2012-10-29.Gio\\EDOARDO.Fot\\.Thumb\\e (10).jpg"
			string srcProv = PathUtil.nomeCompletoProvino(foto);
			
			// "C:\\Users\\edoardo.colantonio\\Desktop\\RULLINI\\2012-10-29.Gio\\EDOARDO.Fot\\.Modif\\e (10).jpg"
			string srcRisu = PathUtil.nomeCompletoRisultante(foto);
			*/

			//File Sorgenti

			string srcFileOrig = Path.Combine(Configurazione.cartellaRepositoryFoto, foto.nomeFile);

			string srcFileProvinoOrig = Path.Combine(Configurazione.cartellaRepositoryFoto, subDirFileProvino,nomeFileOrig);

			string srcFileRisultOrig = Path.Combine(Configurazione.cartellaRepositoryFoto, subDirFileRisult,nomeFileOrig);

			//File Destinazione

			string destFileClone = Path.Combine(Configurazione.cartellaRepositoryFoto, subDirFileOrig, nomeFileClone);

			string destFileProvinoClone = Path.Combine(Configurazione.cartellaRepositoryFoto,subDirFileProvino, nomeFileClone);

			string destFileRisultClone = Path.Combine(Configurazione.cartellaRepositoryFoto,subDirFileRisult, nomeFileClone);

			bool sovrascrivi = false;
			bool clonato;

			try {
				// Copio la foto
				File.Copy(srcFileOrig, destFileClone, sovrascrivi);

				// Copio il suo provino
				File.Copy(srcFileProvinoOrig, destFileProvinoClone, sovrascrivi);

				// Copio la sua risultante
				if (File.Exists(srcFileRisultOrig))
					File.Copy(srcFileRisultOrig, destFileRisultClone, sovrascrivi);

				clonato = true;

				++_esitoClone.totFotoClonateOk;
				_esitoClone.fotoDaClonare.Add( new FileInfo( nomeFileClone ) );
				_giornale.Debug("Ok copiato il file : " + foto.nomeFile);

				// rendo il file di sola lettura. Mi serve per protezione
				// TODO un domani potrei anche renderlo HIDDEN
				File.SetAttributes(destFileClone, FileAttributes.Archive | FileAttributes.ReadOnly);

				aggiungiFotoDB(foto, Path.Combine(subDirFileOrig,nomeFileClone));

			} catch( Exception ee ) {
				_esitoClone.riscontratiErrori = true;
				++_esitoClone.totFotoNonClonate;
				clonato = false;
				_giornale.Error("Il file " + foto.nomeFile + " non è stato clonato ", ee);
			}

			return clonato;
		}

		/**
		 * dato il nome del file della immagine, creo l'oggetto Fotografia e lo aggiungo al suo contenitore
		 * (in pratica faccio una insert nel database).
		 */
		private Fotografia aggiungiFotoDB(Fotografia foto, string nomeFileClone)
		{
			// Ad ogni foto persisto.
			// Se per esempio ho 500 foto da salvare, non posso permettermi che se una salta, perdo anche le altre 499 !
			Fotografia fotoClone = null;

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			try
			{
				fotoClone = new Fotografia();
				fotoClone.id = Guid.NewGuid();
				fotoClone.dataOraAcquisizione = foto.dataOraAcquisizione;

				Fotografo f = foto.fotografo;
				OrmUtil.forseAttacca<Fotografo>("Fotografi", ref f);
				fotoClone.fotografo = f;

				if (foto.evento != null)
				{
					Evento e = foto.evento;
					OrmUtil.forseAttacca<Evento>("Eventi", ref e);
					fotoClone.evento = e;
				}
						
				fotoClone.didascalia = foto.didascalia;
				fotoClone.numero = foto.numero;
				fotoClone.correzioniXml = foto.correzioniXml;

				if (foto.imgOrig!=null)
					fotoClone.imgOrig = (IImmagine)foto.imgOrig.Clone();
				if (foto.imgProvino!=null)
					fotoClone.imgProvino = (IImmagine)foto.imgProvino.Clone();
				if (foto.imgRisultante!=null)
					fotoClone.imgRisultante = (IImmagine)foto.imgRisultante.Clone();
						
				fotoClone.faseDelGiorno = foto.faseDelGiorno;
				fotoClone.giornata = foto.giornata;

				// il nome del file, lo memorizzo solamente relativo
				// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
				// Questo perché le stesse foto le devono vedere altri computer della rete che
				// vedono il percorso condiviso in maniera differente.
				fotoClone.nomeFile = nomeFileClone;

				objContext.Fotografie.Add(fotoClone);

				objContext.SaveChanges();
				++conta;

				_giornale.Debug("Clonata nuova foto: " + foto.ToString() + " ora sono " + conta);
			}
			catch (Exception ee)
			{
				_giornale.Error("Non riesco ad inserire una foto clonata. Nel db non c'è ma nel filesystem si: " + fotoClone.nomeFile, ee);
			}

			return foto;
		}

		#region Util;

		/**
		 * Verfico se il file che sto clonando è a sua volta un clone
		 */
		public static bool isClone(Fotografia foto)
		{
			bool clone = false;

			if (!clone && foto.nomeFile.Contains("_CLONE"))
				clone = true;

			return clone;
		}

		public static FileInfo getOriginalFileNameFromClone(Fotografia foto)
		{
			FileInfo fileInfoSrc = new FileInfo(foto.nomeFile);
			return new FileInfo(foto.nomeFile.Remove(foto.nomeFile.IndexOf("_CLONE_"), foto.nomeFile.IndexOf(fileInfoSrc.Extension) - foto.nomeFile.IndexOf("_CLONE_")));
		}

		public static int getMaxCount(Fotografia foto)
		{
			int count = 0;

			// "2012-10-29.Gio\\EDOARDO.Fot"
			string subDirFileOrig = Path.GetDirectoryName(foto.nomeFile);
			// "2012-10-29.Gio\\EDOARDO.Fot\\.Thumb"
			string subDirFileProvino = Path.Combine(subDirFileOrig, PathUtil.THUMB);
			// "2012-10-29.Gio\EDOARDO.Fot\.Modif"
			string subDirFileRisult = Path.Combine(subDirFileOrig, PathUtil.MODIF);

			FileInfo fileInfoSrc = new FileInfo(foto.nomeFile);
			// "e (10).jpg"
			string nomeFileOrig = fileInfoSrc.Name;

			// "e (10)"
			string nomeFileOrigWithoutExtension = Path.GetFileNameWithoutExtension(nomeFileOrig);

			count = getMaxCount(Path.Combine(Configurazione.cartellaRepositoryFoto, subDirFileOrig), nomeFileOrigWithoutExtension);

			return count;
		}

		/**
		 * Mi ricalcolo il nuovo numero da qui partire
		 */
		private static int getMaxCount(string path, string nomeFileOrigWithoutExtension)
		{
			string max = Directory.EnumerateFiles(path, nomeFileOrigWithoutExtension + "_CLONE_[*.*").Max();
			if (max == null)
				return 0;
			string[] num = max.Split(new Char[] { '[', ']' });

			return Int32.Parse(num[1]);
		}

		public static string getNomeFileClone(String nomeFileOrigWithoutExtension, int count, FileInfo fileInfoSrc)
		{
			StringBuilder nomeFileCloneBuilder = new StringBuilder();
			nomeFileCloneBuilder.Append(nomeFileOrigWithoutExtension);
			nomeFileCloneBuilder.Append("_CLONE_[");
			nomeFileCloneBuilder.Append((count + 1).ToString("000"));
			nomeFileCloneBuilder.Append("]");
			nomeFileCloneBuilder.Append(fileInfoSrc.Extension);

			return nomeFileCloneBuilder.ToString();
		}

		/// <summary>
		/// Calcola il nome della foto clone con progressivo
		/// </summary>
		/// <param name="foto"></param>
		/// <returns></returns>
		public static string getNomeFileClone(Fotografia foto)
		{
			FileInfo fileInfoSrc = new FileInfo( foto.nomeFile );

			//La foto è un clone, recupero il suo originale
			if (isClone(foto))
				fileInfoSrc = getOriginalFileNameFromClone(foto);

			string nomeFileOrig = fileInfoSrc.Name;

			string nomeFileOrigWithoutExtension = Path.GetFileNameWithoutExtension(nomeFileOrig);

			int count = getMaxCount(foto);

			return getNomeFileClone(nomeFileOrigWithoutExtension, count, fileInfoSrc);
		}

		#endregion Util

	}

}
