﻿using System;
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
using Digiphoto.Lumen.Servizi.Selezionare;
using System.Data.Objects;
using System.Data;

namespace Digiphoto.Lumen.Servizi.Ritoccare {

	/// <summary>
	/// Lavora sempre sulla "Fotografia" e non sulla IImmagine.
	/// Gestisce le Correzioni 
	/// </summary>
	public class FotoRitoccoSrvImpl : ServizioImpl, IFotoRitoccoSrv {

		public FotoRitoccoSrvImpl() {

			// Faccio una prova scema: TODO togliere.
			LumenEntities dbContext = UnitOfWorkScope.CurrentObjectContext;
			fotografieDaModificare = dbContext.Fotografie.Top( Convert.ToString( 10 ) ).ToList();
			foreach( Fotografia foto in fotografieDaModificare )
				Digiphoto.Lumen.Util.AiutanteFoto.idrataImmaginiFoto( foto );
		}

		private List<Fotografia> _fotografieDaModificare;
		public List<Fotografia> fotografieDaModificare {
			get {
				return _fotografieDaModificare;
			}
			set {
				_fotografieDaModificare = value;
			}
		}


		public void tornaOriginale( Target target ) {

			if( target == Target.Selezionate ) {
				foreach( Fotografia f in fotoSelezionate )
					tornaOriginale( f );
			}
		}

		public void tornaOriginale( Fotografia fotografia ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;

			fotografia.correzioniXml = null;

			AiutanteFoto.creaProvinoFoto( fotografia );

			objContext.SaveChanges();
		}

		public void addCorrezione( Target target, Correzione correzione ) {

			if( target == Target.Selezionate ) {
				foreach( Fotografia f in fotoSelezionate )
					addCorrezione( f, correzione, false );
			}


		}

		// TODO Non è efficiente. Sostituire
		// http://stackoverflow.com/questions/451748/wpf-m-v-vm-get-selected-items-from-a-listcollectionview
		public IEnumerable<Fotografia> fotoSelezionate {
			get {
				return fotografieDaModificare.Where( f => f.isSelezionata == true );
			}
		}




		// ok ho deciso che la correzione viene accettata
		public void addCorrezione( Fotografia fotografia, Correzione correzione ) {
			addCorrezione( fotografia, correzione, true );
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
		public void tornaOriginale( Fotografia fotografia, bool salvare ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;

			fotografia.correzioniXml = null;

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

			objectContext.Attach( fotografia );
			objContext.Refresh( RefreshMode.StoreWins, fotografia );

			fotografia.imgProvino = null;  // Questo forza la rilettura del provino da disco
			AiutanteFoto.idrataImmaginiFoto( IdrataTarget.Provino, fotografia );
		}

		public void undoCorrezioniTransienti( Target target ) {
			if( target == Target.Selezionate ) {
				foreach( Fotografia f in fotoSelezionate )
					undoCorrezioniTransienti( f );
			}
		}


		public void salvaCorrezioniTransienti( Fotografia fotografia ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			
			objContext.Fotografie.Attach( fotografia );
			objContext.ObjectStateManager.ChangeObjectState( fotografia, EntityState.Modified );
			objContext.SaveChanges();

			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			gis.save( fotografia.imgProvino, PathUtil.nomeCompletoProvino( fotografia ) );
		}

		public void salvaCorrezioniTransienti( Target target ) {
			if( target == Target.Selezionate ) {
				foreach( Fotografia f in fotoSelezionate )
					salvaCorrezioniTransienti( f );
			}
		}


		public void modificaMetadati( Fotografia foto ) {
			// TODO
			throw new NotImplementedException();
		}
	}
}
