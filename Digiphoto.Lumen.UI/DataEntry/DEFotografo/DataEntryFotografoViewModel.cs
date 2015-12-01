using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Digiphoto.Lumen.Model;
using System.Linq.Expressions;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Servizi.Vendere;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Util;
using System.IO;
using Digiphoto.Lumen.Servizi.Io;
using System.ComponentModel;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using Digiphoto.Lumen.UI.Util;

namespace Digiphoto.Lumen.UI.DataEntry.DEFotografo {

	public class DataEntryFotografoViewModel : DataEntryViewModel<Fotografo> {


		public DataEntryFotografoViewModel() : base() {
			collectionView.CurrentChanged += entitaCorrenteCambiata;
		}

		#region Metodi

		protected override void OnDispose() {
			collectionView.CurrentChanged -= entitaCorrenteCambiata;
			base.OnDispose();
		}

		void entitaCorrenteCambiata( object sender, EventArgs e ) {
			caricaImmagineEsistente();
		}

		protected override void passoPreparaAddNew( Fotografo fotografo ) {

			// Calcolo un codice numerico da 4 cifre
			object prox = entityRepositorySrv.getNextId();
			if( prox != null )
				fotografo.id = (string)prox;

			fotografo.attivo = true;
			fotografo.umano = true;
		}

		protected override bool cancella( Fotografo entita ) {
			bool esito = base.cancella( entita );
			if( esito == true ) {
				string nomeFileImg = AiutanteFoto.nomeFileImgFotografo( entita );
				try {
					File.Delete( nomeFileImg );
				} catch( Exception ) {
					_giornale.Warn( "Impossibile cancellare il file con l'immagine del fotografo: " + nomeFileImg );
				}
			}
			return esito;
		}

		protected override void passoPrimaDiSalvare( Fotografo fotografo ) {
			collectionView.Refresh();
		}

		protected override object passoCaricaDati() {
			IQueryable<Fotografo> q = entityRepositorySrv.Query();
			return q.OrderByDescending( gg => gg.id );
		}

		protected override void passoPreparaEdit( Fotografo fotografo ) {
			collectionView.Refresh();
		}

		bool esisteImmagineFotografoCorrente() {
			string nomeFile = AiutanteFoto.nomeFileImgFotografo( entitaCorrente );
			return File.Exists( nomeFile );
		}

		private void caricaImmagineEsistente() {
			
			string nomeFile = AiutanteFoto.nomeFileImgFotografo( entitaCorrente );
			if( nomeFile != null && File.Exists( nomeFile ) ) {
				IGestoreImmagineSrv g = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
				immagineFotografo = g.load( nomeFile );
			} else
				immagineFotografo = null;
		}

		private void uploadNuovaImmagine() {
			
			string nomeImmagine = AiutanteUI.scegliFileImmagineDialog( null );
			if( nomeImmagine != null ) {
				string nomeFileDest = AiutanteFoto.nomeFileImgFotografo(entitaCorrente);
				DirectoryInfo dInfo = new DirectoryInfo(nomeFileDest).Parent;
				if (!dInfo.Exists)
					Directory.CreateDirectory( dInfo.FullName );
                File.Copy( nomeImmagine, nomeFileDest, true );
				caricaImmagineEsistente();
			}

		}

		#endregion

		#region Proprietà

		/// <summary>
		/// Immagine del fotografo attivo
		/// </summary>
		private IImmagine _immagineFotografo;
		public IImmagine immagineFotografo {
			get {
				return _immagineFotografo;
			}
			set {
				if( _immagineFotografo != value ) {
					_immagineFotografo = value;
					OnPropertyChanged( "immagineFotografo" );
				}
			}
		}

		public bool possoUploadNuovaImmagine {
			get {
				return canEditFields;
			}
		}

		#endregion Proprietà
		
		#region Comandi

		private RelayCommand _commandUploadNuovaImmagine;
		public ICommand commandUploadNuovaImmagine {
			get {
				if( _commandUploadNuovaImmagine == null ) {
					_commandUploadNuovaImmagine = new RelayCommand( param => uploadNuovaImmagine(),
																    param => possoUploadNuovaImmagine,
																    false );
				}
				return _commandUploadNuovaImmagine;
			}
		}

		#endregion Comandi
	}
}
