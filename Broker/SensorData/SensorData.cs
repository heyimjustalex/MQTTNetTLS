using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Server.Sensor
{
    public class SensorData
    {
        private string _parameterName;
        private string _parameterValue;

        public SensorData(string parameterName, string parameterValue)
        {
            ParameterName = parameterName;
            ParameterValue = parameterValue;
        }
        [JsonProperty("ParameterName")]
        public string ParameterName
        {
            get { return _parameterName; }
            set { _parameterName = value; }
        }
        [JsonProperty("ParameterValue")]
        public string ParameterValue
        {
            get { return _parameterValue; }
            set { _parameterValue = value; }
        }
    }
}
