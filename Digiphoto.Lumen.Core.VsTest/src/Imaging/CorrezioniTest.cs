﻿using System;
using System.Diagnostics;
using System.Linq;
using Digiphoto.Lumen.Applicazione;
using Digiphoto.Lumen.Imaging;
using Digiphoto.Lumen.Imaging.Correzioni;
using Digiphoto.Lumen.Imaging.Wic;
using Digiphoto.Lumen.Imaging.Wic.Correzioni;
using Digiphoto.Lumen.Model;
using Digiphoto.Lumen.Servizi.Io;
using Digiphoto.Lumen.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digiphoto.Lumen.Core.VsTest {

	[TestClass]
	public class CorrezioniTest {


		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void CarrelloTestInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}

		[ClassCleanup()]
		public static void classeCleanup() {
			LumenApplication.Instance.ferma();
		}

		private LogoCorrettore _correttore;
		[TestMethod]
		public void applicaCorrezioneLogo() {

			using( LumenEntities dbContext = new LumenEntities() ) {


				// Scelgo una foto qualsiasi e prendo l'immagine originale (cosi non rischio di avere già dei loghi)
				Fotografia foto = dbContext.Fotografie.FirstOrDefault();
				if( foto == null )
					return;

				AiutanteFoto.idrataImmaginiFoto( foto, IdrataTarget.Originale );

				ImmagineWic iw = (ImmagineWic)foto.imgOrig;

				_correttore = new LogoCorrettore();
				Logo logo = LogoCorrettore.creaLogoDefault();

				


				/*
				logo.zoom = new Zoom {
					fattore = 3
				};

				logo.rotazione = new Ruota( 45 );
		
				logo.traslazione = new Trasla {
					offsetX = 100,
					offsetY = 300,
					rifW = iw.ww,
					rifH = iw.hh
				};
				*/

				//
				logo.posiz = Logo.PosizLogo.NordEst;
				vediLogo( foto.imgOrig, logo );
				//
				logo.posiz = Logo.PosizLogo.NordOvest;
				vediLogo( foto.imgOrig, logo );
				//
				logo.posiz = Logo.PosizLogo.SudEst;
				vediLogo( foto.imgOrig, logo );
				//
				logo.posiz = Logo.PosizLogo.SudOvest;
				vediLogo( foto.imgOrig, logo );
			}

		}

		private void vediLogo( IImmagine immagine, Logo logo ) {

			IImmagine imgConLogo = _correttore.applica( immagine, logo );

			// Salvo su disco l'immagine di destinazione
			IGestoreImmagineSrv gis = LumenApplication.Instance.getServizioAvviato<IGestoreImmagineSrv>();
			String nomeFile =  @"c:\tmp\imgConLogo-" + logo.posiz.ToString() + ".png";
			gis.save( imgConLogo, nomeFile );

			Process p = new Process();
			p.StartInfo.FileName = "rundll32.exe";

			//Arguments
			p.StartInfo.Arguments = @"C:\WINDOWS\System32\shimgvw.dll,ImageView_Fullscreen " + nomeFile;
			p.Start();
		}



	}
}
