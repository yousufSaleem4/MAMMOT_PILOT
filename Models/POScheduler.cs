using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PlusCP.Models
{
    public class POScheduler
    {
        cDAL oDAL;
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstScheduler { get; set; }

        public DataTable DTPO { get; set; }
        public List<ArrayList> lstPOs { get; set; }
        public bool GetList()
        {
            oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = string.Empty;
            query = @"SELECT  [Id]
                     ,[PO]
                     ,[NoOfDays]
                     ,[CreatedBy]
                     ,[CreatedOn]

                 FROM [dbo].[POScheduler] 
                 Order By CreatedOn Desc
";

           

            DataTable dt = oDAL.GetData(query);

            DTPO = HttpContext.Current.Session["DashboardData"] as DataTable; //Session["DashboardData"] as List<ArrayList>;
            DTPO.Columns.Add("PONumber");

            foreach (DataRow row in dt.Rows)
            {
                row["PONumber"] = $"{row["POHeader_PONum"]}-{row["PODetail_POLine"]}-{row["PORel_PORelNum"]}";

            }

                if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstScheduler = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

    }
}