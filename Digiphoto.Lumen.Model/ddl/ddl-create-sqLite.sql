
-- --------------------------------------------------
-- Date Created: 12/16/2013 07:38:19
-- compatible SQLite
-- Generated from EDMX file: C:\Users\bluca\Documents\Visual Studio 2012\Projects\lumen\Digiphoto.Lumen.Model\LumenModel.edmx
-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

    
	DROP TABLE if exists [InfosFisse];
    
	DROP TABLE if exists [Eventi];
    
	DROP TABLE if exists [Fotografie];
    
	DROP TABLE if exists [Fotografi];
    
	DROP TABLE if exists [Giornate];
    
	DROP TABLE if exists [ScarichiCards];
    
	DROP TABLE if exists [FormatiCarta];
    
	DROP TABLE if exists [ConsumiCartaGiornalieri];
    
	DROP TABLE if exists [Carrelli];
    
	DROP TABLE if exists [RigheCarrelli];
    
	DROP TABLE if exists [IncassiFotografi];

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'InfosFisse'
CREATE TABLE [InfosFisse] (
    [id] nvarchar(1)   DEFAULT 'K' NOT NULL ,
    [ultimoNumFotogramma] integer   NOT NULL ,
    [dataUltimoScarico] datetime   NULL ,
    [versioneDbCompatibile] nvarchar(10)   DEFAULT '1.0' NOT NULL ,
    [modoNumerazFoto] nvarchar(1)   DEFAULT 'M' NOT NULL ,
    [pixelProvino] smallint   NOT NULL ,
    [idPuntoVendita] nvarchar(5)   NULL ,
    [descrizPuntoVendita] nvarchar(50)   NULL ,
    [numGiorniEliminaFoto] smallint   NOT NULL ,
    [varie] nvarchar(200)   NULL 
 , PRIMARY KEY ([id])	
		);

-- Creating table 'Eventi'
CREATE TABLE [Eventi] (
    [id] uniqueidentifier   NOT NULL ,
    [descrizione] nvarchar(50)   NOT NULL ,
    [attivo] bit   DEFAULT 'True' NOT NULL ,
    [ordinamento] smallint   NULL 
 , PRIMARY KEY ([id])	
		);

-- Creating table 'Fotografie'
CREATE TABLE [Fotografie] (
    [id] uniqueidentifier   NOT NULL ,
    [nomeFile] nvarchar(100)   NOT NULL ,
    [dataOraScatto] datetime   NULL ,
    [didascalia] nvarchar(99)   NULL ,
    [dataOraAcquisizione] datetime   NOT NULL ,
    [numero] integer   NOT NULL ,
    [faseDelGiorno] smallint   NULL ,
    [giornata] datetime   NOT NULL ,
    [correzioniXml] nvarchar(2147483647)   NULL ,
    [contaStampata] smallint   DEFAULT '0' NOT NULL ,
    [contaMasterizzata] smallint   DEFAULT '0' NOT NULL ,
    [evento_id] uniqueidentifier   NULL ,
    [fotografo_id] nvarchar(16)   NOT NULL 
 , PRIMARY KEY ([id])	
					
		,CONSTRAINT [FK_EventoFotografia]
    		FOREIGN KEY ([evento_id])
    		REFERENCES [Eventi] ([id])					
    		
						
		,CONSTRAINT [FK_FotografoFotografia]
    		FOREIGN KEY ([fotografo_id])
    		REFERENCES [Fotografi] ([id])					
    		
			);

-- Creating table 'Fotografi'
CREATE TABLE [Fotografi] (
    [id] nvarchar(16)   NOT NULL ,
    [cognomeNome] nvarchar(50)   NOT NULL ,
    [iniziali] nvarchar(2)   NOT NULL ,
    [attivo] bit   DEFAULT 'True' NOT NULL ,
    [umano] bit   DEFAULT 'True' NOT NULL ,
    [note] nvarchar(2147483647)   NULL 
 , PRIMARY KEY ([id])	
		);

-- Creating table 'Giornate'
CREATE TABLE [Giornate] (
    [id] datetime   NOT NULL ,
    [orologio] datetime   NOT NULL ,
    [incassoDichiarato] decimal(7,2)   NOT NULL ,
    [note] nvarchar(2147483647)   NULL ,
    [incassoPrevisto] decimal(7,2)   NOT NULL ,
    [prgTaglierina1] nvarchar(20)   NULL ,
    [prgTaglierina2] nvarchar(20)   NULL ,
    [prgTaglierina3] nvarchar(20)   NULL ,
    [totScarti] smallint   NULL ,
    [firma] nvarchar(50)   NOT NULL 
 , PRIMARY KEY ([id])	
		);

-- Creating table 'ScarichiCards'
CREATE TABLE [ScarichiCards] (
    [id] uniqueidentifier   NOT NULL ,
    [tempo] datetime   NOT NULL ,
    [totFoto] smallint   NOT NULL ,
    [giornata] datetime   NOT NULL ,
    [fotografo_id] nvarchar(16)   NOT NULL 
 , PRIMARY KEY ([id])	
					
		,CONSTRAINT [FK_FotografoScaricoCard]
    		FOREIGN KEY ([fotografo_id])
    		REFERENCES [Fotografi] ([id])					
    		
			);

-- Creating table 'FormatiCarta'
CREATE TABLE [FormatiCarta] (
    [id] uniqueidentifier   NOT NULL ,
    [descrizione] nvarchar(50)   NOT NULL ,
    [prezzo] decimal(6,2)   NOT NULL ,
    [attivo] bit   DEFAULT 'True' NOT NULL ,
    [ordinamento] smallint   NULL 
 , PRIMARY KEY ([id])	
		);

-- Creating table 'ConsumiCartaGiornalieri'
CREATE TABLE [ConsumiCartaGiornalieri] (
    [id] uniqueidentifier   NOT NULL ,
    [giornata] datetime   NOT NULL ,
    [totFogli] smallint   NOT NULL ,
    [diCuiProvini] smallint   NOT NULL ,
    [diCuiFoto] smallint   NOT NULL ,
    [formatoCarta_id] uniqueidentifier   NOT NULL 
 , PRIMARY KEY ([id])	
					
		,CONSTRAINT [FK_FormatoCartaConsumoCartaGiornaliero]
    		FOREIGN KEY ([formatoCarta_id])
    		REFERENCES [FormatiCarta] ([id])					
    		
			);

-- Creating table 'Carrelli'
CREATE TABLE [Carrelli] (
    [id] uniqueidentifier   NOT NULL ,
    [giornata] datetime   NOT NULL ,
    [tempo] datetime   NOT NULL ,
    [totaleAPagare] decimal(6,2)   NOT NULL ,
    [intestazione] nvarchar(100)   NULL ,
    [venduto] bit   DEFAULT 'False' NOT NULL ,
    [note] nvarchar(2147483647)   NULL ,
    [totMasterizzate] smallint   DEFAULT '0' NOT NULL ,
    [prezzoDischetto] decimal(6,2)   NULL 
 , PRIMARY KEY ([id])	
		);

-- Creating table 'RigheCarrelli'
CREATE TABLE [RigheCarrelli] (
    [id] uniqueidentifier   NOT NULL ,
    [prezzoLordoUnitario] decimal(6,2)   NOT NULL ,
    [quantita] smallint   NOT NULL ,
    [prezzoNettoTotale] decimal(6,2)   NOT NULL ,
    [sconto] decimal(6,2)   NULL ,
    [descrizione] nvarchar(99)   NOT NULL ,
    [discriminator] nvarchar(1)   NOT NULL ,
    [totFogliStampati] smallint   NULL ,
    [nomeStampante] nvarchar(255)   NULL ,
    [bordiBianchi] bit   NULL ,
    [carrello_id] uniqueidentifier   NOT NULL ,
    [fotografo_id] nvarchar(16)   NOT NULL ,
    [fotografia_id] uniqueidentifier   NULL ,
    [formatoCarta_id] uniqueidentifier   NULL 
 , PRIMARY KEY ([id])	
					
		,CONSTRAINT [FK_CarrelloRigaCarrello]
    		FOREIGN KEY ([carrello_id])
    		REFERENCES [Carrelli] ([id])					
    		ON DELETE CASCADE
						
		,CONSTRAINT [FK_FotografoRigaCarrello]
    		FOREIGN KEY ([fotografo_id])
    		REFERENCES [Fotografi] ([id])					
    		
						
		,CONSTRAINT [FK_FotografiaRigaCarrello]
    		FOREIGN KEY ([fotografia_id])
    		REFERENCES [Fotografie] ([id])	
			ON DELETE SET NULL				
    		
						
		,CONSTRAINT [FK_FormatoCartaRigaCarrello]
    		FOREIGN KEY ([formatoCarta_id])
    		REFERENCES [FormatiCarta] ([id])					
    		
			);

-- Creating table 'IncassiFotografi'
CREATE TABLE [IncassiFotografi] (
    [id] uniqueidentifier   NOT NULL ,
    [incasso] decimal(6,2)   NOT NULL ,
    [incassoStampe] decimal(6,2)   NOT NULL ,
    [incassoMasterizzate] decimal(6,2)   NOT NULL ,
    [contaStampe] smallint   NOT NULL ,
    [contaMasterizzate] smallint   NOT NULL ,
    [provvigioni] decimal(6,2)   NULL ,
    [carrello_id] uniqueidentifier   NOT NULL ,
    [fotografo_id] nvarchar(16)   NOT NULL 
 , PRIMARY KEY ([id])	
					
		,CONSTRAINT [FK_CarrelloIncassoFotografo]
    		FOREIGN KEY ([carrello_id])
    		REFERENCES [Carrelli] ([id])					
    		ON DELETE CASCADE
						
		,CONSTRAINT [FK_FotografoIncassoFotografo]
    		FOREIGN KEY ([fotografo_id])
    		REFERENCES [Fotografi] ([id])					
    		
			);


-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------