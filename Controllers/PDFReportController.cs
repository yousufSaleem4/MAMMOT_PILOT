using IP.ActionFilters;
using IP.Classess;
using Newtonsoft.Json.Linq;
using PlusCP.Classess;
using PlusCP.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace PlusCP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PDFReportController : Controller
    {
        // GET: PDFReport
        PDFReport oPDF = new PDFReport();
        NewPOCommon oPOCommon = new NewPOCommon();
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPDF = new PDFReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = "PDF Report";
            return View(oPDF);
        }

        public JsonResult GetPDFReport()
        {
            string menuTitle = string.Empty;
            string RptCode;
            DataTable dt = new DataTable();
            NewPOCommon oNewPOCommon = new NewPOCommon();
            string DeployMode = Session["DefaultDB"].ToString();
            if (DeployMode.ToUpper() == "PROD" || DeployMode.ToUpper() == "PILOT")
            {
                dt =  oPOCommon.GetPOAsync("ALL");

            }
            else
            {
                dt = oNewPOCommon.GetPOListFromSQL("ALL");
            }

            oPDF.GetList(dt);
            
            var jsonResult = Json(oPDF, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }

        [HttpPost]
        public ActionResult OpenPDF(string PoNo)
        {
            try
            {
                string ConnctionType = Session["DefaultDB"].ToString();
                if (ConnctionType.ToUpper() == "PROD" || ConnctionType.ToUpper() == "PILOT")
                {
                    double PONum = Convert.ToDouble(PoNo);
                    int PO = (int)PONum;
                    string result = "";

                    // Assuming PDFAPICall is not async, otherwise you should await it
                    result = PDFAPICall(PoNo);

                    if (result == "OK")
                    {
                        

                        DataTable dtURLPDF = cCommon.GetEmailURL(ConnctionType.ToUpper(), "GETPDF"); // Assuming GetEmailURL returns a DataTable
                        string URLPDF = dtURLPDF.Rows[0]["URL"].ToString();
                        string PageURLPDF = dtURLPDF.Rows[0]["PageURL"].ToString();
                        string userNamePDF = dtURLPDF.Rows[0]["UserName"].ToString();
                        string passwordPDF = dtURLPDF.Rows[0]["Password"].ToString();
                        passwordPDF = BasicEncrypt.Instance.Decrypt(passwordPDF.Trim());

                        var finalURL = URLPDF + PageURLPDF + "?PONum=" + PO;
                        var clientPDF = new RestClient(URLPDF);
                        var requestPDF = new RestRequest(finalURL, Method.Get);

                        // Add basic authentication header
                        requestPDF.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userNamePDF + ":" + passwordPDF)));
                        requestPDF.AddHeader("api-key", dtURLPDF.Rows[0]["TokenKey"].ToString());
                      
                        // Wait for 5 seconds before proceeding
                        Task.Delay(15000);
                        var response = clientPDF.Execute(requestPDF);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            byte[] pdfBytes = response.RawBytes;
                            string base64String = Convert.ToBase64String(pdfBytes);
                            string pdfString = Encoding.UTF8.GetString(pdfBytes);
                            JObject jsonObject = JObject.Parse(pdfString);

                            // Extract the value associated with the key "SysRptLst_RptData"
                            string base64Value = (string)jsonObject["value"][0]["SysRptLst_RptData"];
                            byte[] pdfBytearr = Convert.FromBase64String(base64Value);

                            int chunkSize = 1000; // Example chunk size (adjust as needed)
                            List<string> dataChunks = new List<string>();

                            for (int i = 0; i < pdfBytearr.Length; i += chunkSize)
                            {
                                int length = Math.Min(chunkSize, pdfBytearr.Length - i);
                                byte[] chunk = new byte[length];
                                Array.Copy(pdfBytearr, i, chunk, 0, length);
                                dataChunks.Add(Convert.ToBase64String(chunk)); // Convert chunk to base64 string
                            }
                            return Json(new { success = true, dataChunks = dataChunks });
                        }
                        else
                        {
                            string responseContent = response.Content.ToString();
                            JObject json = JObject.Parse(responseContent);
                            string errorMessage = json["ErrorMessage"]?.ToString();
                            return Content(errorMessage);
                        }
                    }
                    else
                    {
                        return Content(result);
                    }
                }
                else
                {
                    double PONum = Convert.ToDouble(PoNo);
                    int PO = (int)PONum;

                    NewPO oPO = new NewPO();
                    string TaskNum = oPO.OpenPDF(PO.ToString());

                    DataTable dtURL = cCommon.GetEmailURL(ConnctionType.ToUpper(), "OpenPDF");
                    string URL = dtURL.Rows[0]["URL"].ToString();
                    var client = new RestClient(URL);
                    var request = new RestRequest(dtURL.Rows[0]["PageURL"].ToString() + "/?TaskNum=" + TaskNum, Method.Get);
                    string userName = dtURL.Rows[0]["UserName"].ToString();
                    string password = dtURL.Rows[0]["Password"].ToString();
                    password = BasicEncrypt.Instance.Decrypt(password.Trim());

                    request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password)));

                    var response = client.Execute(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        byte[] pdfBytes = response.RawBytes;
                        string base64String = Convert.ToBase64String(pdfBytes);
                        string pdfString = Encoding.UTF8.GetString(pdfBytes);
                        JObject jsonObject = JObject.Parse(pdfString);

                        string base64Value = (string)jsonObject["value"][0]["SysRptLst_RptData"];
                        byte[] pdfBytearr = Convert.FromBase64String(base64Value);

                        int chunkSize = 1000;
                        List<string> dataChunks = new List<string>();

                        for (int i = 0; i < pdfBytearr.Length; i += chunkSize)
                        {
                            int length = Math.Min(chunkSize, pdfBytearr.Length - i);
                            byte[] chunk = new byte[length];
                            Array.Copy(pdfBytearr, i, chunk, 0, length);
                            dataChunks.Add(Convert.ToBase64String(chunk));
                        }
                        return Json(new { success = true, dataChunks = dataChunks });
                    }
                    else
                    {
                        string responseContent = response.Content.ToString();
                        JObject json = JObject.Parse(responseContent);
                        string errorMessage = json["ErrorMessage"]?.ToString();
                        return Content(errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content($"Warning: Epicor did not respond to the request. Please try again.");
            }
        }

        public JsonResult SendPDFEmail(string PONo, string SupplierName, string SupplierEmail, string EmailAddress, string CCEmailAddress)
        {
            string menuTitle = string.Empty;
            string RptCode;
            string result = "";

            oPDF = new PDFReport();
            result = PDFAPICall(PONo);
            if(result == "OK")
            {
                oPDF.SendPDFEmail(PONo, SupplierName, EmailAddress, CCEmailAddress);
                var jsonResult = Json(oPDF, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                //LOAD MRU & LOG QUERY
                if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
                {
                    menuTitle = TempData["ReportTitle"] as string;
                    RptCode = TempData["RptCode"].ToString();
                    TempData.Keep();
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }
                return jsonResult;
            }
            else
            {
                return Json(result);
            }
        }

        public string PDFAPICall(string PONo)
        {
            string DeployMode = Session["DefaultDB"].ToString();
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
                string responseContent = response.Content.ToString();  // Using the synchronous method

                // Parse the JSON content to extract the ErrorMessage
                JObject json = JObject.Parse(responseContent);
                string errorMessage = json["ErrorMessage"]?.ToString();
                result = errorMessage;
            }
                
            
            return result;
        }

        public string MakeJsonBody(string PONo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string Json = "";
            string sql = "Select APIJson from [dbo].[APIJson] WHERE APIType = 'PDF' ";
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
    }
}