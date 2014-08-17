<%@ Page Language="C#" 
    AutoEventWireup="true" 
    CodeBehind="FotoExplorer.aspx.cs" 
    Inherits="Digiphoto.Lumen.SelfService.WebUI.FotoExplorer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Lumen - SelfService</title>
    <link rel="stylesheet" href="css/fotoexplorer.css" type="text/css" media="screen" />
</head>
<body>
    <form id="form1" runat="server">

    <div id="pulsantiera">
        <asp:Button ID="buttonPrev" CssClass="bottone-spostamento" runat="server" OnClick="buttonPrev_Click" Text="&lt;- Indietro" />
        <asp:Button ID="buttonNext" CssClass="bottone-spostamento" runat="server" OnClick="buttonNext_Click" Text="Avanti -&gt;" />

        <asp:Label ID="Label2" runat="server" Text="Pagina" />
        <asp:Label ID="Label1" runat="server" Text="<%# numPagina %>" />

        <asp:Label ID="Label3" runat="server" Text="Num foto" />
        <asp:Label ID="Label4" runat="server" Text="<%# fotoCorrente == null ? ' ' : fotoCorrente.numero %>" />

    </div>


    <div id="image-container" >
        <asp:Image ID="imageFotografia" runat="server" ImageUrl="<%# urlImmagineCorrente %>"  />
    </div>

    </form>
</body>
</html>
