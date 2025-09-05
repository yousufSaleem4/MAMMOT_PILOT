<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VendorPOEmail.aspx.cs" Inherits="PlusCP.Externals.VendorPOEmail" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" runat="server">
<head runat="server">
    <link href="../Content/css/all.css" rel="stylesheet" />
    <link href="../Content/css/_all-skins.min.css" rel="stylesheet" />
    <link href="../Content/css/AdminLTE.min.css" rel="stylesheet" />
    <link href="../Content/css/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/css/all.css" rel="stylesheet" />
    <link href="../Content/css/Site.css" rel="stylesheet" />
    <link href="../Content/css/AlertMessage.css" rel="stylesheet" />
    <script src="../Scripts/jquery-3.3.1.js"></script>
    <script src="../Scripts/jquery.min.js"></script>
    <script src="../Scripts/bootstrap.min.js"></script>
    <script src="../Scripts/adminlte.js"></script>
    <script src="../Scripts/AlertMessage.js"></script>
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
        width:80px;
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
      <img src="/Content/Images/Collablly.gif" style="margin-left: 25px; margin-top: 10px; width: 15%;  margin-bottom: 3px;" />
</a>
       <%-- <hr style="background-color: #003B59; border: 0px; min-height: 4px; margin-top: 4px;" />--%>
    </div>
    <div class="col-md-10 showform" style="border-radius: 10px; border: 10px solid #f3f3f3; box-shadow: 0px 2px 2px rgba(0,0,0,0.3); padding: 25px; margin-top: 0px; width: 100%;">
        <form id="form1" runat="server">

            <div class="gridview-container">
                <div style="vertical-align: middle; text-align: center; font-size: 20px; color: #003B59">
                    <label>Supplier Submission Form</label>
                </div>

                <asp:GridView ID="dgvVendor" runat="server" CssClass="gridview" AutoGenerateColumns="false">
                    <Columns>
                        <asp:BoundField DataField="vendorName" HeaderText="Supplier" />
                        <asp:BoundField DataField="TrackingNo" HeaderText="Tracking No." Visible="false" />
                        <asp:BoundField DataField="PONO" HeaderText="PO-Line-Rel" />
                        <asp:BoundField DataField="PartNo" HeaderText="Part No." />
                        <asp:BoundField DataField="Qty" HeaderText="Qty" />
                        <asp:BoundField DataField="DueDate" DataFormatString="{0:MM/dd/yyyy}" HeaderText="Due Date" />
                        <asp:BoundField DataField="Price" HeaderText="Price" />
                    </Columns>
                </asp:GridView>
                <br />
                <div>
                    <table runat="server" id="tblChange">
                        <tr>
                            <td>
                                <asp:Label ID="lblDueDate" Font-Bold runat="server">New Due Date:</asp:Label></td>
                        </tr>
                        <tr>
                            <td class="form-group">
                                <input type="date" visible="false" class="form-control" runat="server" id="txtDueDate" /></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="lblQty" Font-Bold runat="server">New Qty:</asp:Label></td>
                        </tr>
                        <tr>
                            <td class="form-group">
                                <input type="text" visible="false" class="form-control" runat="server" onkeypress="return validateNumberInput(event)" onpaste="return handlePaste(event)" id="txtQty" required /></td>
                        </tr>
                          <tr>
                            <td>
                                <asp:Label ID="lblNewPrice"  Font-Bold runat="server">New Price:</asp:Label></td>
                        </tr>
                        <tr>
                            <td class="form-group">
                                <input type="text"  class="form-control" runat="server" onkeypress="return validateDecimalInput(event)" onpaste="return handleDecimalPaste(event)" id="txtPrice" required /></td>
                        </tr>
                        <tr>
                            <td class="form-group">
                                <input type="text" visible="false" class="form-control" runat="server" onkeypress="return validateNumberInput(event)" onpaste="return handlePaste(event)" id="Text1" required />
                            </td>
                        </tr>
                         <tr>
                            <td>
                                <asp:Label ID="lblExpressLink" runat="server" ForeColor="Blue">For FTL or LTL go to our shipping portal <a href="https://auth.wwex.com/login?state=hKFo2SBoQnduRURRRE1OOXBENDhZYko3WjRaUTZjUTVIdFBWaKFupWxvZ2luo3RpZNkgTTQ2ZVVlazA3S1pEWXZlTGhOSWdhNnp4QlE3Y2haam2jY2lk2SBuQ25XZjFMNEtuRUFyUjhRTW1VcHNvVTFCWE9XSXUwNg&client=nCnWf1L4KnEArR8QMmUpsoU1BXOWIu06&protocol=oauth2&response_type=code&audience=wwex-apig&redirect_uri=https%3A%2F%2Fwww.speedship.com%2Fcallback&ui_locales=en&scope=openid%20profile%20email&response_mode=query&nonce=aFRTSW9mY0wyTkZDOXEwdEt5WU9QMnJzNlNwVzhOVzVyWm1YQ1p3ZkdDZg%3D%3D&code_challenge=hX_26lyPRC2tPgKJPKFNA_fXmYWkj3v86HOiBLoFUmA&code_challenge_method=S256&auth0Client=eyJuYW1lIjoiYXV0aDAtc3BhLWpzIiwidmVyc2lvbiI6IjEuMjIuNiJ9" target="_blank">Worldwide Express (wwex.com)</a></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="lblTrackingNo" Font-Bold="true" runat="server">Tracking No.:</asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td class="form-group">
                                <div class="row">
                                    <div class="col-md-6">
                                        <input type="text" visible="false" class="form-control" runat="server" id="txtTrackingNo" />
                                    </div>
                                    <div class="col-md-6">
                                        <select class="form-control" runat="server" id="ddlType">
                                            <option value="Select">Select from list</option>
                                            <option value="FedEx">FedEx</option>
                                            <option value="UPS Next Day Air">UPS Next Day Air</option>
                                            <option value="UPS 2nd Day Air">UPS 2nd Day Air</option>
                                            <option value="UPS Ground">UPS Ground</option>

                                        </select>
                                    </div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>
                                <asp:Label ID="lblFileUpload" Font-Bold runat="server">Upload File:</asp:Label></td>
                        </tr>
                        <tr>
                            <td class="form-group">
                                <input type="file" visible="false" class="form-control" runat="server" accept=".pdf" id="fileUpload" />
                                <asp:Label ID="lblFileError" runat="server" ForeColor="Red" Visible="false">Invalid file type or size exceeds 1MB.</asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="lblNote" runat="server" ForeColor="Blue">Please attach only PDF files. Maximum size allowed is 1MB.</asp:Label>
                            </td>
                        </tr>

                       

                      


                    </table>
                </div>

                <div style="text-align: center; vertical-align: middle">
                    <asp:Button ID="btnSubmit" class="btn-hover-save" runat="server" Text="Submit" OnClick="btnSubmit_Click" />
                    <br />
                    <asp:Label ID="lblMsg" runat="server" ForeColor="red"></asp:Label>
                    <asp:Label Visible="false" ID="lblMessage" runat="server" ForeColor="red" Font-Size="Larger" Font-Bold="true">Please wait for buyer response.</asp:Label>
                </div>

            </div>
        </form>
    </div>
    <asp:Label ID="lblsignid" runat="server" ForeColor="White"></asp:Label>

    <div class="tryagain" style="visibility: hidden"><span class="text-red"><b>Your request has expired.</b></span></div>
</body>
</html>

<script type="text/javascript">

    //----For User Request----//
    var PO = '';
    var partNo = '';
    var dueDate = '';
    var qty = '';
    var price = '';
    var buyer = '';

    window.load = load();

    function load() {
        //$(".showform").hide();
        $(".showmessage").hide();
        $(".tryagain").hide();
      //var signid = '<%= Request.QueryString["PO"]%>';
      <%--  PO = document.getElementById("<%=lblPONo.ClientID %>").innerHTML;--%>


    }
    function openPopup() {
        // Add your code here to open the popup window
        // For example:
        window.open('AddDueDate.aspx', 'PopupWindow', 'width=800,height=600');
    }


    function validateNumberInput(evt) {
        var charCode = (evt.which) ? evt.which : evt.keyCode;
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }
        return true;
    }


    function handlePaste(event) {
        var clipboardData, pastedData;

        event.stopPropagation();
        event.preventDefault();

        clipboardData = event.clipboardData || window.clipboardData;
        pastedData = clipboardData.getData('text');

        if (!(/^\d+$/.test(pastedData))) {
            return false;
        }
    }
    // Allow only numeric values and a single decimal point
    function validateDecimalInput(event) {
        const char = String.fromCharCode(event.which || event.keyCode);
        const input = event.target.value;

        // Allow digits (0-9) and a single decimal point
        if (!/^[0-9.]$/.test(char)) return false;

        // Allow only one decimal point
        if (char === '.' && input.includes('.')) return false;

        return true;
    }

    // Validate pasted input to allow only valid decimal format
    function handleDecimalPaste(event) {
        const pastedData = (event.clipboardData || window.clipboardData).getData('text');

        // Ensure only valid numeric with one optional decimal point (e.g., 123.45)
        if (!/^\d+(\.\d{0,2})?$/.test(pastedData)) {
            event.preventDefault();
            return false;
        }

        return true;
    }

    //$(".button").click(function () {
    //    var retCaptcha;
    //    var retVal = PasswordIsValid();
    //    if (retVal == true) {
    //        retCaptcha = ValidCaptcha();
    //    }

    //    if (retVal == true && retCaptcha == true) {
    //        var newPwd = $("#newPwd").val();
    //        var confirmPwd = $('#confirmPwd').val();

    //        $.ajax({
    //            type: 'POST',
    //            url: '/Home/ChangeExternalPassword',
    //            data: { newPwd: newPwd, confirmPwd: confirmPwd, signId: signId },
    //            success: function (data) {
    //                $('#lblMsg').empty();
    //                $("#newPwd").val = '';
    //                $('#confirmPwd').val = '';
    //                $('#lblMsg').append(data);
    //                if (data == "Old and new password cannot be same.") {
    //                    $('#lblMsg').removeClass('SeaGreenColor');
    //                }
    //                if (data == "Password has been reset.") {
    //                    $('#lblMsg').addClass('SeaGreenColor');
    //                    window.location.href = '/Home/Logout';
    //                }
    //                else
    //                    $('#lblMsg').removeClass('SeaGreenColor');
    //            },
    //            fail: function () {
    //                alert('fail');
    //            },
    //            error: function (r) {
    //                alert('error');
    //            }

    //        });
    //    }
    //});




</script>

