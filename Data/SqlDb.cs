using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ReactApp.Server.Library
{
    public class SqlDb
    {
        private readonly string strSqlConnection;


        public SqlDb(IConfiguration configuration)
        {
            strSqlConnection = configuration.GetConnectionString("Sqlconnection");

            if (string.IsNullOrEmpty(strSqlConnection))
            {
                throw new InvalidOperationException("Connection string 'Sqlconnection' not found in configuration.");
            }
        }

        public DataSet ExecuteStoreProc_GetDataSet(string storedProcedureName, params SqlParameter[] arrParam)
        {
            DataSet ds = new();
            using SqlConnection cnn = new(strSqlConnection);
            using SqlCommand cmd = new(storedProcedureName, cnn);
            using SqlDataAdapter da = new(cmd);

            cmd.CommandType = CommandType.StoredProcedure;

            if (arrParam != null)
            {
                foreach (var param in arrParam)
                    cmd.Parameters.Add(param);
            }

            cnn.Open();
            da.Fill(ds);
            return ds;
        }

        public DataTable ExecuteStoreProc_GetTable(string storedProcedureName, params SqlParameter[] arrParam)
        {
            DataTable dt = new();
            using SqlConnection cnn = new(strSqlConnection);
            using SqlCommand cmd = new(storedProcedureName, cnn);
            using SqlDataAdapter da = new(cmd);

            cmd.CommandType = CommandType.StoredProcedure;

            if (arrParam != null)
            {
                foreach (var param in arrParam)
                    cmd.Parameters.Add(param);
            }

            cnn.Open();
            da.Fill(dt);
            return dt;
        }

        public DataTable ExecuteStoreProc_GetTable(string storedProcedureName)
        {
            return ExecuteStoreProc_GetTable(storedProcedureName, null);
        }

        public DataTable ExecuteNonQuery_GetTable(string strSqlQry)
        {
            DataTable dt = new();
            using SqlConnection cnn = new(strSqlConnection);
            using SqlCommand cmd = new(strSqlQry, cnn);
            using SqlDataAdapter da = new(cmd);

            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 600;

            cnn.Open();
            da.Fill(dt);
            return dt;
        }

        public int ExecuteStoreProc(string storedProcedureName, Hashtable sparam)
        {
            using SqlConnection cnn = new(strSqlConnection);
            SqlCommand cmd = new(storedProcedureName, cnn);

            cmd.CommandType = CommandType.StoredProcedure;
            AddParam(ref cmd, ref sparam);

            cnn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int ExecuteStoreProc(string storedProcedureName, Param sparam)
        {
            using SqlConnection cnn = new(strSqlConnection);
            SqlCommand cmd = new(storedProcedureName, cnn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 600;
            AddParam(ref cmd, ref sparam);

            cnn.Open();
            int rowseff = cmd.ExecuteNonQuery();
            SetParam(ref cmd, ref sparam);
            return rowseff;
        }

        // Utility methods
        private void AddParam(ref SqlCommand cmd, ref Hashtable paramTable)
        {
            foreach (DictionaryEntry entry in paramTable)
            {
                cmd.Parameters.Add(new SqlParameter(entry.Key.ToString(), entry.Value));
            }
        }

        private void AddParam(ref SqlCommand cmd, ref Param paramTable)
        {
            foreach (var kvp in paramTable.GetParams())
            {
                object value = kvp.Value ?? "";
                SqlParameter param = new(kvp.Key, value);

                param.SqlDbType = value switch
                {
                    DateTime => SqlDbType.DateTime,
                    string => SqlDbType.VarChar,
                    decimal => SqlDbType.Decimal,
                    int => SqlDbType.Int,
                    bool => SqlDbType.Bit,
                    _ => SqlDbType.VarChar
                };

                if (kvp.Key.StartsWith("O") || kvp.Key.StartsWith("@O", StringComparison.OrdinalIgnoreCase))
                {
                    param.Direction = ParameterDirection.Output;
                    if (value is string) param.Size = 200;
                }
                else
                {
                    param.Direction = ParameterDirection.Input;
                }

                cmd.Parameters.Add(param);
            }
        }

        private void SetParam(ref SqlCommand cmd, ref Param sparam)
        {
            foreach (SqlParameter param in cmd.Parameters)
            {
                if (param.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                {
                    sparam.SetValue(param.ParameterName, param.Value);
                }
            }
        }
    }

    public class Param
    {
        private readonly Dictionary<string, object> _params = new();

        public void Add(object key, object value) => _params[key.ToString()] = value;
        public int Count() => _params.Count;
        public Dictionary<string, object> GetParams() => _params;
        public object GetValue(string key) => _params[key];
        public void SetValue(string key, object value) => _params[key] = value;
        public void Clear() => _params.Clear();
    }

}