using UnityEngine;

public class SoccerBallCollider : MonoBehaviour
{
    public SoccerEnvController area;
    // public GameObject area
    [HideInInspector]
    public HearingSensor hearingSensor;
    private float _inRange = 40f;

    void Start()
    {
    }

    void Update() {
        foreach (var item in area.AgentsList) 
        {
            if(Vector3.Distance(gameObject.transform.position, item.Agent.transform.position) < _inRange)
            {
                if (item.Agent.GetComponent<HearingSensorComponent>().hearingSensor != null) {
                    item.Agent.GetComponent<HearingSensorComponent>().hearingSensor.OnTriggerEnter(gameObject);
                }
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // hearingSensor.OnTriggerEnter(gameObject);
    }

//    void OnDrawGizmosSelected()
//    {
//        // Draw a yellow sphere at the transform's position
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawSphere(transform.position, 4);
//    }
}
