using IP.ActionFilters;
using PlusCP.Classess;
using PlusCP.Models;
using System;
using System.Data;
using System.Web.Mvc;
using System.Linq;

namespace PlusCP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class TicketSystemController : Controller
    {
        TicketSystem oTicketSystem = new TicketSystem();

        public ActionResult Index()
        {

            return View();
        }


        public JsonResult GetList(string status, string ticket_type, string priority)
        {
            string menuTitle = string.Empty;
            string RptCode;
          
           
            oTicketSystem = new TicketSystem();
            oTicketSystem.GetData(status, ticket_type, priority);
            var jsonResult = Json(oTicketSystem, JsonRequestBehavior.AllowGet);
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

  
        public ActionResult Detail(string ticketId)
        {
            try
            {
                TicketSystem oTicket = new TicketSystem();
                
                bool success = oTicket.GetDetail(ticketId);
                oTicket.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                
                if (success)
                    return View(oTicket);
                else
                    return View(oTicket);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }


        [HttpPost]
        public JsonResult SaveTicket(string title, string description, string ticket_type, string status,
                             string priority, DateTime eta, int progress_percentage, string notes)
        {
            TicketSystem oTicket = new TicketSystem();
            int created_by = Convert.ToInt32(Session["SigninId"]);

            bool result = oTicket.CreateTicket(
                title,
                description,
                ticket_type,
                status,
                priority,
                created_by,
                eta,
                progress_percentage,
                notes
            );

            if (result)
            {
                string adminEmail = "mohsinsaleemshahani@gmail.com"; 
                string subject = $"New Ticket Created - {title}";
                string htmlBody = $@"
<html>
<body style='font-family:Segoe UI, Arial, sans-serif; background-color:#f8f9fb; padding:20px;'>
    <div style='max-width:600px; margin:auto; background:#fff; border-radius:8px; box-shadow:0 3px 8px rgba(0,0,0,0.1); padding:20px;'>
        <h3 style='color:#0078D7;'>New Ticket Created</h3>
        <p><b>Title:</b> {title}</p>
        <p><b>Description:</b> {description}</p>
        <p><b>Priority:</b> {priority}</p>
        <p><b>Status:</b> {status}</p>
        <p><b>ETA:</b> {eta:yyyy-MM-dd}</p>
        <p><b>Created By ID:</b> {created_by}</p>
        <hr style='border:none; border-top:1px solid #e0e0e0; margin:20px 0;' />
        <p style='font-size:13px; color:#666;'>This is an auto-generated email from the Ticket System.</p>
    </div>
</body>
</html>";


                string emailResult = cCommon.SendEmail(adminEmail, subject, htmlBody, "", null);

                if (emailResult == "SENT")
                    return Json(new { success = true, message = "✅ Ticket created and email sent successfully!" });
                else
                    return Json(new { success = true, message = "✅ Ticket created but email failed to send." });
            }
            else
            {
                return Json(new { success = false, message = oTicket.ErrorMessage });
            }
        }




        public JsonResult GetAssigneeList()
        {
            TicketSystem oTicket = new TicketSystem();
            DataTable dt = oTicket.GetAssignee();

            var assigneeList = dt.AsEnumerable().Select(row => new
            {
                userId = row["UserId"].ToString(),
                assignee = row["Assignee"].ToString()
            }).ToList();

            return Json(assigneeList, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult InsertComment(int ticketId, string commentText)
        {
            TicketSystem oTicket = new TicketSystem();

            // ✅ Created_By session se lo (user ID)
            int userId = Convert.ToInt32(Session["SigninId"]);

            bool result = oTicket.SaveComment(
                ticketId,
                userId,
                commentText
            );

            if (result)
                return Json(new { success = true, message = "✅ Ticket created successfully!" });
            else
                return Json(new { success = false, message = oTicket.ErrorMessage });
        }


    }


}
