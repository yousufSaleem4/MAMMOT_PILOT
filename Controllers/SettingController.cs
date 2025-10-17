using IP.Classess;
using PlusCP.Classess;
using PlusCP.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlusCP.Controllers
{
    public class SettingController : Controller
    {
        // GET: Setting

        public ActionResult Index()
        {
            Home oHome = new Home();
            DataSet dsCon = oHome.GetConnections();

            DataTable dt = dsCon.Tables["CONN"];
            if (dt.Rows.Count > 0)
            {
                dt.DefaultView.RowFilter = "IsDropDown = true";
                ViewBag.Connections = cCommon.ToDropDownList(dt.DefaultView.ToTable(), "ConType", "ConText", Session["DefaultDB"].ToString(), "ConText");

            }
            oHome.GetSysSettings();
            oHome.GetAPISettings();
            oHome.GetCCEmail();

            ViewBag.HoursValue = oHome.Hours;
            ViewBag.IsQtyUpdate = oHome.IsQtyUpdate;
            ViewBag.IsPriceUpdate = oHome.IsPriceUpdate;


            ViewBag.ApiUrl = oHome.ApiUrl;
            ViewBag.username = oHome.Username;
            ViewBag.ApiPassword = oHome.password;
            ViewBag.ApiToken = oHome.token;
            ViewBag.TermsCondition = oHome.TermsCondition;
            


            // Example: List of cities and their corresponding timezones
            DateTime utcNow = DateTime.UtcNow;

            var timeZones = TimeZoneInfo.GetSystemTimeZones()
                             .Where(tz => tz.Id.Contains("US") ||
                                          tz.Id.Contains("Pacific") ||
                                          tz.Id.Contains("Eastern") ||
                                          tz.Id.Contains("Central") ||
                                          tz.Id.Contains("Mountain") ||
                                          tz.Id.Contains("Hawaiian") ||
                                          tz.Id.Contains("Alaskan") ||
                                          tz.Id.Contains("Pakistan Standard Time"))
                             .Select(tz =>
                             {
                     // Check if DST is currently in effect for this timezone
                     bool isDST = tz.IsDaylightSavingTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz));

                                 string displayName = $"{tz.DisplayName} ({tz.Id})";
                                 if (isDST)
                                     displayName += " - DST in effect";

                                 return new SelectListItem
                                 {
                                     Text = displayName,
                                     Value = tz.Id
                                 };
                             })
                             .ToList();

            // Convert to DataTable
            DataTable dtTime = cCommon.ConvertSelectListToDataTable(timeZones);
            ViewBag.ddlTimeZone = cCommon.ToDropDownList(dtTime, "Value", "Text", Session["TimeZone"].ToString(), "Value");

            ViewBag.CCEmailAddress = oHome.CCEmail;

            return View(oHome);
        }

        [HttpPost]
        public ActionResult UpdateSetting(string conType, string Hours, string ApiUrl, string Username, string password, string token, string SQlConn, string SQLUsername, string SQLpassword, string TimeZone, string CCEmail, string TermsCondition, bool IsUpdateQty, bool IsUpdatePrice)
        {
            string ConnctionType = Session["DefaultDB"].ToString();
            if (cCommon.IsSessionExpired())
            {
                return RedirectToAction("Login");
            }
            else
            {
                Home oHome = new Home();
                cAuth oAuth = new cAuth();

                bool isUpdateSetting = updateHours(Hours);
                bool isUpdateAPISetting = updateAPISetting(ConnctionType, ApiUrl, Username, password, token, SQlConn, SQLUsername, SQLpassword);
                //updateDefaultDB(conType);
                string decodedTerms = HttpUtility.UrlDecode(TermsCondition);
                updatetermCondition(decodedTerms);
                Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oHome.GetConnectionString(ConnctionType));
                //  Session["CONN_TYPE"] = conType;
                bool isUpdated = oAuth.UpdateDfltCon(ConnctionType);

                oAuth.UpdateTimeZone(TimeZone);
                UpdatePriceQtySettings(IsUpdateQty, IsUpdatePrice);

                Session["TimeZone"] = TimeZone;
                oAuth.UpdateCCEmail(CCEmail);


                if (isUpdated)
                {
                    Session["DefaultDB"] = ConnctionType;

                }

                var jsonResult = Json("Updated", JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

        }

        public bool updatetermCondition(string TermsCondition)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);

            // single quotes escape karna zaroori hai
            TermsCondition = TermsCondition.Replace("'", "''");

            string sql = $@"
        UPDATE [dbo].[zSysIni] 
        SET SysValue = '{TermsCondition}' 
        WHERE SysDesc = 'TermsandCondition'  ";

            oDAL.Execute(sql);

            if (oDAL.HasErrors)
                return false;

            return true;
        }
        public bool updateDefaultDB(string conType)
        {

            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"UPDATE [SRM].[UserInfo]
                           SET DefaultDB = '" + conType + "' ";

            oDAL.Execute(sql);
            if (oDAL.HasErrors)
                return false;
            return true;
        }
        public bool updateHours(string Hours)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"UPDATE [dbo].[zSysIni]
                           SET SysValue = '" + Hours + "' " +
                           "WHERE SysDesc = 'Hours' ";
            oDAL.Execute(sql);
            if (oDAL.HasErrors)
                return false;
            return true;
        }
        public bool updateAPISetting(string conType, string ApiUrl, string Username, string password, string token, string SQlConn, string SQLUsername, string SQLpassword)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string Password = BasicEncrypt.Instance.Encrypt(password.Trim());

            string sql = "";
       
                sql = @"UPDATE [dbo].[URLSetup]
                           SET URL = '" + ApiUrl + "', " +
                        "Username = '" + Username + "', " +
                        "password = '" + Password + "', " +
                        "TokenKey = '" + token + "' " +
                        "where URLType = 'API' AND Deploymode = '" + conType + "'";
                oDAL.Execute(sql);
                if (oDAL.HasErrors)
                    return false;
            
            
            return true;
        }

        public bool UpdatePriceQtySettings(bool IsUpdateQty, bool IsUpdatePrice)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"UPDATE [dbo].[zSysIni]
                           SET SysValue = '" + IsUpdateQty + "' " +
                           "WHERE SysDesc = 'IsQtyUpdate'; ";

            sql += @"UPDATE [dbo].[zSysIni]
                           SET SysValue = '" + IsUpdatePrice + "' " +
                           "WHERE SysDesc = 'IsPriceUpdate';";

            oDAL.Execute(sql);
            if (oDAL.HasErrors)
            {
                
                return false;
            }
            Session["IsQtyUpdate"] = IsUpdateQty;
            Session["IsPriceUpdate"] = IsUpdatePrice;
            return true;
        }



        public JsonResult CheckAPI(string conType, string ApiUrl, string Username, string password, string token)
        {
            NewPOCommon oPOCommon = new NewPOCommon();
            DataTable dt = new DataTable();

            string menuTitle = string.Empty;
            string ConnctionType = Session["DefaultDB"].ToString();

            DataTable dtURL = new DataTable();
            dtURL = cCommon.GetEmailURL(ConnctionType.ToUpper(), "APIOPENPO");

            var client = new RestClient(ApiUrl);
            var request = new RestRequest(dtURL.Rows[0]["PageURL"].ToString() + "?$top=1", Method.Get);

            // Add basic authentication header
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(Username + ":" + password)));
            request.AddHeader("api-key", token);

            var response = client.Execute(request);
            if (response.IsSuccessStatusCode == true)
            {
                var jsonResult = Json("OK", JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }

            else
            {
                var jsonResult = Json("NOT", JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        public JsonResult CheckSQLConnction(string conType, string SQlConn, string SQLUsername, string SQLpassword)
        {

            DataTable dt = new DataTable();

            if (IsSqlConnectionOk(SQlConn))
            {
                var jsonResult = Json("OK", JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }

            else
            {
                var jsonResult = Json("NOT", JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        public JsonResult GetConnectionData(string conType)
        {
            string menuTitle = string.Empty;
            string RptCode;

            Home oHome = new Home();
            oHome.GetConnectionData(conType);
            var jsonResult = Json(oHome, JsonRequestBehavior.AllowGet);
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

        static bool IsSqlConnectionOk(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }


    
    }
}