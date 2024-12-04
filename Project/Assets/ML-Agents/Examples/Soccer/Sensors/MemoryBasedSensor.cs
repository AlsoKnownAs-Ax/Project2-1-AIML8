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

    /// <summary>
    /// Initialize the sensor with references to the agent, ball, and teammates.
    /// This sets up the necessary objects and initializes the memory queues.
    /// </summary>
    /// <param name="agent">The soccer agent using this sensor.</param>
    /// <param name="ball">The ball object in the environment</param>
    /// <param name="teammates">The list of teammates for the agent.</param>
    public void InitializeSensor(AgentSoccer agent, GameObject ball, List<AgentSoccer> teammates)
    {
        this.agent = agent;
        this.ball = ball;
        this.teammates = teammates;

        previousPosition = agent.transform.position;
        cumulativeDistance = 0f;

        pastPositions = new Queue<Vector3>(memorySize);
        pastRelativeBallPositions = new Queue<Vector3>(memorySize);
        pastRelativeTeammatePositions = new Queue<Vector3>(memorySize);

        ClearAndFillQueues();
    }

    /// <summary>
    /// Clears and initializes the memory queues with default values.
    /// This ensures that the sensor starts with a consistent state.
    /// </summary>
    private void InitializeMemory()
    {
        if (agent == null) return;
        ClearAndFillQueues();
    }

    /// <summary>
    /// Clears all memory queues and fills them with default values based on the current state.
    /// Used during initialization or resets (after goal) to provide a baseline memory state.
    /// </summary>
    private void ClearAndFillQueues()
    {
        // Clear existing entries
        if (pastPositions != null) pastPositions.Clear();
        if (pastRelativeBallPositions != null) pastRelativeBallPositions.Clear();
        if (pastRelativeTeammatePositions != null) pastRelativeTeammatePositions.Clear();

        // Initialize with default values if agent exists
        if (agent != null)
        {
            Vector3 agentPos = agent.transform.position; // Current agent position
            Vector3 ballRelativePos = ball != null ? agent.transform.position - ball.transform.position : Vector3.zero; // Relative ball position

            for (int i = 0; i < memorySize; i++)
            {
                pastPositions.Enqueue(agentPos); // Add agent position into memory
                pastRelativeBallPositions.Enqueue(ballRelativePos);  // Add ball position into memory
                pastRelativeTeammatePositions.Enqueue(Vector3.zero); // Add teammate position into memory
            }
        }
    }

    /// <summary>
    /// Updates the memory queues with the agent's current state.
    /// Maintains a rolling history of positions and relative positions.
    /// </summary>
    public void UpdateMemory()
    {
        if (agent == null) return;

        //Calculate the distance moved and update the cumulative distance
        float distanceMoved = Vector3.Distance(agent.transform.position, previousPosition);
        cumulativeDistance += distanceMoved;
        previousPosition = agent.transform.position;

        //Ensure queue sizes remain within memorySize limits
        if (pastPositions.Count >= memorySize)
        {
            pastPositions.Dequeue();
            pastRelativeBallPositions.Dequeue();
            pastRelativeTeammatePositions.Dequeue();
        }

        // Add current position and relative positions to queues
        pastPositions.Enqueue(agent.transform.position);
        pastRelativeBallPositions.Enqueue(GetRelativeBallPosition());
        pastRelativeTeammatePositions.Enqueue(GetClosestTeammatePosition());
    }
    /// <summary>
    /// Calculates the relative position of the closest teammate with respect to the agent.
    /// Returns Vector3.zero if the teammate list is null or empty.
    /// </summary>
    /// <returns>Vector3 representing the relative position from agent to closest teammate</returns>
    private Vector3 GetClosestTeammatePosition()
    {
        if (teammates == null || teammates.Count == 0) return Vector3.zero; // No teammates check

        Vector3 closest = Vector3.zero;
        float minDist = float.MaxValue;

        foreach (var teammate in teammates)
        {
            if (teammate != null && teammate != agent)
            {
                float dist = Vector3.Distance(agent.transform.position, teammate.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = agent.transform.position - teammate.transform.position;
                }
            }
        }
        return closest;
    }


    /// <summary>
    /// Calculates the relative position of the ball with respect to the agent.
    /// Returns Vector3.zero if the ball is null.
    /// </summary>
    /// <returns>Vector3 representing the relative position from agent to ball</returns>
    private Vector3 GetRelativeBallPosition()
    {
        if (ball == null || agent == null) return Vector3.zero;
        return agent.transform.position - ball.transform.position;
    }


    /// <summary>
    /// Adds rewards to the agent based on historical positions.
    /// Rewards are scaled using the rewardMultiplier value.
    /// Formula used: Reward = Distance(current position, past position) * rewardMultiplier.
    /// Lower distances result in higher rewards, encouraging the agent to maintain consistent positions.
    /// </summary>
    /// <param name="agent">The agent receiving the rewards.</param>
    public void AddMemoryRewards(AgentSoccer agent)
    {
        if (agent == null) return;

        // Reward based on the distance from past positions
        foreach (var pastPosition in pastPositions)
        {
            // Reward for staying close to past positions.
            agent.AddReward(Vector3.Distance(agent.transform.position, pastPosition) * rewardMultiplier);
        }

        if (ball != null)
        {
            // Reward based on the distance from past relative ball positions
            foreach (var pastRelativeBallPosition in pastRelativeBallPositions)
            {
                // Reward for proximity to past relative ball positions.
                agent.AddReward(Vector3.Distance(agent.transform.position - ball.transform.position, pastRelativeBallPosition) * rewardMultiplier);
            }
        }

        // Reward based on the distance from past relative teammate positions
        foreach (var pastRelativeTeammatePosition in pastRelativeTeammatePositions)
        {
            // Reward for proximity to past relative teammate positions.
            agent.AddReward(Vector3.Distance(agent.transform.position - pastRelativeTeammatePosition, pastRelativeTeammatePosition) * rewardMultiplier);
        }
    }

    /// <summary>
    /// Clears all memory queues.
    /// This effectively resets the memory of the sensor.
    /// </summary>
    public void ClearMemory()
    {
        if (pastPositions != null) pastPositions.Clear();
        if (pastRelativeBallPositions != null) pastRelativeBallPositions.Clear();
        if (pastRelativeTeammatePositions != null) pastRelativeTeammatePositions.Clear();
    }

    /// <summary>
    /// Updates the sensor by calling UpdateMemory.
    /// This is the main entry point for periodic updates.
    /// </summary>
    public void UpdateSensor()
    {
        UpdateMemory();
    }

    /// <summary>
    /// Resets the sensor to its initial state.
    /// Memory queues are cleared and refilled with default values.
    /// </summary>
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

    /// <summary>
    /// Returns the name identifier for this sensor
    /// </summary>
    public string GetName()
    {
        return "MemoryBasedSensor";
    }

    /// <summary>
    /// Returns the shape of the observations produced by this sensor.
    /// The size is calculated as: memorySize * 3(xyz coordinates) * 3(agent, ball, teammate positions)
    /// </summary>
    public int[] GetObservationShape()
    {
        return new int[] { GetObservationSize() };
    }

    /// <summary>
    /// This sensor does not use compression, so returns null
    /// </summary>
    public byte[] GetCompressedObservation()
    {
        return null;
    }

    /// <summary>
    /// Writes the sensor's observations into the provided ObservationWriter.
    /// The observations include:
    /// - Past positions of the agent (x,y,z)
    /// - Past relative positions of the ball to the agent (x,y,z)
    /// - Past relative positions of teammates to the agent (x,y,z)
    /// </summary>
    /// <returns>Total number of observations written (memorySize * 3 coordinates * 3 types of positions)</returns>
    public int Write(ObservationWriter writer)
    {
        // If any required data is missing, fill with zeros
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

        // Reinitialize if memory queues are null
        if (pastPositions == null || pastRelativeBallPositions == null || pastRelativeTeammatePositions == null)
        {
            InitializeMemory();
        }

        // Write past agent positions 
        foreach (var position in pastPositions)
        {
            writer[index++] = position.x;
            writer[index++] = position.y;
            writer[index++] = position.z;
        }

        // Write past relative ball positions
        foreach (var ballPos in pastRelativeBallPositions)
        {
            writer[index++] = ballPos.x;
            writer[index++] = ballPos.y;
            writer[index++] = ballPos.z;
        }

        // Write past relative teammate positions
        foreach (var teammatePos in pastRelativeTeammatePositions)
        {
            writer[index++] = teammatePos.x;
            writer[index++] = teammatePos.y;
            writer[index++] = teammatePos.z;
        }

        return memorySize * 3 * 3;
    }

    // Initialization happens through InitializeSensor instead
    private void OnEnable()
    {
        // Leave empty - initialization will happen through InitializeSensor
    }

    /// <summary>
    /// Empty Update method required by Unity MonoBehaviour.
    /// Sensor updates are handled through UpdateSensor() instead.
    /// </summary>
    public void Update() { }

    /// <summary>
    /// Resets the sensor state when signaled by the ML-Agents environment.
    /// </summary>
    /// <param name="sentSignal">Signal from the environment indicating type of reset</param>
    public void Reset(bool sentSignal)
    {
        Reset();
    }

    /// <summary>
    /// Returns the default compression specification since this sensor doesn't use compression
    /// </summary>
    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    /// <summary>
    /// Specifies that this sensor produces a vector of 256 observations
    /// </summary>
    public ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Vector(GetObservationSize());
    }

    /// <summary>
    /// Calculates the total size of observations based on memory size and tracked positions.
    /// Returns memorySize * 3(xyz coordinates) * 3(agent, ball, teammate positions)
    /// </summary>
    private int GetObservationSize()
    {
        return memorySize * 3 * 3; // positions(xyz) * 3 types of positions
    }
}