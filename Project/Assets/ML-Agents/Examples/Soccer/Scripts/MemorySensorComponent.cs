using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class MemorySensorComponent : SensorComponent
{
    private GameObject ball;
    private List<AgentSoccer> teammates;
    private AgentSoccer agent;
    public MemoryBasedSensor memoryBasedSensor;
    private void InitializeParameters()
    {
        ball = GameObject.FindWithTag("ball");
        if (ball == null)
        {
            Debug.LogError("Ball GameObject not found! Ensure it has the 'ball' tag.\n" + "Ball GameObject is required for MemorySensorComponent!");
            return;
        }

        agent = GetComponent<AgentSoccer>();
        if (agent == null)
        {
            Debug.LogError("AgentSoccer component is required for MemorySensorComponent!");
            return;
        }

        teammates = new List<AgentSoccer>();
        var allAgents = FindObjectsOfType<AgentSoccer>();
        foreach (var potentialTeammate in allAgents)
        {
            if (potentialTeammate != agent && potentialTeammate.team == agent.team && Vector3.Distance(transform.position, potentialTeammate.transform.position) < 50f) // Adjust the distance threshold as needed
            {
                teammates.Add(potentialTeammate);
            }
        }
        if (teammates.Count == 0)
        {
            Debug.LogWarning("No teammates found for the agent.");
        }

        if (teammates == null || teammates.Count == 0)
        {
            Debug.LogError("Teammates list is required for MemorySensorComponent!");
            return;
        }
    }

    public override ISensor[] CreateSensors()
    {
        InitializeParameters();
        memoryBasedSensor = new MemoryBasedSensor(agent, ball, teammates);
        return new ISensor[] { memoryBasedSensor };
    }
}
