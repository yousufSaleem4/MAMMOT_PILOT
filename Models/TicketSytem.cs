using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PlusCP.Models
{
    public class TicketSystem
    {
        public List<Hashtable> lstTicket { get; set; }
        public List<Hashtable> lstDetail { get; set; }
        public string ErrorMessage { get; set; }

        public string ticket_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string ticket_type { get; set; }
        public string status { get; set; }
        public string priority { get; set; }
        public string created_by { get; set; }
        public string assigned_to { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string notes { get; set; }
        public int progress { get; set; }
        public string eta { get; set; }
        public int createdDays { get; set; }
        public int updatedDays { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }


        public DataTable GetAssignee()
        {
            DataTable dt = new DataTable();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = @"SELECT  UserId
      , concat(FirstName,' ', LastName)  as Assignee
  FROM SRM.UserInfo 
where email = 'mohsin@collablly.com' ";
            dt = oDAL.GetData(sql);        

            return dt;
        }


        public bool GetData(string status, string ticket_type, string priority)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string sql = @"SELECT  
    t.Id,
    t.Ticket_Id,
    t.Title,
    t.Description,
    t.Ticket_Type,
    t.Status,
    t.Priority,
    CONCAT(cu.FirstName, ' ', cu.LastName) AS Created_By_Name,
    CONCAT(au.FirstName, ' ', au.LastName) AS Assigned_To_Name,
    FORMAT(t.Created_At, 'yyyy-MM-dd HH:mm') AS Created_At,
    FORMAT(t.Updated_At, 'yyyy-MM-dd HH:mm') AS Updated_At,
    FORMAT(t.ETA, 'yyyy-MM-dd') AS ETA,
    t.Progress_Percentage,
    DATEDIFF(DAY, t.Created_At, GETDATE()) AS Days_Since_Created
FROM SRM.Tickets t
LEFT JOIN SRM.UserInfo cu ON t.Created_By = cu.UserId
LEFT JOIN SRM.UserInfo au ON t.Assigned_To = au.UserId
                WHERE 1=1";

            if (!string.IsNullOrEmpty(status))
                sql += " AND  t.Status = '" + status + "'";

            if (!string.IsNullOrEmpty(ticket_type))
                sql += " AND  t.Ticket_Type = '" + ticket_type + "'";

            if (!string.IsNullOrEmpty(priority))
                sql += " AND t.Priority = '" + priority + "'";

            sql += " ORDER BY t.Created_At DESC;";

            DataTable dt = oDAL.GetData(sql);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                lstTicket = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }


        public bool UpdateTicket(string Ticket_ID, string Title, string Description, string Ticket_Type, string Status, string Priority, DateTime ETA, int Progress_Percentage, string Notes)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string sql = $@"
            UPDATE SRM.Tickets
            SET 
                Title = '{Title.Replace("'", "''")}',
                Description = '{Description.Replace("'", "''")}',
                Ticket_Type = '{Ticket_Type}',
                Status = '{Status}',
                Priority = '{Priority}',
                ETA = '{ETA:yyyy-MM-dd}',
                Progress_Percentage = {Progress_Percentage},
                Notes = '{Notes.Replace("'", "''")}',
                Updated_At = GETDATE()
            WHERE Ticket_Id = '{Ticket_ID}'";

            oDAL.Execute(sql);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            return true;
        }
  

    public bool GetDetail(string ticketId)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string sql = @"SELECT  
    t.Id,
    t.Ticket_Id,
    t.Title,
    t.Description,
    t.Ticket_Type,
    t.Status,
    t.Priority,
    CONCAT(cu.FirstName, ' ', cu.LastName) AS Created_By_Name,
    CONCAT(au.FirstName, ' ', au.LastName) AS Assigned_To_Name,
    FORMAT(t.Created_At, 'yyyy-MM-dd HH:mm') AS Created_At,
    FORMAT(t.Updated_At, 'yyyy-MM-dd HH:mm') AS Updated_At,
    FORMAT(t.ETA, 'yyyy-MM-dd') AS ETA,
    t.Progress_Percentage,
    t.Notes,
    DATEDIFF(DAY, t.Created_At, GETDATE()) AS Days_Since_Created,  
 DATEDIFF(DAY, t.created_at, GETDATE()) AS Days_Since_Updated
FROM SRM.Tickets t
LEFT JOIN SRM.UserInfo cu ON t.Created_By = cu.UserId
LEFT JOIN SRM.UserInfo au ON t.Assigned_To = au.UserId

    WHERE t.ticket_id = '" + ticketId + "'";

            DataTable dtHeader = oDAL.GetData(sql);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }

            if (dtHeader.Rows.Count > 0)
            {
                DataRow dr = dtHeader.Rows[0];

                ticket_id = dr["Ticket_Id"].ToString();
                title = dr["Title"].ToString();
                description = dr["Description"].ToString();
                ticket_type = dr["Ticket_Type"].ToString();
                status = dr["Status"].ToString();
                priority = dr["Priority"].ToString();
                created_by = dr["Created_By_Name"].ToString();
                assigned_to = dr["Assigned_To_Name"].ToString();

                created_at = dr["Created_At"] == DBNull.Value ? "" : dr["created_at"].ToString();
                updated_at = dr["Updated_At"] == DBNull.Value ? "" : dr["updated_at"].ToString();
                eta = dr["ETA"] == DBNull.Value ? "" : dr["ETA"].ToString();

                // 👇 convert progress to int safely
                progress = dr["Progress_Percentage"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Progress_Percentage"]);
                notes = dr["Notes"].ToString();
                createdDays = dr["Days_Since_Created"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Days_Since_Created"]);
                updatedDays = dr["Days_Since_Updated"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Days_Since_Updated"]);

                return true;
            }

            return false;
        }


        public string CreateTicket(string title, string description, string ticket_type, string status, string priority,
                             int created_by, DateTime eta, int progress_percentage, string notes)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string sql = "EXEC SRM.usp_CreateTicket " +
                         "@Title = '" + title + "', " +
                         "@Description = '" + description + "', " +
                         "@Ticket_Type = '" + ticket_type + "', " +
                         "@Status = '" + status + "', " +
                         "@Priority = '" + priority + "', " +
                         "@Created_By = " + created_by + ", " +
                         "@Assigned_To = 1026, " +
                         "@ETA = '" + eta + "', " +
                         "@Progress_Percentage = " + progress_percentage + ", " +
                         "@Notes = '" + notes + "'";

            DataTable dt = oDAL.GetData(sql);

            if (oDAL.HasErrors || dt.Rows.Count == 0)
            {
                ErrorMessage = oDAL.ErrMessage;
                return null;
            }

            return dt.Rows[0]["Ticket_Id"].ToString();
        }






        public bool SaveTicketHistory(string Ticket_ID, string Status, int Progress_Percentage)
        {
            try
            {
                cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

                // 🔹 Safely get current user ID
                int updated_by = 0;
                if (HttpContext.Current.Session["SigninId"] != null)
                    updated_by = Convert.ToInt32(HttpContext.Current.Session["SigninId"]);

                string sql = "INSERT INTO SRM.Ticket_History " +
                             "(TicketID, Status, Progress_Percentage, Updated_By, Updated_At) " +
                             "VALUES ('" + Ticket_ID + "', '" + Status + "', " + Progress_Percentage + ", " + updated_by + ", GETDATE())";

                oDAL.Execute(sql);

                if (oDAL.HasErrors)
                {
                    ErrorMessage = oDAL.ErrMessage;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }


        public bool SaveComment(string ticketId, int userId, string commentText)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string sql = "INSERT INTO SRM.TicketComments (TicketID, UserID, Comment_Text, Created_At, Updated_At) " +
                         "VALUES ('" + ticketId + "', " + userId + ", '" + commentText.Replace("'", "''") + "', GETDATE(), GETDATE())";

            oDAL.Execute(sql);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }

            return true;
        }

        public DataTable GetComments(string ticketId)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string sql = @"
        SELECT 
            c.CommentID, 
              u.FirstName + ' ' + u.LastName AS UserName, 
            c.Comment_Text, 
            c.Created_At
        FROM SRM.TicketComments c
        INNER JOIN SRM.UserInfo u ON c.UserID = u.UserID
        WHERE c.TicketID = '" + ticketId.Replace("'", "''") + @"'
        ORDER BY c.Created_At DESC";

            DataTable dt = oDAL.GetData(sql);
            return dt;
        }





        public DataTable NewTicketEmailTemplate()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = @"
        SELECT TOP 1 SysValue 
        FROM [dbo].[zSysIni] 
        WHERE SysDesc = 'NEWTICKETEMAIL'
        ORDER BY SysCode DESC";
            return oDAL.GetData(sql);
        }

        public DataTable GetTicketDetails(string ticketId)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = "SELECT Ticket_ID, Title, Priority, Status FROM SRM.Tickets WHERE Ticket_ID = '" + ticketId + "'";
            return oDAL.GetData(sql);
        }

        public DataTable GetUserName(int userId)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = "SELECT FirstName + ' ' + LastName AS UserName FROM SRM.UserInfo WHERE userId = '" + userId + "'";
            return oDAL.GetData(sql);
        }

        public DataTable NewCommentEmailTemplate()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = "SELECT SysValue FROM zSysIni WHERE SysDesc = 'NewCommentEmail'" +
                "ORDER BY SysCode DESC";
            return oDAL.GetData(sql);
        }


        public DataTable GetAdminEmail()
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string sql = @"
        SELECT TOP 1 Email 
        FROM [SRM].[userinfo] 
        WHERE Firstname = 'Super' AND IsAdmin = 1
        ORDER BY UserId DESC ";
            return oDAL.GetData(sql);
        }

    }
}
