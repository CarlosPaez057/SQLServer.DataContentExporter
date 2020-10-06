using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace SQLServer.DataContentExporter.Models
{
    public static  class DataToJson
    {
        public static IEnumerable<Dictionary<string, object>> ToDictionary(this DataTable table)
        {
            string[] columns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            IEnumerable<Dictionary<string, object>> result = table.Rows.Cast<DataRow>()
                    .Select(dr => columns.ToDictionary(c => c, c => dr[c]));
            return result;
        }

        public static string ToJson(this object value)
        {
            if (value == null) return null;

            return System.Text.Json.JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// To json generic implementation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T value)
        {
            if (value == null) return null;

            return System.Text.Json.JsonSerializer.Serialize<T>(value);
        }


        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}
