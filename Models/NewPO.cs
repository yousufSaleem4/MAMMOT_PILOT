using IP.Classess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlusCP.Classess;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace PlusCP.Models
{
    public class NewPO
    {
        string DeployMode = WebConfigurationManager.AppSettings["DEPLOYMODE"];
        #region Fields
        //[Display(Name = "Part No.:")]
        //public string partNo { get; set; }


        public string EmailResult { get; set; }
        [Display(Name = "Email :")]
        public string EmailId { get; set; }

        [Display(Name = "CC Email :")]
        public string CCEmailId { get; set; }


        [Display(Name = "Email :")]
        public string AlertEmailId { get; set; }

        [Display(Name = "CC Email :")]
        public string AlertCCEmailId { get; set; }

        [Display(Name = "Message")]
        public string AlertMessage { get; set; }

        [Display(Name = "Buyer Id :")]
        public string BuyerId { get; set; }

        [Display(Name = "Buyer Company :")]
        public string BuyerCompany { get; set; }

        [Display(Name = "Buyer Name :")]
        public string BuyerName { get; set; }

        [Display(Name = "Line :")]
        public string Line { get; set; }

        [Display(Name = "Rel :")]
        public string Release { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }
        [Display(Name = "PO No.")]
        public string PONo { get; set; }

        [Display(Name = "VendorId")]
        public string VendorId { get; set; }

        [Display(Name = "Vendor")]
        public string Vendor { get; set; }

        [Display(Name = "Part No.")]
        public string PartNo { get; set; }

        [Display(Name = "Part Desc.")]
        public string PartDesc { get; set; }

        [Display(Name = "UOM")]
        public string UOM { get; set; }

        [Display(Name = "Order Date")]
        public string OrderDate { get; set; }

        [Display(Name = "Due Date")]
        public string DueDate { get; set; }

        [Display(Name = "Qty")]
        public string Qty { get; set; }

        [Display(Name = "Contact Reason")]
        public string ContactReason { get; set; }

        [Display(Name = "Price")]
        public string Price { get; set; }

        [Display(Name = "OrgId")]
        public string OrgId { get; set; }

        // Supplier variables
        [Display(Name = "Current Qty")]
        public string CurrentQty { get; set; }

        [Display(Name = "Committed Qty")]
        public string CommitQty { get; set; }

        [Display(Name = "Proposed Qty")]
        public string ProposedQty { get; set; }

        [Display(Name = "Current Price")]
        public string CurrentPrice { get; set; }

        [Display(Name = "Committed Price")]
        public string CommitPrice { get; set; }

        [Display(Name = "Proposed Price")]
        public string ProposedPrice { get; set; }

        [Display(Name = "Current DueDate")]
        public string CurrentDueDate { get; set; }

        [Display(Name = "Committed DueDate")]
        public string CommitDueDate { get; set; }

        [Display(Name = "Proposed DueDate")]
        public string ProposedDueDate { get; set; }

        [Display(Name = "Created On")]
        public string CreatedOn { get; set; }
        [Display(Name = "Vendor Email")]
        public string VendorEmail { get; set; }
        [Display(Name = "PO No.")]
        public string ProposePONo { get; set; }

        [Display(Name = "Part No.")]
        public string ProposePartNo { get; set; }
        [Display(Name = "Vendor Name")]
        public string ProposeVendorName { get; set; }
        [Display(Name = "Vendor Name")]
        public string ProposeMessage { get; set; }

        [Display(Name = "Part No.")]
        public string FilterPartNo { get; set; }

        [Display(Name = "Part Desc.")]
        public string FilterPartDesc { get; set; }

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDate { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string toDate { get { return _toDt; } set { _toDt = value; } }

        [Display(Name = "newDueDate:")]
        public string _newDueDate = DateTime.Now.ToString(Format.DateOnly);
        public string newDueDate { get { return _newDueDate; } set { _newDueDate = value; } }


        //[Display(Name = "From Date")]
        //public string fromDate { get; set; }

        //[Display(Name = "To Date")]
        //public string toDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Action Status")]
        public string ActionStatus { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        //Widgets
        public string AllOpen { get; set; }
        public string Pending { get; set; }
        public string Late { get; set; }
        public string Arrived { get; set; }
        public string Update { get; set; }
        public string AllPO { get; set; }

        public string POStatus { get; set; }
        public string POInsertedOn { get; set; }
        public string POUpdatedOn { get; set; }

        public List<Hashtable> lstPO { get; set; }

        public List<Hashtable> lstPurchaseOrders { get; set; }
        public List<Hashtable> SelectedlstPO { get; set; }
        public List<Hashtable> lstUpdate { get; set; }
        public List<Hashtable> lstPOTran { get; set; }
        public List<Hashtable> lstPOHeadTran { get; set; }
        public List<object> lstMst = new List<object>();
        #endregion


        public bool GetPO(DataTable dt, string POStatus, string ConnectionType, string Email, string userType)
        {
            // Add extra columns if needed for checkboxes or view
            dt.Columns.Add("Checkbox");
            dt.Columns.Add("View");
            // Convert to hashtable list
            lstPurchaseOrders = cCommon.ConvertDtToHashTable(dt);

            return true;
        }

        public bool GetList(DataTable dt, string POStatus)
        {
            try
            {
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                int hours = GetHours();

                // Add Columns for jquery conditions
                string[] columnNames = {
            "Checkbox", "Updates", "Message", "BuyerEmailSent", "VendorEmailSent",
            "Awaiting", "Resent", "ResentCreatedDate", "ResentVendorEmail",
            "CommunicationStatus", "GUID", "ERPStatus", "PDFPO", "CalculatedExtCost", "LastCommunication", "PONumber"
        };

                // Loop through each column name and check if it exists
                foreach (string columnName in columnNames)
                {
                    // Check if column already exists
                    if (!dt.Columns.Contains(columnName))
                    {
                        // Add column if it doesn't exist
                        dt.Columns.Add(columnName);
                    }
                }

                decimal Qty = 0;
                decimal ArrivedQty = 0;
                decimal FinalQty = 0;
                decimal ExtCost = 0;

                //---------------------------------------------------------------------------

                // ADD PONUmber column which is concatenated like PO-LINE-REL
                //DataColumn concatenatedColumn = new DataColumn("PONumber", typeof(string));
                //dt.Columns.Add(concatenatedColumn);
                foreach (DataRow row in dt.Rows)
                {
                    row["PONumber"] = $"{row["POHeader_PONum"]}-{row["PODetail_POLine"]}-{row["PORel_PORelNum"]}";
                    if (row["PODetail_OrderQty"] == row["Calculated_ArrivedQty"])
                        row["CommunicationStatus"] = "Completed";
                    else
                        row["CommunicationStatus"] = "New";

                    row["PDFPO"] = row["POHeader_PONum"];

                    Qty = Convert.ToDecimal(row["PODetail_OrderQty"]);
                    ArrivedQty = Convert.ToDecimal(row["Calculated_ArrivedQty"]);
                    FinalQty = Qty - ArrivedQty;
                    ExtCost = Convert.ToDecimal(row["PODetail_UnitCost"]);

                    row["CalculatedExtCost"] = FinalQty * ExtCost;

                }
                //---------------------------------------------------------------------------

                //---------------------- Email, Awaiting Email, Actions Required, Resend Email logic
                string Buyersql = "";
                DataTable DtbuyerEmail = new DataTable();
                Buyersql = @"select CONCAT(PONum,+'-'+ [lineNo] + '-' +RelNo) As PONo, GUID, HasAction, CommunicationStatus, CreatedOn, VendorEmail
                                from[SRM].[BuyerPO] ";
                DtbuyerEmail = oDAL.GetData(Buyersql);
                foreach (DataRow row2 in DtbuyerEmail.Rows)
                {
                    string poNo = row2["PONo"].ToString();
                    string CommunicationStatus = row2["CommunicationStatus"].ToString();
                    string GUID = row2["GUID"].ToString();
                    string CreatedOn = row2["CreatedOn"].ToString();
                    string VendorEmail = row2["VendorEmail"].ToString();
                    DataRow[] matchingRows = dt.Select($"PONumber = '{poNo}'");
                    if (matchingRows.Length > 0)
                    {
                        foreach (DataRow row1 in matchingRows)
                        {

                            row1["CommunicationStatus"] = CommunicationStatus;
                            row1["GUID"] = GUID;
                            row1["ResentCreatedDate"] = CreatedOn;
                            row1["ResentVendorEmail"] = VendorEmail;
                        }
                    }


                }

                //---------------------------------------------------------------------------

                // -------------------72 hours datetime logic
                string awaitingSql = "";
                awaitingSql = @"Select CONCAT(PONum,+'-'+ [lineNo] + '-' +RelNo) As PONo, CreatedOn, VendorEmail from [SRM].[BuyerPO]
                    Where GUID NOT IN(Select GUID from [SRM].[VendorCommunication])";
                DataTable dtAwaiting = new DataTable();
                dtAwaiting = oDAL.GetData(awaitingSql);



                foreach (DataRow firstRow in dtAwaiting.Rows)
                {
                    string poNum = firstRow["PONo"].ToString();
                    DateTime createdDate = Convert.ToDateTime(firstRow["CreatedOn"]);
                    string VendorEmail = firstRow["VendorEmail"].ToString();
                    DateTime currentDate = DateTime.Now;

                    // Calculate the difference in hours
                    TimeSpan difference = createdDate - currentDate;

                    // If the PONum's created date is equal or greater than 72 hours ago
                    if (difference.TotalHours >= hours)
                    {
                        // Check if second DataTable contains this PONum and update
                        foreach (DataRow secondRow in dt.Rows)
                        {
                            string secondPONum = secondRow["PONumber"].ToString();
                            if (poNum == secondPONum)
                            {
                                secondRow["Resent"] = poNum;
                                secondRow["ResentCreatedDate"] = createdDate;
                                secondRow["ResentVendorEmail"] = VendorEmail;
                                break; // No need to continue looping
                            }
                        }
                    }
                    else
                    {
                        // Check if second DataTable contains this PONum and update
                        foreach (DataRow secondRow in dt.Rows)
                        {
                            string secondPONum = secondRow["PONumber"].ToString();
                            if (poNum == secondPONum)
                            {
                                secondRow["Awaiting"] = poNum;
                                secondRow["ResentCreatedDate"] = createdDate;
                                secondRow["ResentVendorEmail"] = VendorEmail;
                                break; // No need to continue looping
                            }
                        }
                    }
                }

                //---------------------------------------------------------------------------

                // Set Last Communication date

                DataTable dtLastComm = new DataTable();
                string LastCommSql = "";
                LastCommSql = @"WITH RankedPO AS (
            SELECT 
                PONo, 
                CreatedOn,
                ROW_NUMBER() OVER (PARTITION BY PONo ORDER BY CreatedOn DESC) AS RowNum
            FROM [SRM].[Transaction]
            WHERE HasAction <> 'Document'
        )
        SELECT 
            PONo, 
            CreatedOn
        FROM RankedPO
        WHERE RowNum = 1;";
                dtLastComm = oDAL.GetData(LastCommSql);

                foreach (DataRow firstRow in dtLastComm.Rows)
                {
                    string poNum = firstRow["PONo"].ToString();
                    DateTime createdDate = Convert.ToDateTime(firstRow["CreatedOn"]);

                    // Check if second DataTable contains this PONum and update
                    foreach (DataRow secondRow in dt.Rows)
                    {
                        string secondPONum = secondRow["PONumber"].ToString();
                        if (poNum == secondPONum)
                        {
                            secondRow["LastCommunication"] = createdDate;
                            break; // No need to continue looping
                        }
                    }

                }

                // Set the ERP Status based on conditions
                foreach (DataRow row in dt.Rows)
                {
                    string po = row["POHeader_PONum"].ToString();
                    string Line = row["PODetail_POLine"].ToString();
                    // Handle DateTime fields with null checks
                    DateTime? dueDate = row.IsNull("Calculated_DueDate") ? (DateTime?)null : row.Field<DateTime>("Calculated_DueDate");
                    DateTime? arrivedDate = row.IsNull("Calculated_ArrivedDate") ? (DateTime?)null : Convert.ToDateTime(row["Calculated_ArrivedDate"]);

                    // Parse quantity columns with null and conversion checks
                    string arrivedQtyStr = row.IsNull("Calculated_ArrivedQty") ? "0" : row["Calculated_ArrivedQty"].ToString().Replace(",", "");
                    decimal decimalArrivedValue;
                    int arrivedQty = 0;

                    if (decimal.TryParse(arrivedQtyStr, out decimalArrivedValue))
                    {
                        arrivedQty = (int)decimalArrivedValue; // Convert decimal to int
                    }

                    string receivedQtyStr = row.IsNull("Calculated_ReceivedQty") ? "0" : row["Calculated_ReceivedQty"].ToString().Replace(",", "");
                    decimal decimalReceivedValue;
                    int receivedQty = 0;

                    if (decimal.TryParse(receivedQtyStr, out decimalReceivedValue))
                    {
                        receivedQty = (int)decimalReceivedValue; // Convert decimal to int
                    }

                    string orderQtyStr = row.IsNull("PODetail_OrderQty") ? "0" : row["PODetail_OrderQty"].ToString().Replace(",", "");
                    decimal decimalOrderValue;
                    int orderQty = 0;

                    if (decimal.TryParse(orderQtyStr, out decimalOrderValue))
                    {
                        orderQty = (int)decimalOrderValue; // Convert decimal to int
                    }

                    // Set ERP Status column Value with null checks
                    if (dueDate.HasValue)
                    {
                        DateTime compareDate = arrivedDate ?? DateTime.Now; // Use arrivedDate if available, otherwise DateTime.Now

                        if (compareDate > dueDate.Value)
                        {
                            if (arrivedQty == orderQty)
                                row["ERPStatus"] = "Late PI"; // Pending Inspection
                            else if (receivedQty == orderQty)
                                row["ERPStatus"] = "Late R"; // Received
                            else if (arrivedQty < orderQty)
                                row["ERPStatus"] = "Late PRPI"; // Partially rec. pending Inspection
                            else if (receivedQty < orderQty)
                                row["ERPStatus"] = "Late PR"; // Partially Rec.
                            else
                                row["ERPStatus"] = "Late";
                        }
                        else
                        {
                            row["ERPStatus"] = "On Time";
                        }
                    }
                    else
                    {
                        // If due date is not available, we cannot evaluate late/on time
                        row["ERPStatus"] = "Unknown";
                    }
                }


                DataView dv = dt.DefaultView;
                dv.Sort = "ResentCreatedDate DESC"; // You can use DESC for descending order
                DataTable sortedDt = dv.ToTable();
                dt = sortedDt;

                //--------------- Filter By Status
                if (POStatus == "Late")
                {
                    if (oDAL.HasErrors)
                    {
                        ErrorMessage = oDAL.ErrMessage;
                        return false;
                    }
                    else
                    {
                        lstPO = cCommon.ConvertDtToHashTable(dt); // Non-matching rows
                        return true;
                    }
                }
                else if (POStatus == "All Open")
                {
                    if (oDAL.HasErrors)
                    {
                        ErrorMessage = oDAL.ErrMessage;
                        return false;
                    }
                    else
                    {
                        lstPO = cCommon.ConvertDtToHashTable(dt);
                        return true;
                    }
                }
                else if (POStatus == "Pending")
                {
                    if (oDAL.HasErrors)
                    {
                        ErrorMessage = oDAL.ErrMessage;
                        return false;
                    }
                    else
                    {
                        lstPO = cCommon.ConvertDtToHashTable(dt);
                        return true;
                    }
                }
                else if (POStatus == "Awaiting Response")
                {
                    DataTable table1 = new DataTable();
                    string sql = @"select * from [SRM].[BuyerPO]
        Where GUID NOT IN(Select GUID from [SRM].[VendorCommunication]) ";
                    table1 = oDAL.GetData(sql);
                    // Create a new datatable to store matching rows
                    DataTable matchingRows = dt.Clone(); // Clone the structure of dt2
                    if (table1.Rows.Count > 0)
                    {
                        foreach (DataRow row1 in table1.Rows)
                        {
                            string PONo = (string)row1["PONum"];
                            string PartNo = (string)row1["PartNo"];
                            string LineNo = (string)row1["LineNo"];
                            string RelNo = (string)row1["RelNo"];
                            // Check if the parts match any row in dt2
                            DataRow[] foundRows = dt.Select($"POHeader_PONum = '{PONo}' AND PODetail_POLine = '{LineNo}' AND PORel_PORelNum = '{RelNo}' AND PODetail_PartNum = '{PartNo}' AND PODetail_OrderQty <> Calculated_ArrivedQty ");

                            if (foundRows.Length > 0)
                            {
                                foreach (DataRow foundRow in foundRows)
                                {
                                    matchingRows.ImportRow(foundRow);
                                }
                            }

                        }

                    }
                    if (oDAL.HasErrors)
                    {
                        ErrorMessage = oDAL.ErrMessage;
                        return false;
                    }
                    else
                    {
                        if (matchingRows.Rows.Count > 0)
                            lstPO = cCommon.ConvertDtToHashTable(matchingRows);

                        return true;
                    }
                }
                else if (POStatus == "Arrived")
                {
                    if (oDAL.HasErrors)
                    {
                        ErrorMessage = oDAL.ErrMessage;
                        return false;
                    }
                    else
                    {
                        lstPO = cCommon.ConvertDtToHashTable(dt);
                        return true;
                    }
                }
                else if (POStatus == "Widget")
                {
                    if (dt.Rows.Count > 0)
                    {
                        lstPO = cCommon.ConvertDtToHashTable(dt);
                    }
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        lstPO = cCommon.ConvertDtToHashTable(dt);
                    }

                }
                    return true;
                
            }
            //---------------------------------------------------------------------------

            catch (Exception ex)
            {
                cLog oLog;
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "PO Detail Main method");
                return false;
            }
        }


        public void Get_NewWidgetAPI(DataTable dt, string email, string userType)
        {
            string query = string.Empty;
            //object Result;
            cDAL oDAL;
            DataTable dtAllOpen = new DataTable();

            oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            if (dt.Columns.Contains("CommunicationStatus"))
            {
                dt.Columns.Remove("CommunicationStatus");
            }

            if (dt.Columns.Contains("PONumber"))
            {
                dt.Columns.Remove("PONumber");
            }

            dt.Columns.Add("CommunicationStatus");
            DataColumn concatenatedColumn = new DataColumn("PONumber", typeof(string));
            dt.Columns.Add(concatenatedColumn);
            foreach (DataRow row in dt.Rows)
            {
                row["PONumber"] = $"{row["POHeader_PONum"]}-{row["PODetail_POLine"]}-{row["PORel_PORelNum"]}";
                if (row["PODetail_OrderQty"] == row["Calculated_ArrivedQty"])
                    row["CommunicationStatus"] = "Completed";
                else
                    row["CommunicationStatus"] = "New";

            }
            string Buyersql = "";
            DataTable DtbuyerEmail = new DataTable();
            Buyersql = @"select CONCAT(PONum,+'-'+ [lineNo] + '-' +RelNo) As PONo, CommunicationStatus
                        from[SRM].[BuyerPO] ";
            DtbuyerEmail = oDAL.GetData(Buyersql);
            foreach (DataRow row2 in DtbuyerEmail.Rows)
            {
                string poNo = row2["PONo"].ToString();
                string CommunicationStatus = row2["CommunicationStatus"].ToString();

                DataRow[] matchingRows = dt.Select($"PONumber = '{poNo}'");
                if (matchingRows.Length > 0)
                {
                    foreach (DataRow row1 in matchingRows)
                    {
                        row1["CommunicationStatus"] = CommunicationStatus;
                    }
                }


            }

            // All PO

            if (userType.ToUpper() == "BUYER")
            {
                DataTable dtAllPO = dt.Clone(); // Clone structure (columns)
                var overdueRows = from row in dt.AsEnumerable()
                                  where string.Equals(row["PurAgent_EMailAddress"].ToString(), email, StringComparison.OrdinalIgnoreCase)
                                  select row;
                // Populate the overdueDataTable with the overdue rows
                foreach (var row in overdueRows)
                {
                    dtAllPO.ImportRow(row);
                }
                AllPO = dtAllPO.Rows.Count.ToString();
            }
            else if (userType.ToUpper() == "SUPPLIER")
            {
                DataTable dtAllPO = dt.Clone(); // Clone structure (columns)
                var overdueRows = from row in dt.AsEnumerable()
                                  where string.Equals(row["Vendor_EMailAddress"].ToString(), email, StringComparison.OrdinalIgnoreCase)
                                  select row;
                // Populate the overdueDataTable with the overdue rows
                foreach (var row in overdueRows)
                {
                    dtAllPO.ImportRow(row);
                }
                AllPO = dtAllPO.Rows.Count.ToString();
            }

            else
            {
                //DataTable dtAllPO = dt.Clone(); // Clone structure (columns)
                //var overdueRows = from row in dt.AsEnumerable()
                //                  where Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"]) 
                //                  select row;
                //// Populate the overdueDataTable with the overdue rows
                //foreach (var row in overdueRows)
                //{
                //    dtAllPO.ImportRow(row);
                //}
                AllPO = dt.Rows.Count.ToString();
            }


            //All Open
            if (userType.ToUpper() == "BUYER")
            {
                dtAllOpen = dt.Clone(); // Clone structure (columns)
                var allOpenRows = from row in dt.AsEnumerable()
                                  where (row.IsNull("Calculated_ArrivedDate")  // Include records where ArrivedDate is null
                                         || Convert.ToDateTime(row["Calculated_ArrivedDate"]) < Convert.ToDateTime(row["Calculated_DueDate"]))
                                        && !row.IsNull("Calculated_DueDate") // Ensure DueDate is not null
                                                                             //&& Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                      && string.Equals(row["PurAgent_EMailAddress"].ToString(), email, StringComparison.OrdinalIgnoreCase)
                                        && row["CommunicationStatus"].ToString() == "New"
                                  select row;
                // Populate the overdueDataTable with the overdue rows
                foreach (var row in allOpenRows)
                {
                    dtAllOpen.ImportRow(row);
                }
                AllOpen = dtAllOpen.Rows.Count.ToString();
            }
            else if (userType.ToUpper() == "SUPPLIER")
            {
                dtAllOpen = dt.Clone(); // Clone structure (columns)
                var allOpenRows = from row in dt.AsEnumerable()
                                  where Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"]) &&
                                  string.Equals(row["Vendor_EMailAddress"].ToString(), email, StringComparison.OrdinalIgnoreCase) &&
                                  row["CommunicationStatus"].ToString() == "New"
                                  select row;
                // Populate the overdueDataTable with the overdue rows
                foreach (var row in allOpenRows)
                {
                    dtAllOpen.ImportRow(row);
                }
                AllOpen = dtAllOpen.Rows.Count.ToString();
            }
            else
            {
                dtAllOpen = dt.Clone(); // Clone structure (columns)
                var allOpenRows = from row in dt.AsEnumerable()
                                  where (row.IsNull("Calculated_ArrivedDate")  // Include records where ArrivedDate is null
                                         || Convert.ToDateTime(row["Calculated_ArrivedDate"]) < Convert.ToDateTime(row["Calculated_DueDate"]))
                                        && !row.IsNull("Calculated_DueDate") // Ensure DueDate is not null
                                                                             //&& Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                        && row["CommunicationStatus"].ToString() == "New"
                                  select row;
                // Populate the overdueDataTable with the overdue rows
                foreach (var row in allOpenRows)
                {
                    dtAllOpen.ImportRow(row);
                }
                AllOpen = dtAllOpen.Rows.Count.ToString();
            }


            //Pending

            DataTable table1 = new DataTable();
            string sql = @"select * from [SRM].[BuyerPO] ";

            if (userType.ToUpper() == "BUYER")
            {
                sql += "Where BuyerEmail = '" + email + "'";
                table1 = oDAL.GetData(sql);
                DataTable dtPending = dt.Clone(); // Clone structure (columns)
                var overdueRows = from row in dt.AsEnumerable()
                                  where Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                  select row;

                // Populate the overdueDataTable with the overdue rows
                foreach (var row in overdueRows)
                {
                    dtPending.ImportRow(row);
                }


                table1 = oDAL.GetData(sql);

                // Create a new DataTable to store matching rows
                DataTable matchingRows = dtPending.Clone(); // Clone the structure of dtPending

                // Iterate through rows of dtPending
                foreach (DataRow row2 in dtPending.Rows)
                {
                    // Check if the row in dtPending has a matching record in table1
                    bool isMatching = table1.AsEnumerable()
                                            .Any(row1 => row2["POHeader_PONum"].ToString() == row1["PONum"].ToString() &&
                                                         row2["PODetail_POLine"].ToString() == row1["LineNo"].ToString() &&
                                                         row2["PORel_PORelNum"].ToString() == row1["RelNo"].ToString());

                    // If it's matching, add the row to matchingRows
                    if (isMatching)
                    {
                        matchingRows.ImportRow(row2);
                    }
                }

                Pending = matchingRows.Rows.Count.ToString();
            }
            else if (userType.ToUpper() == "SUPPLIER")
            {
                sql += "Where VendorEmail = '" + email + "'";
                table1 = oDAL.GetData(sql);
                DataTable dtPending = dt.Clone(); // Clone structure (columns)
                var overdueRows = from row in dt.AsEnumerable()
                                  where Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                  select row;

                // Populate the overdueDataTable with the overdue rows
                foreach (var row in overdueRows)
                {
                    dtPending.ImportRow(row);
                }


                table1 = oDAL.GetData(sql);

                // Create a new DataTable to store matching rows
                DataTable matchingRows = dtPending.Clone(); // Clone the structure of dtPending

                // Iterate through rows of dtPending
                foreach (DataRow row2 in dtPending.Rows)
                {
                    // Check if the row in dtPending has a matching record in table1
                    bool isMatching = table1.AsEnumerable()
                                            .Any(row1 => row2["POHeader_PONum"].ToString() == row1["PONum"].ToString() &&
                                                         row2["PODetail_POLine"].ToString() == row1["LineNo"].ToString() &&
                                                         row2["PORel_PORelNum"].ToString() == row1["RelNo"].ToString());

                    // If it's matching, add the row to matchingRows
                    if (isMatching)
                    {
                        matchingRows.ImportRow(row2);
                    }
                }

                Pending = matchingRows.Rows.Count.ToString();
            }
            else
            {
                DataTable dtPending = dt.Clone(); // Clone structure (columns)
                var overdueRows = from row in dt.AsEnumerable()
                                  where Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                  select row;

                // Populate the overdueDataTable with the overdue rows
                foreach (var row in overdueRows)
                {
                    dtPending.ImportRow(row);
                }


                table1 = oDAL.GetData(sql);

                // Create a new DataTable to store matching rows
                DataTable matchingRows = dtPending.Clone(); // Clone the structure of dtPending

                // Iterate through rows of dtPending
                foreach (DataRow row2 in dtPending.Rows)
                {
                    // Check if the row in dtPending has a matching record in table1
                    bool isMatching = table1.AsEnumerable()
                                            .Any(row1 => row2["POHeader_PONum"].ToString() == row1["PONum"].ToString() &&
                                                         row2["PODetail_POLine"].ToString() == row1["LineNo"].ToString() &&
                                                         row2["PORel_PORelNum"].ToString() == row1["RelNo"].ToString());

                    // If it's matching, add the row to matchingRows
                    if (isMatching)
                    {
                        matchingRows.ImportRow(row2);
                    }
                }

                Pending = matchingRows.Rows.Count.ToString();
            }


            //Late
            if (userType.ToUpper() == "BUYER")
            {
                // Create a new DataTable to store the overdue records
                DataTable overdueDataTable = dt.Clone(); // Clone structure (columns)

                // Query to get rows where ArrivedDate is greater than DueDate
                var overdueRows = from row in dt.AsEnumerable()
                                  where !row.IsNull("Calculated_ArrivedDate") && !row.IsNull("Calculated_DueDate")
                                        && Convert.ToDateTime(row["Calculated_ArrivedDate"]) > Convert.ToDateTime(row["Calculated_DueDate"])
                                        && !row.IsNull("PODetail_OrderQty") && !row.IsNull("Calculated_ArrivedQty")
                                        && Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                        && string.Equals(row["PurAgent_EMailAddress"].ToString(), email, StringComparison.OrdinalIgnoreCase)
                                  select row;

                // Populate the overdueDataTable with the overdue rows
                foreach (var row in overdueRows)
                {
                    overdueDataTable.ImportRow(row);
                }
                table1 = new DataTable();
                sql = @"select * from [SRM].[BuyerPO] Order By CreatedOn Desc";
                table1 = oDAL.GetData(sql);

                // Create a new DataTable to store non-matching rows
                DataTable NotmatchingRows = overdueDataTable.Clone(); // Clone the structure of overdueDataTable
                //int COUNT = overdueDataTable.Rows.Count;
                // Iterate through rows of overdueDataTable
                foreach (DataRow row2 in overdueDataTable.Rows)
                {
                    // Check if the row in overdueDataTable exists in table1
                    bool isMatching = table1.AsEnumerable()
                                            .Any(row1 => row2["POHeader_PONum"].ToString() == row1["PONum"].ToString() &&
                                                         row2["PODetail_POLine"].ToString() == row1["LineNo"].ToString() &&
                                                         row2["PORel_PORelNum"].ToString() == row1["RelNo"].ToString());

                    // If it's not matching, add the row to NotmatchingRows
                    if (!isMatching) // This ensures that we only add rows that do not match
                    {
                        NotmatchingRows.ImportRow(row2);
                    }
                }
                Late = NotmatchingRows.Rows.Count.ToString();
            }
            else if (userType.ToUpper() == "SUPPLIER")
            {
                // Create a new DataTable to store the overdue records
                DataTable overdueDataTable = dt.Clone(); // Clone structure (columns)

                // Query to get rows where ArrivedDate is greater than DueDate
                var overdueRows = from row in dt.AsEnumerable()
                                  where Convert.ToDateTime(row["Calculated_ArrivedDate"]) > Convert.ToDateTime(row["Calculated_DueDate"])
                                  && Convert.ToDecimal(row["PODetail_OrderQty"]) != Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                  && string.Equals(row["Vendor_EMailAddress"].ToString(), email, StringComparison.OrdinalIgnoreCase)
                                  select row;

                // Populate the overdueDataTable with the overdue rows
                foreach (var row in overdueRows)
                {
                    overdueDataTable.ImportRow(row);
                }
                table1 = new DataTable();
                sql = @"select * from [SRM].[BuyerPO] Order By CreatedOn Desc";
                table1 = oDAL.GetData(sql);

                // Create a new DataTable to store non-matching rows
                DataTable NotmatchingRows = overdueDataTable.Clone(); // Clone the structure of overdueDataTable
                //int COUNT = overdueDataTable.Rows.Count;
                // Iterate through rows of overdueDataTable
                foreach (DataRow row2 in overdueDataTable.Rows)
                {
                    // Check if the row in overdueDataTable exists in table1
                    bool isMatching = table1.AsEnumerable()
                                            .Any(row1 => row2["POHeader_PONum"].ToString() == row1["PONum"].ToString() &&
                                                         row2["PODetail_POLine"].ToString() == row1["LineNo"].ToString() &&
                                                         row2["PORel_PORelNum"].ToString() == row1["RelNo"].ToString());

                    // If it's not matching, add the row to NotmatchingRows
                    if (!isMatching) // This ensures that we only add rows that do not match
                    {
                        NotmatchingRows.ImportRow(row2);
                    }
                }
                Late = NotmatchingRows.Rows.Count.ToString();
            }
            else
            {
                // Create a new DataTable to store the overdue records
                DataTable overdueDataTable = dt.Clone(); // Clone structure (columns)

                // Query to get rows where ArrivedDate is greater than DueDate
                var overdueRows = from row in dt.AsEnumerable()
                                  let arrivedDate = row["Calculated_ArrivedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["Calculated_ArrivedDate"])
                                  let dueDate = row["Calculated_DueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["Calculated_DueDate"])
                                  let orderQty = row["PODetail_OrderQty"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["PODetail_OrderQty"])
                                  let arrivedQty = row["Calculated_ArrivedQty"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                  where arrivedDate.HasValue && dueDate.HasValue && orderQty.HasValue && arrivedQty.HasValue
                                        && arrivedDate.Value > dueDate.Value
                                        && orderQty.Value != arrivedQty.Value
                                  select row;


                // Populate the overdueDataTable with the overdue rows
                foreach (var row in overdueRows)
                {
                    overdueDataTable.ImportRow(row);
                }
                table1 = new DataTable();
                sql = @"select * from [SRM].[BuyerPO] Order By CreatedOn Desc";
                table1 = oDAL.GetData(sql);

                // Create a new DataTable to store non-matching rows
                DataTable NotmatchingRows = overdueDataTable.Clone(); // Clone the structure of overdueDataTable
                //int COUNT = overdueDataTable.Rows.Count;
                // Iterate through rows of overdueDataTable
                foreach (DataRow row2 in overdueDataTable.Rows)
                {
                    // Check if the row in overdueDataTable exists in table1
                    bool isMatching = table1.AsEnumerable()
                                            .Any(row1 => row2["POHeader_PONum"].ToString() == row1["PONum"].ToString() &&
                                                         row2["PODetail_POLine"].ToString() == row1["LineNo"].ToString() &&
                                                         row2["PORel_PORelNum"].ToString() == row1["RelNo"].ToString());

                    // If it's not matching, add the row to NotmatchingRows
                    if (!isMatching) // This ensures that we only add rows that do not match
                    {
                        NotmatchingRows.ImportRow(row2);
                    }
                }
                Late = NotmatchingRows.Rows.Count.ToString();
            }


            //Arrived
            if (userType.ToUpper() == "BUYER")
            {
                // Create a new DataTable to store the overdue records
                // Handle arrived records
                DataTable ArriveDt = dt.Clone(); // Clone structure (columns)

                // Query to get rows where ArrivedDate is less than DueDate
                var arrivedRows = from row in dt.AsEnumerable()
                                  where !row.IsNull("Calculated_ArrivedDate") && !row.IsNull("Calculated_DueDate")
                                  && Convert.ToDateTime(row["Calculated_ArrivedDate"]) < Convert.ToDateTime(row["Calculated_DueDate"])
                                  && !row.IsNull("PODetail_OrderQty") && !row.IsNull("Calculated_ArrivedQty")
                                  && Convert.ToDecimal(row["PODetail_OrderQty"]) == Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                  && string.Equals(row["PurAgent_EMailAddress"].ToString(), email, StringComparison.OrdinalIgnoreCase)
                                  select row;

                // Populate the ArriveDt with the arrived rows
                foreach (var row in arrivedRows)
                {
                    ArriveDt.ImportRow(row);
                }
                Arrived = ArriveDt.Rows.Count.ToString();
            }
            else if (userType.ToUpper() == "SUPPLIER")
            {
                // Create a new DataTable to store the overdue records
                DataTable ArriveDt = dt.Clone(); // Clone structure (columns)

                // Query to get rows where ArrivedDate is greater than DueDate
                var arrivedRows = from row in dt.AsEnumerable()
                                  where Convert.ToDateTime(row["Calculated_ArrivedDate"]) < Convert.ToDateTime(row["Calculated_DueDate"])
                                  && Convert.ToDecimal(row["PODetail_OrderQty"]) == Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                  && string.Equals(row["Vendor_EMailAddress"].ToString(), email, StringComparison.OrdinalIgnoreCase)
                                  select row;

                // Populate the overdueDataTable with the overdue rows
                foreach (var row in arrivedRows)
                {
                    ArriveDt.ImportRow(row);
                }
                Arrived = ArriveDt.Rows.Count.ToString();
            }
            else
            {
                // Create a new DataTable to store the overdue records
                DataTable ArriveDt = dt.Clone(); // Clone structure (columns)

                // Query to get rows where ArrivedDate is greater than DueDate
                var arrivedRows = from row in dt.AsEnumerable()
                                  let arrivedDate = row["Calculated_ArrivedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["Calculated_ArrivedDate"])
                                  let dueDate = row["Calculated_DueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["Calculated_DueDate"])
                                  let orderQty = row["PODetail_OrderQty"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["PODetail_OrderQty"])
                                  let arrivedQty = row["Calculated_ArrivedQty"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["Calculated_ArrivedQty"])
                                  where arrivedDate.HasValue && dueDate.HasValue && orderQty.HasValue && arrivedQty.HasValue
                                        && arrivedDate.Value < dueDate.Value
                                        && orderQty.Value == arrivedQty.Value
                                  select row;


                // Populate the overdueDataTable with the overdue rows
                foreach (var row in arrivedRows)
                {
                    ArriveDt.ImportRow(row);
                }
                Arrived = ArriveDt.Rows.Count.ToString();
            }

            //Update

            query = @"WITH RankedRecords AS (
    SELECT 
        B.GUID, 
        CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo) AS PONo, 
        B.PartNo, 
        B.PartDesc,
        B.VendorName, 
        B.OrderQty, 
        B.Qty AS CurrentQty, 
        B.Price AS CurrentPrice,
        B.DueDate AS CurrentDueDate, 
        V.Qty AS CommitQty, 
        V.Price AS CommitPrice,
        V.DueDate AS CommitDueDate, 
        V.TrackingNo, 
        V.AttachFile, 
        V.FileExt,
        V.CreatedOn,
 
        ROW_NUMBER() OVER (PARTITION BY CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo) ORDER BY V.CreatedOn DESC) AS RowNum
    FROM 
        [SRM].[BuyerPO] B
    JOIN 
        [SRM].[VendorCommunication] V 
        ON B.GUID = V.GUID 
        AND B.VendorName = V.VendorName  
        AND CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo) = V.PONo
    WHERE 
        B.CommunicationStatus = 'Completed' 
         <BuyerSupplierClasue>
)
SELECT 
    PONo, 
    GUID, 
    PartNo, 
    PartDesc,
    VendorName, 
    OrderQty, 
    CurrentQty, 
    CurrentPrice,
    CurrentDueDate, 
    CommitQty, 
    CommitPrice,
    CommitDueDate, 
    TrackingNo, 
    AttachFile, 
    FileExt
FROM 
    RankedRecords
WHERE 
    RowNum = 1
ORDER BY 
    CreatedOn DESC; ";

            if (userType.ToUpper() == "BUYER")
                query = query.Replace("<BuyerSupplierClasue>", " AND V.BuyerEmail = '" + email + "' ");
            else if (userType == "SUPPLIER")
                query = query.Replace("<BuyerSupplierClasue>", " AND V.VendorEmail = '" + email + "' ");
            else
                query = query.Replace("<BuyerSupplierClasue>", " ");

            DataTable dtUpdate = oDAL.GetData(query);
            if (dtUpdate.Rows.Count > 0)
                Update = dtUpdate.Rows.Count.ToString();
            else
                Update = "0";
        }

        public bool GetSelectedList(DataTable dt)
        {
            SelectedlstPO = cCommon.ConvertDtToHashTable(dt);
            return true;
        }
        public int GetHours()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            object result = null;
            string sql = "select SysValue from [dbo].[zSysIni] Where SysDesc = 'Hours' ";
            result = oDAL.GetObject(sql);
            if (result != null)
                return Convert.ToInt32(result);
            else
                return 0;

        }

        public void InsertBuyerCommunication(string POSender, string POReceiever, string POSubject, string POBody, string GUID, string POCreatedBy)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = @"INSERT INTO [SRM].[BuyerCommunication]
           ([Sender]
           ,[Receiver]
           ,[Subject]
           ,[Body]
           ,[GUID]
           ,[CreatedBy]
)
     VALUES
           ('<Sender>'
           ,'<Receiver>'
           ,'<Subject>'
           ,'<Body>'
           ,'<GUID>'
           ,'<CreatedBy>'
)";

            sql = sql.Replace("<Sender>", POSender);
            sql = sql.Replace("<Receiver>", POReceiever);
            sql = sql.Replace("<Subject>", POSubject);
            POBody = POBody.Replace("'", "''");
            sql = sql.Replace("<Body>", POBody);
            sql = sql.Replace("<GUID>", GUID.ToString());
            sql = sql.Replace("<CreatedBy>", POCreatedBy);

            oDAL.Execute(sql);
        }

        public bool AddInBuyers(string PONo, string POline, string PORelNo, string POPartNo, string POpartDesc, string POQty,
            string POprice, string POUOM, string PODueDate, string POOrderDate, string POBuyer,
            string POVendorId, string POVendorName, string POVendorEmail, string POBuyerId, string POContactReason, string POCreatedBy, string newGUID, string message, bool isUpdate, string NewDueDate, string OrderQty, string TrackingNo, string BuyerEmail, string SupplierCompany)
        {
            string sql = "";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            // Escape single quotes
            string escapedMessage = message.Replace("'", "''");
            string escapedPOVendorName = POVendorName.Replace("'", "''");

            if (!isUpdate)
            {
                sql = @"INSERT INTO [SRM].[BuyerPO]
           ([PONum]
           ,[LineNo]
           ,[RelNo]
           ,[PartNo]
           ,[PartDesc]
           ,[Qty]
           ,[Price]
           ,[UOM]
           ,[DueDate]
           ,[OrderDate]
           ,[POStatus]
           ,[Buyer]
           ,[VendorId]
           ,[VendorName]
           ,[VendorEmail]
           ,[BuyerId]
           ,[ContactReason]
           ,[Message]
           ,[OrgId]
           ,[GUID]
           ,[CreatedBy]
           ,[CommunicationStatus]
           ,[NewDueDate]
           ,[OrderQty]
           ,[BuyerEmail]
           
		   )
     VALUES
           ('<PONum>'
           ,'<LineNo>'
           ,'<RelNo>'
           ,'<PartNo>'
           ,'<PartDesc>'
           ,'<Qty>'
           ,'<Price>'
           ,'<UOM>'
           ,'<DueDate>'
           ,'<OrderDate>'
           ,'<POStatus>'
           ,'<Buyer>'
           ,'<VendorId>'
           ,'<VendorName>'
           ,'<VendorEmail>'
           ,'<BuyerId>'
           ,'<ContactReason>'
           ,'<Message>'
           ,'<OrgId>'
           ,'<GUID>'
           ,'<CreatedBy>'
           ,'<CommunicationStatus>'
           ,'<NewDueDate>'
           ,'<OrderQty>'
           ,'<BuyerEmail>'
           

		   )";


            }
            else
            {
                sql = @"UPDATE [SRM].[BuyerPO]
                   SET [PONum] = '<PONum>'
                      ,[LineNo] = '<LineNo>'
                      ,[RelNo] = '<RelNo>'
                      ,[PartNo] = '<PartNo>'
                      ,[PartDesc] = '<PartDesc>'
                      ,[Qty] = '<Qty>'
                      ,[Price] = '<Price>'
                      ,[UOM] = '<UOM>'
                      ,[DueDate] = '<DueDate>'
                      ,[OrderDate] = '<OrderDate>'
                      ,[POStatus] = '<POStatus>'
                      ,[Buyer] = '<Buyer>'
                      ,[VendorId] = '<VendorId>'
                      ,[VendorName] = '<VendorName>'
                      ,[VendorEmail] = '<VendorEmail>'
                      ,[BuyerId] = '<BuyerId>'
                      ,[ContactReason] = '<ContactReason>'
                      ,[Message] = '<Message>'
                      ,[OrgId] = '<OrgId>'
                      ,[GUID] = '<GUID>'
                      ,[HasAction] = '<HasAction>'
                      ,[CreatedBy] = '<CreatedBy>'
                      ,[CommunicationStatus] = '<CommunicationStatus>'
                      ,[NewDueDate] = '<NewDueDate>'
                      ,[OrderQty] = '<OrderQty>'
                      ,[BuyerEmail] = '<BuyerEmail>'
                      
                 WHERE GUID = '<GUID>' AND PONum = '<PONum>' AND [LineNo] = '<LineNo>' AND RelNo = '<RelNo>' ";
            }
            // Convert string to DateTime using DateTime.Parse
            //DateTime NewDatetime = new DateTime();
            DateTime ConvertedNewDueDate = new DateTime();
            if (!string.IsNullOrEmpty(NewDueDate))
            {
                ConvertedNewDueDate = cCommon.ConvertToDateOnly(NewDueDate);
            }

            DateTime ConvertedDueDate = cCommon.ConvertToDateOnly(PODueDate);

            DateTime ConvertedOrderDate = DateTime.MinValue; // Better default than 'new DateTime()'

            if (!string.IsNullOrEmpty(POOrderDate))
                ConvertedOrderDate = cCommon.ConvertToDateOnly(POOrderDate);


            double Qtyvalue = Convert.ToDouble(POQty);
            double POFinalPrice = 0;
            //double PriceValue = 0;
            if (!string.IsNullOrEmpty(POprice))
            {
                POFinalPrice = double.Parse(POprice);
            }

            //double tackingNo = Convert.ToDouble(TrackingNo);
            //int finalTrackingNo = (int)tackingNo;

            double FinalOrderQty = double.Parse(OrderQty);

            int Quantity = (int)Qtyvalue;



            string[] _PO = PONo.Split('-');
            sql = sql.Replace("<PONum>", _PO[0]);
            sql = sql.Replace("<LineNo>", POline);
            sql = sql.Replace("<RelNo>", PORelNo);
            sql = sql.Replace("<PartNo>", POPartNo);
            sql = sql.Replace("<PartDesc>", POpartDesc);
            sql = sql.Replace("<Qty>", Qtyvalue.ToString());
            sql = sql.Replace("<Price>", POFinalPrice.ToString());
            sql = sql.Replace("<UOM>", POUOM);
            sql = sql.Replace("<DueDate>", ConvertedDueDate.ToString());
            sql = sql.Replace("<OrderDate>", "");
            sql = sql.Replace("<POStatus>", "New");
            sql = sql.Replace("<Buyer>", POBuyer);
            sql = sql.Replace("<VendorId>", POVendorId);
            sql = sql.Replace("<VendorName>", escapedPOVendorName);
            sql = sql.Replace("<VendorEmail>", POVendorEmail);

            sql = sql.Replace("<BuyerId>", POBuyerId);
            sql = sql.Replace("<ContactReason>", POContactReason);
            sql = sql.Replace("<Message>", escapedMessage);
            sql = sql.Replace("<OrgId>", SupplierCompany);
            sql = sql.Replace("<GUID>", newGUID.ToString());
            sql = sql.Replace("<HasAction>", "");
            sql = sql.Replace("<CreatedBy>", POCreatedBy);
            sql = sql.Replace("<CommunicationStatus>", "Awaiting");
            sql = sql.Replace("<NewDueDate>", ConvertedNewDueDate.ToString("MM/dd/yyyy"));
            sql = sql.Replace("<OrderQty>", FinalOrderQty.ToString());
            sql = sql.Replace("<BuyerEmail>", BuyerEmail);
            //sql = sql.Replace("<TrackingNo>", finalTrackingNo.ToString());

            oDAL.Execute(sql);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                UpdateVendorTable(PONo, newGUID.ToString());
                // Insert in Transaction table
                AddInTransaction(PONo, POPartNo, newGUID.ToString(), "New", Qtyvalue.ToString(), POFinalPrice.ToString(), ConvertedDueDate.ToString(), POCreatedBy, message);

                return true;
            }

        }

        public void AddInTransaction(string tPO, string tpartNo, string tGUID, string tHasAction, string tQty, string tprice, string tDueDate, string tcreatedBy, string tMessage)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = "";
            sql = @"INSERT INTO [SRM].[Transaction]
           ([PONo]
           ,[PartNo]
           ,[Type]
           ,[GUID]
           ,[HasAction]
           ,[Qty]
           ,[Price]
           ,[DueDate]
           ,[Message]
		   ,[CreatedBy]
           ,[CreatedOn]
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
           ,'<Message>'
		   ,'<CreatedBy>'
           ,'<CreatedOn>'
		   )";


            sql = sql.Replace("<PONo>", tPO);
            sql = sql.Replace("<PartNo>", tpartNo);
            sql = sql.Replace("<Type>", "Buyer");
            sql = sql.Replace("<GUID>", tGUID);
            sql = sql.Replace("<HasAction>", tHasAction);
            sql = sql.Replace("<Qty>", tQty);
            sql = sql.Replace("<Price>", tprice);
            sql = sql.Replace("<DueDate>", tDueDate);
            sql = sql.Replace("<Message>", tMessage);
            sql = sql.Replace("<CreatedBy>", tcreatedBy);

            string timezone = HttpContext.Current.Session["TimeZone"].ToString();

            DateTime utcTime = DateTime.UtcNow;

            // Get the TimeZoneInfo object for the selected timezone
            TimeZoneInfo selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);

            // Get the offset of the selected timezone from UTC
            TimeSpan offset = selectedTimeZone.GetUtcOffset(utcTime);
            DateTime localTime = utcTime + offset;

            sql = sql.Replace("<CreatedOn>", localTime.ToString());
            oDAL.Execute(sql);
        }


        public DateTime ConvertToUtc(DateTime localDateTime, string timezoneId)
        {
            TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

            // If the DateTime's Kind is not already Unspecified or Utc, set it to Unspecified
            if (localDateTime.Kind != DateTimeKind.Unspecified)
            {
                localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
            }

            // Convert the local time (assuming it's in the specified timezone) to UTC
            DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timezone);
            return utcDateTime;
        }
        public DateTime ConvertFromUtc(DateTime utcDateTime, string timezoneId)
        {
            // Ensure the DateTimeKind is Utc before converting
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timezone);
            return localDateTime;
        }

        public bool GetUpdateData(string PONo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            DataTable dt = new DataTable();

            string sql = "";
            sql = @"Select B.GUID, CONCAT(B.PONum,'-', B.[LineNo],'-', B.RelNo) PONo, B.PartNo, B.PartDesc,B.VendorName, B.Qty CurrentQty, B.Price CurrentPrice,B.DueDate CurrentDueDate, V.Qty CommitQty, V.Price CommitPrice,
V.DueDate CommitDueDate, V.TrackingNo, V.ServiceType, V.ServiceURL, V.AttachFile, V.FileExt
from [SRM].[BuyerPO] B
LEFT JOIN [SRM].[VendorCommunication] V ON B.GUID = V.GUID  AND CONCAT(B.PONum,'-', B.[LineNo],'-', B.RelNo) = V.PONo
Where CONCAT(B.PONum,'-', B.[LineNo],'-', B.RelNo) = '" + PONo + "' ";


            //if (userType.ToUpper() == "BUYER")
            //    sql += " AND BuyerEmail = '" + Email + "' ";
            //else if (userType == "SUPPLIER")
            //    sql += " AND VendorEmail = '" + Email + "' ";


            sql += " Order by V.CreatedOn DESC ";
            dt = oDAL.GetData(sql);

            int count = dt.Rows.Count;

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstUpdate = cCommon.ConvertDtToHashTable(dt);

                return true;
            }
        }

        public bool GetUpdateDataForDashboard()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            DataTable dt = new DataTable();

            string sql = "";
            sql = @"WITH LatestVendorComm AS (
    SELECT *, ROW_NUMBER() OVER (
        PARTITION BY PONo ORDER BY CreatedOn DESC
    ) AS rn
    FROM [SRM].[VendorCommunication]
),
LatestTransaction AS (
    SELECT PONo, MAX(CreatedOn) AS LastCommunication
    FROM [SRM].[Transaction]
    WHERE HasAction <> 'Document'
    GROUP BY PONo
)
SELECT 
    B.GUID, 
    CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo) AS PONo,
    B.PartNo, B.PartDesc, B.VendorName, 
    B.OrderQty, B.Qty AS CurrentQty, B.Price AS CurrentPrice,
    B.DueDate AS CurrentDueDate,
    V.Qty AS CommitQty, V.Price AS CommitPrice, V.DueDate AS CommitDueDate, 
    V.TrackingNo, V.AttachFile, V.FileExt, V.CreatedOn,
    T.LastCommunication,
    'Collablly' AS CompletedBy
FROM [SRM].[BuyerPO] B
JOIN LatestVendorComm V 
    ON B.GUID = V.GUID
    AND V.PONo = CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo)
    AND V.rn = 1
JOIN [dbo].[PODetail] P 
    ON P.POHeader_PONum = B.PONum
    AND P.PODetail_POLine = B.[LineNo]
    AND P.PORel_PORelNum = B.RelNo
LEFT JOIN LatestTransaction T 
    ON T.PONo = CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo)
WHERE 
    ISNULL(ROUND(P.PODetail_XOrderQty, 2), 0) = ISNULL(ROUND(P.Calculated_ReceivedQty, 2), 0)
    <BuyerSupplierClasue>

UNION ALL

SELECT 
    NULL AS GUID, 
    CONCAT(P.POHeader_PONum, '-', P.PODetail_POLine, '-', P.PORel_PORelNum) AS PONo,
    P.PODetail_PartNum AS PartNo, P.PODetail_LineDesc AS PartDesc, 
    P.Vendor_Name AS VendorName, 
    P.PODetail_OrderQty AS OrderQty, NULL AS CurrentQty, NULL AS CurrentPrice,
    P.Calculated_DueDate AS CurrentDueDate,
    NULL AS CommitQty, NULL AS CommitPrice, NULL AS CommitDueDate,
    NULL AS TrackingNo, NULL AS AttachFile, NULL AS FileExt, NULL AS CreatedOn,
    NULL AS LastCommunication,
    'ERP' AS CompletedBy
FROM [dbo].[PODetail] P
WHERE 
    ISNULL(ROUND(P.PODetail_XOrderQty, 2), 0) = ISNULL(ROUND(P.Calculated_ReceivedQty, 2), 0)
    AND NOT EXISTS (
        SELECT 1 FROM [SRM].[BuyerPO] B
        WHERE 
            B.PONum = P.POHeader_PONum
            AND B.[LineNo] = P.PODetail_POLine
            AND B.RelNo = P.PORel_PORelNum
    )
    <BuyerSupplierClasue>
ORDER BY CreatedOn DESC

";

            if (userType.ToUpper() == "BUYER")
                sql = sql.Replace("<BuyerSupplierClasue>", " AND P.PurAgent_EMailAddress = '" + Email + "' ");
            else
                sql = sql.Replace("<BuyerSupplierClasue>", " ");

            dt = oDAL.GetData(sql);

            int count = dt.Rows.Count;

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstUpdate = cCommon.ConvertDtToHashTable(dt);

                return true;
            }
        }


        // Not Calling
        public void UpdateHasActionAHR(string Action, string GUID, string PONo, string Line, string Rel, string FinalQty)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            DataTable dt = new DataTable();
            object result;
            string OrderQty = "";
            string sql = "";
            string CommunicationStatus = "Email";

            sql = "SELECT OrderQty FROM [SRM].[BuyerPO] Where GUID = '" + GUID + "' AND PONum = '" + PONo + "' AND [LineNo] = '" + Line + "' AND RelNo = '" + Rel + "'";
            result = oDAL.GetObject(sql);
            if (result != null)
                OrderQty = result.ToString();

            if (Convert.ToDecimal(OrderQty) == Convert.ToDecimal(FinalQty))
            {
                CommunicationStatus = "Completed";
            }
            sql = @"Update [SRM].[BuyerPO] SET HasAction = '<HasAction>', CommunicationStatus = '<Communication>' Where GUID = '<GUID>' 
                   AND PONum = '<PO>' AND [LineNo] = '<Line>' AND RelNo = '<rel>' ";

            sql = sql.Replace("<HasAction>", Action);
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<Communication>", CommunicationStatus);
            sql = sql.Replace("<PO>", PONo);
            sql = sql.Replace("<Line>", Line);
            sql = sql.Replace("<rel>", Rel);

            oDAL.Execute(sql);
            sql = @"SELECT TOP 1 B.PONum, B.[LineNo], B.relNo, B.PartNo, B.VendorName, V.Qty, V.Price, V.DueDate, B.VendorEmail, B.CreatedBy from [SRM].[BuyerPO] B
                    Inner Join[SRM].[VendorCommunication] V ON B.GUID = V.GUID AND V.PONo = CONCAT(B.PONum,+'-'+ B.[lineNo] + '-' +B.RelNo)
                    WHERE V.GUID = '<GUID>' AND B.PoNum = '<PO>' AND B.[LineNO] = '<LineNo>' and B.RelNo = '<RelNo>' ORDER BY V.Id DESC";
            sql = sql.Replace("<GUID>", GUID);
            sql = sql.Replace("<PO>", PONo);
            sql = sql.Replace("<LineNo>", Line);
            sql = sql.Replace("<RelNo>", Rel);

            dt = oDAL.GetData(sql);
            string vPONo = "";
            string vLine = "";
            string vRel = "";
            string vPart = "";
            string vQty = "";
            string vPrice = "";
            string vDueDate = "";
            string vCreatedBy = "";
            string vVendorEmail = "";
            string vVendorName = "";
            if (dt.Rows.Count > 0)
            {
                vPONo = Convert.ToString(dt.Rows[0]["PONum"]);
                vLine = Convert.ToString(dt.Rows[0]["LineNo"]);
                vRel = Convert.ToString(dt.Rows[0]["relNo"]);
                vPart = Convert.ToString(dt.Rows[0]["PartNo"]);
                vQty = Convert.ToString(dt.Rows[0]["Qty"]);
                vPrice = Convert.ToString(dt.Rows[0]["Price"]);
                vDueDate = Convert.ToString(dt.Rows[0]["DueDate"]);
                vCreatedBy = Convert.ToString(dt.Rows[0]["CreatedBy"]);
                vVendorEmail = Convert.ToString(dt.Rows[0]["VendorEmail"]);
                vVendorName = Convert.ToString(dt.Rows[0]["VendorName"]);

                string vmessage = "";
                if (Action == "Accept")
                    vmessage = "You request of this PO " + vPONo + "-" + vLine + "-" + vRel + " has been Accepted";
                else if (Action == "Reject")
                    vmessage = "You request of this PO " + vPONo + "-" + vLine + "-" + vRel + " has been Rejected";
                else if (Action == "Hold")
                    vmessage = "You request of this PO " + vPONo + "-" + vLine + "-" + vRel + " has been Hold";

                //SendAcceptPDF(vPONo, GUID);
                sendVendorEmailAHR(vCreatedBy, vPONo, vPart, vQty, vDueDate, vPrice, vmessage, GUID, vVendorEmail,vVendorName);
                //Insert in Transaction
                string PoNumber = vPONo + "-" + vLine + "-" + vRel;
                AddInTransaction(PoNumber, vPart, GUID.ToString(), Action, vQty.ToString(), vPrice.ToString(), vDueDate.ToString(), vCreatedBy, "In Epicor");

            }


        }

        public string sendVendorEmail(string EuserName, string EPONo, string EpartNo, string EQty, string EDueDate, string EPrice, string Emessage, string GUID, string newDueDate, string contactReason, string ccEmail, HttpPostedFileBase Attachfile, string Receiveremail, string[] CCemails)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string DefaultDB = HttpContext.Current.Session["DefaultDB"].ToString();
            // Subject
            var subject = "PO Information against this PONo. " + EPONo + "";

            // Recipient's email address    
            string recipientEmail = Receiveremail;

            // Make URL
            string Accepturl = "";
            string Changeurl = "";
            string EncPO = "";
            EncPO = BasicEncrypt.Instance.Encrypt(EPONo.Trim());

            DataTable dtURL = new DataTable();
            dtURL = cCommon.GetEmailURL(DeployMode, "VendorEmail");

            if (DeployMode == "PROD")
            {
                Accepturl = dtURL.Rows[0]["URL"].ToString() + dtURL.Rows[0]["PageURL"].ToString();
                Changeurl = dtURL.Rows[1]["URL"].ToString() + dtURL.Rows[1]["PageURL"].ToString();

                Accepturl = Accepturl.Replace("<GUID>", GUID.ToString());
                Accepturl = Accepturl.Replace("<PO>", EncPO);
                Accepturl = Accepturl.Replace("<Connection>", DefaultDB);


                Changeurl = Changeurl.Replace("<GUID>", GUID.ToString());
                Changeurl = Changeurl.Replace("<PO>", EncPO);
                Changeurl = Changeurl.Replace("<Connection>", DefaultDB);
            }
            else
            {
                Accepturl = dtURL.Rows[0]["URL"].ToString() + dtURL.Rows[0]["PageURL"].ToString();
                Changeurl = dtURL.Rows[1]["URL"].ToString() + dtURL.Rows[1]["PageURL"].ToString();

                Accepturl = Accepturl.Replace("<GUID>", GUID.ToString());
                Accepturl = Accepturl.Replace("<PO>", EncPO);
                Accepturl = Accepturl.Replace("<Connection>", DefaultDB);

                Changeurl = Changeurl.Replace("<GUID>", GUID.ToString());
                Changeurl = Changeurl.Replace("<PO>", EncPO);
                Changeurl = Changeurl.Replace("<Connection>", DefaultDB);
            }

            // HTML body containing the form
            DateTime ConvertedNewDueDate = new DateTime();
            if (!string.IsNullOrEmpty(newDueDate))
            {
                ConvertedNewDueDate = DateTime.Parse(newDueDate);
            }
            string htmlBody = MakeEmailBody(ConvertedNewDueDate.ToString("MM/dd/yyyy"), contactReason, EuserName, EPONo, EpartNo, EQty, EDueDate, EPrice, Emessage, Accepturl, Changeurl);
            string cc = "";
            if (CCemails != null && CCemails.Length > 0)
            {
                cc = string.Join(",", CCemails);
            }


            EmailResult = cCommon.SendEmail(recipientEmail, subject, htmlBody, cc, Attachfile);
            if (EmailResult == "SENT")
            {
                // Send the email
                string SenderEmail = HttpContext.Current.Session["Email"].ToString();
                InsertBuyerCommunication(SenderEmail, recipientEmail, subject, htmlBody, GUID.ToString(), EuserName);

            }
            else
            {
                EmailResult = "NOT";
            }


            return EmailResult;
        }


        public string MakeEmailBody(string NewDueDate, string contactReason, string EuserName, string EPONo, string EpartNo, string EQty, string EDueDate, string EPrice, string Emessage, string Accepturl, string Changeurl)
        {
            string htmlBody = "";
            string changeHeading = "";
            DataTable dt = new DataTable();
            string query = "";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            query = "Select SYSValue from dbo.zSysIni ";


            if (contactReason == "Change")
            {
                changeHeading = "Due Date Change Request";
                query += "WHERE SysDesc = 'VendorEmailChange' ";
                dt = oDAL.GetData(query);
                htmlBody = dt.Rows[0]["SYSValue"].ToString();
            }
            else if (contactReason == "Update")
            {
                changeHeading = "Update Request";
                query += "WHERE SysDesc = 'VendorEmailUpdate' ";
                dt = oDAL.GetData(query);
                htmlBody = dt.Rows[0]["SYSValue"].ToString();
            }
            else
            {
                changeHeading = "Confirmation Request";
                query += "WHERE SysDesc = 'VendorEmailConfirm' ";
                dt = oDAL.GetData(query);
                htmlBody = dt.Rows[0]["SYSValue"].ToString();
            }
            htmlBody = htmlBody.Replace("<ChangeHeaderDueDate>", changeHeading);
            htmlBody = htmlBody.Replace("<NewDueDate>", NewDueDate);
            htmlBody = htmlBody.Replace("<BuyerName>", EuserName);
            htmlBody = htmlBody.Replace("<PONo>", EPONo);
            htmlBody = htmlBody.Replace("<PartNo>", EpartNo);
            htmlBody = htmlBody.Replace("<Qty>", EQty);
            htmlBody = htmlBody.Replace("<DueDate>", EDueDate);
            htmlBody = htmlBody.Replace("<Price>", EPrice);
            htmlBody = htmlBody.Replace("<Message>", Emessage);
            htmlBody = htmlBody.Replace("<Accept_URL>", Accepturl);
            htmlBody = htmlBody.Replace("<Change_URL>", Changeurl);

            return htmlBody;
        }

        public void sendVendorEmailAHR(string EuserName, string EPONo, string EpartNo, string EQty, string EDueDate, string EPrice, string Emessage, string GUID, string VendorEmial, string VendorName)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string EmailAddress = HttpContext.Current.Session["Email"].ToString();
            string LastName = HttpContext.Current.Session["LastName"].ToString();
            string FirstName = HttpContext.Current.Session["FirstName"].ToString();

            string query = string.Empty;
            query = @"INSERT INTO [dbo].[BuyerAcceptPDF]
                       ([PONumber]
                       ,[Vendor]
                       ,[VendorEmail]
                       ,[Buyer]
                       ,[InsertedBy])
                 VALUES
                       ('<PONumber>'
                       ,'<VendorName>'
                       ,'<VendorEmail>'
                       ,'<Buyer>'
                       ,'<InsertedBy>')";

            query = query.Replace("<PONumber>", EPONo);
            query = query.Replace("<VendorName>", VendorName);
            query = query.Replace("<VendorEmail>", VendorEmial);
            query = query.Replace("<Buyer>", LastName + ' ' + FirstName);
            query = query.Replace("<InsertedBy>", EmailAddress);
            
            oDAL.Execute(query);
     

        }

        public void UpdatePropose(string uGUID, string uQty, string uPrice, string uDueDate, string uMessage, string userName, string uPONo, string uPartNo, string ProposeEmailId, string[] CCemails)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string escapedMessage = uMessage.Replace("'", "''");
            string[] POLineRel = uPONo.Split('-');
            //DateTime udate = DateTime.ParseExact(uDueDate, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string sql = @"UPDATE [SRM].[BuyerPO]
                           SET 
                               [Qty] = '<Qty>'
                              ,[Price] = '<Price>'
                              ,[DueDate] = '<DueDate>'
                              ,[POStatus] = '<POStatus>'
                              ,[HasAction] = '<HasAction>'
                              ,[CommunicationStatus] = '<CommStatus>'
                              ,[ContactReason] = ''
                         WHERE GUID = '<GUID>' AND PONum = '<PO>' AND [LineNo] = '<Line>' AND RelNo = '<Rel>' ";
            DateTime ConvertedDueDate = new DateTime();
            if (!string.IsNullOrEmpty(uDueDate))
            {
                ConvertedDueDate = DateTime.Parse(uDueDate);
            }


            sql = sql.Replace("<Qty>", uQty);
            sql = sql.Replace("<Price>", uPrice);
            sql = sql.Replace("<DueDate>", ConvertedDueDate.ToString("MM/dd/yyyy"));
            sql = sql.Replace("<POStatus>", "New");
            sql = sql.Replace("<HasAction>", "Suggest");
            sql = sql.Replace("<CommStatus>", "Awaiting");
            sql = sql.Replace("<GUID>", uGUID);
            sql = sql.Replace("<PO>", POLineRel[0]);
            sql = sql.Replace("<Line>", POLineRel[1]);
            sql = sql.Replace("<Rel>", POLineRel[2]);
            oDAL.Execute(sql);

            //DataTable dt = new DataTable();
            //string RecpientEmail = "";
            //sql = "SELECT VendorEmail from  [SRM].[BuyerPO] where GUID = '" + uGUID + "' ";
            //dt = oDAL.GetData(sql);
            //if (dt.Rows.Count > 0)
            //{
            //    RecpientEmail = dt.Rows[0]["VendorEmail"].ToString();
            //}
            //Insert in Transaction
            if (!oDAL.HasErrors)
            {

                AddInTransaction(uPONo, uPartNo, uGUID.ToString(), "Suggest", uQty.ToString(), uPrice.ToString(), ConvertedDueDate.ToString("MM/dd/yyyy"), userName, escapedMessage);
                sendVendorEmail(userName, uPONo, uPartNo, uQty, ConvertedDueDate.ToString("MM/dd/yyyy"), uPrice, uMessage, uGUID, "", "", "", null, ProposeEmailId, CCemails);
                UpdateVendorTable(uPONo, uGUID);
            }

        }

        public void UpdateVendorTable(string PO, string GUID)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = "";
            sql = "UPDATE [SRM].[VendorCommunication] SET IsAnswered = '0' WHERE PONO = '<PO>' and GUID = '<GUID>' ";
            sql = sql.Replace("<PO>", PO);
            sql = sql.Replace("<GUID>", GUID);
            oDAL.Execute(sql);
        }

        public void sendMultiEmail(string htmlBody, string EuserName, string EPONo, string EpartNo, string EQty, string EDueDate, string EPrice, string Emessage, string GUID, string VendorEmail, string CCVendorEmail)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            DataTable dt = new DataTable();
            string result = "";
            //string sql = @"Select VendorEmail From [SRM].[BuyerPO]
            //        WHERE GUID = '<GUID>'";
            //sql = sql.Replace("<GUID>", GUID);
            //dt = oDAL.GetData(sql);
            //string PVendorEmail = "";
            //if (dt.Rows.Count > 0)
            //{
            //    PVendorEmail = Convert.ToString(dt.Rows[0]["VendorEmail"]);
            //}
            var subject = "Purchase Order Confirmation";
            // Recipient's email address
            string recipientEmail = VendorEmail;

            DataTable dtSender = new DataTable();
            dtSender = cCommon.GetEmailSMTPSetup();
            // Sender's email address and password
            string senderEmail = dtSender.Rows[0]["SenderEmail"].ToString();


            try
            {
                // Send the email
                InsertBuyerCommunication(senderEmail, recipientEmail, subject, htmlBody, GUID.ToString(), EuserName);
                result = cCommon.SendEmail(recipientEmail, subject, htmlBody, CCVendorEmail, null);

            }
            catch (Exception)
            {

            }
        }

        public bool GetTransactionDtl(string poNo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = string.Empty;
            DataTable dtDocument = new DataTable();
            DataTable sortedDT = new DataTable();
            query = @"select T.Id 
                            ,T.[PONo]
                            ,T.[PartNo]
                            ,T.[Type]
                            ,T.[GUID]
                            ,T.[HasAction]
                            ,T.[Qty]
                            ,T.[Price]
                            ,T.[DueDate]
                            ,T.[Message]
                            ,T.[OrgId]
                            ,T.[CreatedBy]
                            ,T.[CreatedOn]
                            ,V.ServiceType	                       
	                        ,V.ServiceURL
                            ,V.FileExt
                            ,V.AttachFile
	                        from [SRM].[Transaction] T 
                            Left Join [SRM].[VendorCommunication] V ON V.Id = T.VendorCommId 
                            Where T.PONo = '<PONO>'
                            Order by T.Id desc ";

            query = query.Replace("<PONO>", poNo);
            DataTable dt = oDAL.GetData(query);

            if (!oDAL.HasErrors)
            {
                DataView dv = dt.DefaultView;
                dv.Sort = "Id DESC"; // You can change "ID" to your desired column for sorting
                sortedDT = dv.ToTable();
                lstPOTran = new List<Hashtable>();
                lstPOTran = cCommon.ConvertDtToHashTable(sortedDT);
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool GetPOTransactionDtl(string poNo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = @"select Id, PONumber, Vendor, VendorEmail, [Status],Buyer, InsertedOn, 
                                InsertedBy, UserType from [dbo].[BuyerPOHeaderTran]
                    Where PONumber = '<PONumber>' Order by Id Desc ";

            query = query.Replace("<PONumber>", poNo);
            DataTable dt = oDAL.GetData(query);

            if (!oDAL.HasErrors)
            {
           
                    lstPOHeadTran = new List<Hashtable>();
                    lstPOHeadTran = cCommon.ConvertDtToHashTable(dt);
                
               
                return true;
            }
            else
            {
                return false;
            }
        }

        public string AddPOHeaderTransaction(string PO, string Vendor, string VendorEmail, string newGuid)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            NewPOCommon oPO = new NewPOCommon();
            string EmailAddress = HttpContext.Current.Session["Email"].ToString();
            string LastName = HttpContext.Current.Session["LastName"].ToString();
            string FirstName = HttpContext.Current.Session["FirstName"].ToString();

            string query = string.Empty;
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

            query = query.Replace("<PONumber>", PO);
            query = query.Replace("<Vendor>", Vendor);
            query = query.Replace("<VendorEmail>", VendorEmail);
            query = query.Replace("<Status>", "Sent");
            query = query.Replace("<GUID>", newGuid);
            query = query.Replace("<Buyer>", LastName + ' ' + FirstName);
            query = query.Replace("<UserType>", "Buyer");
            query = query.Replace("<InsertedBy>", EmailAddress);


            DateTime localTime = oPO.ConvertGenericDateTime();
            query = query.Replace("<InsertedOn>", localTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            oDAL.Execute(query);
            if (!oDAL.HasErrors)
                return "OK";
            else
                return "NOTOK";

        }


        public string OpenPDF(string poNo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = string.Empty;
            object result;
            query = "select TaskNum from [dbo].[tblPdf] where PONum = '<PONO>'";
            query = query.Replace("<PONO>", poNo);
            result = oDAL.GetObject(query);
            if (result != null)
            {
                return result.ToString();
            }
            else
            {
                return "";
            }

        }

        public void SendAcceptPDF(string PONo, string GUID)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = string.Empty;
            PDFReport oPDF = new PDFReport();
            DataTable dt = new DataTable();
            string VendorEmail = "";
            string VendorName = "";
            string result = "";
            string[] POVal = PONo.Split('-');
            string PO = POVal[0];
            string Line = POVal[1];
            string Rel = POVal[2];
            query = "SELECT VendorName, VendorEmail FROM [SRM].[BuyerPO] Where GUID =  '" + GUID + "'";
            dt = oDAL.GetData(query);
            if (dt.Rows.Count > 0)
            {
                VendorEmail = dt.Rows[0]["VendorEmail"].ToString();
                VendorName = dt.Rows[0]["VendorName"].ToString();
            }
            if (result.ToString() == "OK")
            {
                SendPDFEmail(PO, VendorName, VendorEmail);
                oPDF.InsertPDFRecord(PO, VendorName, VendorEmail);
            }

        }

        public void SendPDFEmail(string PONo, string SupplierName, string SupplierEmail)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            NewPO oNewPO = new NewPO();
            string ConnctionType = HttpContext.Current.Session["DefaultDB"].ToString();
            var subject = "PDF report of PO No.:  " + PONo + "";
            // Recipient's email address
            string recipientEmail = SupplierEmail;

            string URL = "";
            //URL
            DataTable dtURL = new DataTable();
            dtURL = cCommon.GetEmailURL(ConnctionType, "EmailPDF");

            if (ConnctionType == "PROD")
            {
                URL = dtURL.Rows[0]["URL"].ToString() + dtURL.Rows[0]["PageURL"].ToString();

            }
            else
            {
                URL = dtURL.Rows[0]["URL"].ToString() + dtURL.Rows[0]["PageURL"].ToString();

            }
            URL = URL.Replace("<PO>", PONo);
            URL = URL.Replace("<Connection>", ConnctionType);


            string htmlBody = MakeEmailBody(PONo, SupplierName, URL);


            // Email Send 
            string result = cCommon.SendEmail(recipientEmail, subject, htmlBody, "", null);
            if (result == "SENT")
            {
                string createdBy = HttpContext.Current.Session["Username"].ToString();
                oNewPO.AddInTransaction(PONo, "", "", "Document", "", "", "", createdBy, "Sent PDF to Supplier");
            }
        }

        public string MakeEmailBody(string PONo, string Supplier, string URL)
        {

            string htmlBody = "";
            DataTable dt = new DataTable();
            string query = "";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            query = "Select SYSValue from dbo.zSysIni WHERE SysDesc = 'AcceptPDF'";
            dt = oDAL.GetData(query);

            htmlBody = dt.Rows[0]["SYSValue"].ToString();

            htmlBody = htmlBody.Replace("<SupplierName>", Supplier);
            htmlBody = htmlBody.Replace("<PONo>", PONo);
            htmlBody = htmlBody.Replace("<PDFURL>", URL);



            return htmlBody;
        }

        public string PDFAPICall(string PONo)
        {
            string DeployMode = ConfigurationManager.AppSettings["DEPLOYMODE"];
            DataTable dtURL = cCommon.GetEmailURL(DeployMode, "INSERTPDF"); // Assuming GetEmailURL returns a DataTable
            string URL = dtURL.Rows[0]["URL"].ToString();
            string PageURL = dtURL.Rows[0]["PageURL"].ToString();
            string userName = dtURL.Rows[0]["UserName"].ToString();
            string password = dtURL.Rows[0]["Password"].ToString();
            password = BasicEncrypt.Instance.Decrypt(password.Trim());

            string Key = dtURL.Rows[0]["TokenKey"].ToString();

            var client = new RestClient(URL);
            var request = new RestRequest(PageURL, Method.Post);
            // Add basic authentication header
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password)));
            request.AddHeader("api-key", Key);

            var jsonBody = MakeJsonBody(PONo);
            request.AddJsonBody(jsonBody);

            var response = client.Execute(request);
            string result = "";
            if (response.IsSuccessStatusCode == true)
            {
                result = "OK";
            }
            else
            {
                JObject json = JObject.Parse(response.Content);
                string errorMessage = json["ErrorMessage"]?.ToString();
                cLog oLog = new cLog();
                oLog.RecordError(errorMessage, response.Content.ToString(), "Patch Api Calling PDFAPICall method");
                result = "Not Ok";
            }


            return result;
        }

        public string MakeJsonBody(string PONo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string Json = "";
            string sql = "Select APIJson from [dbo].[APIJson]";
            DataTable dt = new DataTable();

            dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                Json = dt.Rows[0]["APIJson"].ToString();
            }

            JObject jsonObject = JObject.Parse(Json);

            // Change the value of "PONum" dynamically
            jsonObject["ds"]["POFormParam"][0]["PONum"] = PONo;
            jsonObject["ds"]["POFormParam"][0]["AttachmentType"] = PONo;
            // Convert the modified JSON object back to string
            string modifiedJsonString = jsonObject.ToString();

            return modifiedJsonString;

        }

        public void UpdateStatus(string PO, string GUID)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = string.Empty;
            DataTable dt = new DataTable();
            string[] POLineRel = PO.Split('-');
            query = @"UPDATE [SRM].[BuyerPO]
                        SET POStatus = 'Completed' , CommunicationStatus = 'Completed'
                        WHERE Qty = OrderQty AND GUID = '" + GUID + "' AND PONum = '" + POLineRel[0] + "' " +
                        "AND [LineNo] = '" + POLineRel[1] + "' AND RelNo = '" + POLineRel[2] + "' ";

            oDAL.Execute(query);
        }


        public string GetGUID(string PO)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string result = "";
            string query = @"SELECT GUID FROM [BuyerPOHeader] WHERE PONumber = '" + PO + "' ";
            DataTable dt = oDAL.GetData(query);
            if (dt.Rows.Count > 0)
                result = dt.Rows[0]["GUID"].ToString();

            return result;
        }
        public string AddPOHeaderData(string PO, string VendorId, string Vendor, string VendorEmail, string POStatus, string newGuid)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            NewPOCommon oPO = new NewPOCommon();
            string EmailAddress = HttpContext.Current.Session["Email"].ToString();
            string LastName = HttpContext.Current.Session["LastName"].ToString();
            string FirstName = HttpContext.Current.Session["FirstName"].ToString();

            string Buyer = LastName + " " + FirstName;
            DateTime localTime = oPO.ConvertGenericDateTime();

            string query = @"
        IF EXISTS (SELECT 1 FROM [dbo].[BuyerPOHeader] WHERE PONumber = '<PONumber>' AND GUID = '<GUID>')
        BEGIN
            UPDATE [dbo].[BuyerPOHeader]
               SET [VendorId]    = '<VendorId>',
                   [Vendor]      = '<Vendor>',
                   [VendorEmail] = '<VendorEmail>',
                   [Status]      = '<Status>',
                   [IsEmailSent] = '<IsEmailSent>',
                   [Buyer]       = '<Buyer>',
                   [UpdatedBy]  = '<InsertedBy>',
                   [UpdatedOn]  = '<InsertedOn>',
                   [IsAnswerd]  = '<IsAnswerd>'

             WHERE PONumber = '<PONumber>' AND GUID = '<GUID>'
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[BuyerPOHeader]
                   ([PONumber]
                   ,[VendorId]
                   ,[Vendor]
                   ,[VendorEmail]
                   ,[Status]
                   ,[GUID]
                   ,[IsEmailSent]
                   ,[Buyer]
                   ,[InsertedBy]
                   ,[InsertedOn]
                   ,[IsAnswerd]
)
             VALUES
                   ('<PONumber>'
                   ,'<VendorId>'
                   ,'<Vendor>'
                   ,'<VendorEmail>'
                   ,'<Status>'
                   ,'<GUID>'
                   ,'<IsEmailSent>'
                   ,'<Buyer>'
                   ,'<InsertedBy>'
                   ,'<InsertedOn>'
                   ,'<IsAnswerd>'
)
        END";

            query = query.Replace("<PONumber>", PO);
            query = query.Replace("<VendorId>", VendorId);
            query = query.Replace("<Vendor>", Vendor);
            query = query.Replace("<VendorEmail>", VendorEmail);
            query = query.Replace("<Status>", "Sent");
            query = query.Replace("<IsEmailSent>", "0");
            query = query.Replace("<GUID>", newGuid);
            query = query.Replace("<Buyer>", Buyer);
            query = query.Replace("<InsertedBy>", EmailAddress);
            query = query.Replace("<InsertedOn>", localTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            query = query.Replace("<IsAnswerd>", "0");

            oDAL.Execute(query);

            return oDAL.HasErrors ? "NOTOK" : "OK";
        }

        public DataTable GetBuyerList()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = "SELECT BuyerID, [Name] FROM  [dbo].[BuyerInfo]  WHERE [Name] <> '' ";
            return oDAL.GetData(sql);
        }

        public DataTable GetSupplierList()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = "SELECT DISTINCT Vendor_VendorID, Vendor_Name FROM [dbo].[PODetail] WHERE Vendor_Name IS NOT NULL ";
            return oDAL.GetData(sql);
        }
        public DataTable GetFilteredPOs(string buyerEmail, string supplierEmail, string dueFrom, string dueTo, string PartNo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);

            string sql = @"
        SELECT 
            PO.POHeader_PONum,
            PO.POHeader_Company,
            PO.Calculated_OrderDate AS POHeader_OrderDate,
            PO.Vendor_VendorID,
            PO.Vendor_Name,
            PO.POHeader_BuyerID,
            PO.PurAgent_Name,
            PO.Vendor_EMailAddress,
            PO.PurAgent_EMailAddress,
            PO.POHeader_Approve,
	        PO.PODetail_PartNum,
            PO.RowIdent,
            ISNULL(BPH.Status, 'New') AS POStatus
        FROM (
            SELECT *,
                   ROW_NUMBER() OVER (PARTITION BY POHeader_PONum ORDER BY RowIdent) AS rn
            FROM [dbo].[PODetail]
            WHERE 1=1
            {BuyerEmailFilter}
             {SupplierEmailFilter}
             {FromDateFilter}
             {ToDateFilter}
             {PartNo}
        ) AS PO
        LEFT JOIN [dbo].[BuyerPOHeader] BPH ON PO.POHeader_PONum = BPH.PONumber
        WHERE PO.rn = 1 ORDER BY BPH.UpdatedOn DESC, BPH.InsertedOn DESC;";

            // Build filters
            string buyerFilter = string.IsNullOrEmpty(buyerEmail)
                ? ""
                : $"AND POHeader_BuyerId = '{buyerEmail}'";

            string supplierFilter = string.IsNullOrEmpty(supplierEmail)
                ? ""
                : $"AND Vendor_VendorId = '{supplierEmail}'";

            string fromDateFilter = string.IsNullOrEmpty(dueFrom)
                ? ""
                : $"AND Calculated_DueDate >= '{dueFrom}'";

            string toDateFilter = string.IsNullOrEmpty(dueTo)
                ? ""
                : $"AND Calculated_DueDate <= '{dueTo}'";
            string toPartNo = string.IsNullOrEmpty(PartNo)
               ? ""
               : $"AND PODetail_PartNum LIKE '%{PartNo}%'";


            // Replace placeholders
            sql = sql.Replace("{BuyerEmailFilter}", buyerFilter);
            sql = sql.Replace("{SupplierEmailFilter}", supplierFilter);
            sql = sql.Replace("{FromDateFilter}", fromDateFilter);
            sql = sql.Replace("{ToDateFilter}", toDateFilter);
            sql = sql.Replace("{PartNo}", toPartNo);
            DataTable dt = new DataTable();

            dt = oDAL.GetData(sql);

            return dt;

        }

    }
}


public class ByteArrayPostedFileBase : HttpPostedFileBase
{
    private readonly byte[] _fileBytes;
    private readonly string _fileName;
    private readonly string _contentType;

    public ByteArrayPostedFileBase(byte[] fileBytes, string fileName, string contentType)
    {
        _fileBytes = fileBytes;
        _fileName = fileName;
        _contentType = contentType;
    }

    public override int ContentLength => _fileBytes.Length;
    public override string FileName => _fileName;
    public override Stream InputStream => new MemoryStream(_fileBytes);
    public override string ContentType => _contentType;
}