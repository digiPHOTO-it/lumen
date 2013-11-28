
call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

copy ..\packages\System.Data.SQLite.x64.1.0.81.0\lib\net40\*.dll ..\Digiphoto.Lumen.Core.VsTest\bin\Release


cd ..\Digiphoto.Lumen.Core.VsTest\bin\Release

rem mstest /testcontainer:Digiphoto.Lumen.Core.VsTest.dll /test:testApriChiudi
mstest /testcontainer:Digiphoto.Lumen.Core.VsTest.dll 

pause