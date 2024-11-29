using UnityEngine;
using Unity.MLAgents.Sensors;

public class HearingSensor : ISensor
{
    private Vector3 lastBallPosition;
    private Vector3 lastPlayerPosition;

    public HearingSensor()
    {
        Reset();
    }

    public void Reset()
    {
        lastBallPosition = Vector3.zero;
        lastPlayerPosition = Vector3.zero;
    }

    // ISensor Implementation
    public string GetName()
    {
        return "Hearing Sensor";
    }

    public int[] GetObservationShape()
    {
        return new int[] { 3 }; // [ballPos.xyz ]
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public int Write(ObservationWriter writer)
    {
        int index = 0;
        // Debug.Log(lastBallPosition.x);
        writer[index++] = lastBallPosition.x;
        writer[index++] = lastBallPosition.y;
        writer[index++] = lastBallPosition.z;
        // writer[index++] = lastPlayerPosition.x;
        // writer[index++] = lastPlayerPosition.y;
        // writer[index++] = lastPlayerPosition.z;
        return index;
    }

    public void Update() { }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Vector(8);
    }

    public void OnTriggerEnter(GameObject other)
    {
        if (other.CompareTag("ball"))
        {
            lastBallPosition = other.transform.position;
        }
        else if (other.CompareTag("Player"))
        {
            lastPlayerPosition = other.transform.position;
        }
    }
}