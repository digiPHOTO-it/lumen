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

namespace Digiphoto.Lumen.Servizi.Ritoccare {

	/// <summary>
	/// Lavora sempre sulla "Fotografia" e non sulla IImmagine.
	/// Gestisce le Correzioni 
	/// </summary>
	public class FotoRitoccoSrvImpl : SelettoreMultiFotoImpl, IFotoRitoccoSrv {

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
					addCorrezione( f, correzione );
			}


		}

		// ok ho deciso che la correzione viene accettata
		public void addCorrezione( Fotografia fotografia, Correzione correzione ) {
			addCorrezione( fotografia, correzione, true );
		}

		// ok ho deciso che la correzione viene accettata
		public void addCorrezione( Fotografia fotografia, Correzione correzione, bool salvare ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;
			CorrezioniList correzioni;

			// Deserializzo la stringa con le eventuali correzioni attuali
			if( fotografia.correzioniXml == null )
				correzioni = new CorrezioniList();
			else
				correzioni = SerializzaUtil.stringToObject<CorrezioniList>( fotografia.correzioniXml );

			// Aggiungo in fondo
			correzioni.Add( correzione );

			// ricalcolo il provino
			IImmagine nuova = gestoreImmaginiSrv.applicaCorrezione( fotografia.imgProvino, correzione );

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

		public override IEnumerable<Fotografia> tutteLeFoto {
			get {
				return fotografieDaModificare;
			}
		}

		private IGestoreImmagineSrv gestoreImmaginiSrv {
			get {
				return (IGestoreImmagineSrv) LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			}
		}

		public void tornaOriginale( Fotografia fotografia, bool salvare ) {

			LumenEntities objContext = UnitOfWorkScope.CurrentObjectContext;

			fotografia.correzioniXml = null;

			AiutanteFoto.creaProvinoFoto( fotografia );

			objContext.SaveChanges();
		}
	}
}
