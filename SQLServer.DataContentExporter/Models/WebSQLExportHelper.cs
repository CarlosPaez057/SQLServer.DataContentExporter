using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SQLServer.DataContentExporter.Models
{
    public abstract class WebSqlExportHelper : IDataContentExport
    {
        private SqlConnection _Conn;
        private string _FileExtension = "JSON";

        string _sqlQuery = "";
        string _sqlConnectionString = "";
        string _customerName = "";
        string _exportDir = "";
        List<string> _repurposeColumns = new List<string>();
        public string SQLQuery { get => _sqlQuery; set { _sqlQuery = value; } }
        public string SQLConnectionString { get => _sqlConnectionString; set { _sqlConnectionString = value; } }
        public string CustomerName { get => _customerName; set { _customerName = value; } }
        public string ExportDir { get => _exportDir; set { _exportDir = value; } }
        public List<string> RepurposeColumns { get => _repurposeColumns; set { _repurposeColumns = value; } }

        public string FileExtension
        {
            get { return _FileExtension; }
            set { _FileExtension = value; }
        }

        public WebSqlExportHelper (string customerName,string connectionString)
        {
            _customerName = customerName;
            _sqlConnectionString = (string)connectionString;

            _Conn = new SqlConnection(connectionString);

        }

        public DataTable getData()
        {
            string SqlString = this.SQLQuery;
            SqlDataAdapter sda = new SqlDataAdapter(SqlString, _Conn);
            DataTable dt = new DataTable();
            try
            {
                _Conn.Open();
                sda.Fill(dt);
            }
            catch (SqlException se)
            {
                switch (se.ErrorCode)
                {
                    case -2146232060:
                        break;
                    default:
                        throw se;
                }
                //              DBErLog.DbServLog(se, se.ToString());
            }
            finally
            {
                _Conn.Close();
            }
            return dt;
        }

        public abstract bool PullLegacyData();
    }
}

