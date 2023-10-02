using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Sensor
{
    [Serializable]
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
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SensorData other = (SensorData)obj;

            return ParameterName == other.ParameterName && ParameterValue == other.ParameterValue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ParameterName.GetHashCode();
                hash = hash * 23 + ParameterValue.GetHashCode();
                return hash;
            }
        }
        public override string ToString()
        {
            return $"{{{_parameterName}:{_parameterValue}}}";
        }
    }

}

