using Client.Sensor;
using Client.SensorService;
using Client.SensorServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.DecisionManager
{
    internal class LocalDecisionMaker : ILocalDecisionMaker
    {
        public bool shouldAlarmStateBeReversedBasedOnBrokerData(List<SensorData> brokerSensorDatas, SensorBuzzerService buzzerService)
        {
            foreach (SensorData sensorData in brokerSensorDatas)
            {     //Console.WriteLine(sensorData.ParameterName);
                //Console.WriteLine(sensorData.ParameterValue);
                string ParameterName = sensorData.ParameterName;
                string ParameterValue = sensorData.ParameterValue;
                switch (ParameterName)
                {
                    case "BUZZER":
                        bool result = bool.Parse(ParameterValue);
                        if (result)
                        {
                            Console.WriteLine("DECISION BASED ON BROKER: Checking if buzzer enabled...");
                            if (!buzzerService.check())
                            {
                                Console.WriteLine("DECISION BASED ON BROKER: I'm enabling my buzzer as client (checked buzzer was off)");
                                return true;
                            }
                            else
                            {
                                Console.WriteLine("DECISION BASED ON BROKER: Buzzer was enabled so Im not enabling again");
                                return false;
                            }
                        }
                        else
                        {
                            if (buzzerService.check())
                            {
                                Console.WriteLine("DECISION BASED ON BROKER: I'm disabling my buzzer as client (checked buzzer was on)");
                                return true;
                            }
                            else
                            {
                                Console.WriteLine("DECISION BASED ON BROKER: Buzzer was disabled so Im not disabling again");
                                return false;
                            }
                       
                        }
                        break;

                    case "SMOKE":
                        break;
                    default:
                        Console.WriteLine($"Unknow parameter:{ParameterName}:{ParameterValue}");
                        break;
                };
             
            }
            return false;
        }


        public bool shouldAlarmStateBeReversedBasedOnLocalData(List<SensorData> localSensorDatas, SensorBuzzerService buzzerService)
        {
            foreach (SensorData sensorData in localSensorDatas)
            {
                string ParameterName = sensorData.ParameterName;
                string ParameterValue = sensorData.ParameterValue;
                switch (ParameterName)
                {
                    case "SMOKE":
                        bool result = bool.Parse(ParameterValue);
                        if (result)
                        {
                            Console.WriteLine("LOCAL DECISION: localBuzzerEnableIfSmokeDetected(): I detected smoke. Checking if buzzer enabled...");
                            if (!buzzerService.check())
                            {
                                Console.WriteLine("LOCAL DECISION: I'm enabling my buzzer as client (checked buzzer was off)");
                                return true;
                            }
                            else
                            {
                                Console.WriteLine("LOCAL DECISION: Buzzer was enabled");
                                return false;
                            }

                        }
                        else
                        {
                            // we don't want to switch it if sensor hasnt detected smoke (brokers might know about smoke in other room)
                        }
                        break;
                    case "BUZZER":
                        //Console.WriteLine("LOCAL DECISION: localBuzzerEnableIfSmokeDetected(): Omitting Buzzer data, cuz not relevant for local decision");

                        break;
                    default:
                        Console.WriteLine($"localBuzzerEnableIfSmokeDetected: Unknow parameter:{ParameterName}:{ParameterValue}");
                        break;

                     
                };
                
            }
            return false;
        }
    }
}
