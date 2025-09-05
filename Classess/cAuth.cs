using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using IP.Classess;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Net.Mail;
using System.Net;

namespace PlusCP.Models
{
    public class cAuth
    {
        //cDAL oDAL;
        public string SigninId { get; set; }
        public string Password { get; set; }
        public string ProgramId { get; set; }
        public string Program { get; set; }
        public string FirstName { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string LastSignInDt { get; set; }
        public string Message { get; set; }
        public string isValidUser { get; set; }

        public string EmailMessage { get; set; }
        public string BuyerEmail { get; set; }
        public string EmailMessageType { get; set; }
        public string UsrMenu { get; set; }
        public string isAdmin { get; set; }
        public int isActiveUser { get; set; }
        public string Permission { get; set; }
        public string isCustomer { get; set; }
        public string UserType { get; set; }
        public string Captcha { get; set; }

        public string lblMsg { get; set; }
        public string DefaultProgram { get; set; }
        public string DefaultDB { get; set; }
        public string Pwd { get; set; }
        public string IsTempKey { get; set; }
        public string TimeZoneId { get; set; }

        public DataTable VendorList { get; set; }
        public DataTable Connection { get; set; }

        string Active;
        int LoginAttempt;
        //DateTime lockTime;
        string result = "";
        DataTable dtUser = new DataTable();
        object retVal;
        //string isActive = "0";
        public bool IsValidUser(string Email, string password)
        {

            //cAuth.SetConnectionString();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dtUser = new DataTable();
            string isActive = "0";

            string sql = @"select * from [SRM].[UserInfo]  Where Email = '<Email>' ";
            sql = sql.Replace("<Email>", Email);
            dtUser = oDAL.GetData(sql);
            if (dtUser.Rows.Count > 0)
            {
                string isEmailVerified = dtUser.Rows[0]["isEmailVerified"].ToString();
                isActive = dtUser.Rows[0]["isActive"].ToString();
                if(isActive == "1")
                {
                    if (isEmailVerified == "True")
                    {
                        int pendingTime = 0;
                        isActive = dtUser.Rows[0]["IsActive"].ToString();
                        if (dtUser.Rows[0]["LockTime"] != DBNull.Value)
                            pendingTime = GetPendingTime(Convert.ToDateTime(dtUser.Rows[0]["LockTime"]));

                        if (isActive == "0" && pendingTime >= 0)
                        {

                            Message = $"Your account has been locked for 15 minutes. ({pendingTime} minutes left)";
                            isValidUser = "Not";
                            return false;
                        }
                        else
                        {
                            string dbPassword = BasicEncrypt.Instance.Decrypt(dtUser.Rows[0]["Password"].ToString().Trim());
                            if (password.Trim() == dbPassword)
                            {
                                SigninId = dtUser.Rows[0]["UserId"].ToString();
                                FirstName = dtUser.Rows[0]["FirstName"].ToString();
                                LastName = dtUser.Rows[0]["LastName"].ToString();
                                EmailAddress = dtUser.Rows[0]["Email"].ToString();
                                UserType = dtUser.Rows[0]["Type"].ToString();
                                DefaultDB = dtUser.Rows[0]["DefaultDB"].ToString();
                                IsTempKey = dtUser.Rows[0]["IsTempKey"].ToString();
                                TimeZoneId = dtUser.Rows[0]["TimeZone"].ToString();
                                GetSesionValues(Email);
                                ResetUserLock(SigninId, oDAL);
                                return true;
                            }
                            else
                            {
                                result = UserLock(dtUser.Rows[0]["UserId"].ToString());
                                if (result == "")
                                {
                                    Message = "Email Address or Password is incorrect";
                                    isValidUser = "Not";
                                }
                                else
                                {
                                    Message = result;
                                    isValidUser = "Not";
                                }

                                return false;
                            }
                        }

                    }
                    else
                    {
                        Message = "Error: Your Email is not verified";
                        isValidUser = "Not";
                        return false;
                    }
                }
                else
                {
                    Message = "Error: Your Account is not active";
                    isValidUser = "Not";
                    return false;
                }
                
            }
            else
            {
                Message = "Email Address or Password is incorrect";
                isValidUser = "Not";
                return false;
            }


        }

        

        public string UserLock(string userId)
        {
            const int LockDurationMinutes = 15;
            const int MaxLoginAttempts = 3;

            string message = string.Empty;
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            try
            {
               
                string sql = "SELECT isActive, loginAttempt, LockTime FROM SRM.userInfo WHERE userId = " + userId + "";
                DataTable dtUserInfo = oDAL.GetData(sql);

                if (dtUserInfo.Rows.Count == 0)
                {
                    return "User not found.";
                }

                DataRow userRow = dtUserInfo.Rows[0];
                string isActive = userRow["isActive"].ToString();
                int loginAttempt = 0;
                if(userRow["loginAttempt"].ToString() != "" )
                {
                    loginAttempt = Convert.ToInt32(userRow["loginAttempt"]);
                }
                
                DateTime? lockTime = userRow["LockTime"] != DBNull.Value ? Convert.ToDateTime(userRow["LockTime"]) : (DateTime?)null;

                int pendingTime = GetPendingTime(lockTime);

                if (isActive != "1")
                {
                    if (pendingTime <= 0)
                    {
                        if (loginAttempt >= MaxLoginAttempts - 1)
                        {
                            ResetUserLock(userId, oDAL);
                        }
                        else
                        {
                            UpdateLoginAttempt(userId, loginAttempt + 1, oDAL);
                        }
                    }
                    else
                    {
                        message = $"Your account has been locked for {LockDurationMinutes} minutes. ({pendingTime} minutes left)";
                    }
                }
                else
                {
                    if (loginAttempt < MaxLoginAttempts)
                    {
                        UpdateLoginAttempt(userId, loginAttempt + 1, oDAL);

                        if (loginAttempt + 1 >= MaxLoginAttempts)
                        {
                            LockUserAccount(userId, oDAL);
                            message = $"Your account has been locked for {LockDurationMinutes} minutes.";
                        }
                    }
                    else
                    {
                        message = $"Your account has been locked for {LockDurationMinutes} minutes.";
                    }
                }
            }
            catch (Exception)
            {
                // Log the exception as needed
                message = "An error occurred while processing your request.";
            }

            return message;
        }

        private void ResetUserLock(string userId, cDAL oDAL)
        {
            string sql = "UPDATE SRM.userInfo SET loginAttempt = 0, LockTime = NULL, isActive = '1' WHERE userId = " + userId + "";
            oDAL.Execute(sql);
        }

        private void UpdateLoginAttempt(string userId, int loginAttempt, cDAL oDAL)
        {
            string sql = "UPDATE SRM.userInfo SET loginAttempt = " + loginAttempt + " WHERE userId = " + userId + "";
            oDAL.Execute(sql);
        }

        private void LockUserAccount(string userId, cDAL oDAL)
        {
            string sql = "UPDATE SRM.userInfo SET isActive = '0', LockTime = '" + DateTime.Now + "' WHERE userId = " + userId + "";
            oDAL.Execute(sql);
        }

        public void GetSesionValues(string Email)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"select * from [SRM].[UserInfo]  Where Email = '<Email>' ";
            sql = sql.Replace("<Email>", Email);
            dtUser = oDAL.GetData(sql);

            SigninId = dtUser.Rows[0]["UserId"].ToString();
            FirstName = dtUser.Rows[0]["FirstName"].ToString();
            LastName = dtUser.Rows[0]["LastName"].ToString();
            UserName = dtUser.Rows[0]["FirstName"].ToString() + ' ' + dtUser.Rows[0]["LastName"].ToString();

            string DefaultDtlquery = "SELECT IsAdmin, DefaultProgram, DefaultDB FROM SRM.UserInfo WHERE Email = '" + Email + "'";
            DataTable dtDefaultDtl = oDAL.GetData(DefaultDtlquery);
            if (dtDefaultDtl.Rows.Count > 0)
            {
                isAdmin = dtDefaultDtl.Rows[0]["isAdmin"].ToString();
                DefaultProgram = dtDefaultDtl.Rows[0]["DefaultProgram"].ToString();
                DefaultDB = dtDefaultDtl.Rows[0]["DefaultDB"].ToString();
            }

            //sql = "EXEC [utl].[spUserLastLogOn] " + userId;
            //oDAL.Execute(sql);

            string query = @"SELECT isAdmin from [SRM].[UserInfo] where USERID = '" + SigninId + "'";
            query += "order by isAdmin desc";
            string query1 = @"SELECT IsCustomer from [SRM].[UserInfo] where USERID = '" + SigninId + "'";
            //query1 += "order by IsCustomer desc";
            DataTable dtIsAdmin = new DataTable();
            DataTable dtIsCustomer = new DataTable();
            dtIsAdmin = oDAL.GetData(query);
            dtIsCustomer = oDAL.GetData(query1);
            if (dtIsAdmin.Rows.Count > 0)
                isAdmin = dtIsAdmin.Rows[0]["isAdmin"].ToString();
            if (dtIsCustomer.Rows.Count > 0)
                isCustomer = dtIsCustomer.Rows[0]["IsCustomer"].ToString();

            if (isAdmin == "True")
            {

                oDAL = new cDAL(cDAL.ConnectionType.INIT);
                string vendorQuery = "SELECT ID, (Name + ' - ' + Site) AS NAME FROM SRM.Program ORDER BY Name";
                DataTable dtVendor = oDAL.GetData(vendorQuery);
                if (dtVendor.Rows.Count > 0)
                {
                    ProgramId = dtVendor.Rows[0]["ID"].ToString();
                    Program = dtVendor.Rows[0]["Name"].ToString();

                    VendorList = new DataTable();
                    VendorList = dtVendor.DefaultView.ToTable(true, "ID", "Name").Copy();
                }

            }
            else
            {

                string vendorQuery = "SELECT ProgramId, Program FROM [SRM].[UserProgramX] WHERE USERID =" + SigninId;
                DataTable dtVendor = oDAL.GetData(vendorQuery);
                if (dtVendor.Rows.Count > 0)
                {
                    ProgramId = dtVendor.Rows[0]["ProgramId"].ToString();
                    Program = dtVendor.Rows[0]["Program"].ToString();

                    VendorList = new DataTable();
                    VendorList = dtVendor.DefaultView.ToTable(true, "ProgramId", "Program").Copy();
                }

            }
        }


        //public bool IsValidUser(string username, string password)
        //{

        //    cAuth.SetConnectionString();
        //    cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
        //    DataTable dtUser = new DataTable();


        //    string sql = @"select PrimaryUse from [SRM].[UserInfo]  Where PrimaryUse = '<username>' ";
        //    sql = sql.Replace("<username>", username);
        //    dtUser = oDAL.GetData(sql);
        //    if(dtUser.Rows.Count == 0)
        //    {
        //        Message = "Error: Username is not valid";
        //        return false;
        //    }

        //    string UsrId = "";

        //     sql = @"select UserId, PrimaryUse, Password , Type from [SRM].[UserInfo]  Where PrimaryUse = '<username>' AND Password = '<password>' ";
        //    sql = sql.Replace("<username>", username);
        //    sql = sql.Replace("<password>", password);
        //    dtUser = oDAL.GetData(sql);
        //    if(dtUser.Rows.Count == 0)
        //    {
        //        result = "Error: password is incorrect";
        //    }
        //    else
        //    {
        //        result = "OK";
        //    }
        //    UserType = dtUser.Rows[0]["Type"].ToString();

        //    sql = "SELECT UserId, isActive FROM [SRM].[UserInfo] WHERE PrimaryUse  ='" + username + "' ";
        //    DataTable dtUsr = oDAL.GetData(sql);
        //    if (dtUsr.Rows.Count > 0)
        //        UsrId = dtUsr.Rows[0]["UserId"].ToString();

        //    sql = "SELECT isActive, loginAttempt, LockTime FROM SRM.userInfo WHERE userId = '" + UsrId + "' ";

        //    DataTable dtUserInfo = oDAL.GetData(sql);
        //    if(dtUserInfo.Rows.Count > 0)
        //    {
        //        Active = dtUserInfo.Rows[0]["isActive"].ToString();
        //        LoginAttempt = Convert.ToInt32(dtUserInfo.Rows[0]["loginAttempt"]);
        //        if (dtUserInfo.Rows[0]["LockTime"].ToString() != "")
        //        {
        //            lockTime = Convert.ToDateTime(dtUserInfo.Rows[0]["LockTime"]);
        //        }

        //    }

        //    int pendingTime = GetPendingTime(lockTime);

        //    if (result.Contains("password"))
        //    {
        //        if (Active == "N")
        //        {

        //            if (pendingTime <= 0)
        //            {
        //                sql = "UPDATE SRM.UserInfo SET LoginAttempt = 0, LockTime = NULL, isActive = 'Y' Where UserId =" + UsrId;
        //                oDAL.Execute(sql);
        //                GetUerDtl(UsrId);
        //                if (LoginAttempt <= 3)
        //                {
        //                    if (dtUserInfo.Rows.Count > 0)
        //                    {
        //                        LoginAttempt = LoginAttempt + 1;
        //                        sql = "UPDATE SRM.UserInfo SET LoginAttempt = '" + LoginAttempt + "' Where UserId =" + UsrId;
        //                        oDAL.Execute(sql);
        //                        if (LoginAttempt >= 3)
        //                        {
        //                            sql = "UPDATE SRM.UserInfo SET isActive = 'N', LockTime = '" + DateTime.Now.ToString() + "' WHERE UserId =" + UsrId;
        //                            oDAL.Execute(sql);
        //                        }
        //                    }
        //                }
        //            }

        //            else
        //            {
        //                Message = "Your account has been locked for 15 minutes. " + " (" + pendingTime + " minutes left" + ") ";
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            if (LoginAttempt <= 3)
        //            {
        //                if (dtUserInfo.Rows.Count > 0)
        //                {

        //                    LoginAttempt = LoginAttempt + 1;
        //                    sql = "UPDATE SRM.UserInfo SET LoginAttempt = '" + LoginAttempt + "' Where UserId =" + UsrId;
        //                    oDAL.Execute(sql);
        //                    if (LoginAttempt >= 3)
        //                    {
        //                        sql = "UPDATE SRM.UserInfo SET isActive = 'N', LockTime = '" + DateTime.Now.ToString() + "' WHERE UserId =" + UsrId;
        //                        oDAL.Execute(sql);
        //                        Message = "Your account has been locked for 15 minutes.";
        //                        return false;
        //                    }
        //                }
        //            }
        //        }
        //        Message = result;
        //        return false;
        //    }
        //    // After 3 Attempt and time is over
        //    if (pendingTime <= 0)
        //    {
        //        if (result.Contains("OK"))
        //        {
        //            var userId = Regex.Match(result, @"\d+").Value;
        //            if (LoginAttempt <= 3)
        //            {
        //                sql = "UPDATE SRM.UserInfo SET LoginAttempt = 0, LockTime = NULL, isActive = 'Y' Where UserId =" + UsrId;
        //                oDAL.Execute(sql);
        //            }
        //            sql = "SELECT FirstName, LastName FROM [SRM].[UserInfo]  WHERE UserId = '" + UsrId + "'";
        //             dtUser = oDAL.GetData(sql);
        //            if (dtUser.Rows.Count > 0)
        //            {
        //                //Updating User Password
        //                sql = @"UPDATE SRM.UserInfo SET Password = '@Password' WHERE UserId = '" + UsrId + "' ";
        //                sql = sql.Replace("@Password", password);
        //                oDAL.Execute(sql);

        //                SigninId = UsrId;
        //                FirstName = dtUser.Rows[0]["FirstName"].ToString();
        //                LastName = dtUser.Rows[0]["LastName"].ToString();
        //                UserName = username;
        //                string DefaultDtlquery = "SELECT IsAdmin, DefaultProgram, DefaultDB FROM SRM.UserInfo WHERE UserId = '" + UsrId + "'";
        //                DataTable dtDefaultDtl = oDAL.GetData(DefaultDtlquery);
        //                if (dtDefaultDtl.Rows.Count > 0)
        //                {
        //                    isAdmin = dtDefaultDtl.Rows[0]["isAdmin"].ToString();
        //                    DefaultProgram = dtDefaultDtl.Rows[0]["DefaultProgram"].ToString();
        //                    DefaultDB = dtDefaultDtl.Rows[0]["DefaultDB"].ToString();
        //                }

        //                //sql = "EXEC [utl].[spUserLastLogOn] " + userId;
        //                //oDAL.Execute(sql);

        //                string query = @"SELECT isAdmin from [SRM].[UserInfo] where USERID = '" + SigninId + "'";
        //                query += "order by isAdmin desc";
        //                string query1 = @"SELECT IsCustomer from [SRM].[UserInfo] where USERID = '" + SigninId + "'";
        //                //query1 += "order by IsCustomer desc";
        //                DataTable dtIsAdmin = new DataTable();
        //                DataTable dtIsCustomer = new DataTable();
        //                dtIsAdmin = oDAL.GetData(query);
        //                dtIsCustomer = oDAL.GetData(query1);
        //                if (dtIsAdmin.Rows.Count > 0)
        //                    isAdmin = dtIsAdmin.Rows[0]["isAdmin"].ToString();
        //                if (dtIsCustomer.Rows.Count > 0)
        //                    isCustomer = dtIsCustomer.Rows[0]["IsCustomer"].ToString();
        //            }
        //            if (isAdmin == "True")
        //            {

        //                oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
        //                string vendorQuery = "SELECT ID, (Name + ' - ' + Site) AS NAME FROM SRM.Program ORDER BY Name";
        //                DataTable dtVendor = oDAL.GetData(vendorQuery);
        //                if (dtVendor.Rows.Count > 0)
        //                {
        //                    ProgramId = dtVendor.Rows[0]["ID"].ToString();
        //                    Program = dtVendor.Rows[0]["Name"].ToString();

        //                    VendorList = new DataTable();
        //                    VendorList = dtVendor.DefaultView.ToTable(true, "ID", "Name").Copy();
        //                }

        //            }
        //            else
        //            {


        //                string ProgramCount = "SELECT count(UserId) FROM [SRM].[UserProgramX] WHERE USERID =" + userId;
        //                var dtVendor1 = oDAL.GetObject(ProgramCount);
        //                string MenuCount = "SELECT count(UserId) FROM [SRM].[UserMnuX] WHERE USERID = cast('" + userId + "' as varchar)";
        //                var dtVendor2 = oDAL.GetObject(MenuCount);
        //                if (dtVendor1.Equals(0) || dtVendor2.Equals(0))
        //                {
        //                    Permission = "0";
        //                }
        //                else
        //                {
        //                    string vendorQuery = "SELECT ProgramId, Program FROM [SRM].[UserProgramX] WHERE USERID =" + userId;
        //                    DataTable dtVendor = oDAL.GetData(vendorQuery);
        //                    if (dtVendor.Rows.Count > 0)
        //                    {
        //                        ProgramId = dtVendor.Rows[0]["ProgramId"].ToString();
        //                        Program = dtVendor.Rows[0]["Program"].ToString();

        //                        VendorList = new DataTable();
        //                        VendorList = dtVendor.DefaultView.ToTable(true, "ProgramId", "Program").Copy();
        //                    }
        //                }
        //            }
        //            return true;
        //        }
        //    }


        //    else
        //    {
        //        Message = "Your account has been locked for 15 minutes. " + " (" + pendingTime + " minutes left" + ") ";
        //        return false;
        //    }


        //    Message = result;
        //    return false;
        //}
        private int GetPendingTime(DateTime? lockTime)
        {
            if (lockTime == null)
            {
                return 0;
            }

            TimeSpan elapsed = DateTime.Now - lockTime.Value;
            return Math.Max(0, (int)(15 - elapsed.TotalMinutes));
        }
        public int GetPendingTime(DateTime lockTimes)
        {
            DateTime cdatetime = Convert.ToDateTime(DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"));
            TimeSpan ts = cdatetime.Subtract(lockTimes);
            Int32 minuteslocked = Convert.ToInt32(ts.TotalMinutes);
            Int32 pendingminutes = 15 - minuteslocked;
            return pendingminutes;
        }
        public void GetUerDtl(string userId)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = "SELECT IsActive, loginAttempt, LockTime FROM SRM.userInfo WHERE userId = '" + userId + "' ";
            DataTable dtUserInfo = oDAL.GetData(sql);
            Active = dtUserInfo.Rows[0]["IsActive"].ToString();
            LoginAttempt = Convert.ToInt32(dtUserInfo.Rows[0]["loginAttempt"]);

        }
        //public bool ChkIsCust(string userId)
        //{
        //    cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
        //    string sql = "SELECT IsCustomer FROM EP.userInfo WHERE userId = '" + userId + "' ";
        //    DataTable dtUserInfo = oDAL.GetData(sql);
        //    isCustomer = dtUserInfo.Rows[0]["IsCustomer"].ToString();

        //    //LoginAttempt = Convert.ToInt32(dtUserInfo.Rows[0]["loginAttempt"]);
        //    return true;
        //}

        public static void SetConnectionString()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dt = oDAL.GetData("SELECT * FROM SRM.zConStr");
            if (dt.Rows.Count > 0)
            {
                //dt.DefaultView.RowFilter = "ConType = 'TEST'";
                HttpContext.Current.Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(dt.DefaultView.ToTable().Rows[0]["ConValue"].ToString());

            }
        }

        public bool UpdateDfltVendor(string ProgramId)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"UPDATE EP.UserInfo
                           SET DefaultProgram = '" + ProgramId + "' " +
                           "WHERE UserId = '" + HttpContext.Current.Session["SigninId"] + "'";
            oDAL.Execute(sql);
            if (oDAL.HasErrors)
                return false;
            return true;
        }

        public bool UpdateDfltCon(string conType)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"UPDATE SRM.UserInfo
                           SET DefaultDB = '" + conType + "' " +
                           "WHERE UserId = '" + HttpContext.Current.Session["SigninId"] + "'";
            oDAL.Execute(sql);
            lblMsg = "Updated";
            if (oDAL.HasErrors)
                return false;
            return true;
        }

        public bool UpdateTimeZone(string TimeZone)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"UPDATE SRM.UserInfo
                           SET TimeZone = '" + TimeZone + "' ";
            oDAL.Execute(sql);
            lblMsg = "Updated";
            if (oDAL.HasErrors)
                return false;
            return true;
        }

        public bool UpdateCCEmail(string CCEmail)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"UPDATE EmailSetup
                           SET [CCEmailAddress] = '" + CCEmail + "' ";
            oDAL.Execute(sql);
            lblMsg = "Updated";
            if (oDAL.HasErrors)
                return false;
            return true;
        }

        public bool SendMail(string SignInId) // string ProgramId, Commented
        {
            string DeployMode = WebConfigurationManager.AppSettings["DEPLOYMODE"];
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            cAuth oAuth = new cAuth();
            string URL = "";
            string sql = @"SELECT * FROM [SRM].[UserInfo] WHERE Email = '" + SignInId + "' ";

            DataTable dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                string Id = dt.Rows[0]["UserId"].ToString();
                string encrypId = BasicEncrypt.Instance.Encrypt(Id.Trim());
                string urlEncodedId = HttpUtility.UrlEncode(encrypId);
                string Email = dt.Rows[0]["Email"].ToString();

                string requestedOn = DateTime.Now.ToString();
                string urlExpTime = "24";
                double urlexptime = Convert.ToDouble(urlExpTime);
                object expiresOn = Convert.ToDateTime(requestedOn).AddHours(urlexptime);
                DataTable dtURL = new DataTable();
                dtURL = cCommon.GetEmailURL(DeployMode, "ForgotPassword");

                if (DeployMode == "PROD")
                {
                    URL = dtURL.Rows[0]["URL"].ToString() + "" + dtURL.Rows[0]["PageURL"].ToString() + urlEncodedId;

                }
                else
                {
                    URL = dtURL.Rows[0]["URL"].ToString() + "" + dtURL.Rows[0]["PageURL"].ToString() + urlEncodedId;
                }


                string htmlbody = ForgotEmailBody(URL, Email);


                oDAL = new cDAL(cDAL.ConnectionType.INIT);
                sql = @"INSERT INTO SRM.ExtUrls(RptUrl, SigninId, RequestOn, ExpiresOn) 
                        VALUES('" + URL + "', '" + Id + "', '" + requestedOn + "', '" + expiresOn + "')";
                oDAL.Execute(sql);

                cCommon.SendEmail(SignInId, "Reset Password Request", htmlbody, "", null);
               
            }
            else
            {
                EmailMessage = "Email is not valid";
                return false;
            }
            EmailMessage = "sent";
            return true;

        }

        public string ForgotEmailBody(string URL, string Email)
        {
            string htmlBody = @"";
            DataTable dt = new DataTable();
            string query = "";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            query = "Select SYSValue from dbo.zSysIni WHERE SysDesc = 'FORGOTPASSWORD' ";
            dt = oDAL.GetData(query);
            htmlBody = dt.Rows[0]["SYSValue"].ToString();

            htmlBody = htmlBody.Replace("<ForgotURL>", URL);
            htmlBody = htmlBody.Replace("<Email>", Email);

            return htmlBody;
        }
        public void SendEmailVerification(string FirsthName, string LastName, string senderEmail, string EmailAddress, string subject, string htmlBody)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = @"INSERT INTO [dbo].[UserEmail]
           ([Sender]
           ,[Receiver]
           ,[Subject]
           ,[Body])
     VALUES
           ('<Sender>'
           ,'<Receiver>'
           ,'<Subject>'
           ,'<Body>')";

            sql = sql.Replace("<Sender>", senderEmail);
            sql = sql.Replace("<Receiver>", EmailAddress);
            sql = sql.Replace("<Subject>", subject);
            sql = sql.Replace("<Body>", htmlBody);


            oDAL.Execute(sql);
        }

        public void UserVerified(string tokenId, string FirstName, string LastName, string EmailAddress, string Pwd, string Type, string BuyerEmail)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = "";
            Password = BasicEncrypt.Instance.Encrypt(Pwd.Trim());
            if (Type == "Supplier")
            {
                sql = @"INSERT INTO [SRM].[UserInfo]
    ([FirstName], [LastName], [Password], [isEmailVerified], [Email], [CreatedOn],[Type],[tokenId],[BuyerEmail], [DefaultProgram])
    VALUES
    ('<FirstName>', '<LastName>', '<Pwd>', '0', '<Email>', GETDATE(), '<Type>', '" + tokenId + "', '<BuyerEmail>', '<DefaultProgram>')";
            }
            else
            {
                sql = @"INSERT INTO [SRM].[UserInfo]
    ([FirstName], [LastName], [Password], [isEmailVerified], [Email], [CreatedOn],[Type],[tokenId], [DefaultProgram])
    VALUES
    ('<FirstName>', '<LastName>', '<Pwd>', '0', '<Email>', GETDATE(), '<Type>', '" + tokenId + "', '<DefaultProgram>')";
            }



            sql = sql.Replace("<FirstName>", FirstName);
            sql = sql.Replace("<LastName>", LastName);
            sql = sql.Replace("<Pwd>", Password);
            sql = sql.Replace("<Email>", EmailAddress);
            sql = sql.Replace("<Type>", Type);
            sql = sql.Replace("<BuyerEmail>", BuyerEmail);
            sql = sql.Replace("<DefaultProgram>", "10000-Newyork");

            oDAL.Execute(sql);

            sql = "Select UserId From [SRM].[UserInfo] Where Email = '" + EmailAddress + "'";
            retVal = oDAL.GetObject(sql);

            sql = @"INSERT INTO [SRM].[UserProgramX]
           ([UserId]
           ,[ProgramId]
           ,[Program]
           ,[CreatedBy]
)
     VALUES
           (
		   '<UserId>'
           ,'<ProgramId>'
           ,'<Program>'
           ,'<CreatedBy>'
		   ) ";

            sql = sql.Replace("<UserId>", retVal.ToString());
            sql = sql.Replace("<ProgramId>", "10000");
            sql = sql.Replace("<Program>", "SRM-Newyork");
            sql = sql.Replace("<CreatedBy>", FirstName);

            oDAL.Execute(sql);
        }

        public void IsVerified(string tokenId, string IsVerified)
        {

            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);

            string sql = @"UPDATE SRM.UserInfo SET isEmailVerified = 1 where tokenId = '" + tokenId + "'";

            oDAL.Execute(sql);
            if (!oDAL.HasErrors)
            {
                DataTable dt = new DataTable();
                sql = "Select Email from SRM.UserInfo where tokenId = '" + tokenId + "' ";
                dt = oDAL.GetData(sql);
                if (dt.Rows.Count > 0)
                {
                    SendConfirmationEmail(dt.Rows[0]["Email"].ToString());
                }

            }
        }
        public string SendConfirmationEmail(string EmailAddress)
        {
            cAuth oAuth = new cAuth();
            string result = "";
            string DeployMode = WebConfigurationManager.AppSettings["DEPLOYMODE"];
            //URL
            DataTable dtURL = new DataTable();
            dtURL = cCommon.GetEmailURL(DeployMode, "Login");
            string Url = dtURL.Rows[0]["URL"].ToString();
            string Baseurl = Url + dtURL.Rows[0]["PageURL"].ToString();


            var subject = "Collablly Account Invitation";
            // Recipient's email address
            string recipientEmail = EmailAddress;
  
            string htmlBody = "";
            // HTML body containing the form
            htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'>
    <title>Email Verification</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f8f9fa;
            padding: 20px;
        }}
        .card {{
            margin: 20px 0;
            max-width: 600px;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
            border: 1px solid #e3e6f0;
            border-radius: 0.35rem;
            background-color: #fff;
            text-align: left;
        }}
        .card-header {{
            background-color: #325FAB;
            color: #fff;
            padding: 1rem 1.25rem;
            border-bottom: 1px solid #e3e6f0;
            border-top-left-radius: 0.35rem;
            border-top-right-radius: 0.35rem;
        }}
        .card-body {{
            padding: 1.25rem;
        }}
        .btn-primary {{
            display: inline-block;
            color: #fff !important;
            background-color: #325FAB;
            border-color: #325FAB;
            padding: 0.5rem 1rem;
            text-decoration: none;
            border-radius: 0.35rem;
        }}
        .btn-primary:hover {{
            background-color: #2a4b8d;
            border-color: #2a4b8d;
        }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='card-header'>
            <h2>Buyer Email Verification</h2>
        </div>
        <div class='card-body'>
            <p>Dear Buyer,</p>
            <p>Your email has been verified now you can login the portal.:</p>
            <p>
                <a href='{Baseurl}' class='btn-primary' target='_blank'>Click here to login</a>
            </p>
            <p>Thank you.</p>
        </div>
    </div>
</body>
</html>";
            DataTable dt = new DataTable();
            string query = "";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            query = "Select SYSValue from dbo.zSysIni WHERE SysDesc = 'EMAILVERIFICATION'";
            dt = oDAL.GetData(query);
            htmlBody = dt.Rows[0]["SYSValue"].ToString();

            try
            {
                result = cCommon.SendEmail(recipientEmail, subject, htmlBody, "", null);
                
            }
            catch (Exception)
            {

            }
            return oAuth.EmailMessage = "Sent";


        }

        public DataTable GetEmailSMTPSetup()
        {
            string sql = "Select * from [dbo].[EmailSetup]";
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql);
            return dt;
        }
    }
}