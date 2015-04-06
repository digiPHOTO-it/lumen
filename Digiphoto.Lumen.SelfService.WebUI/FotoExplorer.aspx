<%@ Page Language="C#" 
    AutoEventWireup="true" 
    CodeBehind="FotoExplorer.aspx.cs" 
    Inherits="Digiphoto.Lumen.SelfService.WebUI.FotoExplorer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>digiPHOTO - Self Service</title>
    <link rel="stylesheet" media="all"  href="css/common.css" type="text/css"  />
    <link rel="stylesheet" media="all and (orientation:portrait)" href="css/portrait.css" />
    <link rel="stylesheet" media="all and (orientation:landscape)" href="css/landscape.css" /> 


    <script type="text/javascript">

        function posizionaWaterMarks() {
                
            var incrx = 50, offsetx = 40,
                incry = 30, offsety = 10;
            var x = 0, y = 0;
            var quantiXriga = 5;
            var vettore = document.getElementsByClassName("foto-watermark");
            for (var qq = 0; qq < vettore.length; ++qq) {
                var watermark = vettore[qq];

                var idx = parseInt(watermark.id.substring(9));

                var riga = Math.trunc(idx / quantiXriga);
                if ((idx % quantiXriga) != 0)
                    ++riga;
                var colonna = Math.trunc(idx % quantiXriga);
                if (colonna == 0)
                    colonna = quantiXriga;

                x = offsetx - ((riga -1)*5) + (incrx * (colonna - 1));
                y = offsety + ((riga -1)*60) + (incry * (colonna - 1));

                watermark.style.left = "" + x + "px";
                watermark.style.top = "" + y + "px";
            }
        }

    </script>

</head>

<body id="foto-explorer">
    

    <form id="form1" runat="server">

    <div id="pulsantiera">
   
        <asp:Button ID="btnGoHome" runat="server" OnClick="buttonHome_Click" Text="Go Home" />
        
<!--
        <asp:Button ID="buttonAutoPlay" CssClass="bottone-spostamento" runat="server" OnClick="buttonAutoPlay_Click" Text='<%# autoPlay ? "Pause" : "Auto Play" %>' Enabled="<%# possoAutoPlay %>" />
-->
        <asp:Label ID="LabelGiorno" runat="server" Text="Giorno" />
        <asp:TextBox ID="Giornata" runat="server" type="date" 
                     Text='<%# paramRicerca.giorno == null ? "" : ((DateTime)paramRicerca.giorno).ToString("yyyy-MM-dd") %>' 
                     OnTextChanged="Giornata_TextChanged" AutoPostBack="true" />
        <asp:Label ID="LabelPagina" runat="server" Text="Pagina" />
        <asp:Label ID="LabelNumPag" runat="server" Text="<%# paramRicerca.numPagina %>" />

    </div>

        <div id="foto-container" >
            <asp:Repeater ID="FotoRepeater" runat="server" DataSource="<%#listaFotografieDto%>">
                <ItemTemplate>
                    <div runat="server" class="foto">                        
                        <div runat="server" class="foto-img">
                            <asp:Image runat="server" ImageUrl='<%# getUrlImmagine( Container.ItemIndex ) %>' CssClass="foto-immagine" />
                            <span runat="server" class="foto-label"><%# Eval("numero") %></span>
                             
                            <% for( int ciclo = 1; ciclo <= 30; ciclo++ ) { %>
                                <span id="WaterMark<%= ciclo %>"  class="foto-watermark">digiPHOTO</span>
                            <% } %>

                            <div runat="server" class="copertura" />
                        </div>
                        <div id="Div2" runat="server" style="clear: both" />
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>



    <div id="container-sposta">
        <div id="popup-sposta-prev">
            <asp:ImageButton ID="frecciaIndietro" runat="server" 
                Enabled="<%# possoAndareIndietro %>" ImageUrl="~/images/left_arrow-128.png" OnClick="buttonPrev_Click" CausesValidation="false" 
                CssClass="freccia-sposta"
                />
        </div>
        <div id="popup-sposta-next">
            <asp:ImageButton ID="frecciaAvanti" runat="server" 
                Enabled="<%# possoAndareAvanti %>" ImageUrl="~/images/right_arrow-128.png" OnClick="buttonNext_Click" CausesValidation="false" 
                CssClass="freccia-sposta" />
        </div>
    </div>


    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <asp:Timer ID="timerAutoPlay" runat="server" OnTick="timerAutoPlay_Tick" 
        Interval="2000"
        Enabled="<%# autoPlay %>" />    

    </form>

    <script type="text/javascript">
        posizionaWaterMarks();
    </script>

</body>
</html>
