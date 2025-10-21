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

        //public ActionResult GetTicketInfo(string ticketId)
        //{
        //    try
        //    {
        //        TicketSystem oTicket = new TicketSystem();

        //        bool success = oTicket.GetDetail(ticketId);
        //        oTicket.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

        //        if (success)
        //            return View(oTicket);
        //        else
        //            return View(oTicket);
        //    }
        //    catch (Exception e)
        //    {
        //        ViewBag.ErrMessage = e.Message;
        //        return View();
        //    }
        //}

        [HttpPost]
        public JsonResult UpdateTicket(string Ticket_ID, string Title, string Description, string Ticket_Type,
                                 string Status, string Priority, DateTime ETA,
                                 int Progress_Percentage, string Notes)
        {
            TicketSystem oTicket = new TicketSystem();
            bool success = oTicket.UpdateTicket(Ticket_ID, Title, Description, Ticket_Type, Status, Priority, ETA, Progress_Percentage, Notes);
            oTicket.SaveTicketHistory(Ticket_ID, Status, Progress_Percentage);
            if (success)
                return Json(new { success = true, message = "✅ Ticket updated successfully!" });
            else
                return Json(new { success = false, message = "❌ Error: " + oTicket.ErrorMessage });
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

        [HttpGet]
        public JsonResult GetTicketById(string ticketId)
        {
            try
            {
                TicketSystem oTicket = new TicketSystem();
                bool success = oTicket.GetDetail(ticketId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        ticket_id = oTicket.ticket_id,
                        title = oTicket.title,
                        description = oTicket.description,
                        ticket_type = oTicket.ticket_type,
                        status = oTicket.status,
                        priority = oTicket.priority,
                        eta = oTicket.eta,
                        progress = oTicket.progress,
                        notes = oTicket.notes
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Ticket not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
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

                // 🧩 Fetch HTML template from DB
                DataTable dtTemplate = oTicket.NewTicketEmailTemplate();
                string htmlBody = "";

                if (dtTemplate.Rows.Count > 0)
                    htmlBody = dtTemplate.Rows[0]["SysValue"].ToString();
                else
                    htmlBody = "<p>Email template not found in zSysIni.</p>";

                // 🔄 Replace placeholders
                htmlBody = htmlBody
                    .Replace("{title}", title)
                    .Replace("{description}", description)
                    .Replace("{priority}", priority)
                    .Replace("{status}", status)
                    .Replace("{eta}", eta.ToString("yyyy-MM-dd"))
                    .Replace("{created_by}", created_by.ToString());

                // ✉️ Send email
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
