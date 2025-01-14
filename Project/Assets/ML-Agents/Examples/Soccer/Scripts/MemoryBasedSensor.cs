using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class MemoryBasedSensor : MonoBehaviour
{
    private Queue<Vector3> ballMemory = new Queue<Vector3>(); // Store the last 10 positions of the ball
    private Queue<Vector3> teammateMemory = new Queue<Vector3>(); // Store the last 10 positions of the teammate
    private Queue<Vector3> selfMemory = new Queue<Vector3>(); // Store the last 10 positions of the agent
    private Queue<Vector3> opponentMemory = new Queue<Vector3>(); // Store the last 10 positions of the opponent
    private Queue<float[]> rayMemory = new Queue<float[]>(); // Store the last 10 ray observations
    private int maxMemorySize = 15; // Increased to match POCA's batch learning
    private float memoryUpdateInterval = 0.2f; // Update 5 times per second to match training freq
    private float timeSinceLastUpdate = 0.0f; // Time since the last memory update
    private int updateCount = 0; // Count the number of updates
    private int instanceId; // Unique identifier for each instance
    private HashSet<Vector3> processedPositionsThisFrame = new HashSet<Vector3>();

    public int MemorySize
    {
        get => maxMemorySize;
        set => maxMemorySize = value;
    }

    private RayPerceptionSensorComponent3D rayPerception; // Ray perception sensor component

    void Start()
    {
        rayPerception = GetComponent<RayPerceptionSensorComponent3D>(); // Get the ray perception sensor component
        instanceId = GetInstanceID(); // Assign a unique identifier to this instance
    }

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= memoryUpdateInterval)
        {
            updateCount++;
            processedPositionsThisFrame.Clear(); // Clear the processed positions at the start of new update

            if (rayPerception != null) 
            {
                var rayInput = rayPerception.GetRayPerceptionInput();
                var rayOutput = RayPerceptionSensor.Perceive(rayInput, false);

                // Store ray observations using a different method name to avoid overload confusion
                StoreRayMemory(rayMemory, rayOutput.RayOutputs);

                // Collect ray hit information
                List<string> rayHitLogs = new List<string>();
                foreach (var ray in rayOutput.RayOutputs) // Iterate through all rays
                {
                    if (ray.HasHit)
                    {
                        var hitObject = ray.HitGameObject;
                        var distance = Vector3.Distance(ray.StartPositionWorld, ray.StartPositionWorld + (ray.EndPositionWorld - ray.StartPositionWorld) * ray.HitFraction);
                        rayHitLogs.Add($"Ray hit object: {hitObject.name}, Tag: {hitObject.tag}, Distance: {distance}, Ray Origin: {ray.StartPositionWorld}, Ray Direction: {ray.EndPositionWorld - ray.StartPositionWorld}");
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
                            rayHitLogs.Add($"Invalid tag on object {hitObject.name}");
                        }
                    }
                }

                // Log all ray hits in one log
                if (rayHitLogs.Count > 0)
                {
                    // Debug.Log($"[{instanceId}] Agent {gameObject.name} Ray Hits:\n" + string.Join("\n", rayHitLogs));
                }

                // Always store self position
                StoreMemory(selfMemory, transform.position);

                // Log current memory states
                // LogMemoryStates();
            }

            timeSinceLastUpdate = 0.0f; // Reset the timer
        }
    }

    // Store the position of objects (ball, teammate, opponent, self) in memory.
    private void StoreMemory(Queue<Vector3> memoryQueue, Vector3 position)
    {
        // Check if we've already processed this position this frame
        if (processedPositionsThisFrame.Contains(position))
        {
            return;
        }

        // More selective memory storage - only store if significant change
        if (memoryQueue.Count > 0)
        {
            Vector3 lastPosition = memoryQueue.ToArray()[memoryQueue.Count - 1];
            if (Vector3.Distance(lastPosition, position) < 0.5f) // Increased threshold to reduce noise
            {
                // Debug.Log($"[{instanceId}] Update {updateCount}: Skipped storing similar position: {position} (Last position: {lastPosition})");
                return; // Skip storing similar positions
            }
        }

        if (memoryQueue.Count >= maxMemorySize)
        {
            memoryQueue.Dequeue();
        }
        memoryQueue.Enqueue(position);
        processedPositionsThisFrame.Add(position); // Mark this position as processed
        // Debug.Log($"[{instanceId}] Update {updateCount}: Stored new position: {position}");
    }

    // Renamed method to avoid overload confusion
    private void StoreRayMemory(Queue<float[]> memoryQueue, RayPerceptionOutput.RayOutput[] rayOutputs)
    {
        if (memoryQueue.Count >= maxMemorySize)
        {
            memoryQueue.Dequeue();
        }

        float[] rayDistances = new float[rayOutputs.Length];
        for (int i = 0; i < rayOutputs.Length; i++)
        {
            rayDistances[i] = rayOutputs[i].HitFraction;
        }
        memoryQueue.Enqueue(rayDistances);
    }

    // Clear all memory
    public void ClearMemory()
    {
        ballMemory.Clear();
        teammateMemory.Clear();
        selfMemory.Clear();
        opponentMemory.Clear();
        rayMemory.Clear();
        // Debug.Log("Agent " + gameObject.name + $"[{instanceId}] Memory cleared");
    }

    // Log current memory states
    private void LogMemoryStates()
    {
        Debug.Log($"[{instanceId}] Agent {gameObject.name} Memory States:\n" +
                  $"Ball Memory: {string.Join(", ", ballMemory)}\n" +
                  $"Teammate Memory: {string.Join(", ", teammateMemory)}\n" +
                  $"Self Memory: {string.Join(", ", selfMemory)}\n" +
                  $"Opponent Memory: {string.Join(", ", opponentMemory)}\n" +
                  $"Ray Memory: {string.Join(", ", rayMemory)}");
    }
}
