using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Imaging;
using System.Data.Objects;
using System.Data;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Config;
using System.Threading;
using System.IO;
using log4net;
using Digiphoto.Lumen.Servizi.Ritoccare.Clona;
using Digiphoto.Lumen.Database;

namespace Digiphoto.Lumen.Servizi.Ritoccare {

	/// <summary>
	/// Lavora sempre sulla "Fotografia" e non sulla IImmagine.
	/// Gestisce le Correzioni 
	/// </summary>
	public class FotoRitoccoSrvImpl : ServizioImpl, IFotoRitoccoSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( FotoRitoccoSrvImpl ) );

		ClonaImmaginiWorker _clonaImmaginiWorker;

		public FotoRitoccoSrvImpl() {
		}

		~FotoRitoccoSrvImpl()
		{
			// avviso se il thread di copia è ancora attivo
			if (_clonaImmaginiWorker != null && _clonaImmaginiWorker.disposed == false)
			{
				_giornale.Warn( "Il thread di clone fotografie è ancora attivo. Non è stata fatta la Join o la Abort.\nProababilmente il programma si inchioderà" );
			}
		}

		// Aggiungo la correzione ma non scrivo il file su disco
		public void addCorrezione( Fotografia fotografia, Correzione correzione ) {
			addCorrezione( fotografia, correzione, false );
		}

		// ok ho deciso che la correzione viene accettata
		public void addCorrezione( Fotografia fotografia, Correzione correzioneNuova, bool salvare ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			CorrezioniList correzioni;

			// Deserializzo la stringa con le eventuali correzioni attuali
			if( fotografia.correzioniXml == null )
				correzioni = new CorrezioniList();
			else
				correzioni = SerializzaUtil.stringToObject<CorrezioniList>( fotografia.correzioniXml );


			// Alcune correzioni, non devono andare sempre in aggiunta, ma possono sommarsi l'un l'altra.
			// Per esempio la rotazione. Se ruoto 90° poi altri 90, l'effetto finale è quello di avere una sola da 180°
			// TODO : gestire il caso che la somma è INEFFICACE (per esempio +90 e -90 fa 0 che non serve a niente)
			Correzione daSost = null;
			Correzione vecchia = null;
			foreach( Correzione c in correzioni ) {
				if( c.isSommabile( correzioneNuova ) ) {
					vecchia = c;
					daSost = c.somma( correzioneNuova );
					break;
				}
			}

			if( daSost != null ) {
				// Sostituisco la correzione con quella ricalcolata
				correzioni.sostituire( vecchia, daSost );
			} else {
				// Aggiungo in fondo
				correzioni.Add( correzioneNuova );
			}

			// ricalcolo il provino
			IImmagine nuova = gestoreImmaginiSrv.applicaCorrezione( fotografia.imgProvino, correzioneNuova );

			// Ora serializzo di nuovo in stringa tutte le correzioni
			
			fotografia.correzioniXml = SerializzaUtil.objectToString( correzioni );
			fotografia.imgProvino = nuova;

			if( salvare ) {
				// Salvo nel db la modifica
				objContext.SaveChanges();

				gestoreImmaginiSrv.save( fotografia.imgProvino, PathUtil.nomeCompletoProvino( fotografia ) );
			}
		}

		
		public void removeCorrezione( Fotografia foto, Type quale ) {

			// Se non ho correzioni è impossibile che ne voglio rimuovere una
			if( foto.correzioniXml == null )
				return;

			// Deserializzo la stringa con le eventuali correzioni attuali
			CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>( foto.correzioniXml );

			bool rimossa = false;
			foreach( Correzione cor in correzioni ) {
				if( cor.GetType().Equals( quale ) ) {
					correzioni.Remove( cor );
					rimossa = true;
					break;
				}
			}

			if( ! rimossa )
				return;

			// Ora serializzo di nuovo in stringa tutte le correzioni
			if( correzioni.Count > 0 )
				foto.correzioniXml = SerializzaUtil.objectToString( correzioni );
			else
				foto.correzioniXml = null;

			AiutanteFoto.creaProvinoFoto( foto );
		}


		public void applicaCorrezioniTutte( Fotografia fotografia ) {
			throw new NotImplementedException();
		}


		private IGestoreImmagineSrv gestoreImmaginiSrv {
			get {
				return (IGestoreImmagineSrv) LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			}
		}

		/// <summary>
		/// Elimina tutte le Correzioni da una foto e quindi ricrea il provino
		/// </summary>
		public void tornaOriginale( Fotografia fotografia ) {
			tornaOriginale( fotografia, true );
		}

		/// <summary>
		/// Elimina tutte le Correzioni da una foto e quindi ricrea il provino
		/// </summary>
		public void tornaOriginale( Fotografia fotografia, bool salvare ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;

			fotografia.correzioniXml = null;
			
			// Rimuovo anche eventuale file su disco
			string nomeFileRis = PathUtil.nomeCompletoRisultante( fotografia );
			if( File.Exists( nomeFileRis ) )
				File.Delete( nomeFileRis );
			
			AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Provino );
			AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Risultante );

			AiutanteFoto.creaProvinoFoto( fotografia );

			if( salvare )
				objContext.SaveChanges();
		}


		/// <summary>
		/// Rileggo dal db la fotografia. In questo modo, ricopro la proprietà correzioniXml che
		/// era stata modificata dall'utente applicando delle correzioni che poi non ha confermato.
		/// </summary>
		/// <param name="fotografia"></param>
		public void undoCorrezioniTransienti( Fotografia fotografia ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;

			objectContext.Fotografie.Attach( fotografia );
			objContext.ObjectContext.Refresh( RefreshMode.StoreWins, fotografia );

			fotografia.imgProvino = null;  // Questo forza la rilettura del provino da disco
			AiutanteFoto.idrataImmaginiFoto( fotografia, IdrataTarget.Provino );
		}


		public void salvaCorrezioniTransienti( Fotografia fotografia ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			
			objContext.Fotografie.Attach( fotografia );
			objContext.ObjectContext.ObjectStateManager.ChangeObjectState( fotografia, EntityState.Modified );
			objContext.SaveChanges();

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			gis.save( fotografia.imgProvino, PathUtil.nomeCompletoProvino( fotografia ) );
		}


		public void modificaMetadati( Fotografia foto ) {
			// TODO
			throw new NotImplementedException();
		}


		/// <summary>
		/// Lancio GIMP e gli passo l'elenco delle foto indicate
		/// </summary>
		/// <param name="fotografie"></param>
		public Fotografia [] modificaConProgrammaEsterno( Fotografia [] fotografie ) {

			LanciatoreEditor lanciatore = new LanciatoreEditor( fotografie );
			lanciatore.lancia();

			List<Fotografia> modificate = lanciatore.applicaImmaginiModificate();

			foreach( Fotografia foto in modificate ) {

				// Ora idrato l'immagine risultante
				AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Risultante );

				// Per forza di cose, devo ricreare il provino partendo dalla risultante
				IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
				foto.imgProvino = gis.creaProvino( foto.imgRisultante );

				gis.save( foto.imgProvino, PathUtil.nomeCompletoProvino( foto ) );
			}

			return modificate.ToArray();
		}

		public void acquisisciImmagineIncorniciataWithArtista( string nomeFileImg ) {

			// Per fare entrare la nuova foto, uso lo stesso servizio che uso normalmente per scaricare le memory-card
			using( IScaricatoreFotoSrv srv = LumenApplication.Instance.creaServizio<IScaricatoreFotoSrv>() ) {

				srv.start();

				ParamScarica param = new ParamScarica();
				param.nomeFileSingolo = nomeFileImg;
				param.flashCardConfig = new Config.FlashCardConfig {
					idFotografo = Configurazione.ID_FOTOGRAFO_ARTISTA
				};

				srv.scarica( param );

				// Non devo attendere il completamento, perché quando scarico la singola foto, tutto avviene nello stesso thread
			}
		}
	
		public void clonaImmagineIncorniciata(Fotografia fotoOrig, string nomeFileImg)
		{
			FileInfo fileInfoSrc = new FileInfo( fotoOrig.nomeFile );
			string nomeOrig = fileInfoSrc.Name;
			string nomeFotoClone = ClonaImmaginiWorker.getNomeFileClone(fotoOrig);
			string nomeFileDest = Path.Combine(Config.Configurazione.cartellaRepositoryFoto, Path.GetDirectoryName(fotoOrig.nomeFile), nomeFotoClone);
		
			//Sposto la foto nella coartellaRepository e gli attribuisco il suo nome originale.
			File.Move(nomeFileImg, nomeFileDest);

			Fotografia fotoMsk = null;
			using (new UnitOfWorkScope(false))
			{
				LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
				try
				{
					fotoMsk = new Fotografia();
					fotoMsk.id = Guid.NewGuid();
					fotoMsk.dataOraAcquisizione = fotoOrig.dataOraAcquisizione;

					Fotografo f = fotoOrig.fotografo;
					OrmUtil.forseAttacca<Fotografo>("Fotografi", ref f);
					fotoMsk.fotografo = f;

					if (fotoOrig.evento != null)
					{
						Evento e = fotoOrig.evento;
						OrmUtil.forseAttacca<Evento>("Eventi", ref e);
						fotoMsk.evento = e;
					}

					fotoMsk.didascalia = fotoOrig.didascalia;
					fotoMsk.numero = fotoOrig.numero;
					fotoMsk.correzioniXml = fotoOrig.correzioniXml;

					fotoMsk.faseDelGiorno = fotoOrig.faseDelGiorno;
					fotoMsk.giornata = fotoOrig.giornata;

					// il nome del file, lo memorizzo solamente relativo
					// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
					// Questo perché le stesse foto le devono vedere altri computer della rete che
					// vedono il percorso condiviso in maniera differente.
					fotoMsk.nomeFile = Path.Combine(Path.GetDirectoryName(fotoOrig.nomeFile), nomeFotoClone);

					objContext.Fotografie.Add(fotoMsk);

					objContext.SaveChanges();
				}

				catch (Exception ee)
				{
					_giornale.Error("Non riesco ad inserire una foto clonata. Nel db non c'è ma nel filesystem si: " + fotoOrig.nomeFile, ee);
				}

				AiutanteFoto.creaProvinoFoto(nomeFileDest, fotoMsk);

				// Libero la memoria occupata dalle immagini, altrimenti esplode.
				AiutanteFoto.disposeImmagini(fotoMsk, IdrataTarget.Tutte);
			}
		}

		public void clonaFotografie(Fotografia[] fotografie)
		{
			_clonaImmaginiWorker = new ClonaImmaginiWorker(fotografie, fineClone);

			if (fotografie.Count()==1)
				_clonaImmaginiWorker.Start();
			else
				_clonaImmaginiWorker.StartSingleThread();
		}

		private void fineClone(EsitoClone esitoScarico)
		{

			ClonaFotoMsg clonaFotoMsg = new ClonaFotoMsg(this, "Completato Clone di " + esitoScarico.totFotoClonateOk + " Foto");
			clonaFotoMsg.fase = FaseClone.FineClone;

			clonaFotoMsg.showInStatusBar = true;
			pubblicaMessaggio(clonaFotoMsg);
		
		}

		public string [] caricaMiniatureMaschere() {

			List<string> nomiFileMiniature = new List<string>();

			string dirMaschere = Configurazione.UserConfigLumen.cartellaMaschere;
			if( ! Directory.Exists(dirMaschere) )
				return null;

			// preparo la cartella per le miniature
			string dirMiniature = Path.Combine( dirMaschere, PathUtil.THUMB );
			if( ! Directory.Exists(dirMiniature) ) 
				Directory.CreateDirectory( dirMiniature );


			// Faccio giri diversi per i vari formati grafici che sono indicati nella configurazione (jpg, tif)
			foreach( string estensione in Configurazione.estensioniGraficheAmmesse ) {

				// Questa è la lista dei files di dimensioni grandi.
				string [] nomiFilesMaschere = Directory.GetFiles( Configurazione.UserConfigLumen.cartellaMaschere, searchPattern: "*" + estensione, searchOption: SearchOption.TopDirectoryOnly );

				// Adesso controllo che per ogni file grande, esista la sua miniatura.
				foreach( string nomeFileMaschera in nomiFilesMaschere ) {

					FileInfo fi = new FileInfo( nomeFileMaschera );
					string nomeFileMiniatura = Path.Combine( dirMiniature, fi.Name );

					if( creaMiniaturaMaschera( nomeFileMaschera, nomeFileMiniatura ) )
						nomiFileMiniature.Add( nomeFileMiniatura );
				}
			}

			return nomiFileMiniature.ToArray();
		}


		private bool creaMiniaturaMaschera( string nomeFileMaschera, string nomeFileMiniatura ) {

			bool esiste = false;

			if( !File.Exists( nomeFileMiniatura ) ) {

				try {
					IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
					IImmagine immagineMaschera = gis.load( nomeFileMaschera );
					IImmagine immagineMiniatura = gis.creaProvino( immagineMaschera, 80 );   // creo una immagine più piccola
					gis.save( immagineMiniatura, nomeFileMiniatura );
					esiste = true;
				} catch( Exception ee ) {
					_giornale.Error( "Non riesco a creare la miniatura della maschera : " + nomeFileMaschera, ee );
				}

			} else
				esiste = true;

			return esiste;
		}
	}
}
