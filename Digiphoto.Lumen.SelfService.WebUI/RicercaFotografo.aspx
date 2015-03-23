<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RicercaFotografo.aspx.cs" Inherits="Digiphoto.Lumen.SelfService.WebUI.RicercaFotografo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div class="intestazione">Self Service - Imposta Fotografo (2/3)</div>    
    
        <div class="spiegazione">
            Se ti ricordi il fotografo che ti ha scattato le foto, clicca sulla
            sua immagine.
            Se invece non te lo ricordi prosegui oltre con il tasto avanti
        </div>

        <asp:ListView runat="server" ID="ListViewFotografi" DataSource="<%# listaFotografiDto %>">
            <ItemTemplate>
                <li runat="server" class="elem-lista-fotografi">
                    <div runat="server">

                        <asp:ImageButton ID="ImageFotografo" runat="server"  ImageUrl='<%# getImage( Eval("id").ToString() ) %>' CssClass="img-fotografo" OnClick="ImageFotografo_Click" CommandArgument='<%# Eval("id").ToString() %>' Width="80" />
                        <asp:Label ID="NomeFotografo" runat="server" Text='<%#Eval("cognomeNome") %>' CssClass="nome-fotografo" />
                    </div>                    
                </li>
            </ItemTemplate>

            <LayoutTemplate>
                <div runat="server" id="divListaFotografi">
                    <ul runat="server" id="listaFotografi">
                        <div id="ItemPlaceholder" runat="server" />
                    </ul>
                </div>
            </LayoutTemplate>

        </asp:ListView>
                          

        <div class="navigazione">
            <asp:LinkButton CssClass="nav-indietro" ID="linkButtonNavIndietro" runat="server" Text="<%$ Resources:LocalizedText, Indietro %>" OnClick="linkButtonNavIndietro_Click"  />
            <asp:LinkButton CssClass="nav-avanti" ID="linkButtonNavAvanti" runat="server" Text="<%$ Resources:LocalizedText, Avanti %>" OnClick="linkButtonNavAvanti_Click" />
        </div>

    </div>
    </form>
</body>
</html>
