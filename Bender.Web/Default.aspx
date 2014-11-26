<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Bender.Web.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:HiddenField ID="hdnId" runat="server"></asp:HiddenField>
        <span>Nome: </span>
        <asp:TextBox ID="txtNome" runat="server"></asp:TextBox>
        <div>
            <asp:RadioButtonList ID="rblSesso" runat="server" DataTextField="Descrizione" DataValueField="Id">
                <%--
                <asp:ListItem Text="Femmina" Value="F" />
                <asp:ListItem Text="Maschio" Value="M" />
                --%>
            </asp:RadioButtonList>
        </div>
        <div>
            <asp:CheckBoxList ID="cblPatenti" runat="server" DataTextField="Descrizione" DataValueField="Id"></asp:CheckBoxList>
        </div>
        <div>
            <asp:DropDownList ID="Colors" runat="server">
                <asp:ListItem Text="Red" Value="1" />
                <asp:ListItem Text="Green" Value="2" />
                <asp:ListItem Text="Blue" Value="3" />
            </asp:DropDownList>
        </div>
        <asp:Panel ID="pnlPannello" runat="server">
            <asp:GridView ID="grdAlbums" runat="server" AutoGenerateColumns="false">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            Album</HeaderTemplate>
                        <ItemTemplate>
                            <asp:HiddenField ID="hdnId" runat="server" Value='<%# Eval("Id") %>'></asp:HiddenField>
                            <asp:Label ID="lblNome" runat="server" Text='<%# Eval("Nome") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            Autore</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="lblAutore" runat="server" Text='<%# Eval("Autore") %>'></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            Data</HeaderTemplate>
                        <ItemTemplate>
                            <asp:TextBox ID="txtData" runat="server" Text='<%# Eval("Data") %>'></asp:TextBox></ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>
        <asp:Button ID="btnSalva" runat="server" Text="Salva" onclick="btnSalva_Click"></asp:Button>
    </div>
    </form>
</body>
</html>
