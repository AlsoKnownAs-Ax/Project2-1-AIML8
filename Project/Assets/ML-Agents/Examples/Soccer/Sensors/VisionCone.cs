using UnityEngine;
using System.Collections;

public class VisionCone : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float viewRadius = 10f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Movement Settings")]
    [SerializeField] private float maxRotationSpeed = 180f; // degrees per second
    [SerializeField] private float maxAngleChange = 45f;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private Transform agentTransform;
    private Vector3 visionDirection;
    private Coroutine patternCoroutine;

    [Header("Pattern Settings")]
    [SerializeField] private VisionPattern currentPattern = VisionPattern.Scanning;
    
    public enum VisionPattern
    {
        Scanning, Random
    }

    private void Awake()
    {
        agentTransform = transform;
        visionDirection = agentTransform.forward;
    }

    private void Start()
    {
        SetVisionPattern(currentPattern);
    }

    public void SetVisionPattern(VisionPattern pattern)
    {
        if (patternCoroutine != null)
            StopCoroutine(patternCoroutine);

        currentPattern = pattern;

        if (pattern == VisionPattern.Scanning)
            patternCoroutine = StartCoroutine(ScanningPattern());
        else if (pattern == VisionPattern.Random)
            patternCoroutine = StartCoroutine(RandomPattern());
    }

private IEnumerator ScanningPattern()
{
    float scanAngle = 0f;
    bool scanningRight = true;

    while (true)
    {
        // Calculate the angle increment based on the direction of scanning
        float angleIncrement = (scanningRight ? 1 : -1) * maxRotationSpeed * Time.deltaTime;
        scanAngle += angleIncrement;

        // Check if the scan angle has reached the maximum angle change
        if (Mathf.Abs(scanAngle) >= maxAngleChange)
        {
            // Reverse the scanning direction
            scanningRight = !scanningRight;
            // Ensure the scan angle stays within the bounds
            scanAngle = Mathf.Clamp(scanAngle, -maxAngleChange, maxAngleChange);
        }

        UpdateVisionDirection(scanAngle);
        yield return null;
    }
}

    private IEnumerator RandomPattern()
    {
        while (true)
        {
            float targetAngle = Random.Range(-maxAngleChange, maxAngleChange);
            float currentRotation = 0f;

            while (Mathf.Abs(currentRotation - targetAngle) > 0.1f)
            {
                float step = maxRotationSpeed * Time.deltaTime;
                currentRotation = Mathf.MoveTowards(currentRotation, targetAngle, step);
                UpdateVisionDirection(currentRotation);
                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        }
    }

    private void UpdateVisionDirection(float angle)
    {
        visionDirection = Quaternion.Euler(0, angle, 0) * agentTransform.forward;
    }

    public bool IsTargetVisible(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - agentTransform.position).normalized;
        float angleToTarget = Vector3.Angle(visionDirection, directionToTarget);

        if (angleToTarget <= viewAngle / 2)
        {
            float distanceToTarget = Vector3.Distance(agentTransform.position, targetPosition);
            if (distanceToTarget <= viewRadius)
            {
                if (!Physics.Raycast(agentTransform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!debugMode)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(agentTransform.position, viewRadius);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * visionDirection * viewRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * visionDirection * viewRadius;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(agentTransform.position, agentTransform.position + leftBoundary);
        Gizmos.DrawLine(agentTransform.position, agentTransform.position + rightBoundary);

        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawMesh(CreateVisionMesh(), agentTransform.position, Quaternion.identity);
    }

    private Mesh CreateVisionMesh()
    {
        Mesh mesh = new Mesh();
        int segments = 10;
        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(segments + 1) * 3];

        vertices[0] = Vector3.zero;
        float angleStep = viewAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = -viewAngle / 2 + angleStep * i;
            vertices[i + 1] = Quaternion.Euler(0, angle, 0) * visionDirection * viewRadius;

            if (i < segments)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
