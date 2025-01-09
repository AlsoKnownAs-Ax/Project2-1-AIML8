using UnityEngine;
using Unity.MLAgents.Sensors;

public class HearingSensorComponent : SensorComponent
{
    public HearingSensor hearingSensor;

    private void Awake()
    {
        InitializeHearingSensor();
    }

    private void InitializeHearingSensor()
    {
        hearingSensor = gameObject.GetComponent<HearingSensor>() ?? gameObject.AddComponent<HearingSensor>();
    }
    
    public override ISensor[] CreateSensors()
    {
        if (hearingSensor == null)
        {
            InitializeHearingSensor();
        }

        return new ISensor[] { hearingSensor };
    }
}