using UnityEngine;
using Unity.MLAgents.Sensors;

public class MemoryBasedSensorComponent : MonoBehaviour, ISoccerSensor
{
    private const string SENSOR_NAME = "MemoryBasedSensor";
    
    [SerializeField, Range(1, 20)]
    private int memorySize = 10;

    private MemoryBasedSensor sensor;

    void Start()
    {
        sensor = gameObject.AddComponent<MemoryBasedSensor>();
        if (sensor != null)
        {
            sensor.MemorySize = memorySize;
        }
    }

    public void UpdateSensor()
    {
        // The sensor updates automatically in its Update() method
    }

    public void ClearSensor()
    {
        if (sensor != null)
        {
            sensor.ClearMemory();
        }
    }

    public string GetSensorName()
    {
        return SENSOR_NAME;
    }
}
