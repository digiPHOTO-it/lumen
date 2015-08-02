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
    
    public partial class Fotografia
    {
        public Fotografia()
        {
            this.contaStampata = 0;
            this.contaMasterizzata = 0;
        }
    
        public System.Guid id { get; set; }
        public string nomeFile { get; set; }
        public Nullable<System.DateTime> dataOraScatto { get; set; }
		
		private string _didascalia;
		public string didascalia 
		{
			get
			{
				return _didascalia;
			}
			set
			{
				if (value != null)
				{
					_didascalia = value;
					OnPropertyChanged("didascalia");
				}
			}
		}
		
        public System.DateTime dataOraAcquisizione { get; set; }
        public int numero { get; set; }
        public Nullable<short> faseDelGiorno { get; set; }
        public System.DateTime giornata { get; set; }
        public string correzioniXml { get; set; }
        public short contaStampata { get; set; }
        public short contaMasterizzata { get; set; }
    
        public virtual Evento evento { get; set; }
        public virtual Fotografo fotografo { get; set; }
    }
}
