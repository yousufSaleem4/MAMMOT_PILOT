<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="POHeadPDF.aspx.cs" Inherits="PlusCP.Externals.POHeadPDF" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PO Acknowledgment</title>


    <style>
        input.status-new[readonly] {
            background: #EFF6FF !important;
            border-color: #BFDBFE !important;
            color: #1D4ED8 !important;
        }

        input.status-sent[readonly] {
            background: #FFF7ED !important;
            border-color: #FED7AA !important;
            color: #C2410C !important;
        }

        input.status-received[readonly] {
            background: #ECFDF5 !important;
            border-color: #A7F3D0 !important;
            color: #065F46 !important;
        }

        input.status-reject[readonly] {
            background: #FEF2F4 !important;
            border-color: #F8C8D1 !important;
            color: #8f0308 !important;
        }

        .btn-hover-save {
            -webkit-tap-highlight-color: transparent;
            background: linear-gradient(180deg, #015070, #003B59);
            border: 1px solid #003B59;
            color: #fff;
            height: 50px; /* your original height */
            padding: 0 26px; /* comfy horizontal padding */
            border-radius: 999px; /* pill shape */
            font-weight: 700;
            letter-spacing: .3px;
            cursor: pointer;
            box-shadow: 0 10px 18px rgba(0,59,89,.25), /* outer glow */
            inset 0 1px 0 rgba(255,255,255,.25); /* subtle top sheen */
            transition: transform .08s ease, box-shadow .2s ease, filter .2s ease, background-color .2s ease, color .2s ease;
        }

            .btn-hover-save:hover {
                background: transparent; /* your original hover intent */
                color: #003B59;
                border-color: #003B59;
                box-shadow: 0 12px 22px rgba(0,59,89,.28), inset 0 1px 0 rgba(255,255,255,.0);
            }

            .btn-hover-save:active {
                transform: translateY(1px);
                box-shadow: 0 6px 14px rgba(0,59,89,.22), inset 0 1px 0 rgba(255,255,255,.0);
            }

            .btn-hover-save:focus {
                outline: none;
                box-shadow: 0 0 0 4px rgba(0,59,89,.18), /* focus ring */
                0 10px 18px rgba(0,59,89,.25), inset 0 1px 0 rgba(255,255,255,.25);
            }

            /* Disabled state (if needed) */
            .btn-hover-save[disabled],
            .btn-hover-save.disabled {
                opacity: .6;
                cursor: not-allowed;
                filter: grayscale(.1);
                box-shadow: none;
            }

        /* ==== UPDATED: larger Terms & Conditions modal + roomier padding ==== */
        html, body {
            height: 100%;
        }

        .form-gap {
            display: none;
        }

        .center-vh {
            min-height: calc(100vh - 110px);
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 24px 0;
        }

        :root {
            --brand: #003B59;
            --ok: #16a34a;
            --ok2: #22c55e;
            --danger: #ef4444;
            --ink: #0f172a;
            --muted: #6b7280;
            --ring: #38bdf8;
        }

        /* Acknowledge button */
        .btn-ack {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            padding: 12px 22px;
            border-radius: 999px;
            border: 1px solid transparent;
            background: linear-gradient(180deg,var(--brand),#012838);
            color: #fff;
            font-weight: 600;
            letter-spacing: .3px;
            cursor: pointer;
            box-shadow: 0 10px 18px rgba(0,59,89,.25), inset 0 1px 0 rgba(255,255,255,.15);
            transition: transform .08s ease, box-shadow .18s ease, filter .18s ease;
        }

            .btn-ack:hover {
                filter: brightness(1.06)
            }

            .btn-ack:active {
                transform: translateY(1px)
            }

            .btn-ack:focus {
                outline: none;
                box-shadow: 0 0 0 4px rgba(56,189,248,.25)
            }

        /* Overlay + dialog */
        .tm-overlay {
            position: fixed;
            inset: 0;
            display: none;
            z-index: 1050;
            background: radial-gradient(1200px 800px at 10% -10%, rgba(56,189,248,.15), transparent 60%), radial-gradient(1000px 700px at 110% 110%, rgba(34,197,94,.12), transparent 55%), rgba(2,6,23,.55);
            backdrop-filter: blur(6px);
        }

            .tm-overlay.show {
                display: block;
            }

        .tm-dialog {
            width: min(960px, 96vw); /* was 720px */
            max-height: 90vh; /* new: keep dialog within viewport */
            margin: 6.5% auto;
            color: #0b1220;
            background: linear-gradient(180deg, rgba(255,255,255,.9), rgba(255,255,255,.86));
            -webkit-backdrop-filter: blur(10px);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(2,6,23,.08);
            border-radius: 18px;
            overflow: hidden;
            position: relative;
            box-shadow: 0 20px 60px rgba(2,6,23,.35), inset 0 1px 0 rgba(255,255,255,.4);
            transform: scale(.98);
            opacity: 0;
            transition: transform .18s ease, opacity .18s ease;
            display: flex;
            flex-direction: column; /* allow interior to size nicely */
        }

        .tm-overlay.show .tm-dialog {
            transform: scale(1);
            opacity: 1;
        }

        .tm-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 20px 24px; /* was 16px 18px 14px */
            border-bottom: 1px solid rgba(2,6,23,.06);
            background: linear-gradient(180deg, #ffffff, #f8fafc);
        }

        .tm-title {
            display: flex;
            align-items: center;
            gap: 10px
        }

            .tm-title i {
                color: var(--brand);
                font-size: 20px
            }

            .tm-title h3 {
                margin: 0;
                font-size: 20px;
                font-weight: 700;
                color: #0f172a;
                letter-spacing: .2px
            }
        /* was 18px */

        .tm-close {
            border: 0;
            background: transparent;
            font-size: 28px;
            line-height: 1;
            padding: 0 6px;
            cursor: pointer;
            color: #334155;
            border-radius: 10px;
            transition: background .15s ease, color .15s ease;
        }

            .tm-close:hover {
                background: #eef2f7;
                color: #0f172a
            }

        .tm-body {
            padding: 22px 24px 10px; /* was 18px 18px 6px */
            overflow: auto; /* allow content to scroll if tall */
        }
        /* FIXED terms area height with its own scroll; long words won't expand width */
        .tm-terms {
            flex: 0 0 360px; /* fixed height */
            max-height: 360px;
            overflow-y: auto;
            overflow-x: hidden;
            padding: 16px 18px;
            background: #fbfdff;
            border: 1px solid #e5e9f0;
            border-radius: 12px;
            line-height: 1.6;
            color: #334155;
            word-break: break-word;
            overflow-wrap: anywhere;
        }

        .tm-accept {
            margin-top: 14px;
            color: #0f172a;
            font-weight: 600
        }

            .tm-accept input[type="checkbox"] {
                margin-right: 8px;
                transform: translateY(1px)
            }

        .tm-footer {
            display: flex;
            justify-content: flex-end;
            gap: 10px;
            padding: 16px 24px 22px; /* was 14px 18px 18px */
            border-top: 1px solid rgba(2,6,23,.06);
            background: linear-gradient(180deg,#fff,#f9fafb);
        }

        /* Beautiful pill buttons */
        /* Clean, elegant Accept / Reject buttons (no icons) */
        .tm-footer {
            display: flex;
            justify-content: flex-end;
            gap: 12px;
            padding: 16px 24px 22px;
        }

        .btn-pill {
            -webkit-appearance: none;
            appearance: none;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            height: 50px;
            min-width: 140px;
            padding: 0 22px;
            border-radius: 999px;
            font-weight: 700;
            letter-spacing: .2px;
            font-size: 15px;
            border: 1px solid transparent;
            cursor: pointer;
            transition: transform .06s ease, box-shadow .18s ease, filter .18s ease, background-color .2s ease, color .2s ease, border-color .2s ease;
        }

            .btn-pill:hover {
                filter: brightness(1.03)
            }

            .btn-pill:active {
                transform: translateY(1px)
            }

            .btn-pill:focus {
                outline: none;
                box-shadow: 0 0 0 4px rgba(56,189,248,.25)
            }

            .btn-pill[disabled] {
                opacity: .55;
                cursor: not-allowed;
                box-shadow: none;
            }

        /* Accept (keeps green) */
        .btn-success-grad {
            color: #052e16;
            background: linear-gradient(180deg,#22c55e,#16a34a);
            border-color: #16a34a;
            box-shadow: 0 10px 20px rgba(34,197,94,.25), inset 0 1px 0 rgba(255,255,255,.28);
        }

            .btn-success-grad:hover {
                filter: brightness(1.05)
            }

        /* Reject (soft red outline) */
        .btn-danger-soft {
            color: #991b1b;
            background: #fff;
            border-color: #fecaca;
            box-shadow: 0 6px 14px rgba(239,68,68,.08);
        }

            .btn-danger-soft:hover {
                background: #fff5f5;
                border-color: #fca5a5;
            }
        /* Read-only inputs */
        input[readonly] {
            background: #f5f7fa;
            color: #334155;
            cursor: default;
            box-shadow: none;
        }

        .body-no-scroll {
            overflow: hidden
        }

        /* Wider on very large screens */
        @media (min-width:1400px) {
            .tm-dialog {
                width: min(1100px, 92vw);
            }
        }
    </style>

    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.5.0/css/font-awesome.min.css" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.5.0/css/font-awesome.min.css" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css" />
    <link href="../Content/css/AlertMessage.css" rel="stylesheet" />
</head>
<body>

    <form id="form2" runat="server">
        <div style="padding-top: 1px; background-color: #D6EFE8">
            <%-- <img src="~/Content/Images/clogo2.jpg" style=" margin-left:5px; width:150px; height:50px;" />--%>

            <a href="https://collablly.com/" target="_blank">
                <img src="/Content/Images/Collablly.gif" style="margin-left: 25px; margin-top: 10px; width: 15%; margin-bottom: 3px;" />
            </a>
            <%-- <hr style="background-color: #003B59; border: 0px; min-height: 4px; margin-top: 4px;" />--%>
        </div>

        <div class="form-gap"></div>
        <div class="container">
            <!-- Centering wrapper -->
            <div class="center-vh">
                <div class="container">
                    <div class="row">
                        <!-- Added xs/sm classes so it stays centered on all screens -->
                        <div class="col-xs-12 col-sm-8 col-sm-offset-2 col-md-6 col-md-offset-3">
                            <div class="panel panel-default panel-colla">
                                <div class="panel-heading text-center">
                                    <h3 class="panel-title" style="font-weight: 600; letter-spacing: .3px">PO Details</h3>
                                    <p class="text-muted" style="margin: 6px 0 0">Initial Communication</p>
                                </div>
                                <div class="panel-body">

                                    <!-- SHOW-ONLY FIELDS -->
                                    <div class="form-group">
                                        <div class="row row-tight">
                                            <!-- PO Number -->
                                            <div class="col-xs-12 col-sm-7">
                                                <label for="txtPoNumber">PO Number</label>
                                                <div class="input-group">
                                                    <span class="input-group-addon"><i class="glyphicon glyphicon-tag"></i></span>
                                                    <asp:TextBox ID="txtPoNumber" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                                </div>
                                            </div>

                                            <!-- Status -->
                                            <div class="col-xs-12 col-sm-5">
                                                <label for="txtStatus">Status</label>
                                                <div class="input-group">
                                                    <span class="input-group-addon"><i class="glyphicon glyphicon-ok-circle"></i></span>
                                                    <!-- Add one of: status-ok / status-warn / status-err / status-info -->
                                                    <asp:TextBox ID="txtStatus" runat="server"
                                                        CssClass="form-control status-readonly status-info" ReadOnly="true"></asp:TextBox>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="form-group">
                                        <label for="txtBuyer">Buyer</label>
                                        <div class="input-group">
                                            <span class="input-group-addon"><i class="glyphicon glyphicon-user"></i></span>
                                            <asp:TextBox ID="txtBuyer" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                        </div>
                                    </div>

                                    <div class="form-group">
                                        <label for="txtVendorName">Vendor Name</label>
                                        <div class="input-group">
                                            <span class="input-group-addon"><i class="glyphicon glyphicon-briefcase"></i></span>
                                            <asp:TextBox ID="txtVendorName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                        </div>
                                    </div>

                                    <div class="form-group">
                                        <label for="txtVendorEmail">Vendor Email</label>
                                        <div class="input-group">
                                            <span class="input-group-addon"><i class="glyphicon glyphicon-envelope"></i></span>
                                            <asp:TextBox ID="txtVendorEmail" runat="server" CssClass="form-control" TextMode="Email" ReadOnly="true"></asp:TextBox>
                                        </div>
                                    </div>

                                    <div class="text-center" style="margin-top: 18px">
                                        <asp:Button ID="btnAcknowledge" runat="server" Text="Acknowledge"
                                            CssClass="btn-hover-save"
                                            OnClientClick="showTerms(); return false;" />
                                    </div>

                                    <div class="text-center" style="margin-top: 12px">
                                        <asp:Label ID="lblMsg" runat="server" CssClass="text-success"></asp:Label>
                                    </div>

                                    <asp:HiddenField ID="token" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div id="wait" class="modal" style="display: none; width: 100%; height: 100%; left: 0; top: 0; text-align: center; vertical-align: middle; background: rgba(255,255,255,.6)">
            <img src="~/Content/images/Loading.gif" width="100" height="100" style="margin-top: 300px" />
        </div>


        <!-- Terms Modal -->
        <div id="termsModal" class="tm-overlay" role="dialog" aria-modal="true" aria-labelledby="tmTitle" aria-hidden="true">
            <div class="tm-dialog" role="document">
                <div class="tm-header">
                    <div class="tm-title">
                        <i class="fa fa-file-text-o"></i>
                        <h3 id="tmTitle">Terms &amp; Conditions</h3>
                    </div>
                    <button type="button" class="tm-close" aria-label="Close" onclick="closeTerms()">&times;</button>
                </div>

                <div class="tm-body">
                    <div class="tm-terms">
                        <asp:Literal ID="litTerms" runat="server" Mode="PassThrough" />
                    </div>

                    <div class="tm-accept">
                        <asp:CheckBox ID="chkAccept" runat="server"
                            Text="I accept the Terms &amp; Conditions"
                            onclick="toggleAccept()" />
                    </div>
                </div>

                <div class="tm-footer">
                    <asp:Button ID="btnReject" runat="server" Text="Reject"
                        CssClass="btn-pill btn-danger-soft"
                        OnClick="btnReject_Click" />

                    <asp:Button ID="btnAccept" runat="server" Text="Accept"
                        CssClass="btn-pill btn-success-grad"
                        Enabled="false"
                        OnClick="btnAccept_Click" />
                </div>
            </div>
        </div>

        <script src="https://code.jquery.com/jquery-2.2.4.min.js"></script>
        <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js"></script>
        <script src="../Scripts/AlertMessage.js"></script>
    </form>


    <script type="text/javascript">
        function showTerms() {
            var m = document.getElementById("termsModal");
            m.classList.add("show");
            document.body.classList.add("body-no-scroll");

            // reset state on open
            var cb = document.getElementById("<%= chkAccept.ClientID %>");
            var btn = document.getElementById("<%= btnAccept.ClientID %>");
            if (cb) { cb.checked = false; }
            if (btn) { btn.disabled = true; }
        }
        function closeTerms() {
            var m = document.getElementById("termsModal");
            m.classList.remove("show");
            document.body.classList.remove("body-no-scroll");
        }
        function toggleAccept() {
            var checkBox = document.getElementById("<%= chkAccept.ClientID %>");
            var acceptBtn = document.getElementById("<%= btnAccept.ClientID %>");
            if (acceptBtn) { acceptBtn.disabled = !checkBox.checked; }
        }
        // click outside to close
        window.addEventListener('click', function (e) {
            var modal = document.getElementById("termsModal");
            if (e.target === modal) { closeTerms(); }
        });
        // esc to close
        window.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') { closeTerms(); }
        });
    </script>
</body>
</html>
