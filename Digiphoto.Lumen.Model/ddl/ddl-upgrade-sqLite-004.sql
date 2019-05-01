CREATE TABLE [Ospiti] ( 
	`id` integer NOT NULL, 
	`nome` VARCHAR(50) NULL, 
	`impronta` BLOB NOT NULL, 
	PRIMARY KEY (`id`)
);

alter table [InfosFisse]
add scannerImpronte varchar(20) null
;