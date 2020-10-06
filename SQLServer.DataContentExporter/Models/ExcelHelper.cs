using System;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

public sealed class ExcelHelper
{
    private const string OLEDB_Engine_12 = "Microsoft.ACE.OLEDB.12.0";
    private const string OLEDB_Engine_4 = "Microsoft.Jet.OLEDB.4.0";

    static public  string GetConnectionString(string fullFileName, string fileExt)
    {
        Dictionary<string, string> props = new Dictionary<string, string>();

        if (fileExt.Equals(".xls"))//for 97-03 Excel file
        {
           // XLS - Excel 2003 and Older
            props["Provider"] = "Microsoft.Jet.OLEDB.4.0";
            props["Extended Properties"] = "Excel 8.0";
            props["Data Source"] = fullFileName.Trim();
        }
        else if (fileExt.Equals(".xlsx"))  //for 2007 Excel file
        {
            // XLSX - Excel 2007, 2010, 2012, 2013
            props["Provider"] = "Microsoft.ACE.OLEDB.12.0";
            props["Extended Properties"] = "\"Excel 12.0 XML\"";
            props["Data Source"] = fullFileName.Trim();
        }
        else if (fileExt.Equals(".csv"))
        {
            //connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Documents;Extended Properties=\"Text;HDR=Yes;FORMAT=Delimited\"";
            props["Provider"] = "Microsoft.ACE.OLEDB.4.0";
            props["Extended Properties"] = "\"Text;HDR=YES;FMT=Delimited\"";
            props["Data Source"] = fullFileName.Trim();
        }


        StringBuilder sb = new StringBuilder();

        foreach (KeyValuePair<string, string> prop in props)
        {
            sb.Append(prop.Key);
            sb.Append('=');
            sb.Append(prop.Value);
            sb.Append(';');
        }

        return sb.ToString();
    }
    public static DataSet GetDataTableFromExcelFile(string fullFileName, string fileExt)
    {
        DataSet ds = new DataSet();

        if (fileExt.Equals(".csv"))
        {
            ds = ConvertCSVtoDataTable(fullFileName,fileExt);
        }
        else
        {
            string connectionString = GetConnectionString(fullFileName, fileExt);

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = conn;

                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                // Loop through all Sheets to get data
                foreach (DataRow dr in dtSheet.Rows)
                {
                    string wksSheetName = dr["TABLE_NAME"].ToString();

                    if (!wksSheetName.EndsWith("$"))
                        continue;

                    // Get all rows from the Sheet
                    cmd.CommandText = "SELECT * FROM [" + wksSheetName + "]";

                    DataTable dt = new DataTable();
                    dt.TableName = wksSheetName;

                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);

                    ds.Tables.Add(dt);
                }

                cmd = null;
                conn.Close();
            }
        }
        if ((ds == null) || (ds.Tables.Count == 0))
            return null;
        else
            return ds;
    }

    public static DataSet ConvertCSVtoDataTable(string strFilePath, string strExt)
    {
        DataSet ds = new DataSet();
        StreamReader sr = new StreamReader(strFilePath);
        string[] headers = sr.ReadLine().Split(',');
        DataTable dt = new DataTable();
        FileInfo systemInfo = new FileInfo(strFilePath);

        dt.TableName = systemInfo.Name.Replace(strExt, "");

        foreach (string header in headers)
        {
            dt.Columns.Add(header);
        }
        while (!sr.EndOfStream)
        {
            string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            DataRow dr = dt.NewRow();
            for (int i = 0; i < headers.Length; i++)
            {
                dr[i] = rows[i];
            }
            dt.Rows.Add(dr);
        }

        ds.Tables.Add(dt);

        sr.Close();

        sr = null;

        return ds;
    }

    private static DataTable GetDataTabletFromCSVFile(string csv_file_path)
    {
        DataTable csvData = new DataTable();

        try
        {

            using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
            {
                csvReader.SetDelimiters(new string[] { "," });
                csvReader.HasFieldsEnclosedInQuotes = true;
                string[] colFields = csvReader.ReadFields();
                foreach (string column in colFields)
                {
                    DataColumn datecolumn = new DataColumn(column);
                    datecolumn.AllowDBNull = true;
                    csvData.Columns.Add(datecolumn);
                }

                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    //Making empty value as null
                    for (int i = 0; i < fieldData.Length; i++)
                    {
                        if (fieldData[i] == "")
                        {
                            fieldData[i] = null;
                        }
                    }
                    csvData.Rows.Add(fieldData);
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return csvData;
    }
    /// <summary>
    /// This method checks if the user entered sheetName exists in the Schema Table
    /// </summary>
    /// <param name="sheetName">Sheet name to be verified</param>
    /// <param name="dtSchema">schema table </param>
    private static bool CheckIfSheetNameExists(string sheetName, DataTable dtSchema)
    {
        foreach (DataRow dataRow in dtSchema.Rows)
        {
            if (sheetName == dataRow["TABLE_NAME"].ToString())
            {
                return true;
            }
        }
        return false;
    }


}

