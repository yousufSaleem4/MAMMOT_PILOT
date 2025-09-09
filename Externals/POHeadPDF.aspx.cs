using IP.Classess;
using PlusCP.Classess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Sql;
using System.Data.SqlClient;
namespace PlusCP.Externals
{
    public partial class POHeadPDF : System.Web.UI.Page
    {
        public string DBConnectionString { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPurchaseOrderDetails();
            }
        }
        private void LoadPurchaseOrderDetails()
        {
            string PONo = Request.QueryString["PONo"];
            string ConnectionType = Request.QueryString["Connection"]; // if needed later
            string GUID = Request.QueryString["GUID"];                 // if needed later

            string sql = @"SELECT [Id],[PONumber],[Vendor],[VendorEmail],[Buyer],[IsAnswerd],[Status]
                   FROM [dbo].[BuyerPOHeader]
                   WHERE PONumber = @PONo";

            string fileName = ConfigurationManager.AppSettings["Key"];
            string DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0]);

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(DBConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PONo", PONo ?? (object)DBNull.Value);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];

                if (string.Equals(Convert.ToString(r["IsAnswerd"]), "True", StringComparison.OrdinalIgnoreCase))
                    btnAcknowledge.Enabled = false;

                txtPoNumber.Text = Convert.ToString(r["PONumber"]);
                txtBuyer.Text = Convert.ToString(r["Buyer"]);
                txtVendorName.Text = Convert.ToString(r["Vendor"]);
                txtVendorEmail.Text = Convert.ToString(r["VendorEmail"]);

                string status = Convert.ToString(r["Status"]);
                if (status == "Sent")
                {
                    txtStatus.Text = "Pending";
                    txtStatus.CssClass = "form-control status-readonly status-sent";
                    //txtStatus.Attributes["style"] = "background:red; border-color:red; color:#C2410C; font-weight:700;";

                }
                else if (status == "Received")
                {
                    txtStatus.Text = "Acknowledged";
                    txtStatus.CssClass = "form-control status-readonly status-received";
                    //txtStatus.Attributes["style"] = "background:#FFF7ED; border-color:#FED7AA; color:#C2410C; font-weight:700;";

                }
                else if (status == "Reject")
                {
                    txtStatus.Text = "Rejected";
                    txtStatus.CssClass = "form-control status-readonly status-reject";
                }
                else
                {
                    txtStatus.Text = "New";
                    txtStatus.CssClass = "form-control status-readonly status-new";
                }

                txtPoNumber.ReadOnly = true;
                txtBuyer.ReadOnly = true;
                txtVendorName.ReadOnly = true;
                txtVendorEmail.ReadOnly = true;

                GetTermCondition();
            }
            else
            {
                txtPoNumber.Text = txtBuyer.Text = txtVendorName.Text = txtVendorEmail.Text = string.Empty;
                txtStatus.Text = string.Empty;
                txtStatus.CssClass = "form-control status-readonly";
            }
        }
        public void GetTermCondition()
        {
            string termsHtml = null;
            string sql = "select SysValue from [dbo].[zSysIni] where [SysCode] = '12647' ";
            string fileName = ConfigurationManager.AppSettings["Key"];
            string DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0]);

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(DBConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                termsHtml = Convert.ToString(r["SysValue"]);
            }
            else
            {
                termsHtml = "";
            }
            litTerms.Text = termsHtml;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // Handle whatever action you need (e.g., send email/continue workflow)

        }
        protected void btnAccept_Click(object sender, EventArgs e)
        {
            string PONo = Request.QueryString["PONo"];
            string GUID = Request.QueryString["GUID"];

            ExternalDAL oDAL = new ExternalDAL();
            string fileName = ConfigurationManager.AppSettings["Key"];
            DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0]);

            string sql = @"UPDATE [dbo].[BuyerPOHeader] 
                           SET STATUS='Received',
                               UpdatedOn=GETDATE(),
                               UpdatedBy='Vendor',
                               IsAnswerd=1
                           WHERE PONumber='" + PONo + "' AND GUID='" + GUID + "'";

            oDAL.Execute(sql, DBConnectionString);

            //Add Transaction
            AddTransaction(PONo, "Received");


             // Close the modal and show a SUCCESS alert (works with/without UpdatePanel)
             var script = @"
  (function(){
    // close custom overlay modal
    var m = document.getElementById('termsModal');
    if (m) { m.classList.remove('show'); }
    document.body.classList.remove('body-no-scroll');

    // fallback: if using Bootstrap modal markup anywhere
    if (window.jQuery && typeof $('#termsModal').modal === 'function') {
      try { $('#termsModal').modal('hide'); } catch(e) {}
    }

    // success message
    Swal.fire({
      icon: 'success',
      title: 'Success',
      text: 'Operation completed successfully.',
      confirmButtonColor: '#003B59',
      confirmButtonText: 'OK'
    });
  })();";

            // If your page uses UpdatePanel:
            ScriptManager.RegisterStartupScript(this, GetType(), "CloseModalAndSuccess", script, true);
            LoadPurchaseOrderDetails();
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            string PONo = Request.QueryString["PONo"];
            string GUID = Request.QueryString["GUID"];

            ExternalDAL oDAL = new ExternalDAL();
            string fileName = ConfigurationManager.AppSettings["Key"];
            DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0]);

            string sql = @"UPDATE [dbo].[BuyerPOHeader] 
                           SET STATUS='Reject',
                               UpdatedOn=GETDATE(),
                               UpdatedBy='Vendor',
                               IsAnswerd=1
                           WHERE PONumber='" + PONo + "' AND GUID='" + GUID + "'";

            oDAL.Execute(sql, DBConnectionString);
            
            //Add Transaction
            AddTransaction(PONo, "Reject");

            // Close the modal and show a SUCCESS alert (works with/without UpdatePanel)
            var script = @"
  (function(){
    // close custom overlay modal
    var m = document.getElementById('termsModal');
    if (m) { m.classList.remove('show'); }
    document.body.classList.remove('body-no-scroll');

    // fallback: if using Bootstrap modal markup anywhere
    if (window.jQuery && typeof $('#termsModal').modal === 'function') {
      try { $('#termsModal').modal('hide'); } catch(e) {}
    }

    // success message
    Swal.fire({
      icon: 'success',
      title: 'Success',
      text: 'Operation completed successfully.',
      confirmButtonColor: '#003B59',
      confirmButtonText: 'OK'
    });
  })();";

            // If your page uses UpdatePanel:
            ScriptManager.RegisterStartupScript(this, GetType(), "CloseModalAndSuccess", script, true);
            LoadPurchaseOrderDetails();
        }

        public DataTable GetPOHeaderData(string PONo)
        {
            string sql = "";
            sql = @"SELECT [Id],[PONumber],[Vendor],[VendorEmail],[Buyer],[IsAnswerd],[Status], [GUID]
                       FROM [dbo].[BuyerPOHeader]
                       WHERE PONumber = @PONo";

            string fileName = ConfigurationManager.AppSettings["Key"];
            string DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0]);

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(DBConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PONo", PONo ?? (object)DBNull.Value);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public void AddTransaction(string PO, string Status)
        {
            ExternalDAL oDAL = new ExternalDAL();
            string fileName = ConfigurationManager.AppSettings["Key"];
            DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0]);
            DataTable dt = GetPOHeaderData(PO);
            string query = string.Empty;
            if (dt.Rows.Count > 0)
            {
                query = @"INSERT INTO [dbo].[BuyerPOHeaderTran]
           ([PONumber]
           ,[Vendor]
           ,[VendorEmail]
           ,[Status]
           ,[GUID]
           ,[Buyer]
           ,[UserType]
           ,[InsertedBy]

)
     VALUES
           ('<PONumber>'
           ,'<Vendor>'
           ,'<VendorEmail>'
           ,'<Status>'
           ,'<GUID>'
           ,'<Buyer>'
           ,'<UserType>'
           ,'<InsertedBy>'

)";

            }

            
            query = query.Replace("<PONumber>", PO);
            query = query.Replace("<Vendor>", dt.Rows[0]["Vendor"].ToString());
            query = query.Replace("<VendorEmail>", dt.Rows[0]["VendorEmail"].ToString());
            query = query.Replace("<Status>", Status);
            query = query.Replace("<GUID>", dt.Rows[0]["GUID"].ToString());
            query = query.Replace("<Buyer>", dt.Rows[0]["Buyer"].ToString());
            query = query.Replace("<UserType>", "Supplier");
            query = query.Replace("<InsertedBy>", dt.Rows[0]["Vendor"].ToString());

            oDAL.Execute(query, DBConnectionString);
        }
        public DateTime ConvertGenericDateTime()
        {
            string sql = "";
            DataTable dt = new DataTable();
            ExternalDAL oDAL = new ExternalDAL();
            sql = "Select top 1 TimeZone from SRM.UserInfo ";
            string fileName = ConfigurationManager.AppSettings["Key"];
            DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());

            dt = oDAL.GetData(sql, DBConnectionString);
            DateTime localTime = new DateTime();
            if (dt.Rows.Count > 0)
            {
                string timezone = dt.Rows[0]["TimeZone"].ToString(); //HttpContext.Current.Session["TimeZone"].ToString();

                DateTime utcTime = DateTime.UtcNow;

                // Get the TimeZoneInfo object for the selected timezone
                TimeZoneInfo selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);

                // Convert UTC time to the selected time zone, DST-aware
                localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, selectedTimeZone);
            }


            return localTime;
        }

    }
}