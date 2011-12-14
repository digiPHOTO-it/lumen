using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.Model;
using System.Collections.Specialized;
using System.Drawing.Printing;
using Digiphoto.Lumen.Imaging;
using log4net;

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

		public override void Dispose() {
			// Faccio la dispose di tutte le code
			foreach( CodaDiStampe c in code )
				c.Dispose();
			base.Dispose();
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

		public void accodaStampa( Fotografia foto, ParamStampaFoto param ) {

			if( param.nomeStampante == null )
				param.nomeStampante = ricavaStampante( param.formatoCarta );

			CodaDiStampe codaDiStampe = ricavaCodaDiStampa( param.nomeStampante );

			// Creo un nuovo lavoro di stampa e lo aggiungo alla coda.
			LavoroDiStampa lavoro = new LavoroDiStampa( foto, param );
			codaDiStampe.EnqueueItem( lavoro );
		}

		private string ricavaStampante( FormatoCarta formatoCarta ) {
			// TODO gestire le preferenze utente dove indicare l'associazione tra stampanti e formati carta
			return "dOPDF v7";
		}

		private CodaDiStampe ricavaCodaDiStampa( string nomeStampante ) {

			// Se non esiste già la stampante nella collezione, allora la istanzio
			CodaDiStampe coda = (from c in this.code
								 where c.Name.Equals( nomeStampante )
								 select c).SingleOrDefault<CodaDiStampe>();

			if( coda == null  ) {
				coda = new CodaDiStampe( nomeStampante, stampaCompletataEventHandler );
				coda.Start();
				this.code.Add( coda );
			}

			return coda;
		}

		private void stampaCompletataEventHandler( object sender, StampatoMsg eventArgs ) {

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
	}
}
