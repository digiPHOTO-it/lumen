<%@ Page Language="C#" 
    AutoEventWireup="true" 
    CodeBehind="FotoExplorer.aspx.cs" 
    Inherits="Digiphoto.Lumen.SelfService.WebUI.FotoExplorer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>digiPHOTO - Foto Explorer</title>
    <link rel="stylesheet" href="css/fotoexplorer.css" type="text/css" media="screen" />
</head>
<body>
    

    <form id="form1" runat="server">

    <div id="pulsantiera">
        <asp:Button ID="Home"           CssClass="bottone-spostamento" runat="server" OnClick="buttonHome_Click"     Text="Home" />
        <asp:Button ID="buttonPrev"     CssClass="bottone-spostamento" runat="server" OnClick="buttonPrev_Click"     Text="&lt;- Indietro" Enabled="<%# possoAndareIndietro %>" />
        <asp:Button ID="buttonAutoPlay" CssClass="bottone-spostamento" runat="server" OnClick="buttonAutoPlay_Click" Text='<%# autoPlay ? "Pause" : "Auto Play" %>' Enabled="<%# possoAutoPlay %>" />
        <asp:Button ID="buttonNext"     CssClass="bottone-spostamento" runat="server" OnClick="buttonNext_Click"     Text="Avanti -&gt;"   Enabled="<%# possoAndareAvanti %>" />

        <asp:Label ID="Label2" runat="server" Text="Pagina" />
        <asp:Label ID="Label1" runat="server" Text="<%# paramRicerca.numPagina %>" />

        <asp:Label ID="Label3" runat="server" Text="Num foto" />
        <asp:Label ID="Label4" runat="server" Text="<%# fotoCorrente == null ? ' ' : fotoCorrente.numero %>" />

        <asp:Label ID="Label5" runat="server" Text="Oper=" />
        <asp:Label ID="Label6" runat="server" Text="<%# fotoCorrente == null ? null : fotoCorrente.nomeFotografo %>" />

        <asp:Label ID="Label7" runat="server" Text="Giorno=" />
        <asp:Label ID="Label8" runat="server" Text="<%# fotoCorrente == null ? null : fotoCorrente.giornata.ToShortDateString() %>" />

    </div>


    <div id="image-container" >
        <asp:Image ID="imageFotografia" runat="server" ImageUrl="<%# urlImmagineCorrente %>"  />
    </div>


    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <asp:Timer ID="timerAutoPlay" runat="server" OnTick="timerAutoPlay_Tick" 
        Interval="2000"
        Enabled="<%# autoPlay %>" />    



    </form>
</body>
</html>
