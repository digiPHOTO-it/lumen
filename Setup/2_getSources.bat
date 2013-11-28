echo off
set ERRORLEVEL=0

set repo=https://digiPHOTO@bitbucket.org/digiPHOTO/lumen


if exist lumen\NUL goto Aggiorna


hg clone %repo%
if %ERRORLEVEL% neq 0 goto GesErrore
goto Fine


rem ------------------
:Aggiorna
cd Lumen
hg pull %repo%
if %ERRORLEVEL% neq 0 goto GesErrore
hg update -C
if %ERRORLEVEL% neq 0 goto GesErrore
goto Fine
rem ------------------




:GesErrore
echo Errore impossibile procedere


:Fine
pause