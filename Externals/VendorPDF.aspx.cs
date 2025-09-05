using IP.Classess;
using Newtonsoft.Json.Linq;
using PlusCP.Classess;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PlusCP.Externals
{
    public partial class VendorPDF : System.Web.UI.Page
    {
        public string DBConnectionString { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    string PONo = "";
                    string ConnectionType = "";
                    PONo = Request.QueryString["PONo"].ToString();
                    ConnectionType = Request.QueryString["Connection"].ToString();
                    string DeployMode = ConfigurationManager.AppSettings["DEPLOYMODE"];

                    DataTable dtURLPDF = GetEmailURL(ConnectionType, "GETPDF"); 
                    string URLPDF = dtURLPDF.Rows[0]["URL"].ToString();
                    string PageURLPDF = dtURLPDF.Rows[0]["PageURL"].ToString();
                    string userNamePDF = dtURLPDF.Rows[0]["UserName"].ToString();
                    string passwordPDF = dtURLPDF.Rows[0]["Password"].ToString();
                    passwordPDF = BasicEncrypt.Instance.Decrypt(passwordPDF.Trim());
                    var finalURL = URLPDF + PageURLPDF + "?PONum=" + PONo;
                    var clientPDF = new RestClient(URLPDF);
                    var requestPDF = new RestRequest(finalURL, Method.Get);
                    // Add basic authentication header
                    requestPDF.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(userNamePDF + ":" + passwordPDF)));
                    requestPDF.AddHeader("api-key", dtURLPDF.Rows[0]["TokenKey"].ToString());

                    var responsePDF = clientPDF.Execute(requestPDF);

                    if (responsePDF.StatusCode == HttpStatusCode.OK)
                    {
                        byte[] pdfBytes = responsePDF.RawBytes;
                        string base64String = Convert.ToBase64String(pdfBytes);
                        string pdfString = Encoding.UTF8.GetString(pdfBytes);
                        JObject jsonObject = JObject.Parse(pdfString);

                        // Extract the value associated with the key "SysRptLst_RptData"
                        string base64Value = (string)jsonObject["value"][0]["SysRptLst_RptData"];
                        byte[] pdfBytearr = Convert.FromBase64String(base64Value);

                        Response.Clear();
                        Response.ContentType = "application/pdf";
                        Response.AddHeader("content-disposition", "inline;filename=YourFileName.pdf");

                        // Write the byte array containing the PDF to the output stream
                        Response.BinaryWrite(pdfBytearr);

                        // End the response
                        HttpContext.Current.ApplicationInstance.CompleteRequest();

                    }

                }
                catch (Exception ex)
                {
                    // Exception occurred, return error message
                    //return Content($"Error: {ex.Message}");
                }
            }
        }

        public DataTable GetEmailURL(string DeployMode, string ViewName)
        {
            ExternalDAL oDAL = new ExternalDAL();
            string sql = "SELECT * FROM [dbo].[URLSetup] WHERE DeployMode = '" + DeployMode + "' AND ViewName = '" + ViewName + "' Order By id ";
            string fileName = ConfigurationManager.AppSettings["Key"];
            DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());
            //DBConnectionString = GetConnectionString(DeployMode);
            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql, DBConnectionString);
            return dt;
        }

        public string GetConnectionString(string ConType)
        {
            // SQL query to get the connection value from the database
            string sql = @"SELECT [ConValue] FROM [SRM].[zConStr] WHERE ConType = @ConType";

            // Read the setup connection string from the configuration file
            string fileName = ConfigurationManager.AppSettings["Key"];
            string setupConnection = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());

            // Variable to store the result connection string
            string connectionString = string.Empty;

            // Use ADO.NET to execute the query
            using (SqlConnection conn = new SqlConnection(setupConnection))
            {
                try
                {
                    conn.Open(); // Open the connection

                    // Create the SqlCommand and add the parameter to avoid SQL injection
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ConType", ConType);

                        // Execute the query and get the result using SqlDataReader
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Retrieve the ConValue column from the result
                                connectionString = reader["ConValue"].ToString();
                                DBConnectionString = connectionString;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    conn.Close(); // Ensure the connection is closed after usage
                }
            }

            return connectionString;
        }
        public string MakeJsonBody(string PONo)
        {
            cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string Json = "";
            string sql = "Select APIJson from [dbo].[APIJson]";
            DataTable dt = new DataTable();

            dt = oDAL.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                Json = dt.Rows[0]["APIJson"].ToString();
            }

            JObject jsonObject = JObject.Parse(Json);

            // Change the value of "PONum" dynamically
            jsonObject["ds"]["POFormParam"][0]["PONum"] = PONo;
            // Convert the modified JSON object back to string
            string modifiedJsonString = jsonObject.ToString();

            return modifiedJsonString;

        }
    }
}