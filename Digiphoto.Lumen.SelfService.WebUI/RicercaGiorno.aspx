<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RicercaGiorno.aspx.cs" Inherits="Digiphoto.Lumen.SelfService.WebUI.RicercaOrario" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        
        <div class="intestazione">Self Service - Home Page (3/3)</div>  
    
        <div class="spiegazione">
            Indica il giorno in cui è stata scattata la foto.
            Se invece non lo sai, prosegui oltre con il tasto avanti.
        </div>

        <asp:Button ID="ButtonOggi"       runat="server" Text="Oggi"         CssClass="btn-ric-giorno" OnClick="ButtonGiorno_Click" CommandArgument="0" />
        <asp:Button ID="ButtonIeri"       runat="server" Text="Ieri"         CssClass="btn-ric-giorno" OnClick="ButtonGiorno_Click" CommandArgument="1" />
        <asp:Button ID="ButtonIeriLaltro" runat="server" Text="L'altro ieri" CssClass="btn-ric-giorno" OnClick="ButtonGiorno_Click" CommandArgument="2" />

        <div class="navigazione">
            <asp:LinkButton CssClass="nav-indietro" ID="linkButtonNavIndietro" runat="server" Text="<%$ Resources:LocalizedText, Indietro %>" OnClick="linkButtonNavIndietro_Click"  />
            <asp:LinkButton CssClass="nav-avanti" ID="linkButtonNavAvanti" runat="server" Text="<%$ Resources:LocalizedText, Avanti %>" OnClick="linkButtonNavAvanti_Click" />
        </div>
    </div>
    </form>
</body>
</html>
