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
    public partial class InfoFissa
    {
        public InfoFissa()
        {
            this.id = "K";
            this.versioneDbCompatibile = "1.0";
            this.modoNumerazFoto = "M";
        }
    
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public int ultimoNumFotogramma { get; set; }
        [DataMember]
        public Nullable<System.DateTime> dataUltimoScarico { get; set; }
        [DataMember]
        public string versioneDbCompatibile { get; set; }
        [DataMember]
        public string modoNumerazFoto { get; set; }
        [DataMember]
        public short pixelProvino { get; set; }
        [DataMember]
        public string idPuntoVendita { get; set; }
        [DataMember]
        public string descrizPuntoVendita { get; set; }
        [DataMember]
        public short numGiorniEliminaFoto { get; set; }
        [DataMember]
        public string varie { get; set; }
    }
    
}
