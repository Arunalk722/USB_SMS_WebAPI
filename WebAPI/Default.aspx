<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebAPI.Index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="bootstrap.min.css" rel="stylesheet" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
          <div class="alert alert-info border-2 col-md-12">
                <asp:Label runat="server" Text="Lable" ID="LblMsg" CssClass="alert alert-info col-md-12"></asp:Label>
          </div>
            <div class="alert alert-dark">
              <div class="col col-md-12">
                Message-
                    <asp:Label runat="server" Text="Lable" ID="LblMessage"></asp:Label>
            </div>
            </div>
           <div class="alert alert-dark">
                    <div class="col col-md-12">
                 Recerver-
                    <asp:Label runat="server" Text="Lable" ID="LblRecerver"></asp:Label>
            </div>
               </div>
        </div>
    </form>
</body>
</html>
