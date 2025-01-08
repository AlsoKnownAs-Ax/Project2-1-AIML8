using UnityEngine;
using Unity.MLAgents.Sensors;
public class HearingSensorComponent : SensorComponent
    {
        public HearingSensor hearingSensor;
        
        /// <summary>
        /// Creates a BasicSensor.
        /// </summary>
        /// <returns></returns>
        public override ISensor[] CreateSensors()
        {
            hearingSensor =  new HearingSensor(gameObject);
            return new ISensor[]{hearingSensor};
        }
    }