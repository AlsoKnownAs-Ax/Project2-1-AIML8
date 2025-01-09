using UnityEngine;
using Unity.MLAgents.Sensors;

public class VisionConeComponent : SensorComponent
{
    public VisionCone visionCone;

    private void Awake()
    {
        InitializeVisionCone();
    }

    private void InitializeVisionCone()
    {
        visionCone = gameObject.GetComponent<VisionCone>() ?? gameObject.AddComponent<VisionCone>();
    }

    public override ISensor[] CreateSensors()
    {
        if (visionCone == null)
        {
            InitializeVisionCone();
        }
        return new ISensor[] { visionCone };
    }
}