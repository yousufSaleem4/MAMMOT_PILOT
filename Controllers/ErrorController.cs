using IP.Classess;
using PlusCP.Classess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlusCP.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        // Adjust your connection string as needed
        private string _connectionString = "";

        [HttpPost]
        public ActionResult SubmitErrorReport(string email, string message, string RecipientEmail)
        {
            // Read the setup connection string from the configuration file
            string fileName = ConfigurationManager.AppSettings["Key"];
            _connectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());
            string EmailResult = "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                return Json(new { success = false, message = "Email and message fields are required." });
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO ErrorReports (Email, Message) VALUES (@Email, @Message)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Message", message);
                        cmd.ExecuteNonQuery();
                    }
                }

                string htmlBody = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8' />
    <title>Error Report</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f6f9fc; padding: 20px; font-size: 16px; }}
        .card {{ max-width: 600px; box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075); border: 1px solid #e3e6f0; border-radius: 0.35rem; background-color: #ffffff; }}
        .card-header {{ background-color: #003b59; color: #fff; padding: 20px; text-align: center; border-top-left-radius: 0.35rem; border-top-right-radius: 0.35rem; }}
        .card-body {{ padding: 20px 40px; }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='card-header'><h2>COLLABLLY | Error Report</h2></div>
        <div class='card-body'>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Message:</strong> {message}</p>
        </div>
    </div>
</body>
</html>";

                DataTable dt = new DataTable();
                dt = GetEmailSMTPSetup();
                EmailResult = cCommon.SendEmailSupplier(dt, RecipientEmail, "Error Report", htmlBody, email, null);

                return Json(new { success = true, message = "Support request submitted successfully." });
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }

        public static DataTable GetEmailSMTPSetup()
        {
            string fileName = ConfigurationManager.AppSettings["Key"];
            string DBConnectionString = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());
            string sql = "Select * from [dbo].[EmailSetup] WHERE IsActive = 1 ";
            ExternalDAL oDAL = new ExternalDAL();
            DataTable dt = new DataTable();
            dt = oDAL.GetData(sql, DBConnectionString);
            return dt;
        }

        public string GetConnectionString()
        {


            // Read the setup connection string from the configuration file
            string fileName = ConfigurationManager.AppSettings["Key"];
            string setupConnection = BasicEncrypt.Instance.Decrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());

            // Variable to store the result connection string
            string connectionString = string.Empty;

            return connectionString;
        }
    }
}
