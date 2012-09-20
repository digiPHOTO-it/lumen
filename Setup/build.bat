call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

rem scarico eventuali pacchetti necessari
..\.nuget\nuget install packages.config -o ..\packages


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