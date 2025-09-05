using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data.Common;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
using IP.Classess;

public sealed class cDAL
{
    public string Parameter { get; set; }
    public object Value { get; set; }
    public enum QueryType
    {
        Insert,
        Update,
        Delete
    }

    public enum ConnectionType
    {
        INIT,
        ACTIVE     
    }

    private DbProviderFactory _dbFactory;
    private DbTransaction _transaction;

    private List<cQuery> _batchQueries;

    public int QueryIndex { get; set; }
    public string InternalErrMsg { get; set; }
    public bool HasErrors { get; set; }
    public string ErrMessage { get; set; }
    public DataSet resultSet;
    public static string QueryExecTime { get; set; }
    public static string RowsFetched { get; set; }
    cLog oLog;
    #region "Initialization Routines"

    private readonly ConnectionType _connectionType;

    //public cDAL(ConnectionType conType = ConnectionType.PIX)
    public cDAL(ConnectionType connectionType = ConnectionType.INIT)
    {
        _dbFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        _connectionType = connectionType;
        this.Initialize();
    }



    private DbConnection CreateConnection()
    {
        DbConnection newConnection;
        string connectionString = string.Empty;

        if (_connectionType.ToString() == "INIT")
        {
            connectionString = HttpContext.Current.Session["CONN_INIT"].ToString();
        }
        else
        {
            connectionString = GetConnectionString();
            connectionString = BasicEncrypt.Instance.Encrypt(connectionString);
        }



        newConnection = _dbFactory.CreateConnection();
        
        newConnection.ConnectionString = BasicEncrypt.Instance.Decrypt(connectionString);

        try
        {
            newConnection.Open();
        }
        catch (Exception ex)
        {
            HasErrors = true;
            ErrMessage = ex.Message;
            oLog = new cLog();
            oLog.RecordError(ex.Message, ex.StackTrace, newConnection.ConnectionString);
        }
        return newConnection;
    }



    private string GetConnectionString()
    {
        cDAL oDAL = new cDAL(cDAL.ConnectionType.INIT);
        string connectionString = string.Empty;
        string DefaultDB = "";
        DefaultDB =   HttpContext.Current.Session["DefaultDB"].ToString();
        DataTable dt = new DataTable();
        string sql = "Select ConValue from [SRM].[zConStr] Where ConType = '" + DefaultDB.ToUpper() + "'";

        string connection = HttpContext.Current.Session["CONN_INIT"].ToString();
        dt = oDAL.GetINITData(BasicEncrypt.Instance.Decrypt(connection).ToString(), sql);
        if (dt.Rows.Count > 0)
        {
            connectionString = dt.Rows[0]["ConValue"].ToString();
        }

        return connectionString;
    }

    public DataTable GetINITData(string connectionString, string query)
    {
        DataTable dataTable = new DataTable();

        // Create a SqlConnection
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                // Open the connection
                connection.Open();

                // Create a SqlCommand
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Create a SqlDataAdapter
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        // Fill the DataTable with the result of the query
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        return dataTable;
    }

    public void Initialize()
    {
        HasErrors = false;
        ErrMessage = string.Empty;

        _batchQueries = new List<cQuery>();
    }

    public DbParameter CreateParameter(string parameterName, object parameterValue, DbType parameterDataType = DbType.String)
    {
        DbParameter _parameter = _dbFactory.CreateParameter();

        _parameter.ParameterName = parameterName;
        _parameter.Value = parameterValue;
        _parameter.DbType = parameterDataType;

        return _parameter;
    }

    #endregion

    #region "Data Retrieval Routines"
    public DataTable GetData(string commandText, Dictionary<string, object> parameters)
    {
        DataTable _resultSet = new DataTable();

        DbDataAdapter _dataAdapter = _dbFactory.CreateDataAdapter();
        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                _dataAdapter.SelectCommand = _dbFactory.CreateCommand();
                _dataAdapter.SelectCommand.Connection = _connection;
                _dataAdapter.SelectCommand.CommandText = commandText;

                // parameterized queries avoid sql injection
                foreach (var p in parameters)
                {
                    var newP = _dataAdapter.SelectCommand.CreateParameter();
                    newP.ParameterName = p.Key;
                    newP.Value = p.Value;
                    _dataAdapter.SelectCommand.Parameters.Add(newP);
                }

                _dataAdapter.Fill(_resultSet);
            }
            catch (DbException ex)
            {
                HasErrors = true;
                ErrMessage = ex.Message;

                if (_connection.State != ConnectionState.Open)
                {
                    oLog = new cLog();
                    oLog.RecordError(ex.Message, ex.StackTrace, _connection.ConnectionString);
                }
                else
                {
                    oLog = new cLog();
                    oLog.RecordError(ex.Message, ex.StackTrace, commandText);
                }
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
                _dataAdapter.Dispose();
            }
        }


        return _resultSet;
    }
    public DataTable GetData(string commandText)
    {
        DataTable _resultSet = new DataTable();
        DbDataAdapter _dataAdapter = _dbFactory.CreateDataAdapter();
        QueryExecTime = "00:00";
        RowsFetched = "0";
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                _dataAdapter.SelectCommand = _dbFactory.CreateCommand();
                _dataAdapter.SelectCommand.Connection = _connection;
                _dataAdapter.SelectCommand.CommandText = commandText;

                _dataAdapter.Fill(_resultSet);
                stopWatch.Stop();
                TimeSpan elapsed = stopWatch.Elapsed;
                QueryExecTime = $"{(int)elapsed.Minutes:00}:{elapsed.Seconds:00}";
                RowsFetched = _resultSet.Rows.Count.ToString();
            }
            catch (DbException ex)
            {
                HasErrors = true;
                ErrMessage = ex.Message;

                if (_connection.State != ConnectionState.Open)
                {
                    oLog = new cLog();
                    oLog.RecordError(ex.Message, ex.StackTrace, _connection.ConnectionString);
                }
                else
                {
                    oLog = new cLog();
                    oLog.RecordError(ex.Message, ex.StackTrace, commandText);
                }
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
                _dataAdapter.Dispose();
            }
        }

        return _resultSet;
    }

    public object GetObject(string commandText)
    {
        DbCommand command = _dbFactory.CreateCommand();

        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                command.CommandText = commandText;
                command.Connection = _connection;

                return command.ExecuteScalar();
            }
            catch (DbException ex)
            {
                HasErrors = true;
                ErrMessage = ex.Message;

                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, commandText);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();

            }
        }

        return null;
    }




    #endregion

    #region "Data Manipulation Routines"

    public void AddQuery(string commandText)
    {
        cQuery oQuery = new cQuery();
        oQuery.CommandText = commandText;

        _batchQueries.Add(oQuery);
    }

    public void Execute(string commandText)
    {
        DbCommand command = _dbFactory.CreateCommand();

        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                command.Connection = _connection;
                command.CommandText = commandText;

                command.ExecuteNonQuery();
            }
            catch (DbException ex)
            {
                HasErrors = true;
                InternalErrMsg = ex.Message;

                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, commandText);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();

            }
        }

    }

    public void ExecuteCommand(SqlCommand command)
    {
        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                // Cast DbConnection to SqlConnection
                SqlConnection sqlConnection = (SqlConnection)_connection;
                command.Connection = sqlConnection;

                command.ExecuteNonQuery();
            }
            catch (DbException ex)
            {
                HasErrors = true;
                InternalErrMsg = ex.Message;

                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, command.CommandText);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }
        }
    }

    public void Commit()
    {
        DbCommand command = _dbFactory.CreateCommand();

        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                _transaction = _connection.BeginTransaction();
                command.Connection = _connection;
                command.Transaction = _transaction;

                if (_batchQueries.Count > 0)
                {

                    foreach (cQuery oParam in _batchQueries)
                    {
                        QueryIndex = _batchQueries.IndexOf(oParam);

                        command.CommandText = oParam.CommandText;
                        command.Parameters.Clear();
                        if ((oParam.Parameters != null))
                            command.Parameters.AddRange(oParam.Parameters);

                        command.ExecuteNonQuery();
                    }

                    _transaction.Commit();

                    this.Initialize();
                }
            }
            catch (DbException ex)
            {
                if (_transaction != null)
                    _transaction.Rollback();

                HasErrors = true;
                InternalErrMsg = ex.Message;
                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, _batchQueries[QueryIndex].CommandText);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();

            }
        }
    }

    public bool ExecuteProcedure(string procedureName, List<DbParameter> parameters)
    {
        DbCommand command = _dbFactory.CreateCommand();

        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = _connection;

                if (parameters.Count > 0)
                    command.Parameters.AddRange(parameters.ToArray());

                DbDataReader reader = command.ExecuteReader();
                resultSet = new DataSet();
                resultSet.Load(reader, LoadOption.OverwriteChanges, "");
                return true;
            }
            catch (DbException ex)
            {
                HasErrors = true;
                ErrMessage = ex.Message;

                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, command.CommandText);
            }
            finally
            {
                command.Parameters.Clear();
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();

            }
        }
        return false;
    }

    public bool ExecuteProcedureWithDS(string procedureName, List<DbParameter> parameters)
    {
        DbCommand command = _dbFactory.CreateCommand();

        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = _connection;

                if (parameters.Count > 0)
                    command.Parameters.AddRange(parameters.ToArray());

                DbDataAdapter adapter = _dbFactory.CreateDataAdapter();
                adapter.SelectCommand = command;
                resultSet = new DataSet();
                adapter.Fill(resultSet);

                return true;
            }
            catch (DbException ex)
            {
                HasErrors = true;
                ErrMessage = ex.Message;

                oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, command.CommandText);
            }
            finally
            {
                command.Parameters.Clear();
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();

            }
        }

        return false;
    }

    public DataSet GetDataSet(string commandText)
    {
        DataSet _resultSet = new DataSet();
        DbDataAdapter _dataAdapter = _dbFactory.CreateDataAdapter();

        using (var _connection = CreateConnection())
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                _dataAdapter.SelectCommand = _dbFactory.CreateCommand();
                _dataAdapter.SelectCommand.Connection = _connection;
                _dataAdapter.SelectCommand.CommandText = commandText;
                //_dataAdapter.Select
                //Command.CommandTimeout = 5;

                _dataAdapter.Fill(_resultSet);
            }
            catch (DbException ex)
            {
                HasErrors = true;
                ErrMessage = ex.Message;

                if (_connection.State != ConnectionState.Open)
                {
                    oLog = new cLog();
                    oLog.RecordError(ex.Message, ex.StackTrace, _connection.ConnectionString);
                }
                else
                {
                    oLog = new cLog();
                    oLog.RecordError(ex.Message, ex.StackTrace, commandText);
                }
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
                _dataAdapter.Dispose();
            }
        }

        return _resultSet;
    }

    internal DataTable GetData(string query, SqlParameter[] sqlParameters)
    {
        throw new NotImplementedException();
    }


    #endregion
}

public class cQuery
{
    public string CommandText { get; set; }
    public DbParameter[] Parameters { get; set; }
}