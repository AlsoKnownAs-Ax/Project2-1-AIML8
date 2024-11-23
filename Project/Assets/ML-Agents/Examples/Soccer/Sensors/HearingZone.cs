using UnityEngine;
using System.Collections;

public class HearingZone : MonoBehaviour
{
    public delegate void ObjectDetected(GameObject obj, Vector3 perceivedPosition);
    public event ObjectDetected OnObjectDetected;

    public float detectionRadius = 10f;

    private void Start()
    {
        StartCoroutine(DetectObjectsInRange());
    }

    private IEnumerator DetectObjectsInRange()
    {
        while (true)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("ball") || collider.CompareTag("Player"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);

                    float baseNoiseMagnitude = 0f;
                    float maxNoiseMagnitude = 5.0f;
                    float noiseMagnitude = Mathf.Lerp(baseNoiseMagnitude, maxNoiseMagnitude, distance / 1000.0f);

                    Vector3 randomNoise = Vector3.zero; //Noise is disabled for now
                    // Vector3 randomNoise = new Vector3(
                    //     Random.Range(-noiseMagnitude, noiseMagnitude),
                    //     Random.Range(-noiseMagnitude, noiseMagnitude),
                    //     Random.Range(-noiseMagnitude, noiseMagnitude)
                    // );

                    Vector3 perceivedPosition = collider.transform.position + randomNoise;

                    Debug.Log($"Object detected: {collider.name} at perceived position: {perceivedPosition}");

                    OnObjectDetected?.Invoke(collider.gameObject, perceivedPosition);
                }
            }

            yield return null;
        }
    }

    //Enable if you want to see detection radiuses on screen
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireSphere(transform.position, detectionRadius);
    // }
}
