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
    
    public partial class FormatoCarta
    {
        public FormatoCarta()
        {
            this.attivo = true;
        }
    
        public System.Guid id { get; set; }
        public string descrizione { get; set; }
        public decimal prezzo { get; set; }
        public bool attivo { get; set; }
        public Nullable<short> ordinamento { get; set; }
    }
}
