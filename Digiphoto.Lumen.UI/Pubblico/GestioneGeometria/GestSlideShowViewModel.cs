﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digiphoto.Lumen.UI.Mvvm;
using System.Windows.Input;
using Digiphoto.Lumen.Config;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using Digiphoto.Lumen.UI.Pubblico.GestioneGeometria;

namespace Digiphoto.Lumen.UI.Pubblico
{
	public class GestSlideShowViewModel : ViewModelBase
	{
		#region CostruttoreStatico

		static GestSlideShowViewModel()
		{
			cfg = Configurazione.UserConfigLumen;

			pSSG = new ParamSlideShowGeom();

			pSSG.fullScreen = cfg.fullScreen;

			pSSG.slideHeight = cfg.slideHeight;
			pSSG.slideWidth = cfg.slideWidth;

			pSSG.slideLeft = cfg.slideLeft;
			pSSG.slideTop = cfg.slideTop;
		}

		#endregion

		#region Proprietà

		public static ParamSlideShowGeom pSSG
		{
			get;
			set;
		}

		public static UserConfigLumen cfg
		{
			get;
			set;
		}

		#endregion

		#region Metodi

		private SlideShowViewModel slideShowViewModel
		{
			get
			{
				if (IsInDesignMode)
					return null;

				App myApp = (App)Application.Current;
				return myApp.slideShowViewModel;
			}
		}

		private void salva()
		{
			cfg = Configurazione.UserConfigLumen;

			cfg.slideHeight = pSSG.slideHeight;
			cfg.slideWidth = pSSG.slideWidth;

			cfg.slideTop = pSSG.slideTop;
			cfg.slideLeft = pSSG.slideLeft;

			cfg.deviceEnum = pSSG.deviceEnum;

			cfg.fullScreen = pSSG.fullScreen;

			_giornale.Debug("Devo salvare la configurazione utente su file xml");
			UserConfigSerializer.serializeToFile(Configurazione.UserConfigLumen);
			_giornale.Info("Salvata la configurazione utente su file xml");

			dialogProvider.ShowMessage("Posizione salvata Correttamente", "Avviso");
		}

		private void ripristina()
		{
			Configurazione.UserConfigLumen = UserConfigSerializer.deserialize();
			riposiziona();
			dialogProvider.ShowMessage("La posizione dello slideShow è stata ripristinata\nPremere salva per confermare","Avviso");
		}

		private void reset()
		{
			Configurazione.creaGeometriaSlideShowSDefault(Configurazione.UserConfigLumen);
			riposiziona();
			dialogProvider.ShowMessage("Modifica Applicata", "Avviso");
			dialogProvider.ShowMessage("La posizione dello slideShow è stata ripristinata\nPremere salva per confermare", "Avviso");
		}

		public static void riposiziona()
		{
			pSSG.slideTop = Configurazione.UserConfigLumen.slideTop;
			pSSG.slideLeft = Configurazione.UserConfigLumen.slideLeft;

			pSSG.slideHeight = Configurazione.UserConfigLumen.slideHeight;
			pSSG.slideWidth = Configurazione.UserConfigLumen.slideWidth;

			pSSG.fullScreen = Configurazione.UserConfigLumen.fullScreen;
		}

		#endregion

		#region Comandi

		private RelayCommand _salvaCommand;
		public ICommand salvaCommand
		{
			get
			{
				if (_salvaCommand == null)
				{
					_salvaCommand = new RelayCommand(param => salva());
				}
				return _salvaCommand;
			}
		}

		private RelayCommand _ripristinaCommand;
		public ICommand ripristinaCommand
		{
			get
			{
				if (_ripristinaCommand == null)
				{
					_ripristinaCommand = new RelayCommand(param => ripristina());
				}
				return _ripristinaCommand;
			}
		}

		private RelayCommand _resetCommand;
		public ICommand resetCommand
		{
			get
			{
				if (_resetCommand == null)
				{
					_resetCommand = new RelayCommand(param => reset());
				}
				return _resetCommand;
			}
		}

		#endregion
	}
}
