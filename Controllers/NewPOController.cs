using IP.ActionFilters;
using IP.Classess;
using Newtonsoft.Json;
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

    [SessionTimeout]
    public class NewPOController : Controller
    {
        // GET: PO
        NewPO oPO = new NewPO();
        public ActionResult Option()
        {
            oPO = new NewPO();
            return View(oPO);
        }

        public ActionResult GetPO(string RptCode, string menuTitle, string WidgetId)
        {
            oPO = new NewPO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            string Status = Request.QueryString["POStatus"];
            if (Status == "All Open")
                Status = "Idle";
            else if (Status == "Pending")
                Status = "In Process";
            else if (Status == "Updated")
                Status = "Completed";
            else if (Status == "Arrived")
                Status = "Early";
            else if (Status == "Widget" || !string.IsNullOrEmpty(WidgetId))
                Status = "Widget";
            else if (Status == "All" || Status == null)
                Status = "All";

            if (WidgetId != null)
                Session["WidgetId"] = WidgetId;
            else
                Session["WidgetId"] = null;

            ViewBag.ReportTitle = "Purchase Order" + " > " + Status;
            return View(oPO);
        }

        public ActionResult Index(string PO, string RptCode, string menuTitle, string WidgetId, string WidgetTitle)
        {
            oPO = new NewPO();

            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            string Status = Request.QueryString["POStatus"];
            if (Status == "All Open")
                Status = "Idle";
            else if (Status == "Pending")
                Status = "In Process";
            else if (Status == "Updated")
                Status = "Completed";
            else if (Status == "Arrived")
                Status = "Early";
            else if (Status == "Widget" || !string.IsNullOrEmpty(WidgetId))
                Status = "Widget";
            else if (Status == "All" || Status == null)
                Status = "All";

            if (WidgetId != null)
                Session["WidgetId"] = WidgetId;
            else
                Session["WidgetId"] = "";
            if (PO == "")
            {
                PO = "0";
            }
            ViewBag.PurchaseOrder = PO;
            Session["PO"] = PO;
            ViewBag.ReportTitle = "PO#" + " > " + Status + " > " + WidgetTitle;
            return View(oPO);

        }
        //////////////////////////////////////////////////////////// Get
        /// <summary>
        /// GET API Calling
        /// </summary>
        /// <param name="POStatus"></param>
        /// <returns></returns>

        //[OutputCache(Duration = 600, VaryByParam = "POStatus")]
        //public JsonResult GetPOList(string POStatus)
        //{
        //    DataTable dt = new DataTable();
        //    dt = Session["dtPOHeader"] as DataTable;
        //    string ConnctionType = Session["DefaultDB"].ToString();
        //    string Email = Session["Email"].ToString();
        //    string userType = Session["UserType"].ToString();

        //    oPO.GetPO(dt, POStatus, ConnctionType, Email,userType);

        //    var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
        //    jsonResult.MaxJsonLength = int.MaxValue;

        //    // LOAD MRU & LOG QUERY
        //    if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
        //    {
        //        string menuTitle = TempData["ReportTitle"] as string;
        //        string RptCode = TempData["RptCode"].ToString();
        //        TempData.Keep();

        //        cLog oLog = new cLog();
        //        oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
        //    }

        //    return jsonResult;
        //}

        [OutputCache(Duration = 600, VaryByParam = "POStatus")]
        public JsonResult GetPOList(string POStatus)
        {

            DataTable dt = new DataTable();
            string ConnctionType = Session["DefaultDB"].ToString();
            string Email = Session["Email"].ToString();
            string userType = Session["UserType"].ToString();

            NewPOCommon oPOCommon = new NewPOCommon();
            if (ConnctionType.ToUpper() == "PROD" || ConnctionType.ToUpper() == "PILOT")
            {
                dt = oPOCommon.GetPOAsync(POStatus);  // Fetch data from API
            }
            else
            {
                dt = oPOCommon.GetPOListFromSQL(POStatus);  // Fetch data from SQL
            }


            oPO.GetPO(dt, POStatus, ConnctionType, Email, userType);

            var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            // LOAD MRU & LOG QUERY
            cLog oLog = new cLog();
            oLog.SaveLog("Purchase Order Form", Request.Url.PathAndQuery, "001");

            return jsonResult;
        }

        public JsonResult GetList(string POStatus, string PONo)
        {
            DataTable dt = new DataTable();
            NewPO oPO = new NewPO();
            NewPOCommon oPOCommon = new NewPOCommon();

            string menuTitle = string.Empty;
            string ConnctionType = Session["DefaultDB"].ToString();
            string widgetId = Session["WidgetId"].ToString();

            if (ConnctionType.ToUpper() == "PROD" || ConnctionType.ToUpper() == "PILOT")
            {
                dt = oPOCommon.GetPODetails(PONo, POStatus, widgetId);
            }
            else
            {
                dt = oPOCommon.GetPODtlFromSQL(PONo);
            }
            string userType = Session["UserType"].ToString();
            ViewBag.UserType = userType;
            oPO.GetList(dt, POStatus);
            var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            cLog oLog = new cLog();
            oLog.SaveLog("PO Details", Request.Url.PathAndQuery, "002");

            return jsonResult;

        }


        public JsonResult SendEmail(string PONo, string Line, string Release, string PartNo, string PartDesc, string UOM,
            string OrderDate, string DueDate, string Qty, string Price, string VendorId, string VendorName,
           string BuyerId, string Receiveremail, string contactReason, string message, string GUID, string NewDueDate, string OrderQty, string ccEmail, string TrackingNo, string BuyerEmail, string SupplierCompany, string[] CCemails, HttpPostedFileBase Attachfile)
        {

            bool isUpdate = false;
            string userName = Session["Username"].ToString();
            Guid newGuid;


            // Create New GUID
            if (GUID == "")
            {
                newGuid = Guid.NewGuid();
            }
            else
            {
                newGuid = new Guid(GUID);
                isUpdate = true;
            }
            if (contactReason != "Change")
            {
                NewDueDate = "";
            }

            // Insert Into BuyerCommunication table and sent email
            string result = oPO.sendVendorEmail(userName, PONo, PartNo, Qty, DueDate, Price, message, newGuid.ToString(), NewDueDate, contactReason, ccEmail, Attachfile, Receiveremail, CCemails);
            // Escape single quotes
            string escapedMessage = message.Replace("'", "''");
            string escapedPOVendorName = VendorName.Replace("'", "''");
            JsonResult jsonResult;
            if (result == "SENT")
            {
                // Insert Into BuyerPO table
                oPO.AddInBuyers(PONo, Line, Release, PartNo, PartDesc, Qty, Price, UOM, DueDate, OrderDate, userName,
                   VendorId, escapedPOVendorName, Receiveremail, BuyerId, contactReason, userName, newGuid.ToString(), escapedMessage, isUpdate, NewDueDate, OrderQty, TrackingNo, BuyerEmail, SupplierCompany);
                jsonResult = Json("Send", JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
            }
            else
            {
                jsonResult = Json("NOT", JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
            }

            return jsonResult;
        }

        public ActionResult SupplierUpdate(string RptCode, string menuTitle)
        {
            oPO = new NewPO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = "Purchase Order > Completed";
            return View(oPO);

        }

        public JsonResult GetUpdateData()
        {
            string menuTitle = string.Empty;
            string RptCode;

            oPO = new NewPO();
            oPO.GetUpdateDataForDashboard();
            var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetPOTransactionDtl(string PONo)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oPO = new NewPO();
            oPO.GetPOTransactionDtl(PONo);

            var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
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


        public ActionResult GetUpdateNew(string PONo)
        {
            try
            {
                ViewBag.ReportTitle = "PO @ " + PONo + "";
                NewPO oPO = new NewPO();
                bool success = oPO.GetUpdateData(PONo);
                oPO.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

                if (success)
                    return View(oPO);
                else
                    return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }

        public JsonResult GetTransactionDtl(string PONo)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oPO = new NewPO();
            oPO.GetTransactionDtl(PONo);
            var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
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

        //public JsonResult GetPOTransactionDtl(string PONo)
        //{
        //    string menuTitle = string.Empty;
        //    string RptCode;

        //    oPO = new NewPO();
        //    bool success = oPO.GetPOTransactionDtl(PONo);

        //    // Create an anonymous object with just the properties we want to return
        //    var result = new
        //    {
        //        Status = oPO.POStatus,
        //        InsertedOn = oPO.POInsertedOn,
        //        UpdatedOn = oPO.POUpdatedOn,
        //        Success = success
        //    };

        //    var jsonResult = Json(result, JsonRequestBehavior.AllowGet);
        //    jsonResult.MaxJsonLength = int.MaxValue;

        //    //LOAD MRU & LOG QUERY
        //    if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
        //    {
        //        menuTitle = TempData["ReportTitle"] as string;
        //        RptCode = TempData["RptCode"].ToString();
        //        TempData.Keep();
        //        cLog oLog = new cLog();
        //        oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
        //    }

        //    return jsonResult;
        //}

        public JsonResult GetResendEmail(string PONo, string Line, string Rel)
        {
            string menuTitle = string.Empty;
            string RptCode;
            DataTable dt = new DataTable();
            NewPOCommon oPOCommon = new NewPOCommon();
            string ConnctionType = Session["DefaultDB"].ToString();
            if (ConnctionType.ToUpper() == "PROD" || ConnctionType.ToUpper() == "PILOT")
            {
                dt = oPOCommon.GetResendEmailInfo(PONo, Line, Rel);
            }
            else
            {
                dt = oPOCommon.GetPOListFromSQL("ALL");
            }

            NewPO oNewPO = new NewPO();
            oNewPO.GetSelectedList(dt);

            var jsonResult = Json(oNewPO, JsonRequestBehavior.AllowGet);
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
                string DeployMode = ConfigurationManager.AppSettings["DEPLOYMODE"];

                string ConnctionType = Session["DefaultDB"].ToString();
                if (ConnctionType.ToUpper() == "PROD" || ConnctionType.ToUpper() == "PILOT")
                {

                }

                double PONum = Convert.ToDouble(PoNo);
                int PO = (int)PONum;

                NewPO oPO = new NewPO(); // Assuming NewPO is a class defined somewhere in your code
                string TaskNum = oPO.OpenPDF(PO.ToString()); // Assuming OpenPDF method returns a string representing the task number

                DataTable dtURL = cCommon.GetEmailURL(DeployMode, "OpenPDF"); // Assuming GetEmailURL returns a DataTable
                string URL = dtURL.Rows[0]["URL"].ToString();
                var client = new RestClient(URL);
                var request = new RestRequest(dtURL.Rows[0]["PageURL"].ToString() + "/?TaskNum=" + TaskNum, Method.Get);
                string userName = dtURL.Rows[0]["UserName"].ToString();
                string password = dtURL.Rows[0]["Password"].ToString();
                password = BasicEncrypt.Instance.Decrypt(password.Trim());

                // Add basic authentication header
                request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password)));

                var response = client.Execute(request);

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
                    // Response status is not OK, return error message
                    return Content("Failed to retrieve PDF from the server.");
                }
            }
            catch (Exception ex)
            {
                // Exception occurred, return error message
                return Content($"Error: {ex.Message}");
            }
        }

        private bool IsPdfContent(string contentType)
        {
            // Assuming you have a method to check if the content type represents a PDF
            return contentType != null && contentType.ToLower().Contains("application/pdf");
        }


        public JsonResult GetActionReq(string PONo)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oPO = new NewPO();
            oPO.GetUpdateData(PONo);
            var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ActionAHR"></param>
        /// <param name="GUID"></param>
        /// <param name="PONum"></param>
        /// <param name="PartNo"></param>
        /// <param name="DueDate"></param>
        /// <param name="Qty"></param>
        /// <param name="Price"></param>
        /// <returns></returns>
        public JsonResult UpdateHasAction(string ActionAHR, string GUID, string PONum, string PartNo, string DueDate, string Qty, string Price, string ArrivedQty)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string ConnctionType = Session["DefaultDB"].ToString();
            string EmailAddress = Session["Email"].ToString();
            oPO = new NewPO();

            if (ConnctionType.ToUpper() == "PROD" || ConnctionType.ToUpper() == "PILOT")
            {
                JsonResult result = PatchDataAPI(ActionAHR, GUID, PONum, PartNo, DueDate, Qty, Price, ArrivedQty, EmailAddress);
                if (result.Data.ToString().Contains("Updated"))
                {

                    oPO.UpdateStatus(PONum, GUID);
                    DataTable dt = new DataTable();
                    //dt = GetWidgetDataFronAPI();
                    //Session["APIData"] = dt;
                    return result;
                }
                else
                {
                    return result; // Allowing GET requests
                }
            }
            else
            {
                JsonResult result = PatchDataSQL(ActionAHR, GUID, PONum, PartNo, DueDate, Qty, Price, ArrivedQty);
                //oPO.SendAcceptPDF(PONum, GUID);
                //oPO.UpdateStatus(PONum, GUID);
                return result;
            }
        }


        // Not Calling
        public JsonResult PatchDataAPI(string ActionAHR, string GUID, string PONum, string PartNo, string DueDate, string Qty, string Price, string ArrivedQty, string EmailAddress)
        {
            cLog oLog;
            string Result = "";
            string query = "";
            string BuyerEmail = "";
            try
            {
                string[] POVal = PONum.Split('-');
                if (POVal.Length < 3)
                {
                    return Json(new { Status = "Error", Message = "PO Number format is incorrect." }, JsonRequestBehavior.AllowGet);
                }

                string PO = POVal[0];
                string Line = POVal[1];
                string Rel = POVal[2];
                // First Check Buyer PO Price Limit
                string POLimit = "";
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                query = "Select BuyerEmail From SRM.BuyerPO Where PONum = '" + PO + "' AND [LineNo] = '" + Line + "' AND RelNo = '" + Rel + "'";
                DataTable dtBuyerEmail = oDAL.GetData(query);
                BuyerEmail = dtBuyerEmail.Rows[0]["BuyerEmail"].ToString();

                query = "Select POLimit From BuyerInfo Where EMailAddress = '" + BuyerEmail + "'";
                DataTable dtBuyer = oDAL.GetData(query);
                if (dtBuyer.Rows.Count > 0)
                    POLimit = dtBuyer.Rows[0]["POLimit"].ToString();
                if (Price != "")
                {
                    if (Convert.ToDouble(Price) > Convert.ToDouble(POLimit))
                    {
                        return Json(new { Status = "Error", Message = "Your, purchasing limit has exceeded.." }, JsonRequestBehavior.AllowGet);
                    }
                }


                string DeployMode = Session["DefaultDB"]?.ToString();
                if (string.IsNullOrEmpty(DeployMode))
                {
                    return Json(new { Status = "Error", Message = "DeployMode is missing." }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(PONum) || !PONum.Contains("-"))
                {
                    return Json(new { Status = "Error", Message = "Invalid PO Number format." }, JsonRequestBehavior.AllowGet);
                }



                int finalQty = 0;
                if (int.TryParse(Qty, out int qtyValue)) finalQty += qtyValue;
                if (int.TryParse(ArrivedQty, out int arrivedQtyValue)) finalQty += arrivedQtyValue;

                if (!DateTime.TryParseExact(DueDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    return Json(new { Status = "Error", Message = "Invalid DueDate format." }, JsonRequestBehavior.AllowGet);
                }

                string formattedDate = parsedDate.ToString("yyyy-MM-ddTHH:mm:ss");

                // Update PO Status API Un Approved

                Result = UpdateStatusPO("PONOTAPPROVED", PO);

                if (Result == "OK")
                {

                    // **Call PODTL API First**
                    DataTable dtURLDtl = cCommon.GetEmailURL(DeployMode.ToUpper(), "PODTL");
                    if (dtURLDtl.Rows.Count == 0)
                    {
                        return Json(new { Status = "Error", Message = "API URL not found for PODTL." }, JsonRequestBehavior.AllowGet);
                    }

                    string TrackingNo = GetTrackingNumber(GUID, PONum);


                    string jsonstringPODTL = MakeJsonBody(PO, Line, Rel, finalQty.ToString(), formattedDate, Price, "GETPODTL", TrackingNo);
                    string apiUrlPODTL = dtURLDtl.Rows[0]["PageURL"].ToString()
                                            .Replace("<company>", "159599")
                                            .Replace("<PONUM>", PO)
                                            .Replace("<POLine>", Line);

                    string userName = dtURLDtl.Rows[0]["UserName"].ToString();
                    string password = BasicEncrypt.Instance.Decrypt(dtURLDtl.Rows[0]["Password"].ToString().Trim());
                    string tokenKey = dtURLDtl.Rows[0]["TokenKey"].ToString();

                    var clientPODTL = new RestClient(dtURLDtl.Rows[0]["URL"].ToString());
                    var postPODtlRequest = new RestRequest(apiUrlPODTL, Method.Patch);
                    postPODtlRequest.AddJsonBody(jsonstringPODTL);
                    postPODtlRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password)));
                    postPODtlRequest.AddHeader("api-key", tokenKey);

                    var postPODtlResponse = clientPODTL.Execute(postPODtlRequest);

                    if (postPODtlResponse.StatusCode == System.Net.HttpStatusCode.Created || postPODtlResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        // **If PODTL API is successful, then Call POREL API**
                        DataTable dtURLREL = cCommon.GetEmailURL(DeployMode.ToUpper(), "POREL");
                        if (dtURLREL.Rows.Count == 0)
                        {
                            return Json(new { Status = "Error", Message = "API URL not found for POREL." }, JsonRequestBehavior.AllowGet);
                        }

                        string jsonstringPOREL = MakeJsonBody(PO, Line, Rel, finalQty.ToString(), formattedDate, Price, "GETPOREL", "");
                        string apiUrlPOREL = dtURLREL.Rows[0]["PageURL"].ToString()
                                                .Replace("<company>", "159599")
                                                .Replace("<PO>", PO)
                                                .Replace("<Line>", Line)
                                                .Replace("<Rel>", Rel);

                        var clientPOREL = new RestClient(dtURLREL.Rows[0]["URL"].ToString());
                        var postPORELRequest = new RestRequest(apiUrlPOREL, Method.Patch);
                        postPORELRequest.AddJsonBody(jsonstringPOREL);
                        postPORELRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password)));
                        postPORELRequest.AddHeader("api-key", tokenKey);

                        var postPORELResponse = clientPOREL.Execute(postPORELRequest);
                        if (postPORELResponse.StatusCode == System.Net.HttpStatusCode.Created || postPORELResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            // **If POREL API is successful, then update data in database**
                            UpdatePORelQty(PO, Line, Rel, finalQty.ToString(), DueDate, Price, TrackingNo);
                            NewPO oPO = new NewPO();
                            Result = UpdateStatusPO("POAPPROVED", PO);

                            oPO.UpdateHasActionAHR(ActionAHR, GUID, PO, Line, Rel, finalQty.ToString());
                            return Json(new { Status = "Updated", Message = "Updated Successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            JObject json = JObject.Parse(postPORELResponse.Content);
                            string errorMessage = json["ErrorMessage"]?.ToString();
                            oLog = new cLog();
                            oLog.RecordError(errorMessage, postPORELResponse.Content.ToString(), "Patch POREL API for QTY and DueDate");

                            return Json(new { Status = "Error", Message = "API Request Failed: " + errorMessage }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else
                    {
                        JObject json = JObject.Parse(postPODtlResponse.Content);
                        string errorMessage = json["ErrorMessage"]?.ToString();
                        oLog = new cLog();
                        oLog.RecordError(errorMessage, postPODtlResponse.Content.ToString(), "Patch PODTL API for Price");
                        return Json(new { Status = "Error", Message = "Failed to update PODTL: " + errorMessage }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { Status = "Error", Message = Result }, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "PatchDataAPI Method");
                return Json(new { Status = "Error", Message = "Exception occurred: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public void UpdatePORelQty(string PONo, string POLine, string PoRel, string RelQty, string DueDate, string Price, string TrackingNo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            // Initial SQL query without the TrackingNo update
            string sql = @"Update [dbo].[PODetail] SET 
                        PORel_RelQty = <RelQty>, 
                        Calculated_DueDate = '<DueDate>', 
                        Calculated_UnitCost = '<Price>', 
                        PODetail_UnitCost = '<Price>'  
                    WHERE POHeader_PONum = <PONum> 
                        AND PODetail_POLine = <Line> 
                        AND PORel_PORelNum = <Rel>";

            // Convert DueDate if it's not empty or null
            DateTime ConvertedNewDueDate = new DateTime();
            if (!string.IsNullOrEmpty(DueDate))
            {
                ConvertedNewDueDate = DateTime.Parse(DueDate);
            }

            // Replace the placeholders with actual values
            sql = sql.Replace("<RelQty>", RelQty);
            sql = sql.Replace("<DueDate>", ConvertedNewDueDate.ToString("yyyy-MM-dd")); // Use appropriate format for SQL
            sql = sql.Replace("<Price>", Price);
            sql = sql.Replace("<PONum>", PONo);
            sql = sql.Replace("<Line>", POLine);
            sql = sql.Replace("<Rel>", PoRel);

            // If TrackingNo is not null or empty, include it in the update query
            if (!string.IsNullOrEmpty(TrackingNo))
            {
                sql += ", PODetail_OrigComment = '" + TrackingNo + "'";  // Append TrackingNo update
            }

            // Execute the final SQL
            oDAL.Execute(sql);
        }

        public string UpdateStatusPO(string Status, string PO)
        {
            cLog oLog;
            try
            {
                string Result = "";
                string DeployMode = Session["DefaultDB"]?.ToString();
                DataTable dtURL = cCommon.GetEmailURL(DeployMode.ToUpper(), "UpdatePOStatus");
                string jsonstring = "";
                jsonstring = MakeJsonBody(PO, "", "", "", "", "", Status, "");

                string URL = dtURL.Rows[0]["URL"].ToString();
                string userName = dtURL.Rows[0]["UserName"].ToString();
                string password = BasicEncrypt.Instance.Decrypt(dtURL.Rows[0]["Password"].ToString().Trim());
                string apiUrl = dtURL.Rows[0]["PageURL"].ToString();

                var client = new RestClient(URL);

                // Update QTY and Duedate API calling
                var postRequest = new RestRequest(apiUrl, Method.Post);

                postRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password)));
                postRequest.AddHeader("api-key", dtURL.Rows[0]["TokenKey"].ToString());
                postRequest.AddJsonBody(jsonstring);

                var postResponse = client.Execute(postRequest);

                if (postResponse.StatusCode == System.Net.HttpStatusCode.Created || postResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Result = "OK";
                }
                else
                {

                    JObject json = JObject.Parse(postResponse.Content);
                    string errorMessage = json["ErrorMessage"]?.ToString();
                    Result = "Error: " + errorMessage;
                    oLog = new cLog();
                    oLog.RecordError(errorMessage, postResponse.Content.ToString(), "UpdatePOStatus Method Calling");
                }
                return Result;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "UpdatePOStatus Method");
                return "Error";
            }

        }
        public string MakeJsonBody(string PONo, string POLine, string PoRel, string QTY, string DueDate, string UnitCost, string APIType, string TrackingNo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string Json = "";
            string sql = "Select APIJson from [dbo].[APIJson] WHERE APIType = '" + APIType + "' ";
            DataTable dt = new DataTable();

            dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                Json = dt.Rows[0]["APIJson"].ToString();
            }
            JObject jsonObject = new JObject();
            jsonObject = JObject.Parse(Json);

            if (APIType == "GETPOREL")
            {
                // Change the value of Jsons dynamically
                jsonObject["PONum"] = PONo;
                jsonObject["POLine"] = POLine;
                jsonObject["PORelNum"] = PoRel;
                jsonObject["DueDate"] = DueDate;
                jsonObject["RelQty"] = QTY;
                jsonObject["RowMod"] = "U";
            }
            else if (APIType == "GETPODTL")
            {
                jsonObject["PONUM"] = PONo;
                jsonObject["POLine"] = POLine;
                jsonObject["UnitCost"] = UnitCost;
                jsonObject["DocUnitCost"] = UnitCost;
                jsonObject["CurrencySwitch"] = "false";
                jsonObject["DocScrUnitCost"] = UnitCost;
                jsonObject["ScrUnitCost"] = UnitCost;
                if (!string.IsNullOrEmpty(TrackingNo))
                    jsonObject["OrigComment"] = TrackingNo;
                else
                    jsonObject.Remove("OrigComment");

                jsonObject["RowMod"] = "U";
            }
            else if (APIType == "POAPPROVED")
            {
                jsonObject["Company"] = "159599";
                jsonObject["OpenOrder"] = "true";
                jsonObject["VoidOrder"] = "true";
                jsonObject["PONum"] = PONo;
                jsonObject["Approve"] = "true";
                jsonObject["ReadyToPrint"] = "true";
                jsonObject["ApprovalStatus"] = "A";
                jsonObject["RowMod"] = "U";
            }
            else if (APIType == "PONOTAPPROVED")
            {
                jsonObject["Company"] = "159599";
                jsonObject["OpenOrder"] = "true";
                jsonObject["VoidOrder"] = "true";
                jsonObject["PONum"] = PONo;
                jsonObject["Approve"] = "false";
                jsonObject["ReadyToPrint"] = "false";
                jsonObject["ApprovalStatus"] = "U";
                jsonObject["RowMod"] = "U";
            }

            // Convert the modified JSON object back to string
            string modifiedJsonString = "";
            modifiedJsonString = jsonObject.ToString();

            return modifiedJsonString;

        }

        public string GetTrackingNumber(string GUID, string PO)
        {
            cLog oLog;
            try
            {
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
                string Result = "";
                string query = @"SELECT TOP 1 CONCAT(serviceType, ':',  TrackingNo) AS TrackingNo    FROM [SRM].[VendorCommunication]
WHERE GUID = '<GUID>' AND PONo = '<PO>'  
ORDER BY Id DESC";
                
                query = query.Replace("<GUID>", GUID);
                query = query.Replace("<PO>", PO);
                DataTable dt = oDAL.GetData(query);
                if (dt.Rows.Count > 0)
                    Result = dt.Rows[0]["TrackingNo"].ToString();

                return Result;
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "Get Tracking Number Method");
                return "Error";
            }

        }

        public JsonResult PatchDataSQL(string ActionAHR, string GUID, string PONum, string PartNo, string DueDate, string Qty, string Price, string ArrivedQty)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string[] POVal = PONum.Split('-');
            string PO = POVal[0];
            string Line = POVal[1];
            string Rel = POVal[2];
            object result;
            int buyerQty = 0;
            int vendorQty = 0;
            int remainingQty = 0;
            int PrevArrivedQty = 0;
            if (string.IsNullOrEmpty(ArrivedQty))
                ArrivedQty = "0";
            decimal FinalQty = Convert.ToDecimal(Qty) + Convert.ToDecimal(ArrivedQty);
            oPO = new NewPO();
            string sql = "";
            // Get BuyerQty
            sql = "Select Qty from [SRM].[BuyerPO] where GUID = '<GUID>' ";
            sql = sql.Replace("<GUID>", GUID);
            result = oDAL.GetObject(sql);

            if (result != null)
            {
                buyerQty = Convert.ToInt32(result);
            }
            // Get VendorQty
            DataTable dtVendorQty = new DataTable();
            sql = "Select TOP 1 Qty from [SRM].[VendorCommunication] where GUID = '<GUID>' ORDER By ID DESC";
            sql = sql.Replace("<GUID>", GUID);
            result = oDAL.GetObject(sql);

            if (result != null)
            {
                vendorQty = Convert.ToInt32(result);
            }


            // Update Remaining Qty
            remainingQty = buyerQty - vendorQty;
            sql = @"Update [SRM].[BuyerPO] SET RemainingQty = '<RemainingQty>' Where GUID = '<GUID>' ";
            sql = sql.Replace("<RemainingQty>", remainingQty.ToString());
            sql = sql.Replace("<GUID>", GUID);

            oDAL.Execute(sql);


            // Update tblPurchaseOrder ArrivedQty

            sql = "Select Calculated_ArrivedQty from [dbo].[tblPurchaseOrder] where POHeader_PONum = '<PONum>' and PODetail_POLine = '<POLine>' and PORel_PORelNum = '<PORelNum>' ";
            sql = sql.Replace("<PONum>", PO);
            sql = sql.Replace("<POLine>", Line);
            sql = sql.Replace("<PORelNum>", Rel);

            result = oDAL.GetObject(sql);
            if (result != null)
            {
                PrevArrivedQty = Convert.ToInt32(result);
            }

            sql = @"Update [dbo].[tblPurchaseOrder] set Calculated_DueDate = '<DueDate>', Calculated_ArrivedQty = '<ArrivedQty>',
PODetail_UnitCost = '<Price>'
where POHeader_PONum = '<PONum>' and PODetail_POLine = '<POLine>' and PORel_PORelNum = '<PORelNum>' ";

            int totalArrivedQty = Convert.ToInt32(Qty) + PrevArrivedQty;
            sql = sql.Replace("<DueDate>", DueDate);
            sql = sql.Replace("<ArrivedQty>", totalArrivedQty.ToString());
            sql = sql.Replace("<Price>", Price);
            sql = sql.Replace("<PONum>", PO);
            sql = sql.Replace("<POLine>", Line);
            sql = sql.Replace("<PORelNum>", Rel);

            oDAL.Execute(sql);

            oPO.UpdateHasActionAHR(ActionAHR, GUID, PO, Line, Rel, FinalQty.ToString());


            return Json(new { Status = "Updated", Message = "Updated Successfully." }, JsonRequestBehavior.AllowGet);
        }

        /// END PATCH Method

        [HttpPost]
        public JsonResult ProposeUpdate(string GUID, string Qty, string Price, string DueDate, string Message, string PONo, string PartNo, string ProposeEmailId, string[] CCemails)
        {

            string userName = Session["Username"].ToString();
            oPO = new NewPO();
            oPO.UpdatePropose(GUID, Qty, Price, DueDate, Message, userName, PONo, PartNo, ProposeEmailId, CCemails);

            var jsonResult = Json("Updated", JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;


            return jsonResult;
        }

        public DataTable GetPODataFilter(string tDate, string fDate, string partNo, string partDesc)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string userName = Session["Firstname"].ToString();
            string sql = @"SELECT * FROM [dbo].[tblPurchaseOrder] WHERE 1=1";

            if (!string.IsNullOrEmpty(tDate))
            {
                sql += " AND Calculated_DueDate <= '" + Convert.ToDateTime(tDate).ToString("yyyy-MM-dd") + "'";
            }

            if (!string.IsNullOrEmpty(fDate))
            {
                sql += " AND Calculated_DueDate >= '" + Convert.ToDateTime(fDate).ToString("yyyy-MM-dd") + "'";
            }

            if (!string.IsNullOrEmpty(partNo))
            {
                sql += " AND PODetail_PartNum = '" + partNo + "'";
            }

            if (!string.IsNullOrEmpty(partDesc))
            {
                sql += " AND PODetail_LineDesc LIKE '%" + partDesc + "%'";
            }

            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql);
            return dt;
        }

        public DataTable GetPODataFilterAPI(string tDate, string fDate, string partNo, string partDesc)
        {
            DataTable dt = Session["APIData"] as DataTable;

            // Convert the DataTable to Enumerable to use LINQ
            var query = dt.AsEnumerable();

            // Apply the date filters if provided
            if (!string.IsNullOrEmpty(tDate))
            {
                DateTime dueDateTo = Convert.ToDateTime(tDate);
                query = query.Where(row => Convert.ToDateTime(row["Calculated_DueDate"]) <= dueDateTo);
            }

            if (!string.IsNullOrEmpty(fDate))
            {
                DateTime dueDateFrom = Convert.ToDateTime(fDate);
                query = query.Where(row => Convert.ToDateTime(row["Calculated_DueDate"]) >= dueDateFrom);
            }

            // Apply the part number filter if provided
            if (!string.IsNullOrEmpty(partNo))
            {
                query = query.Where(row => row["PODetail_PartNum"].ToString() == partNo);
            }

            // Apply the part description filter if provided
            if (!string.IsNullOrEmpty(partDesc))
            {
                query = query.Where(row => row["PODetail_LineDesc"].ToString().Contains(partDesc));
            }

            // Create a new filtered DataTable from the query result
            if (query.Any())
            {
                return query.CopyToDataTable();
            }
            else
            {
                return dt.Clone(); // Return an empty DataTable with the same structure if no rows match
            }
        }

        [HttpPost]
        public ActionResult MultipleEmail(List<Dictionary<string, string>> tableData, string[] CCemails)
        {
            try
            {
                NewPO oPO = new NewPO();
                Guid newGuid = Guid.NewGuid();
                bool isUpdate = false;
                string contactReasonMultiple = "";
                string ConnctionType = Session["DefaultDB"].ToString();
                List<string> CCemailAddresses = new List<string>();
                // Dictionary to hold lists of data for each vendor
                Dictionary<string, List<Dictionary<string, string>>> vendorData = new Dictionary<string, List<Dictionary<string, string>>>();
                // Group data by vendor
                foreach (var row in tableData)
                {
                    // Assuming each row dictionary contains a key for 'vendor'
                    if (row.ContainsKey("Vendor_Name"))
                    {
                        string vendorName = row["Vendor_Name"];
                        contactReasonMultiple = row["Reason"];
                        // Check if the vendor exists in the dictionary
                        if (!vendorData.ContainsKey(vendorName))
                        {
                            // If not, add a new list for this vendor
                            vendorData[vendorName] = new List<Dictionary<string, string>>();
                        }

                        // Add the row to the list corresponding to the vendor
                        vendorData[vendorName].Add(row);

                    }

                    else
                    {
                        // Handle missing 'vendor' key in the row dictionary if necessary
                        // For example: log a warning, skip the row, etc.
                    }
                }
                foreach (var vendorEntry in vendorData)
                {
                    string vendorName = vendorEntry.Key;
                    string PO = "";
                    string orgPO = "";
                    string orgLine = "";
                    string orgRel = "";
                    string PartNo = "";
                    string Qty = "";
                    string Price = "";
                    string DueDate = "";
                    string LineDesc = "";
                    string IUM = "";
                    string OrderDate = "";
                    string VendorId = "";
                    string BuyerId = "";
                    string userName = "";
                    string vendorEmail = "";
                    string CCvendorEmail = "";
                    string ContactReason = "";
                    string NewDueDate = "";
                    string OrderQty = "";
                    string BuyerEmail = "";
                    string SupplierCompany = "";

                    List<Dictionary<string, string>> vendorRows = vendorEntry.Value;

                    // Generate HTML table
                    StringBuilder htmlTable = new StringBuilder();
                    htmlTable = GetMultiEmailTable(contactReasonMultiple);


                    // Append data for each row
                    foreach (var row in vendorRows)
                    {
                        PO = row["PONumber"];
                        string[] POArray = PO.Split('-');

                        orgPO = POArray[0];
                        orgLine = POArray[1];
                        orgRel = POArray[2];
                        PartNo = row["PODetail_PartNum"];

                        Qty = row["POQty"];
                        OrderQty = row["PODetail_OrderQty"];
                        Price = row["PODetail_UnitCost"];
                        DueDate = row["PORel_DueDate"];
                        LineDesc = row["PODetail_LineDesc"];
                        IUM = row["PODetail_IUM"];
                        OrderDate = row["POHeader_OrderDate"];
                        VendorId = row["Vendor_VendorID"];
                        BuyerId = row["POHeader_BuyerID"];
                        userName = Session["UserName"].ToString();
                        vendorEmail = row["SupplierEmail"];
                        CCvendorEmail = row["CCemails"];
                        ContactReason = row["Reason"];
                        BuyerEmail = row["PurAgent_EMailAddress"];
                        SupplierCompany = row["POHeader_Company"];
                        if (ContactReason == "Change")
                        {
                            NewDueDate = row["NewDueDate"];
                        }


                        oPO.AddInBuyers(PO, orgLine, orgRel, PartNo, LineDesc, Qty, Price, IUM, DueDate, OrderDate, userName, VendorId, vendorName, vendorEmail, BuyerId, ContactReason, userName, newGuid.ToString(), "", isUpdate, NewDueDate, OrderQty, "", BuyerEmail, SupplierCompany);
                        if (ContactReason == "Change")
                        {
                            htmlTable.Append("<tr>");
                            htmlTable.Append($"<td>{PO}</td>");
                            htmlTable.Append($"<td>{PartNo}</td>");
                            htmlTable.Append($"<td>{userName}</td>");
                            htmlTable.Append($"<td>{Qty}</td>");
                            htmlTable.Append($"<td>{Price}</td>");
                            htmlTable.Append($"<td>{DueDate}</td>");
                            htmlTable.Append($"<td>{NewDueDate}</td>");
                            htmlTable.Append("</tr>");
                        }
                        else
                        {
                            htmlTable.Append("<tr>");
                            htmlTable.Append($"<td>{PO}</td>");
                            htmlTable.Append($"<td>{PartNo}</td>");
                            htmlTable.Append($"<td>{userName}</td>");
                            htmlTable.Append($"<td>{Qty}</td>");
                            htmlTable.Append($"<td>{Price}</td>");
                            htmlTable.Append($"<td>{DueDate}</td>");
                            htmlTable.Append("</tr>");
                        }


                    }
                    // Close table and card div
                    htmlTable.Append(@"
    </table>
  </div>
</div>
");

                    //URL
                    string DeployMode = WebConfigurationManager.AppSettings["DEPLOYMODE"];
                    DataTable dtURL = new DataTable();
                    dtURL = cCommon.GetEmailURL(DeployMode, "VendorMultiEmail");
                    string Accepturl = "";
                    string Changeurl = "";
                    Accepturl = dtURL.Rows[0]["URL"].ToString();
                    Changeurl = dtURL.Rows[1]["URL"].ToString();

                    // Add buttons
                    if (DeployMode == "PROD")
                    {

                        htmlTable.Append($@"<p>                                
    <a href=""{Accepturl}/Externals/VendorPOEmailMulti.aspx?GUID={newGuid}&Vendor={vendorName}&Action=MultiAccept&Connection={ConnctionType}"" class=""button"" target=""_blank"">Accept</a>
    <a href=""{Changeurl}/Externals/VendorPOEmailMulti.aspx?GUID={newGuid}&Vendor={vendorName}&Action=MultiChange&Connection={ConnctionType}"" class=""button secondary"" target=""_blank"">Change</a>
</p>
");
                    }
                    else
                    {
                        htmlTable.Append($@"<p>                                
    <a href=""{Accepturl}/Externals/VendorPOEmailMulti.aspx?GUID={newGuid}&Vendor={vendorName}&Action=MultiAccept&Connection={ConnctionType}"" class=""button"" target=""_blank"">Accept</a>
    <a href=""{Changeurl}/Externals/VendorPOEmailMulti.aspx?GUID={newGuid}&Vendor={vendorName}&Action=MultiChange&Connection={ConnctionType}"" class=""button secondary"" target=""_blank"">Change</a>
</p>
");
                    }

                    // Close body and HTML tags
                    htmlTable.Append(@"
</body>
</html>
");


                    // Send email
                    oPO.sendMultiEmail(htmlTable.ToString(), userName, PO, PartNo, Qty, DueDate, Price, "", newGuid.ToString(), vendorEmail, CCvendorEmail);
                    //oPO.AddInTransaction(PO, PartNo, newGuid.ToString(), "New", Qty, Price, DueDate, userName, "Multiple PO''s");
                }

                var jsonResult = Json("Updated", JsonRequestBehavior.AllowGet);
                // Optionally, return a JSON response indicating success
                return jsonResult;
            }
            catch (Exception ex)
            {
                cLog oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "Multiple PO Method");
                return Json(new { success = false, message = ex.Message + ex.StackTrace });
            }
        }

        [HttpPost]
        public ActionResult MultipleEmailPO(List<Dictionary<string, string>> tableData)
        {
            try
            {
                NewPO oPO = new NewPO();
                string ConnctionType = Session["DefaultDB"].ToString();
                List<string> CCemailAddresses = new List<string>();
                List<string> notApprovedPOs = new List<string>();
                List<string> approvedPOs = new List<string>();
                foreach (var row in tableData)
                {
                    // Make sure all required keys exist
                    if (row.ContainsKey("PONumber") &&
                        row.ContainsKey("Vendor_VendorID") &&
                        row.ContainsKey("Vendor_Name") &&
                        row.ContainsKey("Vendor_EMailAddress") &&
                        row.ContainsKey("POStatus"))
                    {
                        string PO = row["PONumber"];
                        string VendorId = row["Vendor_VendorID"];
                        string VendorName = row["Vendor_Name"];
                        string VendorEmailAddress = row["Vendor_EMailAddress"];
                        string Status = row["POStatus"];
                        string POApprove = row["POHeader_Approve"];
                        Guid newGuid;
                        if (POApprove == "true" || POApprove == "True")
                        {
                            string GUID = oPO.GetGUID(PO);
                            if (GUID == "")
                            {
                                newGuid = Guid.NewGuid();
                            }
                            else
                            {
                                newGuid = new Guid(GUID);
                            }
                            string result = oPO.AddPOHeaderData(PO, VendorId, VendorName, VendorEmailAddress, Status, newGuid.ToString());
                            oPO.AddPOHeaderTransaction(PO, VendorName, VendorEmailAddress, newGuid.ToString());
                            approvedPOs.Add(PO);
                        }
                        else
                        {

                            notApprovedPOs.Add(PO);
                        }

                    }

                }

                string message = string.Empty;

                if (notApprovedPOs.Count > 0)
                {
                    message = $"PO No. {string.Join(", ", notApprovedPOs)} {(notApprovedPOs.Count > 1 ? "are" : "is")} not approved.";
                    return Json(new
                    {
                        success = false,
                        message = message,
                        approvedCount = approvedPOs.Count,
                        notApprovedCount = notApprovedPOs.Count
                    }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        message = "Email has been sent successfully.",
                        approvedCount = approvedPOs.Count,
                        notApprovedCount = notApprovedPOs.Count
                    }, JsonRequestBehavior.AllowGet);

                }



            }
            catch (Exception ex)
            {
                cLog oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "Multiple PO Method");
                return Json(new { success = false, message = ex.Message + ex.StackTrace });
            }
        }


        public byte[] GetPDFByteArray(string poNo, string connectionType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poNo) || string.IsNullOrWhiteSpace(connectionType))
                    return null;

                double PONum = Convert.ToDouble(poNo);
                int PO = (int)PONum;

                // Example: Simulate Epicor PDF API call status
                string result = PDFAPICall(poNo);
                if (result != "OK")
                    return null;


                // Wait 15 seconds before calling the actual PDF fetch API
                System.Threading.Thread.Sleep(15000);

                // Get API configuration
                DataTable dtURLPDF = cCommon.GetEmailURL(connectionType.ToUpper(), "GETPDF");
                string urlBase = dtURLPDF.Rows[0]["URL"].ToString();
                string pageUrl = dtURLPDF.Rows[0]["PageURL"].ToString();
                string userName = dtURLPDF.Rows[0]["UserName"].ToString();
                string password = BasicEncrypt.Instance.Decrypt(dtURLPDF.Rows[0]["Password"].ToString().Trim());
                string apiKey = dtURLPDF.Rows[0]["TokenKey"].ToString();

                // Final API URL
                string finalUrl = urlBase + pageUrl + "?PONum=" + PO;

                // Setup REST client
                var client = new RestClient(urlBase);
                var request = new RestRequest(finalUrl, Method.Get);
                string auth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password));

                request.AddHeader("Authorization", "Basic " + auth);
                request.AddHeader("api-key", apiKey);

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseString = Encoding.UTF8.GetString(response.RawBytes);
                    JObject json = JObject.Parse(responseString);
                    string base64Data = json["value"]?[0]?["SysRptLst_RptData"]?.ToString();

                    if (!string.IsNullOrEmpty(base64Data))
                        return Convert.FromBase64String(base64Data);
                }

                return null;
            }
            catch
            {
                return null;
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

        public StringBuilder GetMultiEmailTable(string contactReason)
        {
            StringBuilder htmlTable = new StringBuilder();
            htmlTable.Append(@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Email Content</title>
  <style>
    /* Define CSS styles */
    .card {
      box-shadow: 0 4px 8px 0 rgba(0,0,0,0.2);
      transition: 0.3s;
      width: 100%;
      border-radius: 5px;
      margin-bottom: 20px;
    }
    
    .card-body {
      padding: 20px;
    }
    
    .card-title {
      font-size: 18px;
      margin-bottom: 10px;
    }
    
    .card-text {
      font-size: 14px;
      color: #555;
    }
    
    table {
      width: 100%;
      border-collapse: collapse;
    }
    
    th, td {
      border: 1px solid #ddd;
      padding: 8px;
      text-align: left;
    }
    
    th {
      background-color: #f2f2f2;
    }
    
    tr:nth-child(even) {
      background-color: #f2f2f2;
    }
    
    tr:hover {
      background-color: #ddd;
    }
    
    a.button {
      display: inline-block;
      padding: 10px 15px;  
      background-color: #4CAF50;
      color: white;
      text-align: center;
      text-decoration: none;
      border: none;
      border-radius: 5px;
      cursor: pointer;
    }
    
    a.button.secondary {
      background-color: gray;
    }
  </style>
</head>
<body>
");
            if (contactReason == "Change")
            {
                // Add card with table
                htmlTable.Append(@"
<div class=""card"">
  <div class=""card-body"">
    <h5 class=""card-title"">Purchase Order Detail Due Date Change Request</h5>
    <table>
      <tr>
        <th>PO No.</th>
        <th>Part No.</th>
        <th>Buyer</th>
        <th>QTY</th>
        <th>PRICE</th>
        <th>DUE DATE</th>
        <th>NEW DUE DATE</th>
      </tr>
");
            }
            else if (contactReason == "Update")
            {
                // Add card with table
                htmlTable.Append(@"
<div class=""card"">
  <div class=""card-body"">
    <h5 class=""card-title"">Purchase Order Detail Update Request</h5>
    <table>
      <tr>
        <th>PO No.</th>
        <th>Part No.</th>
        <th>Buyer</th>
        <th>QTY</th>
        <th>PRICE</th>
        <th>DUE DATE</th>   
      </tr>
");
            }
            else
            {
                // Add card with table
                htmlTable.Append(@"
<div class=""card"">
  <div class=""card-body"">
    <h5 class=""card-title"">Purchase Order Detail Confirmation Request</h5>
    <table>
      <tr>
        <th>PO No.</th>
        <th>Part No.</th>
        <th>Buyer</th>
        <th>QTY</th>
        <th>PRICE</th>
        <th>DUE DATE</th>
      </tr>
");
            }
            return htmlTable;
        }

        public JsonResult GetFilterList(string POStatus, string tDate, string fDate, string partNo, string partDesc)
        {
            DataTable dt = new DataTable();
            string menuTitle = string.Empty;
            string RptCode;

            string ConnctionType = Session["DefaultDB"].ToString();

            if (ConnctionType.ToUpper() == "PROD" || ConnctionType.ToUpper() == "PILOT")
            {
                dt = GetPODataFilterAPI(tDate, fDate, partNo, partDesc);
            }
            else
            {
                dt = GetPODataFilter(tDate, fDate, partNo, partDesc);
            }
            //string jsonstring = response.Content;

            string userType = Session["UserType"].ToString();
            string Email = Session["Email"].ToString();
            if (userType == "Buyer" || userType == "Supplier")
            {
                string whereCondition = "";
                // Define your WHERE condition
                if (userType == "Buyer")
                    whereCondition = "PurAgent_EMailAddress = '" + Email + "'";
                else if (userType == "Supplier")
                    whereCondition = "Vendor_EMailAddress = '" + Email + "'";
                // Filter the DataTable based on the WHERE condition
                DataRow[] filteredRows = dt.Select(whereCondition);

                // Create a new DataTable with filtered rows
                DataTable filteredTable = dt.Clone(); // Cloning the structure of the original DataTable
                foreach (DataRow row in filteredRows)
                {
                    filteredTable.ImportRow(row);
                }
                oPO.GetList(filteredTable, POStatus);

                var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                //LOAD MRU & LOG QUERY
                if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
                {
                    menuTitle = TempData["ReportTitle"] as string;
                    RptCode = TempData["RptCode"].ToString();
                    TempData.Keep();
                    //cLog oLog = new cLog();
                    //oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }
                return jsonResult;
            }
            else
            {
                oPO.GetList(dt, POStatus);

                var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                //LOAD MRU & LOG QUERY
                if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
                {
                    menuTitle = TempData["ReportTitle"] as string;
                    RptCode = TempData["RptCode"].ToString();
                    TempData.Keep();
                    //cLog oLog = new cLog();
                    //oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }
                return jsonResult;
            }

        }

        public JsonResult GetSuggestEmail(string id)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string email = "";
            string query = @"SELECT 
[VendorEmail]
  FROM [SRM].[BuyerPO]
  WHERE GUID = '" + id + "'";
            DataTable dt = new DataTable();
            dt = oDAL.GetData(query);
            if (dt.Rows.Count > 0)
            {
                email = dt.Rows[0]["VendorEmail"].ToString();
            }

            return Json(new { success = true, email = email }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetDropdownData()
        {
            NewPO oPO = new NewPO();

            // --- Buyers ---
            var dtBuyers = oPO.GetBuyerList();
            List<SelectListItem> buyers = new List<SelectListItem>();
            foreach (DataRow row in dtBuyers.Rows)
            {
                buyers.Add(new SelectListItem
                {
                    Value = row["BuyerID"].ToString(),
                    Text = row["Name"].ToString()
                });
            }

            // --- Suppliers ---
            var dtSuppliers = oPO.GetSupplierList();
            List<SelectListItem> suppliers = new List<SelectListItem>();
            foreach (DataRow row in dtSuppliers.Rows)
            {
                suppliers.Add(new SelectListItem
                {
                    Value = row["Vendor_VendorID"].ToString(),
                    Text = row["Vendor_Name"].ToString()
                });
            }

            return Json(new { Buyers = buyers, Suppliers = suppliers }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FilterPOData(string buyerEmail, string supplierEmail, string dueFrom, string dueTo, string POStatus, string PartNo)
        {
            string ConnctionType = Session["DefaultDB"].ToString();
            string Email = Session["Email"].ToString();
            string userType = Session["UserType"].ToString();
            NewPO oPO = new NewPO();
            DataTable dt = new DataTable();
            dt = oPO.GetFilteredPOs(buyerEmail, supplierEmail, dueFrom, dueTo, PartNo);

            oPO.GetPO(dt, POStatus, ConnctionType, Email, userType);

            var jsonResult = Json(oPO, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            // LOAD MRU & LOG QUERY
            cLog oLog = new cLog();
            oLog.SaveLog("Purchase Order Form Filter", Request.Url.PathAndQuery, "001");

            return jsonResult;
        }
        [HttpPost]
        public JsonResult ResetStatus(string PoNo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string query = @" Update BuyerPOHeader set isActive = '0', Status = 'Deleted' Where PONumber = '" + PoNo + "'";

            oDAL.Execute(query);
            if (!oDAL.HasErrors)
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult ReceivedStatus(string PoNo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string userName = Session["Username"].ToString();
            string query = @" Update BuyerPOHeader set Status = 'Received', UpdatedOn = GetDate(),
                            UpdatedBy = '" + userName + "', IsEmailSent = 1 Where PONumber = '" + PoNo + "' AND Status = 'Sent'";

            oDAL.Execute(query);
            if (!oDAL.HasErrors)
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

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

        public static DataTable ConvertJsonToDataTable(string json)
        {
            // Parse the JSON
            JToken token = JToken.Parse(json);

            // Initialize DataTable
            DataTable dataTable = new DataTable();

            if (token.Type == JTokenType.Array)
            {
                // If JSON is an array, extract column names from the properties of the first object
                JArray jsonArray = (JArray)token;
                if (jsonArray.Count > 0)
                {
                    JObject firstObject = (JObject)jsonArray.First;
                    foreach (JProperty property in firstObject.Properties())
                    {
                        dataTable.Columns.Add(property.Name, typeof(string)); // Adjust type as needed
                    }

                    // Populate the DataTable with JSON data
                    var rows = jsonArray.Select(j => ((JObject)j).Properties()
                                        .Select(p => p.Value.ToString()).ToArray());
                    foreach (var row in rows)
                    {
                        dataTable.Rows.Add(row);
                    }
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                // If JSON is a single object, extract column names from its properties
                JObject jsonObject = (JObject)token;
                foreach (JProperty property in jsonObject.Properties())
                {
                    dataTable.Columns.Add(property.Name, typeof(string)); // Adjust type as needed
                }

                // Populate a single row in the DataTable
                dataTable.Rows.Add(jsonObject.Properties()
                                        .Select(p => p.Value.ToString()).ToArray());
            }
            else
            {
                throw new ArgumentException("Invalid JSON format.");
            }

            return dataTable;
        }

        //static DataTable CreateDataTableWithSameSchema()
        //{
        //    DataTable dataTable = new DataTable();

        //    dataTable.Columns.Add("POHeader_PONum", typeof(string));
        //    dataTable.Columns.Add("PODetail_POLine", typeof(string));
        //    dataTable.Columns.Add("PORel_PORelNum", typeof(string));
        //    dataTable.Columns.Add("PODetail_PartNum", typeof(string));
        //    dataTable.Columns.Add("PODetail_LineDesc", typeof(string));
        //    dataTable.Columns.Add("PODetail_IUM", typeof(string));
        //    dataTable.Columns.Add("PODetail_OrderQty", typeof(string));
        //    dataTable.Columns.Add("PORel_ReceivedQty", typeof(string));
        //    dataTable.Columns.Add("PORel_ArrivedQty", typeof(string));
        //    dataTable.Columns.Add("RcvDtl_OurQty", typeof(string));
        //    dataTable.Columns.Add("POHeader_OrderDate", typeof(string));
        //    dataTable.Columns.Add("PORel_DueDate", typeof(string));
        //    dataTable.Columns.Add("RcvDtl_ArrivedDate", typeof(string));
        //    dataTable.Columns.Add("Calculated_BaseTotalAmount", typeof(string));

        //    return dataTable;
        //}
    }
}