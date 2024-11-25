
using UnityEngine;

public interface ISoccerSensor
{
    void InitializeSensor(AgentSoccer agent, GameObject ball, System.Collections.Generic.List<AgentSoccer> teammates);
    void UpdateSensor();
    void Reset();
}