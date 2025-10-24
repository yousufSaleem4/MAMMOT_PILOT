using IP.ActionFilters;
using PlusCP.Classess;
using PlusCP.Models;
using System;
using System.Data;
using System.Web.Mvc;
using System.Linq;
using System.Configuration;
using System.Web;
using IP.Classess;
using System.Net;
using System.Net.Http;

namespace PlusCP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class TicketSystemController : Controller
    {
        TicketSystem oTicketSystem = new TicketSystem();

        public ActionResult Index()
        {
            string UserType = Session["UserType"] != null ? Session["UserType"].ToString() : "User";
            ViewBag.UserType = UserType;
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
            string decodedDescription = HttpUtility.UrlDecode(Description);
            bool success = oTicket.UpdateTicket(Ticket_ID, Title, decodedDescription, Ticket_Type, Status, Priority, ETA, Progress_Percentage, Notes);
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
                string UserType = Session["UserType"] != null ? Session["UserType"].ToString() : "User";
                ViewBag.UserType = UserType;


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
            string decodedDescription = HttpUtility.UrlDecode(description);
            int progress = 0;
            int.TryParse(Request["progress_percentage"], out progress);

            string ticketId = oTicket.CreateTicket(
                title,
                decodedDescription,
                ticket_type,
                status,
                priority,
                created_by,
                eta,
                progress,
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
                .Replace("{description}", decodedDescription)
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
        public JsonResult InsertComment(string ticketId, string commentText)
        {
            TicketSystem oTicket = new TicketSystem();
            int userId = Convert.ToInt32(Session["SigninId"]);

            // 1️⃣ Save comment first
            bool result = oTicket.SaveComment(ticketId, userId, commentText);

            if (!result)
                return Json(new { success = false, message = oTicket.ErrorMessage });

            // 2️⃣ Get username
            DataTable dtUser = oTicket.GetUserName(userId);
            string userName = dtUser.Rows.Count > 0 ? dtUser.Rows[0]["UserName"].ToString() : "Unknown User";

            // 3️⃣ Get comment email template
            DataTable dtTemplate = oTicket.NewCommentEmailTemplate();
            string htmlBody = dtTemplate.Rows.Count > 0
                ? dtTemplate.Rows[0]["SysValue"].ToString()
                : "<p>Email template not found.</p>";

            // 4️⃣ Get ticket details for context
            DataTable dtTicket = oTicket.GetTicketDetails(ticketId);
            string title = dtTicket.Rows.Count > 0 ? dtTicket.Rows[0]["Title"].ToString() : "";
            string priority = dtTicket.Rows.Count > 0 ? dtTicket.Rows[0]["Priority"].ToString() : "";
            string status = dtTicket.Rows.Count > 0 ? dtTicket.Rows[0]["Status"].ToString() : "";

            // 5️⃣ Replace placeholders in email template
            htmlBody = htmlBody
                .Replace("{ticket_id}", ticketId)
                .Replace("{title}", title)
                .Replace("{comment_text}", commentText)
                .Replace("{comment_by}", userName)
                .Replace("{priority}", priority)
                .Replace("{status}", status)
                .Replace("{datetime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            // 6️⃣ Email Subject
            string subject = $"New Comment on Ticket {ticketId} - {title}";

            // 7️⃣ Determine email recipient (same logic as ticket creation)
            string deployMode = ConfigurationManager.AppSettings["DEPLOYMODE"]?.Trim().ToUpper() ?? "TEST";
            string testEmail = ConfigurationManager.AppSettings["TESTEMAIL"]?.Trim();
            string adminEmail = "";

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

            // 8️⃣ Send Email
            string emailResult = !string.IsNullOrEmpty(adminEmail)
                ? cCommon.SendEmail(adminEmail, subject, htmlBody, "", null)
                : "NO_EMAIL_FOUND";

            // 9️⃣ Return response
            if (emailResult == "SENT" || emailResult == "NO_EMAIL_FOUND")
                return Json(new { success = true, message = "Comment added successfully and email notification sent!" });
            else
                return Json(new { success = true, message = "Comment added successfully (email failed to send)." });
        }




        public JsonResult GetComments(string ticketId)
        {
            TicketSystem oTicket = new TicketSystem();
            DataTable dt = oTicket.GetComments(ticketId);

            var comments = dt.AsEnumerable().Select(row => new
            {
                CommentID = row["CommentID"].ToString(),
                UserName = row["UserName"].ToString(),
                CommentText = row["Comment_Text"].ToString(),
                CreatedAt = Convert.ToDateTime(row["Created_At"]).ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            return Json(new { success = true, comments = comments }, JsonRequestBehavior.AllowGet);
        }




    }


}
