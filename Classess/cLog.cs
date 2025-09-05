using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

public class cLog
{
    cDAL oDal = new cDAL(cDAL.ConnectionType.INIT);
    string sql = string.Empty;

    public void SaveLog(string rptName, string rptUrl, string rptCode)
    {
        SaveMru(rptCode);
        rptUrl = rptUrl.Replace("'", "''");

        sql = "INSERT INTO zLogQuery (SigninId, SigninName, RemoteHost, RptCode, RptName, RptUrl, InsertOn, Origin, QueryExecTime, RowsFetched) ";
        sql += "VALUES (";
        sql += "'" + HttpContext.Current.Session["SigninId"].ToString() + "', ";
        sql += "'" + HttpContext.Current.Session["FirstName"].ToString() + "', ";
        sql += "NULL, ";
        sql += "'" + rptCode + "', ";
        sql += "'" + rptName + "', ";
        sql += "'" + rptUrl + "', ";
        sql += "Getdate(), ";
        sql += "'SRM',";
        sql += "'" + cDAL.QueryExecTime + "', ";
        sql += "'" + cDAL.RowsFetched + "') "; 
        oDal.Execute(sql);
    }
    public void UpdateLog(string gridExecTime, string recNo, string rptUrl) //Added by Huzaifa
    {
        cDAL oDal = new cDAL(cDAL.ConnectionType.INIT);
        string sql = string.Empty;

        sql = "UPDATE zLogQuery SET GridExecTime = '" + gridExecTime + "' " +
            "WHERE RECNUM = '" + recNo + "' " +
            "AND SigninId = '" + HttpContext.Current.Session["SigninId"].ToString() + "'" +
            "AND SigninName ='" + HttpContext.Current.Session["FirstName"].ToString() + "'" +
            "AND RptUrl = '" + rptUrl + "' ";

        oDal.Execute(sql);
    }

    public void RecordError(string errorMsg, string errorStack, string errorQry)
    {
        cDAL oDal = new cDAL(cDAL.ConnectionType.INIT);
        errorMsg = errorMsg.Replace("'", "");
        errorQry = errorQry.Replace("'", "");

        object email = HttpContext.Current.Session["Email"];
        object DefaulDB = HttpContext.Current.Session["DefaultDB"];

        sql = "INSERT INTO zLogError (ErrMsg, ErrStack, ErrBy, ErrOn, ErrFrom, ErrQry, ErrEnvironment) ";
        sql += "VALUES (";
        sql += "'" + errorMsg + "', ";
        sql += "'" + errorStack + "', ";
        sql += "'" + ((email == null) ? "unknown" : email) + "', ";
        sql += "Getdate(), ";
        sql += "'SRM', ";
        sql += "\'" + errorQry + "\',";
        sql += "'" + DefaulDB + "') ";
        oDal.Execute(sql);
    }

   

    public static string GetEmailBody(List<string> lst)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string item in lst)
        {
            sb.AppendLine(item);
        }
        return sb.ToString();
    }

    //internal void AddSqlQuery(string v1, string query, string empty, bool v2)
    //{
    //    throw new NotImplementedException();
    //}
    public void AddSqlQuery(string rptCode, string query, string headerText = "", bool isDetail = false)
    {
        query = query.Replace("'", "`");
        int empId = Convert.ToInt32(HttpContext.Current.Session["SigninId"]);
        if (isDetail == true)
        {
            sql = "DELETE FROM EP.SQLQuery WHERE EmpId = " + empId + " AND RptCode LIKE '" + rptCode + "%'";
            oDal.Execute(sql);
        }

        sql = "SELECT COUNT(*) FROM EP.SQLQuery WHERE EmpId = " + empId + " AND RptCode = '" + rptCode + "'";
        object obj = oDal.GetObject(sql);
        int count = Convert.ToInt32(obj);
        if (count == 0)
        {
            sql = "INSERT INTO EP.SQLQuery (RptCode, EmpId, HeaderText, Query, QueryOn) ";
            sql += "VALUES ('" + rptCode + "', " + empId + ", '" + (headerText == "" ? "" : headerText)
            + "', '" + query + "', GETDATE())";
        }
        else
        {
            sql = "UPDATE EP.SQLQuery Set HeaderText = '" + headerText + "', Query = '" + query + "', QueryOn = GETDATE() ";
            sql += "WHERE EmpId = " + empId + " AND RptCode = '" + rptCode + "'";
        }
        oDal.Execute(sql);
    }

    private void SaveMru(string rptCode)
    {
        int accessCount = 1;
        int empId = Convert.ToInt32(HttpContext.Current.Session["SigninId"]);
        string empName = HttpContext.Current.Session["FirstName"].ToString();

        sql = "SELECT AccessCount FROM zLogMru WHERE EmpId = " + empId + " AND RptCode = '" + rptCode + "' AND Origin = 'SRM' ";
        object obj = oDal.GetObject(sql);

        if (obj == null)
        {
            sql = "INSERT INTO zLogMru (EmpId, EmpName, RptCode, AccessCount,Origin) ";
            sql += "VALUES (" + empId + ", '" + empName + "', '" + rptCode + "', " + accessCount + ",'SRM')";
        }
        else
        {
            accessCount = Convert.ToInt32(obj) + 1;
            sql = "UPDATE zLogMru Set LogDt = GETDATE(), AccessCount = " + accessCount + " ";
            if (empId != 0)
                sql += "WHERE Origin = 'SRM' AND EmpId = " + empId + " AND RptCode = '" + rptCode + "'";
            else
                sql += "WHERE Origin = 'SRM' AND EmpName = '" + empName + "' AND RptCode = '" + rptCode + "'";
        } 

        oDal.Execute(sql);
    }
}
