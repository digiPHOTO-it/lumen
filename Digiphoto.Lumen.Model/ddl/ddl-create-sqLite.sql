
-- --------------------------------------------------
-- Date Created: 06/21/2012 07:07:10
-- compatible SQLite
-- Generated from EDMX file: C:\Users\bluca\Documents\Visual Studio 2010\Projects\lumen\Digiphoto.Lumen.Model\LumenModel.edmx
-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

    
	DROP TABLE if exists [Fotografi];
    
	DROP TABLE if exists [Fotografie];
    
	DROP TABLE if exists [Eventi];
    
	DROP TABLE if exists [Albums];
    
	DROP TABLE if exists [RigheAlbum];
    
	DROP TABLE if exists [ScarichiCards];
    
	DROP TABLE if exists [FormatiCarta];
    
	DROP TABLE if exists [InfosFisse];
    
	DROP TABLE if exists [Carrelli];
    
	DROP TABLE if exists [RigheCarrelli];

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Fotografi'
CREATE TABLE [Fotografi] (
    [id] nvarchar(16)   NOT NULL ,
    [cognomeNome] nvarchar(50)   NOT NULL ,
    [iniziali] nvarchar(2)   NOT NULL ,
    [attivo] bit   DEFAULT 'True' NOT NULL ,
    [umano] bit   DEFAULT 'True' NOT NULL ,
    [note] nvarchar(2147483647)   NULL 
);

-- Creating table 'Fotografie'
CREATE TABLE [Fotografie] (
    [id] uniqueidentifier   NOT NULL ,
    [nomeFile] nvarchar(2147483647)   NOT NULL ,
    [dataOraScatto] datetime   NULL ,
    [didascalia] nvarchar(2147483647)   NULL ,
    [dataOraAcquisizione] datetime   NOT NULL ,
    [numero] integer   NOT NULL ,
    [faseDelGiorno] smallint   NULL ,
    [giornata] datetime   NOT NULL ,
    [correzioniXml] nvarchar(2147483647)   NULL ,
    [fotografo_id] nvarchar(16)   NOT NULL ,
    [evento_id] uniqueidentifier   NULL 
			
		,CONSTRAINT [FK_FotografoFotografia]
    		FOREIGN KEY ([fotografo_id])
    		REFERENCES [Fotografi] ([id])					
    		
						
		,CONSTRAINT [FK_EventoFotografia]
    		FOREIGN KEY ([evento_id])
    		REFERENCES [Eventi] ([id])					
    		
			);

-- Creating table 'Eventi'
CREATE TABLE [Eventi] (
    [id] uniqueidentifier   NOT NULL ,
    [descrizione] nvarchar(50)   NOT NULL ,
    [attivo] bit   DEFAULT 'True' NOT NULL ,
    [ordinamento] smallint   NULL 
);

-- Creating table 'Albums'
CREATE TABLE [Albums] (
    [id] integer PRIMARY KEY AUTOINCREMENT  NOT NULL ,
    [titolo] nvarchar(50)   NOT NULL ,
    [note] nvarchar(2147483647)   NOT NULL ,
    [timestamp] datetime   NOT NULL 
);

-- Creating table 'RigheAlbum'
CREATE TABLE [RigheAlbum] (
    [id] integer PRIMARY KEY AUTOINCREMENT  NOT NULL ,
    [AlbumRigaAlbum_RigaAlbum_id] integer   NOT NULL ,
    [fotografia_id] uniqueidentifier   NOT NULL 
			
		,CONSTRAINT [FK_AlbumRigaAlbum]
    		FOREIGN KEY ([AlbumRigaAlbum_RigaAlbum_id])
    		REFERENCES [Albums] ([id])					
    		ON DELETE CASCADE
						
		,CONSTRAINT [FK_FotografiaRigaAlbum]
    		FOREIGN KEY ([fotografia_id])
    		REFERENCES [Fotografie] ([id])					
    		ON DELETE CASCADE
			);

-- Creating table 'ScarichiCards'
CREATE TABLE [ScarichiCards] (
    [id] uniqueidentifier   NOT NULL ,
    [tempo] datetime   NOT NULL ,
    [totFoto] smallint   NOT NULL ,
    [giornata] datetime   NOT NULL ,
    [fotografo_id] nvarchar(16)   NOT NULL 
			
		,CONSTRAINT [FK_FotografoScaricoCard]
    		FOREIGN KEY ([fotografo_id])
    		REFERENCES [Fotografi] ([id])					
    		
			);

-- Creating table 'FormatiCarta'
CREATE TABLE [FormatiCarta] (
    [id] uniqueidentifier   NOT NULL ,
    [descrizione] nvarchar(50)   NOT NULL ,
    [prezzo] decimal(18,0)   NOT NULL ,
    [attivo] bit   DEFAULT 'True' NOT NULL ,
    [ordinamento] smallint   NULL 
);

-- Creating table 'InfosFisse'
CREATE TABLE [InfosFisse] (
    [id] nvarchar(1)   DEFAULT 'K' NOT NULL ,
    [ultimoNumFotogramma] integer   NOT NULL ,
    [dataUltimoScarico] datetime   NULL ,
    [versioneDbCompatibile] nvarchar(10)   DEFAULT '1.0' NOT NULL ,
    [modoNumerazFoto] nvarchar(2147483647)   DEFAULT 'M' NOT NULL ,
    [pixelProvino] smallint   NOT NULL ,
    [idPuntoVendita] nvarchar(5)   NULL ,
    [descrizPuntoVendita] nvarchar(50)    NULL ,
    [numGiorniEliminaFoto] smallint   NOT NULL ,
    [varie] nvarchar(2147483647)   NULL 
);

-- Creating table 'Carrelli'
CREATE TABLE [Carrelli] (
    [id] uniqueidentifier   NOT NULL ,
    [giornata] datetime   NOT NULL ,
    [tempo] datetime   NOT NULL ,
    [totaleAPagare] decimal(18,0)   NOT NULL ,
    [intestazione] nvarchar(50)   NULL ,
    [venduto] bit   DEFAULT 'False' NOT NULL ,
    [note] nvarchar(2147483647)   NULL 
);

-- Creating table 'RigheCarrelli'
CREATE TABLE [RigheCarrelli] (
    [id] uniqueidentifier   NOT NULL ,
    [prezzoLordoUnitario] decimal(18,0)   NOT NULL ,
    [quantita] smallint   NOT NULL ,
    [prezzoNettoTotale] decimal(18,0)   NOT NULL ,
    [sconto] decimal(18,0)   NULL ,
    [descrizione] nvarchar(100)   NOT NULL ,
    [totFogliStampati] smallint   NULL ,
    [idFotografia] uniqueidentifier   NULL ,
    [totFotoMasterizzate] smallint   NULL ,
    [__Disc__] nvarchar(2147483647)   NOT NULL ,
    [CarrelloRigaCarrello_RigaCarrello_id] uniqueidentifier   NOT NULL ,
    [formatoCarta_id] uniqueidentifier   NULL ,
    [fotografo_id] nvarchar(16)   NULL ,
    [fotografia_id] uniqueidentifier   NULL 
			
		,CONSTRAINT [FK_CarrelloRigaCarrello]
    		FOREIGN KEY ([CarrelloRigaCarrello_RigaCarrello_id])
    		REFERENCES [Carrelli] ([id])					
    		ON DELETE CASCADE
						
		,CONSTRAINT [FK_FormatoCartaRiCaFotoStampata]
    		FOREIGN KEY ([formatoCarta_id])
    		REFERENCES [FormatiCarta] ([id])					
    		
						
		,CONSTRAINT [FK_FotografoRiCaFotoStampata]
    		FOREIGN KEY ([fotografo_id])
    		REFERENCES [Fotografi] ([id])					
    		
						
		,CONSTRAINT [FK_FotografiaRiCaFotoStampata]
    		FOREIGN KEY ([fotografia_id])
    		REFERENCES [Fotografie] ([id])					
    		
			);


-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------