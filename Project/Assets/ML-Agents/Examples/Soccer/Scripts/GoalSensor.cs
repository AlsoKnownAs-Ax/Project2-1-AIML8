using UnityEngine;
using Unity.MLAgents.Sensors;

public class GoalSensor : MonoBehaviour, ISoccerSensor
{
    private GameObject ownGoal;
    private GameObject opponentGoal;
    private AgentSoccer agent;

    void Start()
    {
        agent = GetComponent<AgentSoccer>();
        // Find goals based on team
        var goals = GameObject.FindGameObjectsWithTag("goal");
        foreach (var goal in goals)
        {
            if (goal.name.Contains(agent.team.ToString()))
                ownGoal = goal;
            else
                opponentGoal = goal;
        }
    }

    public void CollectObservations(VectorSensor sensor)
    {
        if (ownGoal == null || opponentGoal == null) return;

        // Distance and direction to own goal
        Vector3 toOwnGoal = ownGoal.transform.position - transform.position;
        sensor.AddObservation(toOwnGoal.magnitude); // Distance
        sensor.AddObservation(Vector3.Dot(transform.forward, toOwnGoal.normalized)); // Angle

        // Distance and direction to opponent's goal
        Vector3 toOpponentGoal = opponentGoal.transform.position - transform.position;
        sensor.AddObservation(toOpponentGoal.magnitude); // Distance
        sensor.AddObservation(Vector3.Dot(transform.forward, toOpponentGoal.normalized)); // Angle

        Debug.Log($"[GoalSensor] Agent:{gameObject.name} OwnGoalDist:{toOwnGoal.magnitude:F2} " +
                 $"OpponentGoalDist:{toOpponentGoal.magnitude:F2}");
    }

    public void OnEpisodeBegin() { }
    public bool IsActive() => enabled;
}
