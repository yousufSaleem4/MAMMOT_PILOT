﻿using IP.Classess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlusCP.Classess;
using PlusCP.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static PlusCP.Models.DashboardModel;

namespace PlusCP.Controllers
{
    [OutputCache(Duration = 1200)]
    public class DashboardController : Controller
    {
        // GET: Dashboard
        DataTable dt = new DataTable();
        public ActionResult Index()
        {
            if (cCommon.IsSessionExpired())
            {
                return RedirectToAction("Logout", "Home");
            }

            DashboardModel oDB = new DashboardModel();
            oDB.GetCardWidgets();
            var dtSuppliers = GetSuppliers();
            ViewBag.ddlSupplier = cCommon.ToDropDownList(dtSuppliers, "ID", "NAME", Session["DefaultProgram"].ToString(), "ID");
            return View(oDB);
        }

        public DataTable GetSuppliers()
        {
            DataTable dt = new DataTable();
            cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
            string sql = "SELECT DISTINCT(vendor_vendorID) AS ID, vendor_name AS [NAME] FROM [dbo].[PODetail] WHERE Vendor_Name IS NOT NULL ORDER BY Vendor_Name";
            dt = oDAL.GetData(sql);

            // Add "Select from List" row at the top
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow newRow = dt.NewRow();
                newRow["ID"] = "";
                newRow["NAME"] = "Select from List";
                dt.Rows.InsertAt(newRow, 0);
            }

            return dt;
        }
        [HttpGet]
        public JsonResult GetWidgetList()
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            DashboardModel oModel = new DashboardModel();
            oModel.Get_Widget_List();
            response["lst_widgets"] = oModel.lst_widgets;
            response["IsValid"] = true;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetWidgetData() // Widgets Count 
        {
            string ConnctionType = Session["DefaultDB"].ToString();
            string Email = Session["Email"].ToString();
            string userType = Session["UserType"].ToString();
            NewPOCommon oPOCommon = new NewPOCommon();
            DataTable dt = new DataTable();
            dt = oPOCommon.GetCounts();
            DashboardModel oModel = new DashboardModel();

            if (ConnctionType.ToUpper() == "PROD" || ConnctionType.ToUpper() == "PILOT")
            {
                oModel.DashboardCount(dt);
            }
            else
            {
                oModel.DashboardCountDemo(dt);
            }
            //oModel.GetCardWidgets();
            var jsonResult = Json(oModel, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;


        }


        [HttpPost]
        public JsonResult SavePOWidgets(string WIdgetTitle, string SupplierId, string SupplierName, string WidgetDesc, string NoOfDays, string[] POListId, string items)
        {
            // Deserialize JSON into a Dictionary with object values
            var dataList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(items);

            DashboardModel oModel = new DashboardModel();
            oModel.SavePOWidgets(WIdgetTitle, SupplierId, SupplierName, WidgetDesc, NoOfDays, POListId, dataList); // yahan dataList bhejna hai
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult EditPOWidgets(string WidgetEditId, string WIdgetTitle, string SupplierId, string SupplierName, string WidgetDesc, string NoOfDays, string[] POListId)
        {

            DashboardModel oModel = new DashboardModel();
            oModel.EditPOWidgets(WidgetEditId, WIdgetTitle, SupplierId, SupplierName, WidgetDesc, NoOfDays, POListId);
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeletePOWidgets(string WidgetDeleteId)
        {

            DashboardModel oModel = new DashboardModel();
            oModel.DeletePOWidgets(WidgetDeleteId);
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult OpenPOWidget(string WidgetId)
        //{
        //    DashboardModel oDB = new DashboardModel();

        //    return RedirectToAction("Index", "NewPO", new { WidgetId = WidgetId });
        //}

        [HttpGet]
        public JsonResult GetPOList(string supplierId, string noOfDays)
        {
            DashboardModel oDB = new DashboardModel();
            oDB.GetPOList(supplierId, noOfDays);
            return Json(oDB, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPOWidgetsById(string WidgetEditId)
        {
            DashboardModel oDB = new DashboardModel();
            oDB.GetWidgetById(WidgetEditId);
            return Json(oDB, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult LoadWidget(string widget_id, string widget_type)
        {
            DashboardModel oModel = new DashboardModel();
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (widget_type.ToLower() == "count")
            {
                oModel.Get_Count_Widget(widget_id);
                response["widget_desc"] = oModel.widget_desc;
                response["widget_count_count"] = oModel.widget_count_count;
                response["widget_count_bg"] = oModel.widget_count_bg;
                response["widget_count_fraction"] = oModel.widget_count_fraction;
                response["widget_count_report_URL"] = oModel.widget_count_report_URL;
            }
            else if (widget_type.ToLower() == "table")
            {
                oModel.Get_Table_Widget(widget_id);
                response["lst_widget_table"] = oModel.lst_widget_table;
                response["widget_table_column_format"] = oModel.widget_table_column_format;
                response["widget_table_cols_count"] = oModel.widget_table_cols_count;

            }
            else if (widget_type.ToLower() == "list")
            {
                oModel.Get_List_Widget(widget_id);
                response["lst_widget_list"] = oModel.lst_widget_list;
            }
            else if (widget_type.ToLower() == "hyperlist")
            {
                oModel.Get_HyperList_Widget(widget_id);
                response["lst_widget_hyperlist"] = oModel.lst_widget_hyperlist;
            }
            else if (widget_type.ToLower() == "chart")
            {
                oModel.Get_Chart_Widget(widget_id);
                response["chart_data"] = oModel.chart_data;
                response["chart_title"] = oModel.chart_title;
                response["widget_count_count"] = oModel.widget_count_count;

            }
            else if (widget_type.ToLower() == "count_group")
            {
                oModel.Get_Group_Counts_Widget(widget_id);
                response["lst_widget_group_counts"] = oModel.lst_widget_group_counts;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveEmployeeWidgets(List<DashboardModel.Employee_Widgets> lst)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            DashboardModel oModel = new DashboardModel();
            oModel.Save_Employee_Widgets(lst);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmployeeWidgets()
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            DashboardModel oModel = new DashboardModel();

            oModel.Get_Employee_Widgets();

            response["lstWidgetsEmployee"] = oModel.lst_employee_widgets;
            response["IsValid"] = true;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetURLInfo(string link)
        {
            DashboardModel oModel = new DashboardModel();
            Dictionary<string, object> response = new Dictionary<string, object>();
            oModel.Get_Report_Info(link);
            response["URL"] = oModel.report_URL;
            response["is_internal"] = oModel.report_is_internal;
            response["code"] = oModel.report_code;
            response["title_short"] = oModel.report_title_short;
            response["designed_by"] = oModel.report_designed_by;
            response["target"] = oModel.report_target;

            return Json(response, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult BigTv()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DBIndex()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Samsung_Horizon()
        {

            return View();
        }

        [HttpGet]
        public ActionResult SamsungRefresh()
        {
            ViewBag.ReportTitle = "Samsung Refresh";
            DashboardModel oSamSung = new DashboardModel();
            oSamSung.getStations();
            oSamSung.getTotalCrnt();
            return View(oSamSung);
        }

        [HttpGet]
        public ActionResult SamsungHorizonChart()
        {
            DashboardModel oDashboard = new DashboardModel();
            bool success = oDashboard.getChart();
            if (success)
                return View(oDashboard);
            else
                return View();
        }
    }
}