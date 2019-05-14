CREATE TABLE IF NOT EXISTS `lumen`.`Ospiti` (
  `id` INT(11) NOT NULL,
  `nome` VARCHAR(50) NULL DEFAULT NULL,
  `impronta` BLOB NOT NULL,
  `ora` DATETIME not null default CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
;

alter table `lumen`.`InfosFisse` 
add scannerImpronte varchar(20) null
;

update `lumen`.`InfosFisse` set versioneDbCompatibile = '5'
;