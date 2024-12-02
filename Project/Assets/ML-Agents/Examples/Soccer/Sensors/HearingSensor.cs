using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class HearingSensor : MonoBehaviour, ISensor, ISoccerSensor
{
    [Header("Hearing Settings")]
    [SerializeField]
    [Range(5f, 50f)]
    [Tooltip("How far the agent can hear")]
    public float hearingRadius = 30f;

    [SerializeField]
    [Range(0.1f, 5f)]
    [Tooltip("How long the agent remembers heard sounds")]
    public float memoryDuration = 1f;

    private Vector3 lastBallPosition;
    private Vector3 lastPlayerPosition;

    private AgentSoccer agent;
    private GameObject ball;
    private List<AgentSoccer> teammates;

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
        return new int[] { 6 }; // Increased from 3
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public int Write(ObservationWriter writer)
    {
        int index = 0;
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
        return ObservationSpec.Vector(6);
    }

    public void OnTriggerEnter(Collider other)  // Changed from GameObject to Collider
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

    public void InitializeSensor(AgentSoccer agent, GameObject ball, List<AgentSoccer> teammates)
    {
        this.agent = agent;
        this.ball = ball;
        this.teammates = teammates;
    }

    public void UpdateSensor()
    {
        // Check for objects in hearing range
        if (ball != null)
        {
            float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);
            if (distanceToBall <= hearingRadius)
            {
                lastBallPosition = ball.transform.position;
            }
        }
    }
}