ALTER TABLE Prodotti ADD CONSTRAINT prodotti_uq UNIQUE (descrizione)
;

update `lumen`.`InfosFisse` set versioneDbCompatibile = '6'
;