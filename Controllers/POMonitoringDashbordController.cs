using IP.ActionFilters;
using PlusCP.Classess;
using PlusCP.Models;
using System;
using System.Data;
using System.Web.Mvc;

namespace PlusCP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class POMonitoringDashbordController : Controller
    {
        POMonitoringDashbord dashboard = new POMonitoringDashbord();

        public ActionResult Index()
        {
            return View();
        }

        // ✅ Query 1 - Supplier Remaining Summary
        public JsonResult GetRemainingSummary()
        {
            var result = dashboard.GetRemainingSummary();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // ✅ Query 2 - Buyer PO & Epicor Line Count
        public JsonResult GetBuyerEpicorSummary()
        {
            var result = dashboard.GetBuyerEpicorSummary();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // ✅ Query 3 - Completed by SRM & Epicor
        public JsonResult GetCompletionSummary()
        {
            var result = dashboard.GetCompletionSummary();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // ✅ Query 4 - On-Time vs Due
        public JsonResult GetOnTimeSummary()
        {
            var result = dashboard.GetOnTimeSummary();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
