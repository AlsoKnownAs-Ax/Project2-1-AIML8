using UnityEngine;
using Unity.MLAgents.Sensors;

public class HearingSensor : MonoBehaviour, ISensor, ISoccerSensor
{
    private Vector3 lastBallPosition;
    private Vector3 lastPlayerPosition;
    private AgentSoccer agent;
    private GameObject ball;
    private System.Collections.Generic.List<AgentSoccer> teammates;
    [SerializeField] private float hearingRadius;

    public void InitializeSensor(AgentSoccer agent, GameObject ball, System.Collections.Generic.List<AgentSoccer> teammates)
    {
        this.agent = agent;
        this.ball = ball;
        this.teammates = teammates;
        Reset();
    }

    public void UpdateSensor()
    {
        // Check for nearby objects and update positions
        CheckForNearbyObjects();
    }

    private void CheckForNearbyObjects()
    {
        if (ball != null)
        {
            float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);
            if (distanceToBall <= hearingRadius) // Add hearingRadius as a serialized field
            {
                lastBallPosition = ball.transform.position;
            }
        }

        if (teammates != null)
        {
            foreach (var teammate in teammates)
            {
                if (teammate != null)
                {
                    float distanceToTeammate = Vector3.Distance(transform.position, teammate.transform.position);
                    if (distanceToTeammate <= hearingRadius)
                    {
                        lastPlayerPosition = teammate.transform.position;
                        break;
                    }
                }
            }
        }
    }

    public void Reset()
    {
        lastBallPosition = Vector3.zero;
        lastPlayerPosition = Vector3.zero;
    }

    // ISensor Implementation
    public string GetName()
    {
        return "HearingSensor";
    }

    public int[] GetObservationShape()
    {
        return new int[] { 8 }; // [ballDetected, ballPos.xyz, playerDetected, playerPos.xyz]
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public int Write(ObservationWriter writer)
    {
        int index = 0;
        bool ballDetected = lastBallPosition != Vector3.zero;
        bool playerDetected = lastPlayerPosition != Vector3.zero;

        writer[index++] = ballDetected ? 1f : 0f;
        writer[index++] = lastBallPosition.x;
        writer[index++] = lastBallPosition.y;
        writer[index++] = lastBallPosition.z;
        writer[index++] = playerDetected ? 1f : 0f;
        writer[index++] = lastPlayerPosition.x;
        writer[index++] = lastPlayerPosition.y;
        writer[index++] = lastPlayerPosition.z;
        
        return 8;
    }

    public void Update() 
    {
        // Regular Unity Update method
    }

    public void Reset(bool sentSignal)
    {
        Reset();
    }

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

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ball"))
        {
            lastBallPosition = Vector3.zero;
        }
        else if (other.CompareTag("Player"))
        {
            lastPlayerPosition = Vector3.zero;
        }
    }
}
