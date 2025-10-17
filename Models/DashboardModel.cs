using PlusCP.Classess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PlusCP.Models
{
    public class DashboardModel
    {
        cDAL oDAL;
        #region Data Fields

        public string AllOpen { get; set; }
        public string Pending { get; set; }
        public string Late { get; set; }
        public string Arrived { get; set; }
        public string Update { get; set; }

        public string AllPO { get; set; }
        public string Result { get; set; }
        public string IsTempKey { get; set; }

        public string WidgetId { get; set; }
        public string WidgetTitle { get; set; }
        public string vendorId { get; set; }
        public string WidgetDesc { get; set; }
        public string WidgetPO { get; set; }
        public string Count { get; set; }
        public string Days { get; set; }
        public string ScheduleDateTime { get; set; }
        public string widget_id { get; set; }
        public string widget_desc { get; set; }
        public string widget_count_count { get; set; }
        public string widget_count_fraction { get; set; }
        public string widget_count_bg { get; set; }
        public string widget_count_report_URL { get; set; }
        public string report_URL { get; set; }
        public string report_name { get; set; }
        public string report_target { get; set; }
        public string report_title_short { get; set; }
        public string report_designed_by { get; set; }
        public bool report_is_internal { get; set; } = true;
        public string report_code { get; set; }
        public string chart_title { get; set; }
        public List<ArrayList> lstPO { get; set; }
        public List<Hashtable> lstPODetails { get; set; }
        public List<Dictionary<string, object>> lst_widgets { get; set; }
        public List<Employee_Widgets> lst_employee_widgets { get; set; }
        public List<ArrayList> lst_widget_hyperlist { get; set; }
        public List<ArrayList> lst_widget_list { get; set; }
        public List<ArrayList> lst_widget_table { get; set; }
        public List<ArrayList> lst_widget_group_counts { get; set; }
        public string widget_table_column_format { get; set; }
        public int widget_table_cols_count { get; set; }
        public Dictionary<string, object> chart_data { get; set; }

        public DataTable dtStations { get; set; }
        public DataTable dtCurrent { get; set; }
        public List<ArrayList> dataChart { get; set; }
        public DataTable dtChart { get; set; }
        public DataTable dtWidgetPO { get; set; }

        public DataTable dtPO { get; set; }
        #endregion

        #region Structures
        public struct Employee_Widgets
        {
            public string WidgetId { get; set; }
            public string WidgetTitle { get; set; }
            public string WidgetType { get; set; }
            public string WidgetMinSize { get; set; }
            public string WidgetMaxSize { get; set; }
            public string WidgetPositionY { get; set; }
            public string WidgetPositionX { get; set; }
            public string WidgetSizeX { get; set; }
            public string WidgetSizeY { get; set; }
        }
        public class CardViewWidgets
        {
            public string Title { get; set; }
            public int Count { get; set; }
        }
        #endregion

        #region Methods

        public void GetCardWidgets()
        {
            string Email = HttpContext.Current.Session["Email"].ToString();
            List<CardViewWidgets> cardData = new List<CardViewWidgets>();
            string sql = @"WITH CTE AS (
    SELECT 
        Token,  
        Id, 
        WidgetTitle,
        VendorName,
        ExpireOn,
        WidgetDesc, 
        Count,
        ROW_NUMBER() OVER (PARTITION BY Token ORDER BY Id) AS RowNum
    FROM 
        [dbo].[Widgets]
    WHERE 
        Email = '<Email>' 
        AND IsActive = 1
)
SELECT 
    Token, 
    Id, 
    WidgetTitle,
    VendorName,
    ExpireOn,
    WidgetDesc, 
    Count
FROM 
    CTE;";

            sql = sql.Replace("<Email>", Email);

            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            dtWidgetPO = oDAL.GetData(sql);


        }

        public void GetPOList(string supplierId, string noOfDays)
        {
            
            string sql = @"

SELECT DISTINCT 
   CONCAT(POHeader_PONum, '-', PODetail_POLine, '-', PORel_PORelNum) AS POHeader_PONum,
    CASE 
        WHEN BH.PONum IS NULL THEN 0  -- Not Exist (checked rahna chahiye)
        ELSE 1  -- Exist (uncheck karna hai)
    END AS IsExist
FROM [dbo].[PODetail] PD
LEFT JOIN [SRM].[BuyerPO] BH
    ON PD.POHeader_PONum = BH.PONum AND PD.PODetail_POLine = BH.[LineNo] AND PD.PORel_PORelNum = BH.RelNo
WHERE Vendor_VendorID = '<supplierId>' AND  DATEDIFF(DAY, GETDATE(), PD.Calculated_DueDate) BETWEEN 0 AND <noOfDays>

";

            sql = sql.Replace("<supplierId>", supplierId);
            sql = sql.Replace("<noOfDays>", noOfDays);

            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dt = oDAL.GetData(sql);
            lstPODetails = cCommon.ConvertDtToHashTable(dt);
        }

        //public void GetPOList(string noOfDays)
        //{
        //    DataTable dt = HttpContext.Current.Session["APIData"] as DataTable;

        //    // Check if the DataTable is null
        //    if (dt == null)
        //    {
        //        throw new InvalidOperationException("APIData session is null or empty.");
        //    }

        //    // Initialize the filtered DataTable
        //    DataTable filteredDt = dt.Clone(); // Clone the structure of the original DataTable

        //    // Check if noOfDays is not null or empty
        //    // Check if noOfDays is not null or empty
        //    if (!string.IsNullOrEmpty(noOfDays))
        //    {
        //        if (int.TryParse(noOfDays, out int days))
        //        {
        //            // Get the date that is 'days' from today
        //            DateTime targetDate = DateTime.Now.Date.AddDays(days);

        //            // LINQ query to filter rows where the Calculated_DueDate is within the range
        //            var filteredRows = from row in dt.AsEnumerable()
        //                               let dueDate = (DateTime)row["Calculated_DueDate"]
        //                               where dueDate.Date >= DateTime.Now.Date &&
        //                                     dueDate.Date <= targetDate &&
        //                                     row["CommunicationStatus"].ToString() != "Completed"
        //                               select row;

        //            // Populate the filtered DataTable
        //            if (filteredRows.Any())
        //            {
        //                filteredDt = filteredRows.CopyToDataTable();
        //            }
        //        }


        //        else
        //        {
        //            throw new ArgumentException("Invalid number of days provided.");
        //        }
        //    }

        //    dt = filteredDt;

        //    DataTable dtPODetails = new DataTable();

        //    // Create a new DataTable to hold the required data
        //    DataTable newDt = new DataTable();
        //    newDt.Columns.Add("Id", typeof(string));      // Assuming Id is of type int
        //    newDt.Columns.Add("PO", typeof(string));   // PO will be a concatenation of POHeader, POLine, and PORel
        //    newDt.Columns.Add("SupplierEmail", typeof(string));
        //    newDt.Columns.Add("poHeader", typeof(string));
        //    newDt.Columns.Add("poLine", typeof(string));
        //    newDt.Columns.Add("poRel", typeof(string));
        //    newDt.Columns.Add("PartNo", typeof(string));
        //    newDt.Columns.Add("partDesc", typeof(string));
        //    newDt.Columns.Add("UOM", typeof(string));
        //    newDt.Columns.Add("orderDate", typeof(string));
        //    newDt.Columns.Add("dueDate", typeof(string));
        //    newDt.Columns.Add("orderQty", typeof(string));
        //    newDt.Columns.Add("arrivedrQty", typeof(string));
        //    newDt.Columns.Add("FinalQty", typeof(string));
        //    newDt.Columns.Add("price", typeof(string));
        //    newDt.Columns.Add("vendorId", typeof(string));
        //    newDt.Columns.Add("vendorName", typeof(string));
        //    newDt.Columns.Add("buyerId", typeof(string));
        //    newDt.Columns.Add("supplierCompany", typeof(string));

        //    // Loop through each row of the original DataTable
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        // Extract the values of POHeader, POLine, and PORel
        //        string poHeader = row["POHeader_PONum"].ToString();
        //        string poLine = row["PODetail_POLine"].ToString();
        //        string poRel = row["PORel_PORelNum"].ToString();
        //        string SupplierEmail = row["Vendor_EMailAddress"].ToString();
        //        // Concatenate POHeader, POLine, and PORel with hyphens
        //        string poValue = $"{poHeader}-{poLine}-{poRel}";
        //        string PartNo = row["PODetail_PartNum"].ToString();
        //        string partDesc = row["PODetail_LineDesc"].ToString();
        //        string UOM = row["PODetail_IUM"].ToString();
        //        string orderDate = row["Calculated_OrderDate"].ToString();
        //        string dueDate = row["Calculated_DueDate"].ToString();
        //        string orderQty = row["PODetail_OrderQty"].ToString();
        //        string arrivedrQty = row["Calculated_ArrivedQty"].ToString();
        //        decimal FinalQty = Convert.ToDecimal(orderQty) - Convert.ToDecimal(arrivedrQty);
        //        string price = row["PODetail_ExtCost"].ToString();
        //        string vendorId = row["Vendor_VendorID"].ToString();
        //        string vendorName = row["Vendor_Name"].ToString();
        //        string buyerId = row["PurAgent_Name"].ToString();
        //        string supplierCompany = row["POHeader_Company"].ToString();
        //        // Add a new row to the new DataTable with Id and concatenated PO value
        //        newDt.Rows.Add(poValue, poValue, SupplierEmail, poHeader, poLine, poRel, PartNo, partDesc, UOM, orderDate, dueDate, orderQty, arrivedrQty, FinalQty, price, vendorId, vendorName, buyerId, supplierCompany);

        //    }
        //    lstPODetails = cCommon.ConvertDtToHashTable(newDt);
        //    // Removing all columns except the first two
        //    for (int i = newDt.Columns.Count - 1; i >= 2; i--)
        //    {
        //        newDt.Columns.RemoveAt(i);
        //    }
        //    lstPO = cCommon.ConvertDtToArrayList(newDt);

        //}

        public void GetWidgetById(string WidgetId)
        {
            string Email = HttpContext.Current.Session["Email"].ToString();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"SELECT 
    Id,
    WidgetTitle,
    vendorId,
    WidgetDesc,
    Days,
    (SELECT STUFF(
        (SELECT ', ' + PO 
         FROM [dbo].[Widgets] AS W2 
         WHERE W2.Token = W1.Token  -- Token based concatenation
         FOR XML PATH('')), 
        1, 2, '')) AS PO
FROM 
    [dbo].[Widgets] AS W1 
where Id = <WidgetId> AND IsActive = 1 ";
            sql = sql.Replace("<WidgetId>", WidgetId);
            DataTable dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                WidgetId = dt.Rows[0]["Id"].ToString();
                WidgetTitle = dt.Rows[0]["WidgetTitle"].ToString();
                vendorId = dt.Rows[0]["vendorId"].ToString();
                WidgetDesc = dt.Rows[0]["WidgetDesc"].ToString();
                WidgetPO = dt.Rows[0]["PO"].ToString();
                Days = dt.Rows[0]["Days"].ToString();
                //ScheduleDateTime = dt.Rows[0]["scheduleDatetime"].ToString();
            }

        }
        public void Get_NewWidgetAPI(DataTable dt, string email, string userType)
        {
            string query = string.Empty;
            //object Result;
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
                Late = overdueDataTable.Rows.Count.ToString();
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

        public void DashboardCount(DataTable dt)
        { 
            if(dt.Rows.Count > 0)
            {
                Arrived = dt.Rows[0]["EarlyCount"].ToString();
                Pending = dt.Rows[0]["PendingCount"].ToString();
                Update = dt.Rows[0]["UpdatedCount"].ToString();
                Late = dt.Rows[0]["LateCount"].ToString();
            }
            else
            {
                Arrived = "0";
                Pending = "0";
                Update = "0";
                Late = "0";
            }

        }
        public void DashboardCountDemo(DataTable dt)
        {
            NewPOCommon oPO = new NewPOCommon();
            AllOpen = oPO.GetIdleCountDemo();
            Pending = oPO.GetInProcessCountDemo();
            Update = oPO.GetCompletedCount();
            if (dt.Rows.Count > 0)
            {
                Late = dt.Rows[0]["Calculated_LatePOs"].ToString();
            }
            else
            {
                Late = "0";
            }

        }

        public void Get_NewWidget()
        {
            string query = string.Empty;
            object Result;
            string Email = HttpContext.Current.Session["Email"].ToString();
            string userType = HttpContext.Current.Session["UserType"].ToString();

            oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            if (userType.ToUpper() == "BUYER" || userType.ToUpper() == "SUPPLIER")
            {
                //All Open
                query = @"SELECT COUNT(*) 
FROM dbo.tblPurchaseOrder AS row1
LEFT JOIN SRM.BuyerPO AS row2
    ON row1.POHeader_PONum = row2.PONum
    AND row1.PODetail_POLine = row2.[LineNo]
    AND row1.PORel_PORelNum = row2.RelNo
WHERE row2.PONum IS NULL AND row1.PODetail_OrderQty <> row1.Calculated_ArrivedQty ";

                if (userType.ToUpper() == "BUYER")
                    query += " AND PurAgent_EMailAddress = '" + Email + "' ";
                else if (userType.ToUpper() == "SUPPLIER")
                    query += " AND Vendor_EMailAddress = '" + Email + "' ";

                Result = oDAL.GetObject(query);
                if (Result != null)
                    AllOpen = Result.ToString();

                //Pending
                query = @"SELECT COUNT(*) 
FROM dbo.tblPurchaseOrder AS row1
INNER JOIN SRM.BuyerPO AS row2
    ON row1.POHeader_PONum = row2.PONum
    AND row1.PODetail_POLine = row2.[LineNo]
    AND row1.PORel_PORelNum = row2.RelNo
WHERE  row1.PODetail_OrderQty <> row1.Calculated_ArrivedQty ";
                if (userType.ToUpper() == "BUYER")
                {
                    query += " AND row1.PurAgent_EMailAddress = '" + Email + "' ";
                }
                else if (userType.ToUpper() == "SUPPLIER")
                {
                    query += " AND row1.Vendor_EMailAddress = '" + Email + "' ";
                }

                Result = oDAL.GetObject(query);
                if (Result != null)
                    Pending = Result.ToString();

                //Late
                query = @"select Count(*) from [dbo].[tblPurchaseOrder]
Where Calculated_ArrivedDate > Calculated_DueDate AND PODetail_OrderQty <> Calculated_ArrivedQty ";
                if (userType.ToUpper() == "BUYER")
                {
                    query += "AND PurAgent_EMailAddress = '" + Email + "' ";
                }
                else if (userType.ToUpper() == "SUPPLIER")
                {
                    query += "AND Vendor_EMailAddress = '" + Email + "' ";
                }

                Result = oDAL.GetObject(query);
                if (Result != null)
                    Late = Result.ToString();



                //Arrived
                query = @"select COUNT(*) from [dbo].[tblPurchaseOrder]
Where Calculated_ArrivedDate < Calculated_DueDate AND PODetail_OrderQty = Calculated_ArrivedQty  ";
                if (userType.ToUpper() == "BUYER")
                {
                    query += "AND PurAgent_EMailAddress = '" + Email + "' ";
                }
                else if (userType.ToUpper() == "SUPPLIER")
                {
                    query += "AND Vendor_EMailAddress = '" + Email + "' ";
                }
                Result = oDAL.GetObject(query);
                if (Result != null)
                    Arrived = Result.ToString();

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
    INNER JOIN 
        [SRM].[VendorCommunication] V 
        ON B.GUID = V.GUID 
        AND B.VendorName = V.VendorName  
        AND CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo) = V.PONo
    WHERE 
        B.CommunicationStatus = 'Completed' <BuyerSupplierClasue>
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
                    query = query.Replace("<BuyerSupplierClasue>", " AND V.BuyerEmail = '" + Email + "' ");
                else if (userType == "SUPPLIER")
                    query = query.Replace("<BuyerSupplierClasue>", " AND V.VendorEmail = '" + Email + "' ");
                else
                    query = query.Replace("<BuyerSupplierClasue>", " ");

                DataTable dtUpdate = oDAL.GetData(query);
                if (dtUpdate.Rows.Count > 0)
                    Update = dtUpdate.Rows.Count.ToString();
                else
                    Update = "0";
            }
            else
            {
                //All Open
                query = @"SELECT COUNT(*) 
FROM dbo.tblPurchaseOrder AS row1
LEFT JOIN SRM.BuyerPO AS row2
    ON row1.POHeader_PONum = row2.PONum
    AND row1.PODetail_POLine = row2.[LineNo]
    AND row1.PORel_PORelNum = row2.RelNo
WHERE row2.PONum IS NULL AND row1.PODetail_OrderQty<> row1.Calculated_ArrivedQty ";
                Result = oDAL.GetObject(query);
                if (Result != null)
                    AllOpen = Result.ToString();

                //Pending
                query = @"SELECT COUNT(*) 
FROM dbo.tblPurchaseOrder AS row1
INNER JOIN SRM.BuyerPO AS row2
    ON row1.POHeader_PONum = row2.PONum
    AND row1.PODetail_POLine = row2.[LineNo]
    AND row1.PORel_PORelNum = row2.RelNo
WHERE  row1.PODetail_OrderQty <> row1.Calculated_ArrivedQty ";

                Result = oDAL.GetObject(query);
                if (Result != null)
                    Pending = Result.ToString();

                //Late
                query = @"select Count(*) from [dbo].[tblPurchaseOrder]
Where Calculated_ArrivedDate > Calculated_DueDate AND PODetail_OrderQty <> Calculated_ArrivedQty ";

                Result = oDAL.GetObject(query);
                if (Result != null)
                    Late = Result.ToString();



                //Arrived
                query = @"select COUNT(*) from [dbo].[tblPurchaseOrder]
Where Calculated_ArrivedDate < Calculated_DueDate AND PODetail_OrderQty = Calculated_ArrivedQty ";

                Result = oDAL.GetObject(query);
                if (Result != null)
                    Arrived = Result.ToString();
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
         '<BuyerSupplierClasue>'
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
                    query = query.Replace("<BuyerSupplierClasue>", " AND V.BuyerEmail = '" + Email + "' ");
                else if (userType == "SUPPLIER")
                    query = query.Replace("<BuyerSupplierClasue>", " AND V.VendorEmail = '" + Email + "' ");
                else
                    query = query.Replace("'<BuyerSupplierClasue>'", " ");

                DataTable dtUpdate = oDAL.GetData(query);
                if (dtUpdate.Rows.Count > 0)
                    Update = dtUpdate.Rows.Count.ToString();
                else
                    Update = "0";
            }

        }

        public void Get_Widget_List()
        {
            string query = string.Empty;
            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            bool isAdmin = Convert.ToBoolean(HttpContext.Current.Session["isAdmin"]);
            if (isAdmin)
            {
                query = @"
SELECT  WidgetId
	  , WidgetIcon
	  , WidgetTitle
	  , WidgetType
	  , WidgetMinSize
	  , WidgetMaxSize 
FROM SRM.Widgets W
WHERE WidgetStatus = 1 ";
            }
            else
            {
                query = @"
-- It is getting common widgets
SELECT  WidgetId
	  , WidgetIcon
	  , WidgetTitle
	  , WidgetType
	  , WidgetMinSize
	  , WidgetMaxSize 
FROM SRM.Widgets W
WHERE W.ForMnuId = 0 AND
	  WidgetStatus = 1  

UNION

-- It is getting widget by mnu assigned to a user
SELECT WidgetId
	 , WidgetIcon
	 , WidgetTitle
	 , WidgetType
	 , WidgetMinSize
	 , WidgetMaxSize 
FROM SRM.Widgets W
INNER JOIN SRM.UserMnuX UMX ON UMX.MnuId = W.ForMnuId
WHERE WidgetStatus = 1 AND 
	  UMX.UserId = '@LOGON_USER'
ORDER BY WidgetTitle ";
            }


            query = query.Replace("@LOGON_USER", HttpContext.Current.Session["SigninId"].ToString());
            DataTable dtWidgetsEmployee = oDAL.GetData(query);

            lst_widgets = ConvertDtToList(dtWidgetsEmployee);
        }

        public void Get_Employee_Widgets()
        {
            string query = string.Empty;
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);
            int employee_id = Convert.ToInt32(HttpContext.Current.Session["SigninId"]);
            bool isAdmin = Convert.ToBoolean(HttpContext.Current.Session["isAdmin"]);
            if (isAdmin)
            {
                query = @"
SELECT W.WidgetId
     , W.WidgetTitle
     , W.WidgetType
     , W.WidgetMinSize
     , W.WidgetMaxSize
     , WidgetPositionX
     , WidgetPositionY
     , WidgetSizeX
     , WidgetSizeY 
FROM SRM.Widgets W 
INNER JOIN SRM.WidgetsEmp E ON E.WidgetId = W.WidgetId 
WHERE W.WidgetStatus = 1 AND E.EmpId = @EMP_ID
";
            }
            else
            {
                query = @"
SELECT W.WidgetId
     , W.WidgetTitle
     , W.WidgetType
     , W.WidgetMinSize
     , W.WidgetMaxSize
     , WidgetPositionX
     , WidgetPositionY
     , WidgetSizeX
     , WidgetSizeY 
FROM SRM.Widgets W 
INNER JOIN SRM.WidgetsEmp E ON E.WidgetId = W.WidgetId 
WHERE W.ForMnuId = 0 AND
	  WidgetStatus = 1 AND
	  E.EmpId = @EMP_ID

UNION

SELECT W.WidgetId
     , W.WidgetTitle
     , W.WidgetType
     , W.WidgetMinSize
     , W.WidgetMaxSize
     , WidgetPositionX
     , WidgetPositionY
     , WidgetSizeX
     , WidgetSizeY 
FROM SRM.Widgets W 
INNER JOIN SRM.WidgetsEmp E ON E.WidgetId = W.WidgetId 
INNER JOIN SRM.UserMnuX UMX ON 
           UMX.MnuId = W.ForMnuId AND 
           UMX.UserId  = CAST(E.EmpId AS VARCHAR)
WHERE W.WidgetStatus = 1 AND E.EmpId = @EMP_ID
";
            }


            query = query.Replace("@EMP_ID", employee_id.ToString());

            DataTable dtWidgetsEmployee = portal_db.GetData(query);

            lst_employee_widgets = new List<Employee_Widgets>();
            Employee_Widgets oEmpWidget;
            foreach (DataRow dr in dtWidgetsEmployee.Rows)
            {
                oEmpWidget = new Employee_Widgets();
                oEmpWidget.WidgetId = dr["WidgetId"].ToString();
                oEmpWidget.WidgetTitle = dr["WidgetTitle"].ToString();
                oEmpWidget.WidgetType = dr["WidgetType"].ToString();
                oEmpWidget.WidgetMinSize = dr["WidgetMinSize"].ToString();
                oEmpWidget.WidgetMaxSize = dr["WidgetMaxSize"].ToString();
                oEmpWidget.WidgetPositionY = dr["WidgetPositionY"].ToString();
                oEmpWidget.WidgetPositionX = dr["WidgetPositionX"].ToString();
                oEmpWidget.WidgetSizeX = dr["WidgetSizeX"].ToString();
                oEmpWidget.WidgetSizeY = dr["WidgetSizeY"].ToString();
                lst_employee_widgets.Add(oEmpWidget);
            }
        }

        public void Get_Count_Widget(string widget_id)
        {
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);

            string query = string.Empty;

            query = "SELECT WidgetQuery, WidgetDesc, ReportURL, CountType, BackgroundColor, ConType FROM SRM.Widgets WHERE WidgetId = " + widget_id + " ";
            DataTable dtWidget = portal_db.GetData(query);
            widget_desc = dtWidget.Rows[0]["WidgetDesc"].ToString();
            widget_count_report_URL = dtWidget.Rows[0]["ReportURL"].ToString();
            widget_count_bg = dtWidget.Rows[0]["BackgroundColor"].ToString();
            query = dtWidget.Rows[0]["WidgetQuery"].ToString();
            query = change_query_params(query);
            query = query.Replace("<programId>", HttpContext.Current.Session["ProgramId"].ToString());

            string connection_type = dtWidget.Rows[0]["ConType"].ToString();
            cDAL widget_db = get_widget_connection(connection_type.ToUpper());
            //string widget_count_type = dtWidget.Rows[0]["CountType"].ToString();
            //if (widget_count_type.ToLower() == "cost")
            //    widget_count_count = cCommon.SetFormat(widget_db.GetObject(query), cCommon.Format.ForQty).ToString();
            //else
            string count_value = cCommon.SetFormat(widget_db.GetObject(query), cCommon.Format.ForQty).ToString();
            if (count_value.Length == 0)
                count_value = "0";
            widget_count_count = count_value;
        }

        public void Get_Group_Counts_Widget(string widget_id)
        {
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);

            string query = string.Empty;

            query = "SELECT A.WidgetTitle, A.WidgetQuery, A.ReportURL, A.CountType, A.BackgroundColor, A.ConType  ";
            query += "FROM SRM.Widgets A ";
            query += "WHERE A.GroupId = " + widget_id + " ";
            DataTable dtWidget = portal_db.GetData(query);
            string connection_type = dtWidget.Rows[0]["ConType"].ToString();
            cDAL widget_db = get_widget_connection(connection_type.ToUpper());
            lst_widget_group_counts = new List<ArrayList>();
            ArrayList count_widget = new ArrayList();

            for (int i = 0; i < dtWidget.Rows.Count; i++)
            {
                count_widget = new ArrayList();
                count_widget.Add(dtWidget.Rows[i]["WidgetTitle"].ToString());
                count_widget.Add(dtWidget.Rows[i]["ReportURL"].ToString());
                count_widget.Add(dtWidget.Rows[i]["CountType"].ToString());
                count_widget.Add(dtWidget.Rows[i]["BackgroundColor"].ToString());

                query = dtWidget.Rows[i]["WidgetQuery"].ToString();
                query = change_query_params(query);
                string count_value = cCommon.SetFormat(widget_db.GetObject(query), cCommon.Format.ForQty).ToString();
                if (count_value.Length == 0)
                    count_value = "0";
                count_widget.Add(count_value);
                lst_widget_group_counts.Add(count_widget);
            }
        }

        public void Get_List_Widget(string widget_id)
        {
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);


            #region SQL
            string query = string.Empty;
            query = "SELECT WidgetQuery, ConType FROM SRM.Widgets WHERE WidgetId = " + widget_id + " ";
            DataTable dtWidget = portal_db.GetData(query);

            query = dtWidget.Rows[0]["WidgetQuery"].ToString();
            query = change_query_params(query);


            string connection_type = dtWidget.Rows[0]["ConType"].ToString();
            cDAL widget_db = get_widget_connection(connection_type.ToUpper());
            DataTable dt = widget_db.GetData(query);
            lst_widget_list = cCommon.ConvertDtToArrayList(dt);
            #endregion
        }

        public void Get_HyperList_Widget(string widget_id)
        {
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);

            string query = string.Empty;

            query = "SELECT WidgetQuery, ConType FROM SRM.Widgets WHERE WidgetId = " + widget_id + " ";
            DataTable dtWidget = portal_db.GetData(query);

            query = dtWidget.Rows[0]["WidgetQuery"].ToString();
            query = change_query_params(query);

            string connection_type = dtWidget.Rows[0]["ConType"].ToString();
            cDAL widget_db = get_widget_connection(connection_type.ToUpper());

            DataTable dt = widget_db.GetData(query);
            lst_widget_hyperlist = cCommon.ConvertDtToArrayList(dt);
        }

        public void Get_Table_Widget(string widget_id)
        {
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);


            string query = string.Empty;

            query = "SELECT WidgetQuery, ColumnFormat, ColumnFormat, ConType FROM SRM.Widgets WHERE WidgetId = " + widget_id + " ";
            DataTable dtWidget = portal_db.GetData(query);
            string table_headers = dtWidget.Rows[0]["ColumnFormat"].ToString();

            query = dtWidget.Rows[0]["WidgetQuery"].ToString();
            query = change_query_params(query);


            widget_table_column_format = dtWidget.Rows[0]["ColumnFormat"].ToString();

            string connection_type = dtWidget.Rows[0]["ConType"].ToString();
            cDAL widget_db = get_widget_connection(connection_type.ToUpper());
            DataTable dt = widget_db.GetData(query);
            widget_table_cols_count = dt.Columns.Count;
            lst_widget_table = cCommon.ConvertDtToArrayList(dt);
        }

        public void Get_Chart_Widget(string widget_id)
        {
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);


            string query = string.Empty;

            query = "SELECT WidgetTitle, WidgetQuery, ColumnFormat, BackgroundColor, ConType FROM SRM.Widgets WHERE WidgetId = " + widget_id + " ";
            DataTable dtWidget = portal_db.GetData(query);


            query = dtWidget.Rows[0]["WidgetQuery"].ToString();
            query = change_query_params(query);
            query = query.Replace("<programId>", HttpContext.Current.Session["ProgramId"].ToString());

            string connection_type = dtWidget.Rows[0]["ConType"].ToString();
            cDAL widget_db = get_widget_connection(connection_type.ToUpper());
            DataTable dt = widget_db.GetData(query);
            if (dt.Rows.Count > 0)
            {
                string count_value = dt.Rows[0]["units"].ToString();
                if (count_value.Length == 0)
                    count_value = "0";
                widget_count_count = count_value;
            }
            else
                widget_count_count = "0";

            chart_data = new Dictionary<string, object>();
            List<Dictionary<string, object>> data_sets = new List<Dictionary<string, object>>();


            string[] x;
            //if (dt.Rows[0]["x"] == string.Empty)
            //    x = new string[dt.Columns.Count - 1];
            //else
            x = new string[dt.Rows.Count];
            string[] headers = dtWidget.Rows[0]["ColumnFormat"].ToString().Split(',');
            string[] back_color = dtWidget.Rows[0]["BackgroundColor"].ToString().Split(',');
            chart_title = dtWidget.Rows[0]["WidgetTitle"].ToString();


            Dictionary<string, object> item;
            string[] y;
            for (int i = 1; i < dt.Columns.Count; i++)
            {
                item = new Dictionary<string, object>();
                item["label"] = headers[i - 1];
                y = new string[dt.Rows.Count];
                //if (dt.Rows[0]["x"] == string.Empty)
                //    x[i - 1] = headers[i - 1];

                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    //if (dt.Rows[j]["x"] != string.Empty)
                    x[j] = dt.Rows[j]["x"].ToString(); //e.g. no group, android parts

                    y[j] = dt.Rows[j][dt.Columns[i].ColumnName].ToString();
                }
                item["data"] = y;
                item["backgroundColor"] = back_color[i - 1]; //background color define;
                data_sets.Add(item);
            }

            chart_data["labels"] = x;
            chart_data["datasets"] = data_sets;
        }

        public void Get_Report_Info(string link)
        {
            if (!link.Contains("\\\\"))
                link = link.Replace("\\", "\\\\");
            if (link.ToLower().Contains("error"))
            {
                report_URL = link;
                return;
            }
            link = change_link_params(link);
            if (link.ToLower().Contains("http://") || link.ToLower().Contains("http://"))
            {
                report_URL = link;
                report_is_internal = false;
                return;
            }
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);

            string query = string.Empty;


            query = "SELECT RptCode as report_code, MnuTitle as report_title, MnuTitleShort as report_title_short,";
            query += "MnuTarget as report_target, DesignedBy as report_designed_by   ";
            query += "FROM SRM.Mnu ";
            query += "WHERE MnuHyperlink = '" + link + "' ";
            DataTable dt = portal_db.GetData(query);
            report_code = dt.Rows[0]["report_code"].ToString();
            report_title_short = dt.Rows[0]["report_title_short"].ToString();
            report_target = dt.Rows[0]["report_target"].ToString();
            report_designed_by = dt.Rows[0]["report_designed_by"].ToString();
            report_URL = link;
        }

        public void Save_Employee_Widgets(List<Employee_Widgets> lst)
        {
            cDAL portal_db = new cDAL(cDAL.ConnectionType.ACTIVE);

            string query = "";
            //INSERTING WIDGETS FOR SINGLE EMPLOYEE
            int employee_id = Convert.ToInt32(HttpContext.Current.Session["SigninId"]);

            query = "DELETE FROM SRM.WidgetsEmp WHERE EmpId = " + employee_id + " ";
            // query += "AND WidgetId IN (SELECT WidgetId FROM EP.Widgets WHERE Company = '" + HttpContext.Current.Session["CompanyCode"].ToString() + "')";
            portal_db.AddQuery(query);
            if (lst != null)
            {
                foreach (var row in lst)
                {
                    query = "INSERT INTO SRM.WidgetsEmp (EmpID, WidgetId,  WidgetPositionY, WidgetPositionX, ";
                    query += "WidgetSizeX, WidgetSizeY) VALUES (";
                    query += "" + Convert.ToInt32(HttpContext.Current.Session["SigninId"]) + ",";
                    query += "'" + row.WidgetId.Remove(0, 1) + "',";
                    query += "'" + row.WidgetPositionY + "',";
                    query += "'" + row.WidgetPositionX + "',";
                    query += "'" + row.WidgetSizeX + "',";
                    query += "'" + row.WidgetSizeY + "'";
                    query += ")";
                    portal_db.AddQuery(query);
                }
            }
            portal_db.Commit();
        }

        public List<Dictionary<string, object>> ConvertDtToList(DataTable dt)
        {
            List<Dictionary<string, object>>
            lstRows = new List<Dictionary<string, object>>();
            Dictionary<string, object> dictRow = null;

            foreach (DataRow dr in dt.Rows)
            {
                dictRow = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dictRow.Add(col.ColumnName, dr[col]);
                }
                lstRows.Add(dictRow);
            }
            return lstRows;
        }

        private string change_query_params(string query)
        {
            //string epic_company = HttpContext.Current.Session["CompanyCode"].ToString();
            //string epic_plant = HttpContext.Current.Session["CompanyPlant"].ToString();
            int employee_id = Convert.ToInt32(HttpContext.Current.Session["SigninId"]);
            string PrgramName = HttpContext.Current.Session["ProgramName"].ToString();
            //string win_login = HttpContext.Current.Session["LogonUser"].ToString();

            //query = query.Replace("@Company", "'" + epic_company + "'");
            //query = query.Replace("@Plant", "'" + epic_plant + "'");
            query = query.Replace("@EmpId", "'" + employee_id.ToString() + "'");
            query = query.Replace("@ProgramName", "'" + PrgramName + "'");
            //query = query.Replace("@WinLogin", "'" + win_login + "'");
            //query = query.Replace("\r\n", "");
            return query;
        }

        private string change_link_params(string link)
        {
            //string epic_company = HttpContext.Current.Session["CompanyCode"].ToString();
            //string epic_plant = HttpContext.Current.Session["CompanyPlant"].ToString();
            int employee_id = Convert.ToInt32(HttpContext.Current.Session["SigninId"]);
            //string win_login = HttpContext.Current.Session["LogonUser"].ToString();

            //link = link.Replace("@Company", epic_company);
            //link = link.Replace("@Plant", epic_plant);
            link = link.Replace("@EmpId", employee_id.ToString());
            //link = link.Replace("@WinLogin", win_login);

            return link;
        }
        private cDAL get_widget_connection(string connection_type)
        {
            switch (connection_type)
            {
                case "PROD":
                    return new cDAL(cDAL.ConnectionType.ACTIVE);
                    break;
                case "TEST":
                    return new cDAL(cDAL.ConnectionType.ACTIVE);
                default:
                    return new cDAL(cDAL.ConnectionType.ACTIVE);
                    break;
            }

        }

        public bool SamsunHorizon()
        {
            return true;
        }

        public void getStations()
        {
            DateTime ClientDate = DateTime.Now;

            ClientDate = ClientDate.ToUniversalTime();
            ClientDate = ClientDate.AddHours(1);
            string s = ClientDate.ToString("HH:mm:ss");
            //string Timeonly = ClientDate.ToLongTimeString();

            //string s = Timeonly.ToString);

            var now = TimeSpan.Parse(s);
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = "SELECT RECNUM, STATION, SHIFT, TARGET, crnt, PASS, LOSS, HOLD FROM RPT.TEST ";
            dtStations = new DataTable();
            dtStations = oDAL.GetData(query);
        }
        public void getTotalCrnt()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = "SELECT SHIFT, SUM(crnt) AS CURRENT_UNIT, SUM(TARGET) AS TARGET  FROM RPT.TEST GROUP BY SHIFT ";
            dtCurrent = new DataTable();
            dtCurrent = oDAL.GetData(query);
        }
        public bool getChart()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = "SELECT STATION, SHIFT, TARGET, crnt, PASS, LOSS, HOLD FROM RPT.TEST  ";
            dtChart = new DataTable();
            dtChart = oDAL.GetData(query);
            dataChart = cCommon.ConvertDtToArrayList(cCommon.GenerateTransposedTable(dtChart));
            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }

        public string SavePOWidgets(string WidgetTitle, string SupplierId, string SupplierName, string WidgetDesc, string NoOfDays, string[] POListId, List<Dictionary<string, object>> poItems)
        {
            string sql = "";
            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            Guid newGuid = Guid.NewGuid();
            string SigninId = HttpContext.Current.Session["SigninId"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();

            string commaSeparatedPO = string.Join(",", POListId);
            int count = POListId.Length;
            DateTime expiryDate = DateTime.Now.AddDays(Convert.ToInt32(NoOfDays));

            sql = @"
INSERT INTO [dbo].[Widgets]
   ([WidgetTitle]
   ,[VendorId]
   ,[VendorName]
   ,[WidgetDesc]
   ,[PO] 
   ,[Days]
   ,[Count]
   ,[Email]
   ,[IsActive]
   ,[ExpireOn]
   ,[CreatedBy]
   ,[CreatedOn])
VALUES
   ('<WidgetTitle>'
    ,'<VendorId>'
   ,'<VendorName>'
   ,'<WidgetDesc>'
   ,'<PO>'
   ,'<Days>'    
   ,'<Count>'
   ,'<Email>'
   ,'<IsActive>'
   ,'<ExpireOn>'
   ,'<CreatedBy>'
   ,'<CreatedOn>')";

            sql = sql.Replace("<WidgetTitle>", WidgetTitle);
            sql = sql.Replace("<VendorId>", SupplierId);
            sql = sql.Replace("<VendorName>", SupplierName);
            sql = sql.Replace("<WidgetDesc>", WidgetDesc);
            sql = sql.Replace("<PO>", commaSeparatedPO);
            sql = sql.Replace("<Days>", NoOfDays);
            sql = sql.Replace("<Count>", count.ToString());
            sql = sql.Replace("<Email>", Email);
            sql = sql.Replace("<IsActive>", "1");
            sql = sql.Replace("<ExpireOn>", expiryDate.ToString("yyyy-MM-dd"));
            sql = sql.Replace("<CreatedBy>", Email);
            sql = sql.Replace("<CreatedOn>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            oDAL.AddQuery(sql);
            oDAL.Commit();


            if (!oDAL.HasErrors)
            {
                Result = "Save";
            }
            else
            {
                Result = "NotSave";
            }
            return Result;
        }


        public string EditPOWidgets(string Id, string WIdgetTitle, string SupplierId, string SupplierName, string WidgetDesc, string NoOfDays, string[] POListId)
        {
            string query = string.Empty;
            oDAL = new cDAL(cDAL.ConnectionType.INIT);

            string SigninId = HttpContext.Current.Session["SigninId"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            string sql = "";
            //sql = "Delete from [dbo].[Widgets] Where Token = '" + Id + "' ";
            //oDAL.Execute(sql);

            string commaSeparatedPO = string.Join(",", POListId);
            int count = POListId.Length;
            DateTime expiryDate = DateTime.Now.AddDays(Convert.ToInt32(NoOfDays));

            sql = @"UPDATE [dbo].[Widgets]
   SET [WidgetTitle] = '<WidgetTitle>'
      ,[VendorId] = '<SupplierId>'
      ,[VendorName] = '<SupplierName>'
      ,[WidgetDesc] = '<WidgetDesc>'
      ,[PO] = '<PO>'
      ,[Days] = '<Days>'
      ,[Count] = '<Count>'
      ,[UserId] = '<UserId>'
      ,[Email] = '<Email>'
      ,[ExpireOn] = '<ExpireOn>'
      ,[CreatedBy] = '<CreatedBy>'
      ,[CreatedOn] = '<CreatedOn>'
 WHERE Id = '<Id>' ";

            sql = sql.Replace("<WidgetTitle>", WIdgetTitle);
            sql = sql.Replace("<SupplierId>", SupplierId);
            sql = sql.Replace("<SupplierName>", SupplierName);
            sql = sql.Replace("<WidgetDesc>", WidgetDesc);
            sql = sql.Replace("<PO>", commaSeparatedPO);
            sql = sql.Replace("<Days>", NoOfDays);
            sql = sql.Replace("<Count>", count.ToString());
            sql = sql.Replace("<UserId>", SigninId);
            sql = sql.Replace("<Email>", Email);
            sql = sql.Replace("<ExpireOn>", expiryDate.ToString("yyyy-MM-dd"));
            sql = sql.Replace("<CreatedBy>", Email);
            sql = sql.Replace("<CreatedOn>", DateTime.Now.ToString());
            sql = sql.Replace("<Id>", Id);

            oDAL.Execute(sql);

            if (!oDAL.HasErrors)
            {
                Result = "Save";
            }
            else
            {
                Result = "NotSave";
            }
            return Result;
        }

        public string DeletePOWidgets(string WIdgetId)
        {
            string query = string.Empty;
            oDAL = new cDAL(cDAL.ConnectionType.INIT);

            string sql = @"UPDATE [dbo].[Widgets] SET IsActive = 0 where id = '" + WIdgetId + "' ";
            oDAL.Execute(sql);

            if (!oDAL.HasErrors)
            {
                Result = "Save";
            }
            else
            {
                Result = "NotSave";
            }
            return Result;
        }

        #endregion
    }
}

public class POItem
{
    public string Value { get; set; }
    public string Text { get; set; }
    public string Id { get; set; }
    public string PO { get; set; }  // Concatenation of POHeader, POLine, and PORel
    public string SupplierEmail { get; set; }
    public string PoHeader { get; set; }
    public string PoLine { get; set; }
    public string PoRel { get; set; }
    public string PartNo { get; set; }
    public string PartDesc { get; set; }
    public string UOM { get; set; }
    public string OrderDate { get; set; }
    public string DueDate { get; set; }
    public string OrderQty { get; set; }
    public string ArrivedQty { get; set; }
    public string FinalQty { get; set; }
    public string Price { get; set; }
    public string VendorId { get; set; }
    public string VendorName { get; set; }
    public string BuyerId { get; set; }
    public string SupplierCompany { get; set; }
}