rem call "C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
call "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\VsDevCmd.bat"


rem scarico eventuali pacchetti necessari
echo "disabilito nuget perche' necessario intervento manuale"
rem ..\.nuget\nuget install packages.config -o ..\packages


msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform=x86
IF %ERRORLEVEL% NEQ 0 goto sub_problema
pause

msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release /property:Platform=x64
IF %ERRORLEVEL% NEQ 0 goto GesErrore


echo tutto ok
goto Fine


:GesErrore
echo Ci sono stati degli errori
pause
exit 1



:Fine
pause