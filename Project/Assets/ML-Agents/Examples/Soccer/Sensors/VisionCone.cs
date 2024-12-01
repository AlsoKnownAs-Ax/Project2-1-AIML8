using UnityEngine;
using System.Collections;
using Unity.MLAgents.Sensors;

public class VisionCone : MonoBehaviour, ISensor, ISoccerSensor
{
    [Header("Vision Settings")]
    [SerializeField] 
    [Range(5f, 20f)]
    [Tooltip("How far the agent can see")]
    public float viewRadius = 10f;

    [SerializeField] 
    [Range(30f, 180f)]
    [Tooltip("Field of view angle")]
    public float viewAngle = 90f;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Movement Settings")]
    [SerializeField] 
    [Range(90f, 360f)]
    [Tooltip("How fast the vision cone rotates")]
    public float maxRotationSpeed = 180f;

    [SerializeField] 
    [Range(15f, 90f)]
    [Tooltip("Maximum angle the vision can turn")]
    public float maxAngleChange = 45f;

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

    private AgentSoccer agent;
    private GameObject ball;
    private System.Collections.Generic.List<AgentSoccer> teammates;

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

    public void InitializeSensor(AgentSoccer agent, GameObject ball, System.Collections.Generic.List<AgentSoccer> teammates)
    {
        this.agent = agent;
        this.ball = ball;
        this.teammates = teammates;
    }

    public void UpdateSensor()
    {
        // Update vision logic is already handled in existing coroutines
    }

    public void Reset()
    {
        if (patternCoroutine != null)
            StopCoroutine(patternCoroutine);
        SetVisionPattern(currentPattern);
    }

    // ISensor Implementation
    public string GetName()
    {
        return "VisionCone";
    }

    public int[] GetObservationShape()
    {
        return new int[] { 74 }; // Keep at 74 observations
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public int Write(ObservationWriter writer)
    {
        int index = 0;
        if (ball != null)
        {
            Vector3 directionToTarget = (ball.transform.position - agent.transform.position).normalized;
            float angleToTarget = Vector3.Angle(visionDirection, directionToTarget);
            float distanceToTarget = Vector3.Distance(agent.transform.position, ball.transform.position);
            bool hasLineOfSight = !Physics.Raycast(agent.transform.position, directionToTarget, distanceToTarget, obstacleMask);
            bool isVisible = IsTargetVisible(ball.transform.position);

            writer[index++] = isVisible ? 1f : 0f;
            writer[index++] = angleToTarget / 180f;
            writer[index++] = distanceToTarget / viewRadius;
            writer[index++] = hasLineOfSight ? 1f : 0f;
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                writer[index++] = 0f;
            }
        }
        return 4;
    }

    public void Update() { }

    public void Reset(bool sentSignal) 
    {
        Reset();
    }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Vector(74);
    }

    public void SetVisionParameters(float radius, float angle)
    {
        viewRadius = radius;
        viewAngle = angle;
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
