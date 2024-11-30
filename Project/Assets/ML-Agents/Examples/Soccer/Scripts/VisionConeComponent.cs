using UnityEngine;
using Unity.MLAgents.Sensors;

public class VisionConeComponent : SensorComponent
{
    public VisionCone visionCone;
    
    public override ISensor[] CreateSensors()
    {
        visionCone = gameObject.GetComponent<VisionCone>() ?? gameObject.AddComponent<VisionCone>();
        return new ISensor[]{visionCone};
    }
}