using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using Digiphoto.Lumen.Config;
using System.Windows;
using Digiphoto.Lumen.UI.Pubblico.GestioneGeometria;
using System.Diagnostics;
using System;

namespace Digiphoto.Lumen.UI.Preferenze
{
	public class PreferenzeViewModel : ClosableWiewModel {

		public PreferenzeViewModel() {

			// Copio i valori dalla configurazione in locale,
			// per permettere di editarli e anche di abortire l'editing
			this.prefGalleryViste = new Griglia[Configurazione.MAX_STELLINE];

			if( cfg.prefGalleryViste != null )
				cfg.prefGalleryViste.CopyTo( this.prefGalleryViste, 0 );
			else {
	
				for( int ii = 0; ii < Configurazione.MAX_STELLINE; ii++ ) {

					Griglia g = new Griglia();

					// Imposto un default possibile
					switch( ii ) {
						case 0:
							g.numRighe = 1;
							g.numColonne = 1;
							break;
						case 1:
							g.numRighe = 2;
							g.numColonne = 4;
							break;
						case 2:
							g.numRighe = 4;
							g.numColonne = 6;
							break;
					}

					this.prefGalleryViste[ii] = g;
				}

			}
		}

		#region Proprietà

		/// <summary>
		/// Un elemento dell'array per ogni stellina. Ogni elemento Griglia contiente righe e colonne.
		/// </summary>
		public Griglia [] prefGalleryViste {
			get;
			private set;
		}

		public static UserConfigLumen cfg
		{
			get {
				return Configurazione.UserConfigLumen;
			}
		}

		#endregion Proprieta

		#region Metodi

		/// <summary>
		/// Copio i valori delle preferenze utente, nella configurazione e quindi la salvo (serializzo su disco)
		/// </summary>
		private void uscire( bool salvare )
		{
		
			if( salvare ) {

				if( cfg.prefGalleryViste == null )
					cfg.prefGalleryViste = new Griglia[Configurazione.MAX_STELLINE];
				
                this.prefGalleryViste.CopyTo( cfg.prefGalleryViste, 0 );

				UserConfigSerializer.serializeToFile( Configurazione.UserConfigLumen );
				LastUsedConfigSerializer.serializeToFile( Configurazione.LastUsedConfigLumen );
				_giornale.Info("Salvate le preferenze utente su file xml");
			}

			this.CloseCommand.Execute( null );
		}

		#endregion Metodi

		#region Comandi

		private RelayCommand _uscireCommand;
		public ICommand uscireCommand
		{
			get
			{
				if (_uscireCommand == null)
				{
					_uscireCommand = new RelayCommand( param => uscire( Boolean.Parse( (String)param ) ) );
				}
				return _uscireCommand;
			}
		}
		
	
		#endregion Comandi
	}
}
