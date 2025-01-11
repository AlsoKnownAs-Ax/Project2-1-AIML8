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

    private int maxMemorySize = 10; // Default maximum number of positions to store

    public int MemorySize
    {
        get => maxMemorySize;
        set => maxMemorySize = value;
    }

    private RayPerceptionSensorComponent3D rayPerception; // Ray perception sensor component

    void Start()
    {
        rayPerception = GetComponent<RayPerceptionSensorComponent3D>(); // Get the ray perception sensor component
    }

    void Update()
    {
        if (rayPerception != null) 
        {
            var rayInput = rayPerception.GetRayPerceptionInput();
            var rayOutput = RayPerceptionSensor.Perceive(rayInput, false);

            // Store ray observations using a different method name to avoid overload confusion
            StoreRayMemory(rayMemory, rayOutput.RayOutputs);

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
    }
}
