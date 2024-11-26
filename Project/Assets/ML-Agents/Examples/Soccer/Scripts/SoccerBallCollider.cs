using UnityEngine;

public class SoccerBallCollider : MonoBehaviour
{
    public GameObject area;
    [HideInInspector]
    public HearingSensor hearingSensor;

    void Start()
    {
        hearingSensor = area.GetComponent<HearingSensorComponent>().hearingSensor;
    }

    void OnCollisionEnter(Collision col)
    {
        hearingSensor.OnTriggerEnter(gameObject);
    }
}
