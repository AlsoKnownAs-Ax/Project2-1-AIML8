using UnityEngine;

/// <summary>
/// Interface for all soccer-specific sensors that provide environmental information to agents.
/// Implemented by VisionCone, MemoryBasedSensor, and HearingSensor components.
/// This interface ensures consistent sensor behavior across different sensor types.
/// </summary>
public interface ISoccerSensor
{
    /// <summary>
    /// Initializes the sensor with necessary game object references.
    /// </summary>
    /// <param name="agent">The soccer agent this sensor belongs to</param>
    /// <param name="ball">Reference to the soccer ball in the scene</param>
    /// <param name="teammates">List of teammate agents on the same team</param>
    void InitializeSensor(AgentSoccer agent, GameObject ball, System.Collections.Generic.List<AgentSoccer> teammates);

    /// <summary>
    /// Updates the sensor's state for the current frame.
    /// Called during the agent's action processing to gather new environmental data.
    /// </summary>
    void UpdateSensor();

    /// <summary>
    /// Resets the sensor's state to its initial conditions.
    /// Called at the start of each episode or when the environment is reset.
    /// </summary>
    void Reset();
}