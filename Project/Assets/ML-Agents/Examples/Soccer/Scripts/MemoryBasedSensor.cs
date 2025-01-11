using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Linq;

public class MemoryBasedSensor : MonoBehaviour, ISoccerSensor
{
    // Short-term memory (last 5 positions)
    private const int SHORT_TERM_SIZE = 5;
    // Long-term memory (last 20 positions, sampled every 4th position)
    private const int LONG_TERM_SIZE = 20;
    private const int LONG_TERM_SAMPLING = 4;
    
    // Importance weighted queues
    private PriorityQueue<Vector3> ballMemoryShort = new PriorityQueue<Vector3>();
    private PriorityQueue<Vector3> ballMemoryLong = new PriorityQueue<Vector3>();
    
    // Remove unused frameCount
    // private int frameCount = 0;

    private Queue<Vector3> ballMemory = new Queue<Vector3>(); // Store the last 10 positions of the ball
    private Queue<Vector3> teammateMemory = new Queue<Vector3>(); // Store the last 10 positions of the teammate
    private Queue<Vector3> selfMemory = new Queue<Vector3>(); // Store the last 10 positions of the agent
    private Queue<Vector3> opponentMemory = new Queue<Vector3>(); // Store the last 10 positions of the opponent
    private Queue<float[]> rayMemory = new Queue<float[]>(); // Store the last 10 ray observations

    private const int maxMemorySize = 10; // Maximum number of positions to store

    private RayPerceptionSensorComponent3D rayPerception; // Ray perception sensor component

    private Vector3 lastBallPosition;
    private Vector3 lastBallVelocity;  // Add this field
    private Vector3 ballVelocity;
    private Vector3 ballAcceleration;
    private float ballControlProbability;
    private float timeSinceLastShot = 0f;
    private bool wasLastActionSuccessful = false;
    private const float VELOCITY_THRESHOLD = 0.1f;
    private float lastCalculationTime;
    private const float VELOCITY_UPDATE_INTERVAL = 0.1f; // Calculate velocity every 0.1 seconds

    void Start()
    {
        rayPerception = GetComponent<RayPerceptionSensorComponent3D>(); // Get the ray perception sensor component
        lastBallPosition = Vector3.zero;
        lastBallVelocity = Vector3.zero;
        ballVelocity = Vector3.zero;
        ballAcceleration = Vector3.zero;
        lastCalculationTime = Time.time;
    }

    void Update()
    {
        if (rayPerception != null) 
        {
            var rayInput = rayPerception.GetRayPerceptionInput(); // Get the ray perception input
            var rayOutput = RayPerceptionSensor.Perceive(rayInput); // Get the ray perception output (its a list of ray outputs)

<<<<<<< HEAD
            // Enhanced ray hit logging
            int hitCount = rayOutput.RayOutputs.Count(ray => ray.HasHit);
            var hitDetails = new System.Text.StringBuilder();
            hitDetails.AppendLine($"[MemoryBasedSensor] {gameObject.name} Rays hit: {hitCount}/{rayOutput.RayOutputs.Length}");
            
            for (int i = 0; i < rayOutput.RayOutputs.Length; i++)
            {
                var ray = rayOutput.RayOutputs[i];
                if (ray.HasHit)
                {
                    var hitObject = ray.HitGameObject;
                    hitDetails.AppendLine($"  Ray {i}: Hit {hitObject.name} (tag: {hitObject.tag}) at distance {ray.HitFraction:F2}");
                }
            }
            Debug.Log(hitDetails.ToString());

            // Store ray observations using a different method name to avoid overload confusion
            StoreRayMemory(rayMemory, rayOutput.RayOutputs);
=======
            // Store ray observations
            StoreMemory(rayMemory, rayOutput.RayOutputs); // Store the ray observations
>>>>>>> parent of 718892c62 (fix: Resolve Ray Perception and Memory-Based Sensor integration)

            // Store current positions if detected by rays
            foreach (var ray in rayOutput.RayOutputs) // Iterate through all rays
            {
                if (ray.HasHit)
                {
                    var hitObject = ray.HitGameObject;
                    try
                    {
                        if (hitObject.CompareTag("ball")) // If the object is the ball
                        {
                            StoreMemory(ballMemory, hitObject.transform.position);
                        }
                        else if (hitObject.CompareTag("blueAgent") || hitObject.CompareTag("purpleAgent")) // If the object is an agent
                        {
                            if (hitObject.CompareTag(gameObject.tag)) // if same team
                            {
                                StoreMemory(teammateMemory, hitObject.transform.position);
                            }
                            else // if opponent team
                            {
                                StoreMemory(opponentMemory, hitObject.transform.position);
                            }
                        }
                    }
                    catch (UnityException)
                    {
                        // If the tag is null, empty, or not a valid tag, just ignore it
                        Debug.LogWarning($"Invalid tag on object {hitObject.name}");
                    }
                }
            }
            // Always store self position
            StoreMemory(selfMemory, transform.position);

            // Calculate ball trajectory prediction with time-based smoothing
            if (ballMemory.Count >= 2)
            {
                float timeSinceLastCalculation = Time.time - lastCalculationTime;
                if (timeSinceLastCalculation >= VELOCITY_UPDATE_INTERVAL)
                {
                    Vector3 ballCurrentPos = ballMemory.Last();  // Changed variable name here
                    
                    // Only calculate velocity if we have a valid last position
                    if (lastBallPosition != Vector3.zero)
                    {
                        ballVelocity = (ballCurrentPos - lastBallPosition) / VELOCITY_UPDATE_INTERVAL;
                        ballAcceleration = (ballVelocity - lastBallVelocity) / VELOCITY_UPDATE_INTERVAL;
                    }
                    else
                    {
                        ballVelocity = Vector3.zero;
                        ballAcceleration = Vector3.zero;
                    }

                    // Always log ball movement info
                    Debug.Log($"[MemoryBasedSensor] Ball Movement - " +
                        $"Velocity: {ballVelocity.magnitude:F2} m/s, " +
                        $"Direction: {(ballVelocity.magnitude > 0 ? ballVelocity.normalized.ToString() : "stationary")}, " +
                        $"Acceleration: {ballAcceleration.magnitude:F2} m/s², " +
                        $"Position: {ballCurrentPos}");

                    lastBallPosition = ballCurrentPos;
                    lastBallVelocity = ballVelocity;
                    lastCalculationTime = Time.time;
                }
            }
            else
            {
                // If we don't have enough positions, ball is considered stationary
                ballVelocity = Vector3.zero;
                ballAcceleration = Vector3.zero;
                if (ballMemory.Count > 0)
                {
                    lastBallPosition = ballMemory.Last();
                }
            }

            // Log team formation
            if (teammateMemory.Count > 0)
            {
                var uniquePositions = new HashSet<Vector3>(teammateMemory);
                var averageTeamPosition = Vector3.zero;
                foreach (var pos in uniquePositions)
                    averageTeamPosition += pos;
                averageTeamPosition /= uniquePositions.Count;
                
                float formationSpread = 0f;
                foreach (var pos in uniquePositions)
                    formationSpread += Vector3.Distance(pos, averageTeamPosition);
                formationSpread /= uniquePositions.Count;

                string formationType = DetermineFormationType(averageTeamPosition, formationSpread);

                Debug.Log($"[MemoryBasedSensor] Team Formation - " +
                    $"Type: {formationType}, " +
                    $"Center: {averageTeamPosition}, " +
                    $"Spread: {formationSpread:F2}, " +
                    $"Players: {uniquePositions.Count}");
            }

            // Log importance metrics
            var currentBallPos = ballMemory.Count > 0 ? ballMemory.Last() : Vector3.zero;
            if (currentBallPos != Vector3.zero)
            {
                float distanceToOwnGoal = Vector3.Distance(currentBallPos, transform.position - Vector3.right * 15); // Approximate goal position
                float distanceToOpponentGoal = Vector3.Distance(currentBallPos, transform.position + Vector3.right * 15);
                float distanceToAgent = Vector3.Distance(currentBallPos, transform.position);
                
                // Calculate ball control probability based on distance and velocity
                ballControlProbability = CalculateBallControlProbability(distanceToAgent, ballVelocity.magnitude);
                
                Debug.Log($"[MemoryBasedSensor] Strategic Analysis - " +
                    $"Ball Control Prob: {ballControlProbability:F2}, " +
                    $"Goal Distance Ratio: {distanceToOwnGoal/distanceToOpponentGoal:F2}, " +
                    $"Field Position: {DetermineFieldPosition(currentBallPos)}");
            }

            lastBallPosition = currentBallPos;
            lastBallVelocity = ballVelocity;
        }
    }

    // Store the position of objects (ball, teammate, opponent, self) in memory.
    private void StoreMemory(Queue<Vector3> memoryQueue, Vector3 position)
    {
        if (memoryQueue.Count >= maxMemorySize) // Check if the memory queue is full
        {
            memoryQueue.Dequeue(); // Remove the oldest position
        }
        memoryQueue.Enqueue(position); // Add the new position
    }

    // Stores the ray observations in memory.
    private void StoreMemory(Queue<float[]> memoryQueue, List<RayPerceptionOutput.RayOutput> rayOutputs)
    {
        if (memoryQueue.Count >= maxMemorySize) // Check if the memory queue is full
        {
            memoryQueue.Dequeue(); // Remove the oldest ray observations
        }

        float[] rayDistances = new float[rayOutputs.Count]; // Create an array to store the ray distances
        for (int i = 0; i < rayOutputs.Count; i++) // Iterate through all rays
        {
            rayDistances[i] = rayOutputs[i].HitFraction; // Store the ray distance
        }
        memoryQueue.Enqueue(rayDistances); // Add the new ray observations
    }

    // Clear all memory
    public void ClearMemory()
    {
        ballMemory.Clear();
        teammateMemory.Clear();
        selfMemory.Clear();
        opponentMemory.Clear();
        rayMemory.Clear();
    }

    public bool IsActive()
    {
        return enabled;
    }

    public void CollectObservations(VectorSensor sensor)
    {
        Debug.Log($"[MemoryBasedSensor] Collecting observations - Ball positions: {ballMemory.Count}, " +
                 $"Teammate positions: {teammateMemory.Count}, " +
                 $"Self positions: {selfMemory.Count}, " +
                 $"Opponent positions: {opponentMemory.Count}, " +
                 $"Ray observations: {rayMemory.Count}");
        
        // Add ball memory observations
        foreach (var pos in ballMemory)
        {
            sensor.AddObservation(pos);
        }
        
        // Add teammate memory observations
        foreach (var pos in teammateMemory)
        {
            sensor.AddObservation(pos);
        }
        
        // Add self memory observations
        foreach (var pos in selfMemory)
        {
            sensor.AddObservation(pos);
        }
        
        // Add opponent memory observations
        foreach (var pos in opponentMemory)
        {
            sensor.AddObservation(pos);
        }
        
        // Add ray memory observations
        foreach (var rayDistances in rayMemory)
        {
            sensor.AddObservation(rayDistances);
        }

        // Log success metrics
        Debug.Log($"[MemoryBasedSensor] Action Success Metrics - " +
                 $"LastActionSuccess: {wasLastActionSuccessful}, " +
                 $"TimeSinceLastSuccess: {timeSinceLastShot:F2}s");
    }

    // Track successful actions
    public void RecordAction(string actionType, bool wasSuccessful)
    {
        wasLastActionSuccessful = wasSuccessful;
        timeSinceLastShot += Time.deltaTime;

        if (wasSuccessful)
        {
            Debug.Log($"[MemoryBasedSensor] Successful {actionType} recorded! Time since last: {timeSinceLastShot:F2}s");
            timeSinceLastShot = 0f;
        }
    }

    public void OnEpisodeBegin()
    {
        ClearMemory();
        lastCalculationTime = Time.time;
        ballVelocity = Vector3.zero;
        lastBallVelocity = Vector3.zero;
    }

    private string DetermineFormationType(Vector3 averagePos, float spread)
    {
        if (spread < 0.5f) return "Defensive-Compact";
        if (spread > 2.0f) return "Attacking-Spread";
        return "Neutral-Balanced";
    }

    private float CalculateBallControlProbability(float distance, float ballSpeed)
    {
        const float MAX_CONTROL_DISTANCE = 5f;
        const float MAX_CONTROL_SPEED = 10f;
        
        float distanceFactor = 1f - Mathf.Clamp01(distance / MAX_CONTROL_DISTANCE);
        float speedFactor = 1f - Mathf.Clamp01(ballSpeed / MAX_CONTROL_SPEED);
        
        return distanceFactor * speedFactor;
    }

    private string DetermineFieldPosition(Vector3 position)
    {
        float normalizedX = (position.x + 50) / 100f; // Assuming field is 100 units wide
        if (normalizedX < 0.3f) return "Defensive-Third";
        if (normalizedX > 0.7f) return "Attacking-Third";
        return "Middle-Third";
    }
}
