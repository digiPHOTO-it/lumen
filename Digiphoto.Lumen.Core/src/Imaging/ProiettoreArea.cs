using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;

namespace Digiphoto.Lumen.Imaging {

	public struct Proiezione {

		public Int32Rect sorg {
			get;
			internal set;
		}

		public Int32Rect dest {
			get;
			internal set;
		}

		/** 
		 * Questa proprietà mi dice se è stata effettuata la rotazione della geometria di destinazione 
		 * Per esempio se voglio stampare una foto landscape su di un area di stampa portrait
		 */
		public bool effettuataRotazione {
			get;
			internal set;
		}
	}

	/**
	 * Serve per calcolare quale porzione della immagine sorgente, 
	 * deve essere proiettata (Draw) su quale porzione dell'area di stampa 
	 * della stampante.
	 */
	public class ProiettoreArea {

		#region Proprietà

		// -- Questa è l'area di destinazione. 
		// -- In pratica è la stampante
		public Int32Rect dest {
			get;
			private set;
		}

		public bool autoZoomToFit {
			get;
			set;
		}

		public bool autoRotate {
			get;
			set;
		}

		public bool autoCentra {
			get;
			set;
		}

		private bool _effettuataRotazione;

		#endregion

		/** Questo calcolatore si basa su di una area di destinazione che in pratica è la stampante */
		public ProiettoreArea( Int32Rect dest ) : this( dest, false ) {
		}

		public ProiettoreArea( Int32Rect dest, bool autoZoomToFit ) {
			this.dest = dest;
			this.autoZoomToFit = autoZoomToFit;
			this.autoRotate = false;
			this.autoCentra = true;   // non ha senso lavorare senza centratura, almeno per default.
		}

		public ProiettoreArea( Rect rectangleF ) : this( new Int32Rect( (int)rectangleF.Left, (int)rectangleF.Top, (int)rectangleF.Width, (int)rectangleF.Height ) ) {
		}

		/**
		 * Calcolo la porzione della immagine sorgente che deve essere disegnata.
		 * Calcolo anche l'area di destinazione (stampante) in cui renderizzare
		 */
		public Proiezione calcola( IImmagine sorgente ) {
			return calcola( new Int32Rect( 0,0, (int)sorgente.ww, (int)sorgente.hh ) );
		}

		public Proiezione calcola( Int32Rect sorgente ) {

			Proiezione proiezione = new Proiezione();

			_effettuataRotazione = false;

			if( autoZoomToFit ) {
				// In questo caso, l'area di stampa è massima e si scarta un pezzo della foto.
				proiezione.sorg = calcolaAreaSorgente( sorgente );
				proiezione.dest = dest;
			} else {
				// In questo caso, la foto è presa sempre tutta, ma l'area di stampa è parziale.
				proiezione.sorg = sorgente;
				proiezione.dest = calcolaAreaDestinazione( sorgente );
			}

			proiezione.effettuataRotazione = _effettuataRotazione;

			return proiezione; 
		}

		public float ratioDest {
			get {
				return (float) dest.Width / (float) dest.Height;
			}
		}

		/** 
		 * Siccome l'area di destinazione è tutta da riempire,
		 * devo calcolare la porzione dell'area sorgente da proiettare sul destinazione
		 */
		private Int32Rect calcolaAreaSorgente( Int32Rect sorgente ) {

			// Se le aree non sono omogenee (ossia una verticale e l'altra orizzontale) ed ho il permesso di ruotare,
			// allora giro per renderle omogenee
			if( autoRotate && !isStessoOrientamento( sorgente, dest ) )
				_effettuataRotazione = true;

			float localRatio = (_effettuataRotazione ? 1f/ratioDest : ratioDest);

			Int32Rect tenta1 = Int32Rect.Empty;
			int ww = (int)(((float)sorgente.Height) * localRatio);
			if( ww <= sorgente.Width )
				tenta1 = new Int32Rect( sorgente.X, sorgente.Y, ww, sorgente.Height );

			Int32Rect tenta2 = Int32Rect.Empty;
			int hh = (int)(((float)sorgente.Width) / localRatio);
			if( hh <= sorgente.Height )
				tenta2 = new Int32Rect( sorgente.X, sorgente.Y, sorgente.Width, hh );

			// Controllo di sicurezza: non possono essere entrambi vuoti
			Debug.Assert( !(tenta1.IsEmpty && tenta2.IsEmpty) );
		
			// --- decido quale dei due risultati sia meglio calcolando l'area della foto
			// --- Prendo quella dove l'area è più grande
			Int32Rect esito = Int32Rect.Empty;
			if( sizeCompare( tenta1, tenta2 ) > 0 )
				esito = tenta1;
			else
				esito = tenta2;

			// -- gestisco la centratura
			if( autoCentra ) {
				esito.X = (sorgente.Width - esito.Width) / 2;
				esito.Y = (sorgente.Height - esito.Height) / 2;
			}
			
			controllaTuttoPositivo( esito );

			return esito;
		}

		/** 
		 * L'area sorgente va proiettata tutta per intero.
		 * Non voglio perdere nessun dettaglio.
		 * Voglio stampare con i bordi bianchi.
		 * In questo caso devo calcolare la porzione dell'area destinazione
		 * su cui proiettare tutta l'area sorgente.
		 */
		private Int32Rect calcolaAreaDestinazione( Int32Rect sorgente ) {
	
			Int32Rect esito = Int32Rect.Empty;

			float ratioSrc = (float)sorgente.Width / (float)sorgente.Height;

			// Se le aree non sono omogenee (ossia una verticale e l'altra orizzontale) ed ho il permesso di ruotare,
			// allora giro per renderle omogenee
			if( autoRotate && !isStessoOrientamento( sorgente, dest ) ) {
				ratioSrc = 1 / ratioSrc;
				_effettuataRotazione = true;
			}
    
			// -- primo tentativo
			// dw : sw  = dh : sh
			//      sw * dh
			// dw = -------
			//         sh
			Int32Rect tenta1 = Int32Rect.Empty;
			tenta1.Height = dest.Height;
			tenta1.Width = (int) (tenta1.Height * ratioSrc );  // tronco senza arrotondare

			if( tenta1.Width > dest.Width ) {
			    //  --- non ci sta. azzero per secondo tentativo
                tenta1 = Int32Rect.Empty;
			}

			// -- secondo tentativo
			// dw : sw  = dh : sh
			//      dw * sh
			// dh = -------
			//         sw
			Int32Rect tenta2 = Int32Rect.Empty;
			tenta2.Width = dest.Width;
			tenta2.Height = (int) (tenta2.Width / ratioSrc );  // Tronco senza arrotondare
			if( tenta2.Height > dest.Height ) {
                // --- non ci sta. impossibile.
                tenta2 = Int32Rect.Empty;
			}

			Debug.Assert( !(tenta1.IsEmpty == true && tenta2.IsEmpty == true) );  // non possono essere entrambi vuoti.

			// --- decido quale dei due risultati sia meglio calcolando l'area della foto
			// --- Prendo quella dove l'area è più grande

			if( sizeCompare( tenta1, tenta2 ) > 0 )
				esito = tenta1;
			else
				esito = tenta2;
				

			// -- gestisco la centratura
			if( autoCentra ) {
				esito.X = (dest.Width - esito.Width) / 2;
				esito.Y = (dest.Height - esito.Height) / 2;
			}

			controllaTuttoPositivo( esito );

			return esito;
		}

		private void controllaTuttoPositivo( Int32Rect esito ) {
			Debug.Assert( esito.X >= 0 );
			Debug.Assert( esito.Y >= 0 );
			Debug.Assert( esito.Width >= 0 );
			Debug.Assert( esito.Height >= 0 );
		}

		public static Int32Rect ruota( Int32Rect rect ) {
			return new Int32Rect( rect.Y, rect.X, rect.Height, rect.Width );
		}

		public static Rect ruota( Rect rect ) {
			return new Rect( rect.Y, rect.X, rect.Height, rect.Width );
		}

		public static Size ruota( Size size ) {
			return new Size( size.Height, size.Width );
		}

		/**
		 * Ritorna un intero minore, uguale, maggiore di zero
		 * rispettivamente se l'area1 è più grande della 2
		 */
		private static int sizeCompare( Size s1, Size s2 ) {
			
			if( s2.IsEmpty && !s1.IsEmpty )
				return 1;

			if( s1.IsEmpty && !s2.IsEmpty )
				return (-1);
			
			double a1 = s1.Width * s1.Height;
			double a2 = s2.Width * s2.Height;
			
			if( a1 == a2 )
				return 0;
			else
				return a1 > a2 ? 1 : -1;
		}

		private static int sizeCompare( Int32Rect r1, Int32Rect r2 ) {
			Size s1 = new Size( r1.Width, r1.Height );
			Size s2 = new Size( r2.Width, r2.Height );
			return sizeCompare( s1, s2 );
		}

		/**
		 * Mi dice se le due aree indicate sono orientate nello stesso verso
		 * (cioè entrambe verticali oppure entrambe orizzontali)
		 */
		public static bool isStessoOrientamento( Size s1, Size s2 ) {
			return isStessoOrientamento( s1.Width, s1.Height, s2.Width, s2.Height );
		}

		public static bool isStessoOrientamento( Int32Rect s1, Int32Rect s2 ) {
			return isStessoOrientamento( s1.Width, s1.Height, s2.Width, s2.Height );
		}

		public static bool isStessoOrientamento( Size s1, IImmagine s2 ) {
			return isStessoOrientamento( s1.Width, s1.Height, s2.ww, s2.hh );
		}

		public static bool isStessoOrientamento( double w1, double h1, double w2, double h2 ) {

			if( w1 > h1 && w2 > h2 )
				return true;

			if( w1 < h1 && w2 < h2 )
				return true;

			if( w1 == h1 && w2 == h2 )
				return true;

			return false;
		}

	}
}
