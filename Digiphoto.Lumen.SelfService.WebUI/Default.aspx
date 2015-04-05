<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Digiphoto.Lumen.SelfService.WebUI.Index" culture="auto" meta:resourcekey="PageResource1" uiculture="auto" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="head" runat="server">
    <title>digiPHOTO - Self Service</title>
    <link rel="stylesheet" media="all"  href="css/common.css" type="text/css"  />
    <link rel="stylesheet" media="all and (orientation:portrait)" href="css/portrait.css" />
    <link rel="stylesheet" media="all and (orientation:landscape)" href="css/landscape.css" /> 
</head>
<body id="home">
    
    <form id="form1" runat="server">

    <div>
        <div class="intestazione">
            <asp:Label runat="server" Text="<%$ Resources:LocalizedText, TitoloBanner1 %>" /> (1/3)
        </div>    

        <div id="language-selector">
            <ul id="lista-bandiere">
                <li>
                    <div>
                        <asp:ImageButton ID="ImageButtonFlagIT" runat="server" ImageUrl="~/images/Flag_IT-128x128.png" OnClick="ImageButtonFlag_Click" CommandArgument="it-IT" />
                    </div>
                </li>
                <li>
                    <div>
                        <asp:ImageButton ID="ImageButtonFlagEN" runat="server" ImageUrl="~/images/Flag_EN-128x128.png" OnClick="ImageButtonFlag_Click" CommandArgument="en-US" />
                    </div>
                </li>
            </ul>
            
        </div>
        <div style="clear: both" />

        <div class="spiegazione">
            <asp:Label ID="LabelSpiagazioneHome" runat="server" Text="<%$ Resources:LocalizedText, LabelSpiagazioneHome %>" />
        </div>
        
        <div class="navigazione">
            <asp:LinkButton CssClass="button-link" ID="linkButtonNavAvanti" runat="server" Text="<%$ Resources:LocalizedText, Avanti %>" OnClick="linkButtonNavAvanti_Click" />
        </div>


    </div>
    </form>
</body>
</html>
