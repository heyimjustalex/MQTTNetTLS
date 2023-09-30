using Client.Sensor;
using Client.SensorService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.DecisionManager
{
    internal interface ILocalDecisionMaker
    {
        public bool shouldAlarmStateBeReversedBasedOnBrokerData(List<SensorData> brokerSensorDatas, SensorBuzzerService buzzerService);
        public bool shouldAlarmStateBeReversedBasedOnLocalData(List<SensorData> localSensorDatas, SensorBuzzerService buzzerService);
    }
}
