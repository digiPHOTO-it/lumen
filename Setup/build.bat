
rem scarico eventuali pacchetti necessari
..\.nuget\nuget install packages.config -o ..\packages


msbuild ..\Digiphoto.Lumen.sln /property:Configuration=Release