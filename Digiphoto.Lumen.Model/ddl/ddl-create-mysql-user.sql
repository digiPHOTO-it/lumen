-- Attenzione: le righe devono terminare con
--             punto e virgola CR LF (tutti attaccati)


CREATE USER 'root'@'%' IDENTIFIED by '';

GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;


CREATE USER 'fotografo'@'%' IDENTIFIED by 'fotografo';

GRANT SELECT, INSERT, UPDATE, DELETE ON lumen.* TO 'fotografo'@'%';

FLUSH PRIVILEGES;


