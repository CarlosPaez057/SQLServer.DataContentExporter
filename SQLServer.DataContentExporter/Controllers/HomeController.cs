using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SQLServer.DataContentExporter.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using SQLServer.DataContentExporter.ViewModels;

namespace SQLServer.DataContentExporter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private List<string> _validExtensions = new List<string>();
        private List<string> _validTemplates = new List<string>();
        private Dictionary<int, ContentModel> _ContentObjDic=null;
        private string _sqlDbConnectionString= "";
        private List<string> _repurposeColumns = new List<string>();
        private string _customerName;
        private string _exportDir = "/home/cpaez/temp";
        public HomeController(ILogger<HomeController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var activeSQLServers = _configuration.GetSection("SQLServers").Get<List<SQLInstance>>();

            _customerName = _configuration.GetValue<string>("ActiveCustomer");
            _exportDir = _configuration.GetValue<string>("ExportDir");

            foreach (var activeItem in activeSQLServers)
            {
                if (activeItem.Customer.ToLower().Equals(_customerName.ToLower()))
                {
                    _sqlDbConnectionString = activeItem.ConnectionString;
                    _repurposeColumns = activeItem.RepurposeColumns;
                }
            }
            _validExtensions.Add(".xls");
            _validExtensions.Add(".csv");
            _validExtensions.Add(".xlsx");
            _validExtensions.Add(".json");



            _validTemplates.Add("attributes");
            _validTemplates.Add("");

            _ContentObjDic = new Dictionary<int, ContentModel>()
            {
                // display choice, model
                
                { 1,new ContentModel(){Text="Users",ID=1,ModelName="WebUserExport" } } ,
                { 2,new ContentModel(){Text="Item2 Disabled",ID=2,ModelName="WebContentData1Export" } },
                { 3,new ContentModel(){Text="Item3 Disabled",ID=3,ModelName ="WebContentData2Export" } },
            };
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult InvalideFileView()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        // GET: Home
        public ActionResult ExportData()
        {
            //Creating generic list
            List<SelectListItem> ContentObjList = new List<SelectListItem>();
            

            foreach(KeyValuePair<int,ContentModel> item in _ContentObjDic)
            {
                ContentObjList.Add(new SelectListItem { Text = item.Value.Text, Value = item.Key.ToString() });
            }
            //Assigning generic list to ViewBag
            ViewBag.ContentItems = ContentObjList;

            return View();
        }
        [HttpPost]
        public ActionResult ExportData(SelectListItem model)
        {
            if (model != null)
            {
                var selectedValue = Request.Form["ContentObjList"].ToString();

                int intValue = 0;

                if (!String.IsNullOrEmpty(selectedValue))
                {
                    try
                    {
                        intValue = Convert.ToInt32(selectedValue);
                    }
                    catch { }
                }
                if (intValue > 0)
                {
                    if (_ContentObjDic.ContainsKey(intValue))
                    {
                        IDataContentExport contentExport = (IDataContentExport)WebContentClassFactory.GetInstance(_ContentObjDic[intValue].ModelName,_customerName,_sqlDbConnectionString);

                        if (contentExport != null)
                        {
                            switch (_ContentObjDic[intValue].ModelName.ToLower())
                            {
                                case "specialexport":
                                case "specialfile":
                                    contentExport.RepurposeColumns = _repurposeColumns;
                                    break;
                                default:
                                    break;

                            }
                            contentExport.ExportDir = _exportDir;
                            contentExport.FileExtension = ".csv";
                            
                            // definitions before pull data

                            contentExport.PullLegacyData();

                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
