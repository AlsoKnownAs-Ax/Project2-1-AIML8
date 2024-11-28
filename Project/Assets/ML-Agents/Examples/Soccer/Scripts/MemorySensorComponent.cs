using UnityEngine;
using Unity.MLAgents.Sensors;

public class MemorySensorComponent : SensorComponent
{
    public MemoryBasedSensor memoryBasedSensor;
    public override ISensor[] CreateSensors()
        {
            memoryBasedSensor =  new MemoryBasedSensor();
            return new ISensor[]{memoryBasedSensor};
        }
}
