﻿using System;
using System.Collections.Generic;
using System.Linq;
using Digiphoto.Lumen.Servizi;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Core.Database;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Servizi.Scaricatore;
using Digiphoto.Lumen.Config;
using System.Threading;
using System.IO;
using log4net;
using Digiphoto.Lumen.Servizi.Ritoccare.Clona;
using Digiphoto.Lumen.Database;
using Digiphoto.Lumen.Servizi.Ritoccare;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Digiphoto.Lumen.Windows.Media.Effects;
using System.Windows.Media.Effects;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Servizi.EntityRepository;
using Digiphoto.Lumen.Imaging.Wic.Correzioni;
using System.ComponentModel;

namespace Digiphoto.Lumen.Imaging.Wic {

	/// <summary>
	/// Lavora sempre sulla "Fotografia" e non sulla IImmagine.
	/// Gestisce le Correzioni 
	/// </summary>
	public class FotoRitoccoSrvImpl : ServizioImpl, IFotoRitoccoSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof( FotoRitoccoSrvImpl ) );

		private ClonaImmaginiWorker _clonaImmaginiWorker;

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

		public void addCorrezione( Fotografia fotografia, Correzione correzione ) {
			addCorrezione( fotografia, correzione, false );
		}

		/// <summary>
		/// Aggiungo una correzione a quelle già eventualmente presenti sulla foto.
		/// Se la correzione esiste già, gestisco eventuale somma, oppure rimozione in caso che sia inutile (ininfluente)
		/// </summary>
		/// <param name="fotografia">la foto</param>
		/// <param name="correzione">la correzione da aggiungere</param>
		public void addCorrezione( Fotografia fotografia, Correzione correzioneNuova, bool salvare ) {

			CorrezioniList correzioni;

			// Deserializzo la stringa con le eventuali correzioni attuali
			if( fotografia.correzioniXml == null )
				correzioni = new CorrezioniList();
			else
				correzioni = SerializzaUtil.stringToObject<CorrezioniList>( fotografia.correzioniXml );


			// Alcune correzioni, non devono andare sempre in aggiunta, ma possono sommarsi l'un l'altra.
			// Per esempio la rotazione. Se ruoto 90° poi altri 90, l'effetto finale è quello di avere una sola da 180°
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
				if( daSost.isInutile )
					correzioni.Remove( vecchia );
				else
					correzioni.sostituire( vecchia, daSost );
			} else {
				// Aggiungo in fondo (se la correzione è inutile, allora non aggiungo nulla)
				if( ! correzioneNuova.isInutile )
					correzioni.Add( correzioneNuova );
			}

			// Ora serializzo di nuovo in stringa tutte le correzioni
			fotografia.correzioniXml = SerializzaUtil.objectToString( correzioni );

			if( salvare ) {
				
				fotografieRepositorySrv.saveChanges();  // Persisto nel db le modifiche

				AiutanteFoto.creaProvinoFoto( fotografia );

				// Devo informare tutti che questa foto è cambiata
				FotoModificateMsg msg = new FotoModificateMsg( this, fotografia );
				pubblicaMessaggio( msg );
			}
		}

		/// <summary>
		/// Aggiungo una correzione ad una lista di correzione.
		/// Se la correzione esiste già, gestisco eventuale somma, oppure rimozione in caso che sia inutile (ininfluente)
		/// </summary>
		/// <param name="correzioni"></param>
		/// <param name="correzioneNuova"></param>
		public void addCorrezione(ref CorrezioniList correzioni, Correzione correzioneNuova)
		{

			// Alcune correzioni, non devono andare sempre in aggiunta, ma possono sommarsi l'un l'altra.
			// Per esempio la rotazione. Se ruoto 90° poi altri 90, l'effetto finale è quello di avere una sola da 180°
			Correzione daSost = null;
			Correzione vecchia = null;
			foreach (Correzione c in correzioni)
			{
				if (c.isSommabile(correzioneNuova))
				{
					vecchia = c;
					daSost = c.somma(correzioneNuova);
					break;
				}
			}

			if (daSost != null)
			{
				// Sostituisco la correzione con quella ricalcolata
				if (daSost.isInutile)
					correzioni.Remove(vecchia);
				else
					correzioni.sostituire(vecchia, daSost);
			}
			else
			{
				// Aggiungo in fondo (se la correzione è inutile, allora non aggiungo nulla)
				if (!correzioneNuova.isInutile)
					correzioni.Add(correzioneNuova);
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

		private IGestoreImmagineSrv gestoreImmaginiSrv {
			get {
				return (IGestoreImmagineSrv) LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			}
		}
		
		private IEntityRepositorySrv<Fotografia> fotografieRepositorySrv {
			get {
				return (IEntityRepositorySrv<Fotografia>)LumenApplication.Instance.getServizioAvviato<IEntityRepositorySrv<Fotografia>>();
			}
		}


		/// <summary>
		/// Quando lavoro su di una collezione, devo essere ottimizzato per visualizzare la progressione del lavoro
		/// </summary>
		/// <param name="fotografie"></param>
		public void tornaOriginale( IEnumerable<Fotografia> fotografie ) {
			tornaOriginale( fotografie, true );
		}

		public void tornaOriginale( IEnumerable<Fotografia> fotografie, bool salvare ) {

			foreach( Fotografia fotografia in fotografie ) {
				
				fotografia.correzioniXml = null;

				// Rimuovo anche eventuale file su disco
				string nomeFileRis = PathUtil.nomeCompletoRisultante( fotografia );
				if( File.Exists( nomeFileRis ) )
					File.Delete( nomeFileRis );

				// Rilascio memoria
				AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Tutte );

				if( salvare ) {
					Fotografia f = fotografia;
					fotografieRepositorySrv.update( ref f, true );
					fotografieRepositorySrv.saveChanges();  // Persisto nel db le modifiche
				}
			}

			// Rifaccio tutti i provini in background
			provinare( fotografie );
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

			fotografia.correzioniXml = null;
			
			// Rimuovo anche eventuale file su disco
			string nomeFileRis = PathUtil.nomeCompletoRisultante( fotografia );
			if( File.Exists( nomeFileRis ) )
				File.Delete( nomeFileRis );

			// Rilascio memoria
			AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Tutte );

			AiutanteFoto.creaProvinoFoto( fotografia );
			
			// Le due foto grandi le rilascio per non intasare la memoria qualora questo metodo è chiamato più volte
			AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Originale );
			AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Risultante );

			if( salvare ) {
				Fotografia f = fotografia;
				fotografieRepositorySrv.update( ref f, true );
				fotografieRepositorySrv.saveChanges();  // Persisto nel db le modifiche
				
				// Devo informate tutti che questa foto è cambiata
				FotoModificateMsg msg = new FotoModificateMsg( this, fotografia );
				pubblicaMessaggio( msg );
			}
		}


		/// <summary>
		/// Rileggo dal db la fotografia. In questo modo, ricopro la proprietà correzioniXml che
		/// era stata modificata dall'utente applicando delle correzioni che poi non ha confermato.
		/// </summary>
		/// <param name="fotografia"></param>
		public void undoCorrezioniTransienti( Fotografia fotografia ) {

			fotografieRepositorySrv.refresh( fotografia );

			fotografia.imgProvino = null;  // Questo forza la rilettura del provino da disco
			AiutanteFoto.idrataImmaginiFoto( fotografia, IdrataTarget.Provino );
		}


		public Correttore getCorrettore( object oo ) {
			if( oo is ShaderEffect )
				return gestoreImmaginiSrv.getCorrettore( EffectsUtil.tipoCorrezioneCorrispondente( oo as ShaderEffect ) );
			else
				return null;
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
			Gimp correzioneGimp = new Gimp();

			foreach( Fotografia foto in modificate ) {

				// Ora idrato l'immagine risultante
				AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Risultante );

				addCorrezione( foto, correzioneGimp, false );
			}

			return modificate.ToArray();
		}

		public Fotografia modificaConProgrammaEsterno( Fotografia fotografia ) {
			Fotografia [] ffin = { fotografia };
			Fotografia[] ffout = modificaConProgrammaEsterno( ffin );
			return ffout[0];
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
				try
				{
					fotoMsk = new Fotografia();
					fotoMsk.id = Guid.NewGuid();
					fotoMsk.dataOraAcquisizione = fotoOrig.dataOraAcquisizione;

					Fotografo f = fotoOrig.fotografo;
					OrmUtil.forseAttacca<Fotografo>( ref f);
					fotoMsk.fotografo = f;

					if (fotoOrig.evento != null)
					{
						Evento e = fotoOrig.evento;
						OrmUtil.forseAttacca<Evento>( ref e);
						fotoMsk.evento = e;
					}

					fotoMsk.didascalia = fotoOrig.didascalia;
					fotoMsk.numero = fotoOrig.numero;
					// Le correzioni non devo duplicarle perché non sono tipiche della composizione finale, ma della foto originale.

					fotoMsk.faseDelGiorno = fotoOrig.faseDelGiorno;
					fotoMsk.giornata = fotoOrig.giornata;

					// il nome del file, lo memorizzo solamente relativo
					// scarto la parte iniziale di tutto il path togliendo il nome della cartella di base delle foto.
					// Questo perché le stesse foto le devono vedere altri computer della rete che
					// vedono il percorso condiviso in maniera differente.
					fotoMsk.nomeFile = Path.Combine(Path.GetDirectoryName(fotoOrig.nomeFile), nomeFotoClone);

					fotografieRepositorySrv.addNew( fotoMsk );
					fotografieRepositorySrv.saveChanges();
				}

				catch (Exception ee)
				{
					_giornale.Error("Non riesco ad inserire una foto clonata. Nel db non c'è ma nel filesystem si: " + fotoOrig.nomeFile, ee);
				}

				AiutanteFoto.creaProvinoFoto(nomeFileDest, fotoMsk);

				// Libero la memoria occupata dalle immagini, altrimenti esplode.
				AiutanteFoto.disposeImmagini(fotoMsk, IdrataTarget.Tutte);

				// Notifico la lista delle foto da mandare in modifica
				NuovaFotoMsg msg = new NuovaFotoMsg(this, fotoMsk);
//				msg.descrizione += Configurazione.ID_FOTOGRAFO_ARTISTA;
				LumenApplication.Instance.bus.Publish(msg);
			}
		}

		public void clonaFotografie(Fotografia[] fotografie)
		{
			_clonaImmaginiWorker = new ClonaImmaginiWorker(fotografie, fineClone);

			if (fotografie.Count()==1)
				_clonaImmaginiWorker.StartSingleThread();
			else {
				// _clonaImmaginiWorker.Start();
				throw new NotSupportedException( "clonare in multithread causa problemi con la Unit-Of-Work" );
			}
		}

		private void fineClone(EsitoClone esitoScarico)
		{

			ClonaFotoMsg clonaFotoMsg = new ClonaFotoMsg(this, "Completato Clone di " + esitoScarico.totFotoClonateOk + " Foto");
			clonaFotoMsg.fase = FaseClone.FineClone;

			clonaFotoMsg.showInStatusBar = true;
			pubblicaMessaggio(clonaFotoMsg);
		
		}

		public void salvaOrdinamentoMaschere( FiltroMask filtro, List<string> nuovaLista ) {

			string dirMaschere = getCartellaMaschera( filtro );
			if( !Directory.Exists( dirMaschere ) )
				return;

			string nomeFileOrdinamento = Path.Combine( dirMaschere, "ordinamento.txt" );

			File.WriteAllLines( nomeFileOrdinamento, nuovaLista );
		}

		/// <summary>
		/// Ricavo la lista di tutte le maschere (senza idratare le immagini, solo i nomi)
		/// </summary>
		/// <param name="filtro"></param>
		/// <returns></returns>
		public List<Digiphoto.Lumen.Model.Maschera> caricaListaMaschere( FiltroMask filtro ) {

			string dirMaschere = getCartellaMaschera( filtro );
			if( !Directory.Exists( dirMaschere ) )
				return null;

			List<Digiphoto.Lumen.Model.Maschera> maschere = new List<Digiphoto.Lumen.Model.Maschera>();

			// preparo la cartella per le miniature
			string dirMiniature = Path.Combine( dirMaschere, PathUtil.THUMB );
			if( !Directory.Exists( dirMiniature ) )
				Directory.CreateDirectory( dirMiniature );

			// Faccio giri diversi per i vari formati grafici che sono indicati nella configurazione (jpg, tif)
			foreach( string estensione in Configurazione.estensioniGraficheAmmesse ) {

				// Questa è la lista dei files di dimensioni grandi.
				string [] nomiFilesMaschere = Directory.GetFiles( dirMaschere, searchPattern: "*" + estensione, searchOption: SearchOption.TopDirectoryOnly );

				// Adesso controllo che per ogni file grande, esista la sua miniatura.
				foreach( string nomeFileMaschera in nomiFilesMaschere ) {

					FileInfo fi = new FileInfo( nomeFileMaschera );

					string nomeFileMiniatura = Path.Combine( dirMiniature, fi.Name );

					if( creaMiniaturaMaschera( nomeFileMaschera, nomeFileMiniatura ) ) {

						Digiphoto.Lumen.Model.Maschera msk = new Model.Maschera();

						// Metto solo il nome del file senza path
						msk.nomeFile = fi.Name;
						msk.tipo = filtro;

						maschere.Add( msk );

					}
				}
			}

			// Prima di ritornare la lisa, controllo se nella cartella è presente un file di testo
			// con eventuale ordinamento
			string nomeFileOrdinamento = Path.Combine( dirMaschere, "ordinamento.txt" );
			List<Model.Maschera> listaOrdinata;
			if( File.Exists( nomeFileOrdinamento ) ) {

				List<string> mioOrdinam = new List<string>( File.ReadAllLines( nomeFileOrdinamento ) );
				listaOrdinata = maschere.OrderBy( d => mioOrdinam.IndexOf( d.nomeFile ) < 0 ? 9999 : mioOrdinam.IndexOf( d.nomeFile ) ).ToList();

			} else {

				// Ordino per nome
				listaOrdinata = maschere.OrderBy( d => d.nomeFile ).ToList();

			}

			return listaOrdinata;
		}

#if false
		public string [] caricaMiniatureMaschere( FiltroMask filtro ) {

			List<string> nomiFileMiniature = new List<string>();

			string dirMaschere = getCartellaMaschera( filtro );
			if( ! Directory.Exists(dirMaschere) )
				return null;

			// preparo la cartella per le miniature
			string dirMiniature = Path.Combine( dirMaschere, PathUtil.THUMB );
			if( ! Directory.Exists(dirMiniature) ) 
				Directory.CreateDirectory( dirMiniature );


			// Faccio giri diversi per i vari formati grafici che sono indicati nella configurazione (jpg, tif)
			foreach( string estensione in Configurazione.estensioniGraficheAmmesse ) {

				// Questa è la lista dei files di dimensioni grandi.
				string[] nomiFilesMaschere = Directory.GetFiles( dirMaschere, searchPattern: "*" + estensione, searchOption: SearchOption.TopDirectoryOnly );
				
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
#endif

		public string getCartellaMaschera( FiltroMask filtro ) {
			return PathUtil.getCartellaMaschera( filtro );
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

		public IList<T> converteCorrezioni<T>( Fotografia fotografia ) {

			if( fotografia.correzioniXml == null )
				return null;

			CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>( fotografia.correzioniXml );
			return converteCorrezioni<T>( correzioni );
		}

		/// <summary>
		/// Prendo le correzioni xml di una foto, 
		/// e le converto nei rispettivi "Effetti" e/o "Trasformazioni"
		/// </summary>
		/// <typeparam name="T">Può essere un ShaderEffect oppure un Transform</typeparam>
		/// <param name="fotografia"></param>
		/// <returns>
		/// Se ho delle correzioni, allora ritorno La lista degli oggetti trasformati. <br/>
		/// Se invece non ho correzioni, allora ritorno null.
		/// </returns>
		public IList<T> converteCorrezioni<T>( CorrezioniList correzioni ) {

			if( correzioni == null )
				return null;

			// ok ho qualcosa da fare. Istanzio la lista da ritornare
			IList<T> convertiti = new List<T>();

			foreach( Correzione correzione in correzioni ) {

				Correttore correttore = gestoreImmaginiSrv.getCorrettore( correzione );
				if( correttore.CanConvertTo( typeof( T ) ) )
					convertiti.Add( (T)correttore.ConvertTo( correzione, typeof( T ) ) );
				else {
					// non do errore, perché è la prassi che venga chiamato questo metodo per estrarre soltanto i tipi gestiti
					// _giornale.Error( "Impossibile convertire " + typeof( T ) + " in una correzione" );
				}
					
			}

			return convertiti;
		}


		public Correzione converteInCorrezione( TipoCorrezione tipoDest, Object trasformazione ) {

			Correzione correzione = null;

			if( trasformazione != null ) {

				Correttore correttore = gestoreImmaginiSrv.getCorrettore( tipoDest );
				if( correttore != null ) {

					if( correttore.CanConvertFrom( trasformazione.GetType() ) )
						correzione = (Correzione)correttore.ConvertFrom( trasformazione );
				}
			}

			return correzione;
		}



		public CorrezioniList converteInCorrezioni( IEnumerable<Object> effetti ) {

			if( effetti == null )
				return null;

			CorrezioniList correzioni = new CorrezioniList();

			foreach( var effettoTrasformazione in effetti ) {

				Correttore correttore = this.getCorrettore( effettoTrasformazione );
				if( correttore == null )
					continue;

				if( correttore.CanConvertFrom( effettoTrasformazione.GetType() ) )
					correzioni.Add( (Correzione)correttore.ConvertFrom( effettoTrasformazione ) );
			}

			return correzioni;
		}

		
		/// <summary>
		/// Prendo la foto pulita e applico tutte le correzioni.
		/// Serve per il fotoritocco.
		/// Faccio tutto in un unico colpo per essere più efficiente.
		/// </summary>
		/// <param name="fotografia"></param>
		/// <param name="cosaRicalcolo"></param>

		public IImmagine applicaCorrezioni( IImmagine partenza, CorrezioniList correzioni, IdrataTarget cosaRicalcolo ) {

			IImmagine modificata = partenza;

			// Ci sono alcune correzioni che sono più "complicate" e non posso trattarle in modo singolo,
			// ma devo gestire nell'insieme in modo efficiente.
			// In questo caso devo cambiare strategia.
			bool complicato = false;
			if( correzioni.SingleOrDefault( c => c is Imaging.Correzioni.Mascheratura ) != null ||
			    correzioni.SingleOrDefault( c => c is Imaging.Correzioni.MascheraturaOrientabile ) != null ||
				correzioni.SingleOrDefault( c => c is Ruota && ((Ruota)c).isAngoloRetto == false ) != null ||
				correzioni.SingleOrDefault( c => c is Trasla ) != null ||
				correzioni.SingleOrDefault( c => c is Zoom ) != null )
				complicato = true;

			if( complicato ) {
				modificata = rigeneraImmagineConCorrezioniComplicate( partenza, correzioni, cosaRicalcolo );

			} else {
				// le correzioni sono tutte applicabili singolarmente. Non necessito di ricalcolo
				foreach( Correzione correzione in correzioni )
					modificata = applicaCorrezione( modificata, correzione );
			}

			return modificata;
		}

		public IImmagine applicaCorrezione( IImmagine immaginePartenza, Correzione correzione ) {
			Correttore correttore = gestoreImmaginiSrv.getCorrettore( correzione );
			_giornale.Debug( "applico correzione: " + correttore );
			return correttore.applica( immaginePartenza, correzione );
		}


		/// <summary>
		///  Devo gestire le correzioni complicate
		/// </summary>
		/// <param name="immaginePartenza"></param>
		/// <param name="correzioni"></param>
		/// <returns></returns>
		private IImmagine rigeneraImmagineConCorrezioniComplicate( IImmagine immaginePartenza, CorrezioniList correzioni, IdrataTarget qualeTarget ) {

			double wwDest = 0, hhDest = 0;

			BitmapSource bmpFoto = null;		// Questa è la foto
			BitmapSource bmpMaschera = null;	// Questa è la maschera (eventuale)


			bmpFoto = ((ImmagineWic)immaginePartenza).bitmapSource;

			// ::: Per prima cosa calcolo la dimensione che deve avere l'immagine di uscita (il canvas)
			//     Verifico quindi se c'è una maschera. In tal caso comanda lei
			Imaging.Correzioni.Mascheratura mascheratura = (Imaging.Correzioni.Mascheratura)correzioni.FirstOrDefault( c => c is Imaging.Correzioni.Mascheratura );

			// La mascheratura potrebbe essere anche indicata come orientabile
			if( mascheratura == null ) {
				Imaging.Correzioni.MascheraturaOrientabile mascheraturaOrientabile = (Imaging.Correzioni.MascheraturaOrientabile)correzioni.FirstOrDefault( c => c is Imaging.Correzioni.MascheraturaOrientabile );
				if( mascheraturaOrientabile != null ) {
					// Ora estraggo la mascheratura giusta per questa foto
					if( immaginePartenza.orientamento == Orientamento.Verticale )
						mascheratura = mascheraturaOrientabile.mascheraturaV;
					if( immaginePartenza.orientamento == Orientamento.Orizzontale )
						mascheratura = mascheraturaOrientabile.mascheraturaH;
				}
			}

			//
			// Ora che ho scoperto se esiste una mascheratura, la applico
			//

			if( mascheratura != null ) {

				ImmagineWic immagineMaschera = new ImmagineWic( Path.Combine( getCartellaMaschera(FiltroMask.MskSingole), mascheratura.nome ) );
				bmpMaschera = immagineMaschera.bitmapSource;
				// Carico la maschera per avere le dimensioni reali definitive
				if( qualeTarget == IdrataTarget.Provino ) {
					IImmagine imgMascheraPiccola = gestoreImmaginiSrv.creaProvino( immagineMaschera );
					bmpMaschera = ((ImmagineWic)imgMascheraPiccola).bitmapSource;
				}
				wwDest = bmpMaschera.PixelWidth;
				hhDest = bmpMaschera.PixelHeight;
			} else {

				// Cerco di intercettare un eventuale rotazione del quadro (non della foto)
				Zoom zzz = (Zoom) correzioni.FirstOrDefault( c => c is Zoom );
				bool rovesciare = zzz != null ? zzz.quadroRuotato : false;

				if( rovesciare ) {
					wwDest = bmpFoto.PixelHeight;
					hhDest = bmpFoto.PixelWidth;
				} else {
					wwDest = bmpFoto.PixelWidth;
					hhDest = bmpFoto.PixelHeight;
				}
			}


			// ::: Il canvas
			Canvas canvas = new Canvas();
			canvas.Background = new SolidColorBrush( Colors.White );
			canvas.Width = wwDest;
			canvas.Height = hhDest;
			canvas.HorizontalAlignment = HorizontalAlignment.Left;
			canvas.VerticalAlignment = VerticalAlignment.Top;



			#region Correzioni
			// ::: Gestisco le correzioni
			TransformGroup traGroup = new TransformGroup();
			IList<ShaderEffect> effetti = null;
//			bool quadroRuotato = false;

			foreach( Correzione correzione in correzioni ) {
				
				Correttore correttore = gestoreImmaginiSrv.getCorrettore( correzione );

				if( correttore.CanConvertTo( typeof( Transform ) ) ) {
					// ::: Trasformazioni
					Transform trasformazione = (Transform)correttore.ConvertTo( correzione, typeof( Transform ) );

					// La trasformazione di spostamento, (Trasla) fa una eccezione perché dipende dalla grandezza del target.
					// Devo sistemarla al volo
					if( trasformazione is TranslateTransform ) {
						TranslateTransform tt = (TranslateTransform)trasformazione;
						// TODO riproporzionare
						TranslateTransform tt2 = new TranslateTransform();
						// Devo riproporzionare X e Y alla dimensione giusta finale.
						//   posx:300=x:finalW       ->    x = posx * finalW / 300
						tt2.X = ((TranslateTransform)tt).X * canvas.Width  / ((Trasla)correzione).rifW;
						tt2.Y = ((TranslateTransform)tt).Y * canvas.Height / ((Trasla)correzione).rifH;
						traGroup.Children.Add( tt2 );
					} else
						traGroup.Children.Add( trasformazione );

				} else if( correttore.CanConvertTo( typeof( ShaderEffectBase ) ) ) {

					// ::: Effetti li sommo poi li faccio tutti in una volta per essere più veloce
					if( effetti == null )
						effetti = new List<ShaderEffect>();
					effetti.Add( (ShaderEffect)correttore.ConvertTo( correzione, typeof( ShaderEffectBase ) ) );

				}
			}
			#endregion Correzioni


			if( effetti != null && effetti.Count > 0 )
				bmpFoto = EffectsUtil.RenderImageWithEffectsToBitmap( bmpFoto, effetti );




			// ::: La foto

			Image fotona = new Image();
			fotona.BeginInit();
			fotona.Source = bmpFoto; // bmpFoto;
			fotona.Stretch = Stretch.Uniform;
			fotona.HorizontalAlignment = HorizontalAlignment.Center;
			fotona.VerticalAlignment = VerticalAlignment.Center;
			fotona.Width = wwDest;
			fotona.Height = hhDest;
			fotona.EndInit();

			// Assegno tutte le trasformazioni
			if( traGroup != null && traGroup.Children.Count > 0 ) {
				fotona.RenderTransform = traGroup;
				fotona.RenderTransformOrigin = new Point( 0.5, 0.5 );  // centrate
			}

			canvas.Children.Add( fotona );

			// ::: La Maschera - per concludere, aggiungo anche la maschera che deve ricoprire tutto.
			Image imageMaschera;
			if( bmpMaschera != null ) {
				imageMaschera = new Image();
				imageMaschera.BeginInit();
				imageMaschera.Stretch = Stretch.Uniform;
				imageMaschera.HorizontalAlignment = HorizontalAlignment.Left;
				imageMaschera.VerticalAlignment = VerticalAlignment.Top;
				imageMaschera.Source = bmpMaschera;
				imageMaschera.Width = wwDest;
				imageMaschera.Height = hhDest;
				imageMaschera.EndInit();
				canvas.Children.Add( imageMaschera );
			}

			// Devo "Arrangiare" il canvas altrimenti non ha dimensione (e la foto viene nera)
			var size = new Size( wwDest, hhDest );
			canvas.Measure( size );
			canvas.Arrange( new Rect( size ) );

			IImmagine immagineMod = null;


			// Creo la bitmap di ritorno
			RenderTargetBitmap rtb = new RenderTargetBitmap( (int)canvas.Width, (int)canvas.Height, 96d, 96d, PixelFormats.Pbgra32 );
			rtb.Render( canvas );
			if( rtb.CanFreeze )
				rtb.Freeze();

			immagineMod = new ImmagineWic( rtb );


			// Come penultima cosa, mi rimane da gestire le Scritte (in realtà una sola)
			foreach( var scritta in correzioni.OfType<Scritta>() ) {
				Correttore correttore = gestoreImmaginiSrv.getCorrettore( scritta );
				immagineMod = correttore.applica( immagineMod, scritta );
			}

			// Per ultima cosa, mi rimane fuori un evenuale logo ...
			Logo correzioneLogo = (Logo) correzioni.FirstOrDefault( c => c is Logo );
			if( correzioneLogo != null ) {
				Correttore correttore = gestoreImmaginiSrv.getCorrettore( correzioneLogo );
				immagineMod = correttore.applica( immagineMod, correzioneLogo );
			}

			// Questa forzatura anche se filosoficamente non è bella, ma mi serve per essere più veloce a creare le correzioni complesse sulla foto da stampare.
			// .. oppure una eventuale area di rispetto (solo sul provino)
			if( qualeTarget == IdrataTarget.Provino ) {
				AreaRispetto correzioneAreaRispetto = (AreaRispetto)correzioni.FirstOrDefault( c => c is AreaRispetto );
				if( correzioneAreaRispetto != null ) {
					Correttore correttore = gestoreImmaginiSrv.getCorrettore( correzioneAreaRispetto );
					immagineMod = correttore.applica( immagineMod, correzioneAreaRispetto );
				}
			}

			return immagineMod;

		}


		private BitmapImage caricaBitmapMaschera2( String nomeFile ) {

			BitmapImage bmpMaschera = null;

			string nomeMaschera = Path.Combine( Configurazione.UserConfigLumen.cartellaMaschere, nomeFile );
			if( File.Exists( nomeMaschera ) ) {
				Uri uriMaschera = new Uri( nomeMaschera );
				bmpMaschera = new BitmapImage( uriMaschera );

			}
			return bmpMaschera;
		}


		private BitmapSource caricaBitmapMaschera( String nomeFile ) {

			BitmapSource bmpMaschera = null;

			string nomeMaschera = Path.Combine( Configurazione.UserConfigLumen.cartellaMaschere, nomeFile );
			if( File.Exists( nomeMaschera ) ) {
				MemoryStream data = new MemoryStream( File.ReadAllBytes( nomeMaschera ) );
				bmpMaschera = BitmapFrame.Create( data );
				bmpMaschera.Freeze();
			}
			return bmpMaschera;
		}




		/// <summary>
		/// Faccio qualche controllo preventivo.
		/// </summary>
		/// <param name="fotografia"></param>
		/// <param name="ruota"></param>
		public void autoRuotaSuOriginale( Fotografia fotografia, Ruota ruota ) {

			if( fotografia.correzioniXml != null )
				throw new LumenException( "Sulla foto " + fotografia.numero + " esistono correzioni.\nImpossibile ruotare sull'originale.\nRimuovere prima le correzioni (torna originale)" );

			if( ! ruota.isAngoloRetto )
				throw new ArgumentException( "La rotazione sull'originale funziona solo con angoli retti" );

			string nomeFileOrig = PathUtil.nomeCompletoOrig( fotografia );
			string estensione = Path.GetExtension( nomeFileOrig );

			if( fotografia.imgOrig == null )
				AiutanteFoto.idrataImmaginiFoto( fotografia, IdrataTarget.Originale );

			IImmagine imgRuotata = applicaCorrezione( fotografia.imgOrig, ruota );

			string nomeFileBackup = Path.ChangeExtension( nomeFileOrig, "BACKUP" + estensione );
			if( !File.Exists( nomeFileBackup ) ) {
				// Salvo per sicurezza il file originale
				File.Move( nomeFileOrig, nomeFileBackup );
			}

			fotografia.imgOrig = imgRuotata;
			gestoreImmaginiSrv.save( imgRuotata, nomeFileOrig );

			AiutanteFoto.creaProvinoFoto( fotografia );

			// Libero memoria. Lascio solo il provino
			AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Originale );
			AiutanteFoto.disposeImmagini( fotografia, IdrataTarget.Risultante );
		}


		/// <summary>
		/// Aggiunge la correzione di tipo Logo alla fotografia indicata, utilizzando le impostazioni prese dalla configurazione.
		/// </summary>
		/// <param name="fotografia">la foto in esame</param>
		/// <param name="posiz">La stringa corrispondere alla enumerazione della posizione del logo</param>
		public Logo addLogoDefault( Fotografia fotografia, string posiz, bool salvare ) {

			Logo logoDefault = LogoCorrettore.creaLogoDefault();

			if( posiz == null )
				logoDefault.posiz = Logo.PosizLogo.SudEst;
			else
				logoDefault.posiz = (Logo.PosizLogo)Enum.Parse( typeof( Logo.PosizLogo ), posiz );

			addLogo( fotografia, logoDefault, salvare );

			return logoDefault;
		}

		public void addLogo( Fotografia fotografia, Logo logo, bool salvare ) {

			if( logo == null ) {
				
				// Rimuovo il logo dalle correzioni
				// Deserializzo la stringa con le eventuali correzioni attuali
				if( fotografia.correzioniXml != null ) {
					CorrezioniList correzioni = SerializzaUtil.stringToObject<CorrezioniList>( fotografia.correzioniXml );
					foreach( Correzione c in correzioni ) {
						if( c is Logo ) {
							correzioni.Remove( c );
							break;
						}
					}
					// Ora serializzo di nuovo
					fotografia.correzioniXml = SerializzaUtil.objectToString( correzioni );
				}

			} else {
				// Siccome ho reso il logo sommabile, questa operazione in realtà non aggiunge ma sostituisce.
				addCorrezione( fotografia, logo, salvare );
			}

			// Ricalcolo il provino giusto per poterlo visualizzare
			AiutanteFoto.creaProvinoFoto( fotografia );
		}

		public void ruotare( IEnumerable<Fotografia> fotografie, int pGradi ) {
			
			Ruota ruota = new Ruota( pGradi );

			// Devo informate tutti che queste foto sono cambiate. Preparo il messaggio
			FotoModificateMsg msg = new FotoModificateMsg( this );

			foreach( Fotografia f in fotografie ) {

				if( ruota.isAngoloRetto ) {
					autoRuotaSuOriginale( f, ruota );
				} else {
					addCorrezione( f, ruota );

					gestoreImmaginiSrv.salvaCorrezioniTransienti( f );
				}
				msg.add( f );
			}

			pubblicaMessaggio( msg );
		}

		public void applicareAzioneAutomatica( IEnumerable<Fotografia> fotografie, AzioneAuto azioneAuto ) {

			// Modifico sul db
			gestoreImmaginiSrv.salvaCorrezioniAutomatiche( fotografie, azioneAuto );

			provinare( fotografie );
		}

		/// <summary>
		/// Questo metodo serve a ricreare i provini di una o più foto.
		/// </summary>
		/// <param name="fotografie"></param>
		public void provinare( IEnumerable<Fotografia> fotografie ) {

			BackgroundWorker provinatore = new BackgroundWorker();
            provinatore.WorkerReportsProgress = true;
			provinatore.WorkerSupportsCancellation = false;
			provinatore.RunWorkerCompleted += Provinatore_RunWorkerCompleted;
			provinatore.DoWork += provinatore_DoWork;  // Questo non viene eseguito nel thread della UI e quindi mi da problemi
			provinatore.ProgressChanged += provinatore_ProgressChanged;
			provinatore.RunWorkerAsync( fotografie );
		}

		private DateTime _cronoInizio;
		private DateTime _cronoFine;

        private void Provinatore_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e ) {

			_cronoFine = DateTime.Now;

			FormuleMagiche.rilasciaMemoria();

			var tempo = _cronoFine - _cronoInizio;
			_giornale.Debug( "Crono = " + tempo.TotalSeconds );
		}

		void provinatore_ProgressChanged( object sender, ProgressChangedEventArgs e ) {

			Fotografia foto = (Fotografia)e.UserState;

			bool esistevaRisultante = foto.imgRisultante != null;


			// Rilascio memoria
			AiutanteFoto.disposeImmagini( foto, IdrataTarget.Tutte );

			AiutanteFoto.creaProvinoFoto( foto );

			// Le due foto grandi le rilascio per non intasare la memoria qualora questo metodo è chiamato più volte
			AiutanteFoto.disposeImmagini( foto, IdrataTarget.Originale );
			AiutanteFoto.disposeImmagini( foto, IdrataTarget.Risultante );

			
			bool esisteRisultante = foto.imgRisultante != null;

			// Siccome è molto probabile che venga idratata l'immagine risultante e siccome sono in un loop,
			// non posso tenere in memoria tanta roba, altrimenti esplode
			if( esisteRisultante && !esistevaRisultante ) {
				// Significa che l'ho idratata io in questo momento
				AiutanteFoto.disposeImmagini( foto, IdrataTarget.Risultante );
			}


			// Avviso tutti che questa foto è cambiata
			FotoModificateMsg msg = new FotoModificateMsg( this, foto );
			pubblicaMessaggio( msg );
		}

		void provinatore_DoWork( object sender, DoWorkEventArgs e ) {

			_cronoInizio = DateTime.Now;

			BackgroundWorker worker = sender as BackgroundWorker;
			IEnumerable<Fotografia> fotografie = (IEnumerable<Fotografia>) e.Argument;

			// TODO 
			// qui abbiamo un problema da risolvere:
			//   se imposto   0 = il programma è veloce ma la UI è bloccatissima (niente refresh, niente mouse)
			//   se imposto da 100 a 500 = la UI comincia a muoversi
			//   se imposto oltre 1000 sarebbe l'effetto più bello, ma ri rallenta di brutto. Su 100 foto si perdono 100 secondi (una enormità).
			// per ora scelgo la strada di tenere un minimo per rinfrescale le foto
			// Cmq non è un buon compromesso.

			short percentuale;
			short percentualePrec = -1;
			int conta = 0;
			int tot = fotografie.Count();


			foreach( Fotografia foto in fotografie ) {

				percentuale = (short) ((100 * (++conta)) / tot );

				// Eseguo il lavoro nel ReportProgess perché questo metodo viene eseguito nel thread della UI
				// Devo eseguirlo nel thread della UI perché altrimenti mi blocca tutto e il prg. diventa tutto bianco
				worker.ReportProgress( percentuale, foto );

				// Faccio degli sleep per fare respirare l'applicazione
				if( tot > 10 && percentuale != percentualePrec ) {
					// Per 3 volte faccio dei respiri lunghi (per dare il tempo alla UI di aggiornarsi)
					if( percentuale == 25 || percentuale == 50 || percentuale == 75 ) {
						Thread.Sleep( 2500 );
					} else
					// Le altre volta un respiro corto
					if( (percentuale % 10) == 0 ) {
						Thread.Sleep( 200 );
					}
				}

				percentualePrec = percentuale;
			}
		}

	}
}
