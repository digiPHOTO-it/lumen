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
    
    public partial class Giornata
    {
        public System.DateTime id { get; set; }
        public System.DateTime orologio { get; set; }
        public decimal incassoDichiarato { get; set; }
        public string note { get; set; }
        public decimal incassoPrevisto { get; set; }
        public string prgTaglierina1 { get; set; }
        public string prgTaglierina2 { get; set; }
        public string prgTaglierina3 { get; set; }
        public Nullable<short> totScarti { get; set; }
        public string firma { get; set; }
    }
}
