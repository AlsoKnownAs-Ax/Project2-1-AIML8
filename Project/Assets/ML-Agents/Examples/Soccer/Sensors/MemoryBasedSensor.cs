using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;

public class MemoryBasedSensor : MonoBehaviour, ISensor, ISoccerSensor
{
    [Header("Memory Settings")]
    [SerializeField] 
    [Range(5, 20)]
    [Tooltip("Number of past positions to remember")]
    public int memorySize = 10;

    [SerializeField]
    [Range(0.001f, 0.1f)]
    [Tooltip("Reward multiplier for memory-based actions")]
    public float rewardMultiplier = 0.01f;

    private AgentSoccer agent;
    private GameObject ball;
    private List<AgentSoccer> teammates;

    private Queue<Vector3> pastPositions;
    private Queue<Vector3> pastRelativeBallPositions;
    private Queue<Vector3> pastRelativeTeammatePositions;

    private Vector3 previousPosition;
    private float cumulativeDistance;

    public void InitializeSensor(AgentSoccer agent, GameObject ball, List<AgentSoccer> teammates)
    {
        this.agent = agent;
        this.ball = ball;
        this.teammates = teammates;

        pastPositions = new Queue<Vector3>(memorySize);
        pastRelativeBallPositions = new Queue<Vector3>(memorySize);
        pastRelativeTeammatePositions = new Queue<Vector3>(memorySize);

        InitializeMemory();
    }

    private void InitializeMemory()
    {
        if (agent == null) return;

        previousPosition = agent.transform.position;
        cumulativeDistance = 0f;

        // Initialize queues with proper capacity
        pastPositions = new Queue<Vector3>(memorySize);
        pastRelativeBallPositions = new Queue<Vector3>(memorySize);
        pastRelativeTeammatePositions = new Queue<Vector3>(memorySize);

        // Clear and fill queues with initial values
        ClearAndFillQueues();
    }

    private void ClearAndFillQueues()
    {
        // Clear existing entries
        if (pastPositions != null) pastPositions.Clear();
        if (pastRelativeBallPositions != null) pastRelativeBallPositions.Clear();
        if (pastRelativeTeammatePositions != null) pastRelativeTeammatePositions.Clear();

        // Initialize with default values if agent exists
        if (agent != null)
        {
            Vector3 agentPos = agent.transform.position;
            Vector3 ballRelativePos = ball != null ? agent.transform.position - ball.transform.position : Vector3.zero;

            for (int i = 0; i < memorySize; i++)
            {
                pastPositions.Enqueue(agentPos);
                pastRelativeBallPositions.Enqueue(ballRelativePos);
                pastRelativeTeammatePositions.Enqueue(Vector3.zero);
            }
        }
    }

    public void UpdateMemory()
    {
        float distanceMoved = Vector3.Distance(agent.transform.position, previousPosition);
        cumulativeDistance += distanceMoved;
        previousPosition = agent.transform.position;

        if (pastPositions.Count >= memorySize)
        {
            pastPositions.Dequeue();
            pastRelativeBallPositions.Dequeue();
            pastRelativeTeammatePositions.Dequeue();
        }

        pastPositions.Enqueue(agent.transform.position);

        Vector3 relativeBallPosition = ball != null ? agent.transform.position - ball.transform.position : Vector3.zero;
        Vector3 relativeTeammatePosition = Vector3.zero;

        if (teammates != null && teammates.Count > 0)
        {
            foreach (var teammate in teammates)
            {
                if (teammate != null && teammate != agent)
                {
                    relativeTeammatePosition = agent.transform.position - teammate.transform.position;
                    break;
                }
            }
        }

        pastRelativeBallPositions.Enqueue(relativeBallPosition);
        pastRelativeTeammatePositions.Enqueue(relativeTeammatePosition);
    }

    public void AddMemoryRewards(AgentSoccer agent)
    {
        foreach (var pastPosition in pastPositions)
        {
            agent.AddReward(Vector3.Distance(agent.transform.position, pastPosition) * rewardMultiplier);
        }

        foreach (var pastRelativeBallPosition in pastRelativeBallPositions)
        {
            agent.AddReward(Vector3.Distance(agent.transform.position - ball.transform.position, pastRelativeBallPosition) * rewardMultiplier);
        }

        foreach (var pastRelativeTeammatePosition in pastRelativeTeammatePositions)
        {
            agent.AddReward(Vector3.Distance(agent.transform.position - pastRelativeTeammatePosition, pastRelativeTeammatePosition) * rewardMultiplier);
        }
    }

    public void ClearMemory()
    {
        if (pastPositions != null) pastPositions.Clear();
        if (pastRelativeBallPositions != null) pastRelativeBallPositions.Clear();
        if (pastRelativeTeammatePositions != null) pastRelativeTeammatePositions.Clear();
    }

    public void UpdateSensor()
    {
        UpdateMemory();
    }

    public void Reset()
    {
        if (pastPositions == null || pastRelativeBallPositions == null || pastRelativeTeammatePositions == null)
        {
            InitializeMemory();
        }
        else
        {
            ClearAndFillQueues();
        }
    }

    // ISensor Implementation
    public string GetName()
    {
        return "MemoryBasedSensor";
    }

    public int[] GetObservationShape()
    {
        return new int[] { memorySize * 3 * 3 }; // 3 vectors (position, ball, teammate) * 3 components (x,y,z) per memory entry
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public int Write(ObservationWriter writer)
    {
        if (pastPositions == null || pastRelativeBallPositions == null || pastRelativeTeammatePositions == null || agent == null)
        {
            // Return zero observations if not properly initialized
            for (int i = 0; i < memorySize * 3 * 3; i++)
            {
                writer[i] = 0f;
            }
            return memorySize * 3 * 3;
        }

        int index = 0;

        if (pastPositions == null || pastRelativeBallPositions == null || pastRelativeTeammatePositions == null)
        {
            InitializeMemory();
        }

        foreach (var position in pastPositions)
        {
            writer[index++] = position.x;
            writer[index++] = position.y;
            writer[index++] = position.z;
        }

        foreach (var ballPos in pastRelativeBallPositions)
        {
            writer[index++] = ballPos.x;
            writer[index++] = ballPos.y;
            writer[index++] = ballPos.z;
        }

        foreach (var teammatePos in pastRelativeTeammatePositions)
        {
            writer[index++] = teammatePos.x;
            writer[index++] = teammatePos.y;
            writer[index++] = teammatePos.z;
        }

        return memorySize * 3 * 3;
    }

    // Remove OnEnable since we'll initialize through InitializeSensor
    private void OnEnable()
    {
        // Leave empty - initialization will happen through InitializeSensor
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
        return ObservationSpec.Vector(memorySize * 3 * 3);
    }
}