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
    MemoryBasedSensor,
    SoundSensor
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
    float m_KickPower;
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f; // Constant power value
    float m_Existential; // Existential reward
    float m_LateralSpeed; // Speed for lateral movement
    float m_ForwardSpeed; // Speed for forward movement

    [Header("Used Sensors")]
    [SerializeField] private List<SensorType> sensors;
    private List<ISoccerSensor> activeSensors = new List<ISoccerSensor>();

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;


    EnvironmentParameters m_ResetParams; // Environment parameters

    Vector3 m_PreviousPosition; // Previous position of the agent
    float m_CumulativeDistance; // Cumulative distance moved
    const float k_DistanceRewardThreshold = 10f; // Distance reward threshold
    const float k_DistanceReward = 0.1f; // Distance reward

    [SerializeField] private GameObject ball; // Reference to the soccer ball
    [SerializeField] private List<AgentSoccer> teammates; // List of teammate agents

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
        // Base rewards that apply to all agents
        if (position == Position.Goalie)
        {
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential);
        }

        // Calculate distance moved and update cumulative distance - basic movement reward

        // Update all sensors
        foreach (var sensor in activeSensors)
        {
            sensor.UpdateSensor();
        }

        // If you need specific sensor behavior, use type checking
        var memorySensor = activeSensors.Find(s => s is MemoryBasedSensor) as MemoryBasedSensor;
        if (memorySensor != null && sensors.Contains(SensorType.MemoryBasedSensor))
        {
            memorySensor.AddMemoryRewards(this);
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
        foreach (var sensor in activeSensors)
        {
            sensor.Reset();
        }
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

    public void addSensorToAgent(string sensor) {
        switch (sensor) {
            case "SoundSensor":
                gameObject.AddComponent<HearingSensorComponent>();
                break;
        }

    }

    private void AttachSensors(List<SensorType> selectedSensors)
    {
        foreach (var sensorType in selectedSensors)
        {
            ISoccerSensor sensor = CreateSensor(sensorType);
            if (sensor != null)
            {
                InitializeBall();
                InitializeTeammates();
                sensor.InitializeSensor(this, ball, teammates);
                activeSensors.Add(sensor);
                Debug.Log($"{sensorType} Sensor attached");
            }
        }
    }

    private ISoccerSensor CreateSensor(SensorType sensorType)
    {
        switch (sensorType)
        {
            case SensorType.VisionCone:
                var visionSensor = gameObject.GetComponent<VisionCone>() ?? gameObject.AddComponent<VisionCone>();
                visionSensor.SetVisionPattern(VisionCone.VisionPattern.Scanning);
                return visionSensor;

            case SensorType.MemoryBasedSensor:
                return gameObject.GetComponent<MemoryBasedSensor>() ?? gameObject.AddComponent<MemoryBasedSensor>();

            // case SensorType.SoundSensor:
            //     return gameObject.GetComponent<HearingSensor>() ?? gameObject.AddComponent<HearingSensor>();

            default:
                Debug.LogWarning($"Unsupported sensor type: {sensorType}");
                return null;
        }
    }
}
