using System;
using Digiphoto.Lumen.UI.Mvvm;
using Digiphoto.Lumen.Servizi.VolumeCambiato;
using Digiphoto.Lumen.Applicazione;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows;
using System.Collections.Generic;

namespace Digiphoto.Lumen.UI {

	public class SelettoreCartellaViewModel : ViewModelBase, IObserver<VolumeCambiatoMsg> {

		public SelettoreCartellaViewModel() {

			DisplayName = "Selettore cartella scarico foto";

			IObservable<VolumeCambiatoMsg> observable = LumenApplication.Instance.bus.Observe<VolumeCambiatoMsg>();
			observable.Subscribe( this );

			caricaElencoDischiRimovibili();

			caricaElencoCartelleRecenti();
		}

		#region Properietà

		public ObservableCollection<String> cartelleRecenti {
			get;
			set;
		}

		public ObservableCollectionEx<DriveInfo> dischiRimovibili {
			get;
			set;
		}

		private string _cartellaSelezionata;
		public string cartellaSelezionata {
			get {
				return _cartellaSelezionata;
			}
			set {
				if( value != _cartellaSelezionata ) {
					_cartellaSelezionata = value;
					OnPropertyChanged( "cartellaSelezionata" );
				}
			}
		}

		IVolumeCambiatoSrv volumeCambiatoSrv {
			get {
				return LumenApplication.Instance.getServizioAvviato<IVolumeCambiatoSrv>();
			}
		}

		#endregion


		#region Comandi

		private RelayCommand _scegliereCartellaCommand;
		public ICommand scegliereCartellaCommand {
			get {
				if( _scegliereCartellaCommand == null ) {
					_scegliereCartellaCommand = new RelayCommand( param => this.scegliereCartella() );
				}
				return _scegliereCartellaCommand;
			}
		}

		#endregion


		#region Metodi
		
		
		private void scegliereCartella() {

			// TODO questa chiamata sarebbe da spostare nella View (non dovrebbe stare nel ViewModel).
			//      vedere: 
			//      http://msdn.microsoft.com/en-us/library/gg405494%28v=pandp.40%29.aspx#UserInteractionPatterns
			//
			using( FolderBrowserDialog dlg = new FolderBrowserDialog() ) {
				dlg.Description = "Cartella da cui scaricare le foto";
				dlg.SelectedPath = cartellaSelezionata;
				dlg.ShowNewFolderButton = false;
				DialogResult result = dlg.ShowDialog();
				if( result == System.Windows.Forms.DialogResult.OK ) {
					cartellaSelezionata = dlg.SelectedPath;

					// Aggiungo la cartella scelta, alla lista delle ultime

					if( ! cartelleRecenti.Contains( cartellaSelezionata ) )
						cartelleRecenti.Add( cartellaSelezionata );
				}
			}
		}

		private void caricaElencoDischiRimovibili() {
			
			DriveInfo [] dischi;

			if( IsInDesignMode ) {
				dischi = creaDischiFinti();
			} else
				dischi = volumeCambiatoSrv.GetDrivesUsbAttivi();
			dischiRimovibili = new ObservableCollectionEx<DriveInfo>( dischi );
			OnPropertyChanged( "dischiRimovibili" );
		}

		private void caricaElencoCartelleRecenti() {

			if( IsInDesignMode ) {
				string [] appo = new string [] { @"c:\aaa\bbb\ccc", @"d:\qqq\www\hhh", @"e:\ppp\kkk" };
				cartelleRecenti = new ObservableCollection<string>( appo );
			} else {
				// Si potrebbe anche deserializzarle da qualche parte. Per ora parto vuoto.
				cartelleRecenti = new ObservableCollection<string>();
			}
		}
	
		private DriveInfo [] creaDischiFinti() {
			
			return new DriveInfo[0];

			// non so perchè ma non funzionano.
			//DriveInfo d1 = new DriveInfo( "C:" );
			//d1.VolumeLabel = "DISCO 1";
			//DriveInfo d2 = new DriveInfo( "C:" );
			//d1.VolumeLabel = "DISCO 2";
			//DriveInfo d3 = new DriveInfo( "C:" );
			//d1.VolumeLabel = "DISCO 3";
			
			//DriveInfo [] ret = new DriveInfo []  { d1, d2, d3 };
			//return ret;
		}
		#endregion


		#region interfaccia IObserver


		public void OnCompleted() {
		}

		public void OnError( Exception error ) {
		}

		public void OnNext( VolumeCambiatoMsg value ) {
			aggiungiTogliNomeVolume( value.montato, value.nomeVolume );
		}

		private void aggiungiTogliNomeVolume( bool aggiungi, string nomeVolume ) {

			// Se è stata inserita una chiavetta usb, mi arriva il messaggio e mi serve per ricaricare la lista.
			DriveInfo driveInfo = cercaDiscoRimovibileConNome( nomeVolume );
			if( driveInfo == null )
				driveInfo = new DriveInfo( nomeVolume );


			if( aggiungi ) {
				if( !dischiRimovibili.Contains( driveInfo ) )
					dischiRimovibili.Add( driveInfo );
			} else {
				// E' stato smontato. lo rimuovo
				if( dischiRimovibili.Contains( driveInfo ) )
					dischiRimovibili.Remove( driveInfo );
			}
			OnPropertyChanged( "dischiRimovibili" );

		}

		// Cerca nella collezione dei dischi rimovibili se ne esiste uno con il nome indicato
		private DriveInfo cercaDiscoRimovibileConNome( string nomeVolume ) {

			DriveInfo ret = null;
			foreach( DriveInfo driveInfo in dischiRimovibili ) {
				if( driveInfo.RootDirectory.Name.StartsWith( nomeVolume ) )  {
					ret = driveInfo;
					break;
				}
			}
			return ret;
		}


		#endregion

	}
}
