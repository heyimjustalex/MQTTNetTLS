using Client.SensorBase;
using Newtonsoft.Json;

namespace Client.Message
{
    [Serializable]
    public class MessageMQTT
    {

        private DateTime _timestamp;
        private string _from;
        private List<SensorData> _sensorDatas;
        public MessageMQTT(DateTime timestamp, string from, List<SensorData> sensorDatas)
        {
            _timestamp = timestamp;
            _from = from;
            _sensorDatas = sensorDatas;
        }

        [JsonProperty("timestamp")]
        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        [JsonProperty("from")]
        public string From
        {
            get { return _from; }
        }
        [JsonProperty("sensorDatas")]
        public List<SensorData> SensorDatas
        {
            get { return _sensorDatas; }
        }
        public override string ToString()
        {
            return $"Timestamp: {_timestamp}, From: {_from}, SensorDatas: {string.Join(" ", _sensorDatas.Select(sensorData => sensorData.ToString()))}";
        }
    }
}
