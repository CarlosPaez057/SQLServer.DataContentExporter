using System;
using System.Net;
using Newtonsoft.Json;
using SQLServer.DataContentExporter.ViewModels;

namespace SQLServer.DataContentExporter.Models
{
    public class WebHelper
    {
        #region Privates
        private string _APIurl = @"http://localhost:5001/api/";
        private string _AccessToken = "";
        private DateTime _AccessTokenExpiration = DateTime.MinValue;

        #endregion

        #region Constructor

        public string APIurl
        {
            get { return _APIurl; }
        }

        public bool Authenticated()
        {
            bool auth = false;

            if ((!String.IsNullOrEmpty(_AccessToken)) && (_AccessTokenExpiration > DateTime.Now))
                auth = true;

            return auth;
        }
        public WebHelper(string Host,int? Port,string BaseAPI)
        {
            _APIurl = string.Format("http://{0}{1}{2}", Host, (Port == null ? string.Empty:":"+Port.ToString()), string.IsNullOrEmpty(BaseAPI)?string.Empty:"/"+BaseAPI+"/");
        }
        #endregion


        public bool AuthLogin(LoginViewModel UserSettings)
        {
            string postMethod = "auth/login";
            bool auth = false;
            
            using (var client = new WebClient())
            {
                var dataString = JsonConvert.SerializeObject(UserSettings);
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                string response = client.UploadString(new Uri(APIurl + postMethod), "POST", dataString);

                dynamic dynJson = JsonConvert.DeserializeObject(response);

                foreach (var item in dynJson)
                {
                    if (item.HasValues == true)
                    {
                        switch(item.Name.ToLower().Trim())
                        {
                            case "access_token":
                                _AccessToken = item.Value.ToString();
                                break;
                            case "expires":
                                _AccessTokenExpiration = DateTime.Parse(item.Value.ToString());
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if ( (! String.IsNullOrEmpty(_AccessToken)) && (_AccessTokenExpiration > DateTime.Now))
                auth = true;
            return auth;
        }

        public  T _download_serialized_json_data<T>(string GetMethod) where T : new()
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                var api_data = string.Empty;
                int records = 0;

                w.Headers.Add(HttpRequestHeader.Authorization,
    "Bearer " + _AccessToken);

                // attempt to download JSON data as a string
                try
                {
                    api_data = w.DownloadString(APIurl + GetMethod);

                    dynamic dynJson = JsonConvert.DeserializeObject(api_data);

                    foreach (var item in dynJson)
                    {
                        if (item.HasValues == true)
                        {
                            switch (item.Name.ToLower().Trim())
                            {
                                case "totalcount":
                                    records = Convert.ToInt32(item.Value.ToString());
                                    break;
                                case "data":
                                    json_data = item.Value.ToString();
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (item != null)
                        {
                            if (((Newtonsoft.Json.Linq.JContainer)item).Count != 0)
                            {
                                records = ((Newtonsoft.Json.Linq.JContainer)item).Count;

                                json_data = api_data;
                            }
                        }
                        
                    }


                }
                catch (WebException we)
                {
                    HttpWebResponse response = (System.Net.HttpWebResponse)we.Response;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        System.Diagnostics.Debug.WriteLine("Not found!");
                }
                // if string with JSON data is not empty, deserialize it to class and return its instance 
                return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
            }
        }

        public  void UploadString(string webpath,string jsonData)
        {
            WebClient client = new WebClient();
            // Optionally specify an encoding for uploading and downloading strings.
            client.Encoding = System.Text.Encoding.UTF8;


            // Upload the data.
            string reply = client.UploadString(APIurl + webpath, jsonData);
            // Disply the server's response.
            Console.WriteLine(reply);
        }

    }
}


//private static T ExecuteViaRest<T>(string url) where T : new()
//{

//    string retValue = "";

//    var client = new RestClient(DGSettings.ApiUrl);

//    string ApiMethod = @"/client/activeindexes";

//    var request = new RestRequest(ApiMethod, Method.GET);
//    request.AddHeader("Authorization", DGSettings.AccessKey);

//    var response = client.Execute(request);

//    string responseString = response.Content;

//    if (response.StatusCode == System.Net.HttpStatusCode.OK)
//    {
//        retValue = responseString;


//        List<ElasticSerachIndex> BlockchainList = JsonConvert.DeserializeObject<List<ElasticSerachIndex>>(responseString);


//        foreach (ElasticSerachIndex mItem in BlockchainList)
//        {
//            if (!DefinedLedgers.ContainsKey(mItem.Provider))
//            {
//                string provider = string.Format("{0}", mItem.Provider);

//                DefinedLedgers.Add(provider, new List<ElasticSerachIndex>());
//            }

//            DefinedLedgers[mItem.Provider].Add(mItem);
//        }
//    }