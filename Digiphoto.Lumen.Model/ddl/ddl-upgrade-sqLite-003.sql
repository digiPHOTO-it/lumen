ALTER TABLE [Fotografie] ADD COLUMN [miPiace]  bit   NULL 
;

alter table [Carrelli] add column [visibileSelfService] bit DEFAULT 1 NOT NULL 
;

update [InfosFisse] set [versioneDbCompatibile] = '1.3'
;
