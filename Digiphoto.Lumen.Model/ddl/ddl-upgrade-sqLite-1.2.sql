
-- UPGRADE 1.2
CREATE TABLE [AzioniAutomatiche] (
	[id] uniqueidentifier   NOT NULL ,
	[nome] nvarchar(30)   NOT NULL ,
	[correzioniXml] nvarchar(2147483647)  not NULL ,
	[attivo] bit   DEFAULT 'True' NOT NULL ,
	PRIMARY KEY ([id])	
)



-- TODO eventuali altre operazioni di upgrade

-- TODO






-- Alla fine aggiorno la versione di compatibilità del database
update InfosFisse
set versioneDbCompatibile = "1.1"
;

commit
;