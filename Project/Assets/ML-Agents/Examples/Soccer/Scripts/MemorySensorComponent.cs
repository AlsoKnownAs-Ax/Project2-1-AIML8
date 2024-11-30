using UnityEngine;
using Unity.MLAgents.Sensors;

public class MemorySensorComponent : SensorComponent
{
    public MemoryBasedSensor memorySensor;
    
    public override ISensor[] CreateSensors()
    {
        memorySensor = gameObject.GetComponent<MemoryBasedSensor>() ?? gameObject.AddComponent<MemoryBasedSensor>();
        return new ISensor[]{memorySensor};
    }
}