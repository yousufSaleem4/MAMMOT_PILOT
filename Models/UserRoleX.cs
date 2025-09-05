using IP.Classess;
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
    public class UserRoleX
    {
        cDAL oDAL;
        #region Fields
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Your password must be at least 1 number, special character, upper case, lower case ")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "LastName")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string Id { get; set; }
        [Display(Name = "Id:")]
        public string Name { get; set; }
        public string Message { get; set; }

        public string MnuId { get; set; }
        [Display(Name = "Menu Id:")]

        public string MnuTitle { get; set; }

        [Display(Name = "Primary use:")]
        public string primaryUse { get; set; }

        public string isAdmin { get; set; }
        public bool isActiveUser { get; set; }
        public string isCustomer { get; set; }
        //int vendorCounter = 0;

        public string lblMsg { get; set; }

        public List<Hashtable> lstUser { get; set; }
        public List<object> lstMst = new List<object>();
        public List<ArrayList> lstMnu { get; set; }
        public List<ArrayList> lstProgram { get; set; }
        public List<ArrayList> lstProgramById { get; set; }
        public List<ArrayList> FinallstProgram { get; set; }
        public List<ArrayList> lstMnuById { get; set; }
        public List<ArrayList> FinallstMnu { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorMessageUpdate { get; set; }

        #endregion

        public bool GetList(string IscreateUser, string type)
        {
            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string query = string.Empty;
            query = @"SELECT '' AS Edit
                            ,UserId
                            ,FirstName
                            ,LastName                
                            ,EMAIL
                            ,CASE WHEN IsACTIVE = 1 THEN 'Y' ELSE 'N' END ACTIVE
                            ,CASE WHEN IsAdmin = 1 THEN 'Y' ELSE 'N' END ADMIN
                            ,FORMAT(CreatedOn, 'yyyy.MM.dd') AS CreatedOn
                            ,Password
                      FROM [SRM].[UserInfo] usr ";

            if (!string.IsNullOrEmpty(type) && type != "-1")
            {
                query += "WHERE usr.ACTIVE ='@type'";
                //query = query.Replace("@type", type.ToString());
            }
            query = query.Replace("@type", type.ToString());

            if (IscreateUser == "Y")
            {
                query += "ORDER BY usr.ID DESC";
            }

            else
            {
                query += "ORDER BY IsCustomer DESC ,usr.FirstName ";
            }

            DataTable dt = oDAL.GetData(query);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstUser = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }

        public bool GetProgram()
        {
            oDAL = new cDAL(cDAL.ConnectionType.INIT);

            try
            {
                string sql = "SELECT ID, (Name + ' - ' + Site) AS NAME FROM pls.Program ORDER BY Name ";
                DataTable dtProgram = new DataTable();
                oDAL = new cDAL(cDAL.ConnectionType.INIT);
                dtProgram = oDAL.GetData(sql);
                lstProgram = cCommon.ConvertDtToArrayList(dtProgram);
                if (!oDAL.HasErrors)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool GetMnu()
        {
            oDAL = new cDAL(cDAL.ConnectionType.INIT);

            try
            {
                string sql = @"SELECT mnuid, mnutitle FROM [EP].[Mnu]
WHERE MnuActive = 1 AND MnuIsReady = 1 AND MNUPARENT IS NOT NULL
ORDER BY MnuTitle ";
                DataTable dtMnu = new DataTable();
                oDAL = new cDAL(cDAL.ConnectionType.INIT);
                dtMnu = oDAL.GetData(sql);
                lstMnu = cCommon.ConvertDtToArrayList(dtMnu);
                if (!oDAL.HasErrors)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool GetDataById(string Userid)
        {
            oDAL = new cDAL(cDAL.ConnectionType.INIT);

            cDAL oDAL1 = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dtActive = new DataTable();
            DataTable dtProgram = new DataTable();
            DataTable dtProgram1 = new DataTable();
            DataTable dtIsAdmin = new DataTable();
            DataTable dtMnu = new DataTable();
            DataTable dtMnu1 = new DataTable();
            //Program
            string isActiveUserQuery = @"SELECT FirstName,LastName,isActive, Password FROM [SRM].[UserInfo] 
WHERE UserId = '" + Userid + "' ";

            //IsAdmin
            string isAdminQuery = @"SELECT isAdmin FROM [SRM].[UserInfo]  
WHERE UserId = '" + Userid + "' ";
            isAdminQuery += "order by isAdmin desc";




            dtActive = oDAL1.GetData(isActiveUserQuery);

            dtIsAdmin = oDAL.GetData(isAdminQuery);
            if (dtIsAdmin.Rows.Count > 0)
            {
                isAdmin = dtIsAdmin.Rows[0]["isAdmin"].ToString();

            }
            isActiveUser = (Convert.ToInt32(dtActive.Rows[0]["isActive"]) == 1) ? true : false;
            FirstName = dtActive.Rows[0]["FirstName"].ToString();
            LastName = dtActive.Rows[0]["LastName"].ToString();
            Password = BasicEncrypt.Instance.Decrypt(dtActive.Rows[0]["Password"].ToString().Trim());


            return true;

        }

        public bool SaveAxs(string userId, string isAdmin, bool isActiveUserchange, string password)
        {
            ErrorMessageUpdate = "";
            #region UpdateUserRoleX_Get_current_Session           
            string Default_Program = HttpContext.Current.Session["DefaultProgram"].ToString();
            string Default_DB = HttpContext.Current.Session["DefaultDB"].ToString();
            string ModifiedBy = HttpContext.Current.Session["FirstName"].ToString();
            string UserPassword = BasicEncrypt.Instance.Encrypt(password.Trim());
            #endregion

            string Type = "";
            string isActive = "";
            string isTempKey = "0";
            if (isAdmin.ToUpper() == "TRUE")
            {
                isAdmin = "1";
                Type = "Admin";
            }
            else
            {
                isAdmin = "0";
                Type = "Buyer";
            }

            if (isActiveUserchange)
                isActive = "1";
            else
                isActive = "0";

            if (!string.IsNullOrEmpty(password))
            {
                isTempKey = "1";
            }
            //check if User Active needs a change
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);



            DataTable dtCurrentUser = new DataTable();

            #region UpdateUserRoleX
            string SearchUserID = @"select * from [SRM].[UserInfo]  WHERE USERID = '" + userId + "'";
            dtCurrentUser = oDAL.GetData(SearchUserID);

            if (dtCurrentUser.Rows.Count > 0)
            {


                string updateUser = @"  UPDATE [SRM].[UserInfo]
                                            SET     isAdmin='<is_Admin>',                                                   
                                                    ModifiedBy='<Modified_By>',
                                                    ModifiedOn=GETDATE(),
                                                    isActive = '<is_Active>',
                                                    Type = '<Type>',
                                                    --Password = '<Password>',
                                                    IsTempKey = '<TempKey>'
                                            where   UserId='<UserId>'";

                updateUser = updateUser.Replace("<is_Admin>", isAdmin);
                updateUser = updateUser.Replace("<Modified_By>", ModifiedBy);
                updateUser = updateUser.Replace("<is_Active>", isActive);
                updateUser = updateUser.Replace("<Type>", Type);
                updateUser = updateUser.Replace("<UserId>", userId);
                //updateUser = updateUser.Replace("<Password>", UserPassword);
                updateUser = updateUser.Replace("<TempKey>", isTempKey);
                oDAL.Execute(updateUser);
            }

            #endregion





            if (oDAL.HasErrors)
            {
                ErrorMessageUpdate = oDAL.InternalErrMsg;
                //string sqlError = "Update denied";
            }
            if (!oDAL.HasErrors)
            {
                lblMsg = "Y";
                return true;
            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }

        }


        public bool SaveTempKey(string userId, string password)
        {
            ErrorMessageUpdate = "";
            #region UpdateUserRoleX_Get_current_Session           
            string Default_Program = HttpContext.Current.Session["DefaultProgram"].ToString();
            string Default_DB = HttpContext.Current.Session["DefaultDB"].ToString();
            string ModifiedBy = HttpContext.Current.Session["FirstName"].ToString();
            string UserPassword = BasicEncrypt.Instance.Encrypt(password.Trim());
            #endregion

            //string Type = "";
            //string isActive = "";
            string isTempKey = "0";


            if (!string.IsNullOrEmpty(password))
            {
                isTempKey = "1";
            }


            //check if User Active needs a change
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);



            DataTable dtCurrentUser = new DataTable();

            #region UpdateUserRoleX
            string SearchUserID = @"select * from [SRM].[UserInfo]  WHERE USERID = '" + userId + "'";
            dtCurrentUser = oDAL.GetData(SearchUserID);

            if (dtCurrentUser.Rows.Count > 0)
            {


                string updateUser = @"  UPDATE [SRM].[UserInfo]
                                            SET     Password = '<Password>',
                                                    IsTempKey = '<TempKey>'
                                            where   UserId='<UserId>'";
                updateUser = updateUser.Replace("<UserId>", userId);
                updateUser = updateUser.Replace("<Password>", UserPassword);
                updateUser = updateUser.Replace("<TempKey>", isTempKey);
                oDAL.Execute(updateUser);
            }

            #endregion





            if (oDAL.HasErrors)
            {
                ErrorMessageUpdate = oDAL.InternalErrMsg;
                //string sqlError = "Update denied";
            }
            if (!oDAL.HasErrors)
            {
                lblMsg = "Y";
                return true;
            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }

        }


        public bool GetRoleButton()
        {


            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string signinId = HttpContext.Current.Session["SigninId"].ToString();

            string query = @"SELECT isAdmin from [EP].[UserInfo] where USERID = '" + signinId + "'";
            DataTable dtIsAdmin = new DataTable();
            dtIsAdmin = oDAL.GetData(query);
            if (dtIsAdmin.Rows.Count > 0)
                isAdmin = dtIsAdmin.Rows[0]["isAdmin"].ToString();

            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }

        public bool checkData(string Userid, string program, string isAdmin, string mnu, string isCustomer)
        {
            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dtProgram = new DataTable();
            DataTable dtIsAdmin = new DataTable();
            DataTable dtMnu = new DataTable();
            string programQuery = @"SELECT ProgramId FROM [PlusRS].[EP].[UserProgramX] 
WHERE UserId = '" + Userid + "' ";

            //IsAdmin
            string isAdminQuery = @"SELECT isAdmin, isCustomer FROM [PlusRS].[EP].UserInfo 
WHERE UserId = '" + Userid + "' ";

            //Menu
            string MenuQuery = @"SELECT MNUID FROM [PlusRS].[EP].[UserMnuX] 
WHERE USERID = '" + Userid + "' ORDER BY MNUID DESC";

            dtProgram = oDAL.GetData(programQuery);
            dtIsAdmin = oDAL.GetData(isAdminQuery);
            dtMnu = oDAL.GetData(MenuQuery);

            string pvsProgram = convertDataTableToString(dtProgram);
            string pvsMnu = convertDataTableToString(dtMnu);

            string pvsIsAdmin;
            string pvsIsCustomer;
            if (dtIsAdmin.Rows.Count > 0)
            {
                pvsIsAdmin = dtIsAdmin.Rows[0]["IsAdmin"].ToString();
                pvsIsCustomer = dtIsAdmin.Rows[0]["IsCustomer"].ToString();
            }
            else
            {
                pvsIsAdmin = "false";
                pvsIsCustomer = "false";

            }
            //if (pvsIsAdmin == "True")
            //    pvsIsAdmin = "true";
            //else if(pvsIsAdmin == "False")
            //    pvsIsAdmin = "false";

            if (pvsProgram == program && pvsIsAdmin == isAdmin && pvsMnu == mnu && pvsIsCustomer == isCustomer)
                return false;
            else
                return true;

        }

        public static string convertDataTableToString(DataTable dataTable)
        {
            string data = string.Empty;
            int rowsCount = dataTable.Rows.Count;
            for (int i = 0; i < rowsCount; i++)
            {
                DataRow row = dataTable.Rows[i];
                int columnsCount = dataTable.Columns.Count;
                for (int j = 0; j < columnsCount; j++)
                {
                    data += row[j];
                    if (j == columnsCount - 1)
                    {
                        if (i != (rowsCount - 1))
                            data += ",";
                    }
                    else
                        data += "";
                }
            }
            return data;
        }

        public string SendInvite(string Type, string Email)
        {
            try
            {
                oDAL = new cDAL(cDAL.ConnectionType.INIT);
                cAuth oAuth = new cAuth();
                string sql = "";
                sql = "Select * from SRM.UserInfo Where Email = '" + Email + "'";
                DataTable dtCheck = new DataTable();
                dtCheck = oDAL.GetData(sql);
                if (dtCheck.Rows.Count == 0)
                {
                    string createdBy = HttpContext.Current.Session["Email"].ToString();
                    string ConnectionType = HttpContext.Current.Session["DefaultDB"].ToString();
                    string AdminName = HttpContext.Current.Session["LastName"].ToString() + ' ' + HttpContext.Current.Session["FirstName"].ToString();
                    Guid newGuid = Guid.NewGuid();
                    sql = @"INSERT INTO [SRM].[Invitation]
           (
           [Email]
           ,[Type]
           ,[GUID]
           ,[CreatedBy]
           ,[ConnectionType]
           )
           VALUES
           (
		   '<Email>'
           ,'<Type>'
           ,'<GUID>'
           ,'<CreatedBy>'
           ,'<ConnectionType>'
		   )";

                    sql = sql.Replace("<Email>", Email);
                    sql = sql.Replace("<Type>", Type);
                    sql = sql.Replace("<GUID>", newGuid.ToString());
                    sql = sql.Replace("<CreatedBy>", createdBy);
                    sql = sql.Replace("<ConnectionType>", ConnectionType);
                    oDAL.Execute(sql);
                    if (!oDAL.HasErrors)
                    {

                        string DeployMode = WebConfigurationManager.AppSettings["DEPLOYMODE"];
                        //URL
                        DataTable dtURL = new DataTable();
                        dtURL = cCommon.GetEmailURL(DeployMode, "Registration");
                        string Url = dtURL.Rows[0]["URL"].ToString();
                        string Baseurl = Url + dtURL.Rows[0]["PageURL"].ToString() + newGuid;


                        var subject = "Collablly Account Invitation";
                        // Recipient's email address
                        string recipientEmail = Email;

                        string htmlBody = "";

                        // HTML body containing the form
                        DataTable dt = new DataTable();
                        string query = "";
                        cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
                        query = "Select SYSValue from dbo.zSysIni WHERE SysDesc = 'JOINSRM' ";
                        dt = oDAL.GetData(query);
                        htmlBody = dt.Rows[0]["SYSValue"].ToString();


                        htmlBody = htmlBody.Replace("{AdminName}", AdminName);
                        htmlBody = htmlBody.Replace("{Baseurl}", Baseurl);
                        try
                        {
                            cCommon.SendEmail(recipientEmail, subject, htmlBody, "", null);
                            Message = "OK";
                            return Message;
                        }
                        catch (Exception)
                        {
                            Message = "Wrong";
                            return Message;
                        }
                    }
                    else
                    {
                        Message = "Wrong";
                        return Message;
                    }
                }
                else
                {
                    Message = "Already";
                    return Message;
                }
            }
            catch (Exception ex)
            {

                cLog oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, "Invitation Send Button");
                Message = "Wrong";
                return Message;
            }

        }

        public string SendTempKey(string Password, string userId)
        {
            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            cAuth oAuth = new cAuth();
            object Email = null;

            string SearchEmail = @"select EMAIL from [SRM].[UserInfo]  WHERE USERID = '" + userId + "'";
            Email = oDAL.GetObject(SearchEmail);

            var subject = "Temperory Password";
            // Recipient's email address

            string htmlBody = "";

            // HTML body containing the form
            DataTable dt = new DataTable();
            string query = "";
             oDAL = new cDAL(cDAL.ConnectionType.INIT);
            query = "Select SYSValue from dbo.zSysIni WHERE SysDesc = 'TEMPASSWORD' ";
            dt = oDAL.GetData(query);
            htmlBody = dt.Rows[0]["SYSValue"].ToString();


            htmlBody = htmlBody.Replace("{Email}", Email.ToString());
            htmlBody = htmlBody.Replace("{TempKey}", Password);
         
            try
            {
                cCommon.SendEmail(Email.ToString(), subject, htmlBody, "", null);
                Message = "OK";
                return Message;
            }
            catch (Exception)
            {
                Message = "Wrong";
                return Message;
            }


        }



        public void CreateUser(string firstName, string lastName, string newPwd, string confirmPwd, string token)
        {
            // Handle the form submission logic here

            string userType = "";
            string buyerEmail = "";
            string emailAddress = "";
            string ConnectionType = "";
            string sql = "";

            DataTable dt = new DataTable();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);

            sql = "select * from [SRM].[Invitation] Where [GUID] = '" + token + "'";
            dt = oDAL.GetData(sql);

            if (dt.Rows.Count > 0)
            {
                userType = dt.Rows[0]["Type"].ToString();
                emailAddress = dt.Rows[0]["Email"].ToString();
                ConnectionType = dt.Rows[0]["ConnectionType"].ToString();
                if (CheckUserEmail(emailAddress))
                {
                    if (UserVerified(token, firstName, lastName, emailAddress, newPwd, userType, buyerEmail, ConnectionType))
                    {
                        Message = "User created successfully";
                        sql = "UPDATE SRM.UserInfo Set IsEmailVerified = '1' Where Email = '" + emailAddress + "'";
                        oDAL.Execute(sql);

                    }
                    else
                    {
                        Message = "Something went wrong!";

                    }
                }
                else
                {
                    Message = "This Email address already exist.";


                }

            }


        }

        public bool UserVerified(string tokenId, string FirstName, string LastName, string EmailAddress, string Pwd, string Type, string BuyerEmail, string ConnectionType)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = "";
            object retVal;
            string Password = "";
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
    ([FirstName], [LastName], [Password], [isEmailVerified], [Email], [CreatedOn],[Type],[tokenId], [DefaultProgram], [DefaultDB])
    VALUES
    ('<FirstName>', '<LastName>', '<Pwd>', '0', '<Email>', GETDATE(), '<Type>', '" + tokenId + "', '<DefaultProgram>' , '<ConnectionType>')";
            }



            sql = sql.Replace("<FirstName>", FirstName);
            sql = sql.Replace("<LastName>", LastName);
            sql = sql.Replace("<Pwd>", Password);
            sql = sql.Replace("<Email>", EmailAddress);
            sql = sql.Replace("<Type>", Type);
            sql = sql.Replace("<BuyerEmail>", BuyerEmail);
            sql = sql.Replace("<DefaultProgram>", "10000-Newyork");
            sql = sql.Replace("<ConnectionType>", ConnectionType);
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
            if (!oDAL.HasErrors)
                return true;
            else
                return false;

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
    }
}

public class clsProgram
{
    public string Id { get; set; }
    public string Name { get; set; }
}