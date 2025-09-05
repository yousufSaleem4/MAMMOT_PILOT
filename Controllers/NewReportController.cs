using IP.ActionFilters;
using Newtonsoft.Json;
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
    public class NewReportController : Controller
    {
        LateChart oLateChart = new LateChart();
        public ActionResult Index()
        {
            
            return View();
        }
        //public JsonResult GetLateChart()
        //{
        //    //string menuTitle = string.Empty;
        //    //string RptCode;
        //    //DataTable dt = new DataTable();
        //    //oLateChart = new LateChart();
        //    //dt = oLateChart.GetLateChart();
        //    //var json = JsonConvert.SerializeObject(dt);
        //    //var jsonResult = Json(json, JsonRequestBehavior.AllowGet);
        //    //jsonResult.MaxJsonLength = int.MaxValue;
        //    ////LOAD MRU & LOG QUERY
        //    //if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
        //    //{
        //    //    menuTitle = TempData["ReportTitle"] as string;
        //    //    RptCode = TempData["RptCode"].ToString();
        //    //    TempData.Keep();
        //    //    cLog oLog = new cLog();
        //    //    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
        //    //}
        //    return jsonResult;
        //}

        public JsonResult GetOnTimeChart()
        {
            string menuTitle = string.Empty;
            string RptCode;
            DataTable dt = new DataTable();
            oLateChart = new LateChart();
            dt = oLateChart.GetOnTimeChart();
            var json = JsonConvert.SerializeObject(dt);
            var jsonResult = Json(json, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetOnTimeVendors()
        {
            string menuTitle = string.Empty;
            string RptCode;
            DataTable dt = new DataTable();
            oLateChart = new LateChart();
            dt = oLateChart.GetOnTimeVendors();
            var json = JsonConvert.SerializeObject(dt);
            var jsonResult = Json(json, JsonRequestBehavior.AllowGet);
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
    }
}