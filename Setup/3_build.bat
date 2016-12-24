@echo off
rem call "C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
rem call "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\VsDevCmd.bat"

call "C:\Program Files\Microsoft Visual Studio 14.0\Common7\Tools\VsMSBuildCmd.bat"



set EnableNuGetPackageRestore=true

rem scarico eventuali pacchetti necessari
..\.nuget\nuget install packages.config -o ..\packages


set GIORNALE=%TEMP%\build.out.txt
msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform="Any CPU" /target:Clean
echo clean completato. Inizio la compilazione
msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform="Any CPU" > %GIORNALE%
IF %ERRORLEVEL% NEQ 0 goto GesErrore
echo build OK: verificare il log %GIORNALE%
pause


echo tutto ok
goto Fine


:GesErrore
echo Ci sono stati degli errori
pause
notepad %GIORNALE%
exit 1


:Fine
pause