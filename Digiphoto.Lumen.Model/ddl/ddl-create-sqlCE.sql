
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server Compact Edition
-- --------------------------------------------------
-- Date Created: 06/21/2012 13:18:48
-- Generated from EDMX file: C:\Users\bluca\Documents\Visual Studio 2010\Projects\lumen\Digiphoto.Lumen.Model\LumenModel.edmx
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Fotografi'
CREATE TABLE [Fotografi] (
    [id] nvarchar(16)  NOT NULL,
    [cognomeNome] nvarchar(50)  NOT NULL,
    [iniziali] nvarchar(2)  NOT NULL,
    [attivo] bit  NOT NULL,
    [umano] bit  NOT NULL,
    [note] nvarchar(4000)  NULL
);
GO

-- Creating table 'Fotografie'
CREATE TABLE [Fotografie] (
    [id] uniqueidentifier  NOT NULL,
    [nomeFile] nvarchar(100)  NOT NULL,
    [dataOraScatto] datetime  NULL,
    [didascalia] nvarchar(4000)  NULL,
    [dataOraAcquisizione] datetime  NOT NULL,
    [numero] int  NOT NULL,
    [faseDelGiorno] smallint  NULL,
    [giornata] datetime  NOT NULL,
    [correzioniXml] nvarchar(4000)  NULL,
    [fotografo_id] nvarchar(16)  NOT NULL,
    [evento_id] uniqueidentifier  NULL
);
GO

-- Creating table 'Eventi'
CREATE TABLE [Eventi] (
    [id] uniqueidentifier  NOT NULL,
    [descrizione] nvarchar(50)  NOT NULL,
    [attivo] bit  NOT NULL,
    [ordinamento] smallint  NULL
);
GO

-- Creating table 'Albums'
CREATE TABLE [Albums] (
    [id] int IDENTITY(1,1) NOT NULL,
    [titolo] nvarchar(50)  NOT NULL,
    [note] nvarchar(4000)  NOT NULL,
    [timestamp] datetime  NOT NULL
);
GO

-- Creating table 'RigheAlbum'
CREATE TABLE [RigheAlbum] (
    [id] int IDENTITY(1,1) NOT NULL,
    [AlbumRigaAlbum_RigaAlbum_id] int  NOT NULL,
    [fotografia_id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ScarichiCards'
CREATE TABLE [ScarichiCards] (
    [id] uniqueidentifier  NOT NULL,
    [tempo] datetime  NOT NULL,
    [totFoto] smallint  NOT NULL,
    [giornata] datetime  NOT NULL,
    [fotografo_id] nvarchar(16)  NOT NULL
);
GO

-- Creating table 'FormatiCarta'
CREATE TABLE [FormatiCarta] (
    [id] uniqueidentifier  NOT NULL,
    [descrizione] nvarchar(50)  NOT NULL,
    [prezzo] decimal(18,0)  NOT NULL,
    [attivo] bit  NOT NULL,
    [ordinamento] smallint  NULL
);
GO

-- Creating table 'InfosFisse'
CREATE TABLE [InfosFisse] (
    [id] nvarchar(1)  NOT NULL,
    [ultimoNumFotogramma] int  NOT NULL,
    [dataUltimoScarico] datetime  NULL,
    [versioneDbCompatibile] nvarchar(10)  NOT NULL,
    [modoNumerazFoto] nvarchar(4000)  NOT NULL,
    [pixelProvino] smallint  NOT NULL,
    [idPuntoVendita] nvarchar(5)  NULL,
    [descrizPuntoVendita] nvarchar(50)  NULL,
    [numGiorniEliminaFoto] smallint  NOT NULL,
    [varie] nvarchar(4000)  NULL
);
GO

-- Creating table 'Carrelli'
CREATE TABLE [Carrelli] (
    [id] uniqueidentifier  NOT NULL,
    [giornata] datetime  NOT NULL,
    [tempo] datetime  NOT NULL,
    [totaleAPagare] decimal(18,0)  NOT NULL,
    [intestazione] nvarchar(100)  NULL,
    [venduto] bit  NOT NULL,
    [note] nvarchar(4000)  NULL
);
GO

-- Creating table 'RigheCarrelli'
CREATE TABLE [RigheCarrelli] (
    [id] uniqueidentifier  NOT NULL,
    [prezzoLordoUnitario] decimal(18,0)  NOT NULL,
    [quantita] smallint  NOT NULL,
    [prezzoNettoTotale] decimal(18,0)  NOT NULL,
    [sconto] decimal(18,0)  NULL,
    [descrizione] nvarchar(4000)  NOT NULL,
    [totFogliStampati] smallint  NULL,
    [idFotografia] uniqueidentifier  NULL,
    [totFotoMasterizzate] smallint  NULL,
    [__Disc__] nvarchar(4000)  NOT NULL,
    [CarrelloRigaCarrello_RigaCarrello_id] uniqueidentifier  NOT NULL,
    [formatoCarta_id] uniqueidentifier  NULL,
    [fotografo_id] nvarchar(16)  NULL,
    [fotografia_id] uniqueidentifier  NULL
);
GO



-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'Fotografi'
ALTER TABLE [Fotografi]
ADD CONSTRAINT [PK_Fotografi]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'Fotografie'
ALTER TABLE [Fotografie]
ADD CONSTRAINT [PK_Fotografie]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'Eventi'
ALTER TABLE [Eventi]
ADD CONSTRAINT [PK_Eventi]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'Albums'
ALTER TABLE [Albums]
ADD CONSTRAINT [PK_Albums]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'RigheAlbum'
ALTER TABLE [RigheAlbum]
ADD CONSTRAINT [PK_RigheAlbum]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'ScarichiCards'
ALTER TABLE [ScarichiCards]
ADD CONSTRAINT [PK_ScarichiCards]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'FormatiCarta'
ALTER TABLE [FormatiCarta]
ADD CONSTRAINT [PK_FormatiCarta]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'InfosFisse'
ALTER TABLE [InfosFisse]
ADD CONSTRAINT [PK_InfosFisse]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'Carrelli'
ALTER TABLE [Carrelli]
ADD CONSTRAINT [PK_Carrelli]
    PRIMARY KEY ([id] );
GO

-- Creating primary key on [id] in table 'RigheCarrelli'
ALTER TABLE [RigheCarrelli]
ADD CONSTRAINT [PK_RigheCarrelli]
    PRIMARY KEY ([id] );
GO



-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [fotografo_id] in table 'Fotografie'
ALTER TABLE [Fotografie]
ADD CONSTRAINT [FK_FotografoFotografia]
    FOREIGN KEY ([fotografo_id])
    REFERENCES [Fotografi]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FotografoFotografia'
CREATE INDEX [IX_FK_FotografoFotografia] ON [Fotografie] ([fotografo_id]);
GO

-- Creating foreign key on [evento_id] in table 'Fotografie'
ALTER TABLE [Fotografie]
ADD CONSTRAINT [FK_EventoFotografia]
    FOREIGN KEY ([evento_id])
    REFERENCES [Eventi]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EventoFotografia'
CREATE INDEX [IX_FK_EventoFotografia] ON [Fotografie] ([evento_id]);
GO

-- Creating foreign key on [AlbumRigaAlbum_RigaAlbum_id] in table 'RigheAlbum'
ALTER TABLE [RigheAlbum]
ADD CONSTRAINT [FK_AlbumRigaAlbum]
    FOREIGN KEY ([AlbumRigaAlbum_RigaAlbum_id])
    REFERENCES [Albums]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AlbumRigaAlbum'
CREATE INDEX [IX_FK_AlbumRigaAlbum] ON [RigheAlbum]   ([AlbumRigaAlbum_RigaAlbum_id]);
GO

-- Creating foreign key on [fotografo_id] in table 'ScarichiCards'
ALTER TABLE [ScarichiCards]
ADD CONSTRAINT [FK_FotografoScaricoCard]
    FOREIGN KEY ([fotografo_id])
    REFERENCES [Fotografi]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FotografoScaricoCard'
CREATE INDEX [IX_FK_FotografoScaricoCard]
ON [ScarichiCards]
    ([fotografo_id]);
GO

-- Creating foreign key on [CarrelloRigaCarrello_RigaCarrello_id] in table 'RigheCarrelli'
ALTER TABLE [RigheCarrelli]
ADD CONSTRAINT [FK_CarrelloRigaCarrello]
    FOREIGN KEY ([CarrelloRigaCarrello_RigaCarrello_id])
    REFERENCES [Carrelli]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CarrelloRigaCarrello'
CREATE INDEX [IX_FK_CarrelloRigaCarrello]
ON [RigheCarrelli]
    ([CarrelloRigaCarrello_RigaCarrello_id]);
GO

-- Creating foreign key on [formatoCarta_id] in table 'RigheCarrelli'
ALTER TABLE [RigheCarrelli]
ADD CONSTRAINT [FK_FormatoCartaRiCaFotoStampata]
    FOREIGN KEY ([formatoCarta_id])
    REFERENCES [FormatiCarta]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FormatoCartaRiCaFotoStampata'
CREATE INDEX [IX_FK_FormatoCartaRiCaFotoStampata]
ON [RigheCarrelli]
    ([formatoCarta_id]);
GO

-- Creating foreign key on [fotografo_id] in table 'RigheCarrelli'
ALTER TABLE [RigheCarrelli]
ADD CONSTRAINT [FK_FotografoRiCaFotoStampata]
    FOREIGN KEY ([fotografo_id])
    REFERENCES [Fotografi]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FotografoRiCaFotoStampata'
CREATE INDEX [IX_FK_FotografoRiCaFotoStampata]
ON [RigheCarrelli]
    ([fotografo_id]);
GO

-- Creating foreign key on [fotografia_id] in table 'RigheAlbum'
ALTER TABLE [RigheAlbum]
ADD CONSTRAINT [FK_FotografiaRigaAlbum]
    FOREIGN KEY ([fotografia_id])
    REFERENCES [Fotografie]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FotografiaRigaAlbum'
CREATE INDEX [IX_FK_FotografiaRigaAlbum]
ON [RigheAlbum]
    ([fotografia_id]);
GO

-- Creating foreign key on [fotografia_id] in table 'RigheCarrelli'
ALTER TABLE [RigheCarrelli]
ADD CONSTRAINT [FK_FotografiaRiCaFotoStampata]
    FOREIGN KEY ([fotografia_id])
    REFERENCES [Fotografie]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FotografiaRiCaFotoStampata'
CREATE INDEX [IX_FK_FotografiaRiCaFotoStampata]
ON [RigheCarrelli]
    ([fotografia_id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------