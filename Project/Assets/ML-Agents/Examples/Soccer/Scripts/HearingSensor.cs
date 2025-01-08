using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;

public class HearingSensor : ISensor
{
    private Vector3 lastBallPosition;
    private Vector3 lastPlayerPosition;
    private GameObject agent;
    private Vector3 maxValue = new Vector3(40,40,40);
    private Vector3 minValue = new Vector3(0,0,0);

    public HearingSensor(GameObject agent)
    {
        this.agent = agent;
        Reset();
    }

    public void Reset()
    {
        lastBallPosition = Vector3.zero;
    }

    // ISensor Implementation
    public string GetName()
    {
        return "Hearing Sensor";
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public int Write(ObservationWriter writer)
    {
        writer.Add(lastBallPosition);
        return 3;
    }

    public void Update() { }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Vector(3);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ball"))
        {
            Vector3 relativeBallPosition = other.transform.position - agent.transform.position;
            lastBallPosition.x = (other.transform.position.x - minValue.x)/(maxValue.x - minValue.x);
            lastBallPosition.y = (other.transform.position.y - minValue.y)/(maxValue.y - minValue.y);
            lastBallPosition.z = (other.transform.position.z - minValue.z)/(maxValue.z - minValue.z);
        }
    }
}