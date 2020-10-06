using System.Collections.Generic;

namespace SQLServer.DataContentExporter.Models
{
    public class SQLInstance
    {
        public SQLInstance()
        {
            RepurposeColumns = new List<string>();
        }
        public string Customer { get; set; }
        public string ConnectionString { get; set; }
        public List<string> RepurposeColumns { get; set; }
    }
}
