using PlusCP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlusCP.Controllers
{
   
    public class POSchedulerController : Controller
    {
        POScheduler oPOScheduler = new POScheduler();
        // GET: POScheduler
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetList()
        {
            oPOScheduler = new POScheduler();
            //oUserRole.GetMnu();
            oPOScheduler.GetList();
            var jsonResult = Json(oPOScheduler, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}