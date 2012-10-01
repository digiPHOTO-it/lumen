﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;

namespace Digiphoto.Lumen.Model
{
    public partial class LumenEntities : DbContext
    {
        public LumenEntities()
            : this(false) { }
    
        public LumenEntities(bool proxyCreationEnabled)	    
            : base("name=LumenEntities")
        {
            this.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
        }
    
        public LumenEntities(string connectionString)
          : this(connectionString, false) { }
    
        public LumenEntities(string connectionString, bool proxyCreationEnabled)
            : base(connectionString)
        {
            this.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
        }	
    
        public ObjectContext ObjectContext
        {
          get { return ((IObjectContextAdapter)this).ObjectContext; }
        }
    
    	protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    	
        public DbSet<Fotografo> Fotografi { get; set; }
        public DbSet<Fotografia> Fotografie { get; set; }
        public DbSet<Evento> Eventi { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<RigaAlbum> RigheAlbum { get; set; }
        public DbSet<ScaricoCard> ScarichiCards { get; set; }
        public DbSet<FormatoCarta> FormatiCarta { get; set; }
        public DbSet<InfoFissa> InfosFisse { get; set; }
        public DbSet<Carrello> Carrelli { get; set; }
        public DbSet<RigaCarrello> RigheCarrelli { get; set; }
        public DbSet<ConsumoCartaGiornaliero> ConsumiCartaGiornalieri { get; set; }
        public DbSet<Giornata> Giornate { get; set; }
    }
}
