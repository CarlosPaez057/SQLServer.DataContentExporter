using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SQLServer.DataContentExporter.Models
{
    public interface IDataContentExport
    {
        string SQLQuery { get; set; }
        string FileExtension { get; set; }
        string ExportDir { get; set; }
        List<string> RepurposeColumns { get; set; }
        bool PullLegacyData();
    }
}
