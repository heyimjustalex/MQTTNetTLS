using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Sensor
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

        public string ParameterName
        {
            get { return _parameterName; }
            set { _parameterName = value; }
        }

        public string ParameterValue
        {
            get { return _parameterValue; }
            set { _parameterValue = value; }
        }
    }
}
