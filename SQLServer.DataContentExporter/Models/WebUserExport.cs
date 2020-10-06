using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SQLServer.DataContentExporter.ViewModels;

namespace SQLServer.DataContentExporter.Models
{
    public class WebUserExport : WebSqlExportHelper
    {
        public WebUserExport(string customerName, string connectionString) : base(customerName,connectionString)
        {
        }

        public override bool PullLegacyData()
        {
            string groupColumnPattern = "Is(.*)",inactiveUserPattern="zzz(.*)";

            bool forceEmailFlag = true;

            bool retFlag = false;
            Dictionary<string, UserProfileViewModel> _userList = new Dictionary<string, UserProfileViewModel>();
            Dictionary<string, string> _userEmployee = new Dictionary<string, string>();
            Dictionary<string, UserProfileViewModel> _userEmailList = new Dictionary<string, UserProfileViewModel>();
            Dictionary<string, UserProfileViewModel> _specialEmployee = new Dictionary<string, UserProfileViewModel>();
            Dictionary<string, UserProfileViewModel> _missingEmailList = new Dictionary<string, UserProfileViewModel>();

            List<string> exportFields = new List<string>()
                            {
                                 "email", "firstName","lastName","lastLogin","isActive","Groups","EmployeeId","Licenses","AccessPrivileges"
                            };

            List<string> actionColumns = new List<string>()
            {
                "ISACTIVE"
            };

            Dictionary<string, string> userGroupColumns = new Dictionary<string, string>();

            List<string> queryList = new List<string>() {
                "SELECT COLUMN_NAME FROM  INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Employee'"
                
                ,"select  u.username,mem.userid as UserId,mem.email " +
                    " , case when trim(e.firstname) <> '' then e.firstname " +
                    " else case when patindex('%.%',u.username) > 0 then substring(u.username,1,patindex('%.%', u.username) - 1) else '' end " +
                    "  end as firstname " +
                    " , case when trim(e.lastname) <> '' then e.lastname " +
                    "  else case when patindex('%.%',u.username) > 0 then substring(u.username, patindex('%.%',u.username)+1,len(u.username)) else u.username end " +
                    " end as lastname " +
                    " ,mem.lastlogindate as lastLoginFromMem,e.isactive,e.employeeid,e.* " +
                    " from aspnet_membership mem " +
                    " inner join dbo.aspnet_users u on mem.userid = u.userid " +
                    " left join dbo.employee e on u.userid = e.userid " +
                    "  order by  u.username,e.firstname,e.lastname "
                ,"select * from Employee" 
            };

            try
            {
                SQLQuery = queryList[0];

                DataTable serverColumnData = this.getData();

                if (serverColumnData != null)
                {
                    foreach (DataRow rowItem in serverColumnData.Rows)
                    {
                        string fieldName = (string)rowItem["COLUMN_NAME"].ToString();

                        if (Regex.IsMatch(fieldName, groupColumnPattern))
                        { 
                            if (!userGroupColumns.ContainsKey(fieldName.ToUpper().Trim()))
                                userGroupColumns.Add(fieldName.ToUpper().Trim(), fieldName);
                        }
                    }  // each row

                }   // serverColumnData

            }
            catch
            { }


            try
            {
                SQLQuery = queryList[1];

                DataTable serverData = this.getData();

                if (serverData != null)
                {
                    foreach (DataRow rowItem in serverData.Rows)
                    {
                        string email = "";
                        string firstName = "";
                        string lastName = "";
                        string employeeID = "";
                        string customerName = "";

                        email = (string)rowItem["email"].ToString();
                        if (!String.IsNullOrEmpty(rowItem["firstName"].ToString()))
                            firstName = (string)rowItem["firstName"].ToString();
                        if (!String.IsNullOrEmpty(rowItem["lastName"].ToString()))
                            lastName = (string)rowItem["lastname"].ToString();
                        if (! String.IsNullOrEmpty(rowItem["employeeId"].ToString()))
                            employeeID = (string)rowItem["employeeId"].ToString();
                        if (!String.IsNullOrEmpty(rowItem["customerName"].ToString()))
                            customerName = (string)rowItem["customerName"].ToString();

                        if (!_userList.ContainsKey(email))
                        {
                            UserProfileViewModel userProfileViewModel = new UserProfileViewModel();

                            userProfileViewModel.customerName = customerName;
                            userProfileViewModel.email = email;
                            userProfileViewModel.firstName = firstName;
                            userProfileViewModel.lastName = lastName;

                            if (!String.IsNullOrEmpty(rowItem["isActive"].ToString()))
                            {
                                bool activeFlag = (bool)rowItem["isActive"];
                            userProfileViewModel.isActive = activeFlag;
                            }
                                
                            if (!String.IsNullOrEmpty(rowItem["lastLoginFromMem"].ToString()))
                                userProfileViewModel.lastLogin = (DateTime?)rowItem["lastLoginFromMem"];
                            userProfileViewModel.groups = new List<GroupViewModel>();

                        if (Regex.IsMatch(lastName, inactiveUserPattern))
                        {
                            lastName = lastName.Substring(3, lastName.Length - 3);

                            userProfileViewModel.lastName = lastName;
                        }
                            

                            foreach(KeyValuePair<string,string> groupItem in userGroupColumns )
                            {
                                if (!actionColumns.Contains(groupItem.Key))
                                {
                                    string groupCode = "";
                                    bool isInGroup = false;
                                    
                                    if (groupItem.Key.Length > 2)
                                    {
                                        groupCode = groupItem.Value.Substring(2, groupItem.Value.Length - 2);

                                        if (!String.IsNullOrEmpty(rowItem[groupItem.Value].ToString()))
                                            isInGroup = (bool)rowItem[groupItem.Value] ;

                                        if (isInGroup)
                                        {
                                            GroupViewModel groupViewModel = new GroupViewModel();

                                            groupViewModel.name = groupCode;

                                            userProfileViewModel.groups.Add(groupViewModel);

                                        }

                                    }

                                }
                            }
                            _userList.Add(email, userProfileViewModel);
                            _userEmployee.Add(email, employeeID);

                            if (!_userEmailList.ContainsKey(email.ToUpper().Trim()))
                                _userEmailList.Add(email.ToUpper().Trim(), userProfileViewModel);

                        }
                    }
                }

                retFlag = true;
            }
            catch
            {
            }

            try
            {
                SQLQuery = queryList[2];

                DataTable serverUserData = this.getData();

                if (serverUserData != null)
                {
                    foreach (DataRow rowItem in serverUserData.Rows)
                    {
                        UserProfileViewModel userProfileViewModel = new UserProfileViewModel();

                        string email = "";
                        string firstName = "";
                        string lastName = "";
                        string employeeID = "";
                        string userId = "";

                        if (!String.IsNullOrEmpty(rowItem["EmailAddress"].ToString()))
                            email = (string)rowItem["EmailAddress"].ToString();
                        else
                            email = "";

                        if (!String.IsNullOrEmpty(rowItem["firstName"].ToString()))
                            firstName = (string)rowItem["firstName"].ToString();
                        if (!String.IsNullOrEmpty(rowItem["lastName"].ToString()))
                            lastName = (string)rowItem["lastName"].ToString();
                        if (!String.IsNullOrEmpty(rowItem["employeeId"].ToString()))
                            employeeID = (string)rowItem["employeeId"].ToString();

                        if (!String.IsNullOrEmpty(rowItem["UserId"].ToString()))
                            userId = (string)rowItem["UserId"].ToString();
                        else
                        {
                            if (employeeID.Length > 0)
                                userId = "<<" + employeeID + ">>";  // to get data if UserId is missing
                        }
                        if (String.IsNullOrEmpty(email))
                        {
                            email = firstName + (String.IsNullOrEmpty(firstName) ? lastName : "_" + lastName)+ "@NotValid.com";

                            if (!forceEmailFlag)
                            {
                                if (!_missingEmailList.ContainsKey(email))
                                    _missingEmailList.Add(email, userProfileViewModel);
                            }
                        }

                        if (_userEmailList.ContainsKey(email.ToUpper().Trim()))
                        {
                            if ((userId.Length > 0) && (email.Length > 0) && (email.Contains("@")))
                                email = "~" + email;
                        }

                        userProfileViewModel.customerName = "CarlosPaezSample";
                        userProfileViewModel.email = email;
                        userProfileViewModel.firstName = firstName;
                        userProfileViewModel.lastName = lastName;
                        userProfileViewModel.isActive = false;
                        
                        if (!String.IsNullOrEmpty(rowItem["IsActive"].ToString()))
                            userProfileViewModel.isActive = ((bool)rowItem["IsActive"]);

                        if (email.Contains("@NotValid.com"))
                            userProfileViewModel.isActive = false;

                        userProfileViewModel.groups = new List<GroupViewModel>();

                        if (Regex.IsMatch(lastName, inactiveUserPattern))
                        {
                            lastName = lastName.Substring(3, lastName.Length - 3);

                            userProfileViewModel.lastName = lastName;
                        }

                        foreach (KeyValuePair<string, string> groupItem in userGroupColumns)
                        {
                            if (!actionColumns.Contains(groupItem.Key))
                            {
                                string groupCode = "";
                                bool isInGroup = false;

                                if (groupItem.Key.Length > 2)
                                {
                                    groupCode = groupItem.Value.Substring(2, groupItem.Value.Length - 2);
                                    isInGroup = (bool)rowItem[groupItem.Value];

                                    if (isInGroup)
                                    {
                                        GroupViewModel groupViewModel = new GroupViewModel();

                                        groupViewModel.name = groupCode;

                                        userProfileViewModel.groups.Add(groupViewModel);

                                    }

                                }

                            }


                                if (userId.Length > 0)
                                {
                                    if (!_userList.ContainsKey(userId))
                                    {
                                        _userList.Add(userId, userProfileViewModel);
                                        _userEmployee.Add(userId, employeeID);

                                        if (!_userEmailList.ContainsKey(email.ToUpper().Trim()))
                                            _userEmailList.Add(email.ToUpper().Trim(), userProfileViewModel);

                                    }
                                }
                                else
                                {

                                    if ((!_specialEmployee.ContainsKey(employeeID)))
                                    {
                                        _specialEmployee.Add(employeeID, userProfileViewModel);

                                        if (!_userEmailList.ContainsKey(email.ToUpper().Trim()))
                                            _userEmailList.Add(email.ToUpper().Trim(), userProfileViewModel);

                                    }
                                }
                        }
                    }
                }

                retFlag = true;
            }
            catch
            {
            }


            if (_userList.Count > 0)
            {
                switch (FileExtension.ToLower())
                {
                    case ".csv":
                        string outFileName = ExportDir  + CustomerName + "_Users_" + DateTime.Now.ToString("yyyyMMdd") + this.FileExtension;

                        FileInfo fileInfoObj = new FileInfo(outFileName);

                        DirectoryInfo directoryInfo = new DirectoryInfo(ExportDir);

                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }

                        if (fileInfoObj.Exists)
                        {
                            try
                            {
                                fileInfoObj.Delete();
                            }
                            catch
                            {
                                outFileName = outFileName.Replace(this.FileExtension, "") + "_" + DateTime.Now.ToString("hhmmss") + this.FileExtension;
                            }

                            fileInfoObj = null;
                        }

                        using (StreamWriter w = File.AppendText(outFileName))
                        {
                            int outCnt = 0;
                            StringBuilder outString = new StringBuilder();


                            foreach (KeyValuePair<string, UserProfileViewModel> dataItem in _userList)
                            {
                                

                                PropertyInfo[] properties = dataItem.Value.GetType().GetProperties();
                                string fieldPropType = "";

                                if (outCnt++ == 0)
                                {
                                    outString.Remove(0, outString.Length);
                                    foreach (var fld in exportFields)
                                    {
                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + fld);
                                    }
                                    w.Write(outString + "\n");
                                }

                                outString.Remove(0, outString.Length);

                                foreach (var fld in exportFields)
                                {
                                    bool dataFound = false;

                                    foreach (PropertyInfo propItem in properties)
                                    {
                                        if (propItem.Name.ToLower().Equals(fld.ToLower()))
                                        {
                                            dynamic propvalue = propItem.GetValue(dataItem.Value, null);

                                            string attType = propItem.PropertyType.Name;

                                            dataFound = true;

                                            switch (propItem.Name.ToLower())
                                            {
                                                case "type":
                                                case "fieldtype":
                                                    fieldPropType = propvalue.ToString();
                                                    break;
                                            }

                                            switch (attType)
                                            {
                                                case "String":
                                                    if (propvalue == null)
                                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                    else
                                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                    break;
                                                case "DateTime":
                                                case "Date":
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                    break;
                                                case "Boolean":
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", (String.IsNullOrEmpty(propvalue.ToString()) ? String.Empty : (propvalue == true ? "Yes" : "No"))));
                                                    break;
                                                case "List`1":
                                                    {
                                                        if (propvalue == null)
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                        else
                                                        {
                                                            List<GroupViewModel> castGroupList = (List < GroupViewModel >) propvalue;
                                                            StringBuilder groupBuildList = new StringBuilder();

                                                            if (castGroupList.Count > 0)
                                                            {
                                                                groupBuildList.Append("\"");

                                                                foreach(GroupViewModel groupViewItem in castGroupList)
                                                                {
                                                                    groupBuildList.Append((groupBuildList.Length > 1?",":string.Empty) + groupViewItem.name);

                                                                }
                                                                groupBuildList.Append("\"");
                                                            }
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + groupBuildList.ToString());

                                                        }

                                                    }
                                                    break;

                                                case "Object":
                                                    switch (fieldPropType.ToUpper())
                                                    {
                                                        case "TEXT":
                                                        case "MEMO":
                                                        case "DROPDOWN":
                                                        case "LIST":
                                                            if (propvalue == null)
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                            else
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                            break;
                                                        default:
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("{0}", (propvalue == null ? string.Empty : propvalue)));
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("{0}", (propvalue == null ? string.Empty : propvalue)));
                                                    break;
                                            }

                                            break;
                                        }
                                    }

                                    if (!dataFound)
                                    {
                                        // -- not in the new system

                                        switch (fld)
                                        {
                                            case "UserId":
 
                                                    outString.Append(",\"" + dataItem.Key + "\"");          // UserId

                                                break;
                                            case "EmployeeId":
                                                if (_userEmployee.ContainsKey(dataItem.Key))
                                                outString.Append(",\"" + _userEmployee[dataItem.Key] + "\"");
                                                else
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + string.Empty);
                                                break;
                                            default:
                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + string.Empty);
                                                break;
                                        }
                                    }
                                }

                                if (!_missingEmailList.ContainsKey(dataItem.Value.email))
                                    w.Write(outString.ToString() + "\n");

                                //foreach (PropertyInfo pi in properties)
                                //{
                                //    sb.Append(
                                //        string.Format("Name: {0} | Value: {1}",
                                //                pi.Name,
                                //                pi.GetValue(user, null)
                                //            )
                                //    );
                                //}

                            }

                            foreach (KeyValuePair<string, UserProfileViewModel> dataItem in _specialEmployee)
                            {
                                PropertyInfo[] properties = dataItem.Value.GetType().GetProperties();
                                string fieldPropType = "";

                                if (outCnt++ == 0)
                                {
                                    outString.Remove(0, outString.Length);
                                    foreach (var fld in exportFields)
                                    {
                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + fld);
                                    }
                                    w.Write(outString + "\n");
                                }

                                outString.Remove(0, outString.Length);

                                foreach (var fld in exportFields)
                                {
                                    bool foundFlag = false;

                                    foreach (PropertyInfo propItem in properties)
                                    {
                                        if (propItem.Name.ToLower().Equals(fld.ToLower()))
                                        {
                                            dynamic propvalue = propItem.GetValue(dataItem.Value, null);

                                            string attType = propItem.PropertyType.Name;
                                            foundFlag = true;
                                            switch (propItem.Name.ToLower())
                                            {
                                                case "type":
                                                case "fieldtype":
                                                    fieldPropType = propvalue.ToString();
                                                    break;
                                            }

                                            switch (attType)
                                            {
                                                case "String":
                                                    if (propvalue == null)
                                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                    else
                                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                    break;
                                                case "DateTime":
                                                case "Date":
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                    break;
                                                case "Boolean":
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", (String.IsNullOrEmpty(propvalue.ToString()) ? String.Empty : (propvalue == true ? "Yes" : "No"))));
                                                    break;
                                                case "List`1":
                                                    {
                                                        if (propvalue == null)
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                        else
                                                        {
                                                            List<GroupViewModel> castGroupList = (List<GroupViewModel>)propvalue;
                                                            StringBuilder groupBuildList = new StringBuilder();

                                                            if (castGroupList.Count > 0)
                                                            {
                                                                groupBuildList.Append("\"");

                                                                foreach (GroupViewModel groupViewItem in castGroupList)
                                                                {
                                                                    groupBuildList.Append((groupBuildList.Length > 1 ? "," : string.Empty) + groupViewItem.name);

                                                                }
                                                                groupBuildList.Append("\"");
                                                            }
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + groupBuildList.ToString());

                                                        }

                                                    }
                                                    break;

                                                case "Object":
                                                    switch (fieldPropType.ToUpper())
                                                    {
                                                        case "TEXT":
                                                        case "MEMO":
                                                        case "DROPDOWN":
                                                        case "LIST":
                                                            if (propvalue == null)
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                            else
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                            break;
                                                        default:
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("{0}", (propvalue == null ? string.Empty : propvalue)));
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("{0}", (propvalue == null ? string.Empty : propvalue)));
                                                    break;
                                            }

                                            break;
                                        }
                                    }

                                    if (!foundFlag)
                                    {
                                        outString.Append(",");
                                        switch (fld.ToLower())
                                        {
                                            case "employeeid":
                                                outString.Append("\"" + dataItem.Key + "\"");
                                                break;
                                        }
                                    }
                                        
                                }

                                w.Write(outString.ToString() + "\n");
      
                            }

                            w.Flush();
                        }

           

                        string outMissingFileName = ExportDir + CustomerName + "_UsersMissing_" + DateTime.Now.ToString("yyyyMMdd") + this.FileExtension;

                        FileInfo fileMissingInfoObj = new FileInfo(outMissingFileName);


                        if (fileMissingInfoObj.Exists)
                        {
                            try
                            {
                                fileMissingInfoObj.Delete();
                            }
                            catch
                            {
                                outMissingFileName = outFileName.Replace(this.FileExtension, "") + "_" + DateTime.Now.ToString("hhmmss") + this.FileExtension;
                            }

                            fileMissingInfoObj = null;
                        }

                        using (StreamWriter w = File.AppendText(outMissingFileName))
                        {
                            int outCnt = 0;
                            StringBuilder outString = new StringBuilder();


                            foreach (KeyValuePair<string, UserProfileViewModel> dataItem in _missingEmailList)
                            {
                                PropertyInfo[] properties = dataItem.Value.GetType().GetProperties();
                                string fieldPropType = "";

                                if (outCnt++ == 0)
                                {
                                    outString.Remove(0, outString.Length);
                                    foreach (var fld in exportFields)
                                    {
                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + fld);
                                    }
                                    w.Write(outString + "\n");
                                }

                                outString.Remove(0, outString.Length);

                                foreach (var fld in exportFields)
                                {
                                    bool dataFound = false;

                                    foreach (PropertyInfo propItem in properties)
                                    {
                                        if (propItem.Name.ToLower().Equals(fld.ToLower()))
                                        {
                                            dynamic propvalue = propItem.GetValue(dataItem.Value, null);

                                            string attType = propItem.PropertyType.Name;

                                            dataFound = true;

                                            switch (propItem.Name.ToLower())
                                            {
                                                case "type":
                                                case "fieldtype":
                                                    fieldPropType = propvalue.ToString();
                                                    break;
                                            }

                                            switch (attType)
                                            {
                                                case "String":
                                                    if (propvalue == null)
                                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                    else
                                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                    break;
                                                case "DateTime":
                                                case "Date":
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                    break;
                                                case "Boolean":
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", (String.IsNullOrEmpty(propvalue.ToString()) ? String.Empty : (propvalue == true ? "Yes" : "No"))));
                                                    break;
                                                case "List`1":
                                                    {
                                                        if (propvalue == null)
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                        else
                                                        {
                                                            List<GroupViewModel> castGroupList = (List<GroupViewModel>)propvalue;
                                                            StringBuilder groupBuildList = new StringBuilder();

                                                            if (castGroupList.Count > 0)
                                                            {
                                                                groupBuildList.Append("\"");

                                                                foreach (GroupViewModel groupViewItem in castGroupList)
                                                                {
                                                                    groupBuildList.Append((groupBuildList.Length > 1 ? "," : string.Empty) + groupViewItem.name);

                                                                }
                                                                groupBuildList.Append("\"");
                                                            }
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + groupBuildList.ToString());

                                                        }

                                                    }
                                                    break;

                                                case "Object":
                                                    switch (fieldPropType.ToUpper())
                                                    {
                                                        case "TEXT":
                                                        case "MEMO":
                                                        case "DROPDOWN":
                                                        case "LIST":
                                                            if (propvalue == null)
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                            else
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                            break;
                                                        default:
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("{0}", (propvalue == null ? string.Empty : propvalue)));
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("{0}", (propvalue == null ? string.Empty : propvalue)));
                                                    break;
                                            }

                                            break;
                                        }
                                    }

                                    if (!dataFound)
                                    {
                                        // -- not in the new system

                                        switch (fld)
                                        {
                                            case "UserId":
                                                outString.Append(",\"" + dataItem.Key + "\"");          // UserId
                                                break;
                                            case "EmployeeId":
                                                if (_userEmployee.ContainsKey(dataItem.Key))
                                                    outString.Append(",\"" + _userEmployee[dataItem.Key] + "\"");
                                                else
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + string.Empty);
                                                break;
                                            default:
                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + string.Empty);
                                                break;
                                        }
                                    }
                                }


                                w.Write(outString.ToString() + "\n");

                                //foreach (PropertyInfo pi in properties)
                                //{
                                //    sb.Append(
                                //        string.Format("Name: {0} | Value: {1}",
                                //                pi.Name,
                                //                pi.GetValue(user, null)
                                //            )
                                //    );
                                //}

                            }

                            foreach (KeyValuePair<string, UserProfileViewModel> dataItem in _specialEmployee)
                            {
                                PropertyInfo[] properties = dataItem.Value.GetType().GetProperties();
                                string fieldPropType = "";

                                if (outCnt++ == 0)
                                {
                                    outString.Remove(0, outString.Length);
                                    foreach (var fld in exportFields)
                                    {
                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + fld);
                                    }
                                    w.Write(outString + "\n");
                                }

                                outString.Remove(0, outString.Length);

                                foreach (var fld in exportFields)
                                {
                                    bool foundFlag = false;

                                    foreach (PropertyInfo propItem in properties)
                                    {
                                        if (propItem.Name.ToLower().Equals(fld.ToLower()))
                                        {
                                            dynamic propvalue = propItem.GetValue(dataItem.Value, null);

                                            string attType = propItem.PropertyType.Name;
                                            foundFlag = true;
                                            switch (propItem.Name.ToLower())
                                            {
                                                case "type":
                                                case "fieldtype":
                                                    fieldPropType = propvalue.ToString();
                                                    break;
                                            }

                                            switch (attType)
                                            {
                                                case "String":
                                                    if (propvalue == null)
                                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                    else
                                                        outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                    break;
                                                case "DateTime":
                                                case "Date":
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                    break;
                                                case "Boolean":
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", (String.IsNullOrEmpty(propvalue.ToString()) ? String.Empty : (propvalue == true ? "Yes" : "No"))));
                                                    break;
                                                case "List`1":
                                                    {
                                                        if (propvalue == null)
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                        else
                                                        {
                                                            List<GroupViewModel> castGroupList = (List<GroupViewModel>)propvalue;
                                                            StringBuilder groupBuildList = new StringBuilder();

                                                            if (castGroupList.Count > 0)
                                                            {
                                                                groupBuildList.Append("\"");

                                                                foreach (GroupViewModel groupViewItem in castGroupList)
                                                                {
                                                                    groupBuildList.Append((groupBuildList.Length > 1 ? "," : string.Empty) + groupViewItem.name);

                                                                }
                                                                groupBuildList.Append("\"");
                                                            }
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + groupBuildList.ToString());

                                                        }

                                                    }
                                                    break;

                                                case "Object":
                                                    switch (fieldPropType.ToUpper())
                                                    {
                                                        case "TEXT":
                                                        case "MEMO":
                                                        case "DROPDOWN":
                                                        case "LIST":
                                                            if (propvalue == null)
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Empty);
                                                            else
                                                                outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("\"{0}\"", propvalue));
                                                            break;
                                                        default:
                                                            outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("{0}", (propvalue == null ? string.Empty : propvalue)));
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    outString.Append((outString.Length > 0 ? "," : string.Empty) + String.Format("{0}", (propvalue == null ? string.Empty : propvalue)));
                                                    break;
                                            }

                                            break;
                                        }
                                    }

                                    if (!foundFlag)
                                    {
                                        outString.Append(",");
                                        switch (fld.ToLower())
                                        {
                                            case "employeeid":
                                                outString.Append("\"" + dataItem.Key + "\"");
                                                break;
                                        }
                                    }

                                }

                                w.Write(outString.ToString() + "\n");

 
                            }

                            w.Flush();
                        }

                        break;

                    default:
                        string outJSONFileName = ExportDir + CustomerName + DateTime.Now.ToString("yyyyMMdd") + this.FileExtension;

                        FileInfo jsonFileObj = new FileInfo(outJSONFileName);


                        if (jsonFileObj.Exists)
                        {
                            try
                            {
                                jsonFileObj.Delete();
                            }
                            catch
                            {
                                outJSONFileName = outJSONFileName.Replace(this.FileExtension, "") + "_" + DateTime.Now.ToString("hhmmss") + this.FileExtension;
                            }
                        }

                        using (StreamWriter w = File.AppendText(outJSONFileName))
                        {
                            foreach (KeyValuePair<string, UserProfileViewModel> dataItem in _userList)
                            {
                                var jsonInputData = DataToJson.ToJson(dataItem.Value);
                                w.WriteLine(jsonInputData);
                            }

                        }

                        break;
                }

            }
            return retFlag;
        }


    }
}
