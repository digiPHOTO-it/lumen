//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Digiphoto.Lumen.Model
{
    [DataContract(IsReference = true)]
    public partial class Giornata
    {
        [DataMember]
        public System.DateTime id { get; set; }
        [DataMember]
        public System.DateTime orologio { get; set; }
        [DataMember]
        public decimal incassoDichiarato { get; set; }
        [DataMember]
        public string note { get; set; }
        [DataMember]
        public decimal incassoPrevisto { get; set; }
        [DataMember]
        public string prgTaglierina1 { get; set; }
        [DataMember]
        public string prgTaglierina2 { get; set; }
        [DataMember]
        public string prgTaglierina3 { get; set; }
        [DataMember]
        public Nullable<short> totScarti { get; set; }
        [DataMember]
        public string firma { get; set; }
    }
    
}
