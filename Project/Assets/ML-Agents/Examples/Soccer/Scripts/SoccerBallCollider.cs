using UnityEngine;

public class SoccerBallCollider : MonoBehaviour
{
    public SoccerEnvController area;
    [HideInInspector]
    public HearingSensor hearingSensor;
    private float _inRange = 40f;

    void Start()
    {
    }

    void Update() 
    {
        foreach (var item in area.AgentsList) 
        {
            if(Vector3.Distance(gameObject.transform.position, item.Agent.transform.position) < _inRange)
            {
                var hearingSensorComponent = item.Agent.GetComponent<HearingSensorComponent>();
                if (hearingSensorComponent?.hearingSensor != null) 
                {
                    // Get the ball's collider component
                    var ballCollider = GetComponent<Collider>();
                    if (ballCollider != null)
                    {
                        hearingSensorComponent.hearingSensor.OnTriggerEnter(ballCollider);
                    }
                }
            }
        }
    }
}
