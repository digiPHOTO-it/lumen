

CREATE TABLE [Ospiti] ( 
	`id` integer NOT NULL, 
	`nome` VARCHAR(50) NULL, 
	`impronta` BLOB NOT NULL, 
	`ora` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`id`)
);

alter table [InfosFisse]
add scannerImpronte varchar(20) null
;

update [InfosFisse] set [versioneDbCompatibile] = '5'
;