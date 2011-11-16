using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Digiphoto.Lumen.Imaging {

	public struct Proiezione {

		public Rectangle sorg {
			get;
			internal set;
		}

		public Rectangle dest {
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
		public Rectangle dest {
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
		public ProiettoreArea( Rectangle dest ) : this( dest, false ) {
		}

		public ProiettoreArea( Rectangle dest, bool autoZoomToFit ) {
			this.dest = dest;
			this.autoZoomToFit = autoZoomToFit;
			this.autoRotate = false;
			this.autoCentra = true;   // non ha senso lavorare senza centratura, almeno per default.
		}

		/**
		 * Calcolo la porzione della immagine sorgente che deve essere disegnata.
		 * Calcolo anche l'area di destinazione (stampante) in cui renderizzare
		 */
		public Proiezione calcola( Immagine sorgente ) {
			return calcola( new Rectangle( 0,0, sorgente.ww, sorgente.hh ) );
		}

		public Proiezione calcola( Rectangle sorgente ) {

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
		private Rectangle calcolaAreaSorgente( Rectangle sorgente ) {

			// Se le aree non sono omogenee (ossia una verticale e l'altra orizzontale) ed ho il permesso di ruotare,
			// allora giro per renderle omogenee
			if( autoRotate && !isStessoOrientamento( sorgente.Size, dest.Size ) )
				_effettuataRotazione = true;

			float localRatio = (_effettuataRotazione ? 1 / ratioDest : ratioDest);

			Rectangle tenta1 = Rectangle.Empty;
			int ww = (int)(((float)sorgente.Height) * localRatio);
			if( ww <= sorgente.Width )
				tenta1 = new Rectangle( sorgente.X, sorgente.Y, ww, sorgente.Height );

			Rectangle tenta2 = Rectangle.Empty;
			int hh = (int)(((float)sorgente.Width) / localRatio);
			if( hh <= sorgente.Height )
				tenta2 = new Rectangle( sorgente.X, sorgente.Y, sorgente.Width, hh );

			// Controllo di sicurezza: non possono essere entrambi vuoti
			Debug.Assert( !(tenta1.IsEmpty && tenta2.IsEmpty) );
		
			// --- decido quale dei due risultati sia meglio calcolando l'area della foto
			// --- Prendo quella dove l'area è più grande
			Rectangle esito = Rectangle.Empty;
			if( sizeCompare( tenta1.Size, tenta2.Size ) > 0 )
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
		private Rectangle calcolaAreaDestinazione( Rectangle sorgente ) {
	
			Rectangle esito = Rectangle.Empty;

			float ratioSrc = (float)sorgente.Width / (float)sorgente.Height;

			// Se le aree non sono omogenee (ossia una verticale e l'altra orizzontale) ed ho il permesso di ruotare,
			// allora giro per renderle omogenee
			if( autoRotate && !isStessoOrientamento( sorgente.Size, dest.Size ) ) {
				ratioSrc = 1 / ratioSrc;
				_effettuataRotazione = true;
			}
    
			// -- primo tentativo
			// dw : sw  = dh : sh
			//      sw * dh
			// dw = -------
			//         sh
			Rectangle tenta1 = Rectangle.Empty;
			tenta1.Height = dest.Height;
			tenta1.Width = (int) (tenta1.Height * ratioSrc );  // tronco senza arrotondare

			if( tenta1.Width > dest.Width ) {
			    //  --- non ci sta. azzero per secondo tentativo
                tenta1 = Rectangle.Empty;
			}

			// -- secondo tentativo
			// dw : sw  = dh : sh
			//      dw * sh
			// dh = -------
			//         sw
			Rectangle tenta2 = Rectangle.Empty;
			tenta2.Width = dest.Width;
			tenta2.Height = (int) (tenta2.Width / ratioSrc );  // Tronco senza arrotondare
			if( tenta2.Height > dest.Height ) {
                // --- non ci sta. impossibile.
                tenta2 = Rectangle.Empty;
			}

			Debug.Assert( !(tenta1.IsEmpty == true && tenta2.IsEmpty == true) );  // non possono essere entrambi vuoti.

			// --- decido quale dei due risultati sia meglio calcolando l'area della foto
			// --- Prendo quella dove l'area è più grande

			if( sizeCompare( tenta1.Size, tenta2.Size ) > 0 )
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

		private void controllaTuttoPositivo( Rectangle esito ) {
			Debug.Assert( esito.Left >= 0 );
			Debug.Assert( esito.Top >= 0 );
			Debug.Assert( esito.Width >= 0 );
			Debug.Assert( esito.Height >= 0 );
		}

		public static Rectangle ruota( Rectangle rect ) {

			Rectangle girato = new Rectangle( rect.Y, rect.X, rect.Height, rect.Width );
			return girato;
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
			
			int a1 = s1.Width * s1.Height;
			int a2 = s2.Width * s2.Height;
			return a1 - a2;
		}

		/**
		 * Mi dice se le due aree indicate sono orientate nello stesso verso
		 * (cioè entrambe verticali oppure entrambe orizzontali)
		 */
		private static bool isStessoOrientamento( Size s1, Size s2 ) {

			if( s1.Width > s1.Height && s2.Width > s2.Height )
				return true;

			if( s1.Width < s1.Height && s2.Width < s2.Height )
				return true;

			if( s1.Width == s1.Height && s2.Width == s2.Height )
				return true;

			return false;
		}
		
	}
}
