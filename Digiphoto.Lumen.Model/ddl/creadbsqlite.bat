@echo off
cls

echo Attenzione: verranno ri-creati i database vuoti. Confermi ?
pause

echo ---( sqLite )---
del ..\dbVuoto.sqlite

echo Creazione tabelle...
sqlite3.exe ..\dbVuoto.sqlite < ddl-create-sqLite.sql
IF ERRORLEVEL 1 GOTO Errore

echo Creazione indici...
sqlite3.exe ..\dbVuoto.sqlite < ddl-indici-sqLite.sql
IF ERRORLEVEL 1 GOTO Errore

rem =============================================================
rem =============================================================

echo ---( Sql Compact )---
del ..\dbVuoto.sdf

echo Creazione database ...
SqlCeCmd40.exe -d "Data Source=..\dbVuoto.sdf" -e create 
IF ERRORLEVEL 1 GOTO Errore

echo Creazione tabelle...
SqlCeCmd40.exe -d "Data Source=..\dbVuoto.sdf" -i ddl-create-sqlCE.sql -n
IF ERRORLEVEL 1 GOTO Errore

echo "Finito tutto ok"

goto fine


:Errore

echo *********************************
echo           ERRORE !!!!!
echo     qualcosa è andato storto
echo *********************************


:fine