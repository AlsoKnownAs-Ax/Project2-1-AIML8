using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class MemoryBasedSensor : MonoBehaviour
{
    private Queue<Vector3> ballMemory = new Queue<Vector3>();
    private Queue<Vector3> teammateMemory = new Queue<Vector3>();
    private Queue<Vector3> selfMemory = new Queue<Vector3>();
    private Queue<Vector3> opponentMemory = new Queue<Vector3>();
    private Queue<float[]> rayMemory = new Queue<float[]>();

    private const int maxMemorySize = 10;

    private RayPerceptionSensorComponent3D rayPerception;

    void Start()
    {
        rayPerception = GetComponent<RayPerceptionSensorComponent3D>();
    }

    void Update()
    {
        // Store current positions
        StoreMemory(ballMemory, FindObjectOfType<Ball>().transform.position);
        StoreMemory(teammateMemory, FindObjectOfType<Teammate>().transform.position);
        StoreMemory(selfMemory, transform.position);
        StoreMemory(opponentMemory, FindObjectOfType<Opponent>().transform.position);

        // Store ray observations
        if (rayPerception != null)
        {
            var rayInput = rayPerception.GetRayPerceptionInput();
            var rayOutput = RayPerceptionSensor.Perceive(rayInput);
            StoreMemory(rayMemory, rayOutput.RayOutputs);
        }
    }

    private void StoreMemory(Queue<Vector3> memoryQueue, Vector3 position)
    {
        if (memoryQueue.Count >= maxMemorySize)
        {
            memoryQueue.Dequeue();
        }
        memoryQueue.Enqueue(position);
    }

    private void StoreMemory(Queue<float[]> memoryQueue, List<RayPerceptionOutput.RayOutput> rayOutputs)
    {
        if (memoryQueue.Count >= maxMemorySize)
        {
            memoryQueue.Dequeue();
        }
        float[] rayDistances = new float[rayOutputs.Count];
        for (int i = 0; i < rayOutputs.Count; i++)
        {
            rayDistances[i] = rayOutputs[i].HitFraction;
        }
        memoryQueue.Enqueue(rayDistances);
    }

    public void ClearMemory()
    {
        ballMemory.Clear();
        teammateMemory.Clear();
        selfMemory.Clear();
        opponentMemory.Clear();
        rayMemory.Clear();
    }
}
