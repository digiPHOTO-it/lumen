
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digiphoto.Lumen.Model {

	[Table("Carrelli")]
	public partial class Carrello : INotifyPropertyChanged {

		public Carrello() {
			this.venduto = false;
			this.visibileSelfService = true;
			this.incassiFotografi = new HashSet<IncassoFotografo>();
			this.righeCarrello = new HashSet<RigaCarrello>();
		}

		#region Attributi

		[Key]
		public System.Guid id { get; set; }

		[Required]
		public System.DateTime giornata { get; set; }

		[Required]
		public System.DateTime tempo { get; set; }

		private Nullable<decimal> _totaleAPagare;
		public Nullable<decimal> totaleAPagare {
			get {
				return _totaleAPagare;
			}
			set {
				if( _totaleAPagare != value ) {
					_totaleAPagare = value;
					OnPropertyChanged( "totaleAPagare" );
				}
			}
		}

		public string intestazione { get; set; }

		public bool venduto { get; set; }

		public string note { get; set; }

		public short totMasterizzate { get; set; }

		public bool visibileSelfService { get; set; }

		/// <summary>
		/// Codice corto di soli quattro caratteri da usare per identificare il carrello nel self-service web
		/// </summary>
		public string idCortoSelfService {
			get; set;
		}

		private Nullable<decimal> _prezzoDischetto;
		public Nullable<decimal> prezzoDischetto {
			get {
				return _prezzoDischetto;
			}
			set {
				if( _prezzoDischetto != value ) {
					_prezzoDischetto = value;
					OnPropertyChanged( "prezzoDischetto" );
				}
			}
		}

		public virtual ICollection<IncassoFotografo> incassiFotografi { get; set; }

		public virtual ICollection<RigaCarrello> righeCarrello { get; set; }


		#endregion Attributi


		#region Uguaglianza

		public override bool Equals( object altro ) {
			bool uguali = false;
			if( altro != null && altro is Carrello ) {
				uguali = this.id.Equals( ((Carrello)altro).id );
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
