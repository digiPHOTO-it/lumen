<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RicercaFotografo.aspx.cs" Inherits="Digiphoto.Lumen.SelfService.WebUI.RicercaFotografo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>digiPHOTO - Self Service</title>
    <link rel="stylesheet" media="all"  href="css/common.css" type="text/css"  />
    <link rel="stylesheet" media="all and (orientation:portrait)" href="css/portrait.css" />
    <link rel="stylesheet" media="all and (orientation:landscape)" href="css/landscape.css" /> 
</head>

<body id="pag-scelta-fotografo">
    <form id="form2" runat="server">
    <div>
        <div class="intestazione">
            <asp:Label runat="server" Text="<%$ Resources:LocalizedText, TitoloBanner2 %>" /> (2/3)
        </div>    
    
        <div class="spiegazione">
            <asp:Label runat="server" Text="<%$ Resources:LocalizedText, Spiegazione2 %>" /> 
        </div>

        <asp:ListView runat="server" ID="ListViewFotografi" DataSource="<%# listaFotografiDto %>">
            <ItemTemplate>
                <li runat="server" class="elem">
                    <div runat="server" class="diapo">
                        <div runat="server" id="fotoImg">
                            <asp:ImageButton ID="image" runat="server"  ImageUrl='<%# getImage( Eval("id").ToString() ) %>' CssClass="img-fotografo" OnClick="ImageFotografo_Click" CommandArgument='<%# Eval("id").ToString() %>' />
                        </div>
                        <div runat="server" id="fotoNome">
                            <asp:Label ID="nome" runat="server" Text='<%#Eval("cognomeNome") %>' CssClass="nome-fotografo" />
                        </div>
                        <div style="clear: both" runat="server"/>
                    </div>                    
                </li>
            </ItemTemplate>

            <LayoutTemplate>
                <div runat="server" id="div">
                    <ul runat="server" id="ul">
                        <div id="ItemPlaceholder" runat="server" />
                    </ul>
                </div>
            </LayoutTemplate>

        </asp:ListView>
            
        <div style="clear: both" />              

        <div class="navigazione">
            <asp:LinkButton CssClass="button-link" ID="linkButtonNavIndietro" runat="server" Text="<%$ Resources:LocalizedText, Indietro %>" OnClick="linkButtonNavIndietro_Click"  />
            <asp:LinkButton CssClass="button-link" ID="linkButtonNavAvanti" runat="server" Text="<%$ Resources:LocalizedText, Avanti %>" OnClick="linkButtonNavAvanti_Click" />
        </div>

    </div>
    </form>
</body>
</html>
