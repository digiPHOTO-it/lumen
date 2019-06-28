using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digiphoto.Lumen.Model {

	[Table( "RigheCarrelli" )]
	public partial class RigaCarrello : INotifyPropertyChanged {

		public const string TIPORIGA_STAMPA = "S";
		public const string TIPORIGA_MASTERIZZATA = "M";

		public RigaCarrello() {
		}

		public RigaCarrello( Prodotto prodotto, short quantita ) {
			this.id = Guid.Empty;
			this.prodotto = prodotto;
			this.quantita = quantita;
			this.prezzoLordoUnitario = prodotto.prezzo;
			this.prezzoNettoTotale = (this.quantita * this.prezzoLordoUnitario);
			this.discriminator = prodotto.tipologia;
			this.descrizione = prodotto.descrizione;
		}

		#region Attributi

		[Key]
		public System.Guid id { get; set; }

		public decimal prezzoLordoUnitario { get; set; }

		[NotMapped]
		public decimal prezzoNettoUnitario {
			get {
				return (sconto == null) ? prezzoLordoUnitario : (prezzoLordoUnitario - (decimal)sconto);
			}
		}

		private short _quantita;
		public short quantita {
			get {
				return _quantita;
			}
			set {
				if( _quantita != value ) {
					_quantita = value;
					OnPropertyChanged( "quantita" );
				}
			}
		}


		private decimal _prezzoNettoTotale;
		public decimal prezzoNettoTotale {
			get {
				return _prezzoNettoTotale;
			}
			set {
				if( _prezzoNettoTotale != value ) {
					_prezzoNettoTotale = value;
					OnPropertyChanged( "prezzoNettoTotale" );
				}
			}
		}

		public Nullable<decimal> sconto { get; set; }

		[Required]
		public string descrizione { get; set; }

		[Required]
		public string discriminator { get; set; }
		
		public Nullable<short> totFogliStampati { get; set; }

		public string nomeStampante { get; set; }

		public Nullable<bool> bordiBianchi { get; set; }

		[Required]
		[ForeignKey("carrello")]
		public System.Guid carrello_id { get; set; }
		public virtual Carrello carrello { get; set; }

		[Required]
		[ForeignKey("fotografo")]
		public string fotografo_id { get; set; }
		public virtual Fotografo fotografo { get; set; }

		[ForeignKey("fotografia")]
		public Nullable<System.Guid> fotografia_id { get; set; }
		public virtual Fotografia fotografia { get; set; }

		[Required]
		[ForeignKey( "prodotto" )]
		public Guid prodotto_id { get; set; }
		public virtual Prodotto prodotto { get; set; }

		#endregion Attributi

		#region Roba strana

		/// <summary>
		/// Data una riga di un carrelllo, ritorno il discriminatore invertito
		/// es.
		/// se Masterizzata -> torno Stampata
		/// se Stampata     -> torno Masterizzata
		/// </summary>
		/// <param name="riga"></param>
		/// <returns></returns>
		public string getDiscriminatorOpposto() {
			return RigaCarrello.getDiscriminatorOpposto( this.discriminator );
		}

		/// <summary>
		/// Data una riga di un carrelllo, ritorno il discriminatore invertito
		/// es.
		/// se Masterizzata -> torno Stampata
		/// se Stampata     -> torno Masterizzata
		/// </summary>
		/// <param name="riga"></param>
		/// <returns></returns>
		public static string getDiscriminatorOpposto( string disc ) {
			if( disc == RigaCarrello.TIPORIGA_MASTERIZZATA )
				return RigaCarrello.TIPORIGA_STAMPA;
			if( disc == RigaCarrello.TIPORIGA_STAMPA )
				return RigaCarrello.TIPORIGA_MASTERIZZATA;

			return null;
		}

		public bool isTipoStampa {
			get {
				return TIPORIGA_STAMPA == this.discriminator;
			}
		}

		public bool isTipoMasterizzata {
			get {
				return TIPORIGA_MASTERIZZATA == this.discriminator;
			}
		}

		#endregion Roba strana

		#region Uguaglianza

		public override bool Equals( object altro ) {
			bool uguali = false;
			if( altro != null && altro is RigaCarrello ) {
				uguali = this.id.Equals( ((RigaCarrello)altro).id );
			}

			return uguali;
		}

		public override int GetHashCode() {
			int hash = 7;
			hash = 31 * hash + (null == this.id ? 0 : this.id.GetHashCode());
			return hash;
		}

		#endregion Uguaglianza

		#region INotifyPropertyChanged

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( string propertyName ) {
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		protected virtual void OnPropertyChanged( PropertyChangedEventArgs e ) {
			if( PropertyChanged != null )
				PropertyChanged( this, e );
		}
		
		#endregion INotifyPropertyChanged	
		
	}
}
