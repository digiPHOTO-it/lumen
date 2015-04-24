@echo off
rem call "C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
call "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\VsDevCmd.bat"

set EnableNuGetPackageRestore=true

rem scarico eventuali pacchetti necessari
echo "disabilito nuget perche' necessario intervento manuale"
rem ..\.nuget\nuget install packages.config -o ..\packages


set GIORNALE=%TEMP%\build.out.txt
rmdir /S ..\Digiphoto.Lumen.SelfService.WebUI\ssWebPackage
msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform="Any CPU" /target:Clean
msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform="Any CPU" > %GIORNALE%
IF %ERRORLEVEL% NEQ 0 goto sub_problema
echo build OK: verificare il log %GIORNALE%
pause


echo tutto ok
goto Fine


:GesErrore
echo Ci sono stati degli errori
pause
exit 1


:Fine
pause