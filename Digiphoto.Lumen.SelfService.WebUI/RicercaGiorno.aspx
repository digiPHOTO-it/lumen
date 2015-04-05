<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RicercaGiorno.aspx.cs" Inherits="Digiphoto.Lumen.SelfService.WebUI.RicercaOrario" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>digiPHOTO - Self Service</title>
    <link rel="stylesheet" media="all"  href="css/common.css" type="text/css"  />
    <link rel="stylesheet" media="all and (orientation:portrait)" href="css/portrait.css" />
    <link rel="stylesheet" media="all and (orientation:landscape)" href="css/landscape.css" /> </head>

<body id="pag-scelta-giorno">

    <form id="formGiorno" runat="server">
    <div>
        
        <div class="intestazione">
            <asp:Label ID="Label1" runat="server" Text="<%$ Resources:LocalizedText, TitoloBanner3 %>" /> (3/3)
        </div>  
    
        <div class="spiegazione">
            <asp:Label ID="Label2" runat="server" Text="<%$ Resources:LocalizedText, Spiegazione3 %>" />
        </div>

        <div id="corpo">

            <div id="quale-giorno">
                <ul id="lista-giorni">
                    <li class="giorno-item"><asp:Button ID="ButtonOggi"       runat="server" Text="<%$ Resources:LocalizedText, Oggi %>"      CssClass="btn-ric-giorno" OnClick="ButtonGiorno_Click" CommandArgument="0" /></li>
                    <li class="giorno-item"><asp:Button ID="ButtonIeri"       runat="server" Text="<%$ Resources:LocalizedText, Ieri %>"      CssClass="btn-ric-giorno" OnClick="ButtonGiorno_Click" CommandArgument="1" /></li>
                    <li class="giorno-item"><asp:Button ID="ButtonIeriLaltro" runat="server" Text="<%$ Resources:LocalizedText, AltroIeri %>" CssClass="btn-ric-giorno" OnClick="ButtonGiorno_Click" CommandArgument="2" /></li>            
                </ul>
            </div>

            <div id="scegli-giorno">
                <asp:Label ID="Label3" runat="server" Text="<%$ Resources:LocalizedText, SelezionaGiornoDalCalendario %>" />
                <asp:Calendar runat="server" ID="GiornoIniz"  BackColor="#99ccff" SelectedDayStyle-BorderColor="red" SelectedDayStyle-BackColor="Yellow" TodayDayStyle-BackColor="LightCoral" />
            </div>
            
            <div style="clear: both" />              
        </div>

        <div class="navigazione">
            <asp:LinkButton CssClass="button-link" ID="linkButtonNavIndietro" runat="server" Text="<%$ Resources:LocalizedText, Indietro %>" OnClick="linkButtonNavIndietro_Click"  />
            <asp:LinkButton CssClass="button-link" ID="linkButtonNavAvanti" runat="server" Text="<%$ Resources:LocalizedText, Avanti %>" OnClick="linkButtonNavAvanti_Click" />
        </div>

    </div>
    </form>

    <script>
        function svuotaGiorno() {
//            document.forms['formGiorno'].elements['GiornoIniz'].value = "";
        }
    </script>

</body>
</html>
