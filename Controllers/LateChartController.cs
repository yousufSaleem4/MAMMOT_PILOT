using IP.ActionFilters;
using Newtonsoft.Json;
using PlusCP.Classess;
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
    public class LateChartController : Controller
    {
        // GET: LateChart
        LateChart oLateChart = new LateChart();
        public ActionResult Index()
        {
            oLateChart = new LateChart();
            return View();
           
        }

        public JsonResult SupplierAllPerformance()
        {
            string menuTitle = string.Empty;
            string RptCode;
            DataTable dt = new DataTable();
            oLateChart = new LateChart();
            oLateChart.SupplierAllPerformance();
            var jsonResult = Json(oLateChart, JsonRequestBehavior.AllowGet);
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


        public JsonResult GetSupplierQty()
        {
            string menuTitle = string.Empty;
            string RptCode;
            DataTable dt = new DataTable();
            oLateChart = new LateChart();
            dt = oLateChart.GetSupplierQty();
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

    

        public JsonResult GetDDLSupplier()
        {
            string menuTitle = string.Empty;
            string RptCode;
            DataTable dt = new DataTable();
            NewPOCommon oPOCommon = new NewPOCommon();
            dt = oPOCommon.GetPOListFromAPI();
            oLateChart = new LateChart();
            dt = oLateChart.GetDDLSupplier(dt);
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