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
    
    public partial class FormatoCarta : INotifyPropertyChanged 
    {
    	public System.Guid id { get; set; }
    	public string descrizione { get; set; }
    	public decimal prezzo { get; set; }
    	public bool attivo { get; set; }
    	public Nullable<short> ordinamento { get; set; }
    
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
