﻿using Digiphoto.Lumen.Servizi.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Digiphoto.Lumen.Servizi.Ricerca;
using Digiphoto.Lumen.Comandi;
using Digiphoto.Lumen.Model;
using System.ComponentModel;
using Digiphoto.Lumen.Applicazione;
using System.Threading;

namespace Digiphoto.Lumen.Core.VsTest
{
    
    
    /// <summary>
    ///This is a test class for FotoExplorerSrvImplTest and is intended
    ///to contain all FotoExplorerSrvImplTest Unit Tests
    ///</summary>
	[TestClass()]
	public class FotoExplorerSrvImplTest : IObserver<RicercaModificataMessaggio> {

		FotoExplorerSrvImpl _impl;
		private TestContext testContextInstance;
		private bool _caricateFoto;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize( TestContext testContext ) {
			LumenApplication.Instance.avvia();
		}
		
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		[TestInitialize()]
		public void MyTestInitialize() {
			IObservable<RicercaModificataMessaggio> observable = LumenApplication.Instance.bus.Observe<RicercaModificataMessaggio>();
			observable.Subscribe( this );
			_impl = new FotoExplorerSrvImpl(); // TODO: Initialize to an appropriate value
		}
		
		//Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup() {
			_impl.Dispose();
		}
		
		#endregion


		/// <summary>
		///A test for cercaFoto
		///</summary>
		[TestMethod()]
		public void cercaFotoTest() {
			
			ParamRicercaFoto param = new ParamRicercaFoto();
			param.giornataIniz = new DateTime( 2000, 1, 1 );
			_impl.cercaFoto( param );

			Assert.IsTrue( _impl.fotografie.Count > 0 );

			while( !_caricateFoto )
				Thread.Sleep( 1000 );
			
			// Prendo la prima foto e la setto come corrente
			_impl.fotoCorrente = _impl.fotografie[0];
		}


		[TestMethod()]
		public void comandoBiancoNeroTest() {

			cercaFotoTest();

			Assert.IsTrue( _caricateFoto );

			Correzione cor = new BiancoNeroCorrezione();
			Comando cmd = new CorrezioneComando( Target.Corrente, cor );
			_impl.invoca( cmd );
		}

		public void OnCompleted() {
			throw new NotImplementedException();
		}

		public void OnError( Exception error ) {
			throw new NotImplementedException();
		}

		public void OnNext( RicercaModificataMessaggio value ) {
			_caricateFoto = true;
		}
	}
}
