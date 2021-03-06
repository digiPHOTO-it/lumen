﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Digiphoto.Lumen.Model {
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Infrastructure;

	public partial class LumenEntities : DbContext {
		// PERSONALIZZAZIONE TEMPLATE LUCA
		public LumenEntities( String mioNome ) : base( mioNome ) {
		}

		public LumenEntities()
			: base( "name=LumenEntities" ) {
		}

		protected override void OnModelCreating( DbModelBuilder modelBuilder ) {

			// Entità derivate
			modelBuilder.Entity<PromoStessaFotoSuFile>();
			modelBuilder.Entity<PromoProdXProd>();
			modelBuilder.Entity<PromoProdXProd>();
			modelBuilder.Entity<ProdottoFile>();
			modelBuilder.Entity<FormatoCarta>();
		}
	

		public virtual DbSet<AzioneAuto> AzioniAutomatiche { get; set; }
		public virtual DbSet<Carrello> Carrelli { get; set; }
		public virtual DbSet<ConsumoCartaGiornaliero> ConsumiCartaGiornalieri { get; set; }
		public virtual DbSet<Evento> Eventi { get; set; }

		public virtual DbSet<Fotografo> Fotografi { get; set; }
		public virtual DbSet<Fotografia> Fotografie { get; set; }
		public virtual DbSet<Giornata> Giornate { get; set; }
		public virtual DbSet<IncassoFotografo> IncassiFotografi { get; set; }
		public virtual DbSet<InfoFissa> InfosFisse { get; set; }
		public virtual DbSet<RigaCarrello> RigheCarrelli { get; set; }
		public virtual DbSet<ScaricoCard> ScarichiCards { get; set; }
		public virtual DbSet<Promozione> Promozioni { get; set; }

		public virtual DbSet<FormatoCarta> FormatiCarta { get; set; }
		public virtual DbSet<ProdottoFile> ProdottiFile { get; set; }
		public virtual DbSet<Prodotto> Prodotti { get; set; }
		public virtual DbSet<Ospite> Ospiti { get; set;	}

	}
}
