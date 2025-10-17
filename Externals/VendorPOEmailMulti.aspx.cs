using IP.Classess;
using PlusCP.Classess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PlusCP.Externals
{
    public partial class VendorPOEmailMulti : System.Web.UI.Page
    {
        public string DBConnectionString { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string Action = Request.QueryString["Action"].ToString();
                string Vendor = Request.QueryString["Vendor"].ToString();
                string GUID = Request.QueryString["GUID"].ToString();
                string ConnectionType = Request.QueryString["Connection"].ToString();

                GetConnectionString(ConnectionType);

                DataTable dt = new DataTable();
                dt = GetData(GUID, Vendor);

                if (!AlreadyAsnwer(GUID, Vendor))
                {
                    ShowData();
                }
                else
                {
                    //btnSubmit.Visible = false;
                    //lblMessage.Visible = true;
                    ThankYou();
                }

            }
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

        public bool AlreadyAsnwer(string GUID, string Vendor)
        {
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            ExternalDAL oDAL = new ExternalDAL();
            string sql = "";
            sql = "SELECT * FROM [SRM].[VendorCommunication] WHERE GUID = '<GUID>' AND IsAnswered = '1' AND VendorName Like '%" + Vendor + "%' ";
            sql = sql.Replace("<GUID>", GUID);

            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql, DBConnectionString);
            if (dt.Rows.Count > 0)
                return true;
            else
                return false;
        }
        public void ShowData()
        {
            string GUID = "";
            string Action = "";
            string Vendor = "";
            GUID = Request.QueryString["GUID"].ToString();
            Action = Request.QueryString["Action"].ToString();
            Vendor = Request.QueryString["Vendor"].ToString();
            DataTable dt = new DataTable();
            dt = GetData(GUID, Vendor);
            ViewState["GridViewData"] = dt;
            dgvVendor.DataSource = dt;
            dgvVendor.DataBind();
        }
        public DataTable GetData(string GUID, string Vendor)
        {
            object result;
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            ExternalDAL oEXDAL = new ExternalDAL();
            string sql = @"SELECT
      ContactReason
  FROM [SRM].[BuyerPO]
  where GUID = '<GUID>' ";
            sql = sql.Replace("<GUID>", GUID);
            result = oEXDAL.GetObject(sql, DBConnectionString);

            if (result.ToString() == "Change")
            {
                sql = @"select vendorName, CONCAT(PONum,'-',[LineNo],'-',RelNo) AS PONO, PartNo,Qty,NewDueDate AS DueDate, Price, VendorEmail, Buyer, FORMAT(DueDate, 'MM/dd/yyyy') AS PreviousDueDate, TrackingNo, '' AS ddlServiceType 
from[SRM].[BuyerPO] WHERE GUID = '<GUID>' AND VendorName Like '%<Vendor>%' ";
                //// Find the column by its header text and set its visibility
                BoundField newDueDateColumn = dgvVendor.Columns.Cast<DataControlField>()
                                                   .FirstOrDefault(column => column.HeaderText == "Previous Due Date") as BoundField;
                if (newDueDateColumn != null)
                {
                    newDueDateColumn.Visible = true;
                }
            }
            else
            {
                sql = @"select vendorName, CONCAT(PONum,'-',[LineNo],'-',RelNo) AS PONO, PartNo,Qty,DueDate, Price, VendorEmail, Buyer, TrackingNo, '' AS ddlServiceType
from[SRM].[BuyerPO] WHERE GUID = '<GUID>' AND VendorName Like '%<Vendor>%' ";

                // Find the column by its header text and set its visibility
                //BoundField newDueDateColumn = dgvVendor.Columns.Cast<DataControlField>()
                //                                   .FirstOrDefault(column => column.HeaderText == "New Due Date") as BoundField;
                //if (newDueDateColumn != null)
                //{
                //    newDueDateColumn.Visible = false;
                //}
            }

            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<Vendor>", Vendor);

            DataTable dt = new DataTable();
            dt = oEXDAL.GetData(sql, DBConnectionString);

            return dt;
        }

        public object GetContactReason(string GUID)
        {
            object result;
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            ExternalDAL oDAL = new ExternalDAL();
            string sql = @"SELECT
      ContactReason
  FROM [SRM].[BuyerPO]
  where GUID = '<GUID>' ";
            sql = sql.Replace("<GUID>", GUID);
            result = oDAL.GetObject(sql, DBConnectionString);
            return result;
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string GUID = "";
            string Action = "";
            string Vendor = "";
            try
            {

                GUID = Request.QueryString["GUID"].ToString();
                Action = Request.QueryString["Action"].ToString();
                Vendor = Request.QueryString["Vendor"].ToString();
                string ConnectionType = Request.QueryString["Connection"].ToString();
                DBConnectionString = GetConnectionString(ConnectionType);
                ExternalDAL oDAL = new ExternalDAL();
                string sql = "";
                sql = "Select * from [SRM].[VendorCommunication] Where GUID = '<GUID>' AND VendorName like '%<VendName>%' ";
                sql = sql.Replace("<GUID>", GUID);
                sql = sql.Replace("<VendName>", Vendor);

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


                string qty = "";
                string dueDate = "";
                string TrackingNo = "";
                string BuyerEmail = "";
                string Buyer = "";
                string VendorEmail = "";
                string serviceType = "";
                string Price = "";
                string serviceUrl = "";

                DataTable dtEmail = new DataTable();
                dtEmail = GetEmailColumns(GUID, Vendor);

                if (dtEmail.Rows.Count > 0)
                {
                    BuyerEmail = dtEmail.Rows[0]["BuyerEmail"].ToString();
                    VendorEmail = dtEmail.Rows[0]["VendorEmail"].ToString();
                    Buyer = dtEmail.Rows[0]["Buyer"].ToString();

                }
                // Validations for DueDate
                foreach (GridViewRow row in dgvVendor.Rows)
                {
                    Label lblDueDate = (Label)row.FindControl("lblDueDate");
                    Label lblQty = (Label)row.FindControl("lblQty");
                    string PO = row.Cells[1].Text;
                    string InputQty = "";
                    dueDate = lblDueDate.Text;
                    InputQty = lblQty.Text;
                    if (Convert.ToDateTime(dueDate) <= DateTime.Today.Date)
                    {
                        string script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Invalid Due Date',
                    text: 'Please select a valid due date for PO No.: {PO}',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                        Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                        return;
                    }
                    if (!IsValidQty(GUID, PO, InputQty))
                    {
                        string script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Invalid Qty ',
                    text: 'Please Enter a valid Qty for PO No.: {PO}',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                        Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                        return;
                    }



            }
                foreach (GridViewRow row in dgvVendor.Rows)
                {
                    sql = @"INSERT INTO [SRM].[VendorCommunication] (VendorName, PONo, PartNo, Qty, DueDate, Price, GUID, VendorEmail, BuyerEmail, BuyerName, TrackingNo, ServiceType, ServiceURL)
                        VALUES ('<VendorName>','<PONo>','<PartNo>','<Qty>','<DueDate>','<Price>','<GUID>', '<VendorEmail>', '<BuyerEmail>','<Buyer>','<TrackingNo>','<ServiceType>', '<ServiceURL>' )";
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        Label lblQty = (Label)row.FindControl("lblQty");
                        Label lblDueDate = (Label)row.FindControl("lblDueDate");
                        Label lblTrackingNo = (Label)row.FindControl("lblTrackingNo");
                        Label lblddlServiceType = (Label)row.FindControl("lblServiceType");
                        Label lblPrice = (Label)row.FindControl("lblPrice");

                        if (lblQty != null && lblDueDate != null && lblTrackingNo != null && lblddlServiceType != null && lblPrice != null)
                        {
                            qty = lblQty.Text;
                            dueDate = lblDueDate.Text;
                            TrackingNo = lblTrackingNo.Text;
                            serviceType = lblddlServiceType.Text;
                            Price = lblPrice.Text;
                        }
                        else
                            return;

                    }

                    // Extract values from the cells
                    string vendorName = row.Cells[0].Text; // Assuming first cell value
                    string poNo = row.Cells[1].Text; // Assuming second cell value
                    string partNo = row.Cells[2].Text;




                    if (serviceType.ToUpper() == "FEDEX")
                    {
                        serviceUrl = "https://www.fedex.com/apps/fedextrack/?tracknumbers=<TrackingNo>";
                        serviceUrl = serviceUrl.Replace("<TrackingNo>", TrackingNo);
                    }
                    else if (serviceType.ToUpper().Contains("UPS"))
                    {
                        serviceUrl = "https://wwwapps.ups.com/WebTracking/track?track=yes&trackNums=<TrackingNo>";
                        serviceUrl = serviceUrl.Replace("<TrackingNo>", TrackingNo);
                    }

                    sql = sql.Replace("<VendorName>", vendorName);
                    sql = sql.Replace("<PONo>", poNo);
                    sql = sql.Replace("<PartNo>", partNo);
                    sql = sql.Replace("<Qty>", qty);
                    sql = sql.Replace("<DueDate>", dueDate);
                    sql = sql.Replace("<Price>", Price);
                    sql = sql.Replace("<GUID>", GUID);
                    sql = sql.Replace("<VendorEmail>", VendorEmail);
                    sql = sql.Replace("<BuyerEmail>", BuyerEmail);
                    sql = sql.Replace("<Buyer>", Buyer);
                    sql = sql.Replace("<TrackingNo>", TrackingNo);
                    sql = sql.Replace("<ServiceType>", serviceType);
                    sql = sql.Replace("<ServiceURL>", serviceUrl);


                    if (oDAL.Execute(sql, DBConnectionString))
                    {
                        UpdateVendorTable(poNo, GUID);
                        updatePO(poNo, GUID, Action);
                        AddInTransaction(poNo, partNo, GUID, Action, qty, Price, dueDate, vendorName);
                        ThankYou();
                    }
                    else
                    {
                        string script = $@"
            <script>
                Swal.fire({{
                    icon: 'error',
                    title: 'Oops! Error',
                    text: 'Something went wrong!',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                        Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                        return;

                    }


                }
            }
            catch (Exception ex)
            {
                Vendor = Request.QueryString["Vendor"].ToString();
                RecordError(ex.Message, ex.StackTrace, "Button Submit Click", Vendor, "PROD");
            }
        }
        public bool IsValidQty(string GUID, string PO, string InputQty)
        {
            object result;
            ExternalDAL oDAL = new ExternalDAL();
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            string sql = @" IF EXISTS (
                SELECT 1 
                FROM SRM.BuyerPO 
                WHERE (PONum + '-' + CAST([LineNo] AS VARCHAR(10)) + '-' + CAST(RelNo AS VARCHAR(10))) = '<PO>' 
                AND GUID = '<GUID>'
                AND @InputQty > OrderQty
            )
                SELECT 0 AS Result
            ELSE
                SELECT 1 AS Result ";
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<PO>", PO);
            sql = sql.Replace("@InputQty", InputQty.ToString());

            result = oDAL.GetObject(sql, DBConnectionString);
            if (result.ToString() == "1")
                return true;
            else
                return false;

        }
        public DataTable GetEmailColumns(string GUID, string Vendor)
        {
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            DataTable dt = new DataTable();
            ExternalDAL oDAL = new ExternalDAL();
            string sql = @"SELECT
      Buyer, BuyerEmail, VendorEmail
  FROM [SRM].[BuyerPO]
  where GUID = '<GUID>' AND VendorName Like '%<Vendor>%'  ";
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<Vendor>", Vendor);

            dt = oDAL.GetData(sql, DBConnectionString);

            return dt;
        }
        protected void dgvVendor_RowEditing(object sender, GridViewEditEventArgs e)
        {
            dgvVendor.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void dgvVendor_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            dgvVendor.EditIndex = -1;
            BindGrid();
        }
        private void BindGrid()
        {
            DataTable dt = ViewState["GridViewData"] as DataTable;
            if (dt != null)
            {
                dgvVendor.DataSource = dt;
                dgvVendor.DataBind();
            }
            else
            {
                ShowData();
            }
        }
        protected void dgvVendor_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow row = dgvVendor.Rows[e.RowIndex];

                TextBox txtQty = row.FindControl("txtQty") as TextBox;
                TextBox txtDueDate = row.FindControl("txtDueDate") as TextBox;
                TextBox txtTrackingNo = row.FindControl("txtTrackingNo") as TextBox;
                TextBox txtPrice = row.FindControl("txtPrice") as TextBox;
                DropDownList ddlType = row.FindControl("ddlServiceType") as DropDownList;
                string PO = row.Cells[1].Text;


                // Validate required fields
                if (string.IsNullOrWhiteSpace(txtQty.Text) || string.IsNullOrWhiteSpace(txtDueDate.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
                {
                    string script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Missing Information',
                    text: 'Please fill in all required fields: Qty, Due Date, and Price for PO No.: {PO}',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                    return;

                }

                // Get the updated values from the controls
                string newQty = txtQty.Text;
                string newDueDate = txtDueDate.Text;
                string trackingNo = txtTrackingNo.Text;
                string Price = txtPrice.Text;
                string serviceType = ddlType.SelectedValue;

                // Store the changes in the ViewState
                DataTable dt = ViewState["GridViewData"] as DataTable;
                if (dt != null)
                {
                    dt.Rows[e.RowIndex]["Qty"] = newQty;
                    dt.Rows[e.RowIndex]["DueDate"] = DateTime.Parse(newDueDate);
                    dt.Rows[e.RowIndex]["TrackingNo"] = trackingNo;
                    dt.Rows[e.RowIndex]["ddlServiceType"] = serviceType;
                    dt.Rows[e.RowIndex]["Price"] = Price;

                    // Rebind the GridView with the updated data
                    dgvVendor.EditIndex = -1;
                    BindGrid();
                }
                else
                {
                    string script = $@"
            <script>
                Swal.fire({{
                    icon: 'warning',
                    title: 'Oops! Something's Missing',
                    text: 'Data is missing.',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);
                    return;
                }
            }
            catch (Exception ex)
            {
                string script = $@"
            <script>
                Swal.fire({{
                    icon: 'error',
                    title: 'Oops! Something's Missing',
                    text: 'An error occurred during update.',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'OK'
                }});
            </script>";

                Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertScript", script);

            }
        }

        // Method to display popup message
        //private void ShowPopup(string message)
        //{
        //    string script = $"alert('{message}');";
        //    ScriptManager.RegisterStartupScript(this, this.GetType(), "Popup", script, true);
        //}


        //protected void dgvVendor_RowDataBound(object sender, GridViewRowEventArgs e)
        //{
        //    string GUID = Request.QueryString["GUID"].ToString();
        //    object result = GetContactReason(GUID);
        //    if (e.Row.RowType == DataControlRowType.DataRow && dgvVendor.EditIndex == e.Row.RowIndex)
        //    {
        //        // Find the index of the "New Due Date" column
        //        int newDueDateColumnIndex = -1;
        //        for (int i = 0; i < dgvVendor.Columns.Count; i++)
        //        {
        //            if (dgvVendor.Columns[i].HeaderText == "New Due Date")
        //            {
        //                newDueDateColumnIndex = i;
        //                break;
        //            }
        //        }

        //        // If "New Due Date" column exists and contact reason is "Change"
        //        if (newDueDateColumnIndex != -1 && result.ToString() == "Change")
        //        {
        //            // Add a TextBox with date picker to the cell in "New Due Date" column
        //            TextBox txtNewDueDate = new TextBox();
        //            txtNewDueDate.ID = "txtNewDueDate";
        //            txtNewDueDate.CssClass = "form-control datepicker";
        //            txtNewDueDate.Text = ((TextBox)e.Row.Cells[newDueDateColumnIndex].Controls[0]).Text; // Set existing value
        //            e.Row.Cells[newDueDateColumnIndex].Controls.Add(txtNewDueDate);
        //        }
        //    }
        //}
        protected void dgvVendor_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) > 0)
            {
                DropDownList ddlServiceType = (DropDownList)e.Row.FindControl("ddlServiceType");
                string serviceType = DataBinder.Eval(e.Row.DataItem, "ddlServiceType").ToString();

                if (!string.IsNullOrEmpty(serviceType))
                {
                    if (ddlServiceType.Items.FindByValue(serviceType) == null)
                    {
                        ddlServiceType.Items.Add(new ListItem(serviceType, serviceType));
                    }
                    ddlServiceType.SelectedValue = serviceType;
                }

                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // Fetch sys values only once per row
                    DataTable dtSysValues = GetSysValues();
                    bool IsQtyUpdate = dtSysValues.AsEnumerable()
                        .Where(r => r.Field<string>("SysDesc") == "IsQtyUpdate")
                        .Select(r => Convert.ToBoolean(r["SysValue"]))
                        .FirstOrDefault();
                    bool IsPriceUpdate = dtSysValues.AsEnumerable()
                        .Where(r => r.Field<string>("SysDesc") == "IsPriceUpdate")
                        .Select(r => Convert.ToBoolean(r["SysValue"]))
                        .FirstOrDefault();

                    if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                    {
                        // In Edit mode, find textboxes only
                        TextBox txtQty = (TextBox)e.Row.FindControl("txtQty");
                        TextBox txtPrice = (TextBox)e.Row.FindControl("txtPrice");

                        if (txtQty != null) txtQty.Enabled = IsQtyUpdate;
                        if (txtPrice != null) txtPrice.Enabled = IsPriceUpdate;
                    }
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

        public void AddInTransaction(string tPO, string tpartNo, string tGUID, string tHasAction, string tQty, string tprice, string tDueDate, string tcreatedBy)
        {
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            ExternalDAL oDAL = new ExternalDAL();
            object result;
            int vendorId = 0;
            string sql = @"SELECT TOP 1 Id from [SRM].[VendorCommunication] where PONo = '<PONo>' And VendorName Like '%<VendorName>%' Order By Id desc";
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

        public void updatePO(string PO, string GUID, string Action)
        {
            string ConnectionType = Request.QueryString["Connection"].ToString();
            DBConnectionString = GetConnectionString(ConnectionType);
            ExternalDAL oDAL = new ExternalDAL();
            string Vendor = Request.QueryString["Vendor"].ToString();
            string sql = @"UPDATE [SRM].[BuyerPO] SET POSTATUS = '<STATUS>' , CommunicationStatus = '<Communication>' WHERE GUID = '<GUID>'  
AND VendorName Like '%<vendor>%' AND CONCAT(PONum,'-',[LineNo],'-',RelNo) = '<PO>' ";

            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<Communication>", "Action");
            sql = sql.Replace("<vendor>", Vendor);
            sql = sql.Replace("<PO>", PO);

            if (Action == "MultiAccept")
            {
                sql = sql.Replace("<STATUS>", "Update");
            }
            else if (Action == "MultiChange")
            {
                sql = sql.Replace("<STATUS>", "Change Update");
            }

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
            lblMessage.Text = "Your response has been submitted successfully!";
            lblMessage.ForeColor = System.Drawing.Color.DarkGreen;
            dgvVendor.Visible = false;
            lblExpressLink.Visible = false;
            lblMessage.Visible = true;
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