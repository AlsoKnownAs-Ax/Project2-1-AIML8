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
            hearingSensor =  new HearingSensor();
            return new ISensor[]{hearingSensor};
        }
    }