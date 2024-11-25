using UnityEngine;
using Unity.MLAgents.Sensors;

public class HearingZone : MonoBehaviour, ISensor, ISoccerSensor
{
    public delegate void ObjectDetected(GameObject obj);
    public event ObjectDetected OnObjectDetected;

    private bool ballDetected;
    private bool playerDetected;
    private Vector3 lastBallPosition;
    private Vector3 lastPlayerPosition;
    private AgentSoccer agent;
    private GameObject ball;
    private System.Collections.Generic.List<AgentSoccer> teammates;

    public void InitializeSensor(AgentSoccer agent, GameObject ball, System.Collections.Generic.List<AgentSoccer> teammates)
    {
        this.agent = agent;
        this.ball = ball;
        this.teammates = teammates;
        Reset();
    }

    public void UpdateSensor()
    {
        // Update is handled by OnTriggerEnter/Exit
    }

    public void Reset()
    {
        ballDetected = false;
        playerDetected = false;
        lastBallPosition = Vector3.zero;
        lastPlayerPosition = Vector3.zero;
    }

    // ISensor Implementation
    public string GetName()
    {
        return "HearingZone";
    }

    public int[] GetObservationShape()
    {
        return new int[] { 8 }; // [ballDetected, playerDetected, ballPos.xyz, playerPos.xyz]
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public int Write(ObservationWriter writer)
    {
        int index = 0;
        writer[index++] = ballDetected ? 1f : 0f;
        writer[index++] = playerDetected ? 1f : 0f;
        writer[index++] = lastBallPosition.x;
        writer[index++] = lastBallPosition.y;
        writer[index++] = lastBallPosition.z;
        writer[index++] = lastPlayerPosition.x;
        writer[index++] = lastPlayerPosition.y;
        writer[index++] = lastPlayerPosition.z;
        return 8;
    }

    public void Update() { }

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ball"))
        {
            ballDetected = true;
            lastBallPosition = other.transform.position;
        }
        else if (other.CompareTag("Player"))
        {
            playerDetected = true;
            lastPlayerPosition = other.transform.position;
        }
        OnObjectDetected?.Invoke(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ball"))
        {
            ballDetected = false;
        }
        else if (other.CompareTag("Player"))
        {
            playerDetected = false;
        }
    }
}
