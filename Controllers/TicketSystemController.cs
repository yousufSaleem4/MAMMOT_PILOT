using IP.ActionFilters;
using PlusCP.Classess;
using PlusCP.Models;
using System;
using System.Data;
using System.Web.Mvc;
using System.Linq;
using System.Configuration;
using System.Web;

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

        

        [HttpPost]
        public JsonResult UpdateTicket(string Ticket_ID, string Title, string Description, string Ticket_Type,
                                 string Status, string Priority, DateTime ETA,
                                 int Progress_Percentage, string Notes)
        {
            TicketSystem oTicket = new TicketSystem();
            bool success = oTicket.UpdateTicket(Ticket_ID, Title, Description, Ticket_Type, Status, Priority, ETA, Progress_Percentage, Notes);
            oTicket.SaveTicketHistory(Ticket_ID, Status, Progress_Percentage);
            if (success)
                return Json(new { success = true, message = "Ticket updated successfully!" });
            else
                return Json(new { success = false, message = " Error: " + oTicket.ErrorMessage });
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
            //string decodedDescription = HttpUtility.UrlDecode(description);

            string ticketId = oTicket.CreateTicket(
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

            if (string.IsNullOrEmpty(ticketId))
                return Json(new { success = false, message = oTicket.ErrorMessage });

            oTicket.SaveTicketHistory(ticketId, status, progress_percentage);

            string deployMode = ConfigurationManager.AppSettings["DEPLOYMODE"]?.Trim().ToUpper() ?? "TEST";
            string testEmail = ConfigurationManager.AppSettings["TESTEMAIL"]?.Trim();

            string adminEmail = string.Empty;

            if (deployMode == "TEST")
            {
                adminEmail = testEmail;
            }
            else
            {
                DataTable dtAdmin = oTicket.GetAdminEmail();
                if (dtAdmin.Rows.Count > 0)
                    adminEmail = dtAdmin.Rows[0]["Email"].ToString();
            }

            string subject = $"New Ticket Created - {title}";
            DataTable dtTemplate = oTicket.NewTicketEmailTemplate();

            string htmlBody = dtTemplate.Rows.Count > 0
                ? dtTemplate.Rows[0]["SysValue"].ToString()
                : "<p>Email template not found in zSysIni.</p>";

            htmlBody = htmlBody
                .Replace("{title}", title)
                .Replace("{description}", description)
                .Replace("{priority}", priority)
                .Replace("{status}", status)
                .Replace("{eta}", eta.ToString("yyyy-MM-dd"))
                .Replace("{created_by}", created_by.ToString());

            string emailResult = !string.IsNullOrEmpty(adminEmail)
                ? cCommon.SendEmail(adminEmail, subject, htmlBody, "", null)
                : "NO_EMAIL_FOUND";

            if (emailResult == "SENT")
                return Json(new { success = true, message = $"Ticket submitted successfully. A member of the Collablly Team will reach out soon." });
            else if (emailResult == "NO_EMAIL_FOUND")
                return Json(new { success = true, message = "Ticket submitted successfully. A member of the Collablly Team will reach out soon." });
            else
                return Json(new { success = true, message = $"Ticket submitted successfully. A member of the Collablly Team will reach out soon." });

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

            int userId = Convert.ToInt32(Session["SigninId"]);

            bool result = oTicket.SaveComment(
                ticketId,
                userId,
                commentText
            );

            if (result)
                return Json(new { success = true, message = "Ticket created successfully!" });
            else
                return Json(new { success = false, message = oTicket.ErrorMessage });
        }


    }


}
