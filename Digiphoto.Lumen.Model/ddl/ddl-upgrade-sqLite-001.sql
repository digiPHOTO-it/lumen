ALTER TABLE "main"."RigheCarrelli" RENAME TO "oXHFcGcd04oXHFcGcd04_RigheCarrelli"
;

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
    		
			)
;

INSERT INTO "main"."RigheCarrelli" 
SELECT *
FROM "main"."oXHFcGcd04oXHFcGcd04_RigheCarrelli"
;

DROP TABLE "main"."oXHFcGcd04oXHFcGcd04_RigheCarrelli"
;
