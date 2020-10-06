using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SQLServer.DataContentExporter.ViewModels
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserProfileFrequencyTypex
    {
        Realtime = 0,
        Hourly = 1,
        Daily = 2
    }
}
