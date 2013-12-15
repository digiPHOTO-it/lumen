
-- --------------------------------------------------
-- Date Created: 06/21/2012 00:32:23
-- compatible SQLite
-- Generated from EDMX file: C:\Users\bluca\Documents\Visual Studio 2010\Projects\lumen\Digiphoto.Lumen.Model\LumenModel.edmx
-- --------------------------------------------------
-- Dropping existing indexes
-- --------------------------------------------------

	DROP INDEX if exists [oiniziali_idx];

	DROP INDEX if exists [cgiornata_idx];

	DROP INDEX if exists [rfsfotografo_idx];

	DROP INDEX if exists [fggfase_idx];

	DROP INDEX if exists [ffotografo_idx];

	DROP INDEX if exists [fevento_idx];

	DROP INDEX if exists [fnumerof_idx];

-- --------------------------------------------------
-- Creating all indexes
-- --------------------------------------------------

	create unique index [oiniziali_idx] on Fotografi (iniziali);

	create index [cgiornata_idx] on Carrelli (giornata);

	create index [rfsfotografo_idx] on RigheCarrelli (fotografo_id);

	create index [fggfase_idx] on Fotografie (giornata, faseDelGiorno);
	
	create index [ffotografo_idx] on Fotografie (fotografo_id);

	create index [fevento_idx] on Fotografie (evento_id);

	create index [fnumerof_idx] on Fotografie (numero);

	create unique index [cggcarta_idx] on ConsumiCartaGiornalieri (giornata,formatoCarta_id);

	-- Descrizione INCASSIFOTOGRAFI_IDX2
	CREATE UNIQUE INDEX "incassifotografi_idx2" on incassifotografi ([CarrelloIncassoFotografo_IncassoFotografo_id] ASC, fotografo_id ASC);

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------