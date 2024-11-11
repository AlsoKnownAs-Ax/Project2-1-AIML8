/*
 * Memory-Based Observation Implementation:
 * - Introduced a memory mechanism to retain observations from the past MemorySize frames.
 *   - Variable: private const int MemorySize = 10;
 *   - Variable: private Queue<Vector3> pastPositions;
 *   - Variable: private Queue<Vector3> pastRelativeBallPositions;
 *   - Variable: private Queue<Vector3> pastRelativeTeammatePositions;
 * - The agent stores its past positions and relative positions in queues to simulate awareness of its previous locations.
 *   - Method: Initialize()
 *   - Method: OnActionReceived(ActionBuffers actionBuffers)
 * - The memory is updated in the OnActionReceived method and used as part of the agent’s state input.
 *   - Method: OnActionReceived(ActionBuffers actionBuffers)
 * - Older observations are forgotten to prevent the memory from growing indefinitely.
 *   - Method: OnActionReceived(ActionBuffers actionBuffers)
 * - The agent receives additional rewards based on the distance to its past positions to encourage exploration:
 *   - For each past position, the agent receives a reward of 0.01 times the distance to that position.
 *     - Method: OnActionReceived(ActionBuffers actionBuffers)
 *     - Reward Calculation: AddReward(Vector3.Distance(transform.position, pastPosition) * 0.01f)
 * - The agent also receives a reward of 0.1 when it moves a cumulative distance of 10 units.
 *   - Variable: const float k_DistanceRewardThreshold = 10f;
 *   - Variable: const float k_DistanceReward = 0.1f;
 *   - Method: OnActionReceived(ActionBuffers actionBuffers)
 *   - Reward Calculation: AddReward(k_DistanceReward)
 */

using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Collections.Generic;

// Enum for team identification
public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentSoccer : Agent
{
    // Enum for player positions
    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }

    [HideInInspector]
    public Team team; // Team of the agent
    float m_KickPower; // Power of the kick
    float m_BallTouch; // Ball touch parameter
    public Position position; // Position of the agent

    const float k_Power = 2000f; // Constant power value
    float m_Existential; // Existential reward
    float m_LateralSpeed; // Speed for lateral movement
    float m_ForwardSpeed; // Speed for forward movement

    [HideInInspector]
    public Rigidbody agentRb; // Rigidbody of the agent
    SoccerSettings m_SoccerSettings; // Soccer settings
    BehaviorParameters m_BehaviorParameters; // Behavior parameters
    public Vector3 initialPos; // Initial position of the agent
    public float rotSign; // Rotation sign

    EnvironmentParameters m_ResetParams; // Environment parameters

    Vector3 m_PreviousPosition; // Previous position of the agent
    float m_CumulativeDistance; // Cumulative distance moved
    const float k_DistanceRewardThreshold = 10f; // Distance reward threshold
    const float k_DistanceReward = 0.1f; // Distance reward

    // Memory for past observations
    private const int MemorySize = 10;
    private Queue<Vector3> pastPositions; // Queue to store past positions
    private Queue<Vector3> pastRelativeBallPositions; // Queue to store past relative ball positions
    private Queue<Vector3> pastRelativeTeammatePositions; // Queue to store past relative teammate positions

    // Add these new fields at the class level
    [SerializeField] private GameObject ball; // Reference to the soccer ball
    [SerializeField] private List<AgentSoccer> teammates; // List of teammate agents

    /*
     * Initialize the agent
     * - Sets up initial parameters and configurations for the agent.
     * - Initializes the memory queues for past positions and relative positions.
     */
    public override void Initialize()
    {
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
        }

        // Set speed based on position
        if (position == Position.Goalie)
        {
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.3f;
        }
        else
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.0f;
        }

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        m_PreviousPosition = transform.position;
        m_CumulativeDistance = 0f;

        // Initialize the memory queues
        pastPositions = new Queue<Vector3>(MemorySize);
        pastRelativeBallPositions = new Queue<Vector3>(MemorySize);
        pastRelativeTeammatePositions = new Queue<Vector3>(MemorySize);

        // Find the ball if not assigned
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("ball");
        }

        // Find teammates if not assigned
        if (teammates == null || teammates.Count == 0)
        {
            teammates = new List<AgentSoccer>();
            var allAgents = FindObjectsOfType<AgentSoccer>();
            foreach (var agent in allAgents)
            {
                if (agent != this && agent.team == this.team)
                {
                    teammates.Add(agent);
                }
            }
        }

        // Initialize queues with initial positions
        for (int i = 0; i < MemorySize; i++)
        {
            pastPositions.Enqueue(transform.position);
            pastRelativeBallPositions.Enqueue(ball != null ? transform.position - ball.transform.position : Vector3.zero);
            pastRelativeTeammatePositions.Enqueue(Vector3.zero);
        }
    }

    /*
     * Move the agent based on actions
     * - Determines the direction and rotation based on the action inputs.
     * - Applies the movement and rotation to the agent.
     */
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        // Determine direction based on action
        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        // Apply rotation and force
        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
    }

    /*
     * Called when an action is received
     * - Updates the agent's state based on received actions.
     * - Updates the memory with the latest position and relative positions.
     * - Adds rewards based on the distance to past positions and cumulative distance moved.
     */
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Add reward based on position
        if (position == Position.Goalie)
        {
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential);
        }

        // Calculate distance moved and update cumulative distance
        float distanceMoved = Vector3.Distance(transform.position, m_PreviousPosition);
        m_CumulativeDistance += distanceMoved;
        m_PreviousPosition = transform.position;

        // Reward for moving a cumulative distance of 10 units
        if (m_CumulativeDistance >= k_DistanceRewardThreshold)
        {
            AddReward(k_DistanceReward); // Reward of 0.1
            m_CumulativeDistance = 0f;
        }

        // Safety check before dequeuing
        if (pastPositions.Count > 0 && ball != null)
        {
            // Update memory with the latest position
            if (pastPositions.Count >= MemorySize)
            {
                pastPositions.Dequeue();
                pastRelativeBallPositions.Dequeue();
                pastRelativeTeammatePositions.Dequeue();
            }
            
            pastPositions.Enqueue(transform.position);

            // Calculate relative positions with null checks
            Vector3 relativeBallPosition = ball != null ? 
                transform.position - ball.transform.position : Vector3.zero;
            Vector3 relativeTeammatePosition = Vector3.zero;
            
            if (teammates != null && teammates.Count > 0)
            {
                foreach (var teammate in teammates)
                {
                    if (teammate != null && teammate != this)
                    {
                        relativeTeammatePosition = transform.position - teammate.transform.position;
                        break;
                    }
                }
            }

            pastRelativeBallPositions.Enqueue(relativeBallPosition);
            pastRelativeTeammatePositions.Enqueue(relativeTeammatePosition);

            // Additional rewards only if we have valid data
            foreach (var pastPosition in pastPositions)
            {
                AddReward(Vector3.Distance(transform.position, pastPosition) * 0.01f);
            }

            foreach (var pastRelativeBallPosition in pastRelativeBallPositions)
            {
                AddReward(Vector3.Distance(relativeBallPosition, pastRelativeBallPosition) * 0.01f);
            }

            foreach (var pastRelativeTeammatePosition in pastRelativeTeammatePositions)
            {
                AddReward(Vector3.Distance(relativeTeammatePosition, pastRelativeTeammatePosition) * 0.01f);
            }
        }

        // Move the agent based on actions
        MoveAgent(actionBuffers.DiscreteActions);
    }

    /*
     * Provide heuristic actions for testing
     * - Allows manual control of the agent using keyboard inputs.
     */
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }
    }

    /*
     * Called when a collision occurs
     * - Adds a reward if the agent touches the ball.
     * - Applies force to the ball based on the agent's kick power.
     */
    void OnCollisionEnter(Collision c)
    {
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }
        if (c.gameObject.CompareTag("ball"))
        {
            AddReward(.2f * m_BallTouch);
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }

    /*
     * Called at the beginning of each episode
     * - Resets the agent's state and clears the memory.
     */
    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
        m_PreviousPosition = transform.position;
        m_CumulativeDistance = 0f;
        // Clear the memory at the beginning of each episode
        pastPositions.Clear();
        pastRelativeBallPositions.Clear();
        pastRelativeTeammatePositions.Clear();
    }
}