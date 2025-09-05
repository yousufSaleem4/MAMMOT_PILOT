using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PlusCP.Classess
{
    public class ExternalDAL
    {
        SqlConnection oSqlConnection;
        SqlCommand oSqlCommand;
        SqlDataAdapter oSqlDataAdapter;
        public string InternalErrMsg { get; set; }
        public bool HasErrors { get; set; }
        public DataTable GetData(string query, string ConnStr) //Get connection
        {
            DataTable dt = new DataTable();
            oSqlConnection = new SqlConnection(ConnStr);
            oSqlConnection.Open();
            oSqlCommand = new SqlCommand(query, oSqlConnection);
            oSqlDataAdapter = new SqlDataAdapter(oSqlCommand);
            oSqlDataAdapter.Fill(dt);
            oSqlConnection.Dispose();
            return dt;
        }
        public object GetObject(string query, string ConnStr)
        {
            object result = null;

            // Using 'using' statements to ensure proper disposal of resources
            try
            {
                using (SqlConnection oSqlConnection = new SqlConnection(ConnStr))
                {
                    oSqlConnection.Open();

                    using (SqlCommand oSqlCommand = new SqlCommand(query, oSqlConnection))
                    {
                        // Execute the query and return the scalar result
                        result = oSqlCommand.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions or log them as needed
                Console.WriteLine("An error occurred: " + ex.Message);
                // Optionally, you could throw the exception to be handled by the caller
                throw;
            }

            return result; // Return the scalar value (could be null if no value is returned)
        }

        public Boolean Execute(string query, string ConnStr)// CRUD Functions
        {
            try
            {
                oSqlConnection = new SqlConnection(ConnStr);
                oSqlConnection.Open();
                oSqlCommand = new SqlCommand(query, oSqlConnection);
                if (oSqlCommand.ExecuteNonQuery() > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                
                return false;
            }
        }

        public void ExecuteCommand(SqlCommand command, string connectionString)
        {
            cLog oLog;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection if it's not already open
                    if (sqlConnection.State == ConnectionState.Closed)
                        sqlConnection.Open();

                    // Assign the connection to the command
                    command.Connection = sqlConnection;

                    // Execute the command
                    command.ExecuteNonQuery();
                }
                catch (DbException ex)
                {
                    HasErrors = true;
                    InternalErrMsg = ex.Message;

                    // Log the error
                    oLog = new cLog();
                    oLog.RecordError(ex.Message, ex.StackTrace, command.CommandText);
                }
                finally
                {
                    // Ensure the connection is closed after the operation
                    if (sqlConnection.State == ConnectionState.Open)
                        sqlConnection.Close();
                }
            }
        }

    }
}