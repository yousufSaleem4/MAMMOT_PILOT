using IP.Classess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace PlusCP.Classess
{
    public class NewPOCommon
    {
        public List<Hashtable> lstPO { get; set; }

        string DeployMode = HttpContext.Current.Session["DefaultDB"].ToString();
        DataTable dt = new DataTable();
        cLog oLog;
        private static readonly HttpClient _httpClient = new HttpClient();
      
       
        // Demo Code
        public DataTable GetPOListFromSQL(string POStatus)
        {
            DataTable dt = new DataTable();
            dt = GetPOByStatusDemo(POStatus);
            return dt;

        }
        public DataTable GetPODtlFromSQL(string PO)
        {
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            DataTable dtERPPODtl = new DataTable();
            object result = "";
            string sql = "select * from [dbo].[tblPurchaseOrder] WHERE POHeader_PONum = '" + PO + "' ";
            if (userType.ToUpper() == "BUYER")
            {
                sql += "AND PurAgent_EMailAddress = '" + Email + "' ";
            }
            dtERPPODtl = oDAL.GetData(sql);
            return dtERPPODtl;

        }
        public string GetIdleCountDemo()
        {
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            DataTable dtERPPO = new DataTable();
            object result = "";
            string sql = @"select Count(*) from  [dbo].[POHeader] where POHeader_PONum IN (SELECT  POD.POHeader_PONum
                                FROM tblPurchaseOrder POD
                                LEFT JOIN SRM.BuyerPO POP 
                                    ON POD.POHeader_PONum = POP.PONum 
                                    AND POD.PODetail_POLine = POP.[LineNo]
                                    AND POD.PORel_PORelNum = POP.RelNo
                                GROUP BY POD.POHeader_PONum
                                HAVING COUNT(DISTINCT CASE WHEN POP.CommunicationStatus = 'Completed' THEN POP.[LineNo] END) 
                                       < COUNT(DISTINCT POD.PODetail_POLine))  ";
            if (userType.ToUpper() == "BUYER")
            {
                sql += "AND purAgent_EmailAddress = '" + Email + "' ";
            }

            result = oDAL.GetObject(sql);

            if (result != null)
                return result.ToString();
            else
                return "0";
        }
        public string GetInProcessCount()
        {
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            DataTable dtERPPO = new DataTable();
            object result = "";
            string sql = @"SELECT COUNT(*) AS InProcess
                                FROM [dbo].[POHeader] PH
                                JOIN (
                                    SELECT PONum, COUNT(*) AS PO_Count
                                    FROM SRM.BuyerPO
                                    WHERE CommunicationStatus <> 'Completed'
                                    GROUP BY PONum
                                ) BP ON PH.POHeader_PONum = BP.PONum
                                WHERE 1=1  ";
            if (userType.ToUpper() == "BUYER")
            {
                sql += "AND BuyerEmail = '" + Email + "' ";
            }

            result = oDAL.GetObject(sql);

            if (result != null)
                return result.ToString();
            else
                return "0";
        }
        public string GetInProcessCountDemo()
        {
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            DataTable dtERPPO = new DataTable();
            object result = "";
            string sql = @"select Count(*) from  [dbo].[POHeader] where POHeader_PONum  IN (SELECT POD.POHeader_PONum
                                FROM tblPurchaseOrder POD
                                LEFT JOIN SRM.BuyerPO POP 
                                    ON POD.POHeader_PONum = POP.PONum 
                                    AND POD.PODetail_POLine = POP.[LineNo]
                                    AND POD.PORel_PORelNum = POP.RelNo
                                GROUP BY POD.POHeader_PONum
                                HAVING COUNT(DISTINCT CASE WHEN POP.CommunicationStatus = 'Completed' THEN POP.[LineNo] END) 
                                       < COUNT(DISTINCT CASE WHEN POP.CommunicationStatus IS NOT NULL THEN POD.PODetail_POLine END))   ";
            if (userType.ToUpper() == "BUYER")
            {
                sql += "AND BuyerEmail = '" + Email + "' ";
            }

            result = oDAL.GetObject(sql);

            if (result != null)
                return result.ToString();
            else
                return "0";
        }
        public string GetCompletedCount()
        {
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            DataTable dtERPPO = new DataTable();
            object result = "";
            string sql = "select count(*) from srm.BuyerPO WHERE CommunicationStatus = 'Completed' ";
            if (userType.ToUpper() == "BUYER")
            {
                sql += "AND BuyerEmail = '" + Email + "' ";
            }

            result = oDAL.GetObject(sql);

            if (result != null)
                return result.ToString();
            else
                return "0";
        }
        public DataTable GetPOByStatusDemo(string POStatus)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            DataTable dt = new DataTable();
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();
            string sql = "";
            if (POStatus.ToUpper() == "ALL")
            {
                sql = "SELECT * from [dbo].[POHeader] ";
                if (userType == "Buyer")
                {
                    sql += "WHERE purAgent_EmailAddress = '" + Email + "' ";
                }
                dt = oDAL.GetData(sql);
                return dt;
            }
            else if (POStatus.ToUpper() == "IDLE" || POStatus.ToUpper() == "ALL OPEN")
            {
                sql = @"select * from  [dbo].[POHeader] where POHeader_PONum  IN (SELECT  POD.POHeader_PONum
FROM tblPurchaseOrder POD
LEFT JOIN SRM.BuyerPO POP
    ON POD.POHeader_PONum = POP.PONum
    AND POD.PODetail_POLine = POP.[LineNo]
    AND POD.PORel_PORelNum = POP.RelNo
GROUP BY POD.POHeader_PONum
HAVING COUNT(DISTINCT CASE WHEN POP.CommunicationStatus = 'Completed' THEN POP.[LineNo] END)
       < COUNT(DISTINCT POD.PODetail_POLine)) ";
                if (userType == "Buyer")
                {
                    sql += "AND purAgent_EmailAddress = '" + Email + "' ";
                }
                dt = oDAL.GetData(sql);
                return dt;
            }
            else if (POStatus.ToUpper() == "PENDING")
            {
                sql = @"select * from  [dbo].[POHeader] where POHeader_PONum  IN (SELECT POD.POHeader_PONum
                            FROM tblPurchaseOrder POD
                            LEFT JOIN SRM.BuyerPO POP 
                                ON POD.POHeader_PONum = POP.PONum 
                                AND POD.PODetail_POLine = POP.[LineNo]
                                AND POD.PORel_PORelNum = POP.RelNo
                            GROUP BY POD.POHeader_PONum
                            HAVING COUNT(DISTINCT CASE WHEN POP.CommunicationStatus = 'Completed' THEN POP.[LineNo] END) 
                                < COUNT(DISTINCT CASE WHEN POP.CommunicationStatus IS NOT NULL THEN POD.PODetail_POLine END))   ";
                if (userType == "Buyer")
                {
                    sql += "AND purAgent_EmailAddress = '" + Email + "' ";
                }
                dt = oDAL.GetData(sql);
                return dt;
            }
            else if (POStatus.ToUpper() == "LATE")
            {
                sql = "select *, Calculated_OrderDate AS POHeader_OrderDate from  tblPurchaseOrder Where Calculated_OrderDate > Calculated_DueDate AND PODetail_OrderQty <> Calculated_ArrivedQty ";
                if (userType == "Buyer")
                {
                    sql += "AND purAgent_EmailAddress = '" + Email + "' ";
                }
                dt = oDAL.GetData(sql);
                return dt;
            }
            else if (POStatus.ToUpper() == "ARRIVED")
            {
                sql = "select  *, Calculated_OrderDate from  tblPurchaseOrder Where Calculated_ArrivedDate < Calculated_DueDate AND PODetail_OrderQty = Calculated_ArrivedQty  ";
                if (userType == "Buyer")
                {
                    sql += "AND purAgent_EmailAddress = '" + Email + "' ";
                }
                dt = oDAL.GetData(sql);
                return dt;
            }
            else
            {
                return dt;
            }

        }
        // End

        // New Code 
        public DataTable GetPOListFromAPI()
        {
            try
            {
                DataTable dt = new DataTable();
                string menuTitle = string.Empty;

                DataTable dtURL = new DataTable();
                dtURL = cCommon.GetEmailURL(DeployMode.ToUpper(), "APIOPENPO");
                string URL = dtURL.Rows[0]["URL"].ToString();
                var client = new RestClient(URL);
                var request = new RestRequest(dtURL.Rows[0]["PageURL"].ToString(), Method.Get);
                string userName = dtURL.Rows[0]["UserName"].ToString();
                string password = dtURL.Rows[0]["Password"].ToString();
                password = BasicEncrypt.Instance.Decrypt(password.Trim());
                // Add basic authentication header
                request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password)));

                var response = client.Execute(request);
                if (response.IsSuccessStatusCode == true)
                {
                    string jsonstring = response.Content;
                    dt = Tabulate(jsonstring);
                }
                else
                {
                    oLog = new cLog();
                    oLog.RecordError(response.ErrorMessage, response.Content, "GetPOList API Method");
                }
                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetPOList API Method");
                return dt;
            }

        }
        public DataTable GetPOAsync(string POStatus)
        {
            try
            {
                DataTable dt = new DataTable();
                if (POStatus.ToUpper() == "LATE")
                    dt = GETPOLate();
                else if (POStatus.ToUpper() == "ARRIVED")
                    dt = GETPOEarly();
                else if (POStatus.ToUpper() == "PENDING")
                    dt = GETPOPending();
                else if (POStatus.ToUpper() == "ALL")
                    dt = GetAllPOWithStatus();
                else if (POStatus.ToUpper() == "REFRESH")
                    dt = GETPORefresh();
                
                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetPOList API Method");
                return new DataTable(); // Return an empty table on failure
            }
        }
        public DataTable GETPOLate()
        {
            DataTable dt = new DataTable();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();

            string buyerClause = "";
            if (userType.ToUpper() == "BUYER")
            {
                buyerClause = $" AND PurAgent_EMailAddress = '{Email}' ";
            }

            string query = $@"
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
            WHERE 
                Calculated_ArrivedDate > Calculated_DueDate
                AND Calculated_DueDate < GETDATE()
                AND Calculated_ArrivedQty < PODetail_OrderQty
                {buyerClause}
        ) AS PO
        LEFT JOIN (
    SELECT * FROM [dbo].[BuyerPOHeader] WHERE IsActive = 1
) BPH ON PO.POHeader_PONum = BPH.PONumber 
        WHERE PO.rn = 1 ORDER BY BPH.UpdatedOn DESC, BPH.InsertedOn DESC;";

            dt = oDAL.GetData(query);

            return dt;
        }
        public DataTable GetAllPOWithStatus()
        {
            DataTable dt = new DataTable();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string userType = HttpContext.Current.Session["UserType"].ToString();
            string email = HttpContext.Current.Session["Email"].ToString();

            string buyerClause = "";
            if (userType.ToUpper() == "BUYER")
            {
                buyerClause = $" AND PurAgent_EMailAddress = '{email}' ";
            }

            string query = $@"
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
            {buyerClause}
        ) AS PO
       LEFT JOIN (
    SELECT * FROM [dbo].[BuyerPOHeader] WHERE IsActive = 1
) BPH ON PO.POHeader_PONum = BPH.PONumber
        WHERE PO.rn = 1 ORDER BY BPH.UpdatedOn DESC, BPH.InsertedOn DESC;";

            dt = oDAL.GetData(query);

            return dt;
        }
        public DataTable GETPOPending()
        {
            string query = "";
            DataTable dt = new DataTable();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();

            query = @"-- In Process
                SELECT 
                    sub.POHeader_PONum,
                    sub.POHeader_Company,
                    sub.Calculated_OrderDate AS POHeader_OrderDate,
                    sub.Vendor_VendorID,
                    sub.Vendor_Name,
                    sub.POHeader_BuyerID,
                    sub.PurAgent_Name,
                    sub.Vendor_EMailAddress,
                    sub.PurAgent_EMailAddress,
                    sub.POHeader_Approve,
                    sub.PODetail_PartNum,
                    sub.RowIdent,
                     CASE 
                     WHEN BPH.IsActive = 1 THEN BPH.Status 
                     ELSE 'New' 
                     END AS POStatus
                   FROM (
                    SELECT 
                        PD.*,
                        ROW_NUMBER() OVER (PARTITION BY PD.POHeader_PONum ORDER BY PD.RowIdent) AS rn
                    FROM [dbo].[PODetail] PD
                    INNER JOIN [SRM].[BuyerPO] BP 
                        ON PD.POHeader_PONum = BP.PONum 
                       AND PD.PODetail_POLine = BP.[LineNo]
                       AND PD.PORel_PORelNum = BP.RelNo
                       <BuyerClause> 
                    WHERE PD.PODetail_XOrderQty <> PD.Calculated_ReceivedQty
                ) AS sub
                LEFT JOIN [dbo].[BuyerPOHeader] BPH 
                    ON sub.POHeader_PONum = BPH.PONumber
                WHERE sub.rn = 1 ORDER BY BPH.UpdatedOn DESC, BPH.InsertedOn DESC; ";

            if (userType.ToUpper() == "BUYER")
                query = query.Replace("<BuyerClause>", " AND PurAgent_EMailAddress = '" + Email + "' ");
            else
                query = query.Replace("<BuyerClause>", " ");

            dt = oDAL.GetData(query);
            return dt;
        }
        public DataTable GETPOEarly()
        {
            DataTable dt = new DataTable();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string userType = HttpContext.Current.Session["UserType"].ToString();
            string Email = HttpContext.Current.Session["Email"].ToString();

            string buyerClause = "";
            if (userType.ToUpper() == "BUYER")
            {
                buyerClause = $" AND PurAgent_EMailAddress = '{Email}' ";
            }

            string query = $@"
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
            WHERE 
                Calculated_DueDate >= Calculated_ArrivedDate
                AND Calculated_ArrivedQty >= PODetail_OrderQty
                {buyerClause}
        ) AS PO
         LEFT JOIN (
    SELECT * FROM [dbo].[BuyerPOHeader] WHERE IsActive = 1
) BPH ON PO.POHeader_PONum = BPH.PONumber 
        WHERE PO.rn = 1 ORDER BY BPH.UpdatedOn DESC, BPH.InsertedOn DESC;";

            dt = oDAL.GetData(query);

            return dt;
        }
        public DataTable GETPORefresh()
        {
            DataTable dt = GETALLPOREFRESH();
            BulkInsertPO(dt);

            string fileName = ConfigurationManager.AppSettings["Key"];
            string setupConnection = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());

            using (SqlConnection con = new SqlConnection(setupConnection))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("UpsertPODetailBulk", con); // Stored procedure name
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
            dt = GetAllPOWithStatus();
            return dt;
        }
        public void BulkInsertPO(DataTable dt)
        {
            string fileName = ConfigurationManager.AppSettings["Key"];
            string setupConnection = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());

            using (SqlConnection con = new SqlConnection(setupConnection))
            {
                con.Open();

                // Clear the staging table before bulk insert
                using (SqlCommand truncate = new SqlCommand("TRUNCATE TABLE PODetail_Staging", con))
                {
                    truncate.ExecuteNonQuery();
                }

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
                {
                    bulkCopy.DestinationTableName = "PODetail_Staging";
                    bulkCopy.WriteToServer(dt);
                }
            }
        }      
        public DataTable GETALLPOREFRESH()
        {
            cLog oLog;
            try
            {
                // Force TLS 1.2
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                DataTable dt = new DataTable();
                DataTable dtURL = new DataTable();

                string userType = HttpContext.Current.Session["UserType"].ToString();
                string Email = HttpContext.Current.Session["Email"].ToString();
                string ConnectionType = HttpContext.Current.Session["DefaultDB"].ToString();

                dtURL = cCommon.GetEmailURL(ConnectionType.ToUpper(), "APIOPENPO");
                string URL = dtURL.Rows[0]["URL"].ToString();
                string pageURL = dtURL.Rows[0]["PageURL"].ToString();

                string userName = dtURL.Rows[0]["UserName"].ToString();
                string password = BasicEncrypt.Instance.Decrypt(dtURL.Rows[0]["Password"].ToString().Trim());
                string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));

                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.AllowAutoRedirect = true;

                    using (HttpClient client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromMinutes(4); // Extend timeout
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = client.GetAsync(URL + pageURL).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonString = response.Content.ReadAsStringAsync().Result;
                            dt = cCommon.Tabulate(jsonString);
                        }
                        else
                        {
                            throw new Exception($"API returned status: {response.StatusCode}");
                        }
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetAllPOAPI");
                return new DataTable();
            }
        }   
        public DataTable GetPODetails(string PO, string POStatus, string widgetId=null)
        {
            try
            {
                DataTable dt = new DataTable();
                if (POStatus.ToUpper() == "LATE")
                    dt = GETPODtlLate(PO);
                else if (POStatus.ToUpper() == "ARRIVED")
                    dt = GETPODtlEarly(PO);
                else if (POStatus.ToUpper() == "PENDING")
                    dt = GETPODtlPending(PO);
                else if (POStatus.ToUpper() == "ALL" || POStatus.ToUpper() == "REFRESH")
                    dt = GetAllPODtl(PO);
                else if (POStatus.ToUpper() == "WIDGET" )
                    dt = GetWidgetData(widgetId);


                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetPOList API Method");
                return new DataTable(); // Return an empty table on failure
            }
        }
        public DataTable GETPODtlLate(string PO)
        {
            try
            {
                string query = "";
                DataTable dt = new DataTable();
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                string userType = HttpContext.Current.Session["UserType"].ToString();
                string Email = HttpContext.Current.Session["Email"].ToString();

                query = @"SELECT [POHeader_PONum]
                            ,[PODetail_POLine]
                            ,[PORel_PORelNum]
                            ,[PODetail_IUM]
                            ,[Vendor_VendorID]
                            ,[Vendor_Name]
                            ,[POHeader_BuyerID]
                            ,[PurAgent_Name]
                            ,[PODetail_PartNum]
                            ,[PODetail_LineDesc]
                            ,[Calculated_OrderDate]
                            ,[Calculated_DueDate]
                            ,[PODetail_OrderQty]
                            ,[PODetail_XOrderQty]
                            ,[Calculated_ReceivedQty]
                            ,[Calculated_ArrivedQty]
                            ,[PORel_RelQty]
                            ,[Vendor_EMailAddress]
                            ,[PurAgent_EMailAddress]
                            ,[PODetail_UnitCost]
                            ,[PODetail_ExtCost]
                            ,[POHeader_Company]
                            ,[Calculated_OurQty]
                            ,[Calculated_UnitCost]
                            ,[Calculated_ArrivedDate]
                            ,[POHeader_ChangeDate]
                            ,[POHeader_Approve]
                            ,[POHeader_ApprovalStatus]
                            ,[RowIdent]
                         FROM [dbo].[PODetail]  						 
                         WHERE POHeader_PONum = <PO> AND Calculated_ArrivedDate > Calculated_DueDate
                AND Calculated_DueDate < GETDATE()
                AND Calculated_ArrivedQty < PODetail_OrderQty AND PODetail_XOrderQty <> Calculated_ReceivedQty <BuyerClause> ";

                query = query.Replace("<PO>", PO);

                if (userType.ToUpper() == "BUYER")
                    query = query.Replace("<BuyerClause>", " AND PurAgent_EMailAddress = '" + Email + "' ");
                else
                    query = query.Replace("<BuyerClause>", " ");

                dt = oDAL.GetData(query);
                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetPOList API Method");
                return new DataTable(); // Return an empty table on failure
            }
        }
        public DataTable GETPODtlEarly(string PO)
        {
            try
            {
                string query = "";
                DataTable dt = new DataTable();
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                string userType = HttpContext.Current.Session["UserType"].ToString();
                string Email = HttpContext.Current.Session["Email"].ToString();

                query = @"SELECT [POHeader_PONum]
                            ,[PODetail_POLine]
                            ,[PORel_PORelNum]
                            ,[PODetail_IUM]
                            ,[Vendor_VendorID]
                            ,[Vendor_Name]
                            ,[POHeader_BuyerID]
                            ,[PurAgent_Name]
                            ,[PODetail_PartNum]
                            ,[PODetail_LineDesc]
                            ,[Calculated_OrderDate]
                            ,[Calculated_DueDate]
                            ,[PODetail_OrderQty]
                            ,[PODetail_XOrderQty]
                            ,[Calculated_ReceivedQty]
                            ,[Calculated_ArrivedQty]
                            ,[PORel_RelQty]
                            ,[Vendor_EMailAddress]
                            ,[PurAgent_EMailAddress]
                            ,[PODetail_UnitCost]
                            ,[PODetail_ExtCost]
                            ,[POHeader_Company]
                            ,[Calculated_OurQty]
                            ,[Calculated_UnitCost]
                            ,[Calculated_ArrivedDate]
                            ,[POHeader_ChangeDate]
                            ,[POHeader_Approve]
                            ,[POHeader_ApprovalStatus]
                            ,[RowIdent]
                         FROM [dbo].[PODetail]  						 
                         WHERE POHeader_PONum = <PO> AND Calculated_DueDate >= Calculated_ArrivedDate
                AND Calculated_ArrivedQty >= PODetail_OrderQty AND PODetail_XOrderQty <> Calculated_ReceivedQty <BuyerClause> ";

                query = query.Replace("<PO>", PO);

                if (userType.ToUpper() == "BUYER")
                    query = query.Replace("<BuyerClause>", " AND PurAgent_EMailAddress = '" + Email + "' ");
                else
                    query = query.Replace("<BuyerClause>", " ");

                dt = oDAL.GetData(query);
                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetPOList API Method");
                return new DataTable(); // Return an empty table on failure
            }
        }
        public DataTable GETPODtlPending(string PO)
        {
            try
            {
                string query = "";
                DataTable dt = new DataTable();
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                string userType = HttpContext.Current.Session["UserType"].ToString();
                string Email = HttpContext.Current.Session["Email"].ToString();

                query = @"SELECT [POHeader_PONum]
                            ,[PODetail_POLine]
                            ,[PORel_PORelNum]
                            ,[PODetail_IUM]
                            ,[Vendor_VendorID]
                            ,[Vendor_Name]
                            ,[POHeader_BuyerID]
                            ,[PurAgent_Name]
                            ,[PODetail_PartNum]
                            ,[PODetail_LineDesc]
                            ,[Calculated_OrderDate]
                            ,[Calculated_DueDate]
                            ,[PODetail_OrderQty]
                            ,[PODetail_XOrderQty]
                            ,[Calculated_ReceivedQty]
                            ,[Calculated_ArrivedQty]
                            ,[PORel_RelQty]
                            ,[Vendor_EMailAddress]
                            ,[PurAgent_EMailAddress]
                            ,[PODetail_UnitCost]
                            ,[PODetail_ExtCost]
                            ,[POHeader_Company]
                            ,[Calculated_OurQty]
                            ,[Calculated_UnitCost]
                            ,[Calculated_ArrivedDate]
                            ,[POHeader_ChangeDate]
                            ,[POHeader_Approve]
                            ,[POHeader_ApprovalStatus]
                            ,[RowIdent]
                         FROM [dbo].[PODetail] PD 
						 INNER JOIN [SRM].[BuyerPO] BPO  ON PD.POHeader_PONum = BPO.PONum
						 AND PD.PODetail_POLine = BPO.[LineNo]
						 AND PD.PORel_PORelNum = BPO.RelNo
                         WHERE POHeader_PONum = <PO> AND PODetail_XOrderQty <> Calculated_ReceivedQty <BuyerClause> ";

                query = query.Replace("<PO>", PO);

                if (userType.ToUpper() == "BUYER")
                    query = query.Replace("<BuyerClause>", " AND PurAgent_EMailAddress = '" + Email + "' ");
                else
                    query = query.Replace("<BuyerClause>", " ");

                dt = oDAL.GetData(query);
                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetPOList API Method");
                return new DataTable(); // Return an empty table on failure
            }
        }
        public DataTable GetAllPODtl(string PO)
        {
            try
            {
                string query = "";
                DataTable dt = new DataTable();
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                string userType = HttpContext.Current.Session["UserType"].ToString();
                string Email = HttpContext.Current.Session["Email"].ToString();

                query = @"SELECT [POHeader_PONum]
                            ,[PODetail_POLine]
                            ,[PORel_PORelNum]
                            ,[PODetail_IUM]
                            ,[Vendor_VendorID]
                            ,[Vendor_Name]
                            ,[POHeader_BuyerID]
                            ,[PurAgent_Name]
                            ,[PODetail_PartNum]
                            ,[PODetail_LineDesc]
                            ,[Calculated_OrderDate]
                            ,[Calculated_DueDate]
                            ,[PODetail_OrderQty]
                            ,[PODetail_XOrderQty]
                            ,[Calculated_ReceivedQty]
                            ,[Calculated_ArrivedQty]
                            ,[PORel_RelQty]
                            ,[Vendor_EMailAddress]
                            ,[PurAgent_EMailAddress]
                            ,[PODetail_UnitCost]
                            ,[PODetail_ExtCost]
                            ,[POHeader_Company]
                            ,[Calculated_OurQty]
                            ,[Calculated_UnitCost]
                            ,[Calculated_ArrivedDate]
                            ,[POHeader_ChangeDate]
                            ,[POHeader_Approve]
                            ,[POHeader_ApprovalStatus]
                            ,[RowIdent]
                         FROM [dbo].[PODetail]  						 
                         WHERE POHeader_PONum = <PO>  <BuyerClause> ";

                query = query.Replace("<PO>", PO);

                if (userType.ToUpper() == "BUYER")
                    query = query.Replace("<BuyerClause>", " AND PurAgent_EMailAddress = '" + Email + "' ");
                else
                    query = query.Replace("<BuyerClause>", " ");

                dt = oDAL.GetData(query);
                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetPOList API Method");
                return new DataTable(); // Return an empty table on failure
            }
        }

        public DataTable GetWidgetData(string WidgetId)
        {
            try
            {
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                string userType = HttpContext.Current.Session["UserType"].ToString();
                string Email = HttpContext.Current.Session["Email"].ToString();

                string widgetQuery = @"SELECT PO FROM [dbo].[Widgets] WHERE id = '" + WidgetId + "' AND IsActive = 1";

                DataTable widgetDt = oDAL.GetData(widgetQuery);

                if (widgetDt.Rows.Count == 0)
                    return new DataTable();

                var poList = widgetDt.AsEnumerable()
                                      .Select(r => r.Field<string>("PO"))
                                      .Where(s => !string.IsNullOrEmpty(s));

                string poString = string.Join(",", poList);
                if (string.IsNullOrEmpty(poString))
                    return new DataTable();

                string query = @"
            SELECT [POHeader_PONum]
                  ,[PODetail_POLine]
                  ,[PORel_PORelNum]
                  ,[PODetail_IUM]
                  ,[Vendor_VendorID]
                  ,[Vendor_Name]
                  ,[POHeader_BuyerID]
                  ,[PurAgent_Name]
                  ,[PODetail_PartNum]
                  ,[PODetail_LineDesc]
                  ,[Calculated_OrderDate]
                  ,[Calculated_DueDate]
                  ,[PODetail_OrderQty]
                  ,[PODetail_XOrderQty]
                  ,[Calculated_ReceivedQty]
                  ,[Calculated_ArrivedQty]
                  ,[PORel_RelQty]
                  ,[Vendor_EMailAddress]
                  ,[PurAgent_EMailAddress]
                  ,[PODetail_UnitCost]
                  ,[PODetail_ExtCost]
                  ,[POHeader_Company]
                  ,[Calculated_OurQty]
                  ,[Calculated_UnitCost]
                  ,[Calculated_ArrivedDate]
                  ,[POHeader_ChangeDate]
                  ,[POHeader_Approve]
                  ,[POHeader_ApprovalStatus]
                  ,[RowIdent]
            FROM [dbo].[PODetail]
            WHERE CONCAT(POHeader_PONum,'-',PODetail_POLine,'-',PORel_PORelNum) IN ('" + poString.Replace(",", "','") + "')";

                if (userType.ToUpper() == "BUYER")
                    query += " AND PurAgent_EMailAddress = '" + Email + "'";

                DataTable dt = oDAL.GetData(query);



                lstPO = cCommon.ConvertDtToHashTable(dt);
                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetWidgetData Method");
                return new DataTable();
            }
        }


        public DataTable GetResendEmailInfo(string PO, string Line, string Rel)
        {
            try
            {
                string query = "";
                DataTable dt = new DataTable();
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                string userType = HttpContext.Current.Session["UserType"].ToString();
                string Email = HttpContext.Current.Session["Email"].ToString();

                query = @"SELECT [POHeader_PONum]
                            ,[PODetail_POLine]
                            ,[PORel_PORelNum]
                            ,[PODetail_IUM]
                            ,[Vendor_VendorID]
                            ,[Vendor_Name]
                            ,[POHeader_BuyerID]
                            ,[PurAgent_Name]
                            ,[PODetail_PartNum]
                            ,[PODetail_LineDesc]
                            ,[Calculated_OrderDate]
                            ,[Calculated_DueDate]
                            ,[PODetail_OrderQty]
                            ,[PODetail_XOrderQty]
                            ,[Calculated_ReceivedQty]
                            ,[Calculated_ArrivedQty]
                            ,[PORel_RelQty]
                            ,[Vendor_EMailAddress]
                            ,[PurAgent_EMailAddress]
                            ,[PODetail_UnitCost]
                            ,[PODetail_ExtCost]
                            ,[POHeader_Company]
                            ,[Calculated_OurQty]
                            ,[Calculated_UnitCost]
                            ,[Calculated_ArrivedDate]
                            ,[POHeader_ChangeDate]
                            ,[POHeader_Approve]
                            ,[POHeader_ApprovalStatus]
                            ,[RowIdent]
                         FROM [dbo].[PODetail]  						 
                         WHERE POHeader_PONum = <PO> AND PODetail_POLine = <POLine> AND PORel_PORelNum = <RelNo>  ";

                query = query.Replace("<PO>", PO);
                query = query.Replace("<POLine>", Line);
                query = query.Replace("<RelNo>", Rel);

                

                dt = oDAL.GetData(query);
                return dt;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "GetPOList API Method");
                return new DataTable(); // Return an empty table on failure
            }
        }
        // Dashboard Count
        public DataTable GetCounts()
        {
            string query = "";
            DataTable dt = new DataTable();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string userType = HttpContext.Current.Session["UserType"].ToString();
            string email = HttpContext.Current.Session["Email"].ToString();

            query = @"
    SELECT 
        (SELECT COUNT(DISTINCT PD.POHeader_PONum)
         FROM [dbo].[PODetail] PD
         INNER JOIN [SRM].[BuyerPO] BP 
             ON PD.POHeader_PONum = BP.PONum 
            AND PD.PODetail_POLine = BP.[LineNo]
            AND PD.PORel_PORelNum = BP.RelNo
         WHERE 1=1 <BuyerClause>) AS InProcessCount,

        (SELECT COUNT(DISTINCT POHeader_PONum)
         FROM [dbo].[PODetail] PD
         WHERE Calculated_ArrivedDate > Calculated_DueDate
           AND Calculated_DueDate < GETDATE()
           AND Calculated_ArrivedQty < PODetail_OrderQty
           <BuyerClause>) AS LatePOCount,

        (SELECT COUNT(DISTINCT POHeader_PONum)
         FROM [dbo].[PODetail] PD
         WHERE Calculated_DueDate >= Calculated_ArrivedDate
           AND Calculated_ArrivedQty >= PODetail_OrderQty
           <BuyerClause>) AS EarlyCount,

        (SELECT COUNT(*) AS CompletedCount
FROM (
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
        T.LastCommunication
    FROM 
        [SRM].[BuyerPO] B
    JOIN (
        SELECT *,
               ROW_NUMBER() OVER (PARTITION BY PONo ORDER BY CreatedOn DESC) AS RowNum
        FROM [SRM].[VendorCommunication]
    ) V ON B.GUID = V.GUID 
        AND CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo) = V.PONo
        AND V.RowNum = 1
    LEFT JOIN (
        SELECT PONo, MAX(CreatedOn) AS LastCommunication
        FROM [SRM].[Transaction]
        WHERE HasAction <> 'Document'
        GROUP BY PONo
    ) T ON T.PONo = CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo)
    JOIN [dbo].[PODetail] PD 
        ON PD.POHeader_PONum = B.PONum
        AND PD.PODetail_POLine = B.[LineNo]
        AND PD.PORel_PORelNum = B.RelNo
    WHERE 
        ISNULL(ROUND(PD.PODetail_XOrderQty, 2), 0) = ISNULL(ROUND(PD.Calculated_ReceivedQty, 2), 0)
        <BuyerClause>
) AS CountQuery) AS CompletedCount";

            // Apply filter for buyers
            string buyerClause = userType.ToUpper() == "BUYER"
                ? $" AND PD.PurAgent_EMailAddress = '{email}'"
                : "";
           
            // Replace <BuyerClause> placeholders
            query = query.Replace("<BuyerClause>", buyerClause);


            dt = oDAL.GetData(query);
            return dt;
        }

        // End

        public DateTime ConvertGenericDateTime()
        {
            string timezone = HttpContext.Current.Session["TimeZone"].ToString();

            DateTime utcTime = DateTime.UtcNow;

            // Get the TimeZoneInfo object for the selected timezone
            TimeZoneInfo selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);

            // Convert UTC time to the selected time zone, DST-aware
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, selectedTimeZone);

            return localTime;
        }

        public static DataTable Tabulate(string json)
        {
            var jsonLinq = JObject.Parse(json);
            // Find the first array using Linq
            var srcArray = jsonLinq.Descendants().Where(d => d is JArray).First();
            var trgArray = new JArray();
            foreach (JObject row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types
                    if (column.Value is JValue)
                    {
                        cleanRow.Add(column.Name, column.Value);
                    }
                }
                trgArray.Add(cleanRow);


            }
            DataTable dt = new DataTable();
            dt = JsonConvert.DeserializeObject<DataTable>(trgArray.ToString());

            return dt;
        }
    }
}