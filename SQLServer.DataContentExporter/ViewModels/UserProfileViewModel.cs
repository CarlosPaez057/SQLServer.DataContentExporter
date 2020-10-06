using System;
using System.Collections.Generic;

namespace SQLServer.DataContentExporter.ViewModels
{
    public class UserProfileViewModel
    {
        /// <summary>
        /// Stores UserId
        /// </summary>
        public string id { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime? lastLogin { get; set; }
        public bool isActive { get; set; }
        public string customerName { get; set; }
        public bool isEmailAllowed { get; set; }
        public bool isEmailConfirmed { get; set; }
        public List<GroupViewModel> groups { get; set; }
    }
}
