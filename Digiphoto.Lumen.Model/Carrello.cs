//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Digiphoto.Lumen.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    
    public partial class Carrello : INotifyPropertyChanged 
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Carrello()
        {
            this.venduto = false;
            this.incassiFotografi = new HashSet<IncassoFotografo>();
            this.righeCarrello = new HashSet<RigaCarrello>();
        }
    
    	public System.Guid id { get; set; }
    	public System.DateTime giornata { get; set; }
    	public System.DateTime tempo { get; set; }
    	
    	private Nullable<decimal> _totaleAPagare;
    	public Nullable<decimal> totaleAPagare {
    		get {
    			return _totaleAPagare;
    		}
    		set {
    			if( _totaleAPagare != value ) {
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
    	public Nullable<decimal> prezzoDischetto {
    		get {
    			return _prezzoDischetto;
    		}
    		set {
    			if( _prezzoDischetto != value ) {
    				_prezzoDischetto = value;
    				OnPropertyChanged("prezzoDischetto");
    			}
    		}
    	}
    
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IncassoFotografo> incassiFotografi { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RigaCarrello> righeCarrello { get; set; }
    
    	#region INotifyPropertyChanged
    	public event PropertyChangedEventHandler PropertyChanged;
    
    	protected void OnPropertyChanged(string propertyName)
    	{
    		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    	}
    
    	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    	{
    		if (PropertyChanged != null)
    			PropertyChanged(this, e);
    	}
    	#endregion INotifyPropertyChanged	
    }
}
