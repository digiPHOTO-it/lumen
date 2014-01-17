//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Digiphoto.Lumen.Model
{
    using System;
    using System.Collections.Generic;
	using System.ComponentModel;

	// Personalizzazione fuori standard.
	// Per evitare dei refresh continui alle CollectionView delle RigheCarrello,
	// ho deciso di notificare i PropertyChange sui due valori che servono durante la vendita.
	public partial class Carrello : INotifyPropertyChanged
    {
        public Carrello()
        {
            this.venduto = false;
            this.totMasterizzate = 0;
            this.righeCarrello = new HashSet<RigaCarrello>();
            this.incassiFotografi = new HashSet<IncassoFotografo>();
        }
    
        public System.Guid id { get; set; }
        public System.DateTime giornata { get; set; }
        public System.DateTime tempo { get; set; }

		private Nullable<decimal> _totaleAPagare;
		public Nullable<decimal> totaleAPagare
		{
			get
			{
				return _totaleAPagare;
			}
			set
			{
				if (_totaleAPagare != value)
				{
					_totaleAPagare = value;
					OnPropertyChanged("totaleAPagare");
				}
			}
		}

        public string intestazione { get; set; }
        public bool venduto { get; set; }
        public string note { get; set; }
        public short totMasterizzate { get; set; }

		private Nullable<decimal> _prezzoDischetto;
		public Nullable<decimal> prezzoDischetto
		{
			get
			{
				return _prezzoDischetto;
			}
			set
			{
				if (_prezzoDischetto != value)
				{
					_prezzoDischetto = value;
					OnPropertyChanged("prezzoDischetto");
				}
			}
		}
    
        public virtual ICollection<RigaCarrello> righeCarrello { get; set; }
        public virtual ICollection<IncassoFotografo> incassiFotografi { get; set; }

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property on this object has a new value.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The property that has a new value.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{

			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}

		#endregion // INotifyPropertyChanged Members
    }
}
