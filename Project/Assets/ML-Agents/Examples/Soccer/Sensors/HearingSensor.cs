using UnityEngine;
using Unity.MLAgents.Sensors;

public class HearingSensor : ISensor
{
    private Vector3 lastBallPosition;
    private Vector3 lastPlayerPosition;

    // private float MovementThreshold = 0.1f;
    // private float NoiseFactor = 0.05f;

    public HearingSensor()
    {
        Reset();
    }

    public void Reset()
    {
        lastBallPosition = Vector3.zero;
    }

    public string GetName() {
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
        Debug.Log(lastBallPosition.x);
        writer[index++] = lastBallPosition.x;
        writer[index++] = lastBallPosition.y;
        writer[index++] = lastBallPosition.z;
        return index;
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

    public void OnTriggerEnter(GameObject other)
    {
        if (other.CompareTag("ball"))
        {
            lastBallPosition = other.transform.position;
            //Uncomment this block to apply noise on received ball position and comment the line above
            // Vector3 currentBallPosition = other.transform.position;
            // if (Vector3.Distance(lastBallPosition, currentBallPosition) > MovementThreshold)
            // {
            //     float distance = Vector3.Distance(currentBallPosition, lastPlayerPosition);
            //     Vector3 noise = new Vector3(
            //         Random.Range(-1f, 1f),
            //         Random.Range(-1f, 1f),
            //         Random.Range(-1f, 1f)
            //     ) * NoiseFactor * distance;

            //     lastBallPosition = currentBallPosition + noise;
            // }
        }
        else if (other.CompareTag("Player"))
        {
            lastPlayerPosition = other.transform.position;
        }
    }
}