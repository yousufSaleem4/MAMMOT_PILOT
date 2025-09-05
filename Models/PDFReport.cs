using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;

namespace PlusCP.Models
{
    public class PDFReport
    {
        public List<Hashtable> lstPDFPO { get; set; }
        public string ErrorMessage { get; set; }

        [Display(Name = "Message")]
        public string EmailMessage { get; set; }
        public string EmailAddress { get; set; }
        public string CCEmailAddress { get; set; }
        public string ResultMsg { get; set; }

        cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
        public bool GetList(DataTable dt)
        {
            string[] columnNamesToKeep = { "POHeader_PONum", "Vendor_Name", "Vendor_EMailAddress", "POHeader_Approve" };

            // Create a new DataTable with only the specified columns
            DataTable newDt = SelectColumns(dt, columnNamesToKeep);
            newDt.Columns.Add("PDF");
            newDt.Columns.Add("ViewPDF");
            newDt.Columns.Add("CreatedOn");
            newDt.Columns.Add("BuyerName");
            string sql = "";
            DataTable dtEmailPDF = new DataTable();
            sql = @"Select PONo, CreatedOn, BuyerName from [dbo].[tblEmailPdf] ";
            dtEmailPDF = oDAL.GetData(sql);
            foreach (DataRow row2 in dtEmailPDF.Rows)
            {
                string poNo = row2["PONo"].ToString();
                string CreatedOn = row2["CreatedOn"].ToString();
                string Buyer = row2["BuyerName"].ToString();

                DataRow[] matchingRows = newDt.Select($"POHeader_PONum = '{poNo}'");
                if (matchingRows.Length > 0)
                {
                    foreach (DataRow row1 in matchingRows)
                    {

                        row1["CreatedOn"] = CreatedOn;
                        row1["BuyerName"] = Buyer;
                    }
                }

            }

            // Filter out duplicate POHeader_PONum values
            var distinctRows = newDt.AsEnumerable()
                        .GroupBy(row => Convert.ToString(row["POHeader_PONum"]))
                        .Select(group => group.First())
                        .CopyToDataTable();


            DataView dv = distinctRows.DefaultView;
            dv.Sort = "CreatedOn DESC"; // You can use DESC for descending order
            DataTable sortedDt = dv.ToTable();
            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (newDt.Rows.Count > 0)
                    lstPDFPO = cCommon.ConvertDtToHashTable(sortedDt);

                return true;
            }
        }



        public void SendPDFEmail(string PONo, string SupplierName, string SupplierEmail, string ccEmailAddress)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            NewPO oNewPO = new NewPO();
            string ConnctionType = HttpContext.Current.Session["DefaultDB"].ToString();
            string DeployMode = WebConfigurationManager.AppSettings["DEPLOYMODE"];
            string result = "";
          

            var subject = "PDF report of PO No.:  " + PONo + "";
            // Recipient's email address
            string recipientEmail = SupplierEmail;

           
            string URL = "";

            //URL
            DataTable dtURL = new DataTable();
            dtURL = cCommon.GetEmailURL(DeployMode, "EmailPDF");

            if (DeployMode == "PROD")
            {
                URL = dtURL.Rows[0]["URL"].ToString() + dtURL.Rows[0]["PageURL"].ToString();

            }
            else
            {
                URL = dtURL.Rows[0]["URL"].ToString() + dtURL.Rows[0]["PageURL"].ToString();

            }
            URL = URL.Replace("<PO>", PONo);
            URL = URL.Replace("<Connection>", ConnctionType);
            //URL = URL + PONo + "&Connection=" + ConnctionType;

            string htmlBody = MakeEmailBody(PONo, SupplierName, URL);
         

           
            try
            {
                // Send the email
                string createdBy = HttpContext.Current.Session["Username"].ToString();
                result = cCommon.SendEmail(recipientEmail, subject, htmlBody, ccEmailAddress, null);
                InsertPDFRecord(PONo, SupplierName, SupplierEmail);
                oNewPO.AddInTransaction(PONo, "", "", "Document", "", "", "", createdBy, "Sent PDF to Supplier");
                ResultMsg = result;
            }
            catch (Exception)
            {

            }
        }
        public void InsertPDFRecord(string PONo, string SupplierName, string SupplierEmail)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string createdBy = HttpContext.Current.Session["FullName"].ToString();
            string sql = "";
            sql = @"INSERT INTO [dbo].[tblEmailPdf]

           ([PONo]
           ,[VendorName]
           ,[VendorEmail]
           ,[BuyerName])
     VALUES
           ('<PONo>'
           ,'<VendorName>'
           ,'<VendorEmail>'
           ,'<BuyerName>'
		   )";

            sql = sql.Replace("<PONo>", PONo);
            sql = sql.Replace("<VendorName>", SupplierName);
            sql = sql.Replace("<VendorEmail>", SupplierEmail);
            sql = sql.Replace("<BuyerName>", createdBy);
            oDAL.Execute(sql);
        }
        public string MakeEmailBody(string PONo, string Supplier, string URL)
        {
            string htmlBody = "";
           
            DataTable dt = new DataTable();
            string query = "";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            query = "Select SYSValue from dbo.zSysIni WHERE SysDesc = 'POPDF' ";
            dt = oDAL.GetData(query);
            htmlBody = dt.Rows[0]["SYSValue"].ToString();

            htmlBody = htmlBody.Replace("<SupplierName>", Supplier);
            htmlBody = htmlBody.Replace("<PONo>", PONo);
            htmlBody = htmlBody.Replace("<PDFURL>", URL);



            return htmlBody;
        }

        static DataTable SelectColumns(DataTable originalTable, string[] columnNamesToKeep)
        {
            // Create a new DataTable with only the specified columns
            DataTable newTable = originalTable.DefaultView.ToTable(false, columnNamesToKeep);
            return newTable;
        }
    }
}