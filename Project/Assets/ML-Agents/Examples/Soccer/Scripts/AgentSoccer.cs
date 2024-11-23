using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Collections.Generic;

public enum Team
{
    Blue = 0,
    Purple = 1
}

public enum SensorType
{
    VisionCone,
    HearingZone,
    MemoryBasedSensor
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

    public Team team;
    private VisionCone visionCone;
    float m_KickPower;
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f; // Constant power value
    float m_Existential; // Existential reward
    float m_LateralSpeed; // Speed for lateral movement
    float m_ForwardSpeed; // Speed for forward movement

    [Header("Used Sensors")]
    [SerializeField] private List<SensorType> sensors;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    // Hearing Zone integration
    private HearingZone hearingZone;
    private SphereCollider hearingCollider;

    EnvironmentParameters m_ResetParams; // Environment parameters

    Vector3 m_PreviousPosition; // Previous position of the agent
    float m_CumulativeDistance; // Cumulative distance moved
    const float k_DistanceRewardThreshold = 10f; // Distance reward threshold
    const float k_DistanceReward = 0.1f; // Distance reward

    [SerializeField] private GameObject ball; // Reference to the soccer ball
    [SerializeField] private List<AgentSoccer> teammates; // List of teammate agents
    private MemoryBasedSensor memorySensor;

    private Vector3 perceivedBallPosition;
    private Dictionary<AgentSoccer, Vector3> perceivedTeammatePositions = new Dictionary<AgentSoccer, Vector3>();

    void Start()
    {
        // Call OnEpisodeBegin at the start for testing
        OnEpisodeBegin();
    }

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

        AttachSensors(sensors);
    }

    private void HandleDetectedObject(GameObject obj, Vector3 perceivedPosition)
    {
        if (obj.CompareTag("ball"))
        {
            Debug.Log($"Ball detected in hearing range at perceived position: {perceivedPosition}");
            perceivedBallPosition = perceivedPosition;
        }
        else if (obj.CompareTag("Player"))
        {
            Debug.Log($"Player detected in hearing range at perceived position: {perceivedPosition}");
            var agent = obj.GetComponent<AgentSoccer>();
            if (agent != null && agent.team == this.team)
            {
                perceivedTeammatePositions[agent] = perceivedPosition;
            }
        }

        m_PreviousPosition = transform.position;
        m_CumulativeDistance = 0f;

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
        bool isHearingSensorActive = false;
        if (hearingZone != null) {
            isHearingSensorActive = true;
        }
        if (!isHearingSensorActive)
            {
                // Determine direction based on action (only if hearing sensor is not active)
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
            }

        var test = m_LateralSpeed;

        if (isHearingSensorActive)
        {
            Vector3 directionToBall = (perceivedBallPosition - transform.position).normalized;
            dirToGo += directionToBall * m_ForwardSpeed;
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
            AddReward(m_Existential); // Existential reward for goalies
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential); // Existential penalty for strikers
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

        if (memorySensor != null)
        {
            // Update memory sensor and add rewards
            memorySensor.UpdateMemory();
            memorySensor.AddMemoryRewards(this);
            return;
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
    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
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
        if (memorySensor != null) memorySensor.ClearMemory();
    }

    private void InitializeBall()
    {
        // Ensure the ball is assigned
        if (ball == null)
        {
            var balls = GameObject.FindGameObjectsWithTag("ball");
            foreach (var potentialBall in balls)
            {
                if (Vector3.Distance(transform.position, potentialBall.transform.position) < 50f) // Adjust the distance threshold as needed
                {
                    ball = potentialBall;
                    break;
                }
            }
            if (ball == null)
            {
                Debug.LogWarning("Ball not found in the scene.");
            }
        }
    }

    private void InitializeTeammates()
    {
        if (teammates == null || teammates.Count == 0)
        {
            teammates = new List<AgentSoccer>();
            var allAgents = FindObjectsOfType<AgentSoccer>();
            foreach (var agent in allAgents)
            {
                if (agent != this && agent.team == this.team && Vector3.Distance(transform.position, agent.transform.position) < 50f) // Adjust the distance threshold as needed
                {
                    teammates.Add(agent);
                }
            }
            if (teammates.Count == 0)
            {
                Debug.LogWarning("No teammates found for the agent.");
            }
        }
    }

    private void AttachSensors(List<SensorType> selectedSensors)
    {
        foreach (var sensor in selectedSensors)
        {
            switch (sensor)
            {
                case SensorType.VisionCone:
                    visionCone = GetComponent<VisionCone>();
                    if (visionCone == null)
                    {
                        visionCone = gameObject.AddComponent<VisionCone>();
                    }
                    visionCone.SetVisionPattern(VisionCone.VisionPattern.Scanning);
                    Debug.Log("visionCone Sensor attached");

                    break;

                case SensorType.HearingZone:
                    hearingZone = GetComponentInChildren<HearingZone>();
                    if (hearingZone != null)
                    {
                        hearingZone.OnObjectDetected += HandleDetectedObject;
                        Debug.Log("Hearing sensor attached");

                        SphereCollider hearingCollider = hearingZone.GetComponent<SphereCollider>();
                        if (hearingCollider != null)
                        {
                            // Adjust the hearing range here (for example, doubling the radius)
                            hearingCollider.radius = 10000f; // Adjust this value as needed
                            Debug.Log("Hearing zone radius set to: " + hearingCollider.radius);
                        }
                        else
                        {
                            Debug.LogWarning("Hearing zone collider not found");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Hearing zone not found");
                    }
                    break;

                case SensorType.MemoryBasedSensor:
                    memorySensor = GetComponent<MemoryBasedSensor>();
                    if (memorySensor == null)
                    {
                        memorySensor = gameObject.AddComponent<MemoryBasedSensor>();
                    }
                    //Make sure the ball and teammates are assigned
                    InitializeBall();
                    InitializeTeammates();

                    if (ball == null) Debug.LogWarning("Ball not assigned");
                    if (teammates == null || teammates.Count == 0) Debug.LogWarning("Teammates not assigned");

                    memorySensor.InitializeMemoryBasedSensor(this, ball, teammates);
                    Debug.Log("MemoryBased Sensor attached");
                    break;
            }
        }
    }
}
