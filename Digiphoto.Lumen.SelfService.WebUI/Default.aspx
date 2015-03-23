<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Digiphoto.Lumen.SelfService.WebUI.Index" culture="auto" meta:resourcekey="PageResource1" uiculture="auto" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="head" runat="server">
    <title>digiPHOTO - Self Service</title>
    <link rel="stylesheet" href="css/fotoexplorer.css" type="text/css" media="screen" />
</head>
<body>
    
    <form id="form1" runat="server">

    <div>
        <div class="intestazione">Self Service - Home Page (1/3)</div>    

        <div id="language-selector">

            <ul>
                <li>
                    <asp:ImageButton ID="ImageButtonFlagIT" runat="server" ImageUrl="~/images/Flag_IT.png" OnClick="ImageButtonFlag_Click" />
                </li>
                <li>
                    <asp:ImageButton ID="ImageButtonFlagEN" runat="server" ImageUrl="~/images/Flag_EN.png" OnClick="ImageButtonFlag_Click"  />
                </li>
            </ul>
            
        </div>

        <div class="spiegazione">
            <asp:Label ID="LabelSpiagazioneHome" runat="server" Text="<%$ Resources:LocalizedText, LabelSpiagazioneHome %>" />
        </div>
        
        <div class="navigazione">
            <asp:LinkButton CssClass="nav-avanti" ID="linkButtonNavAvanti" runat="server" Text="<%$ Resources:LocalizedText, Avanti %>" OnClick="linkButtonNavAvanti_Click" />
        </div>


    </div>
    </form>
</body>
</html>
