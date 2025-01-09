using UnityEngine;
using Unity.MLAgents.Sensors;

public class MemorySensorComponent : SensorComponent
{
    public MemoryBasedSensor memorySensor;

    private void Awake()
    {
        InitializeMemorySensor();
    }

    private void InitializeMemorySensor()
    {
        memorySensor = gameObject.GetComponent<MemoryBasedSensor>() ?? gameObject.AddComponent<MemoryBasedSensor>();
    }
    
    public override ISensor[] CreateSensors()
    {
        if (memorySensor == null)
        {
            InitializeMemorySensor();
        }
        return new ISensor[]{memorySensor};
    }
}