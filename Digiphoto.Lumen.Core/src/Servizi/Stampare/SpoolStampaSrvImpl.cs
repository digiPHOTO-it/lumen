using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Collections.Specialized;

using Digiphoto.Lumen.Imaging;
using log4net;
using Digiphoto.Lumen.Util;
using Digiphoto.Lumen.Threading;
using Digiphoto.Lumen.Config;

namespace Digiphoto.Lumen.Servizi.Stampare {

	internal class SpoolStampeSrvImpl : ServizioImpl, ISpoolStampeSrv {

		private static readonly ILog _giornale = LogManager.GetLogger( typeof(SpoolStampeSrvImpl) );


		public IList<CodaDiStampe> code {
			get;
			private set;
		}


		public SpoolStampeSrvImpl() {
			// Istanzio la mappa con tutte le code.
			this.code = new List<CodaDiStampe>();
		}

		protected override void Dispose( bool disposing ) {

			// Questo mi chiama lo stop del servizio che chiama lo stop delle code.
 			// Unico particolare che facendo lo stop, non forza l'abort di eventuali lavori di stampa accodati.
			// Quindi prima di uscire occorre aspettare che tutto si sia concluso.
			// Non so se è corretto dal punto di vista filosofico, ma dal punto di vista pratico non vorrei perdere delle stampe che sono già state contabilizzate.
			// Altrimenti
			// Bisognerebbe chiamare 				
			// c.Stop( PendingItemAction.AbortPendingItems );  // Se ci sono lavori in sospeso, devo abortirli.
			// ma dopo avrei dei problemi con la dispose.

			base.Dispose( disposing );

			// Faccio la dispose di tutte le code
			foreach( CodaDiStampe c in code ) {
				c.Dispose();
			}

		}

		/** Avvio tutte le stampe */
		public override void start() {
			foreach( CodaDiStampe c in code )
				c.Start();
			base.start();
		}

		/** Fermo tutte le stampe */
		public override void stop() {
			foreach( CodaDiStampe c in code )
				c.Stop();
			base.stop();
		}

		public void accodaStampaFoto( Fotografia foto, ParamStampaFoto param ) {

			if( param.nomeStampante == null )
				param.nomeStampante = ricavaStampante( param.formatoCarta );

			CodaDiStampe codaDiStampe = ricavaCodaDiStampa( param );

			// Creo un nuovo lavoro di stampa e lo aggiungo alla coda.
			LavoroDiStampaFoto lavoro = new LavoroDiStampaFoto(foto, param);
			codaDiStampe.EnqueueItem( lavoro );
		}

		public void accodaStampaProvini(IList<Fotografia> foto, ParamStampaProvini param)
		{

			if (param.nomeStampante == null)
				param.nomeStampante = ricavaStampante(param.formatoCarta);

			CodaDiStampe codaDiStampe = ricavaCodaDiStampa(param);

			// Creo un nuovo lavoro di stampa e lo aggiungo alla coda.
			LavoroDiStampaProvini lavoro = new LavoroDiStampaProvini(foto, param);
			codaDiStampe.EnqueueItem(lavoro);

		}

		private string ricavaStampante( FormatoCarta formatoCarta ) {

			_giornale.Warn( "Come mai non è definita la stampante? Va beh, la determino io" );

			StampanteAbbinata sa = stampantiAbbinate.FirstOrDefault<StampanteAbbinata>( s => s.FormatoCarta.Equals( formatoCarta ) );
			String nomeStampante = null;
			if( sa != null )
				nomeStampante = sa.StampanteInstallata.NomeStampante;
			else
				_giornale.Warn( "Non riesco a stabilire la stampante di questa carta: " + formatoCarta.descrizione + "(id=" + formatoCarta.id + ")" );

			return nomeStampante;
		}

		private CodaDiStampe ricavaCodaDiStampa( ParamStampa param) {

			string nomeStampante = param.nomeStampante;

			// Se non esiste già la stampante nella collezione, allora la istanzio
			CodaDiStampe coda = (from c in this.code
								 where c.Name.Equals( nomeStampante )
								 select c).SingleOrDefault<CodaDiStampe>();
			
			if( coda == null  ) {
				coda = new CodaDiStampe( param, nomeStampante, stampaCompletataCallback );
				coda.Start();
				this.code.Add( coda );
			}

			return coda;
		}

		private void stampaCompletataCallback( object sender, StampatoMsg eventArgs ) {

			if( eventArgs.lavoroDiStampa.esitostampa == EsitoStampa.Errore )
				_giornale.Error( "Stampa fallita. Esito = " + eventArgs.lavoroDiStampa.esitostampa );
			else
				_giornale.Info( "Stampa completata. Esito = " + eventArgs.lavoroDiStampa.esitostampa );

			// Notifico tutta l'applicazione
			pubblicaMessaggio( eventArgs );
		}

		/**
		 * Svuoto la coda che però rimane nello suo stato precedente.
		 * Se era running rimane running ... ecc.
		 */
		public void svuotaTutteLeCode() {
			foreach( CodaDiStampe c in this.code )
				c.Clear();
		}


		private StampantiAbbinateCollection _stampantiAbbinate;
		public StampantiAbbinateCollection stampantiAbbinate {
			get {
				if( _stampantiAbbinate == null )
					_stampantiAbbinate = StampantiAbbinateUtil.deserializza( Configurazione.UserConfigLumen.stampantiAbbinate );
				return _stampantiAbbinate;
			}
		}


	}
}
