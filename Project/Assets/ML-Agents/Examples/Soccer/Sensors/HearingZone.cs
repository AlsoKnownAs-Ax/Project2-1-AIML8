using UnityEngine;
using Unity.MLAgents.Sensors;

public class HearingZone : MonoBehaviour, ISensor, ISoccerSensor
{
    // public delegate void ObjectDetected(GameObject obj, Vector3 perceivedPosition);
    public delegate void ObjectDetected(GameObject obj);

    public event ObjectDetected OnObjectDetected;


    // public float detectionRadius = 10f;
    // private void Start()
    // {
    //     StartCoroutine(DetectObjectsInRange());
    // }

    // private IEnumerator DetectObjectsInRange()
    // {
    //     while (true)
    //     {
    //         Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

    //         foreach (Collider collider in colliders)
    //         {
    //             if (collider.CompareTag("ball") || collider.CompareTag("Player"))
    //             {
    //                 float distance = Vector3.Distance(transform.position, collider.transform.position);

    //                 float baseNoiseMagnitude = 0f;
    //                 float maxNoiseMagnitude = 5.0f;
    //                 float noiseMagnitude = Mathf.Lerp(baseNoiseMagnitude, maxNoiseMagnitude, distance / 1000.0f);

    //                 Vector3 randomNoise = Vector3.zero; //Noise is disabled for now
    //                 // Vector3 randomNoise = new Vector3(
    //                 //     Random.Range(-noiseMagnitude, noiseMagnitude),
    //                 //     Random.Range(-noiseMagnitude, noiseMagnitude),
    //                 //     Random.Range(-noiseMagnitude, noiseMagnitude)
    //                 // );

    //                 Vector3 perceivedPosition = collider.transform.position + randomNoise;

    //                 Debug.Log($"Object detected: {collider.name} at perceived position: {perceivedPosition}");

    //                 OnObjectDetected?.Invoke(collider.gameObject, perceivedPosition);
    //             }
    //         }

    //         yield return null;
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
