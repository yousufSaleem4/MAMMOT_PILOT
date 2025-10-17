using PlusCP.Extensions;
using IP.Classess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PlusCP.Models
{
    public class Home
    {
        cDAL oDAL;
        string sql = string.Empty;
        public string TotalValue { get; set; }
        public string Menu { get; set; }
        public List<WebMenus> webMnu { get; set; }
        public string Message { get; set; }
        public string isAdmin { get; set; }

        public string Customer { get; set; }
        public string Hours { get; set; }
        public bool IsQtyUpdate { get; set; }
        public bool IsPriceUpdate { get; set; }




        public string ApiUrl { get; set; }
        public string Username { get; set; }
        public string password { get; set; }

        public string token { get; set; }

        public string SQLUsername { get; set; }
        public string SQLpassword { get; set; }
        public string SQlConn { get; set; }
        public string TimeZone { get; set; }
        public string CCEmail { get; set; }
        public string TermsCondition { get; set; }
        public List<Hashtable> lstConnectionData { get; set; }
        private string ProgramId = HttpContext.Current.Session["ProgramId"].ToString();
        private string signinId = HttpContext.Current.Session["SigninId"].ToString();
        public string firstName = HttpContext.Current.Session["FirstName"].ToString();

        private string isadmin = HttpContext.Current.Session["isAdmin"].ToString();
        //  private string customer = HttpContext.Current.Session["isCustomer"].ToString();


        public void GetMenus(string EmpName, String isadmin, string programName)
        {
            try
            {


                string WInITTeam = "Yousuf";

                sql = @"
                                WITH TMP AS
                                (
                                SELECT M.MnuId, RptCode, MnuType, MnuIcon, MnuTitle, MnuTitleShort, rptDesc, MnuDesc, MnuHyperlink, MnuTarget, MnuRights, MnuActive, MnuIsReady, MnuParent, AuthUser, AuthSite, DesignedBy
                                FROM SRM.Mnu  M 
                                WHERE MnuParent IS NULL 
                                AND MnuId IN (
                                                        SELECT DISTINCT MnuParent
                                                        FROM SRM.Mnu M
                                                        WHERE M.MnuActive = 1
                                                    )

                                UNION ALL 
                                SELECT M.MnuId, M.RptCode, M.MnuType, M.MnuIcon, M.MnuTitle, M.MnuTitleShort, M.rptDesc, M.MnuDesc, M.MnuHyperlink, M.MnuTarget, M.MnuRights, M.MnuActive, M.MnuIsReady, M.MnuParent, M.AuthUser, M.AuthSite, M.DesignedBy
                                FROM SRM.Mnu M
                                INNER JOIN TMP ON TMP.MnuId = M.MnuParent
                                WHERE 
							    M.MnuActive = '1' <MENUISREADY>
                                
							   
                                )
                                SELECT MnuId, RptCode, MnuType, MnuIcon, MnuTitle, MnuTitleShort, rptDesc, MnuDesc, MnuHyperlink, MnuTarget, MnuRights, MnuActive, MnuIsReady, MnuParent, AuthUser, AuthSite, DesignedBy 
                                FROM TMP
                                WHERE AuthSite = '<programName>' OR AuthSite = '*'


                     ";
                sql = sql.Replace("<programName>", programName);




                if (!WInITTeam.Contains(firstName))
                    sql = sql.Replace("<MENUISREADY>", "AND M.MnuIsReady = 1 ");
                else
                    sql = sql.Replace("<MENUISREADY>", "");

                sql += " ORDER BY MnuParent, MnuTitle ";
                oDAL = new cDAL(cDAL.ConnectionType.INIT);



                DataTable dt = new DataTable();
                dt = oDAL.GetData(sql);

                webMnu = dt.AsEnumerable().Select(dataRow => new WebMenus
                {
                    MnuId = Convert.ToInt32(dataRow["MnuId"]),
                    MnuType = dataRow["MnuType"].ToString(),
                    MnuIcon = dataRow["MnuIcon"].ToString(),
                    MnuTitle = dataRow["MnuTitle"].ToString(),
                    MnuTitleShort = dataRow["MnuTitleShort"].ToString(),
                    MnuHyperlink = dataRow["MnuHyperlink"].ToString(),
                    MnuTarget = dataRow["MnuTarget"].ToString(),
                    MnuParent = dataRow["MnuParent"].ToString(),
                    DesignedBy = dataRow["DesignedBy"].ToString(),
                    RptCode = dataRow["RptCode"].ToString(),
                    MnuDesc = dataRow["MnuDesc"].ToString(),
                    RptDesc = dataRow["rptDesc"].ToString()

                }).ToList<WebMenus>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet GetConnections()
        {
            try
            {
                var dal = new cDAL(cDAL.ConnectionType.INIT);
                var connections = dal.GetData(@"
                    SELECT ConText
                          ,ConValue
                          ,ConType
                          ,IsDropDown
                    FROM SRM.zConStr ")
                    // IsDropDown is Used For Select TEST,TRAN,PRODRPT ADDED by Junaid Kalwar
                    .ToList<BaseCWConnection>()
                    .Select(c => new BaseCWConnection
                    {
                        ConText = c.ConText,
                        ConType = c.ConType,
                        ConValue = BasicEncrypt.Instance.Encrypt(c.ConValue),
                        IsDropDown = c.IsDropDown
                    });
                DataSet dsconnection = new DataSet();
                DataTable dtConn = connections.ConvertToDataTable();
                dtConn.TableName = "CONN";
                dsconnection.Tables.Add(dtConn);
                return dsconnection;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetSysSettings()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            object result = null;
            DataTable dt = new DataTable();
            string sql = "select SysValue from [dbo].[zSysIni] Where SysDesc = 'Hours' OR SysDesc = 'IsQtyUpdate' OR SysDesc = 'IsPriceUpdate' ORDER BY SysCode DESC  ";
            dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                IsPriceUpdate = Convert.ToBoolean(dt.Rows[0]["SysValue"]);
                IsQtyUpdate = Convert.ToBoolean(dt.Rows[1]["SysValue"]);
                Hours = dt.Rows[2]["SysValue"].ToString();
                
                
            }
            else
            {
                Hours = "0";
                IsQtyUpdate = false;
                IsPriceUpdate = false;
            }


        }

        public void GetCCEmail()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            object result = null;
            string sql = "SELECT TOP 1 [CCEmailAddress] FROM [dbo].[EmailSetup] ";
            result = oDAL.GetObject(sql);
            if (result != null)
                CCEmail = result.ToString();
            else
                CCEmail = "";

        }



        public void GetAPISettings()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            DataTable dt = new DataTable();
            string Type = HttpContext.Current.Session["DefaultDB"].ToString();
            string sql = "select * from [SRM].[zConStr] Where ConType = '" + Type + "'  ";
            dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                SQlConn = dt.Rows[0]["ConValue"].ToString();
                SQLUsername = dt.Rows[0]["UserName"].ToString();
                SQLpassword = dt.Rows[0]["Password"].ToString();
                if (!string.IsNullOrEmpty(SQLpassword))
                {
                    SQLpassword = BasicEncrypt.Instance.Decrypt(SQLpassword);

                }
            }

            sql = "select TOP 1 * from [dbo].[URLSetup] where URLType = 'API' AND DeployMode = '" + Type.ToUpper() + "' ";
            dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                ApiUrl = dt.Rows[0]["URL"].ToString();
                Username = dt.Rows[0]["UserName"].ToString();
                password = dt.Rows[0]["Password"].ToString();
                token = dt.Rows[0]["TokenKey"].ToString();
                password = BasicEncrypt.Instance.Decrypt(password);
            }


            sql = "select SysValue from [dbo].[zSysIni] where  [SysDesc] = 'TermsandCondition' ";
            dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                TermsCondition = dt.Rows[0]["SysValue"].ToString();

            }
        }

        //public DataTable GetTimeZone()
        //{
        //    cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
        //    DataTable dt = new DataTable();
        //    string Type = HttpContext.Current.Session["DefaultDB"].ToString();
        //    string sql = "select Id, City from [dbo].[TimingZone]  ";
        //    dt = oDAL.GetData(sql);

        //    return dt;
        //}

        public bool GetConnectionData(string ConType)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string query = string.Empty;
            DataTable dt = new DataTable();

            query = @"select * from [SRM].[zConStr] Where ConType = '" + ConType + "'   ";
            dt = oDAL.GetData(query);
            foreach (DataRow row in dt.Rows)
            {
                row["Password"] = BasicEncrypt.Instance.Decrypt(row["Password"].ToString());

            }
            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            query = "select TOP 1 URL, UserName, Password, TokenKey AS Token from [dbo].[URLSetup] where URLType = 'API' AND DeployMode = '" + ConType + "' ";
            DataTable dtAPI = new DataTable();
            dtAPI = oDAL.GetData(query);
            foreach (DataRow row in dtAPI.Rows)
            {
                row["Password"] = BasicEncrypt.Instance.Decrypt(row["Password"].ToString());

            }

            dt.Merge(dtAPI);
            if (!oDAL.HasErrors)
            {

                lstConnectionData = new List<Hashtable>();
                lstConnectionData = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool ChangePassword(String oldPwd, string newPwd, string confirmPwd)
        {
            string password = "";
            string UserEmail = HttpContext.Current.Session["Email"].ToString();
            password = BasicEncrypt.Instance.Encrypt(newPwd.Trim());
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = @"Update [SRM].[UserInfo] SET PASSWORD = '<newPwd>' Where Email = '<Email>'";

            sql = sql.Replace("<newPwd>", password);
            sql = sql.Replace("<Email>", UserEmail);

            oDAL.Execute(sql);
            if (!oDAL.HasErrors)
            {
                Message = "Password has been reset.";
                return true;
            }
            else
            {
                Message = "Password not changed.";
                return false;
            }
        }

        public string GetConnectionString(string conType)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            sql = "SELECT ConValue FROM SRM.zConStr WHERE ConType = '" + conType + "'";
            DataTable dt = oDAL.GetData(sql);
            return dt.Rows[0]["ConValue"].ToString();
        }
        public DataTable GetRecord() //Added by Huzaifa
        {
            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string query = string.Empty;
            query = @"select top 1 RECNUM, RptUrl from zLogQuery 
                        where SigninId = '" + HttpContext.Current.Session["SigninId"].ToString() + "'" +
                        "AND QueryExecTime = '" + cDAL.QueryExecTime + "'" +
                        "AND RowsFetched = '" + cDAL.RowsFetched + "'";
            //"AND RemoteHost = '" + HttpContext.Current.Session["RemoteAddr"].ToString() + "'";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        public DateTime? GetLastSyncDateFromDB()
        {
            oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string query = @"Select TOP 1 LastUpdatedFromPortal from [dbo].[PODetail]";
            DataTable dt = oDAL.GetData(query);

            if (dt.Rows.Count > 0 && dt.Rows[0]["LastUpdatedFromPortal"] != DBNull.Value)
            {
                return Convert.ToDateTime(dt.Rows[0]["LastUpdatedFromPortal"]);
            }

            return null;
        }
    }

    public class BaseCWConnection
    {
        public string ConText { get; set; }
        public string ConValue { get; set; }
        public string ConType { get; set; }
        public string IsDropDown { get; set; }// added by junaid

    }

    public class WebMenus
    {
        public int MnuId { get; set; }
        public string MnuType { get; set; }
        public string MnuIcon { get; set; }
        public string MnuTitle { get; set; }
        public string MnuTitleShort { get; set; }
        public string MnuHyperlink { get; set; }
        public string MnuTarget { get; set; }
        public string MnuParent { get; set; }
        public string DesignedBy { get; set; }
        public string RptCode { get; set; }
        public string MnuDesc { get; set; }
        public string RptDesc { get; set; }
        public string isAdmin { get; set; }
    }

}