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
        return ObservationSpec.Vector(3);  // Changed from 8 to 3 to match GetObservationShape
    }

    public void OnTriggerEnter(Collider other)  // Changed from GameObject to Collider
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