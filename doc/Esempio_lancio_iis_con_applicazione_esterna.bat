rem -- lancio iis con l'applicazione selfservice di lumen
rem -- occhio che il parametro path vuole i doppi apici solo all'inizio e non anche alla fine :-(

-- Questo va lanciato come amministratore
netsh http add urlacl url=http://*:8080/ user=everyone

"C:\Program Files (x86)\IIS Express\iisexpress.exe" -path:"C:\Users\bluca\Documents\Visual Studio 2015\Projects\lumen\Digiphoto.Lumen.SelfService.WebUI\bin\Release\Publish\