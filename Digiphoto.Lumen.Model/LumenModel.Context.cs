﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class LumenEntities : DbContext
    {

        public LumenEntities()
            : base("name=LumenEntities")
        {
        }

		public LumenEntities( string nameOrConnectionString )
			: base( nameOrConnectionString ) {
		}
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<InfoFissa> InfosFisse { get; set; }
        public DbSet<Evento> Eventi { get; set; }
        public DbSet<Fotografia> Fotografie { get; set; }
        public DbSet<Fotografo> Fotografi { get; set; }
        public DbSet<Giornata> Giornate { get; set; }
        public DbSet<ScaricoCard> ScarichiCards { get; set; }
        public DbSet<FormatoCarta> FormatiCarta { get; set; }
        public DbSet<ConsumoCartaGiornaliero> ConsumiCartaGiornalieri { get; set; }
        public DbSet<Carrello> Carrelli { get; set; }
        public DbSet<RigaCarrello> RigheCarrelli { get; set; }
        public DbSet<IncassoFotografo> IncassiFotografi { get; set; }
    }
}
