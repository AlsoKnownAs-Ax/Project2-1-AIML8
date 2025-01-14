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

public class AgentSoccer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player

    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }

    [HideInInspector]
    public Team team;
    float m_KickPower;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;

    private static int blueTeamGoals = 0;
    private static int purpleTeamGoals = 0;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    EnvironmentParameters m_ResetParams;

    MemoryBasedSensor memoryBasedSensor;

    [Header("Sensor Configuration")]
    [SerializeField] private bool memoryEnabled = false;
    [SerializeField] private bool visionEnabled = false;
    [SerializeField] private bool hearingEnabled = false;

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

        ApplySensorConfiguration();
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

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

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);
        }
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        // Reset goals scored at the beginning of each episode
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
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

            // Check if a goal is scored
            if (IsGoalScored(c))
            {
                if (team == Team.Blue)
                {
                    blueTeamGoals++;
                }
                else
                {
                    purpleTeamGoals++;
                }
                // Debug.Log($"Goal scored! Blue Team: {blueTeamGoals}, Purple Team: {purpleTeamGoals}");

                // Clear memory when a goal is scored
                if (memoryBasedSensor != null)
                {
                    memoryBasedSensor.ClearMemory();
                }
            }
        }
    }

    private bool IsGoalScored(Collision c)
    {
        return false;
    }

    public override void OnEpisodeBegin()
    {
        // Debug.Log("On Episode Begin");
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
        // Add null check before calling ClearMemory
        if (memoryBasedSensor != null)
        {
            memoryBasedSensor.ClearMemory();
        }
        // Reset goals scored at the beginning of each episode

    }

    // Convenience methods for common combinations
    public void EnableMemoryOnly()
    {
        ConfigureSensors(useMemory: true, useVision: false, useHearing: false);
    }

    public void EnableMemoryAndVision()
    {
        ConfigureSensors(useMemory: true, useVision: true, useHearing: false);
    }

    public void EnableAllSensors()
    {
        ConfigureSensors(useMemory: true, useVision: true, useHearing: true);
    }

    public void DisableAllSensors()
    {
        ConfigureSensors(useMemory: false, useVision: false, useHearing: false);
    }

    // Add these new methods alongside the other Enable methods
    public void EnableVisionOnly()
    {
        ConfigureSensors(useMemory: false, useVision: true, useHearing: false);
    }

    public void EnableHearingOnly()
    {
        ConfigureSensors(useMemory: false, useVision: false, useHearing: true);
    }

    // Add these methods to be called from Unity Editor buttons
    public void ApplySensorConfiguration()
    {
        ConfigureSensors(memoryEnabled, visionEnabled, hearingEnabled);
    }

    private void ConfigureSensors(bool useMemory, bool useVision, bool useHearing)
    {
        string agentInfo = $"[Agent: {gameObject.name}, Team: {team}, Position: {position}] ";
        List<string> activeSensors = new List<string>();

        // Handle Memory Sensor
        if (useMemory)
        {
            if (memoryBasedSensor == null)
            {
                memoryBasedSensor = gameObject.AddComponent<MemoryBasedSensor>();
                activeSensors.Add("Memory");
            }
        }
        else if (memoryBasedSensor != null)
        {
            Destroy(memoryBasedSensor);
            memoryBasedSensor = null;
        }

        // Handle Vision Sensor
        if (useVision)
        {
            activeSensors.Add("Vision");
            // Add vision sensor code when implemented
        }

        // Handle Hearing Sensor
        if (useHearing)
        {
            activeSensors.Add("Hearing");
            // Add hearing sensor code when implemented
        }

        // Update the inspector values
        memoryEnabled = useMemory;
        visionEnabled = useVision;
        hearingEnabled = useHearing;

        // Log active sensors
        string sensorStatus = activeSensors.Count > 0 
            ? $"Active Sensors: {string.Join(", ", activeSensors)}"
            : "No active sensors";
        Debug.Log($"{agentInfo}{sensorStatus}");
    }
}