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


        // 🔹 Load ticket list with filters
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
    FORMAT(t.Created_At, 'yyyy.MM.dd HH:mm') AS Created_At,
    FORMAT(t.Updated_At, 'yyyy.MM.dd HH:mm') AS Updated_At,
    FORMAT(t.ETA, 'yyyy.MM.dd') AS ETA,
    t.Progress_Percentage,
    DATEDIFF(DAY, t.Created_At, GETDATE()) AS Days_Since_Created
FROM SRM.Tickets t
LEFT JOIN SRM.UserInfo cu ON t.Created_By = cu.UserId
LEFT JOIN SRM.UserInfo au ON t.Assigned_To = au.UserId
                WHERE 1=1";

            // 🔹 Apply filters dynamically
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
    FORMAT(t.Created_At, 'yyyy.MM.dd HH:mm') AS Created_At,
    FORMAT(t.Updated_At, 'yyyy.MM.dd HH:mm') AS Updated_At,
    FORMAT(t.ETA, 'yyyy.MM.dd') AS ETA,
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


        public bool CreateTicket(string title, string description, string ticket_type, string status, string priority,
                           int created_by, /*int assigned_to,*/ DateTime eta, int progress_percentage, string notes)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            string sql = "INSERT INTO SRM.Tickets " +
              "(Title, Description, Ticket_Type, Status, Priority, Created_By, Assigned_To, Created_At, Updated_At, ETA, Progress_Percentage, Notes) " +
              "VALUES ('" + title + "', '" + description + "', '" + ticket_type + "', '" + status + "', '" + priority + "', " +
              created_by + ", 30, GETDATE(), GETDATE(), '" + eta + "', " + progress_percentage + ", '" + notes + "')";


            oDAL.Execute(sql);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }

            return true;
        }



        public bool SaveComment(int ticketId, int userId, string commentText)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

            // Build SQL string like your CreateTicket method
            string sql = "INSERT INTO SRM.Comments (TicketID, UserID, Comment_Text, Created_At, Updated_At) " +
                         "VALUES (" + ticketId + ", " + userId + ", '" + commentText.Replace("'", "''") + "', GETDATE(), GETDATE())";

            oDAL.Execute(sql);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }

            return true;
        }






    }
}
