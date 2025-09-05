using IP.ActionFilters;
using PlusCP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlusCP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    [Serializable]
    public class UserRoleXController : Controller
    {
        // GET: UserRoleX

        UserRoleX oUserRole = new UserRoleX();
        public ActionResult Index()
        {
            ViewBag.UserName = Session["FirstName"].ToString().Replace(Session["FirstName"].ToString().Substring(0, 1), Session["FirstName"].ToString().Substring(0, 1).ToUpper());

            ViewBag.isAdmin = Session["isAdmin"].ToString();
            if (Session["isAdmin"].ToString() == "True")
            {

                ViewBag.ddlVendor = cCommon.ToDropDownList((DataTable)Session["VendorList"], "ID", "NAME", Session["ProgramId"].ToString(), "ID");
            }
            else
            {
                ViewBag.ddlVendor = cCommon.ToDropDownList((DataTable)Session["VendorList"], "ProgramId", "Program", Session["ProgramId"].ToString(), "ProgramId");
            }
            oUserRole = new UserRoleX();
            // GetPrograms();

            return View();
        }

        public JsonResult GetList(string IscreateUser, string type)
        {
            oUserRole = new UserRoleX();
            //oUserRole.GetMnu();
            oUserRole.GetList(IscreateUser, type);
            var jsonResult = Json(oUserRole, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }





        public ActionResult GetPrograms()
        {
            UserRoleX oUserRole = new UserRoleX();
            oUserRole.GetProgram();
            oUserRole.GetMnu();
            return View(oUserRole);
        }

        public ActionResult GetMnu()
        {
            //UserRoleX oUserRole = new UserRoleX();
            //oUserRole.GetMnu();
            return View(oUserRole);
        }

        public ActionResult Detail(string ID, string FirstName, string LastName, string Password)
        {
            oUserRole = new UserRoleX();
            //for Programs
            //oUserRole.GetProgram();
            //oUserRole.GetMnu();
            oUserRole.GetDataById(ID);
            ViewBag.FirstName = FirstName;
            ViewBag.LastName = LastName;
            ViewBag.UserName = FirstName + ' ' + LastName;
            ViewBag.Password = Password;

            return View(oUserRole);
        }
        public JsonResult InsertAxs(string userId, string isAdmin, bool isActiveUserchange, string password)
        {

            oUserRole = new UserRoleX();
            oUserRole.SaveAxs(userId, isAdmin, isActiveUserchange, password);
           // string result = oUserRole.SendTempKey(password, userId);
            var jsonResult = Json(oUserRole, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public JsonResult SendTempKey(string userId, string password)
        {

            oUserRole = new UserRoleX();
            oUserRole.SaveTempKey(userId, password);
            string result = oUserRole.SendTempKey(password, userId);
            var jsonResult = Json(oUserRole, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public JsonResult SendInvite(string Type, string Email)
        {
            oUserRole = new UserRoleX();
            //oUserRole.GetMnu();
            oUserRole.SendInvite(Type, Email);
            var jsonResult = Json(oUserRole, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

    }
}