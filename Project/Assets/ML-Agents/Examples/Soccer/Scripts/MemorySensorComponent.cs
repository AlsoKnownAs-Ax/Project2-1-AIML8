using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class MemorySensorComponent : SensorComponent
{
    private List<ISoccerSensor> sensors = new List<ISoccerSensor>();
    private VectorSensor vectorSensor;
    
    public void AddSensor(ISoccerSensor sensor)
    {
        if (!sensors.Contains(sensor))
        {
            sensors.Add(sensor);
            Debug.Log($"[MemorySensorComponent] Added sensor: {sensor.GetType().Name}");
            LogActiveSensors();
        }
    }

    public void RemoveSensor(ISoccerSensor sensor)
    {
        if (sensors.Remove(sensor))
        {
            Debug.Log($"[MemorySensorComponent] Removed sensor: {sensor.GetType().Name}");
            LogActiveSensors();
        }
    }

    public void LogActiveSensors()
    {
        var activeSensors = sensors.Where(s => s.IsActive()).ToList();
        Debug.Log($"[MemorySensorComponent] Active sensors ({activeSensors.Count}):");
        foreach (var sensor in activeSensors)
        {
            Debug.Log($"  - {sensor.GetType().Name}");
        }
    }

    public override ISensor[] CreateSensors()
    {
        LogActiveSensors();
        int totalObservationSize = 0;
        // Calculate total observation size from active sensors
        foreach (var sensor in sensors)
        {
            if (sensor.IsActive())
            {
                // Assuming each sensor contributes fixed size observations
                // You might need to adjust this based on your specific needs
                totalObservationSize += 150; // Adjust size based on your needs
            }
        }

        vectorSensor = new VectorSensor(totalObservationSize);
        return new ISensor[] { vectorSensor };
    }

    public void CollectObservations()
    {
        vectorSensor.Reset();
        foreach (var sensor in sensors)
        {
            if (sensor.IsActive())
            {
                Debug.Log($"[MemorySensorComponent] Collecting observations from {sensor.GetType().Name}");
                sensor.CollectObservations(vectorSensor);
            }
        }
    }

    public void OnEpisodeBegin()
    {
        foreach (var sensor in sensors)
        {
            if (sensor.IsActive())
            {
                sensor.OnEpisodeBegin();
            }
        }
    }
}
