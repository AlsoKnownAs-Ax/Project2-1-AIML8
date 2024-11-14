using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public enum Team
    {
        Blue = 0,
        Purple = 1
    }

public class AgentSoccer : Agent
{

    [HideInInspector]
    public Team team;
    float m_KickPower;
    float m_BallTouch;
    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }

    public Team team;
    private VisionCone visionCone;
    private GameObject ball;
    float m_KickPower;
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    EnvironmentParameters m_ResetParams;

    // Hearing Zone integration
    private HearingZone hearingZone;
    private SphereCollider hearingCollider; 

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
        team = m_BehaviorParameters.TeamId == (int)Team.Blue ? Team.Blue : Team.Purple;
        initialPos = new Vector3(transform.position.x + (team == Team.Blue ? -5f : 5f), 0.5f, transform.position.z);
        rotSign = team == Team.Blue ? 1f : -1f;

        m_LateralSpeed = position == Position.Goalie ? 1.0f : 0.3f;
        m_ForwardSpeed = position == Position.Striker ? 1.3f : 1.0f;

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        // Initialize VisionCone
        visionCone = GetComponent<VisionCone>();
        if (visionCone == null)
        {
            visionCone = gameObject.AddComponent<VisionCone>();
        }

        visionCone.SetVisionPattern(VisionCone.VisionPattern.Scanning);

        // Find the ball in the environment
        ball = GameObject.FindGameObjectWithTag("ball");

        // Initialize Hearing Zone
        hearingZone = GetComponentInChildren<HearingZone>();
            if (hearingZone != null)
            {
                hearingZone.OnObjectDetected += HandleDetectedObject;
                Debug.Log("Hearing zone setup complete");
            }
            else
            {
                Debug.LogWarning("Hearing zone not found"); 
            }
    }

    private void HandleDetectedObject(GameObject obj)
    {
        if (obj.CompareTag("ball"))
        {
            Debug.Log("Ball detected in hearing range");
            AddReward(0.1f); // Reward for detecting the ball
        }
        else if (obj.CompareTag("Player"))
        {
            Debug.Log("Player detected in hearing range");
           
            AddReward(0.05f);
        }
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
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (position == Position.Goalie)
        {
            AddReward(m_Existential); // Existential reward for goalies
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential); // Existential penalty for strikers
        }

        MoveAgent(actionBuffers.DiscreteActions);

        // Check if the ball is within the agent's vision
        if (ball != null && visionCone.IsTargetVisible(ball.transform.position))
        {
            AddReward(0.1f); // Reward for seeing the ball

            // Optional: Adjust movement behavior based on ball visibility
            Vector3 directionToBall = (ball.transform.position - transform.position).normalized;
            MoveAgentToward(directionToBall);
        }
    }

    private void MoveAgentToward(Vector3 direction)
    {
        var dirToGo = direction * m_ForwardSpeed;
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
    }

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

    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }
}
