using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PlusCP.Models;
using IP.Classess;
using CaptchaMvc.HtmlHelpers;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Data.SqlClient;
using PlusCP.Classess;

namespace PlusCP.Controllers
{
    public class HomeController : Controller
    {
        Home oHome;
        cAuth oAuth;
        string DeployMode = WebConfigurationManager.AppSettings["DEPLOYMODE"];
        //cEmployee oEmp = null;
        public ActionResult Login()
        {
            oAuth = new cAuth();
            if (cCommon.IsSessionExpired())
                return View(oAuth);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Login(cAuth oAuth)
        {
            if (string.IsNullOrEmpty(oAuth.SigninId))
            {
                ModelState.AddModelError("SigninId", "Email is required ");
                return View("Login");
            }
            else if (!IsValidEmail(oAuth.SigninId))
            {
                ModelState.AddModelError("SigninId", "Email is incorrect ");
                return View("Login");
            }
            else if (string.IsNullOrEmpty(oAuth.Password))
            {
                ModelState.AddModelError("Password", "Password is required ");
                return View("Login");
            }

            else if (!this.IsCaptchaValid(""))
            {
                ModelState.AddModelError("Captcha", "Please, enter a valid captcha. ");
                return View("Login");
            }




            //ViewBag.ErrMessage = "Error: captcha is not valid.";



            bool isValidUser = oAuth.IsValidUser(oAuth.SigninId, oAuth.Password);


            if (isValidUser)
            {
                Session["SigninId"] = oAuth.SigninId;
                Session["ProgramId"] = oAuth.DefaultProgram;
                Session["Program"] = oAuth.Program;
                Session["FirstName"] = oAuth.FirstName;
                Session["LastName"] = oAuth.LastName;
                Session["VendorList"] = oAuth.VendorList;
                Session["Username"] = oAuth.UserName;
                Session["FullName"] = oAuth.FirstName + " " + oAuth.LastName;
                Session["isAdmin"] = oAuth.isAdmin;
                Session["DefaultProgram"] = oAuth.DefaultProgram;
                Session["DefaultDB"] = oAuth.DefaultDB;
                Session["isCustomer"] = oAuth.isCustomer;
                Session["UserType"] = oAuth.UserType;
                Session["Email"] = oAuth.EmailAddress;
                Session["IsTempKey"] = oAuth.IsTempKey;
                Session["TimeZone"] = oAuth.TimeZoneId;
                //Session["CONN_TYPE"] =oAuth.DefaultDB;
                Session["dtBuyerInfo"] = null;
              
                
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.ErrorMessage = oAuth.Message;
                ModelState.AddModelError("Password", oAuth.Message);
                return View("Login");
            }
        }
        public ActionResult CreatePassword(string OldPassword, string NewPassword)
        {
            DataTable dt = new DataTable();
            string Email = Session["Email"].ToString();
            string _OldPassword = BasicEncrypt.Instance.Encrypt(OldPassword.Trim());
            string _NewPassword = BasicEncrypt.Instance.Encrypt(NewPassword.Trim());

            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = "Select * from [SRM].[UserInfo] where Email = '" + Email + "' AND Password = '" + _OldPassword + "'";
            dt = oDAL.GetData(sql);
            JsonResult jsonResult = null;
            if (dt.Rows.Count > 0)
            {
                sql = "UPDATE [SRM].[UserInfo] SET Password = '" + _NewPassword + "', IsTempKey = 0 WHERE Email = '" + Email + "' ";
                oDAL.Execute(sql);
                Session["IsTempKey"] = false;
                jsonResult = Json("Updated", JsonRequestBehavior.AllowGet);
            }
            else
            {
                jsonResult = Json("NotMatch", JsonRequestBehavior.AllowGet);
              
            }

            return jsonResult;

        }
        static bool IsValidEmail(string email)
        {
            // Regular expression pattern for email validation
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Check if the email matches the pattern
            return Regex.IsMatch(email, pattern);
        }
        //Register Controller
        public ActionResult Register()
        {
            return View(new SignupViewModel());
        }

        [HttpPost]
        public ActionResult Register(SignupViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Generate and send verification email
                //SendVerificationEmail(model.Email);

                // Redirect to a page indicating email verification is required
                return RedirectToAction("VerificationPending");
            }

            return View(model);
        }

        public JsonResult SignupVerification(string FirstName, string LastName, string EmailAddress, string Pwd, string CnfrmPwd, string Type, string BuyerEmail)
        {
            cAuth oAuth = new cAuth();
            string tokenId = Guid.NewGuid().ToString("N").Substring(0, 10); // Take the first 10 characters



            //Check User Email Already Exist
            if (CheckUserEmail(EmailAddress))
            {

                oAuth.UserVerified(tokenId, FirstName, LastName, EmailAddress, Pwd, Type, BuyerEmail);

                //For varification buyer can verified 
                if (Type == "Supplier")
                {
                    EmailAddress = BuyerEmail;
                }
                DataTable dt = new DataTable();
                dt = GetEmailSMTPSetup();
                // Sender's email address and password
                string senderEmail = dt.Rows[0]["SenderEmail"].ToString();
                string senderPassword = dt.Rows[0]["Password"].ToString();

                //URL
                DataTable dtURL = new DataTable();
                dtURL = cCommon.GetEmailURL(DeployMode, "EmailVerify");
                string Url = dtURL.Rows[0]["URL"].ToString();
                string Baseurl = Url + dtURL.Rows[0]["PageURL"].ToString() + tokenId;

                // Send EMail To admins
                cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
                string sql = "select Email from [SRM].[UserInfo] Where isAdmin = 1 AND [Type] = 'Admin' ";
                DataTable dtAdmin = new DataTable();
                dtAdmin = oDAL.GetData(sql);
                var subject = "Email Verification";

                // SMTP server settings (e.g., for Gmail)
                string smtpHost = dt.Rows[0]["SMTPHost"].ToString();
                int smtpPort = Convert.ToInt32(dt.Rows[0]["SMTPPort"]);  // Port 587 for Gmail SMTP
                bool enableSsl = Convert.ToBoolean(dt.Rows[0]["EnableSSL"]);

                // Send Email to admins

                if (dtAdmin.Rows.Count > 0)
                {
                    foreach (DataRow row in dtAdmin.Rows)
                    {
                        string recipientEmail = row["Email"].ToString();
                        MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail);
                        mailMessage.Subject = subject;
                        mailMessage.IsBodyHtml = true;
                        string htmlBody = "";
                        if (Type == "Supplier")
                        {
                            // HTML body containing the form
                            htmlBody = $@"<!DOCTYPE html>
                            <html>
                            <head>
                                <title>Email Verification</title>
                            </head>
                            <body>
                                <h2>Click here to verify Supplier email.</h2>
                                <p>
                                    <a href=""<URL>"" target=""_blank"">Click here to verify Supplier email.</a>
                                </p>
                            </body>
                            </html>";
                        }
                        else
                        {                   
                            DataTable dtHtml = new DataTable();
                            string query = "";
                            oDAL = new cDAL(cDAL.ConnectionType.INIT);
                            query = "Select SYSValue from dbo.zSysIni WHERE SysDesc = 'SIGNUPVERIFICATION'";
                            dt = oDAL.GetData(query);
                            htmlBody = dt.Rows[0]["SYSValue"].ToString();

                        }


                        try
                        {
                            // Send the email
                            cCommon.SendEmail(EmailAddress, subject, htmlBody, "", null);
                            oAuth.SendEmailVerification(FirstName, LastName, senderEmail, row["Email"].ToString(), subject, htmlBody);

                        }
                        catch (Exception)
                        {
                            ViewBag.Error = "Some Error";
                        }
                    }

                }



                oAuth.EmailMessage = "Sent";
                oAuth.EmailMessageType = Type;
                var jsonResult = Json(oAuth, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                oAuth.EmailMessage = "Already";
                oAuth.EmailMessageType = Type;
                return Json(oAuth, JsonRequestBehavior.AllowGet);
            }


        }
        public bool CheckUserEmail(string Email)
        {
            object result = null;
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = "Select Email from [SRM].[UserInfo] where Email = '" + Email + "'";
            result = oDAL.GetObject(sql);

            if (result != null)
                return false;
            else
                return true;

        }
        public DataTable GetEmailSMTPSetup()
        {
            string sql = "Select * from [dbo].[EmailSetup]";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql);
            return dt;
        }
        public DataTable GetEmailURL(string DeployMode)
        {
            string sql = "SELECT * FROM [dbo].[URLSetup] WHERE DeployMode = '" + DeployMode + "'";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql);
            return dt;
        }
        public ActionResult VerificationPending()
        {
            // This view should indicate that the user's signup is successful but email verification is pending
            return View();
        }

        public ActionResult VerifyEmail(string email, string code)
        {
            // Here you would verify the code sent via email
            // If the code matches, you would update the user's status to verified in the database

            // For demonstration purposes, let's assume verification is successful and redirect to a success page
            return RedirectToAction("VerificationSuccess");
        }

        public ActionResult VerificationSuccess()
        {
            // This view should indicate that email verification is successful
            return View();
        }
        //


        public ActionResult Logout()
        {
            cCommon.SessionExpired();
            //this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //this.Response.Cache.SetNoStore();
            //return RedirectToAction("Login", "Home");
            return View();
        }

        public ActionResult Base()
        {
            if (cCommon.IsSessionExpired())
                return RedirectToAction("Login");
            else
                return View();
        }
        [Route("Main")]
        public ActionResult Index()
        {

            cAuth oAuth = new cAuth();
            if (cCommon.IsSessionExpired())
                return RedirectToAction("Login");
            else
            {
                Home oHome = new Home();
                DataSet dsCon = oHome.GetConnections();

                DataTable dt = dsCon.Tables["CONN"];
                if (dt.Rows.Count > 0)
                {
                    dt.DefaultView.RowFilter = "IsDropDown = true";
                    ViewBag.Connections = cCommon.ToDropDownList(dt.DefaultView.ToTable(), "ConType", "ConText", Session["DefaultDB"].ToString(), "ConText");

                    Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oHome.GetConnectionString(Session["DefaultDB"].ToString()));
                    Session["CONN_TYPE"] = Session["DefaultDB"];
                    foreach (DataRow row in dt.Rows)
                        Session["CONN_" + row["ConType"]] = row["ConValue"];
                }

                ViewBag.ConnType = Session["DefaultDB"];
                ViewBag.UserName = Session["FirstName"].ToString().Replace(Session["FirstName"].ToString().Substring(0, 1), Session["FirstName"].ToString().Substring(0, 1).ToUpper());
                ViewBag.IsTempKey = oAuth.IsTempKey;
                ViewBag.isAdmin = Session["isAdmin"].ToString();
                
                if (Session["isAdmin"].ToString() == "True")
                {

                    ViewBag.ddlVendor = cCommon.ToDropDownList((DataTable)Session["VendorList"], "ID", "NAME", Session["DefaultProgram"].ToString(), "ID");
                }
                else
                {
                    ViewBag.ddlVendor = cCommon.ToDropDownList((DataTable)Session["VendorList"], "ProgramId", "Program", Session["DefaultProgram"].ToString(), "ProgramId");
                }
                return View();
            }
        }
        public void LoadBuyerInfo()
        {
            cLog oLog;
            try
            {
                DataTable dt = new DataTable();
                string ConnectionType = Session["DefaultDB"].ToString();
                DataTable dtURL = cCommon.GetEmailURL(ConnectionType.ToUpper(), "BuyerInfo");

                string URL = dtURL.Rows[0]["URL"].ToString();
                var client = new RestClient(URL);
                var request = new RestRequest(dtURL.Rows[0]["PageURL"].ToString(), Method.Get);
                string userName = dtURL.Rows[0]["UserName"].ToString();
                string password = BasicEncrypt.Instance.Decrypt(dtURL.Rows[0]["Password"].ToString().Trim());

                request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password)));

                var response = client.Execute(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonstring = response.Content;
                    dt = cCommon.Tabulate(jsonstring);
                    if (dt.Rows.Count > 0)
                    {
                        cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

                        foreach (DataRow row in dt.Rows)
                        {
                            string BuyerId = row["BuyerID"].ToString();
                            string EmailAddress = row["EMailAddress"].ToString();
                            string POLimit = row["POLimit"].ToString();

                            // Check if Buyer exists
                            string checkQuery = $"SELECT EMailAddress, POLimit FROM BuyerInfo WHERE BuyerId = '{BuyerId}'";
                            DataTable existing = oDAL.GetData(checkQuery);

                            if (existing.Rows.Count > 0)
                            {
                                string existingEmail = existing.Rows[0]["EMailAddress"].ToString();
                                string existingLimit = existing.Rows[0]["POLimit"].ToString();

                                if (existingEmail != EmailAddress || existingLimit != POLimit)
                                {
                                    string updateQuery = $"UPDATE BuyerInfo SET EMailAddress = '{EmailAddress}', POLimit = '{POLimit}' WHERE BuyerId = '{BuyerId}'";
                                    oDAL.Execute(updateQuery); // Execute update directly
                                }
                            }
                            else
                            {
                                string insertQuery = $"INSERT INTO BuyerInfo (BuyerId, EMailAddress, POLimit) VALUES ('{BuyerId}', '{EmailAddress}', '{POLimit}')";
                                oDAL.Execute(insertQuery); // Execute insert directly
                            }
                        }




                        // Load Buyer Info in session
                        DataTable dtBuyerInfo = oDAL.GetData("SELECT * FROM BuyerInfo");
                        Session["dtBuyerInfo"] = dtBuyerInfo;
                    }
                }
                else
                {
                    oLog = new cLog();
                    JObject json = JObject.Parse(response.Content);
                    string errorMessage = json["ErrorMessage"]?.ToString();
                    oLog.RecordError(errorMessage, response.Content.ToString(), "Buyer Info API");
                }
            }
            catch (Exception ex)
            {
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "Buyer Info API");
            }
        }

        public void LoadBuyerInfoDemo()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = "";

            query = "select * from  [dbo].[BuyerInfo]";
            // Load Buyer Info in session
            query = "SELECT * FROM BuyerInfo";
            DataTable dtBuyerInfo = new DataTable();
            dtBuyerInfo = oDAL.GetData(query);
            Session["dtBuyerInfo"] = dtBuyerInfo;
        }

        public ActionResult Error()
        {
            //string message
            //if (System.Web.HttpContext.Current.Session["ErrorMsgMain"] != null)
            //{
            //    message = System.Web.HttpContext.Current.Session["ErrorMsgMain"].ToString();
            //}

            //ViewBag.Message = message;
            return View();
        }
        public ActionResult PageNotFound()
        {
            //Response.StatusCode = 404;
            return View("PageNotFound");
        }
        public ActionResult ChangeDfltVendor(string ProgramId, string program)
        {
            if (cCommon.IsSessionExpired())
            {
                return RedirectToAction("Login");
            }
            else
            {
                cAuth oAuth = new cAuth();
                bool isUpdated = oAuth.UpdateDfltVendor(ProgramId);
                if (isUpdated)
                {
                    Session["ProgramId"] = ProgramId;
                    Session["DefaultProgram"] = ProgramId;
                }
                return RedirectToAction("Index");
            }
        }
        //public void ChangeDBConnection(string conType)
        //{
        //    //if (cCommon.IsSessionExpired())
        //    //{
        //    //    return RedirectToAction("Login");
        //    //}
        //    //else
        //    //{
        //        oHome = new Home();
        //       // cAuth oAuth = new cAuth();
        //        Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oHome.GetConnectionString(conType));
        //        Session["CONN_TYPE"] = conType;
        //        //Session["ProgramId"] = ProgramId;
        //        //Session["Program"] = Program;

        //     //return RedirectToAction("Index");
        //    //}
        //}

        public ActionResult ChangeDBConnection(string conType)
        {
            if (cCommon.IsSessionExpired())
            {
                return RedirectToAction("Login");
            }
            else
            {
                oHome = new Home();
                cAuth oAuth = new cAuth();
                Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oHome.GetConnectionString(conType));
                //  Session["CONN_TYPE"] = conType;
                bool isUpdated = oAuth.UpdateDfltCon(conType);

                if (isUpdated)
                {
                    Session["DefaultDB"] = conType;

                }
                return RedirectToAction("Index");
            }
        }
        public JsonResult GetMenus(string ProgramId, string program)
        {
            oHome = new Home();
            string[] parts = program.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            string programName = parts[0].Trim();
            Session["ProgramName"] = programName;
            string defaultSite = parts[1].Trim();
            Session["DefaultSite"] = defaultSite; 

            String isadmin = Session["isAdmin"].ToString();
            oHome.GetMenus(Session["SigninId"].ToString(), isadmin, programName);
            return Json(oHome.webMnu, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePassword()
        {
            if (cCommon.IsSessionExpired())
                return View("Login");
            else
                return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [OutputCache(Duration = 0)]

        public ActionResult ChangePassword(ChangePassword oChangePwd)
        {

            bool isCaptcha = this.IsCaptchaValid("");
            string oldPwd = oChangePwd.oldPwd;
            string newPwd = oChangePwd.newPwd;
            string confirmPwd = oChangePwd.confirmPwd;
            string msg = string.Empty;

            if (!isCaptcha)
            {
                ViewBag.ErrMessage = "Please, enter a valid captcha. ";
                return View("ChangePassword");
            }
            if (ModelState.IsValid)
            {               // @ViewBag.newPwd = newPwd;

                //bool isValidPwd = ValidatePassword(newPwd, out msg);
                //if (isValidPwd == false)
                //{
                //    ViewBag.ErrMessage = msg;
                //    return View("ChangePassword");
                //}

                //if (string.IsNullOrEmpty(oldPwd))
                //{
                //    ViewBag.ErrMessage = "Enter old password.";
                //    return View("ChangePassword");
                //}

                //else if (string.IsNullOrEmpty(newPwd))
                //{
                //    ViewBag.ErrMessage = "Enter new password.";
                //    return View("ChangePassword");
                //}

                //else if (string.IsNullOrEmpty(confirmPwd))
                //{
                //    ViewBag.ErrMessage = "Enter confirm password.";
                //    return View("ChangePassword");
                //}

                if (oldPwd == newPwd)
                {
                    ViewBag.ErrMessage = "Old and new password cannot be same.";
                    return View("ChangePassword");
                }

                else if (newPwd != confirmPwd)
                {
                    ViewBag.ErrMessage = "Password does not match.";
                    return View("ChangePassword");
                }


                else
                {
                    oHome = new Home();
                    oHome.ChangePassword(oldPwd, newPwd, confirmPwd);
                    ViewBag.ErrMessage = oHome.Message;
                    return View("ChangePassword");
                }

            }
            else
                return View();
        }

        public JsonResult GetExternalRequest(string signId)

        {
            string Id = BasicEncrypt.Instance.Decrypt(signId.Trim());
            ChangePassword oChangePwd = new ChangePassword();
            oChangePwd.GetChangeRequest(Id);
            return Json(oChangePwd.Message, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateUser(string firstName, string lastName, string newPwd, string confirmPwd, string token)
        {

            UserRoleX oHome = new UserRoleX();
            oHome.CreateUser(firstName, lastName, newPwd, confirmPwd, token);
            return Json(oHome.Message, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ChangeExternalPassword(string newPwd, string confirmPwd, string signId)
        {
            string Id = BasicEncrypt.Instance.Decrypt(signId.Trim());
            ChangePassword oChangePwd = new ChangePassword();
            oChangePwd.ChangeExternalPassword(newPwd, confirmPwd, Id);
            return Json(oChangePwd.Message, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SendMail(string email)
        {
            cAuth oAuth = new cAuth();
            oAuth.SendMail(email);
            var jsonResult = Json(oAuth, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        private bool ValidatePassword(string password, out string ErrorMessage)
        {
            var input = password;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Password should not be empty");
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasLowerChar.IsMatch(input))
            {
                ErrorMessage = "Password should contain At least one lower case letter";
                return false;
            }
            else if (!hasUpperChar.IsMatch(input))
            {
                ErrorMessage = "Password should contain At least one upper case letter";
                return false;
            }
            else if (!hasMiniMaxChars.IsMatch(input))
            {
                ErrorMessage = "Password should not be less than or greater than 12 characters";
                return false;
            }
            else if (!hasNumber.IsMatch(input))
            {
                ErrorMessage = "Password should contain At least one numeric value";
                return false;
            }

            else if (!hasSymbols.IsMatch(input))
            {
                ErrorMessage = "Password should contain At least one special case characters";
                return false;
            }
            else
            {
                return true;
            }
        }
        [HttpPost]
        public ActionResult SaveLog(double gridExecutionTime) //Added by Huzaifa
        {
            cLog oLog = new cLog();
            Home oHome = new Home();
            string recNo = "";
            string rptUrl = "";
            DataTable dt = oHome.GetRecord();
            if (dt.Rows.Count > 0)
            {
                recNo = dt.Rows[0][0].ToString();
                rptUrl = dt.Rows[0][1].ToString();
            }

            TimeSpan duration = TimeSpan.FromSeconds(gridExecutionTime);
            string gridExecTime = $"{(int)duration.Minutes:00}:{duration.Seconds:00}";
            oLog.UpdateLog(gridExecTime, recNo, rptUrl);

            return new EmptyResult();
        }


        public JsonResult IsVerified(string tokenId, string IsVerified)
        {
            cAuth oAuth = new cAuth();
            oAuth.IsVerified(tokenId, IsVerified);
            return Json(oAuth.Message, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ForgotPassword(string Email)
        {

            return View();
        }

        public JsonResult CheckSession()
        {
            if (cCommon.IsSessionExpired()) // or any session variable you are using
            {
                return Json(new { IsSessionExpired = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { IsSessionExpired = false }, JsonRequestBehavior.AllowGet);
        }


    }
}