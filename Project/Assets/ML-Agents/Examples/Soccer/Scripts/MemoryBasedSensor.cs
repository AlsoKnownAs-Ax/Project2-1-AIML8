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

    private const int maxMemorySize = 10; // Maximum number of positions to store

    private RayPerceptionSensorComponent3D rayPerception; // Ray perception sensor component

    void Start()
    {
        rayPerception = GetComponent<RayPerceptionSensorComponent3D>(); // Get the ray perception sensor component
    }

    void Update()
    {
        if (rayPerception != null) 
        {
            var rayInput = rayPerception.GetRayPerceptionInput(); // Get the ray perception input
            var rayOutput = RayPerceptionSensor.Perceive(rayInput); // Get the ray perception output (its a list of ray outputs)

            // Store ray observations
            StoreMemory(rayMemory, rayOutput.RayOutputs); // Store the ray observations

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
}
