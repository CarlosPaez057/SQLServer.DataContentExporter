using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SQLServer.DataContentExporter.ViewModels
{
    public class GroupViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int usersCount { get; set; }
    }
}
