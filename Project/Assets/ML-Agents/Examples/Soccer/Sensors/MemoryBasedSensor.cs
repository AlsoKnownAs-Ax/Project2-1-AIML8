using UnityEngine;
using System.Collections.Generic;

public class MemoryBasedSensor : MonoBehaviour
{
     [Header("Memory Settings")]

    [SerializeField] private int MemorySize = 10;
    //private const float k_DistanceRewardThreshold = 10f;
    private const float k_DistanceReward = 0.1f;

    private AgentSoccer agent;
    private GameObject ball;
    private List<AgentSoccer> teammates;

    private Queue<Vector3> pastPositions;
    private Queue<Vector3> pastRelativeBallPositions;
    private Queue<Vector3> pastRelativeTeammatePositions;

    private Vector3 previousPosition;
    private float cumulativeDistance;

    // public MemoryBasedSensor(AgentSoccer agent, GameObject ball, List<AgentSoccer> teammates)
    // {
    //     this.agent = agent;
    //     this.ball = ball;
    //     this.teammates = teammates;

    //     pastPositions = new Queue<Vector3>(MemorySize);
    //     pastRelativeBallPositions = new Queue<Vector3>(MemorySize);
    //     pastRelativeTeammatePositions = new Queue<Vector3>(MemorySize);

    //     InitializeMemory();
    // }

    public void InitializeMemoryBasedSensor(AgentSoccer agent, GameObject ball, List<AgentSoccer> teammates)
    {
        this.agent = agent;
        this.ball = ball;
        this.teammates = teammates;

        pastPositions = new Queue<Vector3>(MemorySize);
        pastRelativeBallPositions = new Queue<Vector3>(MemorySize);
        pastRelativeTeammatePositions = new Queue<Vector3>(MemorySize);

        InitializeMemory();
    }

    private void InitializeMemory()
    {
        previousPosition = agent.transform.position;
        cumulativeDistance = 0f;

        for (int i = 0; i < MemorySize; i++)
        {
            pastPositions.Enqueue(agent.transform.position);
            pastRelativeBallPositions.Enqueue(ball != null ? agent.transform.position - ball.transform.position : Vector3.zero);
            pastRelativeTeammatePositions.Enqueue(Vector3.zero);
        }
    }

    public void UpdateMemory()
    {
        float distanceMoved = Vector3.Distance(agent.transform.position, previousPosition);
        cumulativeDistance += distanceMoved;
        previousPosition = agent.transform.position;

        if (pastPositions.Count >= MemorySize)
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

        Debug.Log($"Agent Position: {agent.transform.position}");
        Debug.Log($"Relative Ball Position: {relativeBallPosition}");
        Debug.Log($"Relative Teammate Position: {relativeTeammatePosition}");
    }

    public void AddMemoryRewards(AgentSoccer agent)
    {
       /* if (cumulativeDistance >= k_DistanceRewardThreshold)
        {
            agent.AddReward(k_DistanceReward);
            cumulativeDistance = 0f;
        }*/

        foreach (var pastPosition in pastPositions)
        {
            agent.AddReward(Vector3.Distance(agent.transform.position, pastPosition) * 0.01f);
        }

        foreach (var pastRelativeBallPosition in pastRelativeBallPositions)
        {
            agent.AddReward(Vector3.Distance(agent.transform.position - ball.transform.position, pastRelativeBallPosition) * 0.01f);
        }

        foreach (var pastRelativeTeammatePosition in pastRelativeTeammatePositions)
        {
            agent.AddReward(Vector3.Distance(agent.transform.position - pastRelativeTeammatePosition, pastRelativeTeammatePosition) * 0.01f);
        }

    }

    public void ClearMemory()
    {
        pastPositions.Clear();
        pastRelativeBallPositions.Clear();
        pastRelativeTeammatePositions.Clear();
    }
}