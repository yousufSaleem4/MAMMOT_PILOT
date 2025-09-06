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
            oHome.GetHours();
            oHome.GetAPISettings();
            oHome.GetCCEmail();

            ViewBag.HoursValue = oHome.Hours;
            ViewBag.URL = oHome.ApiUrl;
            ViewBag.username = oHome.Username;
            ViewBag.password = oHome.password;
            ViewBag.token = oHome.token;

            ViewBag.SQlConn = oHome.SQlConn;
            ViewBag.SQLUsername = oHome.SQLUsername;
            ViewBag.SQLpassword = oHome.SQLpassword;

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

            return View();
        }

        [HttpPost]
        public ActionResult UpdateSetting(string conType, string Hours, string ApiUrl, string Username, string password, string token, string SQlConn, string SQLUsername, string SQLpassword, string TimeZone, string CCEmail, string TermsCondition)
        {

            if (cCommon.IsSessionExpired())
            {
                return RedirectToAction("Login");
            }
            else
            {
                Home oHome = new Home();
                cAuth oAuth = new cAuth();

                bool isUpdateSetting = updateHours(Hours);
                bool isUpdateAPISetting = updateAPISetting(conType, ApiUrl, Username, password, token, SQlConn, SQLUsername, SQLpassword);
                updateDefaultDB(conType);
                string decodedTerms = HttpUtility.UrlDecode(TermsCondition);
                updatetermCondition(decodedTerms);
                Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oHome.GetConnectionString(conType));
                //  Session["CONN_TYPE"] = conType;
                bool isUpdated = oAuth.UpdateDfltCon(conType);

                oAuth.UpdateTimeZone(TimeZone);
                Session["TimeZone"] = TimeZone;
                oAuth.UpdateCCEmail(CCEmail);


                if (isUpdated)
                {
                    Session["DefaultDB"] = conType;

                }


                return RedirectToAction("Index", "Home");
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
        WHERE SysCode = '12647' 
          AND SysDesc = 'Terms and Condition'";

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
            SQLpassword = BasicEncrypt.Instance.Encrypt(SQLpassword.Trim());

            string sql = "";
            if (conType.ToUpper() != "TEST")
            {
                sql = @"UPDATE [SRM].[zConStr] SET ConValue = '<convelue>', username = '<username>', 
                    password = '<password>' WHERE ConType = '" + conType + "'";

                sql = sql.Replace("<convelue>", SQlConn);
                sql = sql.Replace("<username>", SQLUsername);
                sql = sql.Replace("<password>", SQLpassword);
                oDAL.Execute(sql);

                sql = @"UPDATE [dbo].[URLSetup]
                           SET URL = '" + ApiUrl + "', " +
                        "Username = '" + Username + "', " +
                        "password = '" + Password + "', " +
                        "TokenKey = '" + token + "' " +
                        "where URLType = 'API' AND Deploymode = '" + conType + "'";
                oDAL.Execute(sql);
                if (oDAL.HasErrors)
                    return false;
            }
            else
            {
                sql = @"UPDATE [SRM].[zConStr] SET ConValue = '<convelue>', username = '<username>', 
                    password = '<password>' WHERE ConType = '" + conType + "'";

                sql = sql.Replace("<convelue>", SQlConn);
                sql = sql.Replace("<username>", SQLUsername);
                sql = sql.Replace("<password>", SQLpassword);
                oDAL.Execute(sql);
                if (oDAL.HasErrors)
                    return false;
            }
            return true;
        }

        public JsonResult CheckAPI(string conType, string ApiUrl, string Username, string password, string token)
        {
            NewPOCommon oPOCommon = new NewPOCommon();
            DataTable dt = new DataTable();

            string menuTitle = string.Empty;

            DataTable dtURL = new DataTable();
            dtURL = cCommon.GetEmailURL(conType.ToUpper(), "APIOPENPO");

            var client = new RestClient(ApiUrl);
            var request = new RestRequest(dtURL.Rows[0]["PageURL"].ToString(), Method.Get);

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