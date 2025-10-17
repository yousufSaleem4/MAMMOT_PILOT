<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VendorPOEmailMulti.aspx.cs" Inherits="PlusCP.Externals.VendorPOEmailMulti" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <link href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap-datepicker/1.9.0/css/bootstrap-datepicker.min.css" rel="stylesheet" />
    <link href="../Content/css/all.css" rel="stylesheet" />
    <link href="../Content/css/_all-skins.min.css" rel="stylesheet" />
    <link href="../Content/css/AdminLTE.min.css" rel="stylesheet" />
    <link href="../Content/css/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/css/all.css" rel="stylesheet" />
    <link href="../Content/css/Site.css" rel="stylesheet" />
    <link href="../Content/css/AlertMessage.css" rel="stylesheet" />
    <script src="../Scripts/AlertMessage.js"></script>
    <script src="../Scripts/jquery-3.3.1.js"></script>
    <script src="../Scripts/jquery.min.js"></script>
    <script src="../Scripts/bootstrap.min.js"></script>
    <script src="../Scripts/adminlte.js"></script>
    <script src="https://code.jquery.com/ui/1.13.1/jquery-ui.js"></script>
    <!-- Include Bootstrap and Bootstrap Datepicker scripts and stylesheets -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/bootstrap-datepicker/1.9.0/js/bootstrap-datepicker.min.js"></script>
    <title>SRM Portal</title>

    <style>
        /* Custom styles for the grid */
        .gridview-container {
            width: 100%;
            margin: auto;
            font-family: Arial, sans-serif;
        }

        .gridview {
            width: 100%;
            border-collapse: collapse;
        }

            .gridview th, .gridview td {
                padding: 10px;
                border: 1px solid #ddd;
                text-align: left;
            }

            .gridview th {
                background-color: #f2f2f2;
                font-weight: bold;
            }

            .gridview tr:nth-child(even) {
                background-color: #f9f9f9;
            }

            .gridview tr:hover {
                background-color: #f2f2f2;
            }

        .button {
            width: 100px;
            padding: 12px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
            margin-top: 6px;
            margin-bottom: 16px;
            background-color: #325FAB;
            color: white;
        }

        .btn-hover-save {
            background-color: #003B59; /* Default background color */
            border-color: #003B59; /* Default border color */
            color: #fff; /* Default text color */
            height: 30px;
            width: 80px;
        }

            .btn-hover-save:hover {
                background-color: transparent; /* Make background transparent on hover */
                border-color: #003B59; /* Dark border color on hover */
                color: #003B59; /* Dark font color on hover */
            }

        body {
            /*margin-top: 20px;*/
            background: linear-gradient(to top, white, #D6EFE8);
        }
    </style>

</head>
<body runat="server">
    <div style="padding-top: 1px; background-color: #D6EFE8">
        <%-- <img src="~/Content/Images/clogo2.jpg" style=" margin-left:5px; width:150px; height:50px;" />--%>
        <a href="https://collablly.com/" target="_blank">
            <img src="/Content/Images/Collablly.gif" style="margin-left: 25px; margin-top: 10px; width: 15%; margin-bottom: 3px;" />
        </a>
        <%--<hr style="background-color: #325FAB; border: 0px; min-height: 4px; margin-top: 4px;" />--%>
    </div>
    <div class="col-md-10 showform" style="border-radius: 10px; border: 10px solid #f3f3f3; box-shadow: 0px 2px 2px rgba(0,0,0,0.3); padding: 25px; margin-top: 0px; width: 100%;">
        <form id="form1" runat="server">

            <div class="gridview-container">
                <div style="vertical-align: middle; text-align: center; font-size: 20px; color: #003B59">
                    <label>Supplier Submission Form</label>
                </div>
                <asp:GridView ID="dgvVendor" runat="server" CssClass="gridview" AutoGenerateColumns="False" OnRowEditing="dgvVendor_RowEditing"
                    OnRowCancelingEdit="dgvVendor_RowCancelingEdit" OnRowUpdating="dgvVendor_RowUpdating" OnRowDataBound="dgvVendor_RowDataBound" Width="100%">
                    <Columns>
                        <asp:BoundField DataField="vendorName" HeaderText="Supplier" ReadOnly="true" />
                        <asp:BoundField DataField="PONO" HeaderText="PO-Line-Rel" ReadOnly="true" />
                        <asp:BoundField DataField="PartNo" HeaderText="Part No." ReadOnly="true" />
                        <asp:TemplateField HeaderText="Qty">
                            <ItemTemplate>
                                <asp:Label ID="lblQty" runat="server" Text='<%# Eval("Qty") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtQty"  runat="server" Text='<%# Bind("Qty") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemStyle Width="70px" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Due Date">
                            <ItemTemplate>
                                <asp:Label ID="lblDueDate" runat="server" Text='<%# Eval("DueDate", "{0:MM/dd/yyyy}") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtDueDate" runat="server" ClientIDMode="Static" CssClass="form-control datepicker" Text='<%# Bind("DueDate", "{0:MM/dd/yyyy}") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemStyle Width="150px" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Tracking No.">
                            <ItemTemplate>
                                <asp:Label ID="lblTrackingNo" runat="server" Text='<%# Eval("TrackingNo") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtTrackingNo" runat="server" Text='<%# Bind("TrackingNo") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemStyle Width="140px" />
                        </asp:TemplateField>


                        <asp:TemplateField HeaderText="Service Type">
                            <ItemTemplate>
                                <asp:Label ID="lblServiceType" runat="server" Text='<%# Eval("ddlServiceType") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList ID="ddlServiceType" runat="server" AutoPostBack="false" onchange="toggleTrackingTextbox(this)">
                                    <asp:ListItem Value="Select" Selected="True">Select From List</asp:ListItem>
                                    <asp:ListItem Value="Fedex">Fedex</asp:ListItem>
                                    <asp:ListItem Value="UPS Next Day Air">UPS Next Day Air</asp:ListItem>
                                    <asp:ListItem Value="UPS 2nd Day Air">UPS 2nd Day Air</asp:ListItem>
                                    <asp:ListItem Value="UPS Ground">UPS Ground</asp:ListItem>
                                    <asp:ListItem Value="Freight">Freight</asp:ListItem>
                                    <asp:ListItem Value="Others">Others</asp:ListItem>
                                </asp:DropDownList>
                            </EditItemTemplate>
                            <ItemStyle Width="140px" />
                        </asp:TemplateField>

                        <asp:BoundField DataField="PreviousDueDate" HeaderText="Previous Due Date" ReadOnly="true" Visible="false" />
                        <%-- <asp:BoundField DataField="Price" HeaderText="Price" ReadOnly="true" />--%>

                        <asp:TemplateField HeaderText="Price">
                            <ItemTemplate>
                                <asp:Label ID="lblPrice" runat="server" Text='<%# Eval("Price") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtPrice" runat="server" Text='<%# Bind("Price") %>'
                                    oninput="validatePrice(this);" MaxLength="20"></asp:TextBox>
                            </EditItemTemplate>
                            <ItemStyle Width="140px" />
                        </asp:TemplateField>



                        <asp:BoundField DataField="VendorEmail" HeaderText="VendorEmail" ReadOnly="true" Visible="false" />
                        <asp:BoundField DataField="Buyer" HeaderText="BuyerEmail" ReadOnly="true" Visible="false" />
                        <asp:CommandField ShowEditButton="true" />
                    </Columns>
                </asp:GridView>
                <div style="margin-top: 15px">
                    <asp:Label ID="lblExpressLink" runat="server" ForeColor="Blue">For FTL or LTL go to our shipping portal <a href="https://auth.wwex.com/login?state=hKFo2SBoQnduRURRRE1OOXBENDhZYko3WjRaUTZjUTVIdFBWaKFupWxvZ2luo3RpZNkgTTQ2ZVVlazA3S1pEWXZlTGhOSWdhNnp4QlE3Y2haam2jY2lk2SBuQ25XZjFMNEtuRUFyUjhRTW1VcHNvVTFCWE9XSXUwNg&client=nCnWf1L4KnEArR8QMmUpsoU1BXOWIu06&protocol=oauth2&response_type=code&audience=wwex-apig&redirect_uri=https%3A%2F%2Fwww.speedship.com%2Fcallback&ui_locales=en&scope=openid%20profile%20email&response_mode=query&nonce=aFRTSW9mY0wyTkZDOXEwdEt5WU9QMnJzNlNwVzhOVzVyWm1YQ1p3ZkdDZg%3D%3D&code_challenge=hX_26lyPRC2tPgKJPKFNA_fXmYWkj3v86HOiBLoFUmA&code_challenge_method=S256&auth0Client=eyJuYW1lIjoiYXV0aDAtc3BhLWpzIiwidmVyc2lvbiI6IjEuMjIuNiJ9" target="_blank">Worldwide Express (wwex.com)</a></asp:Label>
                </div>

                <div style="text-align: center; margin-top: 30px; vertical-align: middle">
                    <asp:Button ID="btnSubmit" class="btn-hover-save" runat="server" Text="Submit" OnClick="btnSubmit_Click" />
                    <asp:Label Visible="false" ID="lblMessage" runat="server" ForeColor="red" Font-Size="Larger" Font-Bold="true">Please wait for buyer response.</asp:Label>
                </div>
                <br />
                <div>
                </div>
            </div>
        </form>
    </div>
    <asp:Label ID="lblsignid" runat="server" ForeColor="White"></asp:Label>

    <div class="tryagain" style="visibility: hidden"><span class="text-red"><b>Your request has expired.</b></span></div>
</body>
</html>
<script>
    function toggleTrackingTextbox(dropdown) {
        var selectedValue = dropdown.value;

        // Find the tracking number textbox in the same row
        var row = dropdown.closest("tr");
        var trackingTextbox = row.querySelector("[id*='txtTrackingNo']");

        if (selectedValue === "Select") {
            trackingTextbox.style.display = "none"; // Hide
        } else {
            trackingTextbox.style.display = "block"; // Show
        }
    }

    // Ensure all dropdowns hide tracking number when the page loads
    window.onload = function () {
        var dropdowns = document.querySelectorAll("[id*='ddlServiceType']");
        dropdowns.forEach(function (dropdown) {
            toggleTrackingTextbox(dropdown);
        });
    };
    $(function () {
        $('.datepicker').datepicker({
            format: 'mm/dd/yyyy', // Format of the date to display
            autoclose: true, // Close the datepicker when a date is selected
            todayHighlight: true // Highlight today's date
        });
    });

    $(document).ready(function () {
        $('.qtyTextBox').on('blur', function () {
            var inputValue = $(this).val().trim();
            var isValid = /^\d+$/.test(inputValue);
            if (!isValid) {
                $(this).next('.qtyErrorMessage').text('Please enter a valid integer value.');
            } else {
                $(this).next('.qtyErrorMessage').text('');
            }
        });
    });
    function openPopup() {
        // Add your code here to open the popup window
        // For example:
        window.open('AddDueDate.aspx', 'PopupWindow', 'width=800,height=600');
    }
    // Allow only numbers with one optional decimal point and any length after it
    function validatePrice(event, element) {
        var charCode = (event.which) ? event.which : event.keyCode;

        // Allow: backspace (8), delete (46), tab (9), escape (27), enter (13)
        if (charCode == 8 || charCode == 46 || charCode == 9 || charCode == 27 || charCode == 13) {
            return true;
        }

        // Allow numbers (0-9)
        if (charCode >= 48 && charCode <= 57) {
            return true;
        }

        // Allow only one decimal point (.)
        if (charCode == 46 && element.value.indexOf('.') === -1) {
            return true;
        }

        // Block all other characters
        return false;
    }

    function validatePrice(input) {
        // Allow only valid numbers: digits and ONE decimal point
        input.value = input.value.replace(/[^0-9.]/g, '');

        // Ensure only ONE decimal point is allowed
        var parts = input.value.split('.');
        if (parts.length > 2) {
            input.value = parts[0] + '.' + parts.slice(1).join('');
        }
    }
</script>
