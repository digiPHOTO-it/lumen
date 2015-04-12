rem call "C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
call "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\VsDevCmd.bat"

set EnableNuGetPackageRestore=true

rem scarico eventuali pacchetti necessari
echo "disabilito nuget perche' necessario intervento manuale"
rem ..\.nuget\nuget install packages.config -o ..\packages


rmdir /S ..\Digiphoto.Lumen.SelfService.WebUI\ssWebPackage
msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform="Any CPU" /target:Clean
msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform="Any CPU" > c:\tmp\build.out
IF %ERRORLEVEL% NEQ 0 goto sub_problema
pause

rem msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform=x64 /target:Clean
rem msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform=x64
IF %ERRORLEVEL% NEQ 0 goto GesErrore


echo tutto ok
goto Fine


:GesErrore
echo Ci sono stati degli errori
pause
exit 1


:Fine
pause