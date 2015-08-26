
-- UPGRADE 1.2

CREATE TABLE [AzioniAutomatiche] (
	[id] uniqueidentifier   NOT NULL ,
	[nome] nvarchar(30)   NOT NULL ,
	[correzioniXml] nvarchar(2147483647)  not NULL ,
	[attivo] bit   DEFAULT 'True' NOT NULL ,
	PRIMARY KEY ([id])	
)