using UnityEngine;
using Unity.MLAgents.Sensors;

public class HearingSensorComponent : SensorComponent
{
    public HearingSensor hearingSensor;
    
    public override ISensor[] CreateSensors()
    {
        hearingSensor = gameObject.GetComponent<HearingSensor>() ?? gameObject.AddComponent<HearingSensor>();
        return new ISensor[]{hearingSensor};
    }
}