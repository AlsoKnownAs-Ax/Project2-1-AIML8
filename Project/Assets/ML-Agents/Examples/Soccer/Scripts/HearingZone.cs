using UnityEngine;

public class HearingZone : MonoBehaviour
{
    public delegate void ObjectDetected(GameObject obj);
    public event ObjectDetected OnObjectDetected;

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("ball") || other.CompareTag("Player"))
        {
            Debug.Log("Object detected: " + other.name);
            OnObjectDetected?.Invoke(other.gameObject); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ball") || other.CompareTag("Player"))
        {
            Debug.Log("Object exited: " + other.name);
        }
    }
}
