using IP.Classess;
using PlusCP.Classess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PlusCP.Externals
{
    public partial class VendorPOEmail : System.Web.UI.Page
    {
        public string DBConnectionString { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {

                string GUID = "";
                string Action = "";
                string PO = "";
                string ConType = "";
                DateTime DueDate = new DateTime();
                string Qty = "";
                string Price = "";
                string TrackingNo = "";
                string ServiceType = "";
                GUID = Request.QueryString["GUID"].ToString();
                Action = Request.QueryString["Action"].ToString();
                PO = Request.QueryString["PO"].ToString();
                ConType = Request.QueryString["Connection"].ToString();
                PO = Uri.UnescapeDataString(PO.Replace(" ", "+"));
                PO = BasicEncrypt.Instance.Decrypt(PO);

                DataTable dtTracking = new DataTable();
                DataTable dt = new DataTable();
                DataTable dtSysValues = new DataTable();
                if (!AlreadyAsnwer(GUID, PO))
                {
                    dt = GetData(GUID, PO);
                    dgvVendor.DataSource = dt;
                    dgvVendor.DataBind();

                    dtTracking = GetVendorTrackingDtls(GUID, PO);

                    if (dt.Rows.Count > 0)
                    {
                        DueDate = Convert.ToDateTime(dt.Rows[0]["DueDate"]);
                        Qty = dt.Rows[0]["Qty"].ToString();
                        Price = dt.Rows[0]["Price"].ToString();
                    }
                    if (Action == "Accept")
                    {

                        lblDueDate.Visible = false;
                        lblQty.Visible = false;
                        lblNewPrice.Visible = false;
                        txtDueDate.Visible = false;
                        txtQty.Visible = false;
                        txtPrice.Visible = false;
                        lblTrackingNo.Visible = true;
                        txtTrackingNo.Visible = true;
                        lblFileUpload.Visible = true;
                        fileUpload.Visible = true;

                        
                        if (DueDate < DateTime.Now)
                        {
                            lblDueDate.Visible = true;
                            txtDueDate.Visible = true;
                            txtDueDate.Value = DueDate.ToString("yyyy-MM-dd");
                        }
                    }
                    else if (Action == "Change")
                    {
                        lblDueDate.Visible = true;
                        lblQty.Visible = true;
                        txtDueDate.Visible = true;
                        txtQty.Visible = true;
                        lblTrackingNo.Visible = true;
                        txtTrackingNo.Visible = true;
                        lblFileUpload.Visible = true;
                        fileUpload.Visible = true;
                        txtDueDate.Value = DueDate.ToString("yyyy-MM-dd");
                        txtQty.Value = Qty;
                        txtPrice.Value = Price;

                        // Start Qty and Price Enable / Disable
                        dtSysValues = GetSysValues();
                        bool IsQtyUpdate = false;
                        bool IsPriceUpdate = false;
                        if (dtSysValues.Rows.Count > 0) // Check is Qty and Price update Allow
                        {
                            // Get value for SysDesc = 'IsQtyUpdate'
                            IsQtyUpdate = dtSysValues.AsEnumerable()
                              .Where(r => r.Field<string>("SysDesc") == "IsQtyUpdate")
                              .Select(r => Convert.ToBoolean(r["SysValue"]))
                              .FirstOrDefault();

                            // Get value for SysDesc = 'IsPriceUpdate'
                            IsPriceUpdate = dtSysValues.AsEnumerable()
                              .Where(r => r.Field<string>("SysDesc") == "IsPriceUpdate")
                              .Select(r => Convert.ToBoolean(r["SysValue"]))
                              .FirstOrDefault();
                        }

                        lblQty.Visible = IsQtyUpdate;
                        lblNewPrice.Visible = IsPriceUpdate;
                        txtQty.Visible = IsQtyUpdate;      // true → disabled, false → enabled
                        txtPrice.Visible = IsPriceUpdate;

                        // End Qty and Price Enable / Disable

                    }

                    if (dtTracking.Rows.Count > 0)
                    {
                        TrackingNo = dtTracking.Rows[0]["TrackingNo"].ToString();
                        ServiceType = dtTracking.Rows[0]["serviceType"].ToString();

                        if (!string.IsNullOrEmpty(TrackingNo))
                        {
                            txtTrackingNo.Value = TrackingNo;
                        }

                        if (!string.IsNullOrEmpty(ServiceType))
                        {
                            ddlType.Value = ServiceType;
                        }
                    }

                   

                }
                else
                {
                    //btnSubmit.Visible = false;
                    //lblDueDate.Visible = false;
                    //lblQty.Visible = false;
                    //txtDueDate.Visible = false;
                    //txtQty.Visible = false;
                    //lblMessage.Visible = true;
                    //lblTrackingNo.Visible = false;
                    //lblFileUpload.Visible = false;
                    //ddlType.Visible = false;
                    //lblNote.Visible = false;
                    //lblExpressLink.Visible = false;
                    ThankYou();
                }
            }
        }


        public DataTable GetSysValues()
        {
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            string sql = "";
            sql = @"select * from [dbo].[zSysIni] Where  SysDesc = 'IsPriceUpdate' OR SysDesc = 'IsQtyUpdate'
                    ORDER BY SysCode DESC ";

            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql, DBConnectionString);
            return dt;
        }

        public string GetConnectionString(string ConType)
        {
            // SQL query to get the connection value from the database
            string sql = @"SELECT [ConValue] FROM [SRM].[zConStr] WHERE ConType = @ConType";

            // Read the setup connection string from the configuration file
            string fileName = ConfigurationManager.AppSettings["Key"];
            string setupConnection = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());

            // Variable to store the result connection string
            string connectionString = string.Empty;

            // Use ADO.NET to execute the query
            using (SqlConnection conn = new SqlConnection(setupConnection))
            {
                try
                {
                    conn.Open(); // Open the connection

                    // Create the SqlCommand and add the parameter to avoid SQL injection
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ConType", ConType);

                        // Execute the query and get the result using SqlDataReader
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Retrieve the ConValue column from the result
                                connectionString = reader["ConValue"].ToString();
                                DBConnectionString = connectionString;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    conn.Close(); // Ensure the connection is closed after usage
                }
            }

            return connectionString;
        }
        public bool AlreadyAsnwer(string GUID, string PO)
        {
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            string sql = "";
            sql = @"SELECT * FROM [SRM].[VendorCommunication] WHERE GUID = '<GUID>' AND IsAnswered = '1' 
                  AND PONo = '<PO>' Order By Id Desc ";
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<PO>", PO);
            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql, DBConnectionString);
            if (dt.Rows.Count > 0)
                return true;
            else
                return false;
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            DataTable dt = new DataTable();

            string VendorEmail = "";
            string BuyerEmail = "";
            string Buyer = "";
            string Vendor = "";
            string PONo = "";
            string PartNo = "";
            string Qty = "";
            string DueDate = "";
            string Price = "";

            string GUID = "";
            string Action = "";
            string PO = "";
            string sql = "";
            double Qtyvalue = 0;
            double PriceValue = 0;
            int POFinalPrice = 0;
            string serviceType = "";
            string serviceUrl = "";

            Action = Request.QueryString["Action"].ToString();
            GUID = Request.QueryString["GUID"].ToString();
            PO = Request.QueryString["PO"].ToString();
            PO = Uri.UnescapeDataString(PO.Replace(" ", "+"));
            PO = BasicEncrypt.Instance.Decrypt(PO);



          



            string script = "";
            if (Action == "Change")
            {
                if (txtQty.Value == "" || txtQty.Value == "0")
                {
                    script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Invalid Qty',
                    text: 'New Qty value must not be empty or 0.',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                    return;
                }
                if (txtPrice.Value == "" || txtPrice.Value == "0")
                {
                    script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Invalid Price',
                    text: 'New Price value must not be empty or 0.',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                    return;
                }

                // Start Qty and Price Enable / Disable
                DataTable dtSysValues = new DataTable();
                dtSysValues = GetSysValues();
                bool IsQtyUpdate = false;
                bool IsPriceUpdate = false;
                if (dtSysValues.Rows.Count > 0) // Check is Qty and Price update Allow
                {
                    // Get value for SysDesc = 'IsQtyUpdate'
                    IsQtyUpdate = dtSysValues.AsEnumerable()
                      .Where(r => r.Field<string>("SysDesc") == "IsQtyUpdate")
                      .Select(r => Convert.ToBoolean(r["SysValue"]))
                      .FirstOrDefault();

                    // Get value for SysDesc = 'IsPriceUpdate'
                    IsPriceUpdate = dtSysValues.AsEnumerable()
                      .Where(r => r.Field<string>("SysDesc") == "IsPriceUpdate")
                      .Select(r => Convert.ToBoolean(r["SysValue"]))
                      .FirstOrDefault();
                }

                lblQty.Visible = IsQtyUpdate;
                lblNewPrice.Visible = IsPriceUpdate;
                txtQty.Visible = IsQtyUpdate;      // true → disabled, false → enabled
                txtPrice.Visible = IsPriceUpdate;

                // End Qty and Price Enable / Disable


            }


            sql = "Select * from [SRM].[VendorCommunication] Where GUID = '<GUID>' AND PONo = '<PONO>' ";
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<PONO>", PO);

            DataTable dtVendorData = new DataTable();
            dtVendorData = oDAL.GetData(sql, DBConnectionString);
            if (dtVendorData.Rows.Count > 0)
            {
                string isAlreadyAnswer = dtVendorData.Rows[0]["IsAnswered"].ToString();
                if (isAlreadyAnswer == "True")
                {
                    ThankYou();
                    return;
                }
            }
            dt = GetData(GUID, PO);
            int NewQty = 0;
            string NewPrice = "";
            if (Action == "Change")
            {
                NewQty = Convert.ToInt32(txtQty.Value);
                NewPrice = txtPrice.Value.ToString();
            }




            // File Upload
            string filename = "";
            string ext = "";
            Byte[] bytes = null;

            string filePath = fileUpload.PostedFile.FileName;
            filename = Path.GetFileName(filePath);
            ext = Path.GetExtension(filename);
            string contenttype = String.Empty;

            if (ext != "")
            {
                if (ext != ".pdf")
                {
                    lblFileError.Text = "Please attach PDF file";
                    lblFileError.Visible = true;
                    return;
                }
                //Set the contenttype based on File Extension
                switch (ext)
                {
                    case ".doc":
                    case ".docx":
                        contenttype = "application/vnd.ms-word";
                        break;
                    case ".xls":
                    case ".xlsx":
                        contenttype = "application/vnd.ms-excel";
                        break;
                    case ".jpg":
                        contenttype = "image/jpg";
                        break;
                    case ".png":
                        contenttype = "image/png";
                        break;
                    case ".gif":
                        contenttype = "image/gif";
                        break;
                    case ".pdf":
                        contenttype = "application/pdf";
                        break;
                    case "":
                        contenttype = "";
                        break;
                    default:
                        contenttype = "NotSupport";
                        break;

                }
            }


            if (contenttype == "NotSupport")
            {
                lblFileError.Visible = true;
                return;
            }

            if (contenttype != String.Empty)
            {
                Stream fs = fileUpload.PostedFile.InputStream;
                if (fs.Length <= 1024 * 1024)
                {
                    BinaryReader br = new BinaryReader(fs);
                    bytes = br.ReadBytes((Int32)fs.Length);
                }
                else
                {
                    lblFileError.Visible = true;
                    return;
                }
            }



            sql = @"INSERT INTO [SRM].[VendorCommunication] (VendorName, PONo, PartNo, Qty, DueDate, Price, TrackingNo, FileName, FileExt, AttachFile, VendorEmail, BuyerName, BuyerEmail, GUID, ServiceType, ServiceURL)
                VALUES (@VendorName, @PONo, @PartNo, @Qty, @DueDate, @Price, @TrackingNo, @FileName, @FileExt, @AttachFile, @VendorEmail, @Buyer, @BuyerEmail, @GUID, @ServiceType, @ServiceURL)";

            foreach (DataRow row in dt.Rows)
            {
                Vendor = row["vendorName"].ToString();
                PONo = row["PONO"].ToString();
                PartNo = row["PartNo"].ToString();
                Qty = row["Qty"].ToString();
                DueDate = row["DueDate"].ToString();
                Price = row["Price"].ToString();
                Qtyvalue = Convert.ToDouble(Qty);
                PriceValue = Convert.ToDouble(Price);
                POFinalPrice = (int)PriceValue;
            }

            DataTable dtEmails = GetEmailColumns(GUID, Vendor);
            if (dtEmails.Rows.Count > 0)
            {
                VendorEmail = dtEmails.Rows[0]["VendorEmail"].ToString();
                BuyerEmail = dtEmails.Rows[0]["BuyerEmail"].ToString();
                Buyer = dtEmails.Rows[0]["Buyer"].ToString();
            }

            if (txtTrackingNo.Value != "" && ddlType.Value == "Select")
            {
                script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Missing Information',
                    text: 'Please select Service type.',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                return;
            }
            serviceType = ddlType.Value;
            if (serviceType.Contains("FedEx"))
            {
                serviceUrl = "https://www.fedex.com/apps/fedextrack/?tracknumbers=<TrackingNo>";
                serviceUrl = serviceUrl.Replace("<TrackingNo>", txtTrackingNo.Value);
            }
            else if (serviceType.Contains("UPS"))
            {
                serviceUrl = "https://wwwapps.ups.com/WebTracking/track?track=yes&trackNums=<TrackingNo>";
                serviceUrl = serviceUrl.Replace("<TrackingNo>", txtTrackingNo.Value);
            }


            if (Action == "Accept")
            {
                if (txtDueDate.Value == "")
                    txtDueDate.Value = DueDate;
                if (txtPrice.Value == "")
                    txtPrice.Value = POFinalPrice.ToString();
                if (txtQty.Value == "")
                    txtQty.Value = Qtyvalue.ToString();

                if (!IsDueDateValid(Action))
                {
                    tblChange.Visible = true;
                    txtPrice.Visible = false;
                    txtQty.Visible = false;
                    lblNewPrice.Visible = false;
                    lblQty.Visible = false;
                    txtDueDate.Visible = true;

                    script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Invalid Due Date',
                    text: 'Please select a valid due date',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                    return;
                }
                SqlParameter attachFileParam = new SqlParameter("@AttachFile", SqlDbType.VarBinary);
                if (bytes != null && bytes.Length > 0)
                {
                    attachFileParam.Value = bytes;
                }
                else
                {
                    attachFileParam.Value = DBNull.Value;
                }

                SqlCommand cmd = new SqlCommand(sql);
                cmd.Parameters.AddWithValue("@VendorName", Vendor);
                cmd.Parameters.AddWithValue("@PONo", PONo);
                cmd.Parameters.AddWithValue("@PartNo", PartNo);
                cmd.Parameters.AddWithValue("@Qty", Qtyvalue.ToString());
                cmd.Parameters.AddWithValue("@DueDate", txtDueDate.Value);
                cmd.Parameters.AddWithValue("@Price", Price);
                cmd.Parameters.AddWithValue("@TrackingNo", txtTrackingNo.Value);
                cmd.Parameters.AddWithValue("@FileName", filename);
                cmd.Parameters.AddWithValue("@FileExt", ext);
                cmd.Parameters.Add(attachFileParam);

                cmd.Parameters.AddWithValue("@VendorEmail", VendorEmail);
                cmd.Parameters.AddWithValue("@Buyer", Buyer);
                cmd.Parameters.AddWithValue("@BuyerEmail", BuyerEmail);
                cmd.Parameters.AddWithValue("@GUID", GUID);
                cmd.Parameters.AddWithValue("@ServiceType", serviceType);
                cmd.Parameters.AddWithValue("@ServiceURL", serviceUrl);

                oDAL.ExecuteCommand(cmd, DBConnectionString);
            }
            else if (Action == "Change")
            {

                if (string.IsNullOrWhiteSpace(txtDueDate.Value))
                {
                    script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Invalid Due Date',
                    text: 'Please select a valid due date',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                    return;

                }
                if (!IsDueDateValid(Action))
                {
                    script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Invalid Due Date',
                    text: 'Please select a valid due date',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                    return;

                }

                if (Convert.ToInt32(Qty) < NewQty)
                {
                    script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Invalid Qty',
                    text: 'Please enter a valid Qty',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                    return;
                }
                SqlParameter attachFileParam = new SqlParameter("@AttachFile", SqlDbType.VarBinary);
                if (bytes != null && bytes.Length > 0)
                {
                    attachFileParam.Value = bytes;
                }
                else
                {
                    attachFileParam.Value = DBNull.Value;
                }
                SqlCommand cmd = new SqlCommand(sql);
                cmd.Parameters.AddWithValue("@VendorName", Vendor);
                cmd.Parameters.AddWithValue("@PONo", PONo);
                cmd.Parameters.AddWithValue("@PartNo", PartNo);
                cmd.Parameters.AddWithValue("@Qty", txtQty.Value.ToString());
                cmd.Parameters.AddWithValue("@DueDate", txtDueDate.Value);
                cmd.Parameters.AddWithValue("@Price", txtPrice.Value.ToString());
                cmd.Parameters.AddWithValue("@TrackingNo", txtTrackingNo.Value);
                cmd.Parameters.AddWithValue("@FileName", filename);
                cmd.Parameters.AddWithValue("@FileExt", ext);
                cmd.Parameters.Add(attachFileParam);
                cmd.Parameters.AddWithValue("@VendorEmail", VendorEmail);
                cmd.Parameters.AddWithValue("@Buyer", Buyer);
                cmd.Parameters.AddWithValue("@BuyerEmail", BuyerEmail);
                cmd.Parameters.AddWithValue("@GUID", GUID);
                cmd.Parameters.AddWithValue("@ServiceType", serviceType);
                cmd.Parameters.AddWithValue("@ServiceURL", serviceUrl);

                Price = txtPrice.Value.ToString();
                oDAL.ExecuteCommand(cmd, DBConnectionString);

            }

            if (!oDAL.HasErrors)
            {
                updatePO(PONo, GUID, Action);
                AddInTransaction(PONo, PartNo, GUID, Action, txtQty.Value, Price, txtDueDate.Value, Vendor);
                UpdateVendorTable(PONo, GUID);
                ThankYou();

            }
            else
            {
                script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Error',
                    text: 'Something went wrong!',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                RecordError(oDAL.InternalErrMsg, "", "Submit button of Vendor PO Single", Vendor, "PROD");
                return;
            }

        }

        public DataTable GetEmailColumns(string GUID, string Vendor)
        {
            DataTable dt = new DataTable();
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            string sql = @"SELECT
      Buyer,BuyerEmail, VendorEmail
  FROM [SRM].[BuyerPO]
  where GUID = '<GUID>' AND VendorName = '<Vendor>'  ";
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<Vendor>", Vendor);

            dt = oDAL.GetData(sql, DBConnectionString);

            return dt;
        }

        public DataTable GetData(string GUID, string PO)
        {
            object result;
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            string sql = @"SELECT
      ContactReason
  FROM [SRM].[BuyerPO]
  where GUID = '<GUID>' AND CONCAT(PONum,'-',[LineNo],'-',RelNo) = '<PO>' ";
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<PO>", PO);
            result = oDAL.GetObject(sql, DBConnectionString);

            if (result.ToString() != "Change")
            {
                sql = @"select vendorName, TrackingNo, CONCAT(PONum,'-',[LineNo],'-',RelNo) AS PONO, PartNo,Qty,DueDate, Price  
                            from[SRM].[BuyerPO] WHERE GUID = '<GUID>' 
                            AND CONCAT(PONum,'-',[LineNo],'-',RelNo) = '<PO>' ";
            }
            else
            {
                sql = @"select vendorName, TrackingNo, CONCAT(PONum,'-',[LineNo],'-',RelNo) AS PONO, PartNo,Qty,NewDueDate As DueDate, Price  
                            from[SRM].[BuyerPO] WHERE GUID = '<GUID>' 
                            AND CONCAT(PONum,'-',[LineNo],'-',RelNo) = '<PO>' ";
            }
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<PO>", PO);
            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql, DBConnectionString);

            return dt;
        }

        public DataTable GetVendorTrackingDtls(string GUID, string PO)
        {
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            string sql = "";

                sql = @" SELECT TOP 1 serviceType, TrackingNo  FROM [SRM].[VendorCommunication]
                             WHERE GUID = '<GUID>' AND  PONo = '<PO>' ORDER BY ID DESC ";
                    
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<PO>", PO);
            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql, DBConnectionString);

            return dt;
        }

        public void updatePO(string PO, string GUID, string Action)
        {
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            string sql = "UPDATE [SRM].[BuyerPO] SET POSTATUS = '<STATUS>' , CommunicationStatus = '<Communication>' WHERE GUID = '<GUID>'  AND CONCAT(PONum,'-',[LineNo],'-',RelNo) = '<PO>' ";
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<PO>", PO);
            sql = sql.Replace("<Communication>", "Action");
            if (Action == "Accept")
            {
                sql = sql.Replace("<STATUS>", "Update");
            }
            else if (Action == "Change")
            {
                sql = sql.Replace("<STATUS>", "Change Update");
            }

            oDAL.Execute(sql, DBConnectionString);

        }

        public bool IsDueDateValid(string Action)
        {
            bool isValid = true;
            if (Action == "Accept")
            {
                if (txtDueDate.Value == null)
                {
                    foreach (GridViewRow row in dgvVendor.Rows)
                    {
                        if (row.RowType == DataControlRowType.DataRow)
                        {
                            // Assuming your date column is in the second cell (index 1)
                            DateTime dateColumnValue;
                            if (DateTime.TryParse(row.Cells[4].Text, out dateColumnValue))
                            {
                                if (dateColumnValue.Date <= DateTime.Today)
                                {
                                    isValid = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    DateTime InputDueDate = DateTime.Parse(txtDueDate.Value);
                    if (InputDueDate >= DateTime.Today)
                    {
                        isValid = true;
                    }
                    else
                    {
                        isValid = false;
                    }
                }


            }
            else if (Action == "Change")
            {
                DateTime InputDueDate = DateTime.Parse(txtDueDate.Value);
                if (InputDueDate >= DateTime.Today)
                {
                    isValid = true;
                }
                else
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        public void AddInTransaction(string tPO, string tpartNo, string tGUID, string tHasAction, string tQty, string tprice, string tDueDate, string tcreatedBy)
        {
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            object result;
            int vendorId = 0;
            string sql = @"SELECT TOP 1 Id from [SRM].[VendorCommunication] where PONo = '<PONo>' And VendorName = '<VendorName>' Order By Id desc";
            sql = sql.Replace("<PONo>", tPO);
            sql = sql.Replace("<VendorName>", tcreatedBy);

            result = oDAL.GetObject(sql, DBConnectionString);
            if (result != null)
            {
                vendorId = Convert.ToInt32(result);
            }


            sql = @"INSERT INTO [SRM].[Transaction]
           ([PONo]
           ,[PartNo]
           ,[Type]
           ,[GUID]
           ,[HasAction]
           ,[Qty]
           ,[Price]
           ,[DueDate]
		   ,[CreatedBy]
           ,[VendorCommId]
		   )
     VALUES
           (
		   '<PONo>'
           ,'<PartNo>'
           ,'<Type>'
           ,'<GUID>'
           ,'<HasAction>'
           ,'<Qty>'
           ,'<Price>'
           ,'<DueDate>'
		   ,'<CreatedBy>'
           ,<VendorCommId>
		   )";


            sql = sql.Replace("<PONo>", tPO);
            sql = sql.Replace("<PartNo>", tpartNo);
            sql = sql.Replace("<Type>", "Supplier");
            sql = sql.Replace("<GUID>", tGUID);
            sql = sql.Replace("<HasAction>", tHasAction);
            sql = sql.Replace("<Qty>", tQty);
            sql = sql.Replace("<Price>", tprice);
            sql = sql.Replace("<DueDate>", tDueDate);
            sql = sql.Replace("<CreatedBy>", tcreatedBy);
            sql = sql.Replace("<VendorCommId>", vendorId.ToString());

            oDAL.Execute(sql, DBConnectionString);
        }

        public void UpdateVendorTable(string PO, string GUID)
        {
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            string sql = "";
            sql = "UPDATE [SRM].[VendorCommunication] SET IsAnswered = '1' WHERE PONO = '<PO>' and GUID = '<GUID>' ";
            sql = sql.Replace("<PO>", PO);
            sql = sql.Replace("<GUID>", GUID);
            oDAL.Execute(sql, DBConnectionString);
        }

        public void ThankYou()
        {
            btnSubmit.Visible = false;
            lblDueDate.Visible = false;
            lblQty.Visible = false;
            txtDueDate.Visible = false;
            txtQty.Visible = false;
            lblNewPrice.Visible = false;
            txtPrice.Visible = false;
            lblMessage.Visible = true;
            lblTrackingNo.Visible = false;
            lblFileUpload.Visible = false;
            ddlType.Visible = false;
            lblNote.Visible = false;
            lblExpressLink.Visible = false;
            dgvVendor.Visible = false;
            lblFileUpload.Visible = false;
            txtTrackingNo.Visible = false;
            fileUpload.Visible = false;
            lblMessage.Text = "Your response has been submitted successfully!";
            lblMessage.ForeColor = System.Drawing.Color.DarkGreen;
        }

        public void RecordError(string errorMsg, string errorStack, string errorQry, string email, string DefaulDB)
        {
            ExternalDAL oDal = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            errorMsg = errorMsg.Replace("'", "");
            errorQry = errorQry.Replace("'", "");
            string sql = "";


            sql = "INSERT INTO zLogError (ErrMsg, ErrStack, ErrBy, ErrOn, ErrFrom, ErrQry, ErrEnvironment) ";
            sql += "VALUES (";
            sql += "'" + errorMsg + "', ";
            sql += "'" + errorStack + "', ";
            sql += "'" + ((email == null) ? "unknown" : email) + "', ";
            sql += "Getdate(), ";
            sql += "'SRM', ";
            sql += "\'" + errorQry + "\',";
            sql += "'" + DefaulDB + "') ";
            oDal.Execute(sql, DBConnectionString);
        }
    }
}